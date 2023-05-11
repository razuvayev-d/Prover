using Prover.DataStructures;
using Prover.SearchControl;
using Prover.ProofStates;
using Prover.Tokenization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Prover.Genetic
{
   
    internal class GeneticAlgorithm
    {
        GeneticOptions Options;
        SearchParams SearchParams;

        public GeneticAlgorithm(GeneticOptions options, SearchParams searchParams)
        {
            Options = options;
            SearchParams = searchParams;
        }

        public void Evolution()
        {
            ParallelOptions poptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 16
            };
            var fitness = new Fitness(@".\TrainTask");
            Population population;
            if (Options.Mode == GeneticOptions.GeneticMode.CreateNewPopulation)
            {
                population = Population.CreateRandom(Options.Size, Options.GenesLengts);
                Parallel.For(0, Options.Size, poptions, i =>
                {
                    population.individuals[i].Fitness = fitness.Calculate(population.individuals[i], Options.LightTimeOut, SearchParams);
                });
            }
            else
            {
                population = Population.LoadFromFile(Options.PopulationFileName);
                foreach (var g in population.individuals)
                    g.InvalidFitness = true;// true; // false;

                Parallel.For(0, Options.Size, poptions, i =>
                {
                    population.individuals[i].Fitness = fitness.Calculate(population.individuals[i], Options.LightTimeOut, SearchParams);
                });
            }
        
            population.SaveToFile("InitialPopulation.txt");
            Console.WriteLine("Average fitness of generation {0}: {1}", -1, population.AverageFitness);
            Console.WriteLine("Max fitness of generation {0}: {1}", -1, population.MaxFitness);
            Console.WriteLine("Min fitness of generation {0}: {1}", -1, population.MinFitness);

            int timeout = Options.LightTimeOut;

            for (int generation = 1; generation < Options.MaxNumberOfGeneration; generation++)
            {
                Console.WriteLine("Начато в {0}\n", DateTime.Now);
                //if (generation > Options.GenerationTimeOutThreshold) timeout = Options.HardTimeOut;

                Console.WriteLine("\n\nGeneration number: " + generation.ToString());

                List<Individual> CloneOldPopulation = new List<Individual>(population.Size);
                for (int i = 0; i < population.individuals.Count; i++)
                    CloneOldPopulation.Add(null);

                Parallel.For(0, Options.Size, poptions, i =>
                {
                    CloneOldPopulation[i] = population.individuals[i].Clone();
                });
             
                Parallel.For(0, Options.Size, poptions, i =>
                {
                    GeneticOperators.Mutation(population.individuals[i], Options.probWeight, Options.probParam);
                });

                var newIndividuals = new List<Individual>();

                Random random = new Random();
                Parallel.For(0, Options.Size, poptions, i =>
                {
                    var ind = GeneticOperators.Crossover(population.individuals[random.Next(Options.Size)], population.individuals[random.Next(Options.Size)], Options.Favor);
                    lock ("ge")  //faster than ConcurrentBag
                    {
                        newIndividuals.Add(ind);
                    }
                });
   
                Parallel.For(0, Options.Size, poptions, i =>
                {
                    newIndividuals[i].Fitness = fitness.Calculate(newIndividuals[i], timeout, SearchParams);
                    //newIndividuals[i].InvalidFitness = false;
                });

                newIndividuals.AddRange(CloneOldPopulation);
                Population newPopulation = new Population(newIndividuals);

                population = GeneticOperators.Select(newPopulation, Options.Size, Options.elitism);

                //using (StreamWriter wr = new StreamWriter("averF.txt", append: true))
                //{
                //    wr.WriteLine(generation + " : " + population.AverageFitness);
                //}
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

    //Parallel.For(0, Options.Size, poptions,
    //    () => new List<Individual>(), //локальный накопитель
    //    (i, loop, localStorage) =>
    //    {
    //        var ind = GeneticOperators.Crossover(population.individuals[random.Next(Options.Size)], population.individuals[random.Next(Options.Size)], Options.Favor);
    //        localStorage.Add(ind);
    //        return localStorage;
    //    },
    //    (localStorage) =>
    //    {
    //        lock ("kl")
    //            newIndividuals.AddRange(localStorage);
    //    });


}
