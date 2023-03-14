using System.Collections.Generic;

namespace Prover.Heuristics
{
    //delegate int Heuristic(Clause clause);
    //internal abstract class ClauseEvaluationFunction
    //{
    //    protected string name;
    //    protected Heuristic hEval;
    //    public ClauseEvaluationFunction()
    //    {
    //        name = "Virtual base";
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("ClauseEvalFun(%s)", name);
    //    }

    //    public int Call(Clause clause)
    //    {
    //        return hEval(clause);
    //    }
    //}

    //internal class FIFOEvaluation : ClauseEvaluationFunction
    //{
    //    int FIFOCounter;
    //    public FIFOEvaluation()
    //    {
    //        name = "FIFOEval";
    //        FIFOCounter = 0;
    //        hEval = (clause) => { return ++FIFOCounter; };
    //    }
    //}

    //internal class LIFOEvaluation : ClauseEvaluationFunction
    //{
    //    int LIFOCounter;
    //    public LIFOEvaluation()
    //    {
    //        name = "LIFOEval";
    //        LIFOCounter = 0;
    //        hEval = (clause) => { return --LIFOCounter; };
    //    }
    //}

    //internal class SymbolCountEvaluation : ClauseEvaluationFunction
    //{
    //    int fweight;
    //    int vweight;
    //    public SymbolCountEvaluation(int fweight = 2, int vweight = 1)
    //    {
    //        this.fweight = fweight;
    //        this.vweight = vweight;
    //        name = string.Format("ClauseEvalFun(%s, %s)", fweight, vweight);
    //        hEval = (clause) => clause.Weight(fweight, vweight);
    //    }
    //}

    public static class Heuristics
    {
        public static EvalStructure FIFOEval { get; } = new EvalStructure(new FIFOEvaluation(), 1);
        /// <summary>
        /// Стратегия предпочтения более коротких дизъюнктов. Меньше символов -- лучше. Требует чтобы была включена подстановка.
        /// </summary>
        public static EvalStructure SymbolCountEval = new EvalStructure(new SymbolCountEvaluation(2, 1), 1);

        /// <summary>
        /// Чередование стратегии предпочтения более коротких дизъюнктов и FIFO с соотношением весов 5 к 1.
        /// </summary>
        public static EvalStructure PickGiven5 = new EvalStructure(new List<(ClauseEvaluationFunction, int)> {
                                                                                (new SymbolCountEvaluation(2, 1), 5),
                                                                                (new FIFOEvaluation(), 1)
                        });
        /// <summary>
        /// Чередование стратегии предпочтения более коротких дизъюнктов и FIFO с соотношением весов 2 к 1.
        /// </summary>                                                                                                      
        public static EvalStructure PickGiven2 = new EvalStructure(new List<(ClauseEvaluationFunction, int)> {
                                                                                (new SymbolCountEvaluation(2, 1), 2),
                                                                                (new FIFOEvaluation(), 1)
                                                                                                               });
        public static EvalStructure InitialBest = new EvalStructure(new List<(ClauseEvaluationFunction, int)> {
                                                                                (new ConstPrio(6), 6),
                                                                                (new NegatePrio(4), 9),
                                                                                (new SymbolCountEvaluation(1,5), 7),
                                                                                  (new SymbolCountEvaluation(0,7), 9),
                                                                                  (new LIFOEvaluation(), 5)
                                                                                                               });

        public static EvalStructure BreedingBest { get; } = new EvalStructure(new List<(ClauseEvaluationFunction, int)> {
                                                                                (new SymbolCountEvaluation(1, 5), 9),
                                                                                (new FIFOEvaluation(), 5),
                                                                                (new LIFOEvaluation(), 7),
                                                                                  (new SymbolCountEvaluation(1,5), 9),
                                                                                  (new FIFOEvaluation(), 5)
                                                                                                               });

        public static EvalStructure BreedingBestPrio { get; } = new EvalStructure(new List<(ClauseEvaluationFunction, int)> {
                                                                                 (new ByLiteralNumber(PriorityFunctions.PreferGoals), 5),
                                                                                 (new FIFOEvaluationPrio(PriorityFunctions.PreferNonGoals), 4),
                                                                                 (new ByLiteralNumber(PriorityFunctions.PreferNonGround), 5),
                                                                                 (new ClauseWeight(PriorityFunctions.PreferHorn, 1, 6, 6), 2),
                                                                                 (new ByDerivationDepth(PriorityFunctions.PreferNonHorn), 4)
                                                                                                                                       });

        public static EvalStructure BreedingBestPrio43 { get; } = new EvalStructure(new List<(ClauseEvaluationFunction, int)> {
                                                                                 (new ByLiteralNumber(PriorityFunctions.PreferUnits), 2),
                                                                                 (new ClauseWeight(PriorityFunctions.PreferNonUnits, 2,5,8), 2),
                                                                                 (new ByDerivationDepth(PriorityFunctions.SimulateSOS), 5),
                                                                                 (new LIFOEvaluationPrio(PriorityFunctions.PreferAll), 3),
                                                                                 (new FIFOEvaluationPrio(PriorityFunctions.PreferUnits), 3)
                                                                                                                                       });
    }

}
