using Prover.DataStructures;
using Prover.Heuristics;
using System;
using System.Collections.Generic;

namespace Prover.ClauseSets
{
    /// <summary>
    /// Все клаузы этого набора оцениваются эвристическими функциями. Класс поддерживает получение лучшей клаузы в соответствии с любой эвристикой.
    /// </summary>
    internal class HeuristicClauseSet : ClauseSet
    {
        EvalStructure EvalFunctions;
        public HeuristicClauseSet(EvalStructure evalFunctions)
        {
            clauses = new List<Clause>();
            this.EvalFunctions = evalFunctions;
        }

        /// <summary>
        /// Добавляем клаузу в набор. Если в наборе есть эвристики, результаты оценки добавляются в клаузу.
        /// </summary>
        /// <param name="clause"></param>
        public new void AddClause(Clause clause)
        {
            var evals = EvalFunctions.Evaluate(clause);
            clause.AddEval(evals);
            clauses.Add(clause);
        }

        /// <summary>
        /// Возвращает клаузу с минимальным весом по выбранной эвристике. Если список эвристик пуст, null.
        /// </summary>
        /// <param name="heuristicIndex"></param>
        /// <returns></returns>
        public Clause ExtractBestByEval(int heuristicIndex)
        {
            //Console.WriteLine(EvalFunctions.EvalFunctions[heuristicIndex].ToString());
            if (clauses.Count == 0) return null;
            int best = 0;
            int besteval = clauses[0].evaluation[heuristicIndex];

            for (int i = 1; i < clauses.Count; i++)
            {
                if (clauses[i].evaluation[heuristicIndex] < besteval)
                {
                    besteval = clauses[i].evaluation[heuristicIndex];
                    best = i;
                }
            }
            var ret = clauses[best];

            clauses.RemoveAt(best);
            return ret;
        }
        /// <summary>
        /// Извлекает и возвращает следующую лучшую клаузу в соответствии со схемой оценки.
        /// </summary>
        /// <returns></returns>
        public Clause ExtractBest()
        {
            return ExtractBestByEval(EvalFunctions.NextEval);
        }
    }
}
