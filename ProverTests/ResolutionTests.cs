using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prover.DataStructures;
using Prover.ResolutionMethod;
using Prover.Tokenization;

namespace ProverTests
{
    [TestClass]
    public class ResolutionTest
    {
        static Clause c1 = new Clause();
        static Clause c2 = new Clause();
        static Clause c3 = new Clause();
        static Clause c4 = new Clause();
        static Clause c5 = new Clause();
        static Clause c6 = new Clause();
        static Clause c7 = new Clause();
        static Clause c8 = new Clause();
        static Clause c9 = new Clause();
        static Clause c10 = new Clause();
        static Clause c11 = new Clause();
        static Clause c12 = new Clause();
        static Clause c13 = new Clause();

        static ResolutionTest()
        {
            string spec = @"cnf(c1, axiom, p(a, X)|p(X, a)).
                        cnf(c2, axiom,~p(a,b)|p(f(Y), a)).
                        cnf(c3, axiom, p(Z, X)|~p(f(Z),X0)).
                        cnf(c4, axiom, p(X, X)|p(a, f(Y))).
                        cnf(c5, axiom, p(X)|~q|p(a)|~q|p(Y)).
                        cnf(not_p, axiom,~p(a)).
                        cnf(taut, axiom, p(X4)|~p(X4)).
                        ";

            Lexer lex = new Lexer(spec);
            c1 = Clause.ParseClause(lex);
            c2 = Clause.ParseClause(lex);
            c3 = Clause.ParseClause(lex);
            c4 = Clause.ParseClause(lex);
            c5 = Clause.ParseClause(lex);

            string spec2 = "cnf(not_p,axiom,~p(a)).\n" +
                        "cnf(taut,axiom,p(X4)|~p(X4)).\n";
            lex = new Lexer(spec2);
            c6 = Clause.ParseClause(lex);
            c7 = Clause.ParseClause(lex);

            string spec3 = "cnf(00019,plain,disjoint(X212, null_class))." +
                           "cnf(00020,plain,~disjoint(X271, X271)|~member(X270, X271))." +
                           "cnf(c00025,axiom,( product(X1,X1,X1) ))." +
                           "cnf(c00030,plain, ( ~ product(X354,X355,e_1) | ~ product(X354,X355,e_2) ))." +
                           "cnf(c00001,axiom,~killed(X12, X13)|hates(X12, X13))." +
                           "cnf(c00003,axiom,~killed(X3, X4)|~richer(X3, X4)).";
            lex = new Lexer(spec3);
            c8 = Clause.ParseClause(lex);
            c9 = Clause.ParseClause(lex);
            c10 = Clause.ParseClause(lex);
            c11 = Clause.ParseClause(lex);
            c12 = Clause.ParseClause(lex);
            c13 = Clause.ParseClause(lex);
        }
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreNotEqual(Resolution.Apply(c1, 0, c2, 0), null);
        }

        [TestMethod]
        public void TestMethod2()
        {
            Assert.AreEqual(Resolution.Apply(c1, 0, c3, 0), null);
        }

        [TestMethod]
        public void TestMethod3()
        {
            Assert.AreNotEqual(Resolution.Apply(c2, 0, c3, 0), null);
        }
        [TestMethod]
        public void TestMethod4()
        {
            Assert.AreNotEqual(Resolution.Apply(c6, 0, c7, 0), null);
        }

        [TestMethod]
        public void TestMethod5()
        {
            Assert.AreNotEqual(Resolution.Apply(c8, 0, c9, 0), null);
        }

        [TestMethod]
        public void TestMethod6()
        {
            Assert.AreNotEqual(Resolution.Apply(c10, 0, c11, 0), null);
        }

        [TestMethod]
        public void TestMethod7()
        {
            Assert.AreEqual(Resolution.Apply(c12, 0, c13, 0), null);
        }
    }
}
