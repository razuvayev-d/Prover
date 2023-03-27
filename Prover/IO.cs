using Prover.Heuristics;
using Prover.ProofStates;
using System;

namespace Prover
{
    internal class IO
    {

        public IO() { }

        public static void PrintHelp()
        {
            Console.WriteLine("Параметры прувера: \n");

            Console.WriteLine("-h - справка \n" +
                "-i - применение индексирования\n" +
                "-b - обратное поглощение\n" +
                "-f - прямое поглощение\n" +
                "-p - вывести доказательство\n" +
                "-s - вывести предварительные преобразования\n" +
                "-t - установить таймаут" +
                "-S - подавить аксиомы равенства"
                );
        }
        public static SearchParams ParamsSplit(string[] args)
        {
            bool flag = true;
            var param = new SearchParams();
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "-h":
                        PrintHelp();
                        Environment.Exit(0);
                        break;
                    case "-i":
                        param.index = true;
                        break;

                    case "-b":
                        param.backward_subsumption = true; break;
                    case "-f":
                        param.forward_subsumption = true; break;
                    //case "-t": 
                    //    //param.delete_tautologies = true; break;
                    //    param.timeout 
                    case "-p":
                        param.proof = true; break;
                    case "-s":
                        param.simplify = true; break;

                    case "-t":
                        param.timeout = Convert.ToInt32(args[++i]);
                        break;
                    case "-H":
                        var heur = args[++i];
                        param.heuristics = SelectHeuristic(heur);
                        break;
                    case "-S":
                            param.supress_eq_axioms= true; break;
                    default:
                        if (flag)
                            param.file = arg;
                        else
                        {
                            Console.WriteLine("Файл указан более одного раза");
                            Environment.Exit(-1);
                        }

                        break;
                }
            }
            return param;
        }

        public static EvalStructure SelectHeuristic(string name)
        {
            switch (name)
            {
                case "PickGiven5":
                    return Prover.Heuristics.Heuristics.PickGiven5;
                case "PickGiven2":
                    return Prover.Heuristics.Heuristics.PickGiven2;
                case "SymbolCount":
                    return Prover.Heuristics.Heuristics.SymbolCountEval;
                case "FIFO":
                    return Prover.Heuristics.Heuristics.FIFOEval;
                default:
                    Console.WriteLine("Неизвестная эвристика.");
                    Environment.Exit(-1);
                    break;
            }
            return null;
        }
    }
}
