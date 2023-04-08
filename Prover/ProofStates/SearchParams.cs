using Prover.Heuristics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover.ProofStates
{
    internal class SearchParams
    {
        public EvaluationScheme heuristics { get; set; } = Prover.Heuristics.Heuristics.PickGiven5;
        public bool delete_tautologies { get; set; } = false;
        public bool forward_subsumption { get; set; } = false;
        public bool backward_subsumption { get; set; } = false;

        public bool index { get; set; } = false;
        public string literal_selection { get; set; } = null;

        public bool proof { get; set; } = false;

        public bool simplify { get; set; } = false;

        public int timeout { get; set; } = 0;

        public bool supress_eq_axioms { get; set; } = false;

        public string file { get; set; }

        public int degree_of_parallelism { get; set; } = 1;
        public SearchParams()
        {
        }

        public SearchParams(EvaluationScheme heuristics,
                            bool delete_tautologies = false,
                            bool forward_subsumption = false,
                            bool backward_subsumption = false,
                            string literal_selection = null)
        {
            this.heuristics = heuristics;
            this.delete_tautologies = delete_tautologies;
            this.forward_subsumption = forward_subsumption;
            this.backward_subsumption = backward_subsumption;
            this.literal_selection = literal_selection;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Параметры поиска: ");
            sb.Append("\nИспользована эвристика: ");
            sb.Append(heuristics.Name);
            if (delete_tautologies)
            {
                sb.Append("\nУдаление тавтологий");
            }
            if (forward_subsumption)
            {
                sb.Append("\nПрямое поглощение");
            }
            if (backward_subsumption)
            {
                sb.Append("\nОбратное поглощение");
            }
            if (literal_selection != null)
            {
                sb.Append("\nВыбор литералов: " + literal_selection);
            }
            if (timeout > 0)
            {
                sb.AppendLine("\nОграничение времени: " + timeout);
            }
            else sb.AppendLine("\nБез ограничения времени");

            if (degree_of_parallelism > 1)
                sb.Append("\nСтепень параллелизма: " + degree_of_parallelism);

            return sb.ToString();
        }
    }
}
