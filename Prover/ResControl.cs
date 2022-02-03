﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{
    static class ResControl
    {
        public static ClauseSet ComputeAllResolvents(Clause clause, ClauseSet clauseSet)
        {
            ClauseSet res = new ClauseSet();
            for(int lit = 0; lit < clause.Length; lit++)
            {
                List<Clause> clauseres = new List<Clause>(); 
                List<int> indices = new List<int>();

                clauseSet.GetResolutionLiterals(clause[lit], clauseres, indices);
                Debug.Assert(clauseres.Count == indices.Count);
                for (int i = 0; i < clauseres.Count; i++)
                {
                    Clause resolvent = Resolution.Apply(clause, lit, clauseres[i], indices[i]);
                    if (resolvent is not null)
                        res.AddClause(resolvent);
                }
            }
            return res;
        }
    }
}