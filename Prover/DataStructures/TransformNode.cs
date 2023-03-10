using System.Collections.Generic;
using System.Text;

namespace Prover.DataStructures
{
    public class TransformNode
    {
        public TransformNode Parent1;
        public TransformNode Parent2;
        public string TransformOperation;

        public bool from_conjecture { get; private set; } = false;

        public TransformNode(string transformation = "input", TransformNode parent1 = null, TransformNode parent2 = null)
        {
            TransformOperation = transformation;
            Parent1 = parent1;
            Parent2 = parent2;
            //Проброс флага в через преобразования
            if ((parent1 is not null && parent1.from_conjecture) ||
                 (parent2 is not null && parent2.from_conjecture))
            {
                from_conjecture = true;
            }
        }

        public void SetTransform(string transformation, TransformNode parent1 = null, TransformNode parent2 = null)
        {
            TransformOperation = transformation;
            Parent1 = parent1;
            Parent2 = parent2;

            //Проброс флага в через преобразования
            if((parent1 is not null && parent1.from_conjecture) || 
                (parent2 is not null && parent2.from_conjecture))
            {
                from_conjecture = true;
            }
        }

        public string TransformationPath()
        {
            List<TransformNode> path = new List<TransformNode>();
            TransformNode actual = this;
            while (actual.Parent1 is not null)
            {
                path.Add(actual.Parent1);
                actual = actual.Parent1;
            }

            path.Reverse();
            StringBuilder sb = new StringBuilder();
            foreach (TransformNode node in path)
            {
                sb.Append(node.TransformOperation + '\n');
                sb.Append(node.ToString() + "\n\n");
            }
            return sb.ToString();

        }

        public void SetFromConjectureFlag()
        {
            from_conjecture = true;
        }
    }
}
