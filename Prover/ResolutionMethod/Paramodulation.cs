using Prover.DataStructures;
using System.Collections.Generic;

namespace Prover.ResolutionMethod
{
    internal static class Paramodulation
    {

        //public static Clause Apply(Clause clause1, Clause clause2)
        //{
        //    var eqs = clause1.GetEqLiterals();

        //}

        public static List<Literal> GetEqLiterals(this Clause clause)
        {
            List<Literal> result = new List<Literal>();
            foreach (var literal in clause.Literals)
            {
                if (literal.PredicateSymbol == "=" || literal.PredicateSymbol == "!=")
                {
                    result.Add(literal);
                }
            }
            return result;
        }
    }
}
