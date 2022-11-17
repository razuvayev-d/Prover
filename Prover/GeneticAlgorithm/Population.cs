using Porver.GeneticAlgotithm;
using Prover.Heuristics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover.GeneticAlgorithm
{
    [Serializable]
    internal class Population
    {
        public int size { get; set; }
        public List<Individual> individuals { get; set; }

        public List<int> fitness; 
        public Population(int size, List<Individual> inds)
        {
            this.size = size;
            this.individuals = inds;
            this.fitness = new List<int>();
            for(int i = 0; i < inds.Count; i++) 
                fitness.Add(0);
        }

        public static Population GreateNewPopulation(int size)
        {
            List<Individual> individuals = new List<Individual>(size);
            Random random = new Random();
            for (int i = 0; i < size; i++)
            {
                
                //TODO: Сделать динамическое обновление для классов-наследников ClauseEvaluationFunctions через GetAssembly().GetTypes().Where(type => type.IsSubclassOf(...)
                int r = random.Next() %2;
                switch (r)
                {
                    //case 0:
                    //    var individual = new EvalStructure(new FIFOEvaluation(), 1);
                    //    individuals.Add(individual);
                    //    break;
                    //case 1:
                    //    //SymbolCountEvaluation ParamsCount
                    //    individual = new EvalStructure(new SymbolCountEvaluation(random.Next() % 5, random.Next() % 5), 1);
                    //    individuals.Add((EvalStructure)individual);
                    //    break;
                    //default:
                    //    break;
                }
                
            }
            return new Population(size, individuals);
        }
    }
}
