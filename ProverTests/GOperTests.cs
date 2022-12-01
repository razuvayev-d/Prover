using Microsoft.VisualStudio.TestTools.UnitTesting;
using Porver.Genetic;
using Prover.Genetic;

namespace ProverTests
{
    [TestClass]
    public class GeneticOperatorsTest
    {
        static string str1 = @"
cnf(test, axiom, p(a)|p(f(X))).
cnf(test, axiom, (p(a)|p(f(X)))).
cnf(test3, lemma, (p(a)|~p(f(X)))).
cnf(taut, axiom, p(a)|q(a)|~p(a)).
cnf(dup, axiom, p(a)|q(a)|p(a)).
";

        static Individual c1, c2, c3, c4, c5;

        static GeneticOperatorsTest()
        {
            c1 = Individual.GreateRandom(6);
            c2 = Individual.GreateRandom(6);
            c3 = Individual.GreateRandom(6);
            c4 = Individual.GreateRandom(6);
            c5 =   Individual.GreateRandom(6);
        }
        [TestMethod]
        public void InsignificantOuterBrackets()
        {
            GeneticOperators.Crossover(c1, c2, 0.7);
            GeneticOperators.Crossover(c3, c4, 0.7);
            GeneticOperators.Crossover(c1, c4, 0.7);
            GeneticOperators.Crossover(c3, c5, 0.7);
        }
    }
}