using Prover.DataStructures;
using System;

namespace Prover.Heuristics
{
    public enum HeuristicsFunctions
    {
        FIFO,
        NegatePrio,
        SymbolCount,
        LIFO,
        ConstPrio
    }
    public delegate int Heuristic(Clause clause);
    public abstract class ClauseEvaluationFunction
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


    internal class NegatePrio : ClauseEvaluationFunction
    {
        public override int ParamsCount { get { return 1; } }
        public NegatePrio(int weight)
        {

            name = "NegatePrio";
            hEval = (clause) =>
            {
                int sum = 0;
                foreach (var lit in clause.Literals)
                    if (lit.Negative) sum += weight;
                return sum;
            };
        }
    }

    internal class ConstPrio : ClauseEvaluationFunction
    {
        public override int ParamsCount { get { return 1; } }
        public ConstPrio(int weight)
        {

            name = "ConstPrio";
            hEval = (clause) =>
            {
                int sum = 0;
                for (int i = 0; i < clause.Literals.Count; i++)
                    sum += clause.Literals[i].ConstCount;
                return sum;
            };
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

    internal class LIFOEvaluation : ClauseEvaluationFunction
    {
        public override int ParamsCount { get { return 0; } }
        int FIFOCounter;
        public LIFOEvaluation()
        {
            name = "FIFOEval";
            FIFOCounter = 0;
            hEval = (clause) => { return --FIFOCounter; };
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

    internal class ClauseWeight : ClauseEvaluationFunction
    {
        public override int ParamsCount => 3;
        int fweight;
        int vweight;
        public ClauseWeight(Predicate<Clause> prio, int fweight, int vweight, int pos_mult)
        {
            this.fweight = fweight;
            this.vweight = vweight;
            name = string.Format("ClauseEvalFun(%s, %s)", fweight, vweight);
            hEval = (clause) =>
            {
                if (prio(clause))  
                    return clause.PositiveWeight(fweight, vweight, pos_mult); 
                return int.MaxValue;
            };
        }
    }


    internal class LIFOEvaluationPrio : ClauseEvaluationFunction
    {
        public override int ParamsCount => 1;
        int FIFOCounter;
        public LIFOEvaluationPrio(Predicate<Clause> prio)
        {
            name = "FIFOEval";
            FIFOCounter = 0;
            hEval = (clause) => {
                if (prio(clause)) 
                    return --FIFOCounter;
                return int.MaxValue;
            };
        }
    }

    internal class FIFOEvaluationPrio : ClauseEvaluationFunction
    {
        public override int ParamsCount { get { return 0; } }
        int FIFOCounter;
        public FIFOEvaluationPrio(Predicate<Clause> prio)
        {
            name = "FIFOEval";
            FIFOCounter = 0;
            hEval = (clause) => 
            {
                if (prio(clause)) 
                    return ++FIFOCounter;
                return int.MaxValue;
            };
        }
    }

    internal class ByLiteralNumber : ClauseEvaluationFunction
    {
        public override int ParamsCount => 0;

        public ByLiteralNumber(Predicate<Clause> prio)
        {
            hEval = (clause) =>
            {
                if (prio(clause))
                    return clause.Length;
                return int.MaxValue;
            };
        }
    }
    /// <summary>
    /// Отдаем предпочление самым длинным выводам
    /// </summary>
    internal class ByDerivationDepth: ClauseEvaluationFunction
    {
        public override int ParamsCount => 0;
        public ByDerivationDepth(Predicate<Clause> prio)
        {
            hEval = (clause) =>
            {
                if (prio(clause))
                    return int.MaxValue - clause.depth; //так как меньше -- лучше. 
                return int.MaxValue;
            };
        }
    }
    /// <summary>
    /// Отдаем предпочление самым коротким выводам
    /// </summary>
    internal class ByDerivationSize : ClauseEvaluationFunction
    {
        public override int ParamsCount => 0;
        public ByDerivationSize(Predicate<Clause> prio)
        {
            hEval = (clause) =>
            {
                if (prio(clause))
                    return clause.depth; 
                return int.MaxValue;
            };
        }
    }



}
