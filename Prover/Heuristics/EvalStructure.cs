using Prover.DataStructures;
using System;
using System.Collections.Generic;

namespace Prover.Heuristics
{
    /// <summary>
    /// Представляют собой эвристическую схему обработки клауз. Схема
    /// содержит несколько различных функций оценки и способ
    /// чередования между ними.
    /// </summary>
    public class EvalStructure
    {
        public List<ClauseEvaluationFunction> EvalFunctions = null;
        public List<int> EvalVec = null;
        int current = 0;
        int currentCount = 0;
        string name = string.Empty;

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="rating"></param>
        public EvalStructure(List<ClauseEvaluationFunction> descriptor, List<int> rating)
        {
            if (descriptor != null && rating != null && descriptor.Count > 0 && rating.Count > 0)
            {
                EvalFunctions = descriptor;
                EvalVec = rating;

                currentCount = EvalVec[0];
            }
        }


        public EvalStructure(List<(ClauseEvaluationFunction, int)> eval_descriptor)
        {
            if (eval_descriptor.Count == 0) throw new Exception("eval_descriptoir is empty!");
            EvalFunctions = new List<ClauseEvaluationFunction>();
            EvalVec = new List<int>();
            foreach (var eval in eval_descriptor)
            {
                EvalFunctions.Add(eval.Item1);
                EvalVec.Add(eval.Item2);
            }
            currentCount = EvalVec[0];
        }

        public EvalStructure(ClauseEvaluationFunction cef, int rating)
        {
            EvalFunctions = new List<ClauseEvaluationFunction>();
            EvalVec = new List<int>();
            EvalFunctions.Add(cef);
            EvalVec.Add(rating);
            currentCount = EvalVec[0];
        }

        public List<int> Evaluate(Clause clause)
        {
            var evals = new List<int>();
            foreach (var function in EvalFunctions)
                evals.Add(function.Call(clause));
            return evals;
        }

        public int NextEval
        {
            get
            {
                currentCount--;
                if (currentCount >= 0)
                { 
                    return current;
                }
                else
                {
                    current = (current + 1) % EvalVec.Count;
                    currentCount = EvalVec[current] - 1;
                    return current;
                }
            }
        }
    }
}
