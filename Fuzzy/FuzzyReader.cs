using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuzzy
{
    /// <summary>
    /// Used to get a FuzzySystem object from a text file.
    /// </summary>
    public class FuzzyReader
    {
        /// <summary>
        /// Returns a new FuzzySystem of the appropriate sub-type.
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static FuzzySystem GetFuzzySystemFromFile(string FileName)
        {
            // Open the file
            StreamReader file;
            string CurrentLine;
            try
            {
                file = new StreamReader(FileName);
            }
            catch (FileNotFoundException exc)
            {
                throw exc;
            }

            FuzzySystem fuzzy = null;

            // Find the type and create the fuzzy system accordingly
            while ((CurrentLine = file.ReadLine()) != null)
            {
                if (CurrentLine.StartsWith("Type="))
                {
                    FuzzyType type = FuzzySystem.StringToFuzzyType(CurrentLine.Replace("Type=", "").Replace("'", ""));
                    switch (type)
                    {
                        case FuzzyType.Mamdani:
                            fuzzy = new MamdaniFS();
                            break;
                        case FuzzyType.Larsen:
                            fuzzy = new LarsenFS();
                            break;
                        case FuzzyType.Sugeno:
                            fuzzy = new SugenoFS();
                            break;
                        case FuzzyType.Sparse:
                        default:
                            throw new NotImplementedException();
                    }
                file = new StreamReader(FileName);
                break;
                }
            }

            bool FoundSystem = false;
            bool FoundIn = false;
            bool FoundOut = false;
            bool FoundRules = false;
            while ((CurrentLine = file.ReadLine()) != null)
            {
                // When we reach the [System] tag, we read the system setting from the following lines until we hit an empty line or the end of the file
                if (CurrentLine == "[System]")
                {
                    FoundSystem = true;
                    while ((CurrentLine = file.ReadLine()) != null && CurrentLine != "")
                    {
                        if (CurrentLine.StartsWith("Name="))
                            fuzzy.Name = CurrentLine.Replace("Name=", "").Replace("'", "");
                        //else if (CurrentLine.StartsWith("Type="))
                            //fuzzy.Type = StringToFuzzyType(CurrentLine.Replace("Type=", "").Replace("'", ""));
                        else if (CurrentLine.StartsWith("Version="))
                            fuzzy.Version = CurrentLine.Replace("Version=", "");
                        else if (CurrentLine.StartsWith("NumInputs="))
                            fuzzy.NumInputs = int.Parse(CurrentLine.Replace("NumInputs=", ""));
                        else if (CurrentLine.StartsWith("NumOutputs="))
                            fuzzy.NumOutputs = int.Parse(CurrentLine.Replace("NumOutputs=", ""));
                        else if (CurrentLine.StartsWith("NumRules="))
                            fuzzy.NumRules = int.Parse(CurrentLine.Replace("NumRules=", ""));
                        // Method types
                        else if (CurrentLine.StartsWith("AndMethod="))
                            fuzzy.AndMethod = FuzzySystem.StringToAndMethod(CurrentLine.Split('\'')[1]);
                        else if (CurrentLine.StartsWith("OrMethod="))
                            fuzzy.OrMethod = FuzzySystem.StringToOrMethod(CurrentLine.Split('\'')[1]);
                        else if (CurrentLine.StartsWith("ImpMethod="))
                            fuzzy.ImpMethod = FuzzySystem.StringToImpMethod(CurrentLine.Split('\'')[1]);
                        else if (CurrentLine.StartsWith("AggMethod="))
                            fuzzy.AggMethod = FuzzySystem.StringToAggMethod(CurrentLine.Split('\'')[1]);
                        else if (CurrentLine.StartsWith("DefuzzMethod="))
                            fuzzy.DefuzzMethod = FuzzySystem.StringToDefuzzMethod(CurrentLine.Split('\'')[1]);
                    }
                }
                // When we reach an [Input_] tag, we read the setting from the following lines until we hit an empty line or the end of the file
                if (CurrentLine.StartsWith("[Input") && CurrentLine.EndsWith("]"))
                {
                    FoundIn = true;
                    int index = int.Parse(CurrentLine.Replace("[Input", "").Replace("]", "")) - 1;
                    fuzzy.Inputs[index] = new InputOutput
                    {
                        IsInput = true,
                        Index = index + 1
                    };
                    while ((CurrentLine = file.ReadLine()) != null && CurrentLine != "")
                    {
                        if (CurrentLine.StartsWith("Name="))
                            fuzzy.Inputs[index].Name = CurrentLine.Replace("Name=", "").Replace("'", "");
                        else if (CurrentLine.StartsWith("Range="))
                        {
                            fuzzy.Inputs[index].Range[0] = float.Parse(CurrentLine.Replace("Range=[", "").Split(' ')[0], System.Globalization.CultureInfo.InvariantCulture);
                            fuzzy.Inputs[index].Range[1] = float.Parse(CurrentLine.Replace("]", "").Split(' ')[1], System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else if (CurrentLine.StartsWith("NumMFs="))
                            fuzzy.Inputs[index].NumMFs = int.Parse(CurrentLine.Replace("NumMFs=", ""));
                        else if (CurrentLine.StartsWith("MF"))
                            try
                            {
                                fuzzy.Inputs[index].MFs[int.Parse(CurrentLine.Split('=')[0].Remove(0, 2)) - 1] = new MembershipFunction(CurrentLine.Split('=')[1]);
                            }
                            catch (Exception exc)
                            {
                                throw exc;
                            }
                    }
                }
                // When we reach an [Output_] tag, we read the setting from the following lines until we hit an empty line or the end of the file
                if (CurrentLine.StartsWith("[Output") && CurrentLine.EndsWith("]"))
                {
                    FoundOut = true;
                    int index = int.Parse(CurrentLine.Replace("[Output", "").Replace("]", "")) - 1;
                    fuzzy.Outputs[index] = new InputOutput
                    {
                        IsInput = false,
                        Index = index + 1
                    };
                    while ((CurrentLine = file.ReadLine()) != null && CurrentLine != "")
                    {
                        if (CurrentLine.StartsWith("Name="))
                            fuzzy.Outputs[index].Name = CurrentLine.Replace("Name=", "").Replace("'", "");
                        else if (CurrentLine.StartsWith("Range="))
                        {
                            fuzzy.Outputs[index].Range[0] = float.Parse(CurrentLine.Replace("Range=[", "").Split(' ')[0], System.Globalization.CultureInfo.InvariantCulture);
                            fuzzy.Outputs[index].Range[1] = float.Parse(CurrentLine.Replace("]", "").Split(' ')[1], System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else if (CurrentLine.StartsWith("NumMFs="))
                            fuzzy.Outputs[index].NumMFs = int.Parse(CurrentLine.Replace("NumMFs=", ""));
                        else if (CurrentLine.StartsWith("MF"))
                            fuzzy.Outputs[index].MFs[int.Parse(CurrentLine.Split('=')[0].Remove(0, 2)) - 1] = new MembershipFunction(CurrentLine.Split('=')[1]);
                    }
                }
                // When we reach the [Rules] tag, we read the setting from the following lines until we hit an empty line or the end of the file
                if (CurrentLine.StartsWith("[Rules") && CurrentLine.EndsWith("]"))
                {
                    FoundRules = true;
                    int index = 0;
                    while ((CurrentLine = file.ReadLine()) != null && CurrentLine != "")
                    {
                        fuzzy.Rules[index] = new Rule(CurrentLine);
                        index++;
                    }
                }
            }

            // Check if we found all relevant data
            if (!(FoundSystem && FoundIn && FoundOut && FoundRules))
            {
                string missing = "Could not find the following tags:";
                if (!FoundSystem)
                    missing += " [System]";
                if (!FoundIn)
                    missing += " [Input]";
                if (!FoundSystem)
                    missing += " [Output]";
                if (!FoundSystem)
                    missing += " [Rules]";
                throw new Exception(missing);
            }

            if (fuzzy != null)
                return fuzzy;

            throw new Exception("Couldn't read fuzzy system.");
        }
    }
}
