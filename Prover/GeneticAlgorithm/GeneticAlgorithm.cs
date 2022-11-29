using Porver.GeneticAlgotithm;
using Prover.DataStructures;
using Prover.Heuristics;
using Prover.ProofStates;
using Prover.Tokenization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Prover.GeneticAlgorithm
{

    class Fitness
    {
        public static string GetRandomString()
        {
            return "FIFOEval";
        }

        List<string> weightsDictionary = new List<string>()
        {
            "FIFOEval",
            "SymbolCountEvaluation"
        };

        static (ClauseEvaluationFunction, int) CreateWeightFunction(string name, int weight, List<double> param)
        {
            switch (name)
            {
                case "FIFOEval":
                    return (new FIFOEvaluation(), weight);
                case "SymbolCountEvaluation":
                    return (new SymbolCountEvaluation((int)param[0], (int)param[1]), weight);
                default: throw new ArgumentException("Нет функции с именем " + name);
            }
        }



        static string TrainDirectory = @".\TrainTask";
        static int Calculate(Individual individual)
        {
            var heuristic = new List<(ClauseEvaluationFunction, int)>();
            List<double> parameters = new List<double>();
            for (int i = 0; i < individual.genes.Count; i++)
            {
                string name = (string)individual.genes[i][0];
                int weight = (int)individual.genes[i][1];
                for (int j = 2; j < individual.genes[i].Count; j++)
                    parameters.Add((double)individual.genes[i][j]);
                heuristic.Add(CreateWeightFunction(name, weight, parameters));
            }

            var eval = new EvalStructure(heuristic);

            return Calculate(eval);
        }




        private static int Calculate(EvalStructure individual)
        {
            int result = 0;
            string[] files = Directory.GetFiles(TrainDirectory);

            foreach (string file in files)
                if (TrySolve(file, individual)) result++;
            return result;
        }

        static bool TrySolve(string Path, EvalStructure individual)
        {
            Clause.ResetCounter();

            string timeoutStatus = string.Empty;
            var param = new SearchParams();
            param.heuristics = individual;
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

            var state = new ProofState(param, cnf, false, false);


            //stopwatch.Restart();
            //var res = state.Saturate();
            //stopwatch.Stop();
            CancellationTokenSource token = new CancellationTokenSource();
            state.token = token;
            var tsk = new Task<Clause>(() => state.Saturate());

            tsk.Start();
            //var res = state.Saturate();
            bool complete = tsk.Wait(5000);

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
                Console.WriteLine("Timeout: " + Path);
            }

            if (res is null)
            {

                return false;
            }


            if (res.IsEmpty) return true;
            else return false;

        }
        internal class GeneticOptions
        {
            public enum GeneticMode { CreateNewPopulation, LoadExistingPopulation }
            public int Size { get; set; } = 100;
            public GeneticMode Mode { get; set; } = GeneticMode.CreateNewPopulation;

            public double Favor { get; set; } = 0.1;

            public double probWeight { get; set; } = 0.5;
            public double probParam { get; set; } = 0.3;

            public int MaxNumberOfGeneration { get; set; } = 10;

        }
        internal class GeneticAlgorithm
        {
            GeneticOptions Options;

            public GeneticAlgorithm(GeneticOptions options)
            {
                Options = options;
            }

            public void Evolution()
            {
                Population population;
                if (Options.Mode == GeneticOptions.GeneticMode.CreateNewPopulation)
                    population = Population.GreateNewPopulation(Options.Size);
                else
                {
                    population = null;
                    //TODO: population = LoadFromFile
                }

                for (int i = 0; i < Options.Size; i++)
                {
                    population.fitness[i] = Fitness.Calculate(population.individuals[i]);

                    GeneticOperators.Mutation(population.individuals[i], Options.probWeight, Options.probParam);
                }
                Population newPopulation = new Population();
                List<Individual> newIndividuals = new List<Individual>();
                for(int i = 0; i < Options.Size; i++)
                    for(int j = 1; j < i + 1; j++)
                    {
                        newIndividuals.Add(GeneticOperators.Crossover(population.individuals[i], population.individuals[j], Options.Favor));
                    }

            }
        }
    }
}