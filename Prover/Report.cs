using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Prover.ProofStates;

namespace Prover
{

    internal class Report
    {
        public Report(ProofState state)
        {
            statistics = new Statistics();
            statistics.initial_count = state.initial_clause_count;
            statistics.proc_clause_cout = state.proc_clause_count;
            statistics.resolvent_count = state.resolvent_count;
            statistics.factor_count = state.factor_count;
            statistics.forward_subsumed = state.forward_subsumed;
            statistics.backward_subsumed = state.backward_subsumed;
            statistics.tautologies_deleted = state.tautologies_deleted;
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
        }

        
        public string ProblemName { get; set; }
        [JsonPropertyName("Read formula")]
        public string Formula { get; set; }
        public string Result { get; set; }
        public string TPTPResult { get; set; }
        public Statistics statistics { get; set; }
        public string InitialClauses { get; set; }
        public string Proof { get; set; }
    }
}
