using Prover.DataStructures;
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

namespace Prover
{
    class Program
    {
        static string proplamsDirectory = @"./problems/";
        static string answersDirectory = @"./answers/";
        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(proplamsDirectory);
            var s = proplamsDirectory + "SYN941+1.p";
            //FOF(s);
            foreach (string file in files)
                FOFFull(file);
        }
        static void FOFFull(string Path)
        {
            Clause.ResetCounter();
            var param = new SearchParams();
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

            string formulastr = problem.formulas[0].Formula.ToString();

            var cnf = problem.Clausify();

            var state = new ProofState(param, cnf, false, false);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();
            var res = state.Saturate();
            stopwatch.Stop();

            if (res.IsEmpty)
                Console.WriteLine("STATUS: THEOREM");
            else
                Console.WriteLine("STATUS: NOT THEOREM");

            string verdict = res.IsEmpty ? "STATUS: THEOREM" : "STATUS: NOT THEOREM";
            var str = new List<string>();
            Print(state, res, str);

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

            var report = new Report(state)
            {
                ProblemName = System.IO.Path.GetFileName(Path),
                Formula = formulastr,
                Result = verdict,
                TPTPResult = TPTPStatus,
                InitialClauses = initialCl.ToString(),
                Proof = proof.ToString()
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
            Print(state, res, str);

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
                if (res.support.Count == 2)
                {
                    sq.Add(res.Name + ": " + res.ToString() + " from: " + res.support[0] + ", " + res.support[1]);
                    string Name1, Name2;
                    Name1 = res.support[0];
                    Name2 = res.support[1];
                    var q = state.processed.clauses.Where(x => x.Name == Name1).ToList()[0];
                    Print(state, q, sq);
                    q = state.processed.clauses.Where(x => x.Name == Name2).ToList()[0];
                    Print(state, q, sq);
                }
                if (res.support.Count == 1)
                {
                    sq.Add(res.Name + ": " + res.ToString() + " from: " + res.support[0]);
                    string Name1, Name2;
                    Name1 = res.support[0];
                    var q = state.processed.clauses.Where(x => x.Name == Name1).ToList()[0];
                    Print(state, q, sq);

                }

                //var q = state.processed.clauses.Where(x => x.Name == Name1).ToList()[0];
                //Print(state, q, sq);
                //q = state.processed.clauses.Where(x => x.Name == Name2).ToList()[0];
                //Print(state, q, sq);

            }
        }


        static void Print(ProofState state, Clause res, List<string> sq)
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
                if (res.support.Count == 2)
                {
                    sq.Add(res.Name + ": " + res.ToString() + " from: " + res.support[0] + ", " + res.support[1]);
                    string Name1, Name2;
                    Name1 = res.support[0];
                    Name2 = res.support[1];
                    var q = state.processed.clauses.Where(x => x.Name == Name1).ToList()[0];
                    Print(state, q, sq);
                    q = state.processed.clauses.Where(x => x.Name == Name2).ToList()[0];
                    Print(state, q, sq);
                }
                if (res.support.Count == 1)
                {
                    sq.Add(res.Name + ": " + res.ToString() + " from: " + res.support[0]);
                    string Name1, Name2;
                    Name1 = res.support[0];
                    var q = state.processed.clauses.Where(x => x.Name == Name1).ToList()[0];
                    Print(state, q, sq);

                }

                //var q = state.processed.clauses.Where(x => x.Name == Name1).ToList()[0];
                //Print(state, q, sq);
                //q = state.processed.clauses.Where(x => x.Name == Name2).ToList()[0];
                //Print(state, q, sq);

            }
        }

        static void Write(string path, SimpleProofState state, Clause res, List<string> sq)
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
                Write(path, state, q, sq);
                q = state.processed.clauses.Where(x => x.Name == Name2).ToList()[0];
                Write(path, state, q, sq);

            }
        }

        static void CreateReport(string Path, ProofState state, Clause res)
        {

        }
    }
}
