using Prover.DataStructures;
using Prover.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover.ClauseSets
{
    public class IndexedClauseSet : ClauseSet
    {
        ResolutionIndex ResIndex = new ResolutionIndex();
        SubsumptionIndex SubIndex = new SubsumptionIndex();

        public IndexedClauseSet() { }   
        public IndexedClauseSet(List<Clause> clauses):base(clauses) { }

        public override void AddClause(Clause clause)
        {
            ResIndex.InsertClause(clause);
            SubIndex.InsertClause(clause);
            clauses.Add(clause);
        }

        public override Clause ExtractClause(Clause clause) 
        {
            ResIndex.RemoveClause(clause);
            SubIndex.RemoveClause(clause);
            return base.ExtractClause(clause);
        }

        public List<ResolutionIndex.Candidate> GetResolutionLiterals(Literal lit)
        {
            return ResIndex.GetResolutionLiterals(lit);
        }

        public List<Clause> GetSubsumingCandidates(Clause queryClause)
        {
            return SubIndex.GetSubsumingCandidates(queryClause);
        }

        public List<Clause> GetSubsumedCandidates(Clause queryClause)
        {
            return SubIndex.GetSubsumedCandidates(queryClause);
        }

        //public new int Parse(Lexer lexer)
        //{
        //    int count = 0;
        //    while (lexer.LookLit() == "cnf")
        //    {
        //        var clause = Clause.ParseClause(lexer);
        //        AddClause(clause);
        //        count++;
        //    }
        //    return count;
        //}

    }
}
