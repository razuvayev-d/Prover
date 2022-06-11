using System;

namespace Prover.DataStructures
{

    public class Quantor : Formula
    {
        public enum Type
        {
            Existential,
            Universal
        }

        public Term Variable => (Term)Child1;
        public Formula Formula => Child2;
        Type type;
        //public Quantor(Type type, Term variable, Formula formula)
        //{
        //    this.type = type;
        //    this.variable = variable;
        //    this.formula = formula;
        //}

        public Quantor(string type, Term variable, Formula formula) : base(type, variable, formula)
        {
            if (type == "!") this.type = Type.Universal;
            else if (type == "?") this.type = Type.Existential;
            else throw new Exception("Unknown quantor symbol");
        }
    }
}
