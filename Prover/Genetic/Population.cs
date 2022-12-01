using Porver.Genetic;
using Prover.Heuristics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;


namespace Prover.Genetic
{
    [Serializable]
    public class Population
    {
        public int Size
        {
            get
            {
                return individuals.Count;
            }
        }
        public List<Individual> individuals { get; set; }

        public double AverageFitness { get; set; }
        public double MaxFitness { get; set; }
        public double MinFitness { get; set; }

        public Population(List<Individual> inds)
        {
            this.individuals = inds;
        }

        public Population()
        {
            individuals = new List<Individual>();
        }

        public static Population CreateRandom(int n = 100, int genesLength = 5)
        {
            var ret = new Population();
            for(int i = 0; i < n;i++)
            {
                ret.individuals.Add(Individual.GreateRandom(genesLength));
            }
            return ret;
        }


        public void SaveToFile(string name)
        {
            string jsn = JsonSerializer.Serialize(this,
              new JsonSerializerOptions()
              {
                  WriteIndented = true,
                  Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping //JavaScriptEncoder.Create(UnicodeRanges.All) 
              });
            using StreamWriter writer = new StreamWriter(name);
            writer.Write(jsn);

        }

        public static Population LoadFromFile(string name)
        {
            using StreamReader reader = new StreamReader(name);
            string jsn = reader.ReadToEnd();

            var popul = JsonSerializer.Deserialize<Population>(jsn);
            return popul;
        }    
    }
}
