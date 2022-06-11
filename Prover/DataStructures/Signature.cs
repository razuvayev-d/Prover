using System;
using System.Collections.Generic;
using System.Linq;

namespace Prover.DataStructures
{
    /// <summary>
    ///  A signature object, containing function symbols, predicate
    /// symbols, and their associated arities.
    /// </summary>
    public class Signature
    {
        Dictionary<string, int> funs = new Dictionary<string, int>();
        Dictionary<string, int> preds = new Dictionary<string, int>();

        public void AddFun(string f, int arity)
        {
            funs.Add(f, arity);
        }

        public void AddPred(string p, int arity)
        {
            preds.Add(p, arity);
        }

        public bool IsPred(string p)
        {
            return preds.Keys.Contains(p);
        }

        public bool IsFun(string f)
        {
            return funs.Keys.Contains(f);
        }

        public bool IsConstant(string f)
        {
            return IsFun(f) && GetArity(f) == 0;
        }

        public int GetArity(string symbol)
        {
            if (IsFun(symbol))
                return funs[symbol];
            return preds[symbol];
        }
    }
}
