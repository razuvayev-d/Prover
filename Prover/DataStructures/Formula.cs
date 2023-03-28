using Prover.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Prover.DataStructures
{
    enum OpType
    {
        Negation,
        Conjunction,
        Disjunction,
        Implication,
        Biconditional,
        Exist,
        All,
        NoOp
    }


    public partial class Formula
    {
        protected string op;
        Formula subFormula1;
        Formula subFormula2;
        public Formula() { }
        public Formula(string op, Formula sub1, Formula sub2 = null)
        {
            this.op = op;
            this.subFormula1 = sub1;
            this.subFormula2 = sub2;
        }

        public bool IsLiteral => this is Literal;
        public bool IsUnary => subFormula2 is null;

        public bool IsBinary => (subFormula1 is not null) && (subFormula2 is not null) && !IsQuantified;

        public bool IsQuantified => op == "!" || op == "?";

        public bool HasSubform1 => (subFormula1 is not null); //IsUnary || IsBinary;

        public bool HasSubform2 => IsQuantified || IsBinary;

        public Formula Child1 => subFormula1;
        public Formula Child2 => subFormula2;


        public override string ToString()
        {
            string arg1 = null;
            string arg2 = null;
            if (subFormula1 != null) arg1 = subFormula1.ToString();
            if (subFormula2 != null) arg2 = subFormula2.ToString();

            if (op == null) return arg1;
            if (op == "~") return "(~" + arg1 + ")";
            if (op == "!" || op == "?")
            {
                return "(" + op + "[" + arg1 + "]:" + arg2 + ")";
            }
            return "(" + arg1 + op + arg2 + ")";
        }

        public static Formula ParseFormula(Lexer lexer)
        {
            var res = ParseUnitaryFormula(lexer);
            if (lexer.TestTok(TokenType.Or, TokenType.And))
            {
                return ParseAssocFormula(lexer, res);
            }
            if (lexer.TestTok(TokenType.Nor, TokenType.Nand, TokenType.Equiv,
                        TokenType.Xor, TokenType.Implies, TokenType.BImplies))
            {
                var op = lexer.LookLit();
                lexer.Next();
                var rest = ParseUnitaryFormula(lexer);
                return new Formula(op, res, rest);
            }
            return res;
        }
        /// <summary>
        /// Разбираем остальную часть формулы которая начинается с head
        /// </summary>
        public static Formula ParseAssocFormula(Lexer lexer, Formula head)
        {
            var op = lexer.LookLit();
            while (lexer.TestLit(op))
            {
                lexer.AcceptLit(op);
                var next = ParseUnitaryFormula(lexer);
                head = new Formula(op, head, next);
            }
            return head;
        }

        public static Formula ParseUnitaryFormula(Lexer lexer)
        {
            if (lexer.TestTok(TokenType.Universal, TokenType.Existential))
            {
                var quantor = lexer.LookLit();
                lexer.Next();
                lexer.AcceptTok(TokenType.OpenSquare);
                return ParseQuatified(lexer, quantor);
            }
            if (lexer.TestTok(TokenType.OpenPar))
            {
                lexer.AcceptTok(TokenType.OpenPar);
                var res = ParseFormula(lexer);
                lexer.AcceptTok(TokenType.ClosePar);
                return res;
            }
            if (lexer.TestTok(TokenType.Negation))
            {
                lexer.AcceptTok(TokenType.Negation);
                var subform = ParseUnitaryFormula(lexer);
                return new Formula("~", subform, null);
            }
            else
            {
                var lit = Literal.ParseLiteral(lexer);
                return lit;
            }
        }

        /// <summary>
        /// Разбирает остаток формулы начиная с данного квантора
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="quantor"></param>
        public static Formula ParseQuatified(Lexer lexer, string quantor)
        {
            lexer.CheckTok(TokenType.IdentUpper);
            var v = Term.ParseTerm(lexer);//.ToLitera(); //!
            Formula rest;
            if (lexer.TestTok(TokenType.Comma))
            {
                lexer.AcceptTok(TokenType.Comma);
                rest = ParseQuatified(lexer, quantor);
            }
            else
            {
                lexer.AcceptTok(TokenType.CloseSquare);
                lexer.AcceptTok(TokenType.Colon);
                rest = ParseUnitaryFormula(lexer);
            }
            return new Quantor(quantor, v, rest);
        }

        /// <summary>
        ///  Возвращает множдество всех предикативных и функциональных символов в этой формуле
        /// </summary>
        public virtual Signature CollectSig(Signature sig = null)
        {
            if (sig is null) sig = new Signature();

            var todo = new Queue<Formula>();
            todo.Enqueue(this);

            while (todo.Count > 0)
            {
                var f = todo.Dequeue();
                if (f.IsLiteral)
                    f.CollectSig(sig);
                else if (f.IsUnary)
                    todo.Enqueue(f.subFormula1);
                else if (f.IsBinary)
                {
                    todo.Enqueue(f.subFormula1);
                    todo.Enqueue(f.subFormula2);
                }
                else
                {
                    if (f.IsQuantified)
                        todo.Enqueue(f.subFormula2);
                    else
                        throw new Exception("Signature form");
                }
            }
            return sig;
        }


        /// <summary>
        /// Возвращает формулу без лидирующих кванторов
        /// </summary>
        public Formula GetMatrix()
        {
            var f = this;
            while (f.IsQuantified)
                f = f.subFormula2;
            return f;
        }

        /// <summary>
        ///  Возвращает список формул, которые были соединены "&" 
        /// </summary>
        public List<Formula> ConjToList()
        {
            if (op == "&")
            {
                var ret = new List<Formula>();
                ret.AddRange(this.subFormula1.ConjToList());
                ret.AddRange(this.subFormula2.ConjToList());
                return ret;
            }
            return new List<Formula> { this };
        }
        /// <summary>
        ///  Возвращает список формул, которые были соединены "|"
        /// </summary>
        public List<Formula> DisjToList()
        {
            if (op == "|")
            {
                var ret = new List<Formula>();
                ret.AddRange(this.subFormula1.DisjToList());
                ret.AddRange(this.subFormula2.DisjToList());
                return ret;
            }
            return new List<Formula> { this };
        }


        /// <summary>
        /// Упрощает формулу удаляя <=, ~|, ~& и <~>
        /// Используем правила:
        ///F ~|G  -> ~(F|G)
        ///F ~&G  -> ~(F&G)
        ///F<=G  -> G=>F
        ///F<~>G -> ~(F<=>G)
        /// </summary>
        public static (Formula, bool) FormulaOpSimplify(Formula f)
        {
            if (f.IsLiteral)
                return (f, false);
            bool modified = false;

            Formula child1, child2;
            bool m;

            if (f.HasSubform1)
            {
                (child1, m) = FormulaOpSimplify(f.subFormula1);
                modified |= m;
            }
            else
            {
                child1 = f.subFormula1;
            }

            if (f.HasSubform2)
            {
                (child2, m) = FormulaOpSimplify(f.subFormula2);
                modified |= m;
            }
            else
            {
                child2 = null;
            }

            if (modified)
            {
                f = new Formula(f.op, child1, child2);
            }

            if (f.op == "<~>")
            {
                var Handle = new Formula("<=>", f.subFormula1, f.subFormula2);
                var newform = new Formula("~", Handle);
                return (newform, true);
            }
            else if (f.op == "<=")
            {
                var newform = new Formula("=>", f.subFormula2, f.subFormula1);
                return (newform, true);
            }
            else if (f.op == "~|")
            {
                var handle = new Formula("|", f.subFormula1, f.subFormula2);
                var newform = new Formula("~", handle);
                return (newform, true);
            }
            else if (f.op == "~&")
            {
                var handle = new Formula("&", f.subFormula1, f.subFormula2);
                var newform = new Formula("~", handle);
                return (newform, true);
            }
            return (f, modified);
        }

        /// <summary>
        /// Упрощает формулу
        /// </summary>
        public static (Formula, bool) FormulaSimplify(Formula f)
        {
            if (f.IsLiteral)
                return (f, false);

            bool modified = false;
            Formula child1, child2;
            bool m;
            if (f.HasSubform1)
            {
                (child1, m) = FormulaSimplify(f.Child1);
                modified |= m;
            }
            else
            {
                child1 = f.Child1;
            }

            if (f.HasSubform2)
            {
                (child2, m) = FormulaSimplify(f.Child2);
                modified |= m;
            }
            else
            {
                child2 = null;
            }

            if (modified)
            {
                f = new Formula(f.op, child1, child2);
            }
            bool topmod = true;

            while (topmod)
            {
                (f, topmod) = FormulaTopSimplify(f);
                modified |= topmod;
            }
            return (f, modified);
        }
        /// <summary>
        ///    Try to apply the following simplification rules to f at the top
        ////    level.Return(f',m), where f' is the result of simplification,
        ////  and m indicates if f'!=f, i.e. if any of the simplification rules
        ////has been applied.
        /// </summary>
        /// <param name="f"></param>
        private static (Formula, bool) FormulaTopSimplify(Formula f)
        {
            if (f.op == "~")
            {
                if (f.Child1.IsLiteral)
                    // По возможности вставляйте ~ в литералы. Это покрывает случай
                    // ~~P -> P, если одно из отрицаний находится в литерале.
                    return ((f.Child1 as Literal).Negate(), true);
                // (new Formula("", ((Literal)f.Child1/*.Child1*/).Negate()), true);
            }
            else if (f.op == "|")
            {
                if (f.Child1.IsPropConst(true)) return (f.Child1, true); //T | P -> T
                if (f.Child2.IsPropConst(true)) return (f.Child2, true); //P | T -> T
                if (f.Child1.IsPropConst(false)) return (f.Child2, true);//F | P -> P
                if (f.Child2.IsPropConst(false)) return (f.Child1, true);//P | F -> P
                if (f.Child1.Equals(f.Child2)) return (f.Child2, true);  //P | P -> P

                //TODO: тождественно ложная контрарная пара
                //if (f.Child1.IsLiteral && f.Child2.IsLiteral)
                //    if (Literal.IsContrars(f.Child1 as Literal, f.Child2 as Literal))
                //        return (Literal.True, true);
            }
            else if (f.op == "&")
            {
                if (f.Child1.IsPropConst(true)) return (f.Child2, true);
                if (f.Child2.IsPropConst(true)) return (f.Child1, true);
                if (f.Child1.IsPropConst(false)) return (f.Child1, true);
                if (f.Child2.IsPropConst(false)) return (f.Child2, true);
                if (f.Child1.Equals(f.Child2)) return (f.Child2, true);

                //TODO: тождественно ложная контрарная пара
                //if (f.Child1.IsLiteral && f.Child2.IsLiteral)
                //    if (Literal.IsContrars(f.Child1 as Literal, f.Child2 as Literal))
                //        return (Literal.False, true);
            }
            else if (f.op == "<=>")
            {
                if (f.Child1.IsPropConst(true)) return (f.Child2, true);
                if (f.Child2.IsPropConst(true)) return (f.Child1, true);
                if (f.Child1.IsPropConst(false))
                {
                    var newform = new Formula("~", f.Child2);
                    (newform, _) = Formula.FormulaSimplify(newform);
                    return (newform, true);
                }
                if (f.Child2.IsPropConst(false))
                {
                    var newform = new Formula("~", f.Child1);
                    (newform, _) = Formula.FormulaSimplify(newform);
                    return (newform, true);
                }
                if (f.Child1.Equals(f.Child2)) return (Literal.True, true);
            }
            else if (f.op == "=>")
            {
                if (f.Child1.IsPropConst(true)) return (f.Child2, true);
                if (f.Child2.IsPropConst(true)) return (Literal.True, true);
                if (f.Child1.IsPropConst(false)) return (Literal.True, true);
                if (f.Child2.IsPropConst(false))
                {
                    var newform = new Formula("~", f.Child1);
                    (newform, _) = FormulaSimplify(newform);
                    return (newform, true);
                }
                if (f.Child1.Equals(f.Child2)) return (Literal.True, true);
            }
            else if (f.op == "!" || f.op == "?")
            {
                var vars = f.Child2.CollectFreeVars();
                if (!vars.Contains(f.Child1))
                    return (f.Child2, true);
            }
            return (f, false);
        }

        public bool IsPropConst(bool polarity)
        {
            if (!IsLiteral)
            {
                if (Child1 is Literal)
                    if (polarity)
                        return ((Literal)this.Child1).IsPropTrue;
                    else
                        return ((Literal)this.Child1).IsPropFalse;
                else if (Child1 is Literal)
                    if (polarity)
                        return ((Literal)this.Child2).IsPropTrue;
                    else
                        return ((Literal)this.Child2).IsPropFalse;
            }
            return false;
        }

        public virtual bool Equals(Formula other)
        {
            if (op != other.op) return false;

            if (IsLiteral)
                return Literal.Equals(this, other);  //(this as Literal).Equals(other as Literal);
            if (IsUnary)
                return subFormula1.Equals(other.subFormula1);
            if (IsQuantified)
                return Term.Equals(subFormula1, other.subFormula2) && subFormula2.Equals(other.subFormula2);
            if (IsBinary)
                return subFormula1.Equals(other.subFormula1) && subFormula2.Equals(other.subFormula2);
            return false;
        }
        /// <summary>
        /// Возвращает набор всех свободных переменных в формуле
        /// </summary>
        /// <returns></returns>
        public List<Term> CollectFreeVars()
        {
            List<Term> res;
            if (IsLiteral)
            {
                return (this as Literal).CollectVars();
            }
            if (IsUnary)
            {
                return subFormula1.CollectFreeVars();
            }
            else if (IsBinary)
            {
                res = subFormula1.CollectFreeVars();
                res.AddRange(subFormula2.CollectFreeVars());
            }
            else
            {
                //Кванторный случай. Сначала мы собираем все свободные переменные в
                // квантифицированной формуле, затем удаляем ту, которая связана с
                // квантификатором.
                if (!IsQuantified) throw new Exception("Ожидался квантор");
                res = subFormula2.CollectFreeVars();
                res = res.Distinct().ToList();
                res.Remove((Term)subFormula1);
            }
            return res;
        }

    }
}

