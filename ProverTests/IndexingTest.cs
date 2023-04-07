using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prover.ClauseSets;
using Prover.DataStructures;
using Prover.Tokenization;
using System.Collections.Generic;

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
        public void TestResolutionInsertRemove()
        {
            var index = new ResolutionIndex();
            index.InsertClause(c1);
            index.InsertClause(c2);

            Assert.AreEqual(index.pos_ind.Count, 1);
            Assert.AreEqual(index.pos_ind["p"].Count, 3);

            Assert.AreEqual(index.neg_ind.Count, 1);
            Assert.AreEqual(index.neg_ind["p"].Count, 1);

            index.InsertClause(c3);
            Assert.AreEqual(index.pos_ind.Count, 2);
            Assert.AreEqual(index.pos_ind["p"].Count, 3);
            Assert.AreEqual(index.neg_ind.Count, 2);
            Assert.AreEqual(index.neg_ind["p"].Count, 1);
            Assert.AreEqual(index.neg_ind["q"].Count, 1);
            Assert.AreEqual(index.pos_ind["q"].Count, 1);

            index.RemoveClause(c3);
            Assert.AreEqual(index.pos_ind.Count, 2);
            Assert.AreEqual(index.pos_ind["p"].Count, 3);

            Assert.AreEqual(index.neg_ind.Count, 2);
            Assert.AreEqual(index.neg_ind["p"].Count, 1);
            Assert.AreEqual(index.neg_ind["q"].Count, 0);
            Assert.AreEqual(index.pos_ind["q"].Count, 0);
        }
        [TestMethod]
        public void ResolutionRetrievalTest()
        {
            var index = new ResolutionIndex();
            index.InsertClause(c1);
            index.InsertClause(c2);
            index.InsertClause(c3);
            index.InsertClause(c4);
            index.InsertClause(c5);

            var lit = c6.Literals[0];
            var cands = index.GetResolutionLiterals(lit);

            Assert.AreEqual(cands.Count, 8);

            foreach (var cand in cands)
            {
                var l = cand.Clause.Literals[cand.Position];
                Assert.AreEqual(l.Negative, !lit.Negative);
                Assert.AreEqual(l.PredicateSymbol, lit.PredicateSymbol);
            }


            lit = c7.Literals[0];
            cands = index.GetResolutionLiterals(lit);

            Assert.AreEqual(cands.Count, 3);

            foreach (var cand in cands)
            {
                var l = cand.Clause.Literals[cand.Position];
                Assert.AreEqual(l.Negative, !lit.Negative);
                Assert.AreEqual(l.PredicateSymbol, lit.PredicateSymbol);
            }

            lit = c8.Literals[0];
            cands = index.GetResolutionLiterals(lit);

            Assert.AreEqual(cands.Count, 0);
        }


        [TestMethod]
        public void PredAbstractionTest()
        {
            var p1 = new List<PredicateAbstraction>();
            var p2 = new List<PredicateAbstraction>() { (true, "p") };
            var p3 = new List<PredicateAbstraction>() { (true, "p"), (true, "p"), (true, "q") };
            var p4 = new List<PredicateAbstraction>() { (false, "p"), (true, "p") };

            Assert.IsTrue(SubsumptionIndex.PredAbstractionIsSubSequence(p1, p1));
            Assert.IsTrue(SubsumptionIndex.PredAbstractionIsSubSequence(p2, p2));
            Assert.IsTrue(SubsumptionIndex.PredAbstractionIsSubSequence(p3, p3));
            Assert.IsTrue(SubsumptionIndex.PredAbstractionIsSubSequence(p4, p4));

            Assert.IsTrue(SubsumptionIndex.PredAbstractionIsSubSequence(p1, p2));
            Assert.IsTrue(SubsumptionIndex.PredAbstractionIsSubSequence(p1, p3));
            Assert.IsTrue(SubsumptionIndex.PredAbstractionIsSubSequence(p1, p4));

            Assert.IsTrue(SubsumptionIndex.PredAbstractionIsSubSequence(p2, p3));
            Assert.IsTrue(SubsumptionIndex.PredAbstractionIsSubSequence(p2, p4));

            Assert.IsFalse(SubsumptionIndex.PredAbstractionIsSubSequence(p2, p1));
            Assert.IsFalse(SubsumptionIndex.PredAbstractionIsSubSequence(p3, p1));
            Assert.IsFalse(SubsumptionIndex.PredAbstractionIsSubSequence(p4, p1));

            Assert.IsFalse(SubsumptionIndex.PredAbstractionIsSubSequence(p3, p2));
            Assert.IsFalse(SubsumptionIndex.PredAbstractionIsSubSequence(p4, p2));

            Assert.IsFalse(SubsumptionIndex.PredAbstractionIsSubSequence(p3, p4));
            Assert.IsFalse(SubsumptionIndex.PredAbstractionIsSubSequence(p4, p3));
        }
        [TestMethod]
        public void TestSubsumptionIndex()
        {
            var index = new SubsumptionIndex();

            Assert.IsFalse(index.IsIndexed(c1));
            Assert.IsFalse(index.IsIndexed(c6));

            index.InsertClause(c1);
            index.InsertClause(c2);
            index.InsertClause(c3);
            index.InsertClause(c4);
            index.InsertClause(c5);
            index.InsertClause(c6);

            Assert.IsTrue(index.IsIndexed(c1));
            Assert.IsTrue(index.IsIndexed(c2));
            Assert.IsTrue(index.IsIndexed(c3));
            Assert.IsTrue(index.IsIndexed(c4));
            Assert.IsTrue(index.IsIndexed(c5));
            Assert.IsTrue(index.IsIndexed(c6));

            index.RemoveClause(c1);
            index.RemoveClause(c5);
            index.RemoveClause(c3);

            Assert.IsTrue(!index.IsIndexed(c1));
            Assert.IsTrue(index.IsIndexed(c2));
            Assert.IsTrue(!index.IsIndexed(c3));
            Assert.IsTrue(index.IsIndexed(c4));
            Assert.IsTrue(!index.IsIndexed(c5));
            Assert.IsTrue(index.IsIndexed(c6));

            index.InsertClause(c3);
            index.InsertClause(c1);
            index.InsertClause(c5);
            index.InsertClause(c9);

            Assert.IsTrue(index.IsIndexed(c1));
            Assert.IsTrue(index.IsIndexed(c2));
            Assert.IsTrue(index.IsIndexed(c3));
            Assert.IsTrue(index.IsIndexed(c4));
            Assert.IsTrue(index.IsIndexed(c5));
            Assert.IsTrue(index.IsIndexed(c6));
            Assert.IsTrue(index.IsIndexed(c9));

            var cands = index.GetSubsumingCandidates(c1);
            Assert.AreEqual(cands.Count, 3);

            cands = index.GetSubsumingCandidates(c9);
            Assert.AreEqual(cands.Count, 1);

            cands = index.GetSubsumedCandidates(c9);
            Assert.AreEqual(cands.Count, 5);

            cands = index.GetSubsumedCandidates(c8);
            Assert.AreEqual(cands.Count, 0);

            cands = index.GetSubsumedCandidates(c5);
            Assert.AreEqual(cands.Count, 1);
        }
    }
}