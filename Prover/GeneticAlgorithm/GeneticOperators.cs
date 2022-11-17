using Porver.GeneticAlgotithm;
using Prover.Heuristics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover.GeneticAlgorithm
{


    internal static class GeneticOperators
    {
        public static void Mutation(Individual individual, double probWeightMutates, double probParamMutates)
        {
            Random random = new Random();

            for (int i = 0; i < individual.genes.Count; i++)
            {
                if (random.NextDouble() < probWeightMutates)
                {
                    int n = individual.genes[i].Count;
                    for (int j = 0; j < n; j++)
                    {
                        var param = individual.genes[i][j];
                        if (random.NextDouble() < probParamMutates)
                        {
                            if (param is string)
                                param = Fitness.GetRandomString();
                            else if (param is double)
                            {
                                param = random.NextDouble(); //TODO: можно изменить в зависимости от значения param
                            }
                            else if (param is int)
                            {
                                param = random.Next((int)param - 2, (int)param + 2);
                            }
                        }
                    }
                }
            }
        }

        public static Individual Crossover(Individual individual1, Individual individual2, double favor)
        {
            Individual newInd = new();
            var matches = new List<(List<object>, List<object>)>();
            for(int i = 0; i< individual1.genes.Count; i++)
                for(int j = 0; j < individual2.genes.Count; j++)
                    if(individual1.genes[i] == individual2.genes[j]) //TODO: сравнение коллекций
                    {
                        matches.Add((individual1.genes[i], individual2.genes[j]));
                        individual1.genes.RemoveAt(i);
                        individual2.genes.RemoveAt(j);
                        break;
                    }
            var nonmatches = individual1.genes;
            nonmatches.AddRange(individual2.genes);

            //Для всех совпадений перемешиваем гены
            foreach(var match in matches)
                newInd.genes.Add(cxParams(match, favor));

            return newInd;
            //if((string)individual1.individualCode[] 
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
    }
}
