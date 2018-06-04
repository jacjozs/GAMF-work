using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Fuzzy;
using FuzzyManager.Models;
using System.Collections;

namespace FuzzyManager
{
    /// <summary>
    /// Interaction logic for GeneratorWindow.xaml
    /// </summary>
    public partial class GeneratorWindow : Window
    {
        private FuzzySystem fuzzySystem;
        private bool InputLoaded = false;
        private bool OutputLoaded = false;

        private List<InputOutputTemplate> InputTemplates;
        private List<InputOutputTemplate> OutputTemplates;

        private List<List<float>> InputCoords;      //[inputId][coordinateId]
        private List<List<float>> OutputCoords;     //[outputId][coordinateId]

        public GeneratorWindow()
        {
            InitializeComponent();

            TypeBox.ItemsSource = Enum.GetValues(typeof(FuzzyType));
            //TypeBox.ItemsSource = new[] { FuzzyType.Sparse, FuzzyType.Mamdani, FuzzyType.Sugeno };
            TypeBox.SelectedItem = FuzzyType.Mamdani;

            AndBox.ItemsSource = Enum.GetValues(typeof(AndMethodType));
            //AndBox.ItemsSource = new[] {AndMethodType.min, AndMethodType.max};
            AndBox.SelectedItem = AndMethodType.min;

            OrBox.ItemsSource = Enum.GetValues(typeof(OrMethodType));
            //OrBox.ItemsSource = new[] {OrMethodType.min, OrMethodType.max };
            OrBox.SelectedItem = OrMethodType.max;

            ImpBox.ItemsSource = Enum.GetValues(typeof(ImpMethodType));
            //ImpBox.ItemsSource = new[] { ImpMethodType.min, ImpMethodType.max };
            ImpBox.SelectedItem = ImpMethodType.min;

            AggBox.ItemsSource = Enum.GetValues(typeof(AggMethodType));
            //AggBox.ItemsSource = new[] { AggMethodType.min, AggMethodType.max };
            AggBox.SelectedItem = AggMethodType.max;

            DefuzzBox.ItemsSource = Enum.GetValues(typeof(DefuzzMethodType));
            //DefuzzBox.ItemsSource = new[] { DefuzzMethodType.COA, DefuzzMethodType.COG, DefuzzMethodType.LOM, DefuzzMethodType.MOM, DefuzzMethodType.SOM };
            DefuzzBox.SelectedItem = DefuzzMethodType.COG;
        }

