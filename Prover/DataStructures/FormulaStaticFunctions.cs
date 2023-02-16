using Prover.DataStructures;
using Prover.ResolutionMethod;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Prover
{
    public partial class Formula
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
                        f = ((Literal)f.subFormula1).Negate(); //new Formula("", ((Literal)f.subFormula1).Negate());
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

                }
                // TODO: новые изменения
                //else if (f.op == "|")
                //{
                //    if (f.Child1.IsLiteral)
                //    {
                //        var lit = f.Child1 as Literal;
                //        if (lit.IsPropTrue)
                //        {
                //            // True | Q = True                           
                //            f = Literal.True;
                //            m = true;
                //        }
                //        else if (lit.IsPropFalse)
                //        {
                //            // False | Q = Q                          
                //            f = f.Child2;
                //            m = true;
                //        }
                //    }
                //    // symmetric case
                //    if (f.Child2.IsLiteral)
                //    {
                //        var lit = f.Child2 as Literal;
                //        if (lit.IsPropTrue)
                //        {
                //            // Q | True = True
                //            f = Literal.True;
                //            m = true;
                //        }
                //        else if (lit.IsPropFalse)
                //        {
                //            // Q | False = Q
                //            f = f.Child1;
                //            m = true;
                //        }
                //    }

                //}
                else if (f.op == "&")
                {
                    if (f.Child1.IsLiteral)
                    {
                        var lit = f.Child1 as Literal;
                        if (lit.IsPropTrue)
                        {
                            // True & Q = Q                           
                            f = f.Child2;
                            m = true;
                        }
                        else if (lit.IsPropFalse)
                        {
                            // False & Q = False                          
                            f = Literal.False;
                            m = true;
                        }
                    }
                    // symmetric case
                    if (f.Child2.IsLiteral)
                    {
                        var lit = f.Child2 as Literal;
                        if (lit.IsPropTrue)
                        {
                            // Q & True = Q
                            f = f.Child1;
                            m = true;
                        }
                        else if (lit.IsPropFalse)
                        {
                            // Q & False = False
                            f = Literal.False;
                            m = true;
                        }
                    }
                }
                modified |= m;
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
                    if (!subf.subFormula1.CollectFreeVars().Contains(var as Term)) //(!subf.subFormula1.CollectFreeVars().Contains((var as Literal).QuantorVar))
                    {

                        arg2 = new Formula(quant, var, subf.subFormula2);
                        arg1 = subf.subFormula1;
                        f = new Formula(op, arg1, arg2);
                        res = true;
                    }
                    else if (!subf.subFormula2.CollectFreeVars().Contains(var as Term)) //(!subf.subFormula2.CollectFreeVars().Contains(((Literal)var).QuantorVar))
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
            Term var = null, newvar = null, oldBinding = null;
            Formula res = null;
            if (f.IsQuantified)
            {
                // Новая область видимости переменной -> добавить новую привязку к новой
                // переменной. Сохраните потенциальную старую привязку, чтобы восстановить ее при
                // выходе из области видимости позже
                var = ((Term)f.subFormula1);
                newvar = Substitution.FreshVar();
                oldBinding = subst.ModifyBinding(var, newvar);

            }

            if (f.IsLiteral)
            {
                //Formula child = (f as Literal).Substitute(subst);
                //res = (f as Literal).DeepCopy();
                //TODO: тут были изменения, старая версия
                // res = (f as Literal).DeepCopy();
                // (res as Literal).Substitute(subst);
                res = (f as Literal).Substitute(subst);
            }
            else
            {
                //Это составная формула, переименовываем ее
                Term targ1 = null;
                Formula arg1 = null, arg2 = null;

                if (f.IsQuantified)
                {
                    targ1 = newvar;
                    arg2 = FormulaVarRename(f.Child2, subst);
                    res = new Quantor(f.op, targ1, arg2);
                }
                else
                {
                    if (f.HasSubform1)
                        arg1 = FormulaVarRename(f.Child1, subst);
                    if (f.HasSubform2)
                        arg2 = FormulaVarRename(f.Child2, subst);
                    res = new Formula(f.op, arg1, arg2);
                }
            }
            //Мы выходим из области действия квантификатора,
            //поэтому восстанавливаем подстановку.
            if (f.IsQuantified)
            {
                subst.ModifyBinding(var, oldBinding);
            }

            return res;

            throw new NotImplementedException();
        }

        public static Formula FormulaRekSkolemize(Formula f, List<Term> variables, Substitution subst)
        {
            if (f.IsLiteral)
            {
                Formula child = (f/*.subFormula1*/ as Literal).Instantiated(subst);
                f = child; //new Formula("", child);
            }
            else if (f.op == "?")
            {
                var var = f.subFormula1 as Term; // (f as Quantor).Variable;// (f.subFormula1 as Literal).QuantorVar;
                var skTerm = Skolem.NewSkolemTerm(variables);
                Term oldbinding = subst.ModifyBinding(var, skTerm);
                f = FormulaRekSkolemize(f.subFormula2, variables, subst);
                subst.ModifyBinding(var, oldbinding);
            }
            else if (f.op == "!")
            {
                var var = f.subFormula1 as Term;
                variables.Add(var);
                var handle = FormulaRekSkolemize(f.subFormula2, variables, subst);
                f = new Formula("!", (f.subFormula1 as Term), handle);
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

        /// <summary>
        ///     Shift all (universal) quantor to the outermost level.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Formula formulaShiftQuantorsOut(Formula f)
        {
            List<Term> varlist = new List<Term>();
            //(f, varlist) = SeparateQuantors(f);
            f = SeparateQuantors(f, varlist);
            while (varlist.Count > 0)
            {
                Term t = varlist[varlist.Count - 1];
                varlist.Remove(t);
                f = new Quantor("!", t, f);
            }
            return f;
        }
        /// <summary>
        ///  Remove all quantors from f, returning the quantor-free core of the
        /// formula and a list of quantified variables.This will only be
        /// applied to Skolemized formulas, thus finding an existential
        ///quantor is an error.To be useful, the input formula also has to be
        ///variable-normalized.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="varlist"></param>
        /// <returns></returns>
        public static Formula SeparateQuantors(Formula f, List<Term> varlist)
        {
            Formula result = f; //= f.Copy();
            //if (varlist is null)
            //    varlist = new List<Term>();
            if (f.IsQuantified)
            {
                Debug.Assert(f.op.Equals("!"));
                varlist.Add((Term)f.subFormula1);
                result = SeparateQuantors(f.subFormula2, varlist);
            }
            else if (!f.IsLiteral)
            {
                Formula arg1 = null, arg2 = null;
                if (f.HasSubform1)
                    arg1 = SeparateQuantors(f.subFormula1, varlist);
                if (f.HasSubform2)
                    arg2 = SeparateQuantors(f.subFormula2, varlist);
                result = new Formula(f.op, arg1, arg2);
            }
            return result;
        }
        public static Formula FormulaDistributeDisjunctions(Formula f)
        {
            Formula arg1 = null, arg2 = null;
            if (f.IsQuantified)
            {
                arg1 = f.subFormula1;
                arg2 = FormulaDistributeDisjunctions(f.subFormula2);
                f = new Quantor(f.op, (Term)arg1, arg2);
            }
            else if (f.IsLiteral)
            {
            }
            else
            {
                if (f.HasSubform1)
                    arg1 = FormulaDistributeDisjunctions(f.subFormula1);
                if (f.HasSubform2)
                    arg2 = FormulaDistributeDisjunctions(f.subFormula2);
                f = new Formula(f.op, arg1, arg2);
            }
            if (f.op.Equals("|"))
            {
                if (f.subFormula1.op.Equals("&"))
                {
                    // (P&Q)|R -> (P|R) & (Q|R)
                    arg1 = new Formula("|", f.Child1.Child1, f.Child2);
                    arg2 = new Formula("|", f.Child1.Child2, f.Child2);
                    f = new Formula("&", arg1, arg2);
                    f = FormulaDistributeDisjunctions(f);
                }
                else if (f.Child2.op.Equals("&"))
                {
                    // (R|(P&Q) -> (R|P) & (R|Q)
                    arg1 = new Formula("|", f.Child1, f.Child2.Child1);
                    arg2 = new Formula("|", f.Child1, f.Child2.Child2);
                    f = new Formula("&", arg1, arg2);
                    f = FormulaDistributeDisjunctions(f);
                }
            }
            return f;
        }

    }
}





