//using Porver.Genetic;
using Prover.SearchControl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Prover.Genetic
{
    public static class GeneticOperators
    {
        public static void Mutation(Individual individual, double probWeightMutates, double probParamMutates)
        {
            Random random = new Random();

            for (int i = 0; i < individual.genes.Count; i++)
            {
                if (random.NextDouble() < probWeightMutates) //мутация весовой функции
                {
                    int n = individual.genes[i].Count;
                    for (int j = 0; j < n; j++)
                    {
                        var param = individual.genes[i][j];
                        if (random.NextDouble() < probParamMutates)
                        {
                            if (param is string)
                            {
                                individual.InvalidFitness = true;
                                if (j == 2)
                                    individual.genes[i][j] = PriorityFunctions.GetRandomFunctionName();
                                else // j == 0
                                    individual.genes[i][j] = Extensions.GetRandomEvaluationFunction();                               

                            }
                            else if (param is int)
                            {
                                if (individual.genes[i][0].ToString() == "ClauseWeight" || individual.genes[i][0].ToString() == "RefinedWeight")
                                {
                                    individual.InvalidFitness = true;
                                    individual.genes[i][j] = random.NextDouble() > 0.5 ? (int)param + 2 : (int)param - 2;
                                }
                            }
                        }
                    }
                }
            }
        }

        //public static int RandomNormalDistribution(int mean, int std)
        //{
        //    Random rand = new Random(); //reuse this if you are generating many
        //    double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
        //    double u2 = 1.0 - rand.NextDouble();
        //    double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
        //                 Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        //    double randNormal =
        //                 mean + std * randStdNormal; //random normal(mean,stdDev^2)
        //    return Convert.ToInt32(randNormal);
        //}

        public static Individual Crossover(Individual individual1, Individual individual2, double favor)
        {
            Individual newInd = new();
            var matches = new List<(List<object>, List<object>)>();
            var matchI = new List<int>();
            var matchJ = new List<int>();

            for (int i = 0; i < individual1.genes.Count; i++)
                for (int j = 0; j < individual2.genes.Count; j++)
                {
                    if (individual1.genes[i][0] == individual2.genes[j][0]) //Сравниваем имена весовых функций
                    {
                        matches.Add((individual1.genes[i], individual2.genes[j]));
                        //individual1.genes.RemoveAt(i);
                        //individual2.genes.RemoveAt(j);
                        matchI.Add(i);
                        matchJ.Add(j);
                        break;
                    }
                }

            var nonmatches = individual1.GetNonmatches(matchI);
            nonmatches.AddRange(individual2.GetNonmatches(matchJ));
            //var nonmatches = individual1.genes;
            //nonmatches.AddRange(individual2.genes);

            //Для всех совпадений перемешиваем гены
            foreach (var match in matches)
                newInd.genes.Add(cxParams(match, favor));
            Random r = new Random();
            while (newInd.genes.Count < individual1.genes.Count)
            {
                var k = r.Next(nonmatches.Count);
                newInd.genes.Add(nonmatches[k]);
                nonmatches.RemoveAt(k);
            }

            return newInd;
        }

        private static List<object> cxParams((List<object>, List<object>) weight_founcts, double favor)
        {
            var newfunction = new List<object>();
            Random r = new Random();
            for (int i = 0; i < weight_founcts.Item1.Count; i++)
                if (r.NextDouble() < favor)
                    newfunction.Add(weight_founcts.Item1[i]);
                else
                    newfunction.Add(weight_founcts.Item2[i]);
            return newfunction;
        }
        static Random rand = new Random();
        public static Population Select(Population population, int n = 20, double elitism = 0.2)
        {
            //int elites = (int)Math.Round((double)n * elitism);
            //var pop = population.individuals.OrderBy(x => -x.Fitness).Take(elites).ToList();
            //var randoms = population.individuals.OrderBy(x => rand.Next()).Take(n - elites).ToList();
            ////var invalids = population.individuals.MinBy(x => x.Fitness);
            //pop.AddRange(randoms);
            //return new Population(pop);

            int selectset = (int)Math.Round((double)n * elitism);
            List<Individual> pop = new List<Individual>();
            while (pop.Count < n)
            {
                var set = SelectRandomExt(population.individuals, selectset);
                pop.Add(set.MaxBy(x => x.Fitness));
            }
            return new Population(pop);
        }

        private static List<Individual> SelectRandomExt(List<Individual> list, int n)
        {
            int end = list.Count;
            var res = new List<Individual>();
            while (res.Count < n)
                res.Add(list[rand.Next(0, end)]);
            return res;
        }
    }
}
