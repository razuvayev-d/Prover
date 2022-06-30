using Prover.DataStructures;
using Prover.Heuristics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Prover.ProofStates
{
    internal class RatingProofState
    {

        public CancellationTokenSource token;
        SearchParams Params;



        public int initial_clause_count;
        public int proc_clause_count = 0;
        public int factor_count = 0;
        public int resolvent_count = 0;
        public int tautologies_deleted = 0;
        public int forward_subsumed = 0;
        public int backward_subsumed = 0;
        bool silent;

        List<Literal> freqLiterals;
        HeuristicClauseSet unprocessed;
        List<Clause> ClauseArray;

        public List<Clause> allClauses = new List<Clause>();
        public RatingProofState(SearchParams Params, ClauseSet clauses)
        {
            freqLiterals = GetFrequentLiterals(clauses);

            this.Params = Params;
            this.Params.heuristics = new EvalStructure(new RatingEvaluation(freqLiterals), 1);
            unprocessed = new HeuristicClauseSet(Params.heuristics);

            foreach (Clause clause in clauses.clauses)
            {
                unprocessed.AddClause(clause);
                allClauses.Add(clause);
            }


            initial_clause_count = unprocessed.Count;

            unprocessed.clauses.Sort(delegate (Clause c1, Clause c2)
            {
                return c1.evaluation[0].CompareTo(c2.evaluation[0]);
            });

            ClauseArray = unprocessed.clauses;

        }

        public static List<Literal> GetFrequentLiterals(ClauseSet clauses)
        {
            List<Literal> c = new List<Literal>();
            List<Literal> c1 = new List<Literal>();
            List<int> freq = new List<int>();

            Dictionary<Literal, int> FreqLitsDict = new Dictionary<Literal, int>();

            foreach (Clause clause in clauses.clauses)//Создали коллекцию ВСЕХ атомов
                foreach (Literal literal in clause.Literals)
                {
                    c.Add(literal);
                }

            foreach (Literal literal in c) //Коллекция уникальных БЕЗ учета знака
            {
                if (FreqLitsDict.ContainsKey(literal))
                    FreqLitsDict[literal]++;
                else
                    FreqLitsDict.Add(literal, 1);
            }

            var sortedDict = (from entry in FreqLitsDict orderby entry.Value descending select entry).ToList();


            int j = 0;
            List<Literal> result = new List<Literal>();
            while (true)
            {
                result.Add(sortedDict[j].Key);
                if (j < sortedDict.Count - 1)
                {
                    if (sortedDict[j].Value == sortedDict[j + 1].Value)
                    {
                        j++;
                        continue;
                    }
                    else
                    {
                        return result;
                    }
                }
                else
                {
                    return result;
                }
            }
        }

        private List<Clause> GetOrderedClauses()
        {
            unprocessed.clauses.Sort(delegate (Clause c1, Clause c2)
            {
                return c1.evaluation[0].CompareTo(c2.evaluation[0]);
            });
            return unprocessed.clauses;
        }



        public Clause Saturate2()
        {
            int k = 0;

            var M = unprocessed;
            List<Clause> resolvents = new List<Clause>();
   
            List<Clause> Skplus = new List<Clause>();
            //List<List<Clause>> S = new List<List<Clause>>();

            ClauseArray = GetOrderedClauses(); //M_k
            int n = unprocessed.Count;
            int i = 0;
            int j = n - 1;
            while (true)
            {
                //S.Add(new List<Clause>());
                if (resolvents.Where(x => x.IsEmpty).Any())
                {
                    return resolvents.Where((x) => x.IsEmpty).ToList()[0];
                }
                ClauseArray = GetOrderedClauses(); //M_k
                n = unprocessed.Count;
                i = 0;
                j = n - 1;
                
            step32:
                if (ClauseArray.Count < 2) return null;
                Clause resolvent = ResolutionMethod.Resolution.Apply(ClauseArray[i], ClauseArray[j]);

                if (resolvent is null)
                {
                    if (j > i)
                    {
                        j--;
                        goto step32;
                    }
                    else
                    {
                        if (i < n - 1)
                        {
                            i++;
                            j = n - 1;
                            goto step32;
                        }
                        else
                        {
                            if (resolvents.Where(x => x.IsEmpty).Any())
                            {
                                return resolvents.Where((x) => x.IsEmpty).ToList()[0];
                            }
                            else
                            {
                          
                                if (unprocessed.Count > 0) {
                                    var a = ClauseArray[i];
                                    var b = ClauseArray[j];

                                    M.ExtractClause(a);
                                    M.ExtractClause(b);
                                    n -= 2;
                                    continue;
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }

                    }
                }
                else //resolvent is not null
                {
                    resolvent.evaluation = Params.heuristics.Evaluate(resolvent);
                    resolvents.Add(resolvent);

                    allClauses.Add(resolvent);

                    if (j > i)
                    {
                        j--;
                        goto step32;
                    }
                    if (i < n - 1)
                    {
                        i++;
                        j = n - 1;
                        if (j < 0) continue;
                        goto step32;
                    }

                    //step51
                    if (unprocessed.Count > 0)
                    {
                        var a = ClauseArray[i];
                        var b = ClauseArray[j];

                        M.ExtractClause(a);
                        M.ExtractClause(b);
                        n -= 2;

                    }
                    unprocessed.AddClause(resolvent);
                    resolvent_count++;
                    n++;
                    n = unprocessed.Count;

                    continue;
                }
            }

        }

        public Clause Saturate()
        {
            int k = 0;
            var M = unprocessed;
            var S = new List<List<Clause>>();
            int i, j;
            int n;
            List<Clause> Mk;
            S.Add(new List<Clause>());
            S.Add(new List<Clause>());
        step2:
            if (S[k].Where(x => x.IsEmpty).Any())
            {
                return S[k].Where((x) => x.IsEmpty).ToList()[0];
            }

        step3:
        //if disjuncts exist
        step31:
            n = M.Count;
            i = 0;
            j = n - 1;
            Mk = GetOrderedClauses();

        step32:
            if (token.IsCancellationRequested) return null;
            var resolvent = ResolutionMethod.Resolution.Apply(ClauseArray[i], ClauseArray[j]);
            if (resolvent is not null) goto step4;
            else goto step331;

            step331:
            if (j > i)
            {
                j--;
                goto step32;
            }
            else
            {
                if (i < n - 1)
                {
                    i++;
                    j = n - 1;
                    goto step32;
                }
                else goto step5;
            }
        step4:
            S[k + 1].Add(resolvent);
            M.AddClause(resolvent);
            allClauses.Add(resolvent);
            resolvent_count++;
        step5:
            if (S[k + 1].Where(x => x.IsEmpty).Any())
            {
                return S[k + 1].Where((x) => x.IsEmpty).ToList()[0];
            }
            else
            {
                k = k + 1;
                S.Add(new List<Clause>());
                //S[k + 1] = new List<Clause>();
                goto step51;
            }
        step51:
            if (M.Count > 1)
            {
                var tmp = Mk[j];
                M.ExtractClause(Mk[i]);
                M.ExtractClause(tmp);
                n -= 2;
                goto step2;
            }
            else
            {
                if (S[k + 1].Where(x => x.IsEmpty).Any())
                    return S[k + 1].Where(x => x.IsEmpty).ToList()[0];
                else
                    return null;
            }
        }
    }
}
