using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{
    /// <summary>
    /// Класс, представляющий клаузулу. В настоящее время клаузула состоит из
    /// следующие элементы:
    /// - Cписок литералов.
    /// - Тип ("простой", если не задан).
    /// - Имя(генерируется автоматически, если не задано)
    /// </summary>
    class Clause : Derivable
    {
        List<Literal> literals;
        string type;
        string name;

        public string Type => type;

        public static Clause ParseClause(Lexer lexer)
        {
            lexer.AcceptLit("cnf");
            lexer.AcceptTok(TokenType.OpenPar);
            var name = lexer.LookLit();
            lexer.AcceptTok(TokenType.IdentLower);
            lexer.AcceptTok(TokenType.Comma);
            var type = lexer.LookLit();
            List<Literal> lits;

            if (!(type == "axiom" || type == "negated_conjecture"))
                type = "plain";
            lexer.AcceptTok(TokenType.IdentLower);
            lexer.AcceptTok(TokenType.Comma);
            if (lexer.TestTok(TokenType.OpenPar))
            {
                lexer.AcceptTok(TokenType.OpenPar);
                lits = Literal.ParseLiteralList(lexer);
                lexer.AcceptTok(TokenType.ClosePar);
            }
            else
            {
                lits = Literal.ParseLiteralList(lexer);
            }

            lexer.AcceptTok(TokenType.ClosePar);
            lexer.AcceptTok(TokenType.FullStop);

            var res = new Clause(lits, type, name);
            res.Derivation = new Derivation("input");
            return res;
        }

        public Clause(List<Literal> literals, string type = "plain", string name = null) : base(name)
        {
            this.literals = literals;
            this.type = type;
            this.name = name;
        }

        public Clause(List<Formula> literals, string type = "plain", string name = null) : base(name)
        {
            var n = literals.Count;
            List<Literal> lits = new List<Literal>(n);
            for (int i = 0; i < n; i++) 
            {
                // lits[i] = (Literal)literals[i];
                lits.Add((Literal)literals[i]);
            }

            this.literals = lits;
            this.type = type;
            this.name = name;
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.Append("{ ");
            foreach (var lit in literals)
                res.Append(lit.ToString() + ", ");
            res.Remove(res.Length - 2, 2);
            res.Append(" }");
            return res.ToString();
        }
    }
}
