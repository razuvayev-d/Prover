using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prover.DataStructures;
using Prover.Tokenization;

namespace ProverTests
{
    [TestClass]
    public class ClauseTests
    {
        static string str1 = @"
cnf(test, axiom, p(a)|p(f(X))).
cnf(test, axiom, (p(a)|p(f(X)))).
cnf(test3, lemma, (p(a)|~p(f(X)))).
cnf(taut, axiom, p(a)|q(a)|~p(a)).
cnf(dup, axiom, p(a)|q(a)|p(a)).
";

        static Clause c1, c2, c3, c4, c5;

        static ClauseTests()
        {
            var lex = new Lexer(str1);
            c1 = Clause.ParseClause(lex);
            c2 = Clause.ParseClause(lex);
            c3 = Clause.ParseClause(lex);
            c4 = Clause.ParseClause(lex);
            c5 = Clause.ParseClause(lex);
        }
        [TestMethod]
        public void InsignificantOuterBrackets()
        {
            Assert.AreEqual(c1, c2);
        }

        [TestMethod]
        public void WeightTest()
        {
            var cf = c1.FreshVarCopy();

            Assert.AreEqual(c1.Weight(2, 1), c2.Weight(2, 1));
            Assert.AreEqual(c1.Weight(1, 1), c2.Weight(1, 1));
        }
        [TestMethod]
        public void ClauseBasedOnLiteralsTest()
        {
            var cnew = new Clause(c1.Literals);

            Assert.AreEqual(c1[0], cnew[0]);
        }
        [TestMethod]
        public void RemoveDupLits()
        {
            var s = c5.Length;
            c5.RemoveDupLits();
            Assert.IsTrue(c5.Length < s);
        }
        [TestMethod]
        public void TautologyTest()
        {
            Assert.IsTrue(c4.IsTautology);
        }
    }
}