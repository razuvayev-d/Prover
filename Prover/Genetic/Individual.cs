using Prover.DataStructures;
using Prover.SearchControl;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prover.Genetic
{
    [Serializable]
    public class Individual
    {
        public List<List<object>> genes { get; set; } = new List<List<object>>(10);//Каждый элемент список вида [name, weight, par1, par2 ... ]
        public int Fitness { get; set; }
        public bool InvalidFitness { get; set; } = true;


        public Individual Clone()
        {
            Individual clone = new Individual();
            clone.Fitness = Fitness;
            clone.InvalidFitness = InvalidFitness;

            var cgenes = new List<List<object>>(10);

            foreach (var gene in this.genes)
            {
                var a = new List<object>();
                cgenes.Add(a);
                for (int i = 0; i < gene.Count; i++)//9
                {
                    if (i == 0 || i == 2 || i == 8)
                    {
                        a.Add(gene[i].ToString());
                    }
                    else a.Add(Convert.ToInt32(gene[i].ToString()));
                }
            }
            clone.genes = cgenes;
            return clone;
        }
        public Individual() { }

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
            string name = ((HeuristicsFunctions)r.Next(MemberCount)).ToString(); //0
            int weight = r.Next(1, 6); //1
            string prio = PriorityFunctions.GetRandomFunctionName();//2
            int par1 = r.Next(0, 10); //3
            int par2 = r.Next(1000, 2000) % 10; //4
            int par3 = r.Next(0, 10); //5
            int par4 = r.Next(0, 10); //6
            int par5 = r.Next(0, 10); //7
            string litselection = LiteralSelection.GetRandomLitSelectionString(); //8

            return new List<object> { name, weight, prio, par1, par2, par3, par4, par5, litselection };
        }
       
        /// <summary>
        /// Получает гены которые НЕ имеют индексы indecies
        /// </summary>
        public List<List<object>> GetNonmatches(List<int> indecies)
        {
            var ret = new List<List<object>>();
            for (int i = 0; i < this.genes.Count; i++)
            {
                if (!indecies.Contains(i)) ret.Add(genes[i]);
            }
            return ret;
        }

      
        public EvaluationScheme CreateEvalStructure()
        {
            var heuristics = new List<(ClauseEvaluationFunction, int, LiteralSelector)>();
            if (genes.Count == 0) throw new Exception("DDDD");
            for (int i = 0; i < genes.Count; i++)
            {
                heuristics.Add(CreateEvalFunct(genes[i]));
            }
            return new EvaluationScheme(heuristics);
        }
        //public enum HeuristicsFunctions
        //{
        //	FIFO,
        //	NegatePrio,
        //	SymbolCount,
        //	LIFO,
        //	ConstPrio
        //}

        //public enum HeuristicsFunctions
        //{
        //    FIFOPrio,
        //    ClauseWeight,
        //    SymbolCountPrio,
        //    LIFOPrio,
        //    ByLiteralNumber,
        //    ByDerivationDepth,
        //    ByDerivationSize
        //}
        private (ClauseEvaluationFunction, int, LiteralSelector) CreateEvalFunct(List<object> param)
        {
            //switch ((string)param[0])
            //{
            //    case "FIFO":
            //        return (new FIFOEvaluation(), (int)param[1]);
            //    case "LIFO":
            //        return (new LIFOEvaluation(), (int)param[1]);
            //    case "NegatePrio":
            //        return (new NegatePrio((int)param[2]), (int)param[1]);
            //    case "ConstPrio":
            //        return (new ConstPrio((int)param[2]), (int)param[1]);
            //    case "SymbolCount":
            //        return /*(new FIFOEvaluation(), (int)param[1]); //*/(new SymbolCountEvaluation((int)param[2], (int)param[3]), (int)param[1]);
            //    default: throw new ArgumentException(param[0].ToString());
            //}

            Func<object, int> ToInt = (obj) => Convert.ToInt32(obj.ToString());
        
            Predicate<Clause> priof = PriorityFunctions.PriorityFunctionSwitch(param[2].ToString());
            LiteralSelector litsel = LiteralSelection.GetSelector(param[8].ToString());
            switch (param[0].ToString())
            {
                case "FIFOPrio":
                    return (new FIFOEvaluationPrio(priof), ToInt(param[1]), litsel);
                case "LIFOPrio":
                    return (new LIFOEvaluationPrio(priof), ToInt(param[1]), litsel);
                case "ClauseWeight":
                    return (new ClauseWeight(priof, ToInt(param[3]), ToInt(param[4]), ToInt(param[5])), ToInt(param[1]), litsel);
                case "ByDerivationDepth":
                    return (new ByDerivationDepth(priof), Convert.ToInt32(param[1].ToString()), litsel);
                case "ByLiteralNumber":
                    return (new ByLiteralNumber(priof), ToInt(param[1]), litsel);
                case "ByDerivationSize":
                    return (new ByDerivationSize(priof), ToInt(param[1]), litsel);
                case "RefinedWeight":
                    return (new RefinedWeight(priof, ToInt(param[3]), ToInt(param[4]), ToInt(param[5]), ToInt(param[6]), ToInt(param[7])), 
                        ToInt(param[1]), 
                        litsel);
                default: throw new ArgumentException("Нет такой функции " + param[0].ToString());
            }

        }

      
        //    FIFOPrio,
        //    ClauseWeight,
        //    LIFOPrio,
        //    ByLiteralNumber,
        //    ByDerivationDepth,
        //    ByDerivationSize

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