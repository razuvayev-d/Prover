using Prover.DataStructures;
using Prover.Tokenization;
using System;
using System.Collections.Generic;

namespace Prover
{
    internal class EqAxioms
    {

        public static List<Clause> GenerateEquivAxioms()
        {

            Lexer lex = new Lexer("cnf(reflexivity, axiom, X=X)." +
                    "cnf(symmetry, axiom, X!=Y|Y=X)." +
                    "cnf(transitivity, axiom, X!=Y|Y!=Z|X=Z).");
            List<Clause> res = new List<Clause>();
            try
            {
                while (!lex.TestTok(TokenType.EOFToken))
                {
                    Clause c = new Clause();
                    c = Clause.ParseClause(lex);
                    c.rationale = "eq_axiom";
                    res.Add(c);
                }
            }
            catch (Exception pe)
            {
                Console.WriteLine("Error in EqAxioms.generateEquivAxioms(): parse error");
            }
            return res;
        }
    }
}
