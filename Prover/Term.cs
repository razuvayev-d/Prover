using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{ 
    [DebuggerDisplay("{ToString()}")]
    class Term : Formula
    {
        public string name;
        public List<Term> subterms;
        bool constant;

        static Term falseConstant = new Term("$false", constant: true);
        static Term trueConstant = new Term("$true", constant: true);
        public static Term False => falseConstant;
        public static Term True => trueConstant;

        public Term() { }
        public Term(string name, List<Term> subterms = null, bool constant = false)
        {
            this.name = name;
            this.subterms = subterms;
            this.constant = name == name.ToLower();
        }

        public int SubtermsCount {
            get
            {
                if (IsCompound) return subterms.Count;
                else return 0;
            }
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append(name);
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
            if (t.subterms is null) return new Term(t.name, null, t.constant);
            foreach(var v in t.subterms)
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
            var copy = new List<Term>(terms.Count);
            foreach (var t in terms)
            {
                copy.Add(Copy(t));
            }
            return copy;
        }

        public Literal ToLitera()
        {
            //if (constant) throw new Exception("Константа не может быть литерой");
            return new Literal(name, subterms);
        }

        public void AddSubterm(Term t)
        {
            subterms ??= new List<Term>();
            subterms.Add(t);
        }

        public void AddSubterms(List<Term> t)
        {
            subterms ??= new List<Term>();
            subterms.AddRange(t);
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

        public bool IsVar => subterms is null;
        public bool IsCompound => subterms is not null;

        public string Func
        {
            get
            {
                if (IsCompound) return name;
                else throw new Exception("Not Func");
            }
        }

        public List<Term> TermArgs{
            get 
            {
                //if (IsCompound)
                    return subterms;
                throw new Exception("Not args");
            }
        }

        public bool IsConstFalse => this.Equals(Term.False);


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

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var t = (Term)obj;
            if (this.IsVar && t.IsVar) return this.name == t.name;
            if (this.IsVar != t.IsVar) return false;
            if (this.Func != t.Func) return false;
            return TermListEqual(this.TermArgs, t.TermArgs);


            // TODO: write your implementation of Equals() here
            throw new NotImplementedException();
            return base.Equals(obj);
        }

        // override object.GetHashCode
        //public override int GetHashCode()
        //{
        //    // TODO: write your implementation of GetHashCode() here
        //    //throw new NotImplementedException();
        //    //return base.GetHashCode();
        //}
    }
}
