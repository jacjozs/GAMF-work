using Fuzzy;
using Optimization;
using System;
using System.Collections;
using System.Collections.Generic;
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

namespace FuzzyManager
{
    /// <summary>
    /// Interaction logic for OptimizerWindow.xaml
    /// </summary>
    public partial class OptimizerWindow : Window
    {
        private bool UpdatingParamTextBoxes = false;

        private FuzzySystem fuzzySystem;
        private bool InputLoaded = false;
        private bool OutputLoaded = false;

        private OptimizationProgressWindow ProgressWindow;
        private List<MFsPreviewWindow> IoputMfWindows;
        private IOptimizationMethod Optimizer;
        /// <summary>
        /// Number of antibodies
        /// </summary>
        private int NA;
        /// <summary>
        /// An identifier for the selected optimization method
        /// </summary>
        private int MethodId;
        /// <summary>
        /// The type of condition responsible for stopping the optimization
        /// </summary>
        public StoppingType StopType;
        /// <summary>
		/// Allowed number of generations.
		/// </summary>
		public long StopGenerationNumber { get; set; }
        /// <summary>
        /// Allowed number of affinity evaluations.
        /// </summary>
        public long StopAffinityEvaluation { get; set; }
        /// <summary>
        /// Affinity (fitness) treshold.
        /// </summary>
        public double StopFitnessTreshold { get; set; }

        // Clonal generation parameters
        /// <summary>
		/// Clone number coefficient. ( ]0,1] )
		/// </summary>
		private double beta;
        /// <summary>
        /// Mutation coefficient. ( ]0,1] )
        /// </summary>
        private double ro;
        /// <summary>
        /// The number of new antibodies created at the end of each generation.
        /// </summary>
        private int d;
        /// <summary>
        /// Number of antiodies selected for cloning.
        /// </summary>
        private int cgn;
        /// <summary>
        /// Step size relative to the entire search area
        /// </summary>
        private double cglssr;
        /// <summary>
        /// Local searches per generation
        /// </summary>
        private int cgln;

        // Firework parameters
        private int m;
        private double a;
        private double b;
        private double Amax;
        private int mhat;

        // Genetic algorithm parameters
        private int P;
        private double pm;
        private int crossoverCount;

        // Particle swarm parameters
        private double cp;
        private double c0;
        private double cg;
        
        /// <summary>
		/// Aggregates the members of the list into a string separated by commas.
		/// </summary>
		/// <param name="AL">List</param>
		/// <returns>Aggregated string.</returns>
		public string List(ArrayList AL)
        {
            var s = "";
            for (var i = 0; i < AL.Count; i++)
            {
                s = s + $"{AL[i],8:F2}";
                if (i != AL.Count - 1)
                    s = s + "; ";
            }
            return s;
        }
        
