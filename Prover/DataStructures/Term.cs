using Prover.ResolutionMethod;
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

        public string FunctionSymbol;
        public List<Term> Arguments = new List<Term>();
        bool constant;


        public int ConstCount
        {
            get
            {
                if (constant)
                    return 1;
                int sum = 0;
                for (int i = 0; i < Arguments.Count; i++)
                    sum += this.Arguments[i].ConstCount;
                return sum;
            }
        }

        public bool IsConstant
        {
            get { return constant; }
            set { constant = value; }
        }

        static readonly Term falseConstant = new("$false", new List<Term>(), constant: true);
        static readonly Term trueConstant = new("$true", new List<Term>(), constant: true);
        public static Term False => falseConstant;
        public static Term True => trueConstant;

        public Term() { }
        public Term(string name, List<Term> subterms = null)
        {
            this.FunctionSymbol = name;
            if (subterms is not null)
                this.Arguments = subterms;
            constant = name == name.ToLower() && (subterms is null || subterms.Count == 0);
        }
        public Term(string name, List<Term> subterms, bool constant)
        {
            this.FunctionSymbol = name;
            if (subterms is not null)
                this.Arguments = subterms;
            this.constant = constant;
        }

        public static Term FromString(string str)
        {
            return Term.ParseTerm(new Lexer(str));
        }

        public int SubtermsCount
        {
            get
            {
                if (IsCompound) return Arguments.Count;
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
                if (target.IsVar || matcher.FunctionSymbol != target.FunctionSymbol)
                {
                    result = false;
                }
                else
                {
                    for (int i = 0; i < matcher.Arguments.Count; i++)
                    {
                        result = Match(matcher.Arguments[i], target.Arguments[i], subst);
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
            result.Append(FunctionSymbol);
            var n = Arguments.Count;
            if (n > 0)
            {
                result.Append('(');
                for (int i = 0; i < n; i++)
                {
                    result.Append(Arguments[i].ToString());
                    if (i < n - 1)
                        result.Append(", ");
                }
                result.Append(')');
            }
            return result.ToString();
        }
        public static Term Copy(Term t)
        {
            List<Term> copy = new List<Term>();

            foreach (var v in t.Arguments)
            {
                copy.Add(Copy(v));
            }
            return new Term(t.FunctionSymbol, copy, t.constant);
        }


        public Term DeepCopy()
        {
            return Copy(this);
        }

        public static List<Term> ListCopy(List<Term> terms)
        {
            var copy = new List<Term>(terms.Count);
            foreach (var t in terms)
            {
                copy.Add(Copy(t));
            }
            return copy;
        }

        public static implicit operator Literal(Term t)
        {
            //if (t.constant) throw new Exception("Константа не может быть литерой");
            return new Literal(t.FunctionSymbol, t.Arguments);
        }

        public void AddSubterm(Term t)
        {
            Arguments ??= new List<Term>();
            constant = false;
            Arguments.Add(t);
        }

        public void AddSubterms(List<Term> t)
        {
            Arguments ??= new List<Term>();
            constant = false;
            Arguments.AddRange(t);
        }


        public int Weight(int fweight, int vweight)
        {
            if (IsVar) return vweight;
            if (IsConstant) return fweight;

            var res = fweight;
            foreach (var v in Arguments)
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
                //Это термин с подтерминами, поэтому разбираем их
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
            if (Arguments is null) return result;
            for (int i = 0; i < Arguments.Count; i++)
            {
                var newVars = Arguments[i].CollectVars();
                foreach (Term newv in newVars)
                    if (!result.Contains(newv))
                        result.Add(newv);
            }
            return result;
        }

        public override Signature CollectSig(Signature sig = null)
        {
            sig ??= new Signature();

            if (IsCompound)
            {
                sig.AddFun(Func, Arguments.Count);
                foreach (var strm in Arguments)
                    strm.CollectSig(sig);
            }
            return sig;
        }
        /// <summary>
        /// Разбор списка термов разделенных запятыми
        /// </summary>
        public static List<Term> ParseTermList(Lexer lexer)
        {
            var res = new List<Term>();
            res.Add(ParseTerm(lexer));

            while (lexer.TestTok(TokenType.Comma))
            {
                lexer.AcceptTok(TokenType.Comma);
                res.Add(ParseTerm(lexer));
            }
            return res;
        }

        public bool IsVar => Arguments.Count == 0 && !constant; // subterms is null;
        public bool IsCompound => Arguments.Count > 0; //subterms is not null;

        public string Func
        {
            get
            {
                if (IsCompound) return FunctionSymbol;
                else throw new Exception("Not Func");
            }
        }

        public List<Term> TermArgs => Arguments;

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
                return FunctionSymbol == t.FunctionSymbol;
            if (IsVar != t.IsVar) return false;
            if (FunctionSymbol != t.FunctionSymbol) return false;
            return TermListEqual(TermArgs, t.TermArgs);
        }

        public override int GetHashCode()
        {
            int total = 0;
            if (Arguments is null) return total + FunctionSymbol.GetHashCode() * 2;
            else
            {
                total = FunctionSymbol.GetHashCode() * 2;
                for (int i = 0; i < Arguments.Count; i++)
                    total += Arguments[i].GetHashCode();
                return total;
            }
        }
    }

}
