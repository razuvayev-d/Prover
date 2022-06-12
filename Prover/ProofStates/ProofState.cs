using Prover.DataStructures;
using Prover.Heuristics;
using Prover.RosolutionRule;
using System;

namespace Prover.ProofStates
{
    internal class SearchParams
    {
        public EvalStructure heuristics;
        public bool delete_tautologies;
        public bool forward_subsumption;
        public bool backward_subsumption;
        public object literal_selection;

        public SearchParams()
        {
        }

        public SearchParams(EvalStructure heuristics,
                            bool delete_tautologies = false,
                            bool forward_subsumption = false,
                            bool backward_subsumption = false,
                            object literal_selection = null)
        {
            this.heuristics = heuristics;
            this.delete_tautologies = delete_tautologies;
            this.forward_subsumption = forward_subsumption;
            this.backward_subsumption = backward_subsumption;
            this.literal_selection = literal_selection;
        }
    }



    internal class ProofState
    {
        SearchParams Params;
        public HeuristicClauseSet unprocessed;
        public ClauseSet processed;


        public int initial_clause_count;
        public int proc_clause_count = 0;
        public int factor_count = 0;
        public int resolvent_count = 0;
        public int tautologies_deleted = 0;
        public int forward_subsumed = 0;
        public int backward_subsumed = 0;
        bool silent;
        public ProofState(SearchParams Params, ClauseSet clauses, bool silent = false, bool indexed = false)
        {
            this.Params = Params;
            unprocessed = new HeuristicClauseSet(Params.heuristics);
            if (indexed)
                throw new NotImplementedException();
            else
                processed = new ClauseSet();
            foreach (Clause clause in clauses.clauses)
                unprocessed.AddClause(clause);

            initial_clause_count = unprocessed.Count;
            this.silent = silent;
        }

        public Clause ProcessClause()
        {
            var given_clause = unprocessed.ExtractBest();
            given_clause = given_clause.FreshVarCopy(); //TODO: сделать уже копирование...

            if (given_clause.IsEmpty) return given_clause;

            if (Params.delete_tautologies && given_clause.IsTautology)
            {
                tautologies_deleted++;
                return null;
            }

            if (Params.forward_subsumption && Subsumption.Forward(processed, given_clause))
            {
                forward_subsumed++;
                return null;
            }

            if (Params.backward_subsumption)
            {
                backward_subsumed += Subsumption.Backward(given_clause, processed);
            }

            if (Params.literal_selection != null)
            {
                // given_clause.SelectInferenceLits(Params.literal_selection);
            }

            ClauseSet newClauses = new ClauseSet();
            ClauseSet factors = ResControl.ComputeAllFactors(given_clause);


            newClauses.AddRange(factors);
            ClauseSet resolvents = ResControl.ComputeAllResolvents(given_clause, processed);

            factor_count += factors.Count;
            resolvent_count += resolvents.Count;

            resolvents.Distinct();
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
                //unprocessed.clauses = unprocessed.clauses.Distinct().ToList();
                unprocessed.Distinct();
                Clause res = ProcessClause();
                if (res is not null)
                {
                    return res;
                }
            }
            return null;
        }


        public string StatisticsString()
        {
            return string.Format(
                @"Initial clauses    : {0} 
Processed clauses  : {1}
Factors computed   : {2}
Resolvents computed: {3}
Tautologies deleted: {4}
Forward subsumed   : {5}
Backward subsumed  : {6}",
                                     initial_clause_count,
                                     proc_clause_count,
                                     factor_count,
                                     resolvent_count,
                                     tautologies_deleted,
                                     forward_subsumed,
                                     backward_subsumed);
        }
    }
}
