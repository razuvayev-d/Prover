using System;
using System.IO;

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


            using (StreamWriter sw = new StreamWriter("clauses.txt"))
            {
                foreach (var clause in cnf.clauses)
                    sw.WriteLine(clause.ToString());
            }
        }

        static void Print(Clause res) 
        {
            int i = res.depth;
            var tmp1 = res;
            var tmp2 = res;
            //while()
            //Console.WriteLine(i-- ". " + res.Name + ": " res.ToString() + " from: " + res);
        }
    }
}
