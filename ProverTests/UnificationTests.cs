using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prover.DataStructures;
using Prover.ResolutionMethod;
using System.Collections.Generic;

namespace ProverTests
{
    [TestClass]
    public class UnificationTests
    {
        [TestMethod]
        public void Test1()
        {
            var v1 = Term.FromString("X");
            var t1 = Term.FromString("a");
            var a = new Literal("a", new List<Term> { v1 });
            var b = new Literal("a", new List<Term> { t1 });
            var sigma = Unification.MGU(a, b);
            Assert.IsNotNull(sigma);
        }

        [TestMethod]
        public void Test2()
        {
            var v1 = Term.FromString("X");
            var t1 = Term.FromString("f(X)");
            var a = new Literal("a", new List<Term> { v1 });
            var b = new Literal("a", new List<Term> { t1 });
            var sigma = Unification.MGU(a, b);
            Assert.IsNull(sigma);
        }

        [TestMethod]
        public void Test3()
        {
            var v1 = Term.FromString("X");
            var t1 = Term.FromString("f(Y)");
            var a = new Literal("a", new List<Term> { v1 });
            var b = new Literal("a", new List<Term> { t1 });
            var sigma = Unification.MGU(a, b);
            Assert.IsNotNull(sigma);
        }


    }
}
