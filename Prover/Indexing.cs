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
        Dictionary<Literal, List<(Clause, int)>> pos_ind = new Dictionary<Literal, List<(Clause, int)>>();
        Dictionary<Literal, List<(Clause, int)>> neg_ind = new Dictionary<Literal, List<(Clause, int)>>();

        private void InsertData(Dictionary<Literal, List<(Clause, int)>> idx, Literal topsymbol, (Clause, int) payload)
        {
            if (!idx.ContainsKey(topsymbol)) 
                idx.Add(topsymbol, new List<(Clause, int)>());
            idx[topsymbol].Add(payload);
        }

        private void RemoveData(Dictionary<Literal, List<(Clause, int)>> idx, Literal topsymbol, (Clause, int) payload)
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
                        InsertData(pos_ind, lit, (clause, i));
                    else
                        InsertData(neg_ind, lit, (clause, i));
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
                        RemoveData(pos_ind, lit, (clause, i));
                    else
                        RemoveData(neg_ind, lit, (clause, i));
                }
            }
        }

    }
}
