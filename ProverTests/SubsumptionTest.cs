using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prover.ClauseSets;
using Prover.DataStructures;
using Prover.ResolutionMethod;
using Prover.Tokenization;

namespace ProverTests
{


    [TestClass]
    public class SubsumptionTest
    {
        static string spec = @"
cnf(axiom, c2, p(a)).
cnf(axiom, c3, p(X)).
cnf(axiom, c4, p(a)|q(f(X))).
cnf(axiom, c5, p(a)|q(f(b))|p(X)).
cnf(axiom, c6, wise(geoff)).
cnf(axiom, c7, wise(X)).
cnf(axiom, c8, taller(jim,geoff)|wise(geoff)|taller(X,brother_of(X))		).
cnf(axiom, c9, wise(Y)|taller(jim,Z)).
";

        static Clause c1, c2, c3, c4, c5, c6, c7, c8, c9;
        static ClauseSet cset;
        static SubsumptionTest()
        {
            var lex = new Lexer(spec);
            //var c1 = Clause.
            c2 = Clause.ParseClause(lex);
            c3 = Clause.ParseClause(lex);
            c4 = Clause.ParseClause(lex);
            c5 = Clause.ParseClause(lex);
            c6 = Clause.ParseClause(lex);
            c7 = Clause.ParseClause(lex);
            c8 = Clause.ParseClause(lex);
            c9 = Clause.ParseClause(lex);

            cset = new ClauseSet();
            cset.AddClause(c2);
            cset.AddClause(c3);
            cset.AddClause(c4);
            cset.AddClause(c5);
        }

        [TestMethod]
        public void TestSubsumption()
        {
            //var res = Subsumption.Subsumes(c1, c1);
            //Assert.IsTrue(res);


            var res = Subsumption.Subsumes(c2, c2);
            Assert.IsTrue(res);

            res = Subsumption.Subsumes(c3, c3);
            Assert.IsTrue(res);

            res = Subsumption.Subsumes(c4, c4);
            Assert.IsTrue(res);

            //res = Subsumption.Subsumes(c1, c2);
            //Assert.IsTrue(res);

            //res = Subsumption.Subsumes(c2, c1);
            //Assert.IsTrue(!res);

            res = Subsumption.Subsumes(c2, c3);
            Assert.IsTrue(!res);

            res = Subsumption.Subsumes(c3, c2);
            Assert.IsTrue(res);

            res = Subsumption.Subsumes(c4, c5);
            Assert.IsTrue(res);

            res = Subsumption.Subsumes(c5, c4);
            Assert.IsTrue(!res);

            //res = Subsumption.Subsumes(c6, c6);
            //Assert.IsTrue(res);

            //res = Subsumption.Subsumes(c6, c7);
            //Assert.IsTrue(res);
        }

        [TestMethod]
        public void SetSubsumptionTest()
        {
            Assert.IsTrue(Subsumption.Forward(cset, c2));
            // var tmp = Subsumption.Backward(c1, cset);

        }

        [TestMethod]
        public void SubsumptionTest2()
        {
            Assert.IsTrue(Subsumption.Subsumes(c7, c6));
            Assert.IsTrue(Subsumption.Subsumes(c9, c8));
            // var tmp = Subsumption.Backward(c1, cset);

        }
    }
}
