using Prover.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover.DataStructures
{
    /// <summary>
    ///  Datatype for the complete first-order formula, including
    ///meta-information like type and name.
    /// </summary>
    class WFormula : Derivable
    {
        Formula formula;
        string type;
        string name;

        public string Type => type;
        public Formula Formula => formula;

        public WFormula(Formula formula, string type = "plain", string name = null)
        {
            this.formula = formula;
            this.type = type;
            this.name = name;
        }

        public Signature CollectSig(Signature sig = null)
        {
            return formula.CollectSig(sig);
        }

        /// <summary>
        /// Parse a formula in (slightly simplified) TPTP-3 syntax. It is
        ///     written
        ///         fof(<name>, <type>, <lformula>).
        ///  where<name> is a lower-case ident, type is a lower-case ident
        /// from a specific list, and<lformula> is a Formula.

        ///For us, all clause types are essentially the same, so we only
        ///distinguish "axiom", "conjecture", and "negated_conjecture", and
        ///map everything else to "plain".
        /// </summary>
        /// <param name="lexer"></param>
        public static WFormula ParseWFormula(Lexer lexer)
        {
            lexer.AcceptLit("fof");
            lexer.AcceptTok(TokenType.OpenPar);
            var name = lexer.LookLit();
            lexer.AcceptTok(TokenType.IdentLower, TokenType.SQString);
            lexer.AcceptTok(TokenType.Comma);

            var type = lexer.LookLit();

            if (!(type == "axiom" || type == "conjecture" || type == "negated_conjecture"))
                type = "plain";
            lexer.AcceptTok(TokenType.IdentLower);
            lexer.AcceptTok(TokenType.Comma);

            var form = Formula.ParseFormula(lexer);

            lexer.AcceptTok(TokenType.ClosePar);
            lexer.AcceptTok(TokenType.FullStop);

            var res = new WFormula(form, type, name);
            res.Derivation = new Derivation("input");
            return res;
        }
        /// <summary>
        /// Если формула является заключением возвращает ее отрицание,
        /// иначе возвращает неизменную формулу.
        /// </summary>
        /// <returns></returns>
        public WFormula NegateConjecture()
        {
            if (type == "conjecture")
            {
                var negf = new Formula("~", formula);
                var negW = new WFormula(negf, "negated_conjecture");
                negW.Derivation = Derivation.FlatDerivation("assume_negation",
                                          new List<IDerivable> { Derivation },
                                          "status(cth)");
                return negW;
            }
            return this;
        }
        /// <summary>
        ///  Преобразовывает формулу в клаузу
        ///  Исх: wFormulaClausify
        /// </summary>
        public List<Clause> Clausify()
        {
            var wf = WFormulaCnf(this);
            var clauses = ClauseSet.FormulaCNFSplit(wf);

            foreach (var c in clauses)
            {
                c.Derivation = Derivation.FlatDerivation("split_conjunct", new List<IDerivable> { wf.Derivation }); //!
            }
            return clauses;
        }
        /// <summary>
        /// Convert a (wrapped) formula to Conjunctive Normal Form.
        /// </summary>
        public static WFormula WFormulaCnf(WFormula wf) //!!!!
        {
            bool m, m0, m1;
            Formula f;
            WFormula tmp;
            (f, m0) = Formula.FormulaOpSimplify(wf.Formula);
            (f, m1) = Formula.FormulaSimplify(f);

            if (m0 || m1)
            {
                tmp = new WFormula(f, wf.type);
                tmp.Derivation = Derivation.FlatDerivation("fof_simplification", new List<IDerivable> { wf });
                wf = tmp;//!!!
            }

            (f, m) = Formula.FormulaNNF(f, true);
            if (m)
            {
                tmp = new WFormula(f, wf.type);
                tmp.Derivation = Derivation.FlatDerivation("fof_nnf", new List<IDerivable> { wf });
                wf = tmp;
            }

            (f, m) = Formula.FormulaMiniScope(f);
            if (m)
            {
                tmp = new WFormula(f, wf.type);
                tmp.Derivation = Derivation.FlatDerivation("shift_quantors", new List<IDerivable> { wf });
                wf = tmp;
            }
            // TODO: ошибка в FormulaVarRename 
            // f = Formula.FormulaVarRename(f);
            if (!f.Equals(wf.Formula))
            {
                tmp = new WFormula(f, wf.type);
                tmp.Derivation = Derivation.FlatDerivation("variable_rename", new List<IDerivable> { wf });
                wf = tmp;
            }

            f = Formula.FormulaScolemize(f);
            if (!f.Equals(wf.Formula))
            {
                tmp = new WFormula(f, wf.type);
                tmp.Derivation = Derivation.FlatDerivation("skolemize", new List<IDerivable> { wf }, "status(esa)");
                wf = tmp;
            }

            f = Formula.formulaShiftQuantorsOut(f);
            if (!f.Equals(wf.Formula))
            {
                tmp = new WFormula(f, wf.type);
                tmp.Derivation = Derivation.FlatDerivation("shift_quantors", new List<IDerivable> { wf });
                wf = tmp;
            }

            f = Formula.FormulaDistributeDisjunctions(f);
            if (!f.Equals(wf.Formula))
            {
                tmp = new WFormula(f, wf.type);
                tmp.Derivation = Derivation.FlatDerivation("distribute", new List<IDerivable> { wf });
                wf = tmp;
            }
            return wf;
        }
    }
}
