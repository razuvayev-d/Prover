using Prover.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prover.ResolutionMethod
{
    /// <summary>
    /// Подстановки отображают переменные на термы. Подстановки, используемые здесь
    /// всегда полностью развернуты, т.е.каждая переменная привязана непосредственно к
    /// термином, к которому она привязана.
    /// </summary>
    public class Substitution
    {
        public Dictionary<Term, Term> subst = new Dictionary<Term, Term>(/*Term.Comparer*/);
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
        /// Возвращает True, если var связана в этой подстановке, false в противном случае.
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

        // TODO: разобраться почему не работает стандартный метод
        bool ContainsKey(Term key)
        {
            foreach (Term term in subst.Keys)
                if (term.Equals(key)) return true;
            return false;
        }

        /// <summary>
        /// Apply the substitution to a term. Return the result.
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public Term Apply(Term term)
        {
            if (term.IsConstant) return term;

            if (term.IsVar)
            {
                if (subst.ContainsKey(term))
                //if (ContainsKey(term))
                {
                    return subst[term];
                }
                else
                {
                    return term;
                    //res.name = term.name;
                }
            }
            else
            {
                Term res = new Term();
                res.FunctionSymbol = term.FunctionSymbol;
                var n = term.SubtermsCount;
                for (int i = 0; i < n; i++)
                    res.AddSubterm(Apply(term.Arguments[i]));
                return res;
            }
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
            var it = subst.Keys.GetEnumerator();

            while (it.MoveNext())
            {
                Term key = it.Current;
                Term bound = subst[key];
                subst[key] = tmpSubst.Apply(bound);
                //subst.Add(key, tmpSubst.Apply(bound));
            }

            //foreach (var x in vars)
            //{
            //    var bound = subst[x];
            //    subst[x] = tmpSubst.Apply(bound);
            //}
            if (!subst.ContainsKey(var))
                subst[var] = term;
        }

        public static Substitution FreshVarSubst(List<Term> vars)
        {

            Substitution s = new Substitution();
            for (int i = 0; i < vars.Count; i++)
            {
                Term newVar = FreshVar();
                // TODO: подумать не заменить ли Dictionary на List<(Term, Term)> или аналог
                //s.subst.Add(vars[i], newVar);
                if (s.subst.ContainsKey(vars[i]))
                    s.subst[vars[i]] = newVar;
                else
                    s.subst.Add(vars[i], newVar);
            }
            return s;
        }

        public void AddAll(Substitution s)
        {
            var it = s.subst.Keys.GetEnumerator();

            while (it.MoveNext())
            {
                Term t1 = it.Current;
                Term t2 = s.subst[t1];
                //subst.Add(t1, t2);
                subst[t1] = t2;
            }
        }

        public static Term FreshVar()
        {
            freshVarCounter++;
            return new Term(string.Format("X{0}", freshVarCounter));
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("{ ");
            foreach (var sub in subst)
            {
                sb.Append(string.Format("{1}/{0}, ", sub.Key, sub.Value));
            }
            sb[sb.Length - 2] = ' ';
            sb[sb.Length - 1] = '}';
            return sb.ToString();
        }
    }

    /// <summary>
    /// Подстановка, которая не позволяет создавать новые привязки, но взамен позволяет backward_subst
    /// </summary>
    public class BTSubst : Substitution
    {
        //Dictionary<Term, Term> bindings;
        public BTSubst(Dictionary<Term, Term> init)
        {
            subst = init;
        }
        public BTSubst()
        {
        }
        public int GetState => subst.Count;

        public bool BackTrack()
        {
            if (subst.Count == 0) return false;

            var tmp = subst.Last();
            subst.Remove(tmp.Key);
            return true;
        }

        public int BacktrackToState((Substitution, int) btState)
        {
            var (substq, state) = btState;
            if (substq != this) return 0;// throw new Exception();
            int res = 0;

            while (subst.Count > state)
            {
                BackTrack();
                res++;
            }
            return res;
        }

        public void AddBinding(Term var, Term term)
        {
            subst[var] = term;
        }

        //TODO: подумать что сделать с этим безобразием
        public new void ComposeBinding(Term var, Term term)
        {
            throw new NotImplementedException("Для этого класса запрещено");
        }
    }
}
