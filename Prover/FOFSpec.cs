using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Prover
{
    class FOFSpec
    {
        List<Clause> clauses = new List<Clause>();
        // TODO: debug сделать private
        public List<WFormula> formulas = new List<WFormula>();
        bool IsFof = false;
        bool IsConj = false;
        bool hasConj = false;

        /// <summary>
        /// Парсит смешанный формат cnf/fof.
        /// </summary>
        /// <param name="source">Имя файла для парсинга</param>
        /// <param name="refdir"></param>
        public void Parse(string source, string refdir = null)
        {
            Lexer lex = TPTPLexer(source, refdir);

            while (!lex.TestTok(TokenType.EOFToken))
            {
                lex.CheckLit("cnf", "fof", "include");
                if (lex.TestLit("cnf"))
                {
                    var clause = Clause.ParseClause(lex);
                    AddClause(clause);
                }
                else if (lex.TestLit("fof"))
                {
                    var formula = WFormula.ParseWFormula(lex);
                    AddFormula(formula);
                }
                else
                {
                    lex.AcceptLit("include");
                    lex.AcceptTok(TokenType.OpenPar);

                    var m = lex.LookLit();
                    var name = m.Substring(1, m.Length - 2);

                    lex.AcceptTok(TokenType.SQString);
                    lex.AcceptTok(TokenType.ClosePar);
                    lex.AcceptTok(TokenType.FullStop);
                    this.Parse(name, refdir);
                }

            }
        }

        /// <summary>
        /// Читает файл используя конфенцию TPTP/
        /// </summary>
        /// <param name="source"></param>
        /// <param name="refdir"></param>
        public Lexer TPTPLexer(string source, string refdir = null)
        {
            refdir ??= Directory.GetCurrentDirectory();

            var name = Path.Combine(refdir, source);

            using (StreamReader sr = new StreamReader(name))
            {
                var lex = new Lexer(sr.ReadToEnd());
                refdir = Path.GetDirectoryName(name);
                return lex;
            }
        }


        public void AddClause(Clause clause)
        {
            if (clause.Type == "negated_conjecture")
                hasConj = true;
            clauses.Add(clause);
        }

        public void AddFormula(WFormula formula)
        {
            if (formula.Type == "negated_conjecture" || formula.Type == "conjecture")
                hasConj = true;
            IsFof = true;
            formulas.Add(formula);
        }
        
        /// <summary>
        /// Преобразовывает все формулы в клаузы.
        ///
        /// </summary>
        public ClauseSet Clausify()
        {
            while(formulas.Count > 0)
            {
                var form = formulas[formulas.Count - 1];
                formulas.Remove(form);
                form = form.NegateConjecture();
                var tmp = form.Clausify();
                clauses.AddRange(tmp); // should be Add
            }
            return new ClauseSet(clauses);
        }
    }
}
