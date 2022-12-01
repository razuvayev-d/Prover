using Prover.DataStructures;
using System.Collections.Generic;

namespace Prover.Heuristics
{
    static class Heuristics
    {
        public static EvalStructure FIFOEval = new EvalStructure(new FIFOEvaluation(), 1);
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

        public static EvalStructure BreedingBest = new EvalStructure(new List<(ClauseEvaluationFunction, int)> {
                                                                                (new SymbolCountEvaluation(1, 5), 9),
                                                                                (new FIFOEvaluation(), 5),
                                                                                (new LIFOEvaluation(), 7),
                                                                                  (new SymbolCountEvaluation(1,5), 9),
                                                                                  (new FIFOEvaluation(), 5)
                                                                                                               });
    }

}
