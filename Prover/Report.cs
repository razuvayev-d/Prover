using Prover.DataStructures;
using Prover.ProofStates;
using Prover.Tokenization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public Report()
        {
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
        public string FormulaStr { get; set; }
        public string ClausesStr { get; set; }
        public string AxiomStr { get; set; }
        public string HeuristicName { get; set; }
        public string Result { get; set; }
        public string TPTPResult { get; set; }
        public Statistics statistics { get; set; }
        public string InitialClauses { get; set; }
        public string Proof { get; set; }
        public FOFSpec problem;
        public Clause Res { get; set; }
        public ProofState State { get; set; }

        public SearchParams Params { get; set; }

        public bool Timeout { get; set; }
        public void ConsolePrint()
        {
            Console.WriteLine("\n\nЗадача " + ProblemName);
            Console.WriteLine();
            string verdict;
            bool success = false;
            if (Res is not null && Res.IsEmpty)
            {
                verdict = "Доказательство найдено.";
                success = true;
            }
            else
            {
                verdict = "Доказательство не найдено.";
                if (Timeout)
                    verdict += "\nВремя истекло";
            }


            Console.WriteLine(verdict);

            Console.WriteLine("Прочитанная формула: ");
            Console.WriteLine(FormulaStr);

            if(!Params.supress_eq_axioms)
            {
                Console.WriteLine("Добавлены следующие аксиомы равенства:");
                Console.WriteLine(AxiomStr);
            }

            if (Params.simplify)
            {
                var CnfForms = problem.clauses.Select(clause => clause.Parent1).Distinct().ToList();

                Console.WriteLine("Преобразования в клаузы: ");

                for (int j = 0; j < CnfForms.Count; j++)
                {
                    if (CnfForms[j] is null) continue;
                    Console.WriteLine("   Формула " + (j + 1));
                    Console.WriteLine("\n" + CnfForms[j].TransformationPath());
                    Console.WriteLine();
                }

            }

            Console.WriteLine("\nПосле преобразований получены следующие клаузы: ");
            Console.WriteLine(ClausesStr);


            if (Params.proof && success)
            {

                Console.WriteLine("\nДоказательство: ");
                var str1 = new List<string>();
                Print(State, Res, str1);

                str1.Reverse();
                str1 = str1.Distinct().ToList();
                int k = 1;
                foreach (string s in str1)
                {
                    Console.WriteLine((Convert.ToString(k)).PadLeft(3) + ". " + s);
                    k++;

                }
                Console.WriteLine("Найдена пустая клауза. Доказательство завершено.\n");

            }
            Console.WriteLine("\nСтатистика: ");
            Console.WriteLine(State.statistics.ToString());

        }        

        private static void Print(ProofState state, Clause res, List<string> sq)
        {
            //Console.WriteLine(i++ + ". " + res.Name + ": " + res.ToString() + " from: " + res.support[0] + ", " + res.support[1]);
            if (res.Parent1 is not null && res.Parent2 is not null)
            {
                string substStr = res.Sbst is null || res.Sbst.subst.Count == 0 ? "" : " использована подстановка " + res.Sbst.ToString();// +" по " + res.LiteralStr;
                sq.Add(((res.Name + ": ").PadRight(7) + res.ToString()).PadRight(50) + (" [" + (res.Parent1 as Clause).Name + ", " + (res.Parent2 as Clause).Name + "]").PadRight(15) + substStr);

                Print(state, res.Parent1 as Clause, sq);

                Print(state, res.Parent2 as Clause, sq);
            }
            else
            {
                sq.Add((res.Name + ": " + res.ToString()).PadRight(50) + " [input]");// " [" + res.Parent1 + "]");
            }

        }
    }
}
