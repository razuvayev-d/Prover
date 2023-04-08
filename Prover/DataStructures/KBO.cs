using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover.DataStructures
{
    class KBO
    {
        private Dictionary<string, int> SymbolWeights { get; set; }

        public KBO(Dictionary<string, int> symbolWeights)
        {
            SymbolWeights = symbolWeights;
        }

        public int Compare(Term t1, Term t2)
        {
            if (t1.FunctionSymbol != t2.FunctionSymbol)
            {
                return SymbolWeights[t1.FunctionSymbol] - SymbolWeights[t2.FunctionSymbol];
            }

            for (int i = 0; i < t1.Arguments.Count && i < t2.Arguments.Count; i++)
            {
                int result = Compare(t1.Arguments[i], t2.Arguments[i]);
                if (result != 0)
                {
                    return result;
                }
            }

            return t1.Arguments.Count - t2.Arguments.Count;
        }

    }
}
