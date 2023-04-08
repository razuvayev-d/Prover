using Prover.ClauseSets;
using Prover.DataStructures;
using Prover.Genetic;
using Prover.ProofStates;
using Prover.Tokenization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Prover
{
    class Program
    {
        static int Count = 0;
        static string problemsDirectory = @"./problems/"; // @"./TrainTask"; //@"./problems/";
        static string answersDirectory = @"./answers/";

        static List<string> solved = new List<string>();


        static bool indexing = false;
        static bool statonly = false;
        static void Main(string[] args)
        {
            GeneticBreeding();
            //
            string[] files = Directory.GetFiles(problemsDirectory);
            var s = problemsDirectory + "SYN941+1.p";
            //FOF(s);
            //FOFFull(problemsDirectory + "SYN966+1.p");
            // string path = Console.ReadLine();
            //string path = "SYN918+1.p";// "LCL664+1.001.p";// "SYN969 +1.p";
            //FOFFull(path);

            var param = IO.ParamsSplit(args);
            param.index = true;
            //param.delete_tautologies = true;
            //param.forward_subsumption = true;
            //param.backward_subsumption = true;
            //param.delete_tautologies = true;
            //param.heuristics = Heuristics.Heuristics.PickGiven5;

            //param.literal_selection = "large2"; //"largerandom";// largerandom"; //"large";
            //param.literal_selection = "mostfreq";

            param.heuristics = Heuristics.Heuristics.RefinedSOS;
            param.degree_of_parallelism = 1;
            //param.timeout = 105000;
            FOFFullClear(param.file, param);


            //if (args[0] == "-i") indexing = true;
            //////if (args[1] == "-so") statonly = true;
            //FOFFull(args[args.Length - 1]);
            //param.heuristics = Heuristics.Heuristics.BreedingBestPrio;
            //param.timeout = 10000;
            //param.backward_subsumption = true;
            //param.forward_subsumption = true;
            //param.simplify = false;
            //foreach (string file in files)
            //    FOFFullClear(file, param);
            //////Rating(file);

            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine("Solved: {0} / {1}", Count, files.Length);
            //Console.WriteLine("Solved problems:");
            //Console.ResetColor();

            //foreach (var x in solved)
            //{
            //    Console.WriteLine(x);
            //}
        }

        static void FOFFullClear(string Path, SearchParams param = null)
        {

            Report report = new Report();
            report.ProblemName = Path;
            Clause.ResetCounter();
            string timeoutStatus = string.Empty;
            param ??= new SearchParams()
            {
                backward_subsumption = true,
                forward_subsumption = true,
                heuristics = Heuristics.Heuristics.PickGiven5,
                delete_tautologies = true,
                timeout = 100000
            };

            var problem = new FOFSpec();
            
            problem.Parse(Path);
            
            List<Clause> eqaxioms = null;
            if (!param.supress_eq_axioms)
            {
                eqaxioms = problem.AddEqAxioms();
                var set = new ClauseSet(eqaxioms);
                report.AxiomStr = set.ToString();
            }




            //string formulastr = CreateFormulaList(problem.formulas);
            report.FormulaStr = CreateFormulaList(problem.formulas);

            var cnf = problem.Clausify();
            string ClausesStr = cnf.ToString();

            var state = new ProofState(param, cnf, false, indexing);


            report.Params = param;
            report.ClausesStr = cnf.ToString();
            report.problem = problem;
            report.State = state;


            Stopwatch stopwatch = new Stopwatch();
            Clause res;
            CancellationTokenSource token = new CancellationTokenSource();
            state.token = token;
            if (param.timeout != 0)
            {
                var tsk = new Task<Clause>(() => state.Saturate());

                stopwatch.Restart();
                tsk.Start();
                bool complete = tsk.Wait(param.timeout);
                stopwatch.Stop();
                token.Cancel();

                if (complete)
                {
                    res = tsk.Result;
                }
                else
                {
                    res = null;
                    report.Timeout = true;
                    //var a = state.unprocessed.clauses.MaxBy(x => x.depth);
                    //var b = state.processed.clauses.MaxBy(x => x.depth);
                    //state.statistics.search_depth = Math.Max(a.depth, b.depth);
                    //state.statistics.search_depth = Math.Max(state.unprocessed.clauses.Select(x => x.depth).Max(), state.processed.clauses.Select(x => x.depth).Max());
                }
            }
            else
            {
                stopwatch.Restart();
                res = state.Saturate();
                stopwatch.Stop();
            }

            if (res is not null && res.IsEmpty)
            {
                Count++;
                state.statistics.depth = res.depth;
            }
            state.statistics.ElapsedTime = stopwatch.Elapsed.TotalMilliseconds;
            report.Res = res;
            //if (state.statistics.search_depth == 0)
            //{
            //    var list = state.unprocessed.clauses.Select(x => x.depth).ToList();
            //    //var a = state.unprocessed.clauses.MaxBy(x => x.depth).;
            //    var amax = list.Max();
            //    var b = state.processed.clauses.MaxBy(x => x.depth);
            //    var bmax = b.depth;
            //    state.statistics.search_depth = Math.Max(amax, bmax);
            //}

            report.ConsolePrint();
            report.FilePrint(answersDirectory);
        }

        static void GeneticBreeding()
        {
            GeneticOptions options = new GeneticOptions();
            options.Size = 35;
            options.MaxNumberOfGeneration = 50;
            options.GenerationTimeOutThreshold = 100;
            options.probWeight = 0.5;
            options.LightTimeOut = 3000;
            options.probParam = 0.4;
            options.Mode = GeneticOptions.GeneticMode.CreateNewPopulation;
            options.PopulationFileName = "InitialPopulation.txt";

            SearchParams sp = new SearchParams()
            {
                delete_tautologies = true,
                backward_subsumption = true,
                forward_subsumption = true,
                literal_selection = "large"
            };

            GeneticAlgorithm algorithm = new GeneticAlgorithm(options, sp);

            Console.WriteLine("Начато в {0}", DateTime.Now);
            algorithm.Evolution();
        }

        #region Остальное

        static void FOFFull(string Path, SearchParams param = null)
        {

            Console.WriteLine("\n\nЗадача " + Path);
            Console.WriteLine();
            Clause.ResetCounter();
            string timeoutStatus = string.Empty;
            param ??= new SearchParams()
            {
                backward_subsumption = true,
                forward_subsumption = true,
                heuristics = Heuristics.Heuristics.BreedingBestPrio,
                delete_tautologies = true,
                timeout = 100000
            };
            //param.heuristics = Heuristics.Heuristics.PickGiven5;
            string TPTPStatus;
            using (StreamReader sr = new StreamReader(Path))
            {
                string text = sr.ReadToEnd();
                var rg = new Regex("(Status).+(\n)");
                TPTPStatus = rg.Match(text).Value;
            }

            var problem = new FOFSpec();
            problem.Parse(Path);
            if (!param.supress_eq_axioms)
                problem.AddEqAxioms();

            string formulastr = CreateFormulaList(problem.formulas);

            var cnf = problem.Clausify();
            string ClausesStr = cnf.ToString();

            var state = new ProofState(param, cnf, false, indexing);

            Stopwatch stopwatch = new Stopwatch();
            Clause res;
            CancellationTokenSource token = new CancellationTokenSource();
            state.token = token;
            if (param.timeout != 0)
            {
                var tsk = new Task<Clause>(() => state.Saturate());

                stopwatch.Restart();
                tsk.Start();
                bool complete = tsk.Wait(param.timeout);
                stopwatch.Stop();
                token.Cancel();



                if (complete)
                {
                    res = tsk.Result;

                }
                else
                {
                    res = null;
                    timeoutStatus = " Время истекло";
                }
            }
            else
            {
                stopwatch.Restart();
                res = state.Saturate();
                stopwatch.Stop();
            }

            if (res is null)
            {
                Console.WriteLine("Доказательство не найдено.", ConsoleColor.Red);
                Console.WriteLine(timeoutStatus);


                Console.WriteLine("\nПосле преобразований получены следующие клаузы: ");
                Console.WriteLine(ClausesStr);

                var R = new Report(state, -1)
                {
                    Result = "Не теорема. " + timeoutStatus,
                    //HeuristicName = param.heuristics.ToString()
                };

                Console.WriteLine("\nСтатистика: \n");
                R.statistics.ElapsedTime = stopwatch.Elapsed.TotalMilliseconds;
                Console.WriteLine(R.statistics.ToString());

                string jsn = JsonSerializer.Serialize(R,
              new JsonSerializerOptions()
              {
                  WriteIndented = true,
                  Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping //JavaScriptEncoder.Create(UnicodeRanges.All) 
              });
                using (StreamWriter sw = new StreamWriter(answersDirectory + System.IO.Path.GetFileNameWithoutExtension(Path) + ".json"))
                {

                    sw.WriteLine(jsn);
                }
                return;
            }
            else if (res.IsEmpty)
            {
                Count++;
                string verdict = res.IsEmpty ? "STATUS: THEOREM" : "STATUS: NOT THEOREM";
                var str = new List<string>();
                //Console.WriteLine(Path);
                //Print(state, res, str);

                str.Reverse();
                str = str.Distinct().ToList();
                int i = 1;
                //Console.WriteLine("\nPROOF: ");
                //foreach (string s in str)
                //    Console.WriteLine(i++ + ". " + s);


                i = 1;
                StringBuilder initialCl = new StringBuilder();
                foreach (var s in cnf.clauses)
                    initialCl.Append(i++ + ". " + s.ToString() + "\n");

                i = 1;
                StringBuilder proof = new StringBuilder();
                foreach (string s in str)
                    proof.Append(i++ + ". " + s + "\n");

                var report = new Report(state, res.depth)
                {
                    ProblemName = System.IO.Path.GetFileName(Path),
                    Formula = formulastr,
                    Result = verdict,
                    TPTPResult = TPTPStatus,
                    InitialClauses = initialCl.ToString(),
                    Proof = proof.ToString(),
                    HeuristicName = param.heuristics.ToString()
                };
                report.statistics.ElapsedTime = stopwatch.Elapsed.TotalMilliseconds;
                StreamWriter r = new StreamWriter(System.IO.Path.Combine(answersDirectory, System.IO.Path.GetFileName(Path)));


                Console.WriteLine("Доказательство найдено!\n", ConsoleColor.Green);
                r.WriteLine("Доказательство найдено!\n");

                Console.WriteLine("Прочитанная формула: ");
                r.WriteLine("Прочитанная формула: ");
                Console.WriteLine(formulastr);
                r.WriteLine(formulastr);

                if (param.simplify)
                {
                    var CnfForms = problem.clauses.Select(clause => clause.Parent1).Distinct().ToList();

                    Console.WriteLine("Преобразования в клаузы: ");
                    r.WriteLine("Преобразования в клаузы: ");
                    for (int j = 0; j < CnfForms.Count; j++)
                    {
                        if (CnfForms[j] is null) continue;
                        Console.WriteLine("   Формула " + (j + 1));
                        r.WriteLine("   Формула " + (j + 1));

                        Console.WriteLine("\n" + CnfForms[j].TransformationPath());
                        r.WriteLine("\n" + CnfForms[j].TransformationPath());

                        Console.WriteLine();
                        r.WriteLine();
                    }
                }


                Console.WriteLine("\nПосле преобразований получены следующие клаузы: ");
                Console.WriteLine(ClausesStr);

                r.WriteLine("\nПосле преобразований получены следующие клаузы: ");
                r.WriteLine(ClausesStr);
                if (param.proof)
                {
                    r.WriteLine("Доказательство");
                    Console.WriteLine("\nДоказательство: ");
                    var str1 = new List<string>();
                    //Print(state, res, str1);

                    str1.Reverse();
                    str1 = str1.Distinct().ToList();
                    int k = 1;
                    foreach (string s in str1)
                    {
                        Console.WriteLine((Convert.ToString(k)).PadLeft(3) + ". " + s);
                        r.WriteLine((Convert.ToString(k)).PadLeft(3) + ". " + s);
                        k++;

                    }
                    Console.WriteLine("Найдена пустая клауза. Доказательство завершено.\n");
                    r.WriteLine("Найдена пустая клауза. Доказательство завершено.\n");

                }
                Console.WriteLine("\nСтатистика: ");
                Console.WriteLine(report.statistics.ToString());

                r.WriteLine("\nСтатистика: ");
                r.WriteLine(report.statistics.ToString());
            }






            //string json = JsonSerializer.Serialize(report,
            //    new JsonSerializerOptions()
            //    {
            //        WriteIndented = true,
            //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping //JavaScriptEncoder.Create(UnicodeRanges.All) 
            //    });



            //using (StreamWriter sw = new StreamWriter(answersDirectory + System.IO.Path.GetFileNameWithoutExtension(Path) + ".json"))
            //{
            //    sw.WriteLine(json);
            //    //sw.WriteLine("Read formula: ");
            //    //sw.WriteLine(formulastr);
            //    //sw.WriteLine("Result: " + verdict);
            //    //sw.WriteLine("TPTP  : " + TPTPStatus);

            //    //sw.WriteLine("\nStatistics: ");
            //    //sw.WriteLine("Elapsed time: " + stopwatch.Elapsed.TotalMilliseconds);
            //    //sw.WriteLine(state.StatisticsString());
            //    //sw.WriteLine("\nInitial Clauses: ");
            //    //i = 1;
            //    //foreach (var s in cnf.clauses)
            //    //    sw.WriteLine(i++ + ". " + s.ToString());

            //    //sw.WriteLine("\nProof: \n");
            //    //i = 1;
            //    //foreach (string s in str)
            //    //    sw.WriteLine(i++ + ". " + s);
            //}
        }


        static void Rating(string Path)
        {
            Clause.ResetCounter();
            var param = new SearchParams();
            //param.heuristics = Heuristics.Heuristics.PickGiven5;

            string timeoutStatus = string.Empty;
            string TPTPStatus;
            using (StreamReader sr = new StreamReader(Path))
            {
                string text = sr.ReadToEnd();
                var rg = new Regex("(Status).+(\n)");
                TPTPStatus = rg.Match(text).Value;
            }

            var problem = new FOFSpec();
            problem.Parse(Path);

            //string formulastr = problem.clauses[0].ToString();
            string formulastr = "FORMULA";// problem.formulas[0].Formula.ToString();

            ClauseSet cnf = problem.Clausify();



            var state = new RatingProofState(param, cnf);


            Stopwatch stopwatch = new Stopwatch();
            CancellationTokenSource token = new CancellationTokenSource();
            state.token = token;
            var tsk = new Task<Clause>(() => state.Saturate());
            stopwatch.Restart();
            tsk.Start();
            //var res = state.Saturate();
            bool complete = tsk.Wait(5000);
            stopwatch.Stop();
            token.Cancel();

            Clause res;
            if (complete)
            {
                res = tsk.Result;
            }
            else
            {
                res = null;
                timeoutStatus = " - timeout";
                Console.Write("Timeout" + Path);
            }

            if (res is null)
            {
                Console.WriteLine("FAIL", ConsoleColor.Red);
                var R = new Report(state, -1)
                {
                    Result = "NOT THEOREM",
                    HeuristicName = "Rating"
                };
                string jsn = JsonSerializer.Serialize(R,
              new JsonSerializerOptions()
              {
                  WriteIndented = true,
                  Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping //JavaScriptEncoder.Create(UnicodeRanges.All) 
              });
                using (StreamWriter sw = new StreamWriter(answersDirectory + System.IO.Path.GetFileNameWithoutExtension(Path) + ".json"))
                {

                    sw.WriteLine(jsn);
                }
                return;
            }
            if (res.IsEmpty)
            {
                Count++;
                Console.WriteLine("STATUS: THEOREM");
            }
            else
                Console.WriteLine("STATUS: NOT THEOREM");

            string verdict = res.IsEmpty ? "STATUS: THEOREM" : "STATUS: NOT THEOREM";
            var str = new List<string>();
            //Print(state, res, str);

            str.Reverse();
            str = str.Distinct().ToList();
            int i = 1;
            Console.WriteLine("\nPROOF: ");
            foreach (string s in str)
                Console.WriteLine(i++ + ". " + s);


            i = 1;
            StringBuilder initialCl = new StringBuilder();
            foreach (var s in cnf.clauses)
                initialCl.Append(i++ + ". " + s.ToString() + "\n");

            i = 1;
            StringBuilder proof = new StringBuilder();
            foreach (string s in str)
                proof.Append(i++ + ". " + s + "\n");

            var report = new Report(state, res.depth)
            {
                ProblemName = System.IO.Path.GetFileName(Path),
                Formula = formulastr,
                Result = verdict + timeoutStatus,
                TPTPResult = TPTPStatus,
                InitialClauses = initialCl.ToString(),
                Proof = proof.ToString(),
                HeuristicName = "Rating"

            };
            report.statistics.ElapsedTime = stopwatch.Elapsed.TotalMilliseconds;


            string json = JsonSerializer.Serialize(report,
                new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping //JavaScriptEncoder.Create(UnicodeRanges.All) 
                });



            using (StreamWriter sw = new StreamWriter(answersDirectory + System.IO.Path.GetFileNameWithoutExtension(Path) + ".json"))
            {
                sw.WriteLine(json);
            }
        }
        static void FOFFull1(string Path)
        {

            Console.WriteLine("===================================================");
            Console.WriteLine(Path);
            Clause.ResetCounter();
            string timeoutStatus = string.Empty;
            var param = new SearchParams()
            {
                // delete_tautologies = true,
                forward_subsumption = true,
                //backward_subsumption = true
            };
            param.heuristics = Heuristics.Heuristics.PickGiven5;
            string TPTPStatus;
            using (StreamReader sr = new StreamReader(Path))
            {
                string text = sr.ReadToEnd();
                var rg = new Regex("(Status).+(\n)");
                TPTPStatus = rg.Match(text).Value;
            }

            var problem = new FOFSpec();
            problem.Parse(Path);
            string formulastr = "FORMULA";// problem.formulas[0].Formula.ToString();
            // string formulastr = problem.clauses.ToString();

            var cnf = problem.Clausify();
            string ClausesStr = cnf.ToString();

            var state = new ProofState(param, cnf, false, false);

            Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Restart();
            //var res = state.Saturate();
            //stopwatch.Stop();
            CancellationTokenSource token = new CancellationTokenSource();
            state.token = token;
            var tsk = new Task<Clause>(() => state.Saturate());
            stopwatch.Restart();
            tsk.Start();
            // Thread.Sleep(500000);
            //var res = state.Saturate();
            bool complete = tsk.Wait(5000);
            stopwatch.Stop();
            token.Cancel();
            Clause res;
            if (complete)
            {
                res = tsk.Result;
                Count++;
                solved.Add(Path);
            }
            else
            {
                res = null;
                timeoutStatus = " - timeout";
                Console.WriteLine("Timeout: " + Path);
            }

            if (res is null)
            {
                Console.WriteLine("FAIL", ConsoleColor.Red);
                var R = new Report(state, -1)
                {
                    Result = "NOT THEOREM" + timeoutStatus,
                    HeuristicName = param.heuristics.ToString()
                };
                string jsn = JsonSerializer.Serialize(R,
              new JsonSerializerOptions()
              {
                  WriteIndented = true,
                  Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping //JavaScriptEncoder.Create(UnicodeRanges.All) 
              });
                using (StreamWriter sw = new StreamWriter(answersDirectory + System.IO.Path.GetFileNameWithoutExtension(Path) + ".json"))
                {

                    sw.WriteLine(jsn);
                }
                return;
            }


            if (res.IsEmpty)
                Console.WriteLine("STATUS: THEOREM");
            else
                Console.WriteLine("STATUS: NOT THEOREM");

            string verdict = res.IsEmpty ? "STATUS: THEOREM" : "STATUS: NOT THEOREM";
            var str = new List<string>();
            Console.WriteLine(Path);
            //Print(state, res, str);

            str.Reverse();
            str = str.Distinct().ToList();
            int i = 1;
            Console.WriteLine("\nPROOF: ");
            foreach (string s in str)
                Console.WriteLine(i++ + ". " + s);


            i = 1;
            StringBuilder initialCl = new StringBuilder();
            foreach (var s in cnf.clauses)
                initialCl.Append(i++ + ". " + s.ToString() + "\n");

            i = 1;
            StringBuilder proof = new StringBuilder();
            foreach (string s in str)
                proof.Append(i++ + ". " + s + "\n");

            var report = new Report(state, res.depth)
            {
                ProblemName = System.IO.Path.GetFileName(Path),
                Formula = formulastr,
                Result = verdict,
                TPTPResult = TPTPStatus,
                InitialClauses = initialCl.ToString(),
                Proof = proof.ToString(),
                HeuristicName = param.heuristics.ToString()
            };
            report.statistics.ElapsedTime = stopwatch.Elapsed.TotalMilliseconds;


            string json = JsonSerializer.Serialize(report,
                new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping //JavaScriptEncoder.Create(UnicodeRanges.All) 
                });



            using (StreamWriter sw = new StreamWriter(answersDirectory + System.IO.Path.GetFileNameWithoutExtension(Path) + ".json"))
            {
                //.WriteLine(json);
                //sw.WriteLine("Read formula: ");
                //sw.WriteLine(formulastr);
                //sw.WriteLine("Result: " + verdict);
                //sw.WriteLine("TPTP  : " + TPTPStatus);

                //sw.WriteLine("\nStatistics: ");
                //sw.WriteLine("Elapsed time: " + stopwatch.Elapsed.TotalMilliseconds);
                //sw.WriteLine(state.StatisticsString());
                //sw.WriteLine("\nInitial Clauses: ");
                //i = 1;
                //foreach (var s in cnf.clauses)
                //    sw.WriteLine(i++ + ". " + s.ToString());

                //sw.WriteLine("\nProof: \n");
                //i = 1;
                //foreach (string s in str)
                //    sw.WriteLine(i++ + ". " + s);
            }
        }
        static void FOF(string Path)
        {
            string TPTPStatus;
            using (StreamReader sr = new StreamReader(Path))
            {
                string text = sr.ReadToEnd();
                var rg = new Regex("(Status).+(\n)");
                TPTPStatus = rg.Match(text).Value;
            }

            var problem = new FOFSpec();
            problem.Parse(Path);

            Console.WriteLine("Read formula: ");
            string formulastr = problem.formulas[0].Formula.ToString();
            Console.WriteLine(problem.formulas[0].Formula.ToString());
            var cnf = problem.Clausify();


            var state = new SimpleProofState(cnf);
            Clause res = state.Saturate();

            if (res.IsEmpty)
                Console.WriteLine("STATUS: THEOREM");
            else
                Console.WriteLine("STATUS: NOT THEOREM");

            string verdict = res.IsEmpty ? "STATUS: THEOREM" : "STATUS: NOT THEOREM";
            var str = new List<string>();
            //Print(state, res, str);

            str.Reverse();
            str = str.Distinct().ToList();
            int i = 1;
            Console.WriteLine("\nPROOF: ");
            foreach (string s in str)
                Console.WriteLine(i++ + ". " + s);








            using (StreamWriter sw = new StreamWriter(answersDirectory + System.IO.Path.GetFileName(Path)))
            {
                sw.WriteLine("Read formula: ");
                sw.WriteLine(formulastr);
                sw.WriteLine("Result: " + verdict);
                sw.WriteLine("TPTP  : " + TPTPStatus);

                sw.WriteLine("Initial Clauses: ");
                i = 1;
                foreach (var s in cnf.clauses)
                    sw.WriteLine(i++ + ". " + s.ToString());

                sw.WriteLine("\nProof: \n");
                i = 1;
                foreach (string s in str)
                    sw.WriteLine(i++ + ". " + s);
            }

            //using (StreamWriter sw = new StreamWriter("clauses.txt"))
            //{
            //    foreach (var clause in cnf.clauses)
            //        sw.WriteLine(clause.ToString());
            //}
        }
        //static int i = 1;



        private static string CreateFormulaList(List<WFormula> list)
        {
            int i = 1;
            StringBuilder sb = new StringBuilder();
            foreach (WFormula formula in list)
            {
                sb.Append(i++ + ". ");
                sb.Append(formula.Formula.ToString());
                sb.Append("\n");
            }
            return sb.ToString();
        }

        private static string CreateEqList(List<Clause> list)
        {
            int i = 1;
            StringBuilder sb = new StringBuilder();
            foreach (var formula in list)
            {
                sb.Append(i++ + ". ");
                sb.Append(formula.ToString());
                sb.Append("\n");
            }
            return sb.ToString();
        }

        #endregion
    }
}