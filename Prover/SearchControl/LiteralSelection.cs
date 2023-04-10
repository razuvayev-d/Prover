using Prover.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Prover.SearchControl
{
    public delegate List<Literal> LiteralSelector(List<Literal> literals);
    public static class LiteralSelection
    {
        public static ClauseSets.ClauseSet ClausesInProblems;
        static Random r = new Random();
        public static LiteralSelector GetSelector(string funcName)
        {
            switch (funcName)
            {
                case "first":
                    return FirstLit;
                case "small":
                    return SmallesLit;
                case "large":
                    return LargestLit;
                case "small2":
                    return SmallesLit2;
                case "large2":
                    return LargestLit2;
                case "random":
                    return RandomSelection;
                case "largerandom":
                    return LargestLitRandom;
                case "eq":
                    return EqLit;
                case "mostfreq":
                    return MostFreqLit;
                default:
                    throw new ArgumentException("Неизвестная функция выбора литералов");
            }
        }
        public static List<Literal> MostFreqLit(List<Literal> list)
        {
            var lit = ClausesInProblems.PredStats.MaxBy(x => x.Value).Key;
            var res = list.Where(x => x.PredicateSymbol == lit).ToList();
            if (res.Count > 0)
                return res;
            else return RandomSelection(list);
        }

        /// <summary>
        /// Возвращает первый литерал из списка (как список)
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<Literal> FirstLit(List<Literal> list) => new List<Literal>() { list[0] };

        public static List<Literal> SmallesLit(List<Literal> list)
        {
            //list.Sort((x, y) => x.Weight(1, 1).CompareTo(y.Weight(1, 1)));
            var x = list.MinBy(x => x.Weight(1, 1));

            return new List<Literal>() { x };
        }

        public static List<Literal> SmallesLit2(List<Literal> list)
        {
            //list.Sort((x, y) => x.Weight(2, 1).CompareTo(y.Weight(2, 1)));
            var x = list.MinBy(x => x.Weight(2, 1));
            return new List<Literal>() { x };
        }

        public static List<Literal> EqLit(List<Literal> list)
        {
            var lits = new List<Literal>();
            foreach (var x in list)
            {
                if (x.IsEquational)
                    lits.Add(x);
            }
            return lits;
        }
        public static List<Literal> LargestLit(List<Literal> list)
        {
            //list.Sort((x, y) => y.Weight(1, 1).CompareTo(x.Weight(1, 1)));

            list.Sort((x, y) => y.WeightCache.Weight11.CompareTo(y.WeightCache.Weight11));
            //var x = list.MaxBy(x => x.Weight(1, 1));
            return new List<Literal>() { list[list.Count - 1] };
        }

        public static List<Literal> LargestLitRandom(List<Literal> list)
        {
            //list.Sort((x, y) => y.Weight(1, 1).CompareTo(x.Weight(1, 1)));
            var max = list.Select(x => x.Weight(1, 1)).Max();
            var ret = list.Where(x => x.Weight(1, 1) == max).ToList();

            return new List<Literal>() { ret[r.Next(ret.Count)] };
        }

        public static List<Literal> LargestLit2(List<Literal> list)
        {
            var x = list.MaxBy(x => x.Weight(2, 1));
            return new List<Literal>() { x };
        }

        public static (int, int) VarSizeEval(Literal lit)
        {
            return (lit.CollectVars().Count, -lit.Weight(1, 1));
        }


        public static List<Literal> NegativeLit(List<Literal> list)
        {
            return list.Where(x => x.Negative).ToList();
        }

        public static List<Literal> NegativeLitLargest(List<Literal> list)
        {
            var ret = list.Where(x => x.Negative).ToList();
            return LargestLit2(ret);
        }

        public static List<Literal> NegativeLitSmallest(List<Literal> list)
        {
            var ret = list.Where(x => x.Negative).ToList();
            return SmallesLit2(ret);
        }

        public static List<Literal> RandomSelection(List<Literal> list)
        {
            return new List<Literal> { list[r.Next(list.Count)] };
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
