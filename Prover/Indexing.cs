using Prover.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{
    internal class ResolutionIndex
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

        }

        Dictionary<string, List<Candidate>> pos_ind = new Dictionary<string, List<Candidate>>();
        Dictionary<string, List<Candidate>> neg_ind = new Dictionary<string, List<Candidate>>();

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
            for(int i = 0; i< clause.Length; i++)
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

            if(idx.ContainsKey(lit.Name))
                return idx[lit.Name];
            return new List<Candidate>();
        }

        //public PredAbstractionIsSubSequence(Candidate candidate, )
    }
}
