using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{
    /// <summary>
    /// Класс представляет набор клауз
    /// </summary>
    class ClauseSet
    {
        public List<Clause> clauses;

        public int Count => clauses.Count;

        public ClauseSet(List<Clause> clauses)
        {
            this.clauses = clauses;
        }

        public ClauseSet()
        {
            this.clauses = new List<Clause>();
        }

        internal void AddRange(ClauseSet clauses)
        {
            this.clauses.AddRange(clauses.clauses);
        }

        public void AddClause(Clause clause)
        {
            clauses.Add(clause);
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

        public Clause extractClause(Clause clause)
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
                var litlist = new List<Formula>();
                foreach (var l in disj)
                    litlist.Add(l);
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
            
            if (clauseres.Count != 0)
                throw new Exception("non empty result variable clauseres passed to ClauseSet.getResolutionLiterals()");

           //assert clauseres.size() == 0 : "non empty result variable clauseres passed to ClauseSet.getResolutionLiterals()";
            if(indices.Count != 0) 
                throw new Exception("non empty result variable indices passed to ClauseSet.getResolutionLiterals()");
            for (int i = 0; i < clauses.Count; i++)
            {
                Clause c = clauses[i];
                for (int j = 0; j < c.Length; j++)
                {
                    clauseres.Add(clauses[i]);
                    indices.Add(j);
                }
            }
        }
    }
}
