using Prover.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover.Heuristics
{
    internal static class PriorityFunctions
    {
        /// <summary>
        /// Предпочитать хорновский дизъюнкты (не более 1 положительного литерала)
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static bool PreferHorn(Clause clause)
        {
            int countPositive = 0;
            foreach(var lit in clause.Literals) 
            {
                if (!lit.Negative) countPositive++;
            }
            return countPositive > 1 ? false : true;
        }

        public static bool PreferNonHorn(Clause clause) 
        {
            return !PreferHorn(clause);
        }
        /// <summary>
        /// Предпочитать клаузы без переменных.
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static bool PreferGround(Clause clause) 
        {
            foreach(var lit in clause.Literals)
            {
                if(lit.HasVariables) return false; 
            }
            return true;
        }

        public static bool PreferNonGround(Clause clause)
        { 
            return !PreferGround(clause);   
        }

        /// <summary>
        /// Предпочитать отрицательные клаузы (все литералы отрицательны)
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static bool PreferGoals(Clause clause)
        {
            foreach (var lit in clause.Literals)
            {
                if(!lit.Negative) return false;
            }
            return true;
        }
        /// <summary>
        /// Содержит хотя бы 1 положительный литерал
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static bool PreferNonGoals(Clause clause)
        {
            foreach (var lit in clause.Literals)
            {
                if (!lit.Negative) return true;
            }
            return false;
        }
        /// <summary>
        /// Предпочитать клаузы состоящие из 1 литерала
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static bool PreferUnits(Clause clause)
        {
            return clause.Length == 1;
        }

        public static bool PreferNonUnits(Clause clause)
        {
            return clause.Length != 1;
        }
        /// <summary>
        /// Предпочитать все клаузы (нет разбиения множества на партиции)
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static bool PreferAll(Clause clause) => true;
        /// <summary>
        /// Симулирует стратегию поддержки
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static bool SimulateSOS(Clause clause)
        {
            return clause.from_conjecture;
        }
    }
}
