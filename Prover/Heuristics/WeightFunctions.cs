using Prover.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover.Heuristics
{

 

    delegate int Heuristic(Clause clause);
    internal abstract class ClauseEvaluationFunction
    {
        public abstract int ParamsCount { get; }
        protected string name;
        protected Heuristic hEval;
        public ClauseEvaluationFunction()
        {
            name = "Virtual base";
        }

        public override string ToString()
        {
            return string.Format("ClauseEvalFun(%s)", name);
        }

        public int Call(Clause clause)
        {
            return hEval(clause);
        }
    }

    internal class FIFOEvaluation : ClauseEvaluationFunction
    {
        public override int ParamsCount { get { return 0; } }
        int FIFOCounter;
        public FIFOEvaluation()
        {
            name = "FIFOEval";
            FIFOCounter = 0;
            hEval = (clause) => { return ++FIFOCounter; };
        }
    }

    internal class SymbolCountEvaluation : ClauseEvaluationFunction
    {
        public override int ParamsCount { get { return 2; } }
        int fweight;
        int vweight;
        public SymbolCountEvaluation(int fweight = 2, int vweight = 1)
        {
            this.fweight = fweight;
            this.vweight = vweight;
            name = string.Format("ClauseEvalFun(%s, %s)", fweight, vweight);
            hEval = (clause) => clause.Weight(fweight, vweight);
        }
    }
}
