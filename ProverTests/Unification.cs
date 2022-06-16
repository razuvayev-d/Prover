using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prover.DataStructures;

namespace ProverTests
{
    [TestClass]
    internal class Unification
    {
        [TestMethod]
        public void Test1()
        {
            var v1 = Term.FromString("X");
            var t1 = Term.FromString("a");
            //var sigma = Unification.MGU()
        }
    }
}
