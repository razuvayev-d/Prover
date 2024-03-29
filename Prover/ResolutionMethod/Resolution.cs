﻿using Prover.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Prover.ResolutionMethod
{
    /// <summary>
    /// Реализация правила Резолюции
    /// </summary>
    public class Resolution
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
            if (l1 is null || l2 is null)
                throw new Exception("Error in Resolution.Apply: literals are null.");

            if (l1.Negative == l2.Negative) return null;

            //TODO: Разобраться почему этого не было (потому что спрятано в MGU)
            //if (l1.Name != l2.Name)              return null;

            Substitution sigma = Unification.MGU(l1, l2);
            if (sigma is null) return null;

            List<Literal> lits1 = new List<Literal>();

            for (int i = 0; i < clause1.Length; i++)
            {
                var l = clause1[i];
                if (!l.Equals(l1))
                {
                    var newl = l.Substitute(sigma);
                    lits1.Add(newl);
                }
            }

            List<Literal> lits2 = new List<Literal>();
            for (int i = 0; i < clause2.Length; i++)
            {
                var l = clause2[i];
                if (!l.Equals(l2))
                {
                    var newl = l.Substitute(sigma);
                    lits2.Add(newl);
                }
            }

            lits1.AddRange(lits2);
            lits1 = lits1.Distinct().ToList();

            foreach (Literal l in lits1)
                if (l.IsPropTrue)
                    return null;



            Clause res = new Clause(lits1);

            //Clause res = new Clause();

            //res.CreateName();
            //res.AddRange(lits1.Distinct().ToList());

            //res.RemoveDupLits();

            //if (res.Equals(clause1) || res.Equals(clause2))
            //    return null;

            //TODO: убрать, так как эта функциональность поддерживается через TransformNode
            //res.support.Add(clause1.Name);
            //res.support.Add(clause2.Name);

            //clause1.supportsClauses.Add(res.Name);
            //clause2.supportsClauses.Add(res.Name);

            res.Depth = Math.Max(clause1.Depth, clause2.Depth) + 1;
            //res.subst.AddAll(sigma);
            res.SetTransform("resolution", clause1, clause2, sigma, l1.PredicateSymbol);
            //using (StreamWriter sw = new StreamWriter("given_clauses001.txt", true))
            //{
            //    sw.WriteLine("\t" + res.ToString());
            //}
            return res;
        }

        public static Clause Apply(Clause clause1, Clause clause2)
        {
            Clause res;
            for (int i = 0; i < clause1.Length; i++)
                for (int j = 0; j < clause2.Length; j++)
                {
                    res = Apply(clause1, i, clause2, j);
                    if (res is not null)
                    {
                        if (res.Equals(clause1) || res.Equals(clause2))
                            return null;
                        return res;
                    }                        
                }
            return null;
        }

        public static Clause Factor(Clause clause, int lit1, int lit2)
        {
            var l1 = clause[lit1];
            var l2 = clause[lit2];

            if (l1.Negative != l2.Negative) return null;

            Substitution sigma = null;
            sigma = Unification.MGU(l1, l2);
            if (sigma is null)
                return null;
            List<Literal> literals = new List<Literal>();

            for (int i = 0; i < clause.Length; i++)
            {
                Literal l = clause[i];
                l = l.Substitute(sigma);
                literals.Add(l);
            }

            Clause res = new Clause(literals.Distinct().ToList());
            //res.RemoveDupLits();

            res.Depth = clause.Depth + 1;
            //res.subst.AddAll(sigma);
            res.SetTransform("factoring", clause, null, sigma, l1.PredicateSymbol);
    
            return res;
        }
    }
}
