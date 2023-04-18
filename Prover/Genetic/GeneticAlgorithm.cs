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

            for (int generation = 0; generation < Options.MaxNumberOfGeneration; generation++)
            {
                Console.WriteLine("Начато в {0}\n", DateTime.Now);
                //if (generation > Options.GenerationTimeOutThreshold) timeout = Options.HardTimeOut;

                Console.WriteLine("\n\nGeneration number: " + generation.ToString());

                List<Individual> clone = new List<Individual>(population.Size);
                for (int i = 0; i < population.individuals.Count; i++)
                    clone.Add(null);

                Parallel.For(0, Options.Size, poptions, i =>
                {
                    clone[i] =population.individuals[i].Clone();
                });
                //for (int i = 0; i < population.individuals.Count; i++)
                //    clone.Add(population.individuals[i].Clone());


                //    var bests = population.individuals.Where(x => x.Fitness == population.MinFitness).Take(2).ToList();

                //for (int i = 0; i < bests.Count; i++)
                //    bests[i] = bests[i].Clone();

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
                //for (int i = 0; i < Options.Size; i++)
                //{
                //    newIndividuals.Add(GeneticOperators.Crossover(population.individuals[random.Next(Options.Size)], population.individuals[random.Next(Options.Size)], Options.Favor));
                //}

                Parallel.For(0, Options.Size /*(int)Math.Min(population.Size * 1.5, newIndividuals.Count) /*newIndividuals.Count*/, poptions, i =>
                {
                    var ind = GeneticOperators.Crossover(population.individuals[random.Next(Options.Size)], population.individuals[random.Next(Options.Size)], Options.Favor);
                    lock ("ge")
                    {
                        newIndividuals.Add(ind);
                    }
                });

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


                //newIndividuals.Distinct();
                //List<int> fitness = new List<int>();

                //Secuental calculate fitness
                //for (int i = 0; i < newIndividuals.Count; i++)
                //{
                //    newIndividuals[i].Fitness = Fitness.Calculate(newIndividuals[i]);
                //}
                Parallel.For(0, Options.Size /*(int)Math.Min(population.Size * 1.5, newIndividuals.Count) /*newIndividuals.Count*/, poptions, i =>
                {
                    newIndividuals[i].Fitness = fitness.Calculate(newIndividuals[i], timeout, SearchParams);
                    //newIndividuals[i].InvalidFitness = false;
                });
                //newIndividuals.AddRange(population.individuals);

                newIndividuals.AddRange(clone);

                newPopulation.individuals = newIndividuals;

                population = GeneticOperators.Select(newPopulation, Options.Size, Options.elitism);

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
