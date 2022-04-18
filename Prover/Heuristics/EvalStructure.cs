using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover.Heuristics
{
    internal class EvalStructure
    {
        public List<ClauseEvaluationFunction> EvalFunctions = null;
        public List<int> EvalVec = null;
        int current = 0;
        int currentCount = 0;
        string name = string.Empty;

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
            get {
                current++;
                if (current >= EvalVec.Count)
                    current = 0;
                currentCount = EvalVec[current];
                return current;
            }
        }
    }
}
