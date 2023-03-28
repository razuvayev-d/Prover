using System;
using System.Collections.Generic;
using System.Linq;

namespace Prover.DataStructures
{
    /// <summary>
    /// Сигнатура содержит предикативные и функциональные символы и соответствущую им арность
    /// </summary>
    public class Signature
    {
        public Dictionary<string, int> funs = new Dictionary<string, int>();
        public Dictionary<string, int> preds = new Dictionary<string, int>();

        public void AddFun(string f, int arity)
        {
            funs[f] = arity;
        }

        public void AddPred(string p, int arity)
        {
            preds[p] = arity;
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
