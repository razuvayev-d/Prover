using System.Collections.Generic;

namespace Prover.DataStructures
{
    public class PredicateAbstraction
    {
        bool Sign;
        string Symbol;
        public bool Item1 => Sign;
        public string Item2 => Symbol;

        public PredicateAbstraction(bool sign, string name)
        {
            this.Sign = sign;
            this.Symbol = name;
        }

        public static implicit operator PredicateAbstraction((bool, string) pair)
        {
            return new PredicateAbstraction(pair.Item1, pair.Item2);
        }

        public override int GetHashCode()
        {
            return Symbol.GetHashCode() + Sign.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is PredicateAbstraction)
            {
                return ((PredicateAbstraction)obj).GetHashCode() == GetHashCode();
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", Sign, Symbol);
        }

        public static bool ContainsKey(Dictionary<List<PredicateAbstraction>, List<Clause>> PredAbstrSet, List<PredicateAbstraction> Key)
        {
            var keys = PredAbstrSet.Keys;

            foreach (var key in keys)
            {
                if (CompareList(key, Key)) return true;
            }
            return false;
        }

        private static bool CompareList(List<PredicateAbstraction> a, List<PredicateAbstraction> b)
        {
            if (a.Count != b.Count) return false;
            var n = a.Count;
            for (int i = 0; i < n; i++)
            {
                if (!a[i].Equals(b[i])) return false;
            }
            return true;
        }
    }


    public class PredicateAbstractionArray
    {
        public List<PredicateAbstraction> array = new List<PredicateAbstraction>();


        public int Count => array.Count;
        public PredicateAbstractionArray(List<PredicateAbstraction> array)
        {
            this.array = array;
        }
        public void Add(PredicateAbstraction pa)
        {
            array.Add(pa);
        }


        public static implicit operator PredicateAbstractionArray(List<PredicateAbstraction> list)
        {
            return new PredicateAbstractionArray(list);
        }

        public override int GetHashCode()
        {
            int res = 1;
            foreach (var ar in array)
                res += ar.GetHashCode();
            return res;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PredicateAbstractionArray)) return false;

            PredicateAbstractionArray A = obj as PredicateAbstractionArray;
            if (A.Count != Count) return false;

            var n = A.Count;
            for (int i = 0; i < n; i++)
            {
                if (!A.array[i].Equals(array[i])) return false;
            }
            return true;
        }

        public override string ToString()
        {
            return array.ToString();
        }
    }

}