        private FuzzySystem GetModifiedFuzzySystem(ArrayList ActualParameters)
        {
            // Clone fuzzysystem
            FuzzySystem tempFuz = (FuzzySystem)Activator.CreateInstance(fuzzySystem.GetType());
            tempFuz.CopyDataFrom(fuzzySystem);
            /*FuzzySystem tempFuz = null;
            switch (fuzzySystem.Type)
            {
                case FuzzyType.Mamdani:
                    tempFuz = new MamdaniFS();
                    break;
                case FuzzyType.Larsen:
                    tempFuz = new LarsenFS();
                    break;
                case FuzzyType.Sugeno:
                    tempFuz = new SugenoFS();
                    break;
                case FuzzyType.Sparse:
                default:
                    throw new NotImplementedException();
                    break;
            }*/

            // Read settings
            bool In = false;
            bool Out = false;
            bool RP = false;
            Dispatcher.Invoke(() =>
            {
                In = CheckInput.IsChecked == true;
                Out = CheckOutput.IsChecked == true;
                RP = RadioRefPoint.IsChecked == true;
            });

            // Store the number of MFs handled by the optimization
            int finishedMfCount = 0;

            if (In)
                for (int inputId = 0; inputId < tempFuz.Inputs.Length; inputId++)
                {
                    for (int mfId = 0; mfId < tempFuz.Inputs[inputId].MFs.Length; mfId++)
                        if (RP)
                            tempFuz.Inputs[inputId].MFs[mfId] = tempFuz.Inputs[inputId].MFs[mfId].GetAtRefPoint(
                                    (float)((double)ActualParameters[finishedMfCount + mfId]));
                        else
                            tempFuz.Inputs[inputId].MFs[mfId] = MembershipFunction.CreateTrapezoid(
                                tempFuz.Inputs[inputId].MFs[mfId].GetTrapReferencePoint(),
                                tempFuz.Inputs[inputId].MFs[mfId].GetTrapBottomBase(),
                                tempFuz.Inputs[inputId].MFs[mfId].GetTrapBottomBase() * (float)((double)ActualParameters[finishedMfCount + mfId]));
                    finishedMfCount += tempFuz.Inputs[inputId].NumMFs;
                }
            if (Out)
                for (int outputId = 0; outputId < tempFuz.Outputs.Length; outputId++)
                {
                    for (int mfId = 0; mfId < tempFuz.Outputs[outputId].MFs.Length; mfId++)
                        if (RP)
                            tempFuz.Outputs[outputId].MFs[mfId] = tempFuz.Outputs[outputId].MFs[mfId].GetAtRefPoint(
                                    (float)((double)ActualParameters[finishedMfCount + mfId]));
                        else
                            tempFuz.Outputs[outputId].MFs[mfId] = MembershipFunction.CreateTrapezoid(
                                tempFuz.Outputs[outputId].MFs[mfId].GetTrapReferencePoint(),
                                tempFuz.Outputs[outputId].MFs[mfId].GetTrapBottomBase(),
                                tempFuz.Outputs[outputId].MFs[mfId].GetTrapBottomBase() * (float)((double)ActualParameters[finishedMfCount + mfId]));
                    finishedMfCount += tempFuz.Outputs[outputId].NumMFs;
                }

            return tempFuz;
        }

        private double FitnessFunctionMSE(ArrayList ActualParameters)
        {
            FuzzySystem tempFuz = GetModifiedFuzzySystem(ActualParameters);

            float MSE = 0.0f;
            bool readFailed = false;

            Dispatcher.Invoke(() =>
            {
                try
                {
                    tempFuz.LoadInputsAndSaveOutputs(InputFileTextBox.Text, OutputFileTextBox.Text.Replace(".txt", ".generated.txt"));
                    MSE = FuzzySystem.GetOutputFilesMSE(OutputFileTextBox.Text, OutputFileTextBox.Text.Replace(".txt", ".generated.txt"));
                }
                catch (Exception exc)
                {
                    readFailed = true;
                    Optimizer.Stop = true;
                    MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK);
                }
            });

            if (readFailed)
                return double.NaN;

            //return (double)Math.Sqrt((double)MSE);
            return (double)MSE;
        }

        public OptimizerWindow()
        {
            InitializeComponent();

            // Number of antibodies.
            NA = 20;
            MethodId = 0;
            // Stopping
            StopType = StoppingType.GenerationNumber;
            StopGenerationNumber = 100;
            StopAffinityEvaluation = 2500;
            StopFitnessTreshold = 0.0001;
            //clonal generation params
            beta = 0.5;
            ro = 0.5;
            d = NA / 5;
            cgn = NA / 3;
            cglssr = 0.005;
            cgln = 5;
            // Firework params
            m = 50;
            a = 0.04;
            b = 0.8;
            Amax = 40;
            mhat = NA;
            // Particle swarm parrams
            c0 = 0.8;
            cp = 0.2;
            cg = 0.2;
            // Genetic algorithm params
            P = NA / 2;
            pm = 0.6;
            crossoverCount = NA;

            RadioBaseRatio.IsChecked = true;
            CheckOutput.IsChecked = true;
            cbiFirework.IsSelected = true;
            tbParamN.Text = NA.ToString();

            cbStopType.ItemsSource = Enum.GetValues(typeof(StoppingType));
            cbStopType.SelectedIndex = (int)StopType;
            tbStopGen.Text = StopGenerationNumber.ToString();
            tbStopEval.Text = StopAffinityEvaluation.ToString();
            tbStopFt.Text = StopFitnessTreshold.ToString();
        }

