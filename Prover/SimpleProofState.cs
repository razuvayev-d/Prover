using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{
    class SimpleProofState
    {
        ClauseSet unprocessed = new ClauseSet();
        ClauseSet processed = new ClauseSet();

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
            //throw new NotImplementedException();
            Clause given_clause = unprocessed.ExtractFirst();
            given_clause = given_clause.FreshVarCopy();
            //System.out.println("#" + given_clause.toStringJustify());
            if (given_clause.IsEmpty)    // We have found an explicit contradiction
                return given_clause;

            ClauseSet newClauses = new ClauseSet();
            //TODO: Разобраться с факторами
            // ClauseSet factors = ResControl.ComputeAllFactors(given_clause);
            //System.out.println("INFO in SimpleProofState.processClause(): factors: " + factors);

            //newClauses.AddAll(factors);
            ClauseSet resolvents = ResControl.ComputeAllResolvents(given_clause, processed);
            //System.out.println("INFO in SimpleProofState.processClause(): resolvents: " + resolvents);
           
            newClauses.AddRange(resolvents);

            processed.AddClause(given_clause);
            //System.out.println("INFO in SimpleProofState.processClause(): processed clauses: " + processed);

            for (int i = 0; i < newClauses.Count; i++)
            {
                Clause c = newClauses[i];
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
