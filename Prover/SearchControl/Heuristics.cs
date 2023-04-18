using System;
using System.Collections.Generic;

namespace Prover.SearchControl
{
    public static class Heuristics
    {
        public static EvaluationScheme FIFOEval { get; } = new EvaluationScheme(new FIFOEvaluation(), 1);
        /// <summary>
        /// Стратегия предпочтения более коротких дизъюнктов. Меньше символов -- лучше. Требует чтобы была включена подстановка.
        /// </summary>
        public static EvaluationScheme SymbolCountEval = new EvaluationScheme(new SymbolCountEvaluation(2, 1), 1);

        [Obsolete]
        public static EvaluationScheme RefinedSOS = new EvaluationScheme(new List<(ClauseEvaluationFunction, int)> {
                                                                           (new RefinedWeight(PriorityFunctions.SimulateSOS, 3, 1, 2, 3, 4), 5),
                                                                           (new LIFOEvaluation(), 1),
                                                                           (new ByDerivationDepth(PriorityFunctions.PreferAll), 2),
                                                                           (new ByLiteralNumber(PriorityFunctions.PreferAll), 1)
                                                                            });

        public static EvaluationScheme PickGiven5Lits = new EvaluationScheme(new List<(ClauseEvaluationFunction, int, LiteralSelector)> {
                                                                                (new SymbolCountEvaluation(2, 1), 5, LiteralSelection.MostFreqLit),
                                                                                (new FIFOEvaluation(), 1, LiteralSelection.LeastFreqLit)
                        }, "PickGiven5");


        public static EvaluationScheme BreedingWithLitSel = new EvaluationScheme(new List<(ClauseEvaluationFunction, int, LiteralSelector)> {
                                                                                 (new ByDerivationDepth(PriorityFunctions.PreferNonGround), 1, LiteralSelection.SmallestAll),
                                                                                 (new FIFOEvaluationPrio(PriorityFunctions.PreferHorn), 2, LiteralSelection.SmallestAll),
                                                                                 (new FIFOEvaluationPrio(PriorityFunctions.PreferNonGoals), 7, LiteralSelection.SmallesLit2),
                                                                                 (new RefinedWeight(PriorityFunctions.PreferNonGround, 8, 7, 8, 5, 4), 1, LiteralSelection.LargestAll),
                                                                                 (new LIFOEvaluationPrio(PriorityFunctions.PreferHorn), 4, LiteralSelection.RandomSelection)
        });


        public static EvaluationScheme InitialWithLitSel = new EvaluationScheme(new List<(ClauseEvaluationFunction, int, LiteralSelector)> {
                                                                                 (new FIFOEvaluationPrio(PriorityFunctions.PreferNonUnits), 5, LiteralSelection.Largest2All),
                                                                        
                                                                                 (new ClauseWeight(PriorityFunctions.PreferAll, 8, 3, 7), 5, LiteralSelection.LargestAll),

                                                                                 (new FIFOEvaluationPrio(PriorityFunctions.PreferNonGoals), 5, LiteralSelection.LargestLitRandom),
                                                                                 (new ByLiteralNumber(PriorityFunctions.PreferHorn),3,  LiteralSelection.SmallesLit),
 
                                                                                 (new ByLiteralNumber(PriorityFunctions.PreferNonHorn), 5, LiteralSelection.Smallest2All)
        });









        /// <summary>
        /// Чередование стратегии предпочтения более коротких дизъюнктов и FIFO с соотношением весов 5 к 1.
        /// </summary>
        public static EvaluationScheme PickGiven5 = new EvaluationScheme(new List<(ClauseEvaluationFunction, int)> {
                                                                                (new SymbolCountEvaluation(2, 1), 5),
                                                                                (new FIFOEvaluation(), 1)
                        }, "PickGiven5");


        public static EvaluationScheme PickGiven5_2 = new EvaluationScheme(new List<(ClauseEvaluationFunction, int)> {
                                                                                (new RefinedWeight(PriorityFunctions.PreferGoals, 2, 1, 3, 4, 5), 1),
                                                                                (new FIFOEvaluation(), 5)
                        });
        /// <summary>
        /// Чередование стратегии предпочтения более коротких дизъюнктов и FIFO с соотношением весов 2 к 1.
        /// </summary>                                                                                                      
        public static EvaluationScheme PickGiven2 = new EvaluationScheme(new List<(ClauseEvaluationFunction, int)> {
                                                                                (new SymbolCountEvaluation(2, 1), 2),
                                                                                (new FIFOEvaluation(), 1)
                                                                                                               }, "PickGiven2");
        public static EvaluationScheme InitialBest = new EvaluationScheme(new List<(ClauseEvaluationFunction, int)> {
                                                                                (new ConstPrio(6), 6),
                                                                                (new NegatePrio(4), 9),
                                                                                (new SymbolCountEvaluation(1,5), 7),
                                                                                  (new SymbolCountEvaluation(0,7), 9),
                                                                                  (new LIFOEvaluation(), 5)
                                                                                                               });

        public static EvaluationScheme BreedingBest { get; } = new EvaluationScheme(new List<(ClauseEvaluationFunction, int)> {
                                                                                (new SymbolCountEvaluation(1, 5), 9),
                                                                                (new FIFOEvaluation(), 5),
                                                                                (new LIFOEvaluation(), 7),
                                                                                  (new SymbolCountEvaluation(1,5), 9),
                                                                                  (new FIFOEvaluation(), 5)
                                                                                                               });

        public static EvaluationScheme BreedingBestPrio { get; } = new EvaluationScheme(new List<(ClauseEvaluationFunction, int)> {
                                                                                 (new ByLiteralNumber(PriorityFunctions.PreferGoals), 5),
                                                                                 (new FIFOEvaluationPrio(PriorityFunctions.PreferNonGoals), 4),
                                                                                 (new ByLiteralNumber(PriorityFunctions.PreferNonGround), 5),
                                                                                 (new ClauseWeight(PriorityFunctions.PreferHorn, 1, 6, 6), 2),
                                                                                 (new ByDerivationDepth(PriorityFunctions.PreferNonHorn), 4)
                                                                                                                                       });

        public static EvaluationScheme BreedingBestPrio43 { get; } = new EvaluationScheme(new List<(ClauseEvaluationFunction, int)> {
                                                                                 (new ByLiteralNumber(PriorityFunctions.PreferUnits), 2),
                                                                                 (new ClauseWeight(PriorityFunctions.PreferNonUnits, 2,5,8), 2),
                                                                                 (new ByDerivationDepth(PriorityFunctions.SimulateSOS), 5),
                                                                                 (new LIFOEvaluationPrio(PriorityFunctions.PreferAll), 3),
                                                                                 (new FIFOEvaluationPrio(PriorityFunctions.PreferUnits), 3)
                                                                                                                                       });
    }

}
