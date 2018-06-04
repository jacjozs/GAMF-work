using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuzzy
{
    public class InputOutput
    {
        /// <summary>
        /// The identifier of this ioput
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// THe minimum and maximum value for this ioput
        /// </summary>
        public float[] Range { get; set; }
        /// <summary>
        /// The number of membership functions this ioput has
        /// </summary>
        public int NumMFs
        {
            get { return MFs.Length; }
            set { MFs = new MembershipFunction[value]; }
        }
        /// <summary>
        /// Used to store whether this is used as an input or an output
        /// </summary>
        public bool IsInput { get; set; }
        /// <summary>
        /// The index of this ioput in the fuzzy system
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// The membership functions
        /// </summary>
        public MembershipFunction[] MFs { get; set; }

        public InputOutput()
        {
            Range = new float[2];
        }

        public override string ToString()
        {
            return ToString();
        }
        public string ToString(bool includeValues = true)
        {
            string outString = "";

            outString += "Name='" + Name + "'\r\n";
            outString += "Range=[" + Range[0].ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB")) + " " + Range[1].ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB")) + "]\r\n";
            outString += "NumMFs=" + NumMFs + "\r\n";
            for (int i = 0; i < MFs.Length; i++)
            {
                outString += "MF" + (i + 1) + "='";
                if (IsInput)
                    outString += "A";
                else
                    outString += "B";
                outString += "_{" + Index + ";" + (i + 1) + "}':" + MFs[i].ToString(includeValues);
            }

            return outString;
        }
    }
}