        private void LoadInputFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.DefaultExt = ".txt";
            openFileDialog.Filter = "Text file (*.txt)|*.txt";
            openFileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            Nullable<bool> result = openFileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            string fileName = openFileDialog.FileName;
            InputFileTextBox.Text = fileName;
            StreamReader sr = new StreamReader(fileName);
            InDataBox.Text = sr.ReadToEnd();
            sr.Close();
            InputLoaded = true;
            ReadyUp();
        }
        private void LoadOutputFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.DefaultExt = ".txt";
            openFileDialog.Filter = "Text file (*.txt)|*.txt";
            openFileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            bool? result = openFileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            string fileName = openFileDialog.FileName;
            OutputFileTextBox.Text = fileName;
            StreamReader sr = new StreamReader(fileName);
            OutDataBox.Text = sr.ReadToEnd();
            sr.Close();
            OutputLoaded = true;
            ReadyUp();
        }

        private void ReadyUp()
        {
            if (!InputLoaded || !OutputLoaded)
                return;

            // Clear previuos data
            InputCoords = new List<List<float>>();
            OutputCoords = new List<List<float>>();
            InputTemplates = new List<InputOutputTemplate>();
            OutputTemplates = new List<InputOutputTemplate>();
            
            // Load input-output coordinates
            string CurrentLine;
            string[] coordinateStrings;
            bool CoordListsUninitialized = true;

            // Load inputs
            StreamReader reader = new StreamReader(InputFileTextBox.Text);
            while ((CurrentLine = reader.ReadLine()) != null)
            {
                coordinateStrings = CurrentLine.Trim().Replace("\t", " ").Replace("  ", " ").Replace("  ", " ").Replace(",",".").Split(' ');
                if (CoordListsUninitialized)
                {
                    foreach (string unusedString in coordinateStrings)
                    {
                        InputCoords.Add(new List<float>());
                        InputTemplates.Add(new InputOutputTemplate());
                        // Initialize variables
                        InputTemplates[InputTemplates.Count - 1].NumMF = 3;
                        InputTemplates[InputTemplates.Count - 1].Name = "Input" + InputTemplates.Count;
                        InputTemplates[InputTemplates.Count - 1].Id = "I" + (InputTemplates.Count - 1);
                    }
                    CoordListsUninitialized = false;
                }
                for(int inputId = 0; inputId < coordinateStrings.Length; inputId++)
                {
                    try
                    {
                        InputCoords[inputId].Add(float.Parse(coordinateStrings[inputId], NumberStyles.Float, CultureInfo.InvariantCulture));
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK);
                        InputLoaded = false;
                        InputList.ItemsSource = null;
                        OutputList.ItemsSource = null;
                        GenerateButton.IsEnabled = false;
                        return;
                    }
                }
            }
            reader.Close();

            // Load outputs
            CoordListsUninitialized = true;
            reader = new StreamReader(OutputFileTextBox.Text);
            while ((CurrentLine = reader.ReadLine()) != null)
            {
                coordinateStrings = CurrentLine.Trim().Replace("\t", " ").Replace("  ", " ").Replace("  ", " ").Replace(",", ".").Split(' ');
                if (CoordListsUninitialized)
                {
                    foreach (string unusedString in coordinateStrings)
                    {
                        OutputCoords.Add(new List<float>());
                        OutputTemplates.Add(new InputOutputTemplate());
                        // Initialize variables
                        OutputTemplates[OutputTemplates.Count - 1].NumMF = 3;
                        OutputTemplates[OutputTemplates.Count - 1].Name = "Output" + OutputTemplates.Count;
                        OutputTemplates[OutputTemplates.Count - 1].Id = (OutputTemplates.Count - 1).ToString();
                    }
                    CoordListsUninitialized = false;
                }
                for (int inputId = 0; inputId < coordinateStrings.Length; inputId++)
                {
                    try
                    {
                        OutputCoords[inputId].Add(float.Parse(coordinateStrings[inputId], NumberStyles.Float, CultureInfo.InvariantCulture));
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK);
                        OutputLoaded = false;
                        InputList.ItemsSource = null;
                        OutputList.ItemsSource = null;
                        GenerateButton.IsEnabled = false;
                        return;
                    }
                }
            }
            reader.Close();

            // Find minimum and maximum values for each in/output
            for (int i = 0; i < InputTemplates.Count; i++)
            {
                InputTemplates[i].RangeMin = InputCoords[i].Min().ToString().Replace(",",".");
                InputTemplates[i].RangeMax = InputCoords[i].Max().ToString().Replace(",", ".");
            }
            // For the outputs we need to filter out the invalid NaN-s before trying to find the extremes
            for (int o = 0; o < OutputTemplates.Count; o++)
            {
                List<float> validOutput = new List<float>();
                foreach (float f in OutputCoords[o])
                    if (!float.IsNaN(f))
                        validOutput.Add(f);
                OutputTemplates[o].RangeMin = validOutput.Min().ToString(CultureInfo.InvariantCulture);
                OutputTemplates[o].RangeMax = validOutput.Max().ToString(CultureInfo.InvariantCulture);
            }

            InputList.ItemsSource = InputTemplates;
            OutputList.ItemsSource = OutputTemplates;

            InitializeFuzzySystem();

            GenerateButton.IsEnabled = true;
        }

        private void InitializeFuzzySystem()
        {
            switch (FuzzySystem.StringToFuzzyType(TypeBox.Text))
            {
                case FuzzyType.Mamdani:
                    fuzzySystem = new MamdaniFS();
                    break;
                case FuzzyType.Larsen:
                    fuzzySystem = new LarsenFS();
                    break;
                case FuzzyType.Sugeno:
                    fuzzySystem = new SugenoFS();
                    break;
                case FuzzyType.Sparse:
                    break;
                default:
                    break;
            }
            //fuzzySystem = new FuzzySystem();

            // Load fuzzysystem data
            fuzzySystem.Name = NameBox.Text;
            //fuzzySystem.Type = FuzzySystem.StringToFuzzyType(TypeBox.Text);
            fuzzySystem.Version = VersionBox.Text;
            fuzzySystem.AndMethod = FuzzySystem.StringToAndMethod(AndBox.Text);
            fuzzySystem.OrMethod = FuzzySystem.StringToOrMethod(OrBox.Text);
            fuzzySystem.AggMethod = FuzzySystem.StringToAggMethod(AggBox.Text);
            fuzzySystem.ImpMethod = FuzzySystem.StringToImpMethod(ImpBox.Text);
            fuzzySystem.DefuzzMethod = FuzzySystem.StringToDefuzzMethod(DefuzzBox.Text);

            // Set up the data and declare the arrays
            fuzzySystem.NumInputs = InputTemplates.Count;
            for (int i = 0; i < fuzzySystem.NumInputs; i++)
            {
                fuzzySystem.Inputs[i] = new InputOutput
                {
                    Name = InputTemplates[i].Name,
                    IsInput = true,
                    Index = i + 1
                };
            }
            fuzzySystem.NumOutputs = OutputTemplates.Count;
            for (int i = 0; i < fuzzySystem.NumOutputs; i++)
            {
                fuzzySystem.Outputs[i] = new InputOutput
                {
                    Name = OutputTemplates[i].Name,
                    IsInput = false,
                    Index = i + 1
                };
            }
            // Set the ranges and NumMF-s
            for (int inputId = 0; inputId < InputTemplates.Count; inputId++)
            {
                try
                {
                    fuzzySystem.Inputs[inputId].Range = new float[] { float.Parse(InputTemplates[inputId].RangeMin.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(InputTemplates[inputId].RangeMax.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture) };
                }
                catch (Exception exc)
                {
                    if (exc != null)
                        throw exc;

                    throw new Exception("Failed to read range from" + fuzzySystem.Inputs[inputId].Name);
                }
                if (fuzzySystem.Inputs[inputId].Range[0] >= fuzzySystem.Inputs[inputId].Range[1])
                {
                    throw new Exception("Incorrect range for" + fuzzySystem.Inputs[inputId].Name);
                }
                fuzzySystem.Inputs[inputId].NumMFs = InputTemplates[inputId].NumMF;
            }
            for (int outputId = 0; outputId < OutputTemplates.Count; outputId++)
            {
                try
                {
                    fuzzySystem.Outputs[outputId].Range = new float[] { float.Parse(OutputTemplates[outputId].RangeMin.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(OutputTemplates[outputId].RangeMax.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture) };
                }
                catch (Exception exc)
                {
                    if (exc != null)
                        throw exc;

                    throw new Exception("Failed to read range from" + fuzzySystem.Inputs[outputId].Name);
                }
                if (fuzzySystem.Outputs[outputId].Range[0] >= fuzzySystem.Outputs[outputId].Range[1])
                {
                    throw new Exception("Incorrect range for" + fuzzySystem.Inputs[outputId].Name);
                    //MessageBox.Show("Incorrect range for " + fuzzySystem.Outputs[outputId].Name, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                fuzzySystem.Outputs[outputId].NumMFs = OutputTemplates[outputId].NumMF;
            }
            
            fuzzySystem.GenerateTrapezoidMFs(1.0f/3.0f, true, fuzzySystem.Type != FuzzyType.Sugeno);
            //if (fuzzySystem.Type == FuzzyType.Sugeno)
                //fuzzySystem.GenerateConstantOutputMFs();
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {

            InitializeFuzzySystem();

            // Make sure the selected values are compatible
            {
                bool invalid = true;

                if (fuzzySystem.Type == FuzzyType.Mamdani || fuzzySystem.Type == FuzzyType.Larsen)
                    if (fuzzySystem.DefuzzMethod == DefuzzMethodType.COA || fuzzySystem.DefuzzMethod == DefuzzMethodType.COG || fuzzySystem.DefuzzMethod == DefuzzMethodType.MOM || fuzzySystem.DefuzzMethod == DefuzzMethodType.LOM || fuzzySystem.DefuzzMethod == DefuzzMethodType.SOM)
                        if (fuzzySystem.ImpMethod == ImpMethodType.min || fuzzySystem.ImpMethod == ImpMethodType.max)
                            invalid = false;

                if (fuzzySystem.Type == FuzzyType.Sugeno)
                    if (fuzzySystem.DefuzzMethod == DefuzzMethodType.wtaver || fuzzySystem.DefuzzMethod == DefuzzMethodType.wtsum)
                        if (fuzzySystem.ImpMethod == ImpMethodType.prod)
                            invalid = false;

                if (invalid)
                {
                    MessageBox.Show("Incompatible fuzzy system type and defuzzification method.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            var watch = System.Diagnostics.Stopwatch.StartNew();

            List<Rule> rules = new List<Rule>();

            // Generate the rules
            if (fuzzySystem.Type == FuzzyType.Sugeno)
            {
                // Get a list of inputMF-combinations
                int ruleCount = 1;
                for (int inputId = 0; inputId < fuzzySystem.Inputs.Length; inputId++)
                    ruleCount *= fuzzySystem.Inputs[inputId].NumMFs;

                for (int i = 0; i < ruleCount; i++)
                {
                    rules.Add(new Rule());
                    rules[rules.Count - 1].Inputs = new int[fuzzySystem.Inputs.Length];
                    rules[rules.Count - 1].Inputs[0] = -1;  // Uninitialized rule indicator
                    rules[rules.Count - 1].Outputs = new int[fuzzySystem.Outputs.Length];
                    rules[rules.Count - 1].Outputs[0] = i + 1;
                }

                // Prepare outputs
                fuzzySystem.Outputs[0].NumMFs = ruleCount;
                for (int mfId = 0; mfId < fuzzySystem.Outputs[0].NumMFs; mfId++)
                {
                    fuzzySystem.Outputs[0].MFs[mfId] = new MembershipFunction();
                    fuzzySystem.Outputs[0].MFs[mfId].Type = MFtype.constant;
                }

                Random RNG = new Random();
                for (int ruleId = 0; ruleId < rules.Count; ruleId++)
                {
                    bool newRuleFound = true;
                    do
                    {
                        for (int i = 0; i < rules[ruleId].Inputs.Length; i++)
                        {
                            rules[ruleId].Inputs[i] = RNG.Next(fuzzySystem.Inputs[i].NumMFs) + 1;
                        }
                        newRuleFound = true;
                        for (int r = 0; r < ruleId; r++)
                            if (rules[r].SameInput(rules[ruleId]))
                                newRuleFound = false;
                    }
                    while (!newRuleFound);

                    // For each input combination we calculate their weights at each of the given coordinates

                    List<int> itrValues = new List<int>();
                    List<float> weights = new List<float>();

                    for (int itr = 0; itr < InputCoords[0].Count; itr++)
                    {
                        float weight = 1.0f;
                        for (int inputId = 0; inputId < fuzzySystem.NumInputs; inputId++)
                        {
                            weight = Math.Min(weight, fuzzySystem.Inputs[inputId].MFs[rules[ruleId].Inputs[inputId] - 1].GetMembershipValue(InputCoords[inputId][itr]));
                        }

                        // If both weights are >0, we store these weigths (or their min/avg) and the itr value for this combination
                        if (weight > 0.0f)
                        {
                            weights.Add(weight);
                            itrValues.Add(itr);
                        }
                    }

                    // We get the weighed average of the outputs for each combination
                    // Add a new MF to the output with this value, and set the rule to it
                    float result = 0.0f;
                    for (int i = 0; i < weights.Count; i++)
                        result += weights[i] * OutputCoords[0][itrValues[i]];
                    result /= weights.Sum();

                    fuzzySystem.Outputs[0].MFs[ruleId].Params[0] = result;
                }

                // Remove invalid rules/outputs (NaN output -> no matching input-output values were found)
                // Make new lists of the valid entries
                List<Rule> validRules = new List<Rule>();
                List<MembershipFunction> validOutputMFs = new List<MembershipFunction>();
                for (int i = 0; i < ruleCount; i++)
                {
                    if (!float.IsNaN(fuzzySystem.Outputs[0].MFs[i].Params[0]))
                    {
                        validRules.Add(rules[i]);
                        validRules[validRules.Count - 1].Outputs[0] = validRules.Count;
                        validOutputMFs.Add(fuzzySystem.Outputs[0].MFs[i]);
                    }
                }
                // Overwrite original arrays with the new ones
                rules = validRules;
                fuzzySystem.Outputs[0].NumMFs = validOutputMFs.Count;
                fuzzySystem.Outputs[0].MFs = validOutputMFs.ToArray();
            }
            else if (fuzzySystem.Type == FuzzyType.Mamdani || fuzzySystem.Type == FuzzyType.Larsen)
                // for each input-output data we have (rows in the txt)
                for (int itr = 0; itr < InputCoords[0].Count; itr++)
                {
                    // Get the value of each input's MFs at the specified points
                    List<List<float>> MFvalues = new List<List<float>>();   //MFvalues[input id][MF id]
                    // Add a list foreach ioput
                    for (int i = 0; i < fuzzySystem.NumInputs + fuzzySystem.NumOutputs; i++)
                        MFvalues.Add(new List<float>());
                
                    // for each input
                    for (int inputId = 0; inputId < InputCoords.Count; inputId++)
                        // for each MF
                        for (int mfID = 0; mfID < fuzzySystem.Inputs[inputId].NumMFs; mfID++)
                            // Get the membership value at the given coordinate
                            MFvalues[inputId].Add(fuzzySystem.Inputs[inputId].MFs[mfID].GetMembershipValue(InputCoords[inputId][itr]));

                    // for each output
                    for (int outputId = 0; outputId < OutputCoords.Count; outputId++)
                        // for each MF
                        for (int mfID = 0; mfID < fuzzySystem.Outputs[outputId].NumMFs; mfID++)
                            // Get the membership value at the given coordinate
                            MFvalues[fuzzySystem.NumInputs + outputId].Add(fuzzySystem.Outputs[outputId].MFs[mfID].GetMembershipValue(OutputCoords[outputId][itr]));

                    // Check if we have a NaN
                    bool hasNaN = false;
                    foreach (List<float> MFlist in MFvalues)
                        foreach (float f in MFlist)
                            if (float.IsNaN(f))
                                hasNaN = true;
                    if (hasNaN) continue;

                    // Get each input's MF where the membership was the highest (randomly chosen if equal)
                    int[] highestMF = new int[fuzzySystem.NumInputs + fuzzySystem.NumOutputs];
                    for (int k = 0; k < fuzzySystem.NumInputs; k++)
                    {
                        // Find highest membership value
                        float highestVal = MFvalues[k][0];
                        for (int l = 1; l < fuzzySystem.Inputs[k].NumMFs; l++)
                            highestVal = Math.Max(MFvalues[k][l], highestVal);
                        // Find MFs that have the highest membership value we just found
                        List<int> MFsWithHighestVal = new List<int>();
                        for (int l = 0; l < fuzzySystem.Inputs[k].NumMFs; l++)
                            if (MFvalues[k][l] == highestVal)
                                MFsWithHighestVal.Add(l);
                        // Choose a random one of them
                        highestMF[k] = MFsWithHighestVal[(new Random()).Next(MFsWithHighestVal.Count)];
                    }
                    // Get each output's highest MF index
                    for (int k = 0; k < fuzzySystem.NumOutputs; k++)
                    {
                        // Find highest membership value
                        float highestVal = MFvalues[fuzzySystem.NumInputs + k][0];
                        for (int l = 1; l < fuzzySystem.Outputs[k].NumMFs; l++)
                            highestVal = Math.Max(MFvalues[fuzzySystem.NumInputs + k][l], highestVal);
                        // Find MFs that have the highest membership value we just found
                        List<int> MFsWithHighestVal = new List<int>();
                        for (int l = 0; l < fuzzySystem.Outputs[0].NumMFs; l++)
                            if (MFvalues[fuzzySystem.NumInputs + k][l] == highestVal)
                                MFsWithHighestVal.Add(l);
                        // Choose a random one of them
                        highestMF[fuzzySystem.NumInputs + k] = MFsWithHighestVal[(new Random()).Next(MFsWithHighestVal.Count)];
                    }
                    // Create rule
                    Rule newRule = new Rule()
                    {
                        ConnectionType = 1,
                        Inputs = new int[fuzzySystem.NumInputs],
                        Outputs = new int[fuzzySystem.NumOutputs],
                        Weight = 1.0f
                    };
                    for (int i = 0; i < fuzzySystem.NumInputs; i++)
                    {
                        newRule.Inputs[i] = highestMF[i] + 1;
                    }
                    for (int i = 0; i < fuzzySystem.NumOutputs; i++)
                    {
                        newRule.Outputs[i] = highestMF[fuzzySystem.NumInputs + i] + 1;
                    }

                    // Add to rule list if the inputs don't match any existing rule
                    bool isRuleNew = true;
                    foreach (Rule rule in rules)
                        if (rule.SameInput(newRule))
                        {
                            isRuleNew = false;
                            break;
                        }
                    if (isRuleNew)
                        rules.Add(newRule);
                }

            foreach (Rule rule in rules)
            {
                rule.ConnectionType = 1;
                rule.Weight = 1.0f;
            }
            fuzzySystem.Rules = rules.ToArray();

            watch.Stop();

            GeneratedFuzzyBox.Text = fuzzySystem.ToString(IncludeValuesCheckBox.IsChecked == true);

            SaveButton.IsEnabled = true;
            OptimizeButton.IsEnabled = true;
            UseButton.IsEnabled = true;

            float MSE;

            try
            {
                fuzzySystem.LoadInputsAndSaveOutputs(InputFileTextBox.Text, OutputFileTextBox.Text.Replace(".txt", ".generated.txt"));
                MSE = FuzzySystem.GetOutputFilesMSE(OutputFileTextBox.Text, OutputFileTextBox.Text.Replace(".txt", ".generated.txt"));
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK);
                return;
            }
            
            MessageBox.Show("MSE: " + MSE.ToString(CultureInfo.InvariantCulture) + "\r\nRMSE: " + Math.Sqrt(MSE).ToString(CultureInfo.InvariantCulture) + "\r\nRMSEP: " + (Math.Sqrt(MSE)/(fuzzySystem.Outputs[0].Range[1] - fuzzySystem.Outputs[0].Range[0]) * 100.0f).ToString(CultureInfo.InvariantCulture) + "%\r\nGeneration time in milliseconds: " + watch.ElapsedMilliseconds, "Generation successful!", MessageBoxButton.OK);
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveButton.IsEnabled != true)
                try
                {
                    InitializeFuzzySystem();
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            bool isInput = false;
            int id = 0;
            // Load data from the id tag
            {
                string IdString = ((Button)sender).Tag.ToString();
                if (IdString.StartsWith("I"))
                {
                    isInput = true;
                    IdString = IdString.Replace("I", "");
                }
                id = int.Parse(IdString);
            }
            MFsPreviewWindow prevWindow;
            if (isInput)
                prevWindow = new MFsPreviewWindow(fuzzySystem.Inputs[id]);
            else
            {
                if (fuzzySystem.Outputs[id].MFs.All(x => x != null))
                    prevWindow = new MFsPreviewWindow(fuzzySystem.Outputs[id]);
                else
                {
                    MessageBox.Show("Output could not be previewed.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            prevWindow.Show();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            string name = "GeneratedFuzzySystem";
            if (fuzzySystem.Name != "")
                name = fuzzySystem.Name;

            if (IncludeValuesCheckBox.IsChecked == true)
            {
                saveFileDialog.DefaultExt = ".fisx";
                saveFileDialog.Filter = "Text file (*.fisx)|*.fisx";
                saveFileDialog.FileName = name + ".fisx";
            }
            else
            {
                saveFileDialog.DefaultExt = ".fis";
                saveFileDialog.Filter = "Text file (*.fis)|*.fis";
                saveFileDialog.FileName = name + ".fis";
            }
            saveFileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            Nullable<bool> result = saveFileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            SaveFuzzySystem(saveFileDialog.FileName);
        }
        public void SaveFuzzySystem(string FileName)
        {
            StreamWriter writer = new StreamWriter(FileName);
            writer.Write(GeneratedFuzzyBox.Text);
            writer.Close();
        }

        private void IncludeValues_Checked(object sender, RoutedEventArgs e)
        {
            if (fuzzySystem == null)
                return;

            GeneratedFuzzyBox.Text = fuzzySystem.ToString(true);
        }
        private void IncludeValues_Unchecked(object sender, RoutedEventArgs e)
        {
            if (fuzzySystem == null)
                return;

            GeneratedFuzzyBox.Text = fuzzySystem.ToString(false);
        }

        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If Mamdani/Larsen selected
            if ((FuzzyType)e.AddedItems[0] == FuzzyType.Mamdani || (FuzzyType)e.AddedItems[0] == FuzzyType.Larsen)
            {
                if (FuzzySystem.StringToImpMethod(ImpBox.Text) == ImpMethodType.prod)
                    ImpBox.SelectedItem = ImpMethodType.min;
                if (FuzzySystem.StringToDefuzzMethod(DefuzzBox.Text) == DefuzzMethodType.wtaver || FuzzySystem.StringToDefuzzMethod(DefuzzBox.Text) == DefuzzMethodType.wtsum)
                    DefuzzBox.SelectedItem = DefuzzMethodType.COG;
            }

            // If Sugeno selected
            if ((FuzzyType)e.AddedItems[0] == FuzzyType.Sugeno)
            {
                if (FuzzySystem.StringToImpMethod(ImpBox.Text) == ImpMethodType.min || FuzzySystem.StringToImpMethod(ImpBox.Text) == ImpMethodType.max)
                    ImpBox.SelectedItem = ImpMethodType.prod;
                if (FuzzySystem.StringToDefuzzMethod(DefuzzBox.Text) == DefuzzMethodType.COA || FuzzySystem.StringToDefuzzMethod(DefuzzBox.Text) == DefuzzMethodType.COG || FuzzySystem.StringToDefuzzMethod(DefuzzBox.Text) == DefuzzMethodType.MOM || FuzzySystem.StringToDefuzzMethod(DefuzzBox.Text) == DefuzzMethodType.LOM || FuzzySystem.StringToDefuzzMethod(DefuzzBox.Text) == DefuzzMethodType.SOM)
                    DefuzzBox.SelectedItem = DefuzzMethodType.wtaver;
            }
        }

        private void OptimizeButton_Click(object sender, RoutedEventArgs e)
        {
            OptimizerWindow optimizerWindow = new OptimizerWindow();
            optimizerWindow.Width = Width;
            optimizerWindow.Height = Height;
            optimizerWindow.Left = Left;
            optimizerWindow.Top = Top;
            optimizerWindow.WindowState = WindowState;
            optimizerWindow.Show();
            optimizerWindow.LoadInputFile(InputFileTextBox.Text);
            optimizerWindow.LoadOutputFile(OutputFileTextBox.Text);
            SaveFuzzySystem("TemporaryFuzzySystemFile");
            optimizerWindow.LoadFuzzyFile("TemporaryFuzzySystemFile");
            Close();
        }

        private void UseButton_Click(object sender, RoutedEventArgs e)
        {
            TestWindow testWindow = new TestWindow();
            testWindow.Top = Top + (Height - testWindow.Height) / 2;
            testWindow.Left = Left + (Width - testWindow.Width) / 2;
            App.MoveWindowInsideBounds(testWindow);
            testWindow.Show();
            testWindow.InputValuesText.Text = InputFileTextBox.Text;
            testWindow.OutputValuesText.Text = OutputFileTextBox.Text;
            SaveFuzzySystem("TemporaryFuzzySystemFile");
            testWindow.FuzzySystemText.Text = "TemporaryFuzzySystemFile";
            Close();
        }
    }
}
