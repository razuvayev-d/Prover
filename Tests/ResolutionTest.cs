using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prover;

namespace Tests
{
    [TestClass]
    public class ResolutionTest
    {
        static string spec = @"cnf(c1, axiom, p(a, X)|p(X, a)).
                        cnf(c2, axiom,~p(a,b)|p(f(Y), a)).
                        cnf(c3, axiom, p(Z, X)|~p(f(Z),X0)).
                        cnf(c4, axiom, p(X, X)|p(a, f(Y))).
                        cnf(c5, axiom, p(X)|~q|p(a)|~q|p(Y)).
                        cnf(not_p, axiom,~p(a)).
                        cnf(taut, axiom, p(X4)|~p(X4)).
                        ";


        //public static Clause c1 = new Clause();
        //public static Clause c2 = new Clause();
        //public static Clause c3 = new Clause();
        //public static Clause c4 = new Clause();
        //public static Clause c5 = new Clause();
        //public static Clause c6 = new Clause();
        //public static Clause c7 = new Clause();
        //public static Clause c8 = new Clause();
        //public static Clause c9 = new Clause();
        //public static Clause c10 = new Clause();
        //public static Clause c11 = new Clause();
        //public static Clause c12 = new Clause();
        //public static Clause c13 = new Clause();
        [TestMethod]
        public void TestMethod1()
        {

        }
    }
}