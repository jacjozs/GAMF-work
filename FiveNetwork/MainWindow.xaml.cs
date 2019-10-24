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
        private int[] R = new int[] { 10, 20, 30, 40, 50, 60 };
        private int W = 300;
        private int H = 300;
        private ArrayList Users;
        private int NA;
        private Random RNG = new Random();
        private BaseOptimizationMethod Optimizer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            start();
        }

        private async void start()
        {
            NA = 50;
            //Optimizer = new ArtificialBeeColony()
            //{
            //    ExBeeCount = 10,
            //    MaxStep = 5,
            //    NumberOfElements = NA,
            //    // Number of allowed fitness evaluations.
            //    StoppingNumberOfEvaluations = 0,
            //    // Fitness treshold.
            //    StoppingFitnessTreshold = 0,
            //    // Number of generations.
            //    StoppingNumberOfGenerations = 10,
            //    // Stopping criteria.
            //    StoppingType = StoppingType.GenerationNumber,
            //    Slow = true
            //};
            Optimizer = new ParticleSwarm
            {
                NumberOfElements = NA,

                c0 = 0.6,
                // Multiplication factor for the distance to the personal best position.
                cp = 0.2,
                // Multiplication factor for the distance to the global best position.
                cg = 0.2,

                // Number of allowed fitness evaluations.
                StoppingNumberOfEvaluations = 0,
                // Fitness treshold.
                StoppingFitnessTreshold = 0,
                // Number of generations.
                StoppingNumberOfGenerations = 20,
                // Stopping criteria.
                StoppingType = StoppingType.GenerationNumber,
                Slow = true

            };
            //Optimizer = new GeneticAlgorithm
            //{
            //    // Size of the individual pool.
            //    NumberOfElements = NA,
            //    // Number of parents in each generation.
            //    ParentsInEachGeneration = NA / 2,
            //    // The probability of mutation.
            //    MutationProbability = 0.6,
            //    // The number of crossovers in each generation.
            //    CrossoverPerGeneration = NA,
            //    // Number of allowed fitness evaluations.
            //    StoppingNumberOfEvaluations = 0,
            //    // Fitness treshold.
            //    StoppingFitnessTreshold = 0,
            //    // Number of generations.
            //    StoppingNumberOfGenerations = 10,
            //    // Stopping criteria.
            //    StoppingType = StoppingType.GenerationNumber,
            //    Slow = true
            //};
            Users = new ArrayList();
            for (int i = 0; i < 66; i++)
            {
                Users.Add(new Point(RNG.NextDouble() * 300, RNG.NextDouble() * 300));
            }
            canvas.Width = W;
            canvas.Height = H;
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
            Network network = new Network(W, H, Users, Optimizer, canvas);
            textBlock.Text = "";
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
                    if(region != null && region.Towers != null)
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
                        textBlock.Text += $"Region: {region.id} alluser: {region.AllUser} cveruser: {region.CoverUser} \n";
                    }
                }

            }), DispatcherPriority.Send, null);
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
    }
}
