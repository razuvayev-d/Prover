using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover
{


    interface IDerivable
    {

    }
    /// <summary>
    /// Тип данных для представления производных, то есть обоснований клауз и формул.
    /// Может быть тривиальным (клауза/формула получаена непосредственно из входных данных)
    /// или состоять из правила вывода и списка родителей.
    /// </summary>
    class Derivable : IDerivable
    {
        /// <summary>
        /// Счетчик для генерирования новых имет для клауз
        /// </summary>
        static int derivedIdCounter = 0;

        /// <summary>
        ///  Укажите, следует ли печатать производные как часть объектов Derivable
        /// объектов.Поддержка этого зависит от конкретных классов.
        /// </summary>
        static bool printDerivation = false;

        string name;
        public virtual string Name
        {
            set
            {
                if (value == null)
                    name = String.Format("c{0}", derivedIdCounter++);
                name = value;
            }
        }

        public Derivation Derivation
        {
            set
            {
                derivation = value;
            }
            get
            {
                return derivation;
            }
        }

        Derivation derivation;
        int refCount;

        public Derivable(string name = null, Derivation derivation = null)
        {
            Name = name;
            this.derivation = derivation;
            refCount = 0;
        }

        //public void SetDerivation(Derivation derivation)
        //{
        //    this.derivation = derivation;
        //}
    }

    /// <summary>
    /// Объект деривации. Производная является либо тривиальной ("input"), либо
    /// ссылка на существующий объект деривации("reference"), или
    /// умозаключение со списком предпосылок.
    /// </summary>
    class Derivation : IDerivable
    {
        string op;
        public Derivation(string op, List<IDerivable> parents = null, string status = "status(thm)")
        {
            this.op = op;
        }

        public static Derivation FlatDerivation(string op, List<IDerivable> parents, string status = "status(thm)")
        {
            List<IDerivable> parentList = new List<IDerivable>();
            foreach (var p in parents)
                parentList.Add(new Derivation("reference", new List<IDerivable> { p }));

            return new Derivation(op, parentList, status);
       }
    }

}