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

    }

}
