using Prover.ResolutionMethod;
using Prover.Tokenization;
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
    public class Clause : TransformNode//Derivable
    {
        List<Literal> literals = new List<Literal>();
        string type;
        string name;

        public List<Literal> Literals => literals;
        public string Name => name;
        /// <summary>
        /// Клаузы или формулы из которых получена эта клауза
        /// </summary>
        public List<string> support = new List<string>();
        /// <summary>
        /// Клаузы, которые породила эта клауза
        /// </summary>
        public List<string> supportsClauses = new List<string>();

        static int clauseIdCounter = 0;

        public int depth { get; set; } = 0;
        public string rationale = "input";
        /// <summary>
        /// 
        /// </summary>
        public List<int> evaluation = null;
        public string Type => type;

        public Substitution subst = new Substitution();

        public bool IsEmpty => literals.Count == 0;
        public int Length => literals.Count;

        public bool IsTautology
        {
            get
            {
                //var groups = literals.GroupBy((x) => x.Name);
                //foreach (var group in groups)
                //{
                //    bool f = group.First().Negative;

                //    foreach (var el in group)
                //    {
                //        f ^= el.Negative;
                //    }
                //    if (f) return true;
                //}
                //return false;

                for(int i =0; i< literals.Count; i++)
                {
                    if (Literal.OppositeInLitList(literals[i], literals.Skip(i + 1).ToList())) 
                        return true;
                }
                return false;
            }
        }

        public static void ResetCounter()
        {
            clauseIdCounter = 0;
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
            //res.Derivation = new Derivation("input");
            res.SetTransform("input");
            return res;
        }

        public Clause(List<Literal> literals, string type = "plain", string name = null) : base(name)
        {
            this.literals = literals;
            this.type = type;
            if (name is not null)
                this.name = name;
            else
                this.name = string.Format("c{0}", clauseIdCounter++);
        }

        public Clause(List<Formula> literals, string type = "plain", string name = null) : base(name)
        {
            var n = literals.Count;
            List<Literal> lits = new List<Literal>(n);
            for (int i = 0; i < n; i++)
            {
                // lits[i] = (Literal)literals[i];
                lits.Add((Literal)literals[i]);
            }

            this.literals = lits;
            this.type = type;

            if (name is not null)
                this.name = name;
            else
                this.name = string.Format("c{0}", clauseIdCounter++);
        }
        /// <summary>
        /// Возвращает вес клаузы по количеству символов
        /// </summary>
        /// <param name="fweight"></param>
        /// <param name="vweight"></param>
        /// <returns></returns>
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
                max = weight > max? weight: max;

                if (literal.Negative)
                    res += weight;
                else
                    res += pos_mult * weight;
            }
            res += (lit_pen - 1) * max;
            return res;
        }
        public Clause()
        {
            clauseIdCounter++;
            //this.name = String.Format("c{0}", clauseIdCounter++);
        }

        public Literal this[int index]
        {
            get
            {
                return literals[index];
            }
        }


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
            //res.Append("  rating = " + this.evaluation[0].ToString());
            return res.ToString();
        }

        public Clause FreshVarCopy()
        {
            List<Term> vars = CollectVars();
            Substitution s = Substitution.FreshVarSubst(vars.Distinct().ToList());
            subst.AddAll(s);
            return Substitute(s);
        }
        /// <summary>
        // /** ***************************************************************
        ///Return an instantiated copy of self.Name and type are copied
        /// and need to be overwritten if that is not desired.
        /// </summary>
        /// <param name="subst"></param>
        /// <returns></returns>
        public Clause Substitute(Substitution subst)
        {
            Clause newC = DeepCopy();
            newC.literals = new List<Literal>();
            for (int i = 0; i < literals.Count; i++)
                newC.literals.Add(literals[i].SubstituteWithCopy(subst));
            return newC;
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
        /// <summary>
        /// start is the starting index of the literal list to copy
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public Clause DeepCopy(int start)
        {

            Clause result = new Clause();
            result.name = name;
            result.type = type;
            result.rationale = rationale;
            result.depth = depth;
            for (int i = 0; i < support.Count; i++)
                result.support.Add(support[i]);
            for (int i = start; i < literals.Count; i++)
                result.literals.Add(literals[i].DeepCopy());
            if (subst != null)
                result.subst = subst.DeepCopy();
            result.Parent1 = Parent1;
            result.Parent2 = Parent2;
            if (Sbst != null)
                result.Sbst = Sbst.DeepCopy();
            return result;
        }

        public void CreateName() => name = "c" + clauseIdCounter++;

        public void RemoveDupLits()
        {
            //literals = literals.Distinct().ToList();

            var lits = new List<Literal>();
            for (int i = 0; i < literals.Count; i++)
                //if (!TMP_CONTAINS_LITS(literals[i], lits))
                if (!lits.Contains(literals[i]))
                    lits.Add(literals[i]);
            literals = lits;

        }

        public bool TMP_CONTAINS_LITS(Literal clause, List<Literal> clauses)
        {
            // TODO: замена TMP_CONTAINS_LITS
            foreach (var clause2 in clauses)
                if (clause2.Equals(clause)) return true;
            return false;
        }



        public void AddRange(List<Literal> literals)
        {
            this.literals.AddRange(literals);
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
