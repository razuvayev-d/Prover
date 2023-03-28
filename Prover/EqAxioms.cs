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
                    c.TransformOperation = "eq_axiom";
                    res.Add(c);
                }
            }
            catch (Exception pe)
            {
                Console.WriteLine("Error in EqAxioms.generateEquivAxioms(): parse error");
            }
            return res;
        }

        private static List<Term> GenerateVarLists(string x, int n)
        {
            var res = new List<Term>();
            for(int i =0; i < n; i++)            
                res.Add(new Term(x + i.ToString()));
            
            return res;
        } 

        private static List<Literal> GenerateEqPremise(int arity)
        {
            var res = new List<Literal>();
            var xs = GenerateVarLists("X", arity);
            //xs.AddRange(GenerateVarLists("Y", arity));
            var ys = GenerateVarLists("Y", arity);
            for (int i = 0; i < arity; i++)
                res.Add(new Literal("=", new List<Term> { xs[i], ys[i] }, true));
            return res;
        }

        private static Clause GenerateFunCompatAx(string f, int arity)
        {
            var res = GenerateEqPremise(arity);

            Term lterm = new Term(f, GenerateVarLists("X", arity));
            Term rTerm = new Term(f, GenerateVarLists("Y", arity));
           
            Literal concl = new Literal("=", new List<Term> {lterm, rTerm}, false);  

            res.Add(concl);
            var clause = new Clause(res);
            clause.SetTransform("Добавление аксиомы равенства");
            return clause;
        }

        private static Clause GeneratePredCompatAx(string p, int arity)
        {
            var res = GenerateEqPremise(arity);

            res.Add(new Literal(p, GenerateVarLists("X", arity), true)); //negp
            res.Add(new Literal(p, GenerateVarLists("Y", arity), false)); //posp

            var clause = new Clause(res);
            clause.SetTransform("Добавление аксиомы равенства");

            return clause;
        }

        public static List<Clause> GenerateCompatAxioms(Signature sig)
        {
            var res = new List<Clause>();
            foreach(var f in sig.funs)
            {
                int arity = sig.GetArity(f.Key);
                if(arity > 0) 
                {
                    var c = GenerateFunCompatAx(f.Key, arity);
                    res.Add(c);
                } 
            }

            foreach(var p in sig.preds)
            { 
                var arity = sig.GetArity(p.Key);
                if(arity > 0) 
                {
                    var c = GeneratePredCompatAx(p.Key, arity);
                    res.Add(c);
                }
            }
            return res;
        } 
    }
}
