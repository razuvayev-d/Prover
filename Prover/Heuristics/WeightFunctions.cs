using Prover.DataStructures;
using System;

namespace Prover.Heuristics
{

    /// <summary>
    /// Старая версия -- не поддерживает функции приоритетов
    /// </summary>
    //public enum HeuristicsFunctions
    //{
    //    FIFO,
    //    NegatePrio,
    //    SymbolCount,
    //    LIFO,
    //    ConstPrio
    //}
    public enum HeuristicsFunctions
    {
        FIFOPrio,
        ClauseWeight,
        LIFOPrio,
        ByLiteralNumber,
        ByDerivationDepth,
        ByDerivationSize
    }


    public delegate int Heuristic(Clause clause);
    public abstract class ClauseEvaluationFunction
    {
        public abstract int ParamsCount { get; }
        protected string name = "SymbolCount";
        protected Heuristic hEval;
        public ClauseEvaluationFunction()
        {
            name = "Virtual base";
        }

        public override string ToString()
        {
            return string.Format("ClauseEvalFun({0})", name);
        }

        public int Call(Clause clause)
        {
            return hEval(clause);
        }
    }

    #region obsolete clause evaluetion classes (without priority function support)
    [Obsolete]
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

    [Obsolete]
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

    [Obsolete]
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

    [Obsolete]
    internal class LIFOEvaluation : ClauseEvaluationFunction
    {
        public override int ParamsCount { get { return 0; } }
        int FIFOCounter;
        public LIFOEvaluation()
        {
            name = "LIFOEval";
            FIFOCounter = 0;
            hEval = (clause) => { return --FIFOCounter; };
        }
    }

    [Obsolete]
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

    #endregion

    [Serializable]
    public abstract class ClauseEvaluationFunctionWithPrio : ClauseEvaluationFunction
    {
        protected const int NonPrioConstModifier = int.MaxValue / 2;
        public Predicate<Clause> prio { get; protected set; }
    }

    [Serializable]
    internal class ClauseWeight : ClauseEvaluationFunctionWithPrio
    {
        public override int ParamsCount => 3;
        public int fweight { get; }
        public int vweight { get; }
        public int pos_mult { get; }

        public ClauseWeight(Predicate<Clause> prio, int fweight, int vweight, int pos_mult)
        {
            this.prio = prio;
            this.fweight = fweight;
            this.vweight = vweight;
            this.pos_mult = pos_mult;

            name = string.Format("ClauseWeight({0}, {1}, {2})", fweight, vweight, pos_mult);
            hEval = (clause) =>
            {
                if (prio(clause))
                    return clause.PositiveWeight(fweight, vweight, pos_mult);
                return NonPrioConstModifier + clause.PositiveWeight(fweight, vweight, pos_mult); ;
            };
        }
    }

    [Serializable]
    internal class RefinedWeight : ClauseEvaluationFunctionWithPrio
    {
        public override int ParamsCount => 3;
        public int fweight { get; }
        public int vweight { get; }
        public int pos_mult { get; }

        public int lit_pen { get;}
        public int term_pen { get; }

        public RefinedWeight(Predicate<Clause> prio, int fweight, int vweight, int term_pen, int lit_pen, int pos_mult)
        {
            this.prio = prio;
            this.fweight = fweight;
            this.vweight = vweight;
            this.pos_mult = pos_mult;
            this.lit_pen = lit_pen;
            this.term_pen = term_pen;

            name = string.Format("RefinedWeight({0}, {1}, {2}, {3}, {4})", fweight, vweight, term_pen, lit_pen, pos_mult);
            hEval = (clause) =>
            {
                if (prio(clause))
                    return clause.RefinedWeight(fweight, vweight, term_pen, lit_pen, pos_mult);
                return NonPrioConstModifier + clause.RefinedWeight(fweight, vweight, term_pen, lit_pen, pos_mult);
            };
        }
    }

    [Serializable]
    internal class LIFOEvaluationPrio : ClauseEvaluationFunctionWithPrio
    {
        public override int ParamsCount => 1;
        int FIFOCounter;
        public LIFOEvaluationPrio(Predicate<Clause> prio)
        {
            this.prio = prio;
            name = "LIFOprio";
            FIFOCounter = 0;
            hEval = (clause) =>
            {
                if (prio(clause))
                    return --FIFOCounter;
                return NonPrioConstModifier + --FIFOCounter;
            };
        }
    }

    [Serializable]
    internal class FIFOEvaluationPrio : ClauseEvaluationFunctionWithPrio
    {
        public override int ParamsCount { get { return 0; } }
        int FIFOCounter;
        public FIFOEvaluationPrio(Predicate<Clause> prio)
        {
            this.prio = prio;
            name = "FIFOprio";
            FIFOCounter = 0;
            hEval = (clause) =>
            {
                if (prio(clause))
                    return ++FIFOCounter;
                return NonPrioConstModifier + ++FIFOCounter;
            };
        }
    }
    [Serializable]
    internal class ByLiteralNumber : ClauseEvaluationFunctionWithPrio
    {
        public override int ParamsCount => 0;

        public ByLiteralNumber(Predicate<Clause> prio)
        {
            name = "ByLiteralPrio";
            this.prio = prio;
            hEval = (clause) =>
            {
                if (prio(clause))
                    return clause.Length;
                return NonPrioConstModifier + clause.Length;
            };
        }
    }
    /// <summary>
    /// Отдаем предпочление самым длинным выводам
    /// </summary>
    [Serializable]
    internal class ByDerivationDepth : ClauseEvaluationFunctionWithPrio
    {
        public override int ParamsCount => 0;
        public ByDerivationDepth(Predicate<Clause> prio)
        {
            name = "ByDerivationDepthPrio";
            this.prio = prio;
            hEval = (clause) =>
            {
                if (prio(clause))
                    return int.MaxValue / 2 - clause.depth; //так как меньше -- лучше. 
                    return 10000 + NonPrioConstModifier - clause.depth;
            };
        }
    }
    /// <summary>
    /// Отдаем предпочление самым коротким выводам
    /// </summary>
    [Serializable]
    internal class ByDerivationSize : ClauseEvaluationFunctionWithPrio
    {
        public override int ParamsCount => 0;
        public ByDerivationSize(Predicate<Clause> prio)
        {
            name = "ByDerivationSizePrio";
            this.prio = prio;
            hEval = (clause) =>
            {
                if (prio(clause))
                    return clause.depth;
                return 10000 + clause.depth;
            };
        }
    }
}

