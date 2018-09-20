using Optimization;
using System;
using System.Collections;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OptimizationTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IOptimizationMethod Optimizer;
        private ArrayList InitialParameters, lbp, ubp;
        private bool[] Integer;
        private int NA;
        private int method;
        private bool Preview;
        private bool Slow;
        // Stopping
        private StoppingType stoppingType;
        /// <summary>
		/// Allowed number of generations.
		/// </summary>
		public long ng { get; set; }
        /// <summary>
        /// Allowed number of affinity evaluations.
        /// </summary>
        public long nev { get; set; }
        /// <summary>
        /// Affinity (fitness) treshold.
        /// </summary>
        public double Ftr { get; set; }
        // Fitness function
        private FitnessFunctionDelegate ffd;
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
        /// Number of local searches in each generation
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
        // Bee algorithm parameters
        private double ExploratoryRadius;
        private double SearchRadius;
        private int MaxStep;
        // Bacterial algorithm parameters
        private int Infections;
        private int ClonesCount;
        // Storing the opened parameter list
        private int openParams;

        public MainWindow()
        {
            InitializeComponent();
            openParams = 0;
            // Lower and upper bounds for the parameters.
            lbp = new ArrayList { -100.0, -100.0 };
            ubp = new ArrayList { 100.0, 100.0 };
            // Initial values of the parameters to be optimized.
            InitialParameters = new ArrayList { 60.0, -99.0 };
            // Define whether the seeked values should be restricted to integers (true) or not (false).
            Integer = new bool[] { false, false };
            //Create optimizer object.
            // Number of antibodies.
            NA = 50;
            method = 0;
            Slow = false;
            Preview = false;
            // Stopping
            stoppingType = StoppingType.GenerationNumber;
            ng = 1000;
            nev = 25000;
            Ftr = 0.00001;
            // Fitness function
            ffd = FitnessFunctionParabola;
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
            // Particle swarm params
            c0 = 0.8;
            cp = 0.2;
            cg = 0.2;
            // Genetic algorithm params
            P = NA / 2;
            pm = 0.6;
            crossoverCount = NA;
            // Bee algorithm params
            ExploratoryRadius = 10;
            SearchRadius = 3;
            MaxStep = 30;
            // Bacterial algorithm params
            Infections = 10;
            ClonesCount = 25;
        }

        /// <summary>
        /// Calculates the fitness of a parameter tuple. The calculated fitness is greater or equal to 0, where 0 is the best value.
        /// </summary>
        /// <param name="ActualParameters">The parameter tuple.</param>
        /// <returns>Fitness value. The calculated fitness is greater or equal to 0, where 0 is the best value.</returns>


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

        // Algorithm menu
        private void uncheckMethods()
        {
            miFirework.IsChecked = false;
            miPSO.IsChecked = false;
            miCGO.IsChecked = false;
            miCGOL.IsChecked = false;
            miGa.IsChecked = false;
            miNew.IsChecked = false;
            miBee.IsChecked = false;
            miBacterial.IsChecked = false;
            miAnt.IsChecked = false;
        }
        private void miFirework_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 0;
            miFirework.IsChecked = true;
            if (openParams == 3)
                miParamAlg_Click(sender, e);
        }
        private void miPSO_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 1;
            miPSO.IsChecked = true;
            if (openParams == 3)
                miParamAlg_Click(sender, e);
        }
        private void miCGO_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 2;
            miCGO.IsChecked = true;
            if (openParams == 3)
                miParamAlg_Click(sender, e);
        }
        private void miCGOL_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 4;
            miCGOL.IsChecked = true;
            if (openParams == 3)
                miParamAlg_Click(sender, e);
        }
        private void miGa_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 3;
            miGa.IsChecked = true;
            if (openParams == 3)
                miParamAlg_Click(sender, e);
        }
        private void miNew_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 5;
            miNew.IsChecked = true;
            if (openParams == 3)
                miParamAlg_Click(sender, e);
        }
        private void miBee_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 6;
            miBee.IsChecked = true;
            if (openParams == 3)
                miParamAlg_Click(sender, e);
        }
        private void miBacterial_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 7;
            miBacterial.IsChecked = true;
            if (openParams == 3)
                miParamAlg_Click(sender, e);
        }
        private void miAnt_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 8;
            miAnt.IsChecked = true;
            if (openParams == 3)
                miParamAlg_Click(sender, e);
        }

        // Stopping type menu
        private void UncheckStoppingType()
        {
            miGeneration.IsChecked = false;
            miEvaluation.IsChecked = false;
            miTreshold.IsChecked = false;
        }
        private void miGeneration_Click(object sender, RoutedEventArgs e)
        {
            UncheckStoppingType();
            stoppingType = StoppingType.GenerationNumber;
            miGeneration.IsChecked = true;
        }
        private void miEvaluation_Click(object sender, RoutedEventArgs e)
        {
            UncheckStoppingType();
            stoppingType = StoppingType.EvaluationNumber;
            miEvaluation.IsChecked = true;
        }
        private void miTreshold_Click(object sender, RoutedEventArgs e)
        {
            UncheckStoppingType();
            stoppingType = StoppingType.PerformanceTreshold;
            miTreshold.IsChecked = true;
        }

        // Fitness functions
        private double p(double u)
        {
            //p(u) = 1 if u ≥ 0, p(u) = 0 if u < 0
            if (u >= 0)
                return 1;
            return 0;
        }
        private double FitnessFunctionParabola(ArrayList ActualParameters)
        {
            // We are looking for the minimum point of the function z=x^2+y^2 .
            return (double)ActualParameters[0] * (double)ActualParameters[0] + (double)ActualParameters[1] * (double)ActualParameters[1];
        }
        private double FitnessFunctionTripod(ArrayList ActualParameters)
        {
            // f(x1, x2) = p(x2)(1 + p(x1)) + | x1 + 50p(x2)(1 − 2p(x1)) | + | x2 + 50(1 − 2p(x2)) |
            return p((double)ActualParameters[1]) * (1 + p((double)ActualParameters[0])) +
                   Math.Abs((double)ActualParameters[0] +
                            50 * p((double)ActualParameters[1]) * (1 - 2 * p((double)ActualParameters[0]))) +
                   Math.Abs((double)ActualParameters[1] + 50 * (1 - 2 * p((double)ActualParameters[1])));

        }
        private double FitnessFunctionAlpine(ArrayList ActualParameters)
        {
            //f(xd) = XDd = 1| xdsin(xd) + 0.1xd |
            double result = 0.0;

            for (int d = 0; d < ActualParameters.Count; d++)
            {
                result +=
                    Math.Abs((double)ActualParameters[d] * Math.Sin((double)ActualParameters[d]) +
                             0.1 * (double)ActualParameters[d]);
            }

            return result;
        }
        private double FitnessFunctionGriewank(ArrayList ActualParameters)
        {
            // Az eredeti függvény szerint a subtractThis == 100.0, de én ezt kicseréltem 0.0-ra, mert csak a minimumpontot tolta arrébb a [-100;-100]-as koordinára a középpont helyett.
            double Result = 0.0;
            double subtractThis = 0.0;

            for (int d = 0; d < ActualParameters.Count; d++)
            {
                Result += ((double)ActualParameters[d] - subtractThis) *
                          ((double)ActualParameters[d] - subtractThis);
            }

            Result /= 4000;

            double Temp = 1.0;
            for (int e = 0; e < ActualParameters.Count; e++)
            {
                Temp *= Math.Cos(((double)ActualParameters[e] - subtractThis) / Math.Sqrt(e + 1));
            }

            Result -= Temp;

            Result += 1;

            return Result;
        }
        private double FitnessFunctionRosenbrock(ArrayList ActualParameters)
        {
            double result = 0.0f;

            for (int d = 0; d < ActualParameters.Count - 1; d++)
            {
                result += ((1.0 - (double)ActualParameters[d]) * (1.0 - (double)ActualParameters[d])) + 100 * ((double)ActualParameters[d] * (double)ActualParameters[d] - (double)ActualParameters[d + 1]) * ((double)ActualParameters[d] * (double)ActualParameters[d] - (double)ActualParameters[d + 1]);
            }

            return result;
        }

        // Function menu
        private void miPreview_Checked(object sender, RoutedEventArgs e)
        {
            Preview = true;
        }
        private void miPreview_Unchecked(object sender, RoutedEventArgs e)
        {
            Preview = false;
        }
        private void SetFitnessFunction(FitnessFunctionDelegate argDelegate)
        {
            //uncheck
            miXXplusYY.IsChecked = false;
            miTripod.IsChecked = false;
            miAlpine.IsChecked = false;
            miGriewank.IsChecked = false;
            miRosenbrock.IsChecked = false;
            //set
            ffd = argDelegate;
            //preview
            cvPage.Children.Clear();
            if (Preview)
            {
                Optimizer = new PreviewDisplayer
                {
                    LowerParamBounds = new ArrayList { -100, -100 },
                    UpperParamBounds = new ArrayList { 100, 100 },
                    FitnessFunction = ffd
                };
                Optimizer.GenerationCreated += this.ShowAntibodies;
                Optimizer.Optimize();
            }
        }
        private void miXXplusYY_Click(object sender, RoutedEventArgs e)
        {
            SetFitnessFunction(FitnessFunctionParabola);
            miXXplusYY.IsChecked = true;
        }
        private void miTripod_Click(object sender, RoutedEventArgs e)
        {
            SetFitnessFunction(FitnessFunctionTripod);
            miTripod.IsChecked = true;
        }
        private void miAlpine_Click(object sender, RoutedEventArgs e)
        {
            SetFitnessFunction(FitnessFunctionAlpine);
            miAlpine.IsChecked = true;
        }
        private void miGriewank_Click(object sender, RoutedEventArgs e)
        {
            SetFitnessFunction(FitnessFunctionGriewank);
            miGriewank.IsChecked = true;
        }
        private void miRosenbrock_Click(object sender, RoutedEventArgs e)
        {
            SetFitnessFunction(FitnessFunctionRosenbrock);
            miRosenbrock.IsChecked = true;
        }

        // Optimization process
        private async void btStart_Clicked(object sender, RoutedEventArgs e)
        {
            btStart.Visibility = Visibility.Collapsed;
            btStop.Visibility = Visibility.Visible;
            cvPage.Children.Clear();

            switch (method)
            {
                case 0: //Firework
                    Optimizer = new Firework
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = lbp,
                        UpperParamBounds = ubp,
                        Integer = Integer,
                        FitnessFunction = ffd,
                        // Number of particles in the swarm.
                        NumberOfElements = NA,
                        m = m,
                        ymax = double.MaxValue,
                        a = a,
                        b = b,
                        Amax = Amax,
                        mhat = mhat,
                        StoppingType = stoppingType,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Number of allowed affinity evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Affinity treshold.
                        StoppingFitnessTreshold = Ftr,
                        Slow = Slow
                    };
                    break;
                case 1: //Particle Swarm
                    Optimizer = new ParticleSwarm
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = lbp,
                        UpperParamBounds = ubp,
                        Integer = Integer,
                        FitnessFunction = ffd,
                        // Number of particles in the swarm.
                        NumberOfElements = NA,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,

                        c0 = c0,
                        // Multiplication factor for the distance to the personal best position.
                        cp = cp,
                        // Multiplication factor for the distance to the global best position.
                        cg = cg,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Fitness treshold.
                        StoppingFitnessTreshold = Ftr,
                        StoppingType = stoppingType,
                        Slow = Slow
                    };
                    break;
                case 2: //Clonal generation
                    Optimizer = new ClonalGeneration
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = lbp,
                        UpperParamBounds = ubp,
                        Integer = Integer,
                        FitnessFunction = ffd,
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
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Number of allowed affinity evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Affinity treshold.
                        StoppingFitnessTreshold = Ftr,
                        Slow = Slow
                    };
                    break;
                case 3:
                    Optimizer = new GeneticAlgorithm
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = lbp,
                        UpperParamBounds = ubp,
                        Integer = Integer,
                        FitnessFunction = ffd,
                        // Size of the individual pool.
                        NumberOfElements = NA,
                        // Number of parents in each generation.
                        ParentsInEachGeneration = P,
                        // The probability of mutation.
                        MutationProbability = pm,
                        // The number of crossovers in each generation.
                        CrossoverPerGeneration = crossoverCount,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Fitness treshold.
                        StoppingFitnessTreshold = Ftr,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        Slow = Slow
                    };
                    break;
                case 4: //Clonal generation local
                    Optimizer = new ClonalGenerationLocal
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = lbp,
                        UpperParamBounds = ubp,
                        Integer = Integer,
                        FitnessFunction = ffd,
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
                        // RelativeStepSize
                        StepSizeRelative = cglssr,
                        // Local search/get
                        LocalSearchesPerGeneration = cgln,
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Number of allowed affinity evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Affinity treshold.
                        StoppingFitnessTreshold = Ftr,
                        Slow = Slow
                    };
                    break;
                case 5:
                    Optimizer = new NewtonL
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = lbp,
                        UpperParamBounds = ubp,
                        Integer = Integer,
                        FitnessFunction = ffd,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Fitness treshold.
                        StoppingFitnessTreshold = Ftr,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        Slow = Slow
                    };
                    break;
                case 6:
                    Optimizer = new BeeAlg
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = lbp,
                        UpperParamBounds = ubp,
                        Integer = Integer,
                        FitnessFunction = ffd,
                        // Size of the antibody pool.
                        NumberOfElements = NA,
                        EliteBees = new ArrayList(),
                        //Felderitő méhek keresési rádiusza
                        ExploratoryRadius = ExploratoryRadius,
                        //Kereső méhek keresési rádiusza
                        SearchRadius = SearchRadius,
                        //Felderitő méhek maximális keressi számas ciklus alatt
                        MaxStep = MaxStep,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Fitness treshold.
                        StoppingFitnessTreshold = Ftr,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        Slow = Slow
                    };
                    break;
                case 7:
                    Optimizer = new BacterialAlg
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = lbp,
                        UpperParamBounds = ubp,
                        Integer = Integer,
                        // Size of the antibody pool.
                        NumberOfElements = NA,
                        ClonesCount = ClonesCount,
                        Infections = Infections,
                        FitnessFunction = ffd,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Fitness treshold.
                        StoppingFitnessTreshold = Ftr,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        Slow = Slow
                    };
                    break;
                case 8:
                    Optimizer = new AntAlg
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = lbp,
                        UpperParamBounds = ubp,
                        Integer = Integer,
                        // Size of the antibody pool.
                        NumberOfElements = NA,
                        FitnessFunction = ffd,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Fitness treshold.
                        StoppingFitnessTreshold = Ftr,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        Slow = Slow
                    };
                    break;
            }

            Optimizer.GenerationCreated += this.ShowAntibodies;

            tbResults.Text = "Initial parameters: " + List(InitialParameters) + "\r\n" +
                                             "Lower bounds for the parameters: " + List(lbp) + "\r\n" +
                                             "Upper bounds for the parameters: " + List(ubp) + "\r\n";

            var x = Task.Run(() => (Optimizer.Optimize()));
            var Results = await x;

            tbResults.Text = tbResults.Text + "Initial fitness: " + Results.InfoList[0] + "\r\n" +
                "Final parameters: " + List(Results.OptimizedParameters) + "\r\n" +
                "Final fitness: " + $"{Results.InfoList[1],10:F6}" + "\r\n" +
                "Number of generations: " + Results.InfoList[2] + "\r\n" +
                "Number of fitness evaluations: " + Results.InfoList[3] + "\r\n" +
                "Best affinities in each generation: " + List((ArrayList)Results.InfoList[4]);
            btStart.Visibility = Visibility.Visible;
            btStop.Visibility = Visibility.Collapsed;
        }
        private void btStop_Click(object sender, RoutedEventArgs e)
        {
            Optimizer.Stop = true;
        }
        private void ShowResults()
        {
            grParams.Visibility = Visibility.Collapsed;
            tbResults.Visibility = Visibility.Visible;
        }
        void ShowAntibodies(object sender, ArrayList Antibodies, double[] affinities)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                //clear the canvas
                cvPage.Children.Clear();

                double min = affinities.Min();
                double max = affinities.Max();

                //draw the new state
                for (int i = 0; i < Antibodies.Count; i++)
                {
                    double x = (double)((IElement)Antibodies[i])[0];
                    double y = (double)((IElement)Antibodies[i])[1];
                    // (value-min)/(max-min)*255
                    byte red = (byte)Math.Round((affinities[i] - min) / (max - min) * 255);
                    var circle = new Ellipse
                    {
                        Width = 2,
                        Height = 2,
                        Fill = new SolidColorBrush(Color.FromArgb(255, red, (byte)(255 - red), 0))
                    };
                    circle.SetValue((DependencyProperty)Canvas.TopProperty, y + 100);
                    circle.SetValue((DependencyProperty)Canvas.LeftProperty, x + 100);
                    cvPage.Children.Add((UIElement)circle);
                }
            }), DispatcherPriority.Send, null);
        }

        // Params menu
        private void miGyors_Checked(object sender, RoutedEventArgs e)
        {
            Slow = false;
        }
        private void miGyors_Unchecked(object sender, RoutedEventArgs e)
        {
            Slow = true;
        }
        private void miParamStopping_Click(object sender, RoutedEventArgs e)
        {
            ShowParams();
            openParams = 1;
            lbParam0.Content = "Generation number";
            tbParam0.Text = ng.ToString();
            lbParam1.Content = "Evaluation number";
            tbParam1.Text = nev.ToString();
            lbParam2.Content = "Threshold value";
            tbParam2.Text = Ftr.ToString();
            lbParam3.Content = "";
            tbParam3.Visibility = Visibility.Hidden;
            lbParam4.Content = "";
            tbParam4.Visibility = Visibility.Hidden;
            lbParam5.Content = "";
            tbParam5.Visibility = Visibility.Hidden;
        }
        private void miParamGen_Click(object sender, RoutedEventArgs e)
        {
            ShowParams();
            openParams = 2;
            lbParam0.Content = "Particle count";
            tbParam0.Text = NA.ToString();
            lbParam1.Content = "Integer1";
            tbParam1.Text = Integer[0].ToString();
            lbParam2.Content = "Integer2";
            tbParam2.Text = Integer[1].ToString();
            lbParam3.Content = "";
            tbParam3.Visibility = Visibility.Hidden;
            lbParam4.Content = "";
            tbParam4.Visibility = Visibility.Hidden;
            lbParam5.Content = "";
            tbParam5.Visibility = Visibility.Hidden;
        }
        private void miParamAlg_Click(object sender, RoutedEventArgs e)
        {
            ShowParams();
            openParams = 3;
            switch (method)
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
                    lbParam5.Content = "";
                    tbParam5.Visibility = Visibility.Hidden;
                    break;
                case 1: // Particle swarm
                    lbParam0.Content = "Coefficient for the distance to the previous velocity value";
                    tbParam0.Text = c0.ToString();
                    lbParam1.Content = "Coefficient for the distance to the personal best position";
                    tbParam1.Text = cp.ToString();
                    lbParam2.Content = "Coefficient for the distance to the global best position";
                    tbParam2.Text = cg.ToString();
                    lbParam3.Content = "";
                    tbParam3.Visibility = Visibility.Hidden;
                    lbParam4.Content = "";
                    tbParam4.Visibility = Visibility.Hidden;
                    lbParam5.Content = "";
                    tbParam5.Visibility = Visibility.Hidden;
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
                    lbParam4.Content = "";
                    tbParam4.Visibility = Visibility.Hidden;
                    lbParam5.Content = "";
                    tbParam5.Visibility = Visibility.Hidden;
                    break;
                case 3: // Genetic algorithm
                    lbParam0.Content = "Number of parents in each generation";
                    tbParam0.Text = P.ToString();
                    lbParam1.Content = "The probability of mutation";
                    tbParam1.Text = pm.ToString();
                    lbParam2.Content = "The number of crossovers in each generation";
                    tbParam2.Text = crossoverCount.ToString();
                    lbParam3.Content = "";
                    tbParam3.Visibility = Visibility.Hidden;
                    lbParam4.Content = "";
                    tbParam4.Visibility = Visibility.Hidden;
                    lbParam5.Content = "";
                    tbParam5.Visibility = Visibility.Hidden;
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
                    lbParam4.Content = "Step size realtive to the whole search area";
                    tbParam4.Text = cglssr.ToString();
                    lbParam5.Content = "Number of local searches in each generation";
                    tbParam5.Text = cgln.ToString();
                    break;
            }
        }
        private void ShowParams()
        {
            tbResults.Visibility = Visibility.Collapsed;
            grParams.Visibility = Visibility.Visible;
            tbParam0.Visibility = Visibility.Visible;
            tbParam1.Visibility = Visibility.Visible;
            tbParam2.Visibility = Visibility.Visible;
            tbParam3.Visibility = Visibility.Visible;
            tbParam4.Visibility = Visibility.Visible;
            tbParam5.Visibility = Visibility.Visible;
        }
        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            ShowResults();
            switch (openParams)
            {
                case 1:
                    try
                    {
                        ng = int.Parse(tbParam0.Text);
                        nev = int.Parse(tbParam1.Text);
                        Ftr = double.Parse(tbParam2.Text);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case 2:
                    try
                    {
                        NA = int.Parse(tbParam0.Text);
                        Integer[0] = bool.Parse(tbParam1.Text);
                        Integer[0] = bool.Parse(tbParam2.Text);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case 3:
                    switch (method)
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
                        case 4: // CGAL
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
                    break;
            }
            openParams = 0;
        }

        // Help
        private void miSugo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "1. Choose an algorithm under the Algorithm menu\n\n2. Under Stopping condition you can choose when you want the optimization process to terminate automatically. This can be:\n\t-After X generations\n\t-After X particle value evaluations\n\t-After the best fittness value falling below X threshold value\n\n3. In the Function menu you can choose which function you want to find the minimum of. You can also set whether you want to see a preview of the function, which might take a while to appear after you select one.\n\n4. Under Parameters you can find various settings:\n\t-The Quick option lets you run the optimization without\n\ta delay,  although this might cause irresponsiveness\n\t-You can configure the stopping condition's values.\n\t-You can also access various settings related to the\n\toptimization from here.\n\n5. In the window's lower left corner you can find the Start button, which lets you start the optimization process. This is visualized in the top left panel. The Stop button lets you stop this process at any time. After the optimization is finished you can see the results on the right side.", "Help", MessageBoxButton.OK, MessageBoxImage.Question);
        }

    }
}