        private void CheckPreparation(object sender, RoutedEventArgs e)
        {
            OptimizeButton.IsEnabled = (
                (CheckInput.IsChecked == true || CheckOutput.IsChecked == true) &&
                InputLoaded &&
                OutputLoaded &&
                fuzzySystem != null
            );
        }

        private void RadioRefPoint_Checked(object sender, RoutedEventArgs e)
        {
            RadioBaseRatio.IsChecked = false;
        }
        private void RadioBaseRatio_Checked(object sender, RoutedEventArgs e)
        {
            RadioRefPoint.IsChecked = false;
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

            LoadInputFile(openFileDialog.FileName);
        }
        public void LoadInputFile(string FileName)
        {
            InputFileTextBox.Text = FileName;
            StreamReader sr = new StreamReader(FileName);
            InDataBox.Text = sr.ReadToEnd();
            sr.Close();
            InputLoaded = true;
            CheckPreparation(null, null);
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

            LoadOutputFile(openFileDialog.FileName);
        }
        public void LoadOutputFile(string FileName)
        {
            OutputFileTextBox.Text = FileName;
            StreamReader sr = new StreamReader(FileName);
            OutDataBox.Text = sr.ReadToEnd();
            sr.Close();
            OutputLoaded = true;
            CheckPreparation(null, null);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.DefaultExt = ".txt";
            openFileDialog.Filter = "Text file (*.txt, *.fis, *.fisx)|*.txt;*.fis;*.fisx";
            openFileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            Nullable<bool> result = openFileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            LoadFuzzyFile(openFileDialog.FileName);
        }
        public void LoadFuzzyFile(string FileName)
        {
            StreamReader reader = new StreamReader(FileName);
            FuzzyBox.Text = reader.ReadToEnd();
            reader.Close();

            SaveButton.IsEnabled = true;
            UseButton.IsEnabled = true;

            //fuzzySystem = new FuzzySystem();
            try
            {
                fuzzySystem = FuzzyReader.GetFuzzySystemFromFile(FileName);
            }
            catch (Exception exc)
            {
                fuzzySystem = null;
                MessageBox.Show("Failed to load fuzzy system.\r\n" + (exc != null ? exc.Message : ""), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            StringReader strRead = new StringReader(FuzzyBox.Text);
            string str = "";
            while ((str = strRead.ReadLine()) != null)
            {
                if (str.StartsWith("MF"))
                {
                    if (str.Contains("!"))
                        IncludeValuesCheckBox.IsChecked = true;
                    else
                        IncludeValuesCheckBox.IsChecked = false;
                    break;
                }
            }
            strRead.Close();

            CheckPreparation(null, null);
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = "OptimizedFuzzySystem";
            if (fuzzySystem.Name != "")
                name = fuzzySystem.Name;

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            if (IncludeValuesCheckBox.IsChecked == true)
            {
                saveFileDialog.DefaultExt = ".fisx";
                saveFileDialog.Filter = "Text file (*.fisx)|*.fisx";
                saveFileDialog.FileName = name + "_optimized.fisx";
            }
            else
            {
                saveFileDialog.DefaultExt = ".fis";
                saveFileDialog.Filter = "Text file (*.fis)|*.fis";
                saveFileDialog.FileName = name + "_optimized.fis";
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
            writer.Write(FuzzyBox.Text);
            writer.Close();
        }

        private async void OptimizeButton_Click(object sender, RoutedEventArgs e)
        {
            /*if (fuzzySystem.Type == FuzzyType.Sugeno && CheckOutput.IsChecked == true)
            {
                MessageBox.Show("Output optimization is not valid for Sugeno-type fuzzy systems.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }*/

            // Disable the optimize button and the optimization parameters
            OptimizeButton.IsEnabled = false;
            UseButton.IsEnabled = false;
            RadioBaseRatio.IsEnabled = false;
            RadioRefPoint.IsEnabled = false;
            CheckInput.IsEnabled = false;
            CheckOutput.IsEnabled = false;
            // Initialize variables
            ArrayList initParams = new ArrayList();
            ArrayList lbp = new ArrayList();
            ArrayList ubp = new ArrayList();
            
            // Set up initial parameters
            if (CheckInput.IsChecked == true)
                foreach (InputOutput input in fuzzySystem.Inputs)
                    foreach (MembershipFunction MF in input.MFs)
                    {
                        if (RadioRefPoint.IsChecked == true)
                        {
                            initParams.Add((double)(MF.GetReferencePoint()));
                            lbp.Add((double)(input.Range[0]));
                            ubp.Add((double)(input.Range[1]));
                        }
                        if (RadioBaseRatio.IsChecked == true)
                        {
                            initParams.Add((double)(MF.GetTrapBaseRatio()));
                            lbp.Add(0.0);
                            ubp.Add(0.95);
                        }
                    }
            if (CheckOutput.IsChecked == true)
                foreach (InputOutput output in fuzzySystem.Outputs)
                    foreach (MembershipFunction MF in output.MFs)
                    {
                        if (RadioRefPoint.IsChecked == true)
                        {
                            initParams.Add((double)(MF.GetReferencePoint()));
                            lbp.Add((double)(output.Range[0]));
                            ubp.Add((double)(output.Range[1]));
                        }
                        if (RadioBaseRatio.IsChecked == true)
                        {
                            initParams.Add((double)(MF.GetTrapBaseRatio()));
                            lbp.Add(0.0);
                            ubp.Add(0.95);
                        }
                    }

            bool[] ints = new bool[initParams.Count];

            // Set up the optimizer object depending on the settings
            switch (MethodId)
            {
                case 0: //Firework
                    Optimizer = new Firework
                    {
                        // Number of particles in the swarm.
                        NumberOfElements = NA,
                        m = m,
                        ymax = double.MaxValue,
                        a = a,
                        b = b,
                        Amax = Amax,
                        mhat = mhat,
                        Slow = false
                    };
                    break;
                case 1: //Particle Swarm
                    Optimizer = new ParticleSwarm
                    {
                        // Number of particles in the swarm.
                        NumberOfElements = NA,
                        c0 = c0,
                        // Multiplication factor for the distance to the personal best position.
                        cp = cp,
                        // Multiplication factor for the distance to the global best position.
                        cg = cg,
                        Slow = false
                    };
                    break;
                case 2: //Clonal generation
                    if (cgn > NA)
                    {
                        MessageBox.Show("Number of antibodies selected for cloning at the end of each generation greater than the number of antibodies.", "ERROR", MessageBoxButton.OK);
                        OptimizeButton.IsEnabled = true;
                        return;
                    }
                    Optimizer = new ClonalGeneration
                    {
                        // Size of the antibody pool.
                        NumberOfElements = NA,
                        // Number of antibodies selected for cloning.
                        NumberSelectedForCloning = cgn,
                        // Parameter determining the number of clones created for an antibody that was selected for cloning. (0,1]
                        Beta = beta,
                        // Number of antibodies created with random parameters in each new generation. 
                        RandomAntibodiesPerGeneration = d,
                        // Mutation coefficient (0,1].
                        Ro = ro,
                        Slow = false
                    };
                    break;
                case 3:
                    Optimizer = new GeneticAlgorithm
                    {
                        // Size of the individual pool.
                        NumberOfElements = NA,
                        // Number of parents in each generation.
                        ParentsInEachGeneration = P,
                        // The probability of mutation.
                        MutationProbability = pm,
                        // The number of crossovers in each generation.
                        CrossoverPerGeneration = crossoverCount,
                        Slow = false
                    };
                    break;
                case 4: //Clonal generation
                    if (cgn > NA)
                    {
                        MessageBox.Show("Number of antibodies selected for cloning at the end of each generation greater than the number of antibodies.", "ERROR", MessageBoxButton.OK);
                        OptimizeButton.IsEnabled = true;
                        return;
                    }
                    Optimizer = new ClonalGenerationLocal
                    {
                        // Size of the antibody pool.
                        NumberOfElements = NA,
                        // Number of antibodies selected for cloning.
                        NumberSelectedForCloning = cgn,
                        // Parameter determining the number of clones created for an antibody that was selected for cloning. (0,1]
                        Beta = beta,
                        // Number of antibodies created with random parameters in each new generation. 
                        RandomAntibodiesPerGeneration = d,
                        // Mutation coefficient (0,1].
                        Ro = ro,
                        // Local searches per generation
                        LocalSearchesPerGeneration = cgln,
                        Slow = false
                    };
                    break;
            }

            // Set common parameters
            Optimizer.InitialParameters = initParams;
            Optimizer.LowerParamBounds = lbp;
            Optimizer.UpperParamBounds = ubp;
            // Bounds are required before setting this value
            if (MethodId == 4)
                ((ClonalGenerationLocal)Optimizer).StepSizeRelative = cglssr;
            Optimizer.Integer = ints;
            Optimizer.FitnessFunction = FitnessFunctionMSE;
            Optimizer.StoppingNumberOfGenerations = StopGenerationNumber;
            Optimizer.StoppingNumberOfEvaluations = StopAffinityEvaluation;
            Optimizer.StoppingFitnessTreshold = StopFitnessTreshold;
            Optimizer.StoppingType = StopType;

            // Output string
            string outstr = "Initial parameters: " + List(initParams) + "\r\n";

            // Open progress window and begin the optimization
            ProgressWindow = new OptimizationProgressWindow();
            ProgressWindow.OptWindow = this;
            ProgressWindow.Show();
            if (IoputMfWindows != null)
                foreach (MFsPreviewWindow window in IoputMfWindows)
                    window.Close();
            IoputMfWindows = new List<MFsPreviewWindow>();
            foreach (InputOutput input in fuzzySystem.Inputs)
            {
                IoputMfWindows.Add(new MFsPreviewWindow(input));
                IoputMfWindows[IoputMfWindows.Count - 1].Show();
            }
            foreach (InputOutput output in fuzzySystem.Outputs)
            {
                IoputMfWindows.Add(new MFsPreviewWindow(output));
                IoputMfWindows[IoputMfWindows.Count - 1].Show();
            }
            Optimizer.GenerationCreated += OnNewGeneration;
            // <DELAYER>
            /*var z = Task.Run(() => (delayer()));
            int y = await z;*/
            // </DELAYER>
            var x = Task.Run(() => (Optimizer.Optimize()));
            var Results = await x;
            ProgressWindow.Close();

            // Add results to output string
            outstr += "Initial MSE: " + Results.InfoList[0] + "\r\n" +
                "Final parameters: " + List(Results.OptimizedParameters) + "\r\n" +
                "Final MSE: " + $"{Results.InfoList[1],10:F6}" + "\r\n" +
                "Number of generations: " + Results.InfoList[2] + "\r\n" +
                "Number of fitness evaluations: " + Results.InfoList[3] + "\r\n" +
                "Best affinities in each generation: " + List((ArrayList)Results.InfoList[4]) + "\r\n" +
                "Optimization time in milliseconds: " + Results.InfoList[5];

            fuzzySystem = GetModifiedFuzzySystem(Results.OptimizedParameters);
            FuzzyBox.Text = fuzzySystem.ToString();

            // Display results and reenable the optimize button
            MessageBox.Show(outstr, "Optimization finished", MessageBoxButton.OK);
            OptimizeButton.IsEnabled = true;
            UseButton.IsEnabled = true;
            RadioBaseRatio.IsEnabled = true;
            RadioRefPoint.IsEnabled = true;
            CheckInput.IsEnabled = true;
            CheckOutput.IsEnabled = true;
        }

        // <DELAYER>
        /*private int delayer()
        {
            System.Threading.Thread.Sleep(15000);
            return 0;
        }*/
        // </DELAYER>

        private void OnNewGeneration(object sender, ArrayList Antibodies, double[] affinities)
        {
            // Update output window text
            string text = "";
            for (int i = 0; i < Antibodies.Count; i++)
            {
                text += ("Parameters: " + List(((IElement)Antibodies[i]).Position) + " \tFitness: " + affinities[i].ToString() + "\r\n");
            }
            ProgressWindow.UpdateText(text);
            ProgressWindow.UpdateImage(affinities[0]);

            // Update graphical windows
            FuzzySystem tempFuz = GetModifiedFuzzySystem(((IElement)Antibodies[0]).Position);
            for (int i = 0; i < tempFuz.Inputs.Length; i++)
                IoputMfWindows[i].ShowNewIoput(tempFuz.Inputs[i]);
            for (int i = 0; i < tempFuz.Outputs.Length; i++)
                IoputMfWindows[tempFuz.Inputs.Length + i].ShowNewIoput(tempFuz.Outputs[i]);

            float RMSEP = (float)affinities[0] / (tempFuz.Outputs[0].Range[1] - tempFuz.Outputs[0].Range[0]) * 100.0f;
        }

        public void StopOptimizing()
        {
            Optimizer.Stop = true;
        }

        private void SetVisibleParamCount(int count)
        {
            if (count < 6)
                spParam5.Visibility = Visibility.Collapsed;
            else
                spParam5.Visibility = Visibility.Visible;

            if (count < 5)
                spParam4.Visibility = Visibility.Collapsed;
            else
                spParam4.Visibility = Visibility.Visible;

            if (count < 4)
                spParam3.Visibility = Visibility.Collapsed;
            else
                spParam3.Visibility = Visibility.Visible;

            if (count < 3)
                spParam2.Visibility = Visibility.Collapsed;
            else
                spParam2.Visibility = Visibility.Visible;

            if (count < 2)
                spParam1.Visibility = Visibility.Collapsed;
            else
                spParam1.Visibility = Visibility.Visible;

            if (count < 1)
                spParam0.Visibility = Visibility.Collapsed;
            else
                spParam0.Visibility = Visibility.Visible;
        }

        private void UpdateMethodParams()
        {
            UpdatingParamTextBoxes = true;

            switch (MethodId)
            {
                case 0: // Firework
                    lbParam0.Content = "m?";
                    tbParam0.Text = m.ToString();
                    lbParam1.Content = "a?";
                    tbParam1.Text = a.ToString();
                    lbParam2.Content = "b?";
                    tbParam2.Text = b.ToString();
                    lbParam3.Content = "Maximum explosion amplitude";
                    tbParam3.Text = Amax.ToString();
                    lbParam4.Content = "mhat?";
                    tbParam4.Text = mhat.ToString();
                    SetVisibleParamCount(5);
                    break;
                case 1: // Particle swarm
                    lbParam0.Content = "Coefficient for the distance to the previous velocity value";
                    tbParam0.Text = c0.ToString();
                    lbParam1.Content = "Coefficient for the distance to the personal best position";
                    tbParam1.Text = cp.ToString();
                    lbParam2.Content = "Coefficient for the distance to the global best position";
                    tbParam2.Text = cg.ToString();
                    SetVisibleParamCount(3);
                    break;
                case 2: // Clonal generation
                    lbParam0.Content = "Clone number coefficient";
                    tbParam0.Text = beta.ToString();
                    lbParam1.Content = "Mutation coefficient";
                    tbParam1.Text = ro.ToString();
                    lbParam2.Content = "New antibodies at the end of each generation";
                    tbParam2.Text = d.ToString();
                    lbParam3.Content = "Number of antibodies selected for cloning";
                    tbParam3.Text = cgn.ToString();
                    SetVisibleParamCount(4);
                    break;
                case 3: // Genetic algorithm
                    lbParam0.Content = "Number of parents in each generation";
                    tbParam0.Text = P.ToString();
                    lbParam1.Content = "The probability of mutation";
                    tbParam1.Text = pm.ToString();
                    lbParam2.Content = "The number of crossovers in each generation";
                    tbParam2.Text = crossoverCount.ToString();
                    SetVisibleParamCount(3);
                    break;
                case 4: // Clonal generation local
                    lbParam0.Content = "Clone number coefficient";
                    tbParam0.Text = beta.ToString();
                    lbParam1.Content = "Mutation coefficient";
                    tbParam1.Text = ro.ToString();
                    lbParam2.Content = "New antibodies at the end of each generation";
                    tbParam2.Text = d.ToString();
                    lbParam3.Content = "Number of antibodies selected for cloning";
                    tbParam3.Text = cgn.ToString();
                    lbParam4.Content = "Local search space size relative to the entire search area";
                    tbParam4.Text = cglssr.ToString();
                    lbParam5.Content = "Local searches per generation";
                    tbParam5.Text = cgln.ToString();
                    SetVisibleParamCount(6);
                    break;
            }

            UpdatingParamTextBoxes = false;
        }

        private void cbiFirework_Selected(object sender, RoutedEventArgs e)
        {
            MethodId = 0;
            UpdateMethodParams();
        }

        private void cbiParticleSwarm_Selected(object sender, RoutedEventArgs e)
        {
            MethodId = 1;
            UpdateMethodParams();
        }

        private void cbiClonalGen_Selected(object sender, RoutedEventArgs e)
        {
            MethodId = 2;
            UpdateMethodParams();
        }

        private void cbiClonalGenLoc_Selected(object sender, RoutedEventArgs e)
        {
            MethodId = 4;
            UpdateMethodParams();
        }

        private void cbiGeneticAlg_Selected(object sender, RoutedEventArgs e)
        {
            MethodId = 3;
            UpdateMethodParams();
        }

        private void optParam_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UpdatingParamTextBoxes)
                return;

            try
            {
                NA = int.Parse(tbParamN.Text);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            switch (MethodId)
            {
                case 0: // Firework
                    try
                    {
                        m = int.Parse(tbParam0.Text);
                        a = double.Parse(tbParam1.Text);
                        b = double.Parse(tbParam2.Text);
                        Amax = double.Parse(tbParam3.Text);
                        mhat = int.Parse(tbParam4.Text);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case 1: // PSO
                    try
                    {
                        c0 = double.Parse(tbParam0.Text);
                        cp = double.Parse(tbParam1.Text);
                        cg = double.Parse(tbParam2.Text);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case 2: // CGA
                    try
                    {
                        beta = double.Parse(tbParam0.Text);
                        ro = double.Parse(tbParam1.Text);
                        d = int.Parse(tbParam2.Text);
                        cgn = int.Parse(tbParam3.Text);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case 3: // GA
                    try
                    {
                        P = int.Parse(tbParam0.Text);
                        pm = double.Parse(tbParam1.Text);
                        crossoverCount = int.Parse(tbParam2.Text);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case 4: // CGA
                    try
                    {
                        beta = double.Parse(tbParam0.Text);
                        ro = double.Parse(tbParam1.Text);
                        d = int.Parse(tbParam2.Text);
                        cgn = int.Parse(tbParam3.Text);
                        cglssr = double.Parse(tbParam4.Text);
                        cgln = int.Parse(tbParam5.Text);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
            }
        }

        private void IncludeValues_Checked(object sender, RoutedEventArgs e)
        {
            if (fuzzySystem == null)
                return;
            
            FuzzyBox.Text = fuzzySystem.ToString(true);
        }
        private void IncludeValues_Unchecked(object sender, RoutedEventArgs e)
        {
            if (fuzzySystem == null)
                return;

            FuzzyBox.Text = fuzzySystem.ToString(false);
        }

        // Stopping type changes
        private void tbStopGen_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                StopGenerationNumber = int.Parse(tbStopGen.Text);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void tbStopEval_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                StopAffinityEvaluation = int.Parse(tbStopEval.Text);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void tbStopFt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                StopFitnessTreshold = double.Parse(tbStopGen.Text);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void cbStopType_Selected(object sender, RoutedEventArgs e)
        {
            object Parsed = Enum.Parse(typeof(StoppingType), cbStopType.SelectedItem.ToString(), true);
            if (Parsed != null)
                StopType = (StoppingType)Parsed;
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