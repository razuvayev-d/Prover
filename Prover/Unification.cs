using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{
    class Unification
    {
        public static Substitution MGU(Literal l1, Literal l2)
        {
            List<Term> terms1 = new List<Term>();
            terms1.AddRange(l1.Arguments);

            List<Term> terms2 = new List<Term>();
            terms2.AddRange(l2.Arguments);

            return MGUTermList(terms1, terms2);
        }

        private static Substitution MGUTermList(List<Term> terms1, List<Term> terms2)
        {
            Substitution substitution = new Substitution();

            if (terms1.Count != terms2.Count)
            {
                if (substitution.subst.Keys.Count > 0)
                    return substitution;
                else
                    return null;
            }

            while (terms1.Count > 0)
            {
                Term t1 = terms1[0];
                terms1.RemoveAt(0);
                Term t2 = terms2[0];
                terms2.RemoveAt(0);

                if (t1.IsVar)
                {
                    if (t1.Equals(t2)) continue;

                    if (OccursCheck(t1, t2)) return null;

                    Substitution newBinding = new Substitution(t1, t2);

                    newBinding.Apply(terms1);
                    newBinding.Apply(terms2);
                    substitution.ComposeBinding(t1, t2);
                }
                else if (t2.IsVar)
                {
                    if (OccursCheck(t2, t1)) return null;
                    Substitution newBinding = new Substitution(t2, t1);

                    newBinding.Apply(terms1);
                    newBinding.Apply(terms2);

                    substitution.ComposeBinding(t2, t1);
                }
                else
                {
                    if (!t1.IsCompound || !t2.IsCompound)
                        return null;

                    if (!t1.name.Equals(t2.name))
                        return null;

                    terms1.AddRange(t1.TermArgs);
                    terms2.AddRange(t2.TermArgs);
                }
            }
            return substitution;
        }

        private static bool OccursCheck(Term x, Term t)
        {
            if (t.IsCompound)
            {
                for (int i = 0; i < t.subterms.Count; i++)
                    if (OccursCheck(x, t.subterms[i]))
                        return true;
                return false;
            }
            else
                return x.Equals(t);
        }
    }
}