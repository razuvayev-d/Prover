using System;
using System.Collections.Generic;
using Prover.DataStructures;

namespace Prover.Heuristics
{
    internal class RatingEvaluation : ClauseEvaluationFunction
    {
        public override int ParamsCount { get { return 0; } }
        public RatingEvaluation(List<Literal> freqLiterals)
        {
            name = "RatingEvaluation";
            hEval = (clause) =>
            {
                int rt = 0;

                foreach (Literal c in clause.Literals)
                {
                    if (c.Negative)
                        rt += 2;
                    else
                        rt++;
                    if (Literal.ContainsWithounSigned(freqLiterals, c)) //TODO без знака!
                        rt++;
                }

                if (clause.Length == 1) rt++;


                return rt;
            };
        }

    }
}