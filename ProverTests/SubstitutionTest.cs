using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prover.ClauseSets;
using Prover.DataStructures;
using Prover.ResolutionMethod;
using Prover.Tokenization;

namespace ProverTests
{
    [TestClass]
    public class SubstitutionTest
    {
        static Term t1 = Term.FromString("f(X, g(Y))");
        static Term t2 = Term.FromString("a");
        static Term t3 = Term.FromString("b");
        static Term t4 = Term.FromString("f(a, g(a))");
        static Term t5 = Term.FromString("f(a, g(b))");

        static Substitution sigma1, sigma2;
        static SubstitutionTest()
        {
            sigma1 = new Substitution();
            sigma1.ModifyBinding(new Term("X"), t2);
            sigma1.ModifyBinding(new Term("Y"), t2);

            sigma2 = new Substitution();
            sigma2.ModifyBinding(new Term("X"), t2);
            sigma2.ModifyBinding(new Term("Y"), t3);
        }

        [TestMethod]
        public void BasicTest()
        {
            var tau = sigma1.DeepCopy();
            Assert.IsTrue(Term.Equals(tau[new Term("X")], sigma1[new Term("X")]));
            Assert.IsTrue(Term.Equals(tau[new Term("Y")], sigma1[new Term("Y")]));
            Assert.IsTrue(Term.Equals(tau[new Term("Z")], sigma1[new Term("Z")]));

            var t = tau.ModifyBinding(new Term("X"), t1);
            Assert.IsTrue(Term.Equals(t, t2));

            t = tau.ModifyBinding(new Term("U"), t1);
            Assert.IsTrue(Term.Equals(t, null));
            Assert.IsTrue(tau.IsBound(new Term("U")));
            Assert.IsTrue(Term.Equals(tau[new Term("U")], t1));

            t = tau.ModifyBinding(new Term("U"), null);
            Assert.IsFalse(tau.IsBound(new Term("U")));
        }
        [TestMethod]
        public void ApplyTest()
        {
            Assert.IsTrue(Term.Equals(sigma1.Apply(t1), Term.FromString("f(a,g(a))")));
            Assert.IsTrue(Term.Equals(sigma1.Apply(t1), t4));
            Assert.IsTrue(Term.Equals(sigma2.Apply(t1), t5));
        }


    }
}
