﻿using Prover.ProofStates;
using System.Text.Json.Serialization;
using System.Text;
using System.Runtime.Intrinsics.X86;

namespace Prover
{

    internal class Report
    {
        public Report(ProofState state, int depth = 0)
        {
            statistics = new Statistics();
            statistics.initial_count = state.initial_clause_count;
            statistics.proc_clause_cout = state.proc_clause_count;
            statistics.resolvent_count = state.resolvent_count;
            statistics.factor_count = state.factor_count;
            statistics.forward_subsumed = state.forward_subsumed;
            statistics.backward_subsumed = state.backward_subsumed;
            statistics.tautologies_deleted = state.tautologies_deleted;
            statistics.depth = depth;
        }
        public Report(RatingProofState state, int depth)
        {
            statistics = new Statistics();
            statistics.initial_count = state.initial_clause_count;
            statistics.proc_clause_cout = state.proc_clause_count;
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
            public double ElapsedTime { get; set; }
            [JsonPropertyName("Initial clauses")]
            public int initial_count { get; set; }
            public int proc_clause_cout { get; set; }
            public int factor_count { get; set; }
            public int resolvent_count { get; set; }
            public int tautologies_deleted { get; set; }
            public int forward_subsumed { get; set; }
            public int backward_subsumed { get; set; }
            public int depth { get; set; }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                const int offset = 25;
                sb.Append("Затрачено времени: ".PadRight(offset, ' ') + ElapsedTime);
                sb.Append("\nНачальных клауз: ".PadRight(offset, ' ') + initial_count);
                sb.Append("\nФакторизовано: ".PadRight(offset, ' ') + factor_count);
                sb.Append("\nВычислено резольвент: ".PadRight(offset, ' ') + resolvent_count);
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
