using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover.Genetic
{
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
}
