﻿using Prover.ResolutionMethod;
using Prover.Tokenization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Prover.DataStructures
{
    /// <summary>
    /// Класс, представляющий литерал. Литерал - это подписанный атом. Мы
    /// уже допускаем равносильные атомы с инфиксными "=" или "!="
    /// и нормализуем их при создании.
    /// </summary>
    public class Literal : Formula
    {
        public bool Negative { get; private set; }
        string name;
        public string Name => name;
        public bool HasVariables
        {
            get
            {
                foreach (var term in arguments)
                {
                    if (term.IsVar) return true;
                }
                return false;
            }
        }              
   
        List<Term> arguments = new List<Term>();

        public List<Term> Arguments => arguments;

        static Literal tru = new Literal("$true", new List<Term>()/*{ Term.True }*/);
        static Literal fal = new Literal("$false", new List<Term>() /*{ Term.False }*/);

        public static Literal True => tru;
        public static Literal False => fal;

        public int ConstCount
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < arguments.Count; i++)
                    sum += arguments[i].ConstCount;
                return sum;
            }
        }

        public Literal(string name, List<Term> arguments, bool negative = false) : base(string.Empty, null)
        {

            if(name == "!=")
            {
                this.Negative  =!negative;
                this.name = "=";
                this.arguments = arguments;
            }
            else {
                this.name = name;
                this.arguments = arguments;
                Negative = negative;
            }



        //TODO: разобраться с обработкой равенства при создании литерала
            //if (atom.Func == "!=")
            //{

            //}
            //self.negative = not negative
            //    self.atom = list(["="])
            //    self.atom.extend(termArgs(atom))
            //else:
            //    self.negative = negative
            //    self.atom = atom
            //self.setInferenceLit(True)
        }

        public bool IsInference { get; set; } = true;
        public bool Match(Literal other, Substitution subst)
        {
            int btstate = ((BTSubst)subst).GetState;
            // TODO: зачем я тут написал negative?
            if (Negative != other.Negative) return false;
            if (name != other.name) return false;
            bool res = true;

            //int min = Math.Min(other.arguments.Count, arguments.Count);
            int min = arguments.Count;
            for (int i = 0; i < min; i++)
            {
                res = Term.Match(arguments[i], other.arguments[i], subst);
                if (!res)
                    return false;
            }
            if (res)
                return true;
            ((BTSubst)subst).BacktrackToState((subst, btstate));
            return false;
        }
        public override bool Equals(object obj)
        {
            if (obj is Literal)
            {
                Literal other = (Literal)obj;
                if (Negative != other.Negative) return false;
                if (Name != other.Name) return false;
                if (arguments.Count != other.arguments.Count) return false;
                int n = arguments.Count;
                for (int i = 0; i < n; i++)
                {
                    if (!arguments[i].Equals(other.arguments[i])) return false;
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// True, елсли литералы одинаковы во всем, за исключением знака
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsOpposite(Literal other)
        {
            return this.Negative != other.Negative &&
                this.AtomEquals(other);
        }

        public static bool OppositeInLitList(Literal lit, List<Literal> litList)
        {
            foreach (var l in litList)
                if (l.IsOpposite(lit))
                    return true;
            return false;
        }

        /// <summary>
        /// Сравнивает литералы без учета знака
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool AtomEquals(Literal other)
        {
            if (Name != other.Name) return false;
            if (arguments.Count != other.arguments.Count) return false;
            int n = arguments.Count;
            for (int i = 0; i < n; i++)
            {
                if (!arguments[i].Equals(other.arguments[i])) return false;
            }
            return true;
        }

        public static bool ContainsWithounSigned(List<Literal> coll, Literal literal)
        {
            foreach (Literal c in coll)
            {
                if (c.AtomEquals(literal)) return true;
            }
            return false;
        }

        public List<Term> CollectVars()
        {
            List<Term> vars = new List<Term>();
            if (arguments is null) return vars;
            foreach (Term t in arguments)
                vars.AddRange(t.CollectVars());
            return vars;
        }
        /// <summary>
        /// Для случая когда литерал это кванторная переменная. Возвращает единственный терм. 
        /// Если терм не единственный бросает исключение.
        /// </summary>
        //public Term QuantorVar
        //{
        //    get
        //    {
        //        return new Term(Name, null);
        //        if (arguments.Count > 1)
        //            throw new Exception("Литерал кванторной переменной имеет больше одного аргумента");
        //        return arguments[0];
        //    }
        //}

        public bool IsPropTrue
        {
            get
            {
                //if (arguments is null) return false;
                //if (arguments.Count != 1) return false; // Если аргументов много то очевидно не коннстанта
                return Equals(True);
                return Negative && Term.AtomIsConstFalse(arguments[0])
                    || !Negative && Term.AtomIsConstTrue(arguments[0]);
            }
        }

        public bool IsPropFalse
        {
            get
            {
                //if (arguments is null) return false;
                //if (arguments.Count != 1) return false; // Если аргументов много то очевидно не константа
                return Equals(False);
                return Negative && Term.AtomIsConstTrue(arguments[0])
                    || !Negative && Term.AtomIsConstFalse(arguments[0]);
            }
        }

        public override Signature CollectSig(Signature sig = null)
        {
            if (sig == null) sig = new Signature();

            sig.AddPred(name, arguments.Count);
            foreach (var s in arguments)
                s.CollectSig(sig);
            return sig;
        }
        public override string ToString()
        {
            if (IsEquational)
            {
                var op = "=";
                if(Negative) op = "!=";
                return arguments[0].ToString() + op + arguments[1].ToString();
            }
            else
            {
                StringBuilder result = new StringBuilder();
                if (Negative)
                    result.Append('~');
                result.Append(name);
                if (arguments is not null)
                {
                    var n = arguments.Count;
                    result.Append('(');
                    for (int i = 0; i < n; i++)
                    {
                        result.Append(arguments[i].ToString());
                        if (i < n - 1)
                            result.Append(", ");
                    }
                    result.Append(')');
                }
                return result.ToString();
            }
            
        }
        /// <summary>
        ///  Return a copy of self with oposite polarity.
        /// </summary>
        public Literal Negate()
        {
            //return null;
            if (IsPropTrue)
            {
                return fal;
            }
            else if (IsPropFalse)
            {
                return tru;
            }
            else
            {
                // TODO: Разобраться с отрицанием константных литералов (complete)
                return new Literal(/*"~" + */ name, Term.ListCopy(arguments), !Negative);
            }
        }

        /// <summary>
        /// Принимает на вход два литерала, если они являются контрарной парой возвращает true, иначе false.
        /// </summary>
        /// <returns></returns>
        public static bool IsContrars(Literal one, Literal two)
        {
            if (one.Negative == two.Negative) return false;

            if (one.Negative)
            {
                return one.Equals(two.Negate());
            }
            return two.Equals(one.Negate());
        }

        public Literal Copy()
        {
            throw new NotImplementedException();
            // return new Literal(this.name, arguments.Cl())
        }
        /// <summary>
        /// Применяет указанную подстановку к аргументам литерала
        /// </summary>
        /// <param name="sbst"></param>
        public Literal Substitute(Substitution sbst)
        {
            Literal copy = DeepCopy();
            sbst.Apply(copy.arguments);
            return copy;
        }

        /// <summary>
        /// Возвращает копию литерала с примененной подстановкой
        /// </summary>
        /// <param name="sbst"></param>
        /// <returns></returns>
        public Literal SubstituteWithCopy(Substitution sbst)
        {
            // TODO: этот метод дублирует Substitute
            Literal newLit = DeepCopy();
            return newLit.Substitute(sbst);
            //return newLit;
        }
        /// <summary>
        /// Parse a list of literals separated by "|" (logical or). As per
        ///TPTP 3 syntax, the single word "$false" is interpreted as the
        /// false literal, and ignored.
        /// </summary>
        /// <param name=""></param>
        public static List<Literal> ParseLiteralList(Lexer lexer)
        {
            List<Literal> res = new List<Literal>();

            if (lexer.LookLit() == "$false")
                lexer.Next();
            else
            {
                var lit = ParseLiteral(lexer);
                res.Add(lit);
            }

            while (lexer.TestTok(TokenType.Or))
            {
                lexer.Next();
                if (lexer.LookLit() == "$false")
                    lexer.Next();
                else
                {
                    var lit = ParseLiteral(lexer);
                    res.Add(lit);
                }
            }
            return res;
        }
        /// <summary>
        /// Parse a literal. A literal is an optional negation sign '~',
        /// followed by an atom.
        /// </summary>
        /// <param name="lexer"></param>
        public static Literal ParseLiteral(Lexer lexer)
        {
            var negative = false;
            if (lexer.TestTok(TokenType.Negation))
            {
                negative = true;
                lexer.Next();
            }
            var atom = ParseAtom(lexer);
            atom.Negative = negative;
            return atom;
        }
        public int Weight(int fweight, int vweight)
        {
            var res = fweight;
            foreach (var term in arguments)
                res += term.Weight(fweight, vweight);
            return res;
        }

        public int RefinedWeight(int fweight, int vweight, int term_pen)
        {
            var res = fweight;
            int max = -10000;
            foreach(var term in arguments)
            {
                int weight = term.Weight(fweight, vweight);
                max = weight > max? weight: max;
                res += weight;
            }
            res += (term_pen - 1) * max;
            return res;
        }

        /// <summary>
        /// Абстракция для литерала -- кортеж (знак, предикативный символ). 
        /// Знак -- True если положительный литерал, False если отрицательный. 
        /// </summary>
        /// <returns></returns>
        public PredicateAbstraction PredicateAbstraction()
        {
            return new PredicateAbstraction(!Negative, name);
        }
        public static Literal ParseAtom(Lexer lexer)
        {
            var atom = Term.ParseTerm(lexer);
            if (lexer.TestTok(TokenType.EqualSign, TokenType.NotEqualSign))
            {
                var op = lexer.Next().literal;
                var lhs = atom;
                var rhs = Term.ParseTerm(lexer);
                return new Literal(op, new List<Term> { lhs, rhs });//new List<string> { op, lhs[0], rhs[0] };
            }
            // TODO: замена приведения на функцию
            //return atom.ToLiteral();
            return (Literal)atom;
        }

        /// <summary>
        /// Возвращает копию текущего объекта с примененной подстановкой subst
        /// </summary>
        /// <param name="subst"></param>
        /// <returns></returns>
        public Literal Instantiated(Substitution subst)
        {
            var res = DeepCopy();
            for (int i = 0; i < res.arguments.Count; i++)
            //foreach(var term in res.arguments)
            {
                res.arguments[i] = subst.Apply(res.arguments[i]);
            }
            return res;
        }

        public bool IsEquational => name == "=" || name == "!=";
        public Literal DeepCopy()
        {
            List<Term> newlist = new List<Term>(arguments.Count);
            foreach (var term in arguments)
                newlist.Add(Term.Copy(term));
            return new Literal(name, newlist, Negative);
        }

        /// <summary>
        /// TODO: Хеш-код не зависит от знака! 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int total = Name.GetHashCode() * 3;

            for (int i = 0; i < arguments.Count; i++)
                total += arguments[i].GetHashCode() * i;
            return total;
        }
    }
}
