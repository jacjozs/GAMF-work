using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Optimization;
using System.Collections;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace FiveNetwork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IOptimizationMethod Optimizer;
        private int NA;
        private int method;
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
        // Differential Evolution parameters
        private double weighf;
        private double crossf;
        // Haromony Search parameters
        private double consid_rate;
        private double adjust_rate;
        private double range;
        private int userC;
        private ArrayList Users = new ArrayList();
        private Random RNG = new Random();

        public MainWindow()
        {
            InitializeComponent();
            NA = 25;
            userC = 50;
            method = 0;
            // Stopping
            stoppingType = StoppingType.GenerationNumber;
            ng = 100;
            nev = 25000;
            Ftr = 0.00001;
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
            // Differential Evolution params
            weighf = 1.8;
            crossf = 0.9;
            // Haromony Search params
            consid_rate = 0.95;
            adjust_rate = 0.7;
            range = 0.05;
            tbUserC.Text = this.userC.ToString();
        }

        private async void btStart_Clicked(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            OptMethod(method);
            this.userC = int.Parse(tbUserC.Text);
            if (Users.Count == 0)
            {
                for (int i = 0; i < userC; i++)
                {
                    Users.Add(new Point(RNG.NextDouble() * canvas.ActualWidth, RNG.NextDouble() * canvas.ActualHeight));
                }
            }
            if(Users.Count != userC)
            {
                Users = new ArrayList();
                for (int i = 0; i < userC; i++)
                {
                    Users.Add(new Point(RNG.NextDouble() * canvas.ActualWidth, RNG.NextDouble() * canvas.ActualHeight));
                }
            }
            foreach (Point point in Users)
            {
                var circle = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = Brushes.Red
                };
                circle.SetValue((DependencyProperty)Canvas.TopProperty, point.Y);
                circle.SetValue((DependencyProperty)Canvas.LeftProperty, point.X);
                canvas.Children.Add((UIElement)circle);
            }
            Network network = new Network(canvas.ActualWidth, canvas.ActualHeight, Users, (BaseOptimizationMethod)Optimizer, canvas);
            tbResults.Text = "";
            var x = Task.Run(() => (network.Optimalization()));
            List<Region> Results = await x;

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                //clear the canvas
                canvas.Children.Clear();
                foreach (Point user in Users)
                {
                    var circle = new Ellipse
                    {
                        Width = 4,
                        Height = 4,
                        Fill = Brushes.Red
                    };
                    circle.SetValue((DependencyProperty)Canvas.TopProperty, user.Y - 2);
                    circle.SetValue((DependencyProperty)Canvas.LeftProperty, user.X - 2);
                    canvas.Children.Add((UIElement)circle);
                }
                foreach (Region region in Results)
                {
                    if (region != null && region.Towers != null)
                    {
                        foreach (Tower tower in region.Towers)
                        {
                            var circle = new Ellipse
                            {
                                Width = tower.radius * 2 + 8,
                                Height = tower.radius * 2 + 8,
                                Opacity = 0.4,
                                Fill = Brushes.Blue
                            };
                            circle.SetValue((DependencyProperty)Canvas.TopProperty, tower.position.Y - tower.radius - 4);
                            circle.SetValue((DependencyProperty)Canvas.LeftProperty, tower.position.X - tower.radius - 4);
                            canvas.Children.Add((UIElement)circle);
                        }
                        tbResults.Text += $"Region: {region.id} alluser: {region.AllUser} cveruser: {region.CoverUser} \n";
                    }
                }

            }), DispatcherPriority.Send, null);
        }

        private void OptMethod(int id)
        {
            switch (id)
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
                        StoppingType = stoppingType,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Number of allowed affinity evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Affinity treshold.
                        StoppingFitnessTreshold = Ftr,
                        Slow = false
                    };
                    break;
                case 1: //Particle Swarm
                    Optimizer = new ParticleSwarm
                    {
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
                        Slow = false
                    };
                    break;
                case 2: //Clonal generation
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
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Number of allowed affinity evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Affinity treshold.
                        StoppingFitnessTreshold = Ftr,
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
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Fitness treshold.
                        StoppingFitnessTreshold = Ftr,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        Slow = false
                    };
                    break;
                case 4: //Clonal generation local
                    Optimizer = new ClonalGenerationLocal
                    {
                        LowerParamBounds = new ArrayList { -100.0 },
                        UpperParamBounds = new ArrayList { 100.0 },
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
                        Slow = false
                    };
                    break;
                case 5:
                    Optimizer = new CoordinateDescent
                    {
                        NumberOfElements = NA,
                        StepSizeRelative = 5,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Fitness treshold.
                        StoppingFitnessTreshold = Ftr,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        Slow = false
                    };
                    break;
                case 6:
                    Optimizer = new ArtificialBeeColony
                    {
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
                        Slow = false
                    };
                    break;
                case 7:
                    Optimizer = new BacterialForaging
                    {
                        // Size of the antibody pool.
                        NumberOfElements = NA,
                        ClonesCount = ClonesCount,
                        Infections = Infections,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Fitness treshold.
                        StoppingFitnessTreshold = Ftr,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        Slow = false
                    };
                    break;
                case 8:
                    Optimizer = new DifferentialEvolution()
                    {
                        weighf = weighf,
                        crossf = crossf,
                        NumberOfElements = NA,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Fitness treshold.
                        StoppingFitnessTreshold = Ftr,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        Slow = false
                    };
                    break;
                case 9:
                    Optimizer = new SelfOrgMigrating
                    {
                        NumberOfElements = NA,
                        PRT = 0.1,
                        ParthLength = 3,
                        PopSize = 7,
                        Step = 0.11,
                        Type = SelfOrgMigratingType.AllToRand,
                        FitnessFunction = ffd,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Fitness treshold.
                        StoppingFitnessTreshold = Ftr,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        Slow = false
                    };
                    break;
                case 10:
                    Optimizer = new HaromonySearch
                    {
                        NumberOfElements = NA,
                        consid_rate = consid_rate,
                        adjust_rate = adjust_rate,
                        range = range,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Fitness treshold.
                        StoppingFitnessTreshold = Ftr,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        Slow = false
                    };
                    break;
                case 11:
                    Optimizer = new NelderMead
                    {
                        NumberOfElements = NA,
                        // Number of allowed fitness evaluations.
                        StoppingNumberOfEvaluations = nev,
                        // Fitness treshold.
                        StoppingFitnessTreshold = Ftr,
                        // Number of generations.
                        StoppingNumberOfGenerations = ng,
                        // Stopping criteria.
                        StoppingType = stoppingType,
                        Slow = false
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
            miDifferentialEv.IsChecked = false;
            miHarmonySearch.IsChecked = false;
            miSoma.IsChecked = false;
            miNelderMead.IsChecked = false;
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
        private void miDifferentialEv_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 8;
            miDifferentialEv.IsChecked = true;
        }
        private void miSoma_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 9;
            miSoma.IsChecked = true;
        }
        private void miHarmonySearch_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 10;
            miHarmonySearch.IsChecked = true;
        }
        private void miNelderMead_Click(object sender, RoutedEventArgs e)
        {
            uncheckMethods();
            method = 11;
            miNelderMead.IsChecked = true;
        }
    }
}
