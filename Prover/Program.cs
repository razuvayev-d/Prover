using System;

namespace Prover
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var s = "PUZ001+1.p";
            FOF(s);
        }

        static void FOF(string Path)
        {
            var problem = new FOFSpec();
            problem.Parse(Path);
            var cnf = problem.Clausify();

        }
    }
}
