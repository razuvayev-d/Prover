﻿using System.Collections.Generic;
using System.Text;

namespace Prover.DataStructures
{


    public interface IDerivable
    {

    }
    /// <summary>
    /// Тип данных для представления производных, то есть обоснований клауз и формул.
    /// Может быть тривиальным (клауза/формула получаена непосредственно из входных данных)
    /// или состоять из правила вывода и списка родителей.
    /// </summary>
    public class Derivable : IDerivable
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
                    name = string.Format("c{0}", derivedIdCounter++);
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
    public class Derivation : IDerivable
    {
        string op;
        List<IDerivable> parentsList;
        string status;

        public Derivation(string op, List<IDerivable> parents = null, string status = "status(thm)")
        {
            this.op = op;
            this.parentsList = parents;
            this.status = status;
        }

        public static Derivation FlatDerivation(string op, List<IDerivable> parents, string status = "status(thm)")
        {
            List<IDerivable> parentList = new List<IDerivable>();
            foreach (var p in parents)
                parentList.Add(new Derivation("reference", new List<IDerivable> { p }));

            return new Derivation(op, parentList, status);
        }

        public string TransformPath()
        {
            StringBuilder sb = new StringBuilder();
            var list = new List<string>();

            while (true)
            {
                //list.Add();
            }
        }
    }

}