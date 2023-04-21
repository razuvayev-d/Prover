using Prover.ClauseSets;
using Prover.DataStructures;
using Prover.ProofStates;
using Prover.SearchControl;
using Prover.Tokenization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Prover.Genetic
{
    class Fitness
    {

        List<ClauseSet> clauseSets = new List<ClauseSet>();
        public Fitness(string path)
        {
            string[] files = Directory.GetFiles(path);
            foreach (var p in files)
            {
                var problem = new FOFSpec();

                problem.Parse(p);

                var cnf = problem.Clausify();
                clauseSets.Add(cnf);
            }             
        }


        static string TrainDirectory = @".\TrainTask";

        public int Calculate(Individual individual, int timeout, SearchParams param)
        {
            //if (individual.InvalidFitness)
            //{
            //    individual.InvalidFitness = false;
                return Calculate(individual.CreateEvalStructure(), timeout, param);
            //}
            //else
            //{
            //    Console.WriteLine("Fitness ignore");
            //    return individual.Fitness;
            //}
        }


        private int Calculate(EvaluationScheme individual, int timeout, SearchParams param)
        {
            int result = 0;

            param.heuristics = individual;

            foreach (var cnf in clauseSets)
            {
                var state = new ProofState("", param, cnf.Copy(), false);
                CancellationTokenSource token = new CancellationTokenSource();
                state.token = token;
                var tsk = new Task<Clause>(() => state.Saturate());

                tsk.Start();
                bool complete = tsk.Wait(timeout);
                token.Cancel();
                tsk.Wait(15);
                Clause res;
                if (complete)
                {
                    res = tsk.Result;
                    if (res is not null && res.IsEmpty) result++;
                }


            }
            return result;
        }
            //var PartedData = Partitioner.Create(files, true);
            //Parallel.ForEach(PartedData, (file) =>
            //{
            //    if (TrySolve(file, individual)) Interlocked.Increment(ref result);
            //});
        //    foreach (var file in files)
        //    {
        //        if (TrySolve(file, individual, timeout, param))
        //        {
        //            result++;
        //            //Console.WriteLine(file);
        //             CancellationTokenSource token = new CancellationTokenSource();
        //    state.token = token;
        //    var tsk = new Task<Clause>(() => state.Saturate());

        //    tsk.Start();
        //    bool complete = tsk.Wait(timeout);
        //    token.Cancel();
        //    tsk.Wait(15);
        //    Clause res;
        //    if (complete)
        //        res = tsk.Result;
        //    else
        //        res = null;

        //    if (res is null)
        //    {
        //        return false;
        //    }
        //    if (res.IsEmpty) return true;
        //    else return false;
        //        }

        //    }

        //    return result;
        //}

        static bool TrySolve(string Path, EvaluationScheme individual, int timeout, SearchParams param)
        {




            Clause.ResetCounter();

            string timeoutStatus = string.Empty;

            param.heuristics = individual;

            var problem = new FOFSpec();
            problem.Parse(Path);

            var cnf = problem.Clausify();

            var state = new ProofState(Path, param, cnf, false);

            CancellationTokenSource token = new CancellationTokenSource();
            state.token = token;
            var tsk = new Task<Clause>(() => state.Saturate());

            tsk.Start();
            bool complete = tsk.Wait(timeout);
            token.Cancel();
            tsk.Wait(15);
            Clause res;
            if (complete)
                res = tsk.Result;
            else
                res = null;

            if (res is null)
            {
                return false;
            }
            if (res.IsEmpty) return true;
            else return false;

        }
    }

}
