using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prover.DataStructures;
using Prover.Tokenization;
using Prover;

namespace ProverTests
{
    [TestClass]
    public class IndexingTest
    {
        static string str1 = @"
cnf(c1,axiom,p(a, X)|p(X,a)).
cnf(c2,axiom,~p(a,b)|p(f(Y),a)).
cnf(c3,axiom,q(Z,X)|~q(f(Z),X0)).
cnf(c4,axiom,p(X,X)|p(a,f(Y))).
cnf(c5,axiom,p(X,Y)|~q(b,a)|p(a,b)|~q(a,b)|p(Y,a)).
cnf(c6,axiom,~p(a,X)).
cnf(c7,axiom, q(f(a),a)).
cnf(c8,axiom, r(f(a))).
cnf(c9,axiom, p(X,Y)).
";

        static Clause c1, c2, c3, c4, c5, c6, c7, c8, c9;

        static IndexingTest()
        {
            var lex = new Lexer(str1);
            c1 = Clause.ParseClause(lex);
            c2 = Clause.ParseClause(lex);
            c3 = Clause.ParseClause(lex);
            c4 = Clause.ParseClause(lex);
            c5 = Clause.ParseClause(lex);
            c6 = Clause.ParseClause(lex);
            c7 = Clause.ParseClause(lex);
            c8 = Clause.ParseClause(lex);
            c9 = Clause.ParseClause(lex);
        }
        [TestMethod]
        public void InsignificantOuterBrackets()
        {
            Assert.AreEqual(c1, c2);
        }
         
        [TestMethod]
        public void TestResolutionInsertRemove()
        {
            var index = new ResolutionIndex();
            index.InsertClause(c1);
            index.InsertClause(c2);

            Assert.AreEqual(index.pos_ind.Count, 1);
            Assert.AreEqual(index.pos_ind["p"].Count, 3);

            Assert.AreEqual(index.neg_ind.Count, 1);
            Assert.AreEqual(index.neg_ind["p"].Count, 1);
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