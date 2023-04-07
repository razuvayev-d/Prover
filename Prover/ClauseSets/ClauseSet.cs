using Prover.DataStructures;
using Prover.Tokenization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prover.ClauseSets
{
    /// <summary>
    /// Класс представляет набор клауз
    /// </summary>
    public class ClauseSet
    {
        public List<Clause> clauses;

        public int Count => clauses.Count;

        public ClauseSet(List<Clause> clauses)
        {
            this.clauses = clauses;
        }

        /// <summary>
        /// Сколько раз встечается каждый предикативный символ
        /// </summary>
        public Dictionary<string, int> PredStats { get; } = new Dictionary<string, int>();
        public ClauseSet()
        {
            this.clauses = new List<Clause>();
        }

        internal void AddRange(ClauseSet clauses)
        {
            this.clauses.AddRange(clauses.clauses);

            foreach (var pair in clauses.PredStats)
                if (PredStats.ContainsKey(pair.Key))
                    PredStats[pair.Key] += pair.Value;
                else
                    PredStats[pair.Key] = pair.Value;
        }

        public virtual void AddClause(Clause clause)
        {
            clauses.Add(clause);
            AddPredStats(clause);
        }

        private void AddPredStats(Clause clause)
        {
            foreach (var pair in clause.PredStats)
                if (PredStats.ContainsKey(pair.Key))
                    PredStats[pair.Key] += pair.Value;
                else
                    PredStats[pair.Key] = pair.Value;
        }

        public Clause ExtractFirst()
        {
            if (clauses.Count > 0)
            {
                var clause = clauses[0];
                clauses.RemoveAt(0);
                return clause;
            }
            else
                return null;
        }


        public Clause this[int i]
        {
            get
            {
                return clauses[i];
            }
        }

        public virtual Clause ExtractClause(Clause clause)
        {
            clauses.Remove(clause);
            return clause;
        }

        /// <summary>
        /// Разбирает последовательность клауз. Возвращает количество разобранных клауз.
        /// </summary>
        /// <param name="lexer"></param>
        /// <returns></returns>
        public int Parse(Lexer lexer)
        {
            int count = 0;
            while (lexer.LookLit() == "cnf")
            {
                var clause = Clause.ParseClause(lexer);
                AddClause(clause);
                count++;
            }
            return count;
        }

        public override string ToString()
        {
            int i = 1;
            StringBuilder sb = new StringBuilder();
            foreach (Clause clause in clauses)
            {
                sb.Append(i++ + ". ");
                sb.Append(clause.Name + ": ");
                sb.Append(clause.ToString());
                sb.Append("\n");
            }
            return sb.ToString();
        }


        /// <summary>
        /// Given a formula in CNF, convert it to a set of clauses.
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        public static List<Clause> FormulaCNFSplit(WFormula f)
        {
            var matrix = f.Formula.GetMatrix();
            var res = new List<Clause>();
            var conjuncts = matrix.ConjToList();

            foreach (var c in conjuncts)
            {
                var disj = c.DisjToList();
                var litlist = new List<Literal>();
                foreach (var l in disj)
                    litlist.Add(l as Literal);
                var clause = new Clause(litlist, f.Type);
                res.Add(clause);
            }
            return res;
        }

        /// <summary>
        /// Вычисляет все бинарные резольвенты между данной клаузой и всеми клаузами в наборе.
        /// 
        /// </summary>
        /// <param name="lit"></param>
        /// <param name="clauseres"></param>
        /// <param name="indices"></param>
        public void GetResolutionLiterals(Literal lit, List<Clause> clauseres, List<int> indices)
        {

            //if (clauseres.Count != 0)
            //    throw new Exception("non empty result variable clauseres passed to ClauseSet.getResolutionLiterals()");

            ////assert clauseres.size() == 0 : "non empty result variable clauseres passed to ClauseSet.getResolutionLiterals()";
            //if (indices.Count != 0)
            //    throw new Exception("non empty result variable indices passed to ClauseSet.getResolutionLiterals()");
            for (int i = 0; i < clauses.Count; i++)
            {
                Clause c = clauses[i];
                for (int j = 0; j < c.Length; j++)
                {
                    if (c[j].IsInference)
                    {
                        clauseres.Add(clauses[i]);
                        indices.Add(j);
                    }
                }
            }
        }

        public virtual List<Clause> GetSubsumingCandidates(Clause clause)
        {
            return clauses;
        }

        public virtual List<Clause> GetSubsumedCandidates(Clause clause)
        {
            return clauses;
        }

        public Signature CollectSig(Signature sig)
        {
            foreach (Clause c in clauses)
                sig = c.CollectSig(sig);
            return sig;
        }

        public ClauseSet AddEqAxioms()
        {
            Signature sig = new Signature();
            sig = CollectSig(sig);
            if (sig.IsPred("="))
            {
                var res = EqAxioms.GenerateEquivAxioms();
                res.AddRange(EqAxioms.GenerateCompatAxioms(sig));
                this.clauses.AddRange(res);
            }
            return this;
        }

    }
}
