using Prover.DataStructures;
using System.Collections.Generic;

namespace Prover.ResolutionMethod
{
    internal static class LiteralSelection
    {
        /// <summary>
        /// Возвращает первый литерал из списка (как список)
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<Literal> FirstLit(List<Literal> list) => new List<Literal>() { list[0] };

        public static List<Literal> SmallesLit(List<Literal> list)
        {
            list.Sort((x, y) => x.Weight(1, 1).CompareTo(y.Weight(1, 1)));
            return new List<Literal>() { list[0] };
        }

        public static List<Literal> LargestLit(List<Literal> list)
        {
            list.Sort((x, y) => y.Weight(1, 1).CompareTo(x.Weight(1, 1)));
            return new List<Literal>() { list[0] };
        }


    }
}
