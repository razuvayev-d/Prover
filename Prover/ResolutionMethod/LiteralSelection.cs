using Prover.DataStructures;
using System;
using System.Collections.Generic;

namespace Prover.ResolutionMethod
{
    public delegate List<Literal> LiteralSelector(List<Literal> literals);
    public static class LiteralSelection
    {
        public static LiteralSelector GetSelector(string funcName)
        {
            switch(funcName)
            {
                case "first":
                    return FirstLit;
                case "small":
                    return SmallesLit;
                case "large":
                    return LargestLit;
                default:
                    throw new ArgumentException("Неизвестная функция выбора литералов");
            }
        }


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

        public static (int, int) VarSizeEval(Literal lit)
        {
            return (lit.CollectVars().Count, -lit.Weight(1, 1));
        }

        //public static VarSizeLit(List<Literal> litlist)
        //{
        //    if(litlist.Count > 0)
        //    {
        //        litlist.Sort(x => VarSizeEval(x));
        //    }
        //}

    }
}
