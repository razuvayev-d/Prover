using Prover.Heuristics;
using System;
using System.Collections.Generic;

namespace Porver.GeneticAlgotithm
{
	[Serializable]
	class Individual
	{
		public List<List<object>> genes; //Каждый элемент список вида [name, weight, par1, par2 ... ]

		public int Fitness { get; private set; }

		public bool InvalidFitness = true;

		public Individual(List<List<object>> genes, int Fitness, bool InvalidF)
        {
			this.genes = genes;
			this.Fitness = Fitness;
			this.InvalidFitness = InvalidF;
        }

		[Serializable]
		public class FunctionWithParams
        {
			public EvalStructure func { get; set; }
			public List<float> param { get; set; }

			public FunctionWithParams(EvalStructure structure, List<float> parameters)
            {
				func = structure;
				param = parameters;
            }
		}

		public List<FunctionWithParams> functions { get; set; }
		public List<int> weights { get; set; }

		public Individual(List<FunctionWithParams> funcs, List<int> weights)
        {
			if((funcs.Count != weights.Count) || (funcs.Count == 0))
					throw new ArgumentException("Количество функций должно соответствовать количеству весов. Количество не может быть нулевым");
			functions = funcs;
			this.weights = weights;
		}

		public Individual(List<FunctionWithParams> funcs)
        {
			if (funcs.Count == 0)
				throw new ArgumentException("Количество не может быть нулевым");

			var weights = new List<int>(funcs.Count);
	        for (int i = 0; i < funcs.Count; i++)
				weights.Add(1);

			this.functions = funcs;
			this.weights = weights;	
        }

        public Individual()
        {
        }

        public static Individual CreateNewIndividual()
        {
			return null;
        }
	}
}