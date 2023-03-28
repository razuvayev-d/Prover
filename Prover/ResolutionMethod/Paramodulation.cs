﻿using Prover.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            foreach(var literal in clause.Literals)
            {
                if(literal.Name == "=" || literal.Name == "!=")
                {
                    result.Add(literal);
                }
            }
            return result;
        }
    }
}
