using Optimization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace OptimizationTester
{
    /// <summary>
    /// Interaction logic for WinRoutingTest.xaml
    /// </summary>
    public partial class WinRoutingTest : Window
    {
        private IOptimizationMethod Optimizer;
        private ArrayList InitialParameters, lbp, ubp;
        private bool[] Integer;
        private int NA;
        private int method;
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
        private int ExBeeCount;
        private int MaxStep;
        // Bacterial algorithm parameters
        private int Infections;
        private int ClonesCount;

        private Dictionary<string, double> routingTable = new Dictionary<string, double>();

        private Random RNG = new Random();

        private int[] points;

        public WinRoutingTest()
        {
            InitializeComponent();
            /*
             * Routing tábla elkészítése 
             */
            for (int i = 0; i <= 12; i++)
            {
                for (int j = 0; j <= 12; j++)
                {
                    routingTable.Add(i + "," + j, RNG.NextDouble() * 100);
                }
            }
            // Lower and upper bounds for the parameters.
            lbp = new ArrayList { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            ubp = new ArrayList { 11.0, 11.0, 11.0, 11.0, 11.0, 11.0, 11.0, 11.0, 11.0, 11.0, 11.0, 11.0 };
            // Initial values of the parameters to be optimized.
            InitialParameters = new ArrayList { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0 };
            // Define whether the seeked values should be restricted to integers (true) or not (false).
            Integer = new bool[] { true, true, true, true, true, true, true, true, true, true, true, true };
            points = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            //Create optimizer object.
            // Number of antibodies.
            NA = 50;
            method = 1;
            Slow = false;
            // Stopping
            stoppingType = StoppingType.GenerationNumber;
            ng = 1000;
            nev = 25000;
            Ftr = 0.00001;
            // Fitness function
            ffd = FitnessFunction;
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
            ExBeeCount = 10;
            MaxStep = 5;
            // Bacterial algorithm params
            Infections = 10;
            ClonesCount = 25;
        }

        private double FitnessFunction(ArrayList ActualParameters)
        {
            double result = 0.0;
            int[] pointTmp = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            for (int i = 0; i < ActualParameters.Count; i++)
            {
                int seged = pointTmp[(int)((double)ActualParameters[i])];
                pointTmp[(int)((double)ActualParameters[i])] = pointTmp[i];
                pointTmp[i] = seged;
            }
            result += routingTable["0,"+ pointTmp[0]];
            for (int i = 1; i < pointTmp.Length - 1; i++)
            {
                result += routingTable[pointTmp[i] + "," + pointTmp[i + 1]];
            }
            result += routingTable[pointTmp[pointTmp.Length - 1] + ",0"];

            return result;
        }

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

        private async void btStart_Click(object sender, RoutedEventArgs e)
        {
            tbResults.Text = "";
            OptMethod(method);
            var x = Task.Run(() => (Optimizer.Optimize()));
            var Results = await x;
            ArrayList pointTmp_1 = new ArrayList();
            ArrayList pointTmp_2 = new ArrayList();
            for (int i = 1; i <= 12; i++)
            {
                pointTmp_1.Add(i);
                pointTmp_2.Add(i);
            }
            for (int i = 0; i < InitialParameters.Count; i++)
            {
                int seged = (int)pointTmp_1[(int)((double)InitialParameters[i])];
                pointTmp_1[(int)((double)InitialParameters[i])] = pointTmp_1[i];
                pointTmp_1[i] = seged;

                seged = (int)pointTmp_2[(int)((double)Results.OptimizedParameters[i])];
                pointTmp_2[(int)((double)Results.OptimizedParameters[i])] = pointTmp_2[i];
                pointTmp_2[i] = seged;
            }
            tbResults.Text = tbResults.Text + "Initial fitness: " + Results.InfoList[InfoTypes.InitialFitness] + "\r\n" +
                    "Initial parameters: " + List(InitialParameters) + "\r\n" +
                    "Initial encrypt parameters: " + List(pointTmp_1) + "\r\n" +
                    "Final parameters: " + List(Results.OptimizedParameters) + "\r\n" +
                    "Final encrypt parameters: " + List(pointTmp_2) + "\r\n" +
                    "Final fitness: " + $"{Results.InfoList[InfoTypes.FinalFitness],10:F6}" + "\r\n" +
                    "Number of generations: " + Results.InfoList[InfoTypes.Generations] + "\r\n" +
                    "Number of fitness evaluations: " + Results.InfoList[InfoTypes.Evaluations] + "\r\n";
        }

        private void OptMethod(int id)
        {
            switch (id)
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
                    Optimizer = new CoordinateDescent
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = lbp,
                        UpperParamBounds = ubp,
                        NumberOfElements = NA,
                        Integer = Integer,
                        StepSizeRelative = 8,
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
                    Optimizer = new ABC
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = lbp,
                        UpperParamBounds = ubp,
                        Integer = Integer,
                        FitnessFunction = ffd,
                        NumberOfElements = NA,
                        //Felderitő méhek maximális keresési számas egy ciklus alatt
                        MaxStep = MaxStep,
                        //Felderítő méhek száma
                        ExBeeCount = ExBeeCount,
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
                    Optimizer = new BFOA
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
                    Optimizer = new ACO
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = lbp,
                        UpperParamBounds = ubp,
                        Integer = Integer,
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
                case 10:
                    Optimizer = new SOMA
                    {
                        InitialParameters = InitialParameters,
                        LowerParamBounds = lbp,
                        UpperParamBounds = ubp,
                        Integer = Integer,
                        NumberOfElements = NA,
                        PRT = 0.1,
                        ParthLength = 3,
                        PopSize = 7,
                        Step = 0.11,
                        Type = SOMA_Type.AllToRand,
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
        }

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
            miSoma.IsChecked = false;
        }
        private void miFirework_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 0;
            miFirework.IsChecked = true;
        }
        private void miPSO_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 1;
            miPSO.IsChecked = true;
        }
        private void miCGO_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 2;
            miCGO.IsChecked = true;
        }
        private void miCGOL_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 4;
            miCGOL.IsChecked = true;
        }
        private void miGa_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 3;
            miGa.IsChecked = true;
        }
        private void miNew_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 5;
            miNew.IsChecked = true;
        }
        private void miBee_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 6;
            miBee.IsChecked = true;
        }
        private void miBacterial_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 7;
            miBacterial.IsChecked = true;
        }
        private void miAnt_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 8;
            miAnt.IsChecked = true;
        }
        private void miSoma_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 10;
            miSoma.IsChecked = true;
        }
    }
}
