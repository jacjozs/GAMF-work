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
        private int[] R = new int[] { 10, 20, 30, 40, 50 };
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
            NA = 20;
            Optimizer = new ArtificialBeeColony()
            {
                ExBeeCount = 10,
                MaxStep = 5,
                NumberOfElements = NA,
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
            Optimizer.GenerationCreated += ShowAntibodies;
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
            Network network = new Network(W, H, Users, Optimizer);
            label.Content = "";
            var x = Task.Run(() => (network.Optimalization()));
            ArrayList[] Results = await x;

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
                    circle.SetValue((DependencyProperty)Canvas.TopProperty, user.Y);
                    circle.SetValue((DependencyProperty)Canvas.LeftProperty, user.X);
                    canvas.Children.Add((UIElement)circle);
                }
                ArrayList[] region = Results;
                for (int i = 0; i < region.Length; i++)
                {
                    for (int j = 0; j < region[i].Count / 3; j += 3)
                    {
                        int type = int.Parse(region[i][j + 2].ToString());
                        var circle = new Ellipse
                        {
                            Width = R[type] * 2,
                            Height = R[type] * 2,
                            Opacity = 0.4,
                            Fill = Brushes.Blue
                        };
                        circle.SetValue((DependencyProperty)Canvas.TopProperty, ((double)region[i][j + 1]) - R[type]);
                        circle.SetValue((DependencyProperty)Canvas.LeftProperty, ((double)region[i][j]) - R[type]);
                        canvas.Children.Add((UIElement)circle);
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
        void ShowAntibodies(object sender, ArrayList Antibodies, double[] affinities)
        {
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
                    circle.SetValue((DependencyProperty)Canvas.TopProperty, user.Y);
                    circle.SetValue((DependencyProperty)Canvas.LeftProperty, user.X);
                    canvas.Children.Add((UIElement)circle);
                }
                BaseElement region = (BaseElement)Antibodies[0];
                for (int i = 0; i < region.Position.Count; i += 3)
                {
                    int type = int.Parse(region[i + 2].ToString());
                    var circle = new Ellipse
                    {
                        Width = R[type] * 2,
                        Height = R[type] * 2,
                        Opacity = 0.4,
                        Fill = Brushes.Blue
                    };
                    circle.SetValue((DependencyProperty)Canvas.TopProperty, ((double)region[i + 1]) - R[type]);
                    circle.SetValue((DependencyProperty)Canvas.LeftProperty, ((double)region[i]) - R[type]);
                    canvas.Children.Add((UIElement)circle);
                }

            }), DispatcherPriority.Send, null);
        }
    }
}
