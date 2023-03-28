using Prover.ClauseSets;
using Prover.DataStructures;
using System.Collections.Generic;
using System.Diagnostics;

namespace Prover.ResolutionMethod
{
    static class ResControl
    {
        public static ClauseSet ComputeAllResolvents(Clause clause, ClauseSet clauseSet)
        {
            ClauseSet res = new ClauseSet();
            for (int lit = 0; lit < clause.Length; lit++)
            {
                List<Clause> clauseres = new List<Clause>();
                List<int> indices = new List<int>();
                if (clause[lit].IsInference)
                {
                    clauseSet.GetResolutionLiterals(clause[lit], clauseres, indices);
                    //Debug.Assert(clauseres.Count == indices.Count);
                    for (int i = 0; i < clauseres.Count; i++)
                    {
                        Clause resolvent = Resolution.Apply(clause, lit, clauseres[i], indices[i]);
                        if (resolvent is not null)
                            res.AddClause(resolvent);
                    }
                }
            }
            return res;
        }


        public static ClauseSet ComputeAllResolventsIndexed(Clause clause, IndexedClauseSet clauseSet)
        {
            ClauseSet res = new ClauseSet();
            for (int lit = 0; lit < clause.Length; lit++)
            {
                if (clause[lit].IsInference)
                {
                    var partners = clauseSet.GetResolutionLiterals(clause[lit]);

                    foreach (var p in partners)
                    {
                        var resolvent = Resolution.Apply(clause, lit, p.Clause, p.Position);
                        if (resolvent is not null)
                            res.AddClause(resolvent);
                    }
                }
            }
            return res;
        }

        public static ClauseSet ComputeAllFactors(Clause clause)
        {
            ClauseSet res = new();
            for (int i = 0; i < clause.Length; i++)
                for (int j = i + 1; j < clause.Length; j++)
                {
                    if (clause[j].IsInference || clause[i].IsInference)
                    {
                        Clause fact = Resolution.Factor(clause, i, j);
                        if (fact is not null)
                            res.AddClause(fact);
                    }
                }
            return res;
        }
    }
}
