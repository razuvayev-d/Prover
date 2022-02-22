using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Prover
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var s = "SYN941+1.p";
            FOF(s);
        }

        static void FOF(string Path)
        {
            var problem = new FOFSpec();
            problem.Parse(Path);
            var cnf = problem.Clausify();


            var state = new SimpleProofState(cnf);
            Clause res = state.Saturate();

            if (res.IsEmpty)
                Console.WriteLine("STATUS: THEOREM");
            else
                Console.WriteLine("STATUS: NOT THEOREM");

            var str = new List<string>();
            Print(state, res, str);

            str.Reverse();
            str = str.Distinct().ToList();
            int i = 1;
            Console.WriteLine("\nPROOF: ");
            foreach (string s in str)
                Console.WriteLine(i++ + ". " + s);

            using (StreamWriter sw = new StreamWriter("clauses.txt"))
            {
                foreach (var clause in cnf.clauses)
                    sw.WriteLine(clause.ToString());
            }
        }
        //static int i = 1;
        static void Print(SimpleProofState state, Clause res, List<string> sq) 
        {
            if (res.support.Count == 0)
            {
                var s = res.Name + ": " + res.ToString() + " from: input";
                sq.Add(s);
                return;
            }
            else
            {
                //Console.WriteLine(i++ + ". " + res.Name + ": " + res.ToString() + " from: " + res.support[0] + ", " + res.support[1]);
                sq.Add(res.Name + ": " + res.ToString() + " from: " + res.support[0] + ", " + res.support[1]);
                string Name1, Name2;
                Name1 = res.support[0];
                Name2 = res.support[1];

                var q = state.processed.clauses.Where(x => x.Name == Name1).ToList()[0];
                Print(state, q, sq);
                q = state.processed.clauses.Where(x => x.Name == Name2).ToList()[0];
                Print(state, q, sq);

            }
        }
    }
}
