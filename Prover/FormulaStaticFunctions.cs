using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{
    partial class Formula
    {
        /// <summary>
        ///Convert f into a NNF.Equivalences(<=>) are eliminated
        ///polarity-dependend, top to bottom.Returns(f', m), where f' is a
        /// NNF of f, and m indicates if f!=f'
        /// </summary>
        public static (Formula, bool) FormulaNNF(Formula f, bool polarity)
        {
            bool normalForm = false;
            bool modified = false;
            bool m;
            Formula handle;
            while (!normalForm)
            {
                normalForm = true;
                (f, m) = Formula.RootFormulaNNF(f, polarity);
                modified |= m;            
                if (f.op == "~")
                {
                    (handle, m) = Formula.FormulaNNF(f.subFormula1, !polarity);
                    if (m)
                    {
                        normalForm = false;
                        f = new Formula("~", handle);
                    }
                }
                else if (f.op == "!" || f.op == "?")
                {
                    (handle, m) = Formula.FormulaNNF(f.subFormula2, polarity);
                    if (m)
                    {
                        normalForm = false;
                        f = new Formula(f.op, f.subFormula1, handle);
                    }
                }
                else if (f.op == "|" || f.op == "&")
                {
                    Formula handle1, handle2;
                    bool m1, m2;
                    (handle1, m1) = Formula.FormulaNNF(f.subFormula1, polarity);
                    (handle2, m2) = Formula.FormulaNNF(f.subFormula2, polarity);
                    m = m1 || m2;
                    if (m)
                    {
                        normalForm = false;
                        f = new Formula(f.op, handle1, handle2);
                    }
                }
                else
                {
                    
                    if (!f.IsLiteral)
                    {
                        if ((f.op == "" && f.Child1.IsLiteral))
                        {
                            return (f.Child1, modified);
                        }
                        throw new Exception("Formula is not Literal");
                    }
                }
                modified |= m;
            }
            return (f, modified);
        }

        /// <summary>
        /// Применить все правила преобразования ННФ, которые могут быть применены на верхнем
        /// уровне. Вернуть результат и флаг модификации.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="polarity"></param>
        /// <returns></returns>
        public static (Formula, bool) RootFormulaNNF(Formula f, bool polarity)
        {

            bool normalForm = false;
            bool modified = false;

            while (!normalForm)
            {
                normalForm = true;
                bool m = false;

                if (f.op == "~")
                {
                    if (f.subFormula1.IsLiteral)
                    {
                        // Move negations into literals
                        f = new Formula("", ((Literal)f.subFormula1).Negate());
                        m = true;
                    }

                    else if (f.subFormula1.op == "|")
                    {
                        // De Morgan: ~(P|Q) -> ~P & ~Q
                        f = new Formula("&",
                                    new Formula("~", f.subFormula1.subFormula1),
                                    new Formula("~", f.subFormula1.subFormula2));
                        m = true;
                    }

                    else if (f.subFormula1.op == "&")
                    {
                        // De Morgan: ~(P&Q) -> ~P | ~Q
                        f = new Formula("|",
                                    new Formula("~", f.subFormula1.subFormula1),
                                    new Formula("~", f.subFormula1.subFormula2));
                        m = true;

                    }

                    else if (f.subFormula1.op == "!")
                    {
                        // ~(![X]:P) -> ?[X]:~P
                        f = new Formula("?",
                                    f.subFormula1.subFormula1,
                                    new Formula("~", f.subFormula1.subFormula2));
                        m = true;
                    }

                    else if (f.subFormula1.op == "?")
                    {
                        // ~(?[X]:P) -> ![X]:~P
                        f = new Formula("!",
                                    f.subFormula1.subFormula1,
                                    new Formula("~", f.subFormula1.subFormula2));
                        m = true;
                    }
                }
                else if (f.op == "=>")
                {
                    // Expand P=>Q into ~P|Q
                    f = new Formula("|",
                                new Formula("~", f.subFormula1),
                                f.subFormula2);
                    m = true;
                }

                else if (f.op == "<=>")
                {
                    if (polarity)
                    {
                        // P<=>Q -> (P=>Q)&(Q=>P)
                        f = new Formula("&",
                                    new Formula("=>", f.subFormula1, f.subFormula2),
                                    new Formula("=>", f.subFormula2, f.subFormula1));
                        m = true;
                    }
                    else
                    {
                        // P<=>Q -> (P & Q) | (~P & ~Q)
                        f = new Formula("|",
                                    new Formula("&", f.subFormula1, f.subFormula2),
                                    new Formula("&",
                                            new Formula("~", f.subFormula1),
                                            new Formula("~", f.subFormula2)));
                        m = true;

                    }

                    //normalform = !m;
                    modified |= m;
                }
            }
            return (f, modified);
        }

        /// <summary>
        /// Выполните минископирование, т.е. переместите кванторы как можно дальше, чтобы
        /// чтобы их областью действия была только наименьшая подформула, в которой встречается
        /// встречается переменная.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static (Formula, bool) FormulaMiniScope(Formula f)
        {
            var res = false;
            Formula arg1, arg2;
            if (f.IsQuantified)
            {
                var op = f.subFormula2.op;
                var quant = f.op;
                var var = f.subFormula1;
                var subf = f.subFormula2;

                if (op == "&" || op == "|")
                {
                    if (!subf.subFormula1.CollectFreeVars().Contains((var as Literal).QuantorVar))
                    {

                        arg2 = new Formula(quant, var, subf.subFormula2);
                        arg1 = subf.subFormula1;
                        f = new Formula(op, arg1, arg2);
                        res = true;
                    }
                    else if (!subf.subFormula2.CollectFreeVars().Contains(((Literal)var).QuantorVar))

                    {

                        arg1 = new Formula(quant, var, subf.subFormula1);
                        arg2 = subf.subFormula2;
                        f = new Formula(op, arg1, arg2);
                        res = true;
                    }
                    else
                    {
                        if (op == "&" && quant == "!")
                        {

                            arg1 = new Formula("!", var, subf.subFormula1);
                            arg2 = new Formula("!", var, subf.subFormula2);
                            f = new Formula("&", arg1, arg2);
                            res = true;
                        }
                        else if (op == "|" && quant == "?")
                        {

                            arg1 = new Formula("?", var, subf.subFormula1);
                            arg2 = new Formula("?", var, subf.subFormula2);
                            f = new Formula("|", arg1, arg2);
                            res = true;
                        }
                    }
                }
            }
            arg1 = f.subFormula1;
            arg2 = f.subFormula2;
            bool modified = false;
            bool m;
            if (f.HasSubform1)
            {
                (arg1, m) = Formula.FormulaMiniScope(f.subFormula1);
                modified |= m;
            }
            if (f.HasSubform2)
            {
                (arg2, m) = FormulaMiniScope(f.subFormula2);
                modified |= m;
            }
            if (modified)
            {
                f = new Formula(f.op, arg1, arg2);
                (f, m) = FormulaMiniScope(f);
                res = true;
            }
            return (f, res);
        }



        /// <summary>
        ///  Переименуйте переменные в f так, чтобы все связанные переменные были уникальными.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Formula FormulaVarRename(Formula f, Substitution subst = null)
        {
            subst ??= new Substitution();
            Term newvar, oldBinding;
            if (f.IsQuantified)
            {
                var var = ((Literal)f.subFormula1).Name;
                newvar = Substitution.FreshVar();
                //oldBinding = subst.ModifyBinding(var, newvar);

            }
            throw new NotImplementedException();
        }

        public static Formula FormulaRekSkolemize(Formula f, List<Term> variables, Substitution subst)
        {
            if (f.IsLiteral)
            {
                Formula child = (f/*.subFormula1*/ as Literal).Instantiated(subst);
                f = new Formula("", child);
            }
            else if(f.op == "?")
            {
                var var = (f.subFormula1 as Literal).QuantorVar;
                var skTerm = Skolem.NewSkolemTerm(variables);
                Term oldbinding = subst.ModifyBinding(var, skTerm);
                f = FormulaRekSkolemize(f.subFormula2, variables, subst);
                subst.ModifyBinding(var, oldbinding);
            }
            else if (f.op == "!")
            {
                var var = (f.subFormula1 as Literal).QuantorVar;
                variables.Add(var);
                var handle = FormulaRekSkolemize(f.subFormula2, variables, subst);
                f = new Formula("!", (f.subFormula1 as Literal), handle);
                variables.RemoveAt(variables.Count - 1); // pop
            }
            else
            {
                Formula arg1 = null;
                Formula arg2 = null;
                if (f.HasSubform1)
                    arg1 = FormulaRekSkolemize(f.subFormula1, variables, subst);
                if (f.HasSubform2) 
                    arg2 = FormulaRekSkolemize(f.subFormula2, variables, subst);
                f = new Formula(f.op, arg1, arg2);
            }
            return f;
        }
        public static Formula FormulaScolemize(Formula f)
        {
            var vars = f.CollectFreeVars();
            return FormulaRekSkolemize(f, vars, new Substitution());
            throw new NotImplementedException();
        }
        public static Formula formulaShiftQuantorsOut(Formula f)
        {
            throw new NotImplementedException();
        }
        public static Formula FormulaDistributeDisjunctions(Formula f)
        {
            throw new NotImplementedException();
        }

    }
}
        
                           
                
            

