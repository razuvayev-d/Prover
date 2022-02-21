using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{
    /// <summary>
    /// Подстановки отображают переменные на термы. Подстановки, используемые здесь
    /// всегда полностью развернуты, т.е.каждая переменная привязана непосредственно к
    /// термином, к которому она привязана.
    /// </summary>
    class Substitution
    {
        public Dictionary<Term, Term> subst = new Dictionary<Term, Term>();
        private static int freshVarCounter = 0;

        public Substitution(Term variable, Term value)
        {
            subst[variable] = value;
        }

        public Substitution() { }

        public Term this[Term variable]
        {
            get
            {
                if (subst.ContainsKey(variable))
                    return subst[variable];
                else
                    return variable;
            }
            set
            {
                subst[variable] = value;
            }
        }

        /// <summary>
        /// Возвращает True, если var связана в self, false в противном случае.
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        public bool IsBound(Term var) => subst.ContainsKey(var);

        public void Apply(List<Term> terms)
        {
            int n = terms.Count;
            for (int i = 0; i < n; i++)
                terms[i] = Apply(terms[i]);
        }

        /// <summary>
        /// Apply the substitution to a term. Return the result.
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public Term Apply(Term term)
        {
            Term res = new Term();
            if (term.IsVar)
            {
                if (subst.ContainsKey(term))
                {
                    return subst[term];
                }
                else
                {
                    res.name = term.name;
                }
            }
            else
            {
                res.name = term.name;
                var n = term.SubtermsCount;
                for (int i = 0; i < n; i++)
                    res.AddSubterm(Apply(term.subterms[i]));
                return res;
            }
            return res;
        }
        /// <summary>
        /// Измените подстановку, добавив новую привязку (var,
        /// терм). Если термин None, удалите привязку для var.Если
        /// нет, добавьте привязку. В любом случае верните предыдущую
        /// предыдущую привязку переменной или None, если она была несвязанной.
        /// </summary>
        /// <param name="var"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public Term ModifyBinding(Term var, Term binding)
        {
            Term res = null;
            if (IsBound(var))
                res = subst[var];
            if (binding == null)
            {
                if (IsBound(var))
                    subst.Remove(var);
            }
            else
            {
                subst[var] = binding;
            }
            return res;
        }
        /// <summary>
        ///  Cоставьте новую привязку к существующей подстановке.
        /// </summary>
        public void ComposeBinding(Term var, Term term)
        {
            Substitution tmpSubst = new Substitution(var, term);
            var vars = subst.Keys;
            foreach (var x in vars)
            {
                var bound = subst[x];
                subst[x] = tmpSubst.Apply(bound);
            }
            if (!subst.ContainsKey(var))
                subst[var] = term;
        }

        public static Substitution FreshVarSubst(List<Term> vars)
        {

            Substitution s = new Substitution();
            for (int i = 0; i < vars.Count; i++)
            {
                Term newVar = FreshVar();
                s.subst.Add(vars[i], newVar);
            }
            return s;
        }

        public void AddAll(Substitution s)
        {
            var it = subst.Keys.GetEnumerator();

            while (it.MoveNext())
            {
                Term t1 = it.Current;
                Term t2 = s.subst[t1];
                subst.Add(t1, t2);
            }
        }

        public static Term FreshVar()
        {
            freshVarCounter++;
            return new Term("X" + freshVarCounter.ToString());
        }

        public Substitution DeepCopy()
        {
            Substitution result = new Substitution();
            foreach (var key in subst.Keys)
            {
                Term value = subst[key];
                result.subst.Add(key.DeepCopy(), value.DeepCopy());
            }
            return result;
        }
    }
}
