using Prover.DataStructures;
using System;
using System.Collections.Generic;

namespace Prover.ClauseSets
{
    public class ResolutionIndex
    {
        public class Candidate
        {
            public Clause Clause;
            public int Position;

            public Candidate(Clause clause, int pos)
            {
                Clause = clause;
                Position = pos;
            }

            public static implicit operator Candidate((Clause, int) pair)
            {
                return new Candidate(pair.Item1, pair.Item2);
            }

            public override int GetHashCode()
            {
                return Position + Clause.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (!(obj is Candidate)) return false;
                return ((Candidate)obj).GetHashCode() == this.GetHashCode();
            }

        }

        public Dictionary<string, List<Candidate>> pos_ind = new Dictionary<string, List<Candidate>>();
        public Dictionary<string, List<Candidate>> neg_ind = new Dictionary<string, List<Candidate>>();

        private void InsertData(Dictionary<string, List<Candidate>> idx, string topsymbol, Candidate payload)
        {
            if (!idx.ContainsKey(topsymbol))
                idx.Add(topsymbol, new List<Candidate>());
            idx[topsymbol].Add(payload);
        }

        private void RemoveData(Dictionary<string, List<Candidate>> idx, string topsymbol, Candidate payload)
        {
            idx[topsymbol].Remove(payload);
        }

        public void InsertClause(Clause clause)
        {
            for (int i = 0; i < clause.Length; i++)
            {
                var lit = clause[i];
                if (lit.IsInference)
                {
                    if (!lit.Negative) //positive case
                        InsertData(pos_ind, lit.Name, (clause, i));
                    else
                        InsertData(neg_ind, lit.Name, (clause, i));
                }
            }
        }

        public void RemoveClause(Clause clause)
        {
            for (int i = 0; i < clause.Length; i++)
            {
                var lit = clause[i];
                if (lit.IsInference)
                {
                    if (!lit.Negative) //positive case
                        RemoveData(pos_ind, lit.Name, (clause, i));
                    else
                        RemoveData(neg_ind, lit.Name, (clause, i));
                }
            }
        }

        public List<Candidate> GetResolutionLiterals(Literal lit)
        {
            var idx = neg_ind;
            if (!lit.Negative)
                idx = neg_ind;
            else
                idx = pos_ind;

            if (idx.ContainsKey(lit.Name))
                return idx[lit.Name];
            return new List<Candidate>();
        }

    }




    public class SubsumptionIndex
    {


        public class ArrayElement
        {
            public int LenPA;
            public PredicateAbstractionArray PredicateAbstraction;
            public List<Clause> Entry;

            public ArrayElement(int Len, PredicateAbstractionArray PredicateAbstraction, List<Clause> Entry)
            {
                this.LenPA = Len;
                this.PredicateAbstraction = PredicateAbstraction;
                this.Entry = Entry;
            }

            public static implicit operator ArrayElement((int, PredicateAbstractionArray, List<Clause>) tuple)
            {
                return new ArrayElement(tuple.Item1, tuple.Item2, tuple.Item3);
            }
        }



        public static bool PredAbstractionIsSubSequence(List<PredicateAbstraction> candidate, List<PredicateAbstraction> superseq)
        {
            int i = 0;
            int end = superseq.Count;

            try
            {
                foreach (var sub in candidate)
                {
                    while (!superseq[i].Equals(sub))
                    {
                        i++;
                    }
                    i++;
                }
            }
            catch (Exception e)
            { return false; }
            return true;
        }

        public Dictionary<PredicateAbstractionArray, List<Clause>> PredAbstrSet = new Dictionary<PredicateAbstractionArray, List<Clause>>();

        public List<ArrayElement> PredAbstrArr = new List<ArrayElement>();

        public void InsertClause(Clause clause)
        {
            var pa = clause.PredicateAbstraction();
            if (PredAbstrSet.ContainsKey(pa))
                PredAbstrSet[pa].Add(clause);
            else
            {
                var entry = new List<Clause>();
                PredAbstrSet[pa] = entry;
                PredAbstrSet[pa].Add(clause);
                int l = pa.Count;
                int i = 0;

                foreach (var el in PredAbstrArr)
                {
                    if (el.LenPA >= l)
                        break;
                    i++;
                }
                PredAbstrArr.Insert(i, (l, pa, entry));
            }

        }

        public void RemoveClause(Clause clause)
        {
            var pa = clause.PredicateAbstraction();
            PredAbstrSet[pa].Remove(clause);
        }

        public bool IsIndexed(Clause clause)
        {
            var pa = clause.PredicateAbstraction();
            if (PredAbstrSet.ContainsKey(pa))
            {
                var arr = PredAbstrSet[pa];
                if (arr.Contains(clause)) return true;
            }
            return false;
        }

        public List<Clause> GetSubsumingCandidates(Clause queryclause)
        {
            var pa = queryclause.PredicateAbstraction();
            var pa_len = pa.Count;

            var res = new List<Clause>();
            foreach (var el in PredAbstrArr)
            {
                if (el.LenPA > pa_len)
                    break;
                if (SubsumptionIndex.PredAbstractionIsSubSequence(el.PredicateAbstraction.array, pa.array))
                {
                    res.AddRange(el.Entry);
                }
            }
            return res;
        }

        public List<Clause> GetSubsumedCandidates(Clause queryclause)
        {
            var pa = queryclause.PredicateAbstraction();
            var pa_len = pa.Count;

            var res = new List<Clause>();
            foreach (var el in PredAbstrArr)
            {
                if (el.LenPA < pa_len)
                    continue;
                if (SubsumptionIndex.PredAbstractionIsSubSequence(pa.array, el.PredicateAbstraction.array))
                {
                    res.AddRange(el.Entry);
                }
            }
            return res;
        }
    }
}
