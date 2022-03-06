using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{
    class Skolem
    {
        private static int skolemCount = 0;
        public static string NewSkolemSymbol()
        {
            return String.Format("skolem{0}", ++skolemCount);
        }

        public static Term NewSkolemTerm(List<Term> varlist)
        {
            Term res = new Term();
            
            res.name = NewSkolemSymbol();
            var n = varlist.Count;
            if (n == 0) res.Constant = true;
            for (int i = 0; i < n; i++)
            {
                Term v = varlist[i];
                res.AddSubterm(v);
            }
            return res;
        }
    }
}
