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
        List<Clause> clauses;

        public ClauseSet(List<Clause> clauses)
        {
            this.clauses = clauses;
        }

        public void AddClause(Clause clause)
        {
            clauses.Add(clause);
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
                    litlist.Add(l.Child1);
                var clause = new Clause(litlist, f.Type);
                res.Add(clause);
            }
            return res;
        }
    }
}
