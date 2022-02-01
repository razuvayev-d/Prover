using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{
    class SimpleProofState
    {
        ClauseSet unprocessed;
        ClauseSet processed;

        public SimpleProofState(ClauseSet clauses)
        {
            unprocessed.AddRange(clauses);
            processed = new ClauseSet();
        }

        public Clause processClause()
        {
            throw new NotImplementedException();
            //Clause given_clause = unprocessed.ExtractFirst();
            //given_clause = given_clause.FreshVarCopy();
            ////System.out.println("#" + given_clause.toStringJustify());
            //if (given_clause.IsEmpty)    // We have found an explicit contradiction
            //    return given_clause;

            //ClauseSet newClauses = new ClauseSet();
            //ClauseSet factors = ResControl.ComputeAllFactors(given_clause);
            ////System.out.println("INFO in SimpleProofState.processClause(): factors: " + factors);

            //newClauses.AddAll(factors);
            //ClauseSet resolvents = ResControl.ComputeAllResolvents(given_clause, processed);
            ////System.out.println("INFO in SimpleProofState.processClause(): resolvents: " + resolvents);

            //newClauses.addAll(resolvents);

            //processed.add(given_clause);
            ////System.out.println("INFO in SimpleProofState.processClause(): processed clauses: " + processed);

            //for (int i = 0; i < newClauses.length(); i++)
            //{
            //    Clause c = newClauses.get(i);
            //    unprocessed.add(c);
            //}
            //return null;
        }
    }
}
