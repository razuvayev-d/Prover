using Prover.ClauseSets;
using Prover.DataStructures;
using Prover.Heuristics;
using Prover.ResolutionMethod;
using System;
using System.Text;
using System.Threading;
using System.Linq;
using static Prover.Report;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prover.ProofStates
{
    



    internal class ProofState
    {
        SearchParams Params;
        public HeuristicClauseSet unprocessed;
        public ClauseSet processed;
        public CancellationTokenSource token;

        public Statistics statistics = new Statistics();
        bool indexed = false;

        public LiteralSelector Selector;

        private Barrier barrier; 

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

            LiteralSelection.ClausesInProblems = processed;
        }
        static int iter = 0;
        public Clause ProcessClause()
        {          
            var given_clause = unprocessed.ExtractBest();
            //Console.WriteLine("\nGIVEN========================== {0}", ++iter);
            //Console.WriteLine(given_clause.ToString());
            //Console.WriteLine(given_clause.evaluation[0].ToString() + " " + given_clause.evaluation[1]);
            //Console.WriteLine("PROCESSED: {0}   UNPROC: {1}", processed.Count, unprocessed.Count);

            //using (StreamWriter sw = new StreamWriter("given_clauses000.txt", true))
            //{
            //    sw.WriteLine("1;" + given_clause.ToString() + ";" + ++iter + ";" + processed.Count + ";" + unprocessed.Count + ";" + statistics.factor_count + ";" + statistics.backward_subsumed + ";" + statistics.forward_subsumed);
            //}

            //using (StreamWriter sw = new StreamWriter("given_clauses001.txt", true))
            //{
            //    sw.WriteLine((++iter).ToString() + " " + given_clause.ToString());
            //    sw.WriteLine("\t" + given_clause.evaluation[0] + "\t" + given_clause.evaluation[1]);
            //}
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
            //using (StreamWriter sw = new StreamWriter("given_clauses001.txt", true))
            //{
            //    sw.WriteLine("    RESOLVENTS");
            //}
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
                    var x = unprocessed.clauses.MaxBy(x => x.depth);
                    var y = processed.clauses.MaxBy(x => x.depth);
                    statistics.search_depth = Math.Max(x.depth, y.depth);
                    return res;
                }
            }
            var a = unprocessed.clauses.MaxBy(x => x.depth);
            var b = processed.clauses.MaxBy(x => x.depth);
            statistics.search_depth = Math.Max(a.depth, b.depth);
            return null;
        }

        //public Clause Saturate()
        //{
        //    if (Params.degree_of_parallelism > 1)
        //        return ParallelSaturate();
        //    return SequentalSaturate();
        //}

        private Clause ParallelSaturate()
        {
            List<Task<Clause>> Tasks = new List<Task<Clause>>(Params.degree_of_parallelism);
            for (int i = 0; i < Params.degree_of_parallelism; i++)          
                Tasks.Add(null);
            

                while (unprocessed.Count > 0)
            {
                
                if (token.IsCancellationRequested) return null;
                barrier = new Barrier(Params.degree_of_parallelism);
                for (int i = 0; i < Params.degree_of_parallelism; i++)
                { 
                    Tasks[i] = new Task<Clause>(ProcessClauseThread);
                    Tasks[i].Start();
                }
                Task.WaitAll(Tasks.ToArray());


                for (int i = 0; i < Params.degree_of_parallelism; i++)
                {

                    //unprocessed.clauses = unprocessed.clauses.Distinct().ToList();

                    //unprocessed.Distinct();
                    //processed.Distinct();
                    Clause res = Tasks[i].Result;
                    if (res is not null)
                    {
                        return res;
                    }
                }
            }
            return null;
        }

        public Clause ProcessClauseThread()
        {
            Clause given_clause = null;
            lock ("Extract")
            {
                given_clause = unprocessed.ExtractBest();
            }
            if (given_clause is null)
            {
                barrier.RemoveParticipants(1);
                return null;
            }
                //Console.WriteLine("\nGIVEN========================== {0}", ++iter);
                //Console.WriteLine(given_clause.ToString());
                //Console.WriteLine(given_clause.evaluation[0].ToString() + " " + given_clause.evaluation[1]);
                //Console.WriteLine("PROCESSED: {0}   UNPROC: {1}", processed.Count, unprocessed.Count);

                //using (StreamWriter sw = new StreamWriter("given_clauses000.txt", true))
                //{
                //    sw.WriteLine("1;" +given_clause.ToString() +";" + ++iter + ";" + processed.Count + ";" + unprocessed.Count + ";" + statistics.factor_count + ";" + statistics.backward_subsumed + ";" + statistics.forward_subsumed);
                //}
                given_clause = given_clause.FreshVarCopy();

            if (given_clause.IsEmpty)
            {
                barrier.RemoveParticipant();
                return given_clause;
            }

            if (Params.delete_tautologies && given_clause.IsTautology)
            {
                statistics.tautologies_deleted++;
                barrier.RemoveParticipant();
                return null;
            }

            if (Params.forward_subsumption && Subsumption.Forward(processed, given_clause))
            {
                statistics.forward_subsumed++;
                //Console.WriteLine("Forward");
                barrier.RemoveParticipant();
                return null;
            }

            if (Params.backward_subsumption)
            {
                lock ("backward")
                {
                    var tmp = Subsumption.Backward(given_clause, processed);
                    statistics.backward_subsumed += tmp;
                }// Subsumption.Backward(given_clause, processed);

                //Console.WriteLine("BACKWARD " + tmp);
            }
            barrier.SignalAndWait();
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

            barrier.SignalAndWait();
            lock ("resolwentsAdd")
            {
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
