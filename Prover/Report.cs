using Prover.ProofStates;
using System.Text;
using System.Text.Json.Serialization;

namespace Prover
{

    internal class Report
    {
        public Report(ProofState state, int depth = 0)
        {
            statistics = state.statistics;
            statistics.depth = depth;
            //statistics.initial_count = state.initial_clause_count;
            //statistics.proc_clause_cout = state.proc_clause_count;
            //statistics.resolvent_count = state.resolvent_count;
            //statistics.factor_count = state.factor_count;
            //statistics.forward_subsumed = state.forward_subsumed;
            //statistics.backward_subsumed = state.backward_subsumed;
            //statistics.tautologies_deleted = state.tautologies_deleted;
            //statistics.depth = depth;
        }
        public Report(RatingProofState state, int depth)
        {
            statistics = new Statistics();
            statistics.initial_count = state.initial_clause_count;
            statistics.proc_clause_count = state.proc_clause_count;
            statistics.resolvent_count = state.resolvent_count;
            statistics.factor_count = state.factor_count;
            statistics.forward_subsumed = state.forward_subsumed;
            statistics.backward_subsumed = state.backward_subsumed;
            statistics.tautologies_deleted = state.tautologies_deleted;
            statistics.depth = depth;
        }

        public class Statistics
        {
            [JsonPropertyName("Elapsed time")]
            public double ElapsedTime { get; set; } = 0;
            [JsonPropertyName("Initial clauses")]
            public int initial_count { get; set; } = 0;
            public int proc_clause_count { get; set; } = 0;
            public int factor_count { get; set; } = 0;
            public int resolvent_count { get; set; } = 0;
            public int tautologies_deleted { get; set; } = 0;
            public int forward_subsumed { get; set; } = 0;
            public int backward_subsumed { get; set; } = 0;
            public int depth { get; set; } = 0;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                const int offset = 25;
                sb.Append("Затрачено времени: ".PadRight(offset, ' ') + ElapsedTime);
                sb.Append("\nНачальных клауз: ".PadRight(offset, ' ') + initial_count);
                sb.Append("\nОбработано клауз: ".PadRight(offset, ' ') + proc_clause_count);
                sb.Append("\nФакторизовано: ".PadRight(offset, ' ') + factor_count);
                sb.Append("\nВычислено резольвент: ".PadRight(offset, ' ') + resolvent_count);
                sb.Append("\nПрямое поглощение: ".PadRight(offset, ' ') + forward_subsumed);
                sb.Append("\nОбратное поглощение: ".PadRight(offset, ' ') + backward_subsumed);
                sb.Append("\nУдалено тавтологий: ".PadRight(offset, ' ') + tautologies_deleted);
                sb.Append("\nГлубина вывода: ".PadRight(offset, ' ') + depth);

                return sb.ToString();
            }
        }


        public string ProblemName { get; set; }
        [JsonPropertyName("Read formula")]
        public string Formula { get; set; }

        public string HeuristicName { get; set; }
        public string Result { get; set; }
        public string TPTPResult { get; set; }
        public Statistics statistics { get; set; }
        public string InitialClauses { get; set; }
        public string Proof { get; set; }
    }
}
