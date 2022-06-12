using Prover.RosolutionRule;
using Prover.Tokenization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Prover.DataStructures
{
    [DebuggerDisplay("{ToString()}")]
    public class Term : Formula
    {
        //public static TermEqualityComparer Comparer = new();
        public string name;
        // TODO: может оставить subterms null?
        public List<Term> subterms = new List<Term>();
        bool constant;

        public bool Constant
        {
            get { return constant; }
            set { constant = value; }
        }
        //public List<Term> subterms => _subterms;
        static Term falseConstant = new Term("$false", new List<Term>(), constant: true);
        static Term trueConstant = new Term("$true", new List<Term>(), constant: true);
        public static Term False => falseConstant;
        public static Term True => trueConstant;

        public Term() { }
        public Term(string name, List<Term> subterms = null)
        {
            this.name = name;
            if (subterms is not null)
                this.subterms = subterms;
            constant = name == name.ToLower() && (subterms is null || subterms.Count == 0);
        }
        public Term(string name, List<Term> subterms, bool constant)
        {
            this.name = name;
            if (subterms is not null)
                this.subterms = subterms;
            this.constant = constant;
        }

        public int SubtermsCount
        {
            get
            {
                if (IsCompound) return subterms.Count;
                else return 0;
            }
        }

        public static bool Match(Term matcher, Term target, Substitution subst)
        {
            if (subst is not BTSubst) throw new Exception();
            int btstate = ((BTSubst)subst).GetState;
            bool result = true;

            if (matcher.IsVar)
            {
                if (subst.IsBound(matcher))
                {
                    if (!subst[matcher].Equals(target))

                        result = false;
                }
                else ((BTSubst)subst).AddBinding(matcher, target);
            }
            else
            {
                if (target.IsVar || matcher.name != target.name)
                {
                    result = false;
                }
                else
                {
                    for (int i = 0; i < matcher.subterms.Count; i++)
                    {
                        result = Match(matcher.subterms[i], target.subterms[i], subst);
                        if (!result)
                            break;
                    }
                }
            }
            if (result) return true;
            ((BTSubst)subst).BacktrackToState((subst, btstate));
            return false;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append(name);
            // TODO: subterms null
            if (subterms is not null)
            {
                var n = subterms.Count;
                if (n > 0)
                {
                    result.Append('(');
                    for (int i = 0; i < n; i++)
                    {
                        result.Append(subterms[i].ToString());
                        if (i < n - 1)
                            result.Append(", ");
                    }
                    result.Append(')');
                }
            }
            return result.ToString();
        }
        public static Term Copy(Term t)
        {
            List<Term> copy = new List<Term>();
            //if (t.subterms is null) return new Term(t.name, null, t.constant);
            foreach (var v in t.subterms)
            {
                copy.Add(Copy(v));
            }
            return new Term(t.name, copy, t.constant);
        }


        public Term DeepCopy()
        {
            return Copy(this);
        }

        public static List<Term> ListCopy(List<Term> terms)
        {
            //TODO: tems null
            //if (terms is null) return null;
            var copy = new List<Term>(terms.Count);
            foreach (var t in terms)
            {
                copy.Add(Copy(t));
            }
            return copy;
        }

        public Literal ToLiteral()
        {
            //if (constant) throw new Exception("Константа не может быть литерой");
            return new Literal(name, subterms);
        }

        public static implicit operator Literal(Term t)
        {
            //if (t.constant) throw new Exception("Константа не может быть литерой");
            return new Literal(t.name, t.subterms);
        }

        public void AddSubterm(Term t)
        {
            subterms ??= new List<Term>();
            constant = false;
            subterms.Add(t);
        }

        public void AddSubterms(List<Term> t)
        {
            subterms ??= new List<Term>();
            constant = false;
            subterms.AddRange(t);
        }


        public int Weight(int fweight, int vweight)
        {
            if (IsVar) return vweight;
            if (Constant) return fweight;

            var res = fweight;
            foreach (var v in subterms)
            {
                res = res + v.Weight(fweight, vweight);
            }
            return res;
        }


        public static Term ParseTerm(Lexer lexer)
        {
            if (lexer.TestTok(TokenType.IdentUpper))
            {
                return new Term(lexer.Next().literal);
            }


            lexer.CheckTok(TokenType.IdentLower, TokenType.DefFunctor, TokenType.SQString);
            var res = new Term(lexer.Next().literal);

            if (lexer.TestTok(TokenType.OpenPar))
            {
                //Это термин с соответствующими подтерминами, поэтому разберите их
                lexer.AcceptTok(TokenType.OpenPar);
                res.AddSubterms(ParseTermList(lexer));
                lexer.AcceptTok(TokenType.ClosePar);
            }
            else
                res.constant = true;
            return res;
        }

        public List<Term> CollectVars()
        {
            var result = new List<Term>();
            if (IsVar)
            {
                result.Add(this);
            }
            if (subterms is null) return result;
            for (int i = 0; i < subterms.Count; i++)
            {
                var newVars = subterms[i].CollectVars();
                foreach (Term newv in newVars)
                    if (!result.Contains(newv))
                        result.Add(newv);
            }
            return result;
        }

        /// <summary>
        /// Разбор списка термов разделенных запятыми
        /// </summary>
        /// <param name="lexer"></param>
        /// <returns></returns>
        public static List<Term> ParseTermList(Lexer lexer)
        {
            var res = new List<Term>();
            res.Add(ParseTerm(lexer)); //res.Add(new Term(ParseTerm(lexer).name));

            while (lexer.TestTok(TokenType.Comma))
            {
                lexer.AcceptTok(TokenType.Comma);
                res.Add(ParseTerm(lexer)); // res.Add(new Term(ParseTerm(lexer).name));
            }
            return res;
        }

        public bool IsVar => subterms.Count == 0 && !constant; // subterms is null;
        public bool IsCompound => subterms.Count > 0; //subterms is not null;

        public string Func
        {
            get
            {
                if (IsCompound) return name;
                else throw new Exception("Not Func");
            }
        }

        public List<Term> TermArgs
        {
            get
            {
                //if (IsCompound)
                return subterms;
                throw new Exception("Not args");
            }
        }

        public bool IsConstFalse => Equals(False);


        bool TermListEqual(List<Term> l1, List<Term> l2)
        {
            if (l1.Count != l2.Count) return false;
            if (l1.Count == 0) return true;

            var n = l1.Count;
            for (int i = 0; i < n; i++)
            {
                if (!l1[i].Equals(l2[i])) return false;
            }
            return true;
        }

        public static bool AtomIsConstFalse(Term atom) => atom.Equals(False);

        public static bool AtomIsConstTrue(Term atom) => atom.Equals(True);

        public override bool Equals(object obj)
        {
            if (obj is not Formula) return false;
            return Equals(obj as Formula);
        }

        // override object.Equals
        public override bool Equals(Formula obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var t = (Term)obj;
            if (IsVar && t.IsVar || constant && t.constant)
                return name == t.name;
            if (IsVar != t.IsVar) return false;
            if (name != t.name) return false;
            return TermListEqual(TermArgs, t.TermArgs);
        }

        public override int GetHashCode()
        {
            int total = 0;
            if (subterms is null) return total + name.GetHashCode() * 2;
            else
            {
                total = name.GetHashCode() * 2;
                for (int i = 0; i < subterms.Count; i++)
                    total += subterms[i].GetHashCode();
                return total;
            }
        }
    }

}
