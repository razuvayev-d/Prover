using Prover.DataStructures;
using System;
using System.Collections.Generic;

namespace Prover.SearchControl
{
    /// <summary>
    /// Представляют собой эвристическую схему обработки клауз. Схема
    /// содержит несколько различных функций оценки и способ
    /// чередования между ними.
    /// </summary>
    public class EvaluationScheme
    {
        public List<ClauseEvaluationFunction> EvalFunctions = null;
        public List<int> EvalVec = null;

        public List<LiteralSelector> Selectors = new List<LiteralSelector>();

        int current = 0;
        int currentCount = 0;
        public string Name { get; }

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="rating"></param>
        [Obsolete]
        public EvaluationScheme(List<ClauseEvaluationFunction> descriptor, List<int> rating)
        {
            if (descriptor != null && rating != null && descriptor.Count > 0 && rating.Count > 0)
            {
                EvalFunctions = descriptor;
                EvalVec = rating;

                currentCount = EvalVec[0];
            }
        }


        public EvaluationScheme(List<(ClauseEvaluationFunction, int)> eval_descriptor, string name = null)
        {
            if (eval_descriptor.Count == 0) throw new Exception("eval_descriptoir is empty!");
            EvalFunctions = new List<ClauseEvaluationFunction>();
            EvalVec = new List<int>();
            
            foreach (var eval in eval_descriptor)
            {
                EvalFunctions.Add(eval.Item1);
                EvalVec.Add(eval.Item2);
                Selectors.Add(LiteralSelection.NoSelection);
            }
            currentCount = EvalVec[0];
            this.Name = name;
        }

        public void SetSelector(LiteralSelector selector)
        {
            for (int i = 0; i < Selectors.Count; i++)
                Selectors[i] = (selector);
        }

        public EvaluationScheme(List<(ClauseEvaluationFunction, int, LiteralSelector)> eval_descriptor, string name = null)
        {
            if (eval_descriptor.Count == 0) throw new Exception("eval_descriptoir is empty!");
            EvalFunctions = new List<ClauseEvaluationFunction>();
            EvalVec = new List<int>();
            foreach (var eval in eval_descriptor)
            {
                EvalFunctions.Add(eval.Item1);
                EvalVec.Add(eval.Item2);
                Selectors.Add(eval.Item3);
            }
            currentCount = EvalVec[0];
            this.Name = name;
        }

        public EvaluationScheme(ClauseEvaluationFunction cef, int rating, string name = null)
        {
            EvalFunctions = new List<ClauseEvaluationFunction>();
            EvalVec = new List<int>();
            EvalFunctions.Add(cef);
            EvalVec.Add(rating);
            currentCount = EvalVec[0];
            this.Name = name;
        }

        public List<int> Evaluate(Clause clause)
        {
            var evals = new List<int>();
            foreach (var function in EvalFunctions)
                evals.Add(function.Call(clause));
            return evals;
        }

        public LiteralSelector CurrentSelector => Selectors[current];

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
