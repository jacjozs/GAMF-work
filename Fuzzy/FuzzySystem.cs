using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuzzy
{
    public enum FuzzyType
    {
        Mamdani,
        Larsen,
        Sugeno,
        Sparse
    }
    public enum AndMethodType
    {
        min,
        max
    }
    public enum OrMethodType
    {
        min,
        max
    }
    public enum ImpMethodType
    {
        min,
        max,
        prod
    }
    public enum AggMethodType
    {
        min,
        max
    }
    public enum DefuzzMethodType
    {
        COA,    // Center of area
        COG,    // Center of gravity
        LOM,    // Largest of maximum
        MOM,    // Middle of maximium
        SOM,    // Smallest of maximum
        wtaver, // Weighted average of the x-values, with the y-values used as weights
        wtsum, // Weighted sum of the x-values, with the y-values used as weights
    }

    public class FuzzySystem
    {
        /// <summary>
        /// The identifier of the system
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The type of the system
        /// </summary>
        public FuzzyType Type { get; set; }
        /// <summary>
        /// The version of something?
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Returns the number of inputs or initializes the input array to a given value
        /// </summary>
        public int NumInputs
        {
            get { return Inputs.Length; }
            set { Inputs = new InputOutput[value]; }
        }
        /// <summary>
        /// Returns the number of outputs or initializes the output array to a given value
        /// </summary>
        public int NumOutputs
        {
            get { return Outputs.Length; }
            set { Outputs = new InputOutput[value]; }
        }
        /// <summary>
        /// Returns the number of rules or initializes the rule array to a given value
        /// </summary>
        public int NumRules
        {
            get { return Rules.Length; }
            set { Rules = new Rule[value]; }
        }

        public AndMethodType AndMethod { get; set; }
        public OrMethodType OrMethod { get; set; }
        public ImpMethodType ImpMethod { get; set; }
        public AggMethodType AggMethod { get; set; }
        /// <summary>
        /// The method used for the defuzzification of the system
        /// </summary>
        public DefuzzMethodType DefuzzMethod { get; set; }

        /// <summary>
        /// The inputs
        /// </summary>
        public InputOutput[] Inputs;
        /// <summary>
        /// The outputs
        /// </summary>
        public InputOutput[] Outputs;
        /// <summary>
        /// The rules of the system
        /// </summary>
        public Rule[] Rules;

        public FuzzySystem()
        {

        }
        public FuzzySystem(FuzzySystem otherFuzzySystem)
        {
            Name = otherFuzzySystem.Name;
            //Type = otherFuzzySystem.Type;
            Version = otherFuzzySystem.Version;
            Inputs = (InputOutput[])otherFuzzySystem.Inputs.Clone();
            Outputs = (InputOutput[])otherFuzzySystem.Outputs.Clone();
            Rules = (Rule[])otherFuzzySystem.Rules.Clone();
            AndMethod = otherFuzzySystem.AndMethod;
            OrMethod = otherFuzzySystem.OrMethod;
            ImpMethod = otherFuzzySystem.ImpMethod;
            AggMethod = otherFuzzySystem.AggMethod;
            DefuzzMethod = otherFuzzySystem.DefuzzMethod;
        }

        /// <summary>
        /// Return the stored data in the same format that we processed
        /// </summary>
        public override string ToString()
        {
            return ToString();
        }
        public string ToString(bool includeValues = true)
        {
            string outString = "";

            outString += "[System]\r\n";
            outString += "Name='" + Name + "'\r\n";
            outString += "Type='" + FuzzyTypeToString(Type) + "'\r\n";
            outString += "Version=" + Version + "\r\n";
            outString += "NumInputs=" + NumInputs + "\r\n";
            outString += "NumOutputs=" + NumOutputs + "\r\n";
            outString += "NumRules=" + NumRules + "\r\n";
            outString += "AndMethod='" + AndMethodToString(AndMethod) + "'\r\n";
            outString += "OrMethod='" + OrMethodToString(OrMethod) + "'\r\n";
            outString += "ImpMethod='" + ImpMethodToString(ImpMethod) + "'\r\n";
            outString += "AggMethod='" + AggMethodToString(AggMethod) + "'\r\n";
            outString += "DefuzzMethod='" + DefuzzMethodToString(DefuzzMethod) + "'\r\n";
            outString += "\r\n";
            for (int i = 0; i < Inputs.Length; i++)
            {
                outString += "[Input" + (i + 1) + "]\r\n";
                outString += Inputs[i].ToString(includeValues) + "\r\n";
            }
            for (int i = 0; i < Outputs.Length; i++)
            {
                outString += "[Output" + (i + 1) + "]\r\n";
                outString += Outputs[i].ToString(includeValues) + "\r\n";
            }
            outString += "[Rules]\r\n";
            for (int i = 0; i < Rules.Length; i++)
                outString += Rules[i].ToString();

            return outString;
        }
        
        public void CopyDataFrom(FuzzySystem otherFuzzySystem)
        {
            Name = otherFuzzySystem.Name;
            //Type = fuz.Type;
            Version = otherFuzzySystem.Version;
            Inputs = (InputOutput[])otherFuzzySystem.Inputs.Clone();
            Outputs = (InputOutput[])otherFuzzySystem.Outputs.Clone();
            Rules = (Rule[])otherFuzzySystem.Rules.Clone();
            AndMethod = otherFuzzySystem.AndMethod;
            OrMethod = otherFuzzySystem.OrMethod;
            ImpMethod = otherFuzzySystem.ImpMethod;
            AggMethod = otherFuzzySystem.AggMethod;
            DefuzzMethod = otherFuzzySystem.DefuzzMethod;
        }

        /// <summary>
        /// Used for internal calculations, calculates output for a single rule
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="ruleindex"></param>
        /// <returns>Returns the value of the output specified by rule in a given location</returns>
        public float CalculateRuleValue(float[] coordinates, int ruleindex)
        {
            float[] inputValues = new float[Inputs.Length];
            for (int i = 0; i < Inputs.Length; i++)
            {
                // Get the membership value of the MF in the i-th input
                inputValues[i] = Inputs[i].MFs[Rules[ruleindex].Inputs[i] - 1].GetMembershipValue(coordinates[i]);
            }
            if (Rules[ruleindex].ConnectionType == 0)   // OR type -> return max
                return inputValues.Max();
            if (Rules[ruleindex].ConnectionType == 1)   // AND type -> return min
                return inputValues.Min();

            // This should never be reached when used properly
            throw new Exception("Rule index" + ruleindex + " invalid, has a connection type of " + Rules[ruleindex].ConnectionType + ". Expected 0 or 1.");
        }

        /// <summary>
        /// Get the defuzzied value of the system
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns>The output's defuzzied value for the specified input coordinates</returns>
        public virtual float CalculateOutputValue(float[] coordinates)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Load inputs from a file and print the outputs into an other
        /// </summary>
        public void LoadInputsAndSaveOutputs(string InputFile, string OutputFile)
        {
            string CurrentLine;
            StreamReader inputFile = new StreamReader(InputFile);
            StreamWriter outputFile = new StreamWriter(OutputFile);
            string[] inputStrings;
            float[] inputValues;
            
            while ((CurrentLine = inputFile.ReadLine()) != null)
            {
                inputStrings = CurrentLine.Trim().Replace("\t", " ").Replace("  ", " ").Replace("  ", " ").Replace(",", ".").Split(' ');

                inputValues = new float[inputStrings.Length];
                for (int inputId = 0; inputId < inputStrings.Length; inputId++)
                {
                    try
                    {
                        inputValues[inputId] = float.Parse(inputStrings[inputId], NumberStyles.Float, CultureInfo.InvariantCulture);
                    }
                    catch (Exception exc)
                    {
                        outputFile.WriteLine(exc.Message + ": " + '"' + inputStrings[inputId] + '"');
                        inputFile.Close();
                        outputFile.Close();
                        throw exc;
                    }
                }
                outputFile.WriteLine(CalculateOutputValue(inputValues).ToString(CultureInfo.InvariantCulture));
            }

            inputFile.Close();
            outputFile.Close();
        }

        /// <summary>
        /// Automatically generate equal sized trapezoid MF-s for each MF.
        /// </summary>
        /// <param name="baseRatio">bigger(bottom) base * baseRatio = smaller(top) base</param>
        /// <param name="input">bigger(bottom) base * baseRatio = smaller(top) base</param>
        /// <param name="output">bigger(bottom) base * baseRatio = smaller(top) base</param>
        public void GenerateTrapezoidMFs(float baseRatio = 1.0f / 3.0f, bool input = true, bool output = true)
        {
            // Create input MFs
            if (input)
                for (int i = 0; i < NumInputs; i++)
                {
                    // calculate the steps at which we make steps between the specified trapeze points:
                    // (total range) / (2 * NumMF - 1)
                    float stepSize = (Inputs[i].Range[1] - Inputs[i].Range[0]) / (2.0f * Inputs[i].NumMFs - 1.0f);

                    // create the trapeze shapes
                    for (int j = 0; j < Inputs[i].NumMFs; j++)
                        Inputs[i].MFs[j] = MembershipFunction.CreateTrapezoid(Inputs[i].Range[0] + stepSize * (j * 2 - 1 + 1.5f), stepSize * 3.0f, stepSize * 3.0f * baseRatio);
                }

            if (output)
                // Create output MFs
                for (int i = 0; i < NumOutputs; i++)
                {
                    // calculate the steps at which we make steps between the specified trapeze points:
                    // (total range) / (2 * NumMF - 1)
                    float stepSize = (Outputs[i].Range[1] - Outputs[i].Range[0]) / (2.0f * Outputs[i].NumMFs - 1.0f);

                    // create the trapeze shapes
                    for (int j = 0; j < Outputs[i].NumMFs; j++)
                        Outputs[i].MFs[j] = MembershipFunction.CreateTrapezoid(Outputs[i].Range[0] + stepSize * (j * 2 - 1 + 1.5f), stepSize * 3.0f, stepSize * 3.0f * baseRatio);
                }
        }
        
        public void GenerateConstantOutputMFs()
        {
            for (int i = 0; i < NumOutputs; i++)
            {
                // create the trapeze shapes
                for (int j = 0; j < Outputs[i].NumMFs; j++)
                {
                    Outputs[i].MFs[j] = new MembershipFunction();
                    Outputs[i].MFs[j].Type = MFtype.constant;

                }
            }
        }
        /// <summary>
        /// Returns the MSE (mean squared error) of 2 output files, usually used to to tell the difference between an original and a generated list
        /// </summary>
        /// <param name="File1"></param>
        /// <param name="File2"></param>
        /// <returns>The MSE of 2 output files</returns>
        public static float GetOutputFilesMSE(string File1, string File2)
        {
            string CurrentLine1;
            string CurrentLine2;
            string[] coordinateStrings1;
            string[] coordinateStrings2;

            List<float> diffs = new List<float>();

            StreamReader reader1 = new StreamReader(File1);
            StreamReader reader2 = new StreamReader(File2);
            while ((CurrentLine1 = reader1.ReadLine()) != null && (CurrentLine2 = reader2.ReadLine()) != null)
            {
                // Get the numbers in string format
                coordinateStrings1 = CurrentLine1.Trim().Replace("\t", " ").Replace("  ", " ").Replace("  ", " ").Replace(",", ".").Split(' ');
                coordinateStrings2 = CurrentLine2.Trim().Replace("\t", " ").Replace("  ", " ").Replace("  ", " ").Replace(",", ".").Split(' ');
                for (int outputId = 0; outputId < coordinateStrings1.Length; outputId++)
                {
                    try
                    {
                        // Parse them
                        float num1 = float.Parse(coordinateStrings1[outputId], NumberStyles.Float, CultureInfo.InvariantCulture);
                        float num2 = float.Parse(coordinateStrings2[outputId], NumberStyles.Float, CultureInfo.InvariantCulture);

                        // If only one is NaN return maxvalue (probably the optimization has an invalid combo being tested). If both are NaN continue.
                        if (float.IsNaN(num1))
                            if (float.IsNaN(num2))
                                continue;
                            else
                            {
                                reader1.Close();
                                reader2.Close();
                                return float.MaxValue;
                            }
                        else if (float.IsNaN(num2))
                        {
                            reader1.Close();
                            reader2.Close();
                            return float.MaxValue;
                        }

                        // If neither is a NaN add their squared difference to the list (it will always be positive)
                        diffs.Add((num1 - num2) * (num1 - num2));
                    }
                    catch (Exception exc)
                    {
                        throw exc;
                    }
                }
            }
            reader1.Close();
            reader2.Close();

            if (diffs.Count == 0)
                return float.MaxValue;

            // Return the average
            return diffs.Average();
        }

        // Enum-string conversions

        public static string FuzzyTypeToString(FuzzyType fuzzyType)
        {
            switch (fuzzyType)
            {
                case FuzzyType.Mamdani:
                    return "Mamdani";
                case FuzzyType.Larsen:
                    return "Larsen";
                case FuzzyType.Sugeno:
                    return "Sugeno";
                case FuzzyType.Sparse:
                    return "Sparse";
                default:
                    return "Error: Invalid input";
            }
        }
        public static string AndMethodToString(AndMethodType andMethodType)
        {
            switch (andMethodType)
            {
                case AndMethodType.min:
                    return "min";
                case AndMethodType.max:
                    return "max";
                default:
                    return "Error: Invalid input";
            }
        }
        public static string OrMethodToString(OrMethodType orMethodType)
        {
            switch (orMethodType)
            {
                case OrMethodType.min:
                    return "min";
                case OrMethodType.max:
                    return "max";
                default:
                    return "Error: Invalid input";
            }
        }
        public static string AggMethodToString(AggMethodType aggMethodType)
        {
            switch (aggMethodType)
            {
                case AggMethodType.min:
                    return "min";
                case AggMethodType.max:
                    return "max";
                default:
                    return "Error: Invalid input";
            }
        }
        public static string ImpMethodToString(ImpMethodType impMethodType)
        {
            switch (impMethodType)
            {
                case ImpMethodType.min:
                    return "min";
                case ImpMethodType.max:
                    return "max";
                case ImpMethodType.prod:
                    return "prod";
                default:
                    return "Error: Invalid input";
            }
        }
        public static string DefuzzMethodToString(DefuzzMethodType defuzzMethodType)
        {
            switch (defuzzMethodType)
            {
                case DefuzzMethodType.COA:
                    return "COA";
                case DefuzzMethodType.COG:
                    return "COG";
                case DefuzzMethodType.LOM:
                    return "LOM";
                case DefuzzMethodType.MOM:
                    return "MOM";
                case DefuzzMethodType.wtaver:
                    return "wtaver";
                case DefuzzMethodType.wtsum:
                    return "wtsum";
                case DefuzzMethodType.SOM:
                default:
                    return "SOM";
            }
        }

        public static FuzzyType StringToFuzzyType(string str)
        {
            str = str.ToLower();
            switch (str)
            {
                case "larsen":
                    return FuzzyType.Larsen;
                case "sparse":
                    return FuzzyType.Sparse;
                case "sugeno":
                    return FuzzyType.Sugeno;
                case "mamdani":
                default:
                    return FuzzyType.Mamdani;
            }
        }
        public static AndMethodType StringToAndMethod(string str)
        {
            str = str.ToLower();
            switch (str)
            {
                case "min":
                    return AndMethodType.min;
                case "max":
                default:
                    return AndMethodType.max;
            }
        }
        public static OrMethodType StringToOrMethod(string str)
        {
            str = str.ToLower();
            switch (str)
            {
                case "min":
                    return OrMethodType.min;
                case "max":
                default:
                    return OrMethodType.max;
            }
        }
        public static AggMethodType StringToAggMethod(string str)
        {
            str = str.ToLower();
            switch (str)
            {
                case "min":
                    return AggMethodType.min;
                case "max":
                default:
                    return AggMethodType.max;
            }
        }
        public static ImpMethodType StringToImpMethod(string str)
        {
            str = str.ToLower();
            switch (str)
            {
                case "min":
                    return ImpMethodType.min;
                case "prod":
                    return ImpMethodType.prod;
                case "max":
                default:
                    return ImpMethodType.max;
            }
        }
        public static DefuzzMethodType StringToDefuzzMethod(string str)
        {
            str = str.ToLower();
            switch (str)
            {
                case "coa":
                case "centroid":
                    return DefuzzMethodType.COA;
                case "cog":
                case "bisector":
                    return DefuzzMethodType.COG;
                case "lom":
                    return DefuzzMethodType.LOM;
                case "mom":
                    return DefuzzMethodType.MOM;
                case "wtaver":
                    return DefuzzMethodType.wtaver;
                case "wtsum":
                    return DefuzzMethodType.wtsum;
                case "som":
                default:
                    return DefuzzMethodType.SOM;
            }
        }
    }
}
