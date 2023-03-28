using Prover.ClauseSets;
using Prover.DataStructures;
using Prover.Heuristics;
using Prover.ResolutionMethod;
using System;
using System.Threading;
using static Prover.Report;

namespace Prover.ProofStates
{
    internal class SearchParams
    {
        public EvalStructure heuristics { get; set; } = Prover.Heuristics.Heuristics.PickGiven5;
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

        public SearchParams()
        {
        }

        public SearchParams(EvalStructure heuristics,
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

        //public override string ToString()
        //{

        //}
    }



    internal class ProofState
    {
        SearchParams Params;
        public HeuristicClauseSet unprocessed;
        public ClauseSet processed;
        public CancellationTokenSource token;

        public Statistics statistics = new Statistics();
        bool indexed = false;

        public LiteralSelector Selector;

        //public int initial_clause_count;
        //public int proc_clause_count = 0;
        //public int factor_count = 0;
        //public int resolvent_count = 0;
        //public int tautologies_deleted = 0;
        //public int forward_subsumed = 0;
        //public int backward_subsumed = 0;
        bool silent;
        public ProofState(SearchParams Params, ClauseSet clauses, bool silent = false, bool indexed = false)
        {
            //indexed = true;
            this.Params = Params;
            unprocessed = new HeuristicClauseSet(Params.heuristics);
            this.indexed = indexed;
            if (indexed)
                processed = new IndexedClauseSet();
            else
                processed = new ClauseSet();
            foreach (Clause clause in clauses.clauses)
                unprocessed.AddClause(clause);

            statistics.initial_count = unprocessed.Count;
            this.silent = silent;

            if(Params.literal_selection is not null)
            {
                Selector = LiteralSelection.GetSelector(Params.literal_selection);
            }
        }
        static int iter = 0;
        public Clause ProcessClause()
        {
            
            var given_clause = unprocessed.ExtractBest();
            //Console.WriteLine("\nGIVEN========================== {0}", ++iter);
            //Console.WriteLine(given_clause.ToString());
            //Console.WriteLine(given_clause.evaluation[0].ToString() + " " + given_clause.evaluation[1]);
            //Console.WriteLine("PROCESSED: {0}   UNPROC: {1}", processed.Count, unprocessed.Count);
            given_clause = given_clause.FreshVarCopy();

            if (given_clause.IsEmpty) return given_clause;

            if (Params.delete_tautologies && given_clause.IsTautology)
            {
                statistics.tautologies_deleted++;
                return null;
            }

            if (Params.forward_subsumption && Subsumption.Forward(processed, given_clause))
            {
                statistics.forward_subsumed++;
                //Console.WriteLine("Forward");
                return null;
            }

            if (Params.backward_subsumption)
            {
                var tmp = Subsumption.Backward(given_clause, processed);
                statistics.backward_subsumed += tmp;// Subsumption.Backward(given_clause, processed);

                //Console.WriteLine("BACKWARD " + tmp);
            }

            if (Params.literal_selection is not null)
            {
                given_clause.SelectInferenceLiterals(Selector);
            }

            ClauseSet newClauses = new ClauseSet();
            ClauseSet factors = ResControl.ComputeAllFactors(given_clause);
            statistics.factor_count += factors.Count;

            newClauses.AddRange(factors);
            ClauseSet resolvents;

            if (indexed)
                resolvents = ResControl.ComputeAllResolventsIndexed(given_clause, processed as IndexedClauseSet);
            else
                resolvents = ResControl.ComputeAllResolvents(given_clause, processed);

            //Console.WriteLine("FACTORS LEN {0}, RESOLV LEN {1}", factors.Count, resolvents.Count);
            statistics.resolvent_count += resolvents.Count;
            statistics.proc_clause_count++;
            //resolvents.Distinct();
            newClauses.AddRange(resolvents);

            processed.AddClause(given_clause);

            for (int i = 0; i < newClauses.Count; i++)
            {
                Clause c = newClauses[i];
                if (c.IsEmpty) return c;
                unprocessed.AddClause(c);
            }
            return null;
        
        
        
        }


        public Clause Saturate()
        {
            while (unprocessed.Count > 0)
            {
                if (token.IsCancellationRequested) return null;
                //unprocessed.clauses = unprocessed.clauses.Distinct().ToList();
                //unprocessed.Distinct();
                //processed.Distinct();
                Clause res = ProcessClause();
                if (res is not null)
                {
                    return res;
                }
            }
            return null;
        }


        //        public string StatisticsString()
        //        {
        //            return string.Format(
        //                @"Initial clauses    : {0} 
        //Processed clauses  : {1}
        //Factors computed   : {2}
        //Resolvents computed: {3}
        //Tautologies deleted: {4}
        //Forward subsumed   : {5}
        //Backward subsumed  : {6}",
        //                                     initial_clause_count,
        //                                     proc_clause_count,
        //                                     factor_count,
        //                                     resolvent_count,
        //                                     tautologies_deleted,
        //                                     forward_subsumed,
        //                                     backward_subsumed);
        //        }
    }
}
