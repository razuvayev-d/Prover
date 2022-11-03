using Prover.Heuristics;
using System;
using System.Collections.Generic;

namespace GeneticAlgotithm
{
	[Serializable]
	class Individual
	{
		[Serializable]
		public class FunctionWithParams
        {
			EvalStructure func;
			List<float> param;

			public FunctionWithParams(EvalStructure structure, List<float> parameters)
            {
				func = structure;
				param = parameters;
            }
		}

		List<FunctionWithParams> functions;
		List<int> weights;

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
	}
}