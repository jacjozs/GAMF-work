using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuzzy
{
    public class Rule : IEquatable<Rule>
    {
        /// <summary>
        /// An MF index for each input
        /// </summary>
        public int[] Inputs;
        /// <summary>
        /// An MF index for each output
        /// </summary>
        public int[] Outputs;
        /// <summary>
        /// 
        /// </summary>
        public float Weight;
        /// <summary>
        /// 0 is max, 1 is min
        /// </summary>
        public int ConnectionType;

        public Rule()
        {

        }

        // 2 2, 1 (1) : 1   // input input, output (weight) : connectiontype
        public Rule(string DataString)
        {
            // Load inputs
            string[] inputs = DataString.Split(',')[0].Split(' ');
            Inputs = new int[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
            {
                Inputs[i] = int.Parse(inputs[i]);
            }

            // Load outputs
            string[] outputs = DataString.Split(',')[1].Split('(')[0].Trim().Split(' ');
            Outputs = new int[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
            {
                Outputs[i] = int.Parse(outputs[i]);
            }

            // Load the weight
            Weight = float.Parse(DataString.Split('(')[1].Split(')')[0], System.Globalization.CultureInfo.InvariantCulture);

            // Load the connection type
            ConnectionType = int.Parse(DataString.Split(':')[1].Trim());
        }

        public override string ToString()
        {
            string outString = "";

            for (int i = 0; i < Inputs.Length-1; i++)
            {
                outString += Inputs[i] + " ";
            }
            outString += Inputs[Inputs.Length - 1] + ", ";
            for (int i = 0; i < Outputs.Length; i++)
            {
                outString += Outputs[i] + " ";
            }
            outString += "(" + Weight + ")" + " : " + ConnectionType + "\r\n";
            
            return outString;
        }

        /// <summary>
        /// Returns true if all the values match with an other given rule
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Rule other)
        {
            if (other.Weight != Weight)
                return false;

            if (other.ConnectionType != ConnectionType)
                return false;

            if (other.Inputs.Length != Inputs.Length)
                return false;

            if (other.Outputs.Length != Outputs.Length)
                return false;

            for (int i = 0; i < Outputs.Length; i++)
                if (other.Outputs[i] != Outputs[i])
                    return false;

            for (int i = 0; i < Inputs.Length; i++)
                if (other.Inputs[i] != Inputs[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Returns true if this has the same values for input mf-s as an other given rule
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool SameInput(Rule other)
        {
            if (other.Inputs.Length != Inputs.Length)
                return false;

            for (int i = 0; i < Inputs.Length; i++)
                if (other.Inputs[i] != Inputs[i])
                    return false;

            return true;
        }
    }
}
