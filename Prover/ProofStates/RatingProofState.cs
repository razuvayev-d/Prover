using Prover.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover.ProofStates
{
    internal class RatingProofState
    {


        SearchParams Params;



        public int initial_clause_count;
        public int proc_clause_count = 0;
        public int factor_count = 0;
        public int resolvent_count = 0;
        public int tautologies_deleted = 0;
        public int forward_subsumed = 0;
        public int backward_subsumed = 0;
        bool silent;

        List<Literal> freqLiterals;
        HeuristicClauseSet unprocessed;
        List<Clause> ClauseArray;
        public RatingProofState(SearchParams Params, ClauseSet clauses)
        {
            freqLiterals = GetFrequentLiterals(clauses);

            this.Params = Params;
            unprocessed = new HeuristicClauseSet(Params.heuristics);

            foreach (Clause clause in clauses.clauses)
                unprocessed.AddClause(clause);


            initial_clause_count = unprocessed.Count;

            unprocessed.clauses.Sort(delegate (Clause c1, Clause c2)
            {
                return c1.evaluation[0].CompareTo(c2.evaluation[0]);
            });

            ClauseArray = unprocessed.clauses;

        }

        private List<Literal> GetFrequentLiterals(ClauseSet clauses)
        {
            List<Literal> c = new List<Literal>();
            List<Literal> c1 = new List<Literal>();
            List<int> freq = new List<int>();

            Dictionary<Literal, int> FreqLitsDict = new Dictionary<Literal, int>();

            foreach (Clause clause in clauses.clauses)//Создали коллекцию ВСЕХ атомов
                foreach (Literal literal in clause.Literals)
                {
                    c.Add(literal);
                }

            foreach (Literal literal in c) //Коллекция уникальных БЕЗ учета знака
            {
                if (FreqLitsDict.ContainsKey(literal))
                    FreqLitsDict[literal]++;
                else
                    FreqLitsDict.Add(literal, 1);
            }

            var sortedDict = (from entry in FreqLitsDict orderby entry.Value descending select entry).ToList();


            int j = 0;
            List<Literal> result = new List<Literal>();
            while (true)
            {
                result.Add(sortedDict[j].Key);
                if (j < sortedDict.Count - 1)
                {
                    if (sortedDict[j].Value == sortedDict[j + 1].Value)
                    {
                        j++;
                        continue;
                    }
                    else
                    {
                        return result;
                    }
                }
                else
                {
                    return result;
                }
            }
        }

        private List<Clause> GetOrderedClauses()
        {
            unprocessed.clauses.Sort(delegate (Clause c1, Clause c2)
            {
                return c1.evaluation[0].CompareTo(c2.evaluation[0]);
            });
            return unprocessed.clauses;
        }



        public Clause Saturate()
        {
            int k = 0;

            var M = unprocessed;
            List<Clause> resolvents = new List<Clause>();
   
            List<Clause> Skplus = new List<Clause>();
            List<List<Clause>> S = new List<List<Clause>>();

            ClauseArray = GetOrderedClauses(); //M_k

            int i = 0;
            int j = unprocessed.Count - 1;
            while (true)
            {
                S.Add(new List<Clause>());
                if (resolvents.Where(x => x.IsEmpty).Any())
                {
                    return resolvents.Where((x) => x.IsEmpty).ToList()[0];
                }
                ClauseArray = GetOrderedClauses(); //M_k
            step32:
                Clause resolvent = ResolutionMethod.Resolution.Apply(ClauseArray[i], ClauseArray[j]);

                if (resolvent is null)
                {
                    if (j > i)
                    {
                        j--;
                        goto step32;
                    }
                    else
                    {
                        if (i < ClauseArray.Count - 1)
                        {
                            i++;
                            j = ClauseArray.Count - 1;
                            goto step32;
                        }
                        else
                        {
                            if (resolvents.Where(x => x.IsEmpty).Any())
                            {
                                return resolvents.Where((x) => x.IsEmpty).ToList()[0];
                            }
                            else
                            {



                                if (unprocessed.Count > 0) {
                                    var a = ClauseArray[i];
                                    var b = ClauseArray[j];

                                    M.ExtractClause(a);
                                    M.ExtractClause(b);
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }

                    }
                }
                else //resolvent is not null
                {
                    resolvents.Add(resolvent);
                    continue;
                }
            }

        } 
    }
}
