using Prover.ClauseSets;
using Prover.DataStructures;
using Prover.ResolutionMethod;
using System.Linq;

namespace Prover.ProofStates
{
    class SimpleProofState
    {
        ClauseSet unprocessed = new ClauseSet();
        // TODO: public proof state
        public ClauseSet processed = new ClauseSet();



        public SimpleProofState(ClauseSet clauses)
        {
            unprocessed.AddRange(clauses);
            processed = new ClauseSet();
        }
        /// <summary>
        /// Берет одну клаузу из необработанных клауз и обрабатывает ее. Если найдена пустая клауза, она возвращается,
        /// в противном случае null. 
        /// </summary>
        /// <returns></returns>
        public Clause ProcessClause()
        {
            Clause given_clause = unprocessed.ExtractFirst();
            given_clause = given_clause.FreshVarCopy();

            if (given_clause.IsEmpty)
                return given_clause;

            ClauseSet newClauses = new ClauseSet();

            ClauseSet factors = ResControl.ComputeAllFactors(given_clause);

            newClauses.AddRange(factors);
            ClauseSet resolvents = ResControl.ComputeAllResolvents(given_clause, processed);

            resolvents.clauses.Distinct();
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

        /// <summary>
        /// Главная процедура доказательства
        /// </summary>
        /// <returns></returns>
        public Clause Saturate()
        {
            while (unprocessed.Count > 0)
            {
                //unprocessed.clauses = unprocessed.clauses.Distinct().ToList();
                //unprocessed.Distinct();
                Clause res = ProcessClause();
                if (res is not null)
                {
                    return res;
                }
            }
            return null;
        }
    }
}
