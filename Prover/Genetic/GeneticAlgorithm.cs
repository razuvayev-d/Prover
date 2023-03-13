using Prover.DataStructures;
using Prover.Heuristics;
using Prover.ProofStates;
using Prover.Tokenization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Prover.Genetic
{
    class Fitness
    {
        public static string GetRandomString()
        {
            var MemberCount = Enum.GetNames(typeof(HeuristicsFunctions)).Length;
            Random r = new Random();
            return ((HeuristicsFunctions)r.Next(MemberCount)).ToString();
        }



        static string TrainDirectory = @".\TrainTask";
        public static int Calculate(Individual individual, int timeout)
        {
            if (individual.InvalidFitness)
            {
                individual.InvalidFitness = false;
                return Calculate(individual.CreateEvalStructure(), timeout);
            }
            else
                return individual.Fitness;
        }


        private static int Calculate(EvalStructure individual, int timeout)
        {
            int result = 0;
            string[] files = Directory.GetFiles(TrainDirectory);

            var PartedData = Partitioner.Create(files, true);
            //Parallel.ForEach(PartedData, (file) =>
            //{
            //    if (TrySolve(file, individual)) Interlocked.Increment(ref result);
            //});
            foreach (var file in files)
            {
                if (TrySolve(file, individual, timeout)) result++;
            }

            return result;
        }

        static bool TrySolve(string Path, EvalStructure individual, int timeout)
        {
            Clause.ResetCounter();

            string timeoutStatus = string.Empty;
            var param = new SearchParams()
            {
                forward_subsumption = true,
                //backward_subsumption = true,
                //delete_tautologies = true
            };

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
            bool complete = tsk.Wait(timeout);

            token.Cancel();
            Clause res;
            if (complete)
            {
                res = tsk.Result;
                //lock ("Console")
                //{                 
                //    Console.ForegroundColor = ConsoleColor.Green; // устанавливаем цвет
                //    Console.WriteLine("Solved: " + Path);
                //    Console.ResetColor(); // сбрасываем в стандарт
                //}

            }
            else
            {

                res = null;
                timeoutStatus = " - timeout";
                //lock ("Console")
                //{
                //    Console.ForegroundColor = ConsoleColor.Red; // устанавливаем цвет
                //    Console.WriteLine("Timeout: " + Path);
                //    Console.ResetColor(); // сбрасываем в стандарт
                //}

            }

            if (res is null)
            {

                return false;
            }


            if (res.IsEmpty) return true;
            else return false;

        }
    }
    internal class GeneticOptions
    {

        public enum GeneticMode { CreateNewPopulation, LoadExistingPopulation }
        public int Size { get; set; } = 100;
        public int GenesLengts { get; set; } = 5;
        public GeneticMode Mode { get; set; } = GeneticMode.CreateNewPopulation;
        public string PopulationFileName { get; set; } = null;
        public double Favor { get; set; } = 0.7;

        public double probWeight { get; set; } = 0.5;
        public double probParam { get; set; } = 0.3;

        public double elitism { get; set; } = 0.2;
        public int MaxNumberOfGeneration { get; set; } = 100;

        public int HardTimeOut { get; set; } = 3000;
        public int LightTimeOut { get; set; } = 1000;
        public int GenerationTimeOutThreshold { get; set; } = 75;


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
            ParallelOptions poptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 16
            };

            Population population;
            if (Options.Mode == GeneticOptions.GeneticMode.CreateNewPopulation)
                population = Population.CreateRandom(Options.Size, Options.GenesLengts);
            else
            {
                population = Population.LoadFromFile(Options.PopulationFileName);
            }

            Parallel.For(0, Options.Size, poptions, i =>
            {
                population.individuals[i].Fitness = Fitness.Calculate(population.individuals[i], Options.LightTimeOut);
            });
            population.AverageFitness = population.individuals.Select(x => x.Fitness).Average();
            population.MinFitness = population.individuals.Select(x => x.Fitness).Min();
            population.MaxFitness = population.individuals.Select(x => x.Fitness).Max();

            Console.WriteLine("Average fitness of generation {0}: {1}", -1, population.AverageFitness);
            Console.WriteLine("Max fitness of generation {0}: {1}", -1, population.MaxFitness);
            Console.WriteLine("Min fitness of generation {0}: {1}", -1, population.MinFitness);


            population.SaveToFile("InitialPopulation.txt");


            int timeout = Options.LightTimeOut;
        
            for (int generation = 0; generation < Options.MaxNumberOfGeneration; generation++)
            {
                Console.WriteLine("Начато в {0}\n", DateTime.Now);
                //if (generation > Options.GenerationTimeOutThreshold) timeout = Options.HardTimeOut;

                Console.WriteLine("\n\nGeneration number: " + generation.ToString());


                var bests = population.individuals.Where(x => x.Fitness == population.MinFitness).Take(5).ToList();

                //Sequental mutation
                //for (int i = 0; i < Options.Size; i++)
                //{
                //    GeneticOperators.Mutation(population.individuals[i], Options.probWeight, Options.probParam);
                //}

                Parallel.For(0, Options.Size, poptions, i =>
                {
                    GeneticOperators.Mutation(population.individuals[i], Options.probWeight, Options.probParam);
                });

                Population newPopulation = new Population();
                List<Individual> newIndividuals = new List<Individual>();
                //for (int i = 0; i < Options.Size; i++)
                //    for (int j = 1; j < i + 1; j++)
                //    {
                //        newIndividuals.Add(GeneticOperators.Crossover(population.individuals[i], population.individuals[j], Options.Favor));
                //    }
                Random random = new Random();   
                for(int i =0; i < Options.Size; i++)
                {
                    newIndividuals.Add(GeneticOperators.Crossover(population.individuals[random.Next(Options.Size)], population.individuals[random.Next(Options.Size)], Options.Favor));
                }
                //newIndividuals.Distinct();
                List<int> fitness = new List<int>();

                //Secuental calculate fitness
                //for (int i = 0; i < newIndividuals.Count; i++)
                //{
                //    newIndividuals[i].Fitness = Fitness.Calculate(newIndividuals[i]);
                //}


                Parallel.For(0,Options.Size /*(int)Math.Min(population.Size * 1.5, newIndividuals.Count) /*newIndividuals.Count*/, poptions, i =>
                {
                    newIndividuals[i].Fitness = Fitness.Calculate(newIndividuals[i], timeout);
                });
                newIndividuals.AddRange(population.individuals);

                newIndividuals.AddRange(bests);

                newPopulation.individuals = newIndividuals;

                population = GeneticOperators.Select(newPopulation, Options.Size, Options.elitism);

                population.AverageFitness = newIndividuals.Select(x => x.Fitness).Average();
                population.MinFitness = newIndividuals.Select(x => x.Fitness).Min();
                population.MaxFitness = newIndividuals.Select(x => x.Fitness).Max();

                using (StreamWriter wr = new StreamWriter("averF.txt", append: true))
                {
                    wr.WriteLine(generation + " : " + population.AverageFitness);
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Average fitness of generation {0}: {1}", generation, population.AverageFitness);
                Console.WriteLine("Max fitness of generation {0}: {1}", generation, population.MaxFitness);
                Console.WriteLine("Min fitness of generation {0}: {1}", generation, population.MinFitness);
                Console.ResetColor();
                population.SaveToFile(@"generations\" + generation + ".txt");
            }
            population.SaveToFile("endPopul.txt");
        }
    }
}
