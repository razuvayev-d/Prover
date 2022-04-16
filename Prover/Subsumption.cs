using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{
    internal static class Subsumption
    {
        public static bool SubsumeLitLists(List<Literal> subsumer, List<Literal> subsumed, BTSubst subst)
        {
            //if (subsumer.Count > 0) return true;

            foreach (var lit in subsumed)
            {
                int btstate = subst.GetState;
                if (subsumer[0].Match(lit, subst))
                {
                    var rest = subsumed.Where(x => !x.Equals(lit)).ToList();
                    if (SubsumeLitLists(subsumer.Skip(1).ToList(), rest, subst))
                        return true;
                }
                subst.BacktrackToState((subst, btstate));
            }
            return false;
        }

        public static bool Subsumes(Clause subsumer, Clause subsumed)
        {
            if (subsumer.Length != subsumed.Length) return false;
            var subst = new BTSubst();
            return SubsumeLitLists(subsumer.Literals, subsumed.Literals, subst);
        }

        public static bool Forward(ClauseSet set, Clause clause)
        {
            var candidates = set.clauses;

            foreach (var candidate in candidates)
                if (Subsumes(candidate, clause))
                    return true;
            return false;
        }

        public static int Backward(Clause clause, ClauseSet set)
        {
            var candidates = set.clauses;
            var subsumedSet = new List<Clause>();

            foreach (var candidate in candidates)
                if (Subsumes(clause, candidate))
                    subsumedSet.Add(candidate);
            int res = subsumedSet.Count;
            foreach (var c in subsumedSet)
            {
                set.ExtractClause(c);
            }
            return res;
        }

   
    }
}
