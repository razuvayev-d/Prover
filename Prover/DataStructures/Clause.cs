using Prover.ResolutionMethod;
using Prover.Tokenization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prover.DataStructures
{
    /// <summary>
    /// Класс, представляющий клаузулу. В настоящее время клаузула состоит из
    /// следующие элементы:
    /// - Cписок литералов.
    /// - Тип ("простой", если не задан).
    /// - Имя(генерируется автоматически, если не задано)
    /// </summary>
    public class Clause : TransformNode
    {
        List<Literal> literals = new List<Literal>();
        string type { get; set; }
        public string name;

        public List<Literal> Literals => literals;
        public string Name => name;

        static int clauseIdCounter = 0;

        public int depth { get; set; } = 0;

        /// <summary>
        /// Содержит оценки в соответствии со схемами. Намример для PickGiven5 {5, 10} означает, что клауза имеет оценку 5 по symbolCount и 10 по LIFO.
        /// </summary>
        public List<int> evaluation = null;
        public string Type => type;
        public bool IsEmpty => literals.Count == 0;
        public int Length => literals.Count;

        public bool IsTautology
        {
            get
            {
                for (int i = 0; i < literals.Count; i++)
                {
                    if (Literal.OppositeInLitList(literals[i], literals.Skip(i + 1).ToList()))
                        return true;
                }
                return false;
            }
        }



        public Dictionary<string, int> PredStats = new Dictionary<string, int>();

        public Literal this[int index]
        {
            get
            {
                return literals[index];
            }
        }

        public static void ResetCounter()
        {
            clauseIdCounter = 0;
        }


        public Clause()
        {
            clauseIdCounter++;
            //PredStats = new Dictionary<string, int>();
        }

        private void CollectPredStats()
        {
           
            foreach (var lit in literals)
                if (PredStats.ContainsKey(lit.Name))
                    PredStats[lit.Name]++;
                else
                    PredStats[lit.Name] = 1;
        }
        //Непонятно почему, но так очень хорошо работает (несоответствие веса функ символов) (2 y y и 1 y x)
        private static int LitComparer(Literal x, Literal y) => x.Weight(2, 1).CompareTo(y.Weight(2, 1));
        
        public Clause(List<Literal> literals, string type = "plain", string name = null) : base(name)
        {
            //Непонятно почему, но так очень хорошо работает (несоответствие веса функ символов)
            literals.Sort(LitComparer);// (x, y) => LitComparer(x, y));
            //Console.WriteLine("\n=======================\n");
            //foreach (var l in literals)
            //{
            //    Console.Write(l.ToString() + ", ");
            //}
            //Console.WriteLine("\nafter\n");
            //literals.Sort((x, y) => y.Negative.CompareTo(x.Negative));
            ////literals.Sort((x, y) => x.Weight(2, 1).CompareTo(y.Weight(2, 1)));
            //Console.WriteLine();
            //foreach (var l in literals)
            //{
            //    Console.Write(l.ToString() +", ");
            //}


            this.literals = literals;
            this.type = type;

            CollectPredStats();
            if (name is not null)
                this.name = name;
            else
                this.name = string.Format("c{0}", clauseIdCounter++);
        }   

        public static Clause ParseClause(Lexer lexer)
        {
            lexer.AcceptLit("cnf");
            lexer.AcceptTok(TokenType.OpenPar);
            var name = lexer.LookLit();
            lexer.AcceptTok(TokenType.IdentLower);
            lexer.AcceptTok(TokenType.Comma);
            var type = lexer.LookLit();
            List<Literal> lits;

            if (!(type == "axiom" || type == "negated_conjecture"))
                type = "plain";
            lexer.AcceptTok(TokenType.IdentLower);
            lexer.AcceptTok(TokenType.Comma);
            if (lexer.TestTok(TokenType.OpenPar))
            {
                lexer.AcceptTok(TokenType.OpenPar);
                lits = Literal.ParseLiteralList(lexer);
                lexer.AcceptTok(TokenType.ClosePar);
            }
            else
            {
                lits = Literal.ParseLiteralList(lexer);
            }

            lexer.AcceptTok(TokenType.ClosePar);
            lexer.AcceptTok(TokenType.FullStop);

            var res = new Clause(lits, type, name);
            res.SetTransform("input");
            return res;
        }

        #region Вычисление веса
        /// <summary>
        /// Возвращает вес клаузы по количеству символов
        /// </summary>
        public int Weight(int fweight, int vweight)
        {
            var res = 0;
            foreach (var literal in literals)
                res += literal.Weight(fweight, vweight);
            return res;
        }

        public int PositiveWeight(int fweight, int vweight, int pos_mult)
        {
            var res = 0;
            foreach (var literal in literals)
            {
                if (literal.Negative)
                    res += literal.Weight(fweight, vweight);
                else
                    res += pos_mult * literal.Weight(fweight, vweight);
            }
            return res;
        }

        public int RefinedWeight(int fweight, int vweight, int term_pem, int lit_pen, int pos_mult)
        {
            var res = 0;
            var max = -10000;
            foreach (var literal in literals)
            {
                var weight = literal.RefinedWeight(fweight, vweight, term_pem);
                max = weight > max ? weight : max;

                if (literal.Negative)
                    res += weight;
                else
                    res += pos_mult * weight;
            }
            res += (lit_pen - 1) * max;
            return res;
        }
        #endregion


        public void AddEval(List<int> eval)
        {
            evaluation = eval;
        }

        public override string ToString()
        {
            if (IsEmpty)
            {
                return "{ }";
            }
            StringBuilder res = new StringBuilder();
            res.Append("{ ");
            foreach (var lit in literals)
                res.Append(lit.ToString() + ", ");
            res.Remove(res.Length - 2, 2);
            res.Append(" }");
            return res.ToString();
        }

        public Clause FreshVarCopy()
        {
            List<Term> vars = CollectVars();
            Substitution s = Substitution.FreshVarSubst(vars.Distinct().ToList());
            return Substitute(s);
        }

        /// <summary>
        /// Возвращает глубокую копию такущей клаузы с примененной подстановкой. 
        /// </summary>
        public Clause Substitute(Substitution subst)
        {
            Clause newC = DeepCopy();
            newC.literals = new List<Literal>();
            for (int i = 0; i < literals.Count; i++)
                newC.literals.Add(literals[i].SubstituteWithCopy(subst));
            return newC;
        }

        public Signature CollectSig(Signature sig)
        {
            foreach (Literal lit in literals)
                sig = lit.CollectSig(sig);
            return sig;
        }
        public List<Term> CollectVars()
        {
            List<Term> res = new List<Term>();
            for (int i = 0; i < literals.Count; i++)
                res.AddRange(literals[i].CollectVars());
            return res;
        }

        public Clause DeepCopy()
        {
            return DeepCopy(0);
        }

        private Clause DeepCopy(int start)
        {

            Clause result = new Clause();
            result.name = name;
            result.type = type;

            /*from TransformNode */
            result.TransformOperation = TransformOperation;
            result.Sbst = Sbst;
            result.LiteralStr = LiteralStr;
            result.Parent1 = Parent1;
            result.Parent2 = Parent2;
            if (Sbst != null)
                result.Sbst = Sbst.DeepCopy();
            result.from_conjecture = from_conjecture;

            result.depth = depth;

            for (int i = start; i < literals.Count; i++)
                result.literals.Add(literals[i].DeepCopy());

            result.PredStats = PredStats.ToDictionary(entry => entry.Key, entry => entry.Value);
            return result;
        }

        public void CreateName() => name = "c" + clauseIdCounter++;

        public void RemoveDupLits()
        {
            literals = literals.Distinct().ToList();
            literals.Sort(LitComparer);
            //literals.Sort((x, y) => x.Weight(1, 1).CompareTo(y.Weight(2, 1)));

            //var lits = new List<Literal>();
            //for (int i = 0; i < literals.Count; i++)
            //    if (!lits.Contains(literals[i]))
            //        lits.Add(literals[i]);
            //literals = lits;

        }

        public void AddRange(List<Literal> literals)
        {
            this.literals.AddRange(literals);
            CollectPredStats();

        }

        public override bool Equals(object obj)
        {
            if (!(obj is Clause)) return false;
            var other = (Clause)obj;
            if (Length != other.Length) return false;

            for (int i = 0; i < Length; i++)
            {
                if (!literals[i].Equals(other.literals[i])) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int total = 0;
            for (int i = 0; i < Length; i++)
                total += literals[i].GetHashCode();
            return total;
        }

        
        private List<Literal> GetNegativeLiterals()
        {
            var res = new List<Literal>();
            foreach(var literal in literals)
            {
                if(literal.Negative) 
                    res.Add(literal);
            }
            return res;
        }
        public void SelectInferenceLiterals(LiteralSelector SelectorFunction)
        {
            var candidates = GetNegativeLiterals();
            if(candidates.Count == 0) return;

            foreach (var literal in literals)
                literal.IsInference = false;

            var selected = SelectorFunction(candidates);
            foreach(var literal in selected)
                literal.IsInference = true;
        }

        public PredicateAbstractionArray PredicateAbstraction()
        {
            List<PredicateAbstraction> abstr = new List<PredicateAbstraction>();
            var n = this.literals.Count;
            for (int i = 0; i < n; i++)
            {
                abstr.Add(this.literals[i].PredicateAbstraction());
            }
            abstr.Sort((x, y) =>
            {
                var c = x.Item1.CompareTo(y.Item1);
                if (c == 0)
                    return x.Item2.CompareTo(y.Item2);
                return c;
            });
            return abstr;
        }
    }



}
