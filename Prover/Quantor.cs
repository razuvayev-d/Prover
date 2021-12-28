using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{

    class Quantor : Formula
    {
        public enum Type
        {
            Existential,
            Universal
        }

        public Term Variable => (Term)this.Child1;
        public Formula Formula => this.Child2;
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
