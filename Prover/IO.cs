using Prover.SearchControl;
using Prover.ProofStates;
using Prover.ResolutionMethod;
using System;
using System.IO;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Prover
{
    internal class IO
    {

        public IO() { }

        public static void PrintHelp()
        {
            Console.WriteLine("Параметры прувера: \n");

            Console.WriteLine("-h - справка \n" +
               // "-i - применение индексирования\n" +
                "-t - удаление тавтологий\n" +
                "-b - обратное поглощение\n" +
                "-f - прямое поглощение\n" +
                "-p - вывести доказательство\n" +
                "-s - вывести предварительные преобразования\n" +
                "-T - установить таймаут" +
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
                    case "-t": 
                        param.delete_tautologies = true; break;
                        //break;  param.timeout 
                    case "-p":
                        param.proof = true; break;
                    case "-s":
                        param.simplify = true; break;

                    case "-T":
                        param.timeout = Convert.ToInt32(args[++i]);
                        break;
                    case "-H":
                        var heur = args[++i];
                        param.heuristics = SelectHeuristic(heur);
                        break;
                    case "-S":
                        param.supress_eq_axioms = true; break;
                    case "-ls":
                        //var heur = args[++i];
                        param.literal_selection = args[++i];// LiteralSelection.GetSelector(heur);
                        break;
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

        public static EvaluationScheme SelectHeuristic(string name)
        {
            switch (name)
            {
                case "PickGiven5":
                    return Prover.SearchControl.Heuristics.PickGiven5;
                case "PickGiven2":
                    return Prover.SearchControl.Heuristics.PickGiven2;
                case "SymbolCount":
                    return Prover.SearchControl.Heuristics.SymbolCountEval;
                case "FIFO":
                    return Prover.SearchControl.Heuristics.FIFOEval;
                default:
                    Console.WriteLine("Неизвестная эвристика.");
                    Environment.Exit(-1);
                    break;
            }
            return null;
        }

        public string ReadFile(string path)
        {
            string text = string.Empty;
            try
            {
                
                using (StreamReader sr = new StreamReader(path))
                {
                    text = sr.ReadToEnd();
                    var rg = new Regex("(Status).+(\n)");
                    // TPTPStatus = rg.Match(text).Value;
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка: ");
                Console.WriteLine(ex.Message);
            }
            return text;
        }
    }
}
