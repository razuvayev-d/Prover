﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{
    /// <summary>
    /// Реализация правила Резолюции
    /// </summary>
    class Resolution
    {
        /// <summary>
        /// Применяет правило резолюции к клаузам clause1, clause2.
        /// lit1 и lit2 определяют контрарные литеры в клаузах.
        /// Если резолюция успешна, возвращает резольвенту, в противном случае null. 
        /// </summary>
        /// <param name="clause1"></param>
        /// <param name="lit1"></param>
        /// <param name="clause2"></param>
        /// <param name="lit2"></param>
        /// <returns></returns>
        public static Clause Apply(Clause clause1, int lit1, Clause clause2, int lit2)
        {
            var l1 = clause1[lit1];
            var l2 = clause2[lit2];
            if (l1 == null || l2 == null)
                throw new Exception("Error in Resolution.Apply: literals are null.");

            if (l1.Negative == l2.Negative) return null;

            Substitution sigma = Unification.MGU(l1, l2);

            throw new NotImplementedException();
        }
    }
}