﻿using Prover.Heuristics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Porver.Genetic
{
	[Serializable]
	public class Individual
	{

		public List<List<object>> genes { get; set; } = new List<List<object>>(10);//Каждый элемент список вида [name, weight, par1, par2 ... ]

		public int Fitness { get; set; }

		public bool InvalidFitness = true;

		public Individual(List<List<object>> genes, int Fitness, bool InvalidF)
		{
			this.genes = genes;
			this.Fitness = Fitness;
			this.InvalidFitness = InvalidF;
		}

		public static Individual GreateRandom(int n = 5)
		{
			var genes = new List<List<object>>();
			for (int i = 0; i < n; i++)
				genes.Add(CreateWeightFunction());
			return new Individual(genes, 0, true);
		}
		private static List<object> CreateWeightFunction()
		{
			var MemberCount = Enum.GetNames(typeof(HeuristicsFunctions)).Length;
			Random r = new Random();
			string name = ((HeuristicsFunctions)r.Next(MemberCount)).ToString();
			int weight = r.Next(5, 10);
			int par1 = r.Next(0, 10);
			int par2 = r.Next(1000, 2000) % 10;
			return new List<object> { name, weight, par1, par2 };
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
		/// <summary>
		/// Получает гены которые НЕ имеют индексы indecies
		/// </summary>
		/// <param name="indecies"></param>
		/// <returns></returns>
		public List<List<object>> GetNonmatches(List<int> indecies)
        {
			var ret = new List<List<object>>();
			for(int i=0; i< this.genes.Count; i++)
            {
				if(!indecies.Contains(i)) ret.Add(genes[i]);
            }
			return ret;
        }

        public Individual()
        {
        }
		public EvalStructure CreateEvalStructure()
        {
			var heuristics = new List<(ClauseEvaluationFunction, int)>();
			for(int i = 0; i < genes.Count; i++)
            {
				heuristics.Add(CreateEvalFunct(genes[i]));
            }
			return new EvalStructure(heuristics);
        }
		//public enum HeuristicsFunctions
		//{
		//	FIFO,
		//	NegatePrio,
		//	SymbolCount,
		//	LIFO,
		//	ConstPrio
		//}
		private (ClauseEvaluationFunction, int) CreateEvalFunct(List<object> param)
        {
			switch ((string)param[0])
            {
				case "FIFO":
					return (new FIFOEvaluation(),(int) param[1]);
				case "LIFO":
					return (new LIFOEvaluation(), (int)param[1]);
				case "NegatePrio":
					return (new NegatePrio((int)param[2]), (int)param[1]);
				case "ConstPrio":
					return (new ConstPrio((int)param[2]), (int)param[1]);
				case "SymbolCount":
					return /*(new FIFOEvaluation(), (int)param[1]); //*/(new SymbolCountEvaluation((int)param[2], (int)param[3]), (int)param[1]);
				default: throw new ArgumentException(param[0].ToString());
			}
		}

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
			for (int i = 0; i < genes.Count; i++)
			{
				str.Append("[");
				str.Append(genes[i][1].ToString());
				str.Append(",");
				str.Append(genes[i][0].ToString());
				str.Append("]");
				str.Append(" ");
			}
			return str.ToString();
        }
    }
}