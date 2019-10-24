using Optimization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FiveNetwork
{
    public class Network
    {
        public static int[] R = new int[] { 5, 10, 15, 20, 25, 30 };
        private int actualRegionindex = 0;
        private ArrayList users;
        private BaseOptimizationMethod mehod;
        private int width, height;
        private ArrayList InitialParameters, lbp, ubp;
        private bool[] Integer;
        private Random RNG = new Random();
        private List<Region> regions;
        private Region actualRegion;
        private Canvas canvas;

        public Network(int width, int height, ArrayList users, BaseOptimizationMethod mehod, Canvas canvas)
        {
            this.users = users;
            this.mehod = mehod;
            this.width = width;
            this.height = height;
            this.canvas = canvas;
            this.lbp = new ArrayList();
            this.ubp = new ArrayList();
            this.InitialParameters = new ArrayList();
            this.mehod.FitnessFunction = FitnessFunction;
            this.mehod.GenerationCreated += ShowAntibodies;
            this.regions = new List<Region>();
            int X = this.width / (R[R.Length - 1] * 2);
            int Y = this.height / (R[R.Length - 1] * 2);
            this.actualRegionindex = 0;
            for (int i = 0; i < Y; i++)
            {
                for (int j = 0; j < X; j++)
                {
                    int heightDown = i * (R[R.Length - 1] * 2);
                    int heightUp = (i + 1) * (R[R.Length - 1] * 2);
                    int widhtDown = j * (R[R.Length - 1] * 2);
                    int widhtUp = (j + 1) * (R[R.Length - 1] * 2);
                    List<User> regionUser = new List<User>();
                    foreach (Point user in this.users)
                    {
                        if(user.X >= widhtDown && user.X <= widhtUp && user.Y >= heightDown && user.Y <= heightUp)
                        {
                            regionUser.Add(new User() { position = user, isCover = false});
                        }
                    }
                    regions.Add(new Region(actualRegionindex++, new Point(j, i), regionUser.ToArray(), 0, heightDown, heightUp, widhtDown, widhtUp));
                }
            }
            this.actualRegionindex = 0;
            List<int> neighbors;
            foreach (Region region in this.regions)
            {
                neighbors = new List<int>();
                foreach (Region neighbor in this.regions)
                {
                    if (neighbor.pos.Y == region.pos.Y - 1)
                    {
                        if(neighbor.pos.X == region.pos.X - 1 || neighbor.pos.X == region.pos.X || neighbor.pos.X == region.pos.X + 1)
                        {
                            neighbors.Add(neighbor.id);
                        }
                    } else
                    if (neighbor.pos.Y == region.pos.Y)
                    {
                        if (neighbor.pos.X == region.pos.X - 1 || neighbor.pos.X == region.pos.X + 1)
                        {
                            neighbors.Add(neighbor.id);
                        }
                    } else
                    if (neighbor.pos.Y == region.pos.Y + 1)
                    {
                        if (neighbor.pos.X == region.pos.X - 1 || neighbor.pos.X == region.pos.X || neighbor.pos.X == region.pos.X + 1)
                        {
                            neighbors.Add(neighbor.id);
                        }
                    }
                }
                region.Neighbor = neighbors.ToArray();
            }
        }

        public List<Region> Optimalization()
        {
            for (int count = 1; count < 4; count++)
            {
                ArrayList[] actualElements = new ArrayList[this.width / (R[R.Length - 1] * 2) * this.width / (R[R.Length - 1] * 2)];
                this.actualRegionindex = 0;
                for (int i = 0; i < this.height / (R[R.Length - 1] * 2); i++)
                {
                    for (int j = 0; j < this.width / (R[R.Length - 1] * 2); j++)
                    {
                        this.actualRegion = (Region)this.regions[this.actualRegionindex].Clone();
                        if (this.actualRegion.AllUser != 0)
                        {
                            ParameterSet(count);
                            if (this.mehod is ArtificialBeeColony)
                            {
                                ((ArtificialBeeColony)this.mehod).reset();
                            }
                            if (this.mehod is ParticleSwarm)
                            {
                                ((ParticleSwarm)this.mehod).reset();
                            }
                            this.mehod.InitialParameters = this.InitialParameters;
                            this.mehod.LowerParamBounds = this.lbp;
                            this.mehod.UpperParamBounds = this.ubp;
                            this.mehod.Integer = this.Integer;
                            Result result = this.mehod.Optimize();
                            this.mehod.FitnessFunction(result.OptimizedParameters);
                            this.actualRegion.fitness = (double)result.InfoList[InfoTypes.FinalFitness];
                            Differencie();
                            this.actualRegionindex++;
                        }
                        else
                        {
                            this.actualRegionindex++;
                        }
                    }
                }
            }
            foreach (Region region in this.regions)
            {
                if(region.Towers != null)
                {
                    foreach (Tower tower in region.Towers)
                    {
                        foreach (User user in region.Users)
                        {
                            if (user.isCover) continue;
                            double alpha = ((user.position.X - tower.position.X) * (user.position.X - tower.position.X)) + ((user.position.Y - tower.position.Y) * (user.position.Y - tower.position.Y));
                            if (alpha < tower.radius * tower.radius)
                            {
                                user.isCover = true;
                            }
                        }
                    }
                }
            }
            return this.regions;
        }
        private void ShowAntibodies(object sender, ArrayList Antibodies, double[] affinities)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                //clear the canvas
                canvas.Children.Clear();

                foreach (Point user in this.users)
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
                BaseElement region = (BaseElement)Antibodies[0];
                for (int i = 0; i < region.Position.Count; i += 3)
                {
                    int type = int.Parse(region[i + 2].ToString());
                    var circle = new Ellipse
                    {
                        Width = Network.R[type] * 2 + 8,
                        Height = Network.R[type] * 2 + 8,
                        Opacity = 0.4,
                        Fill = Brushes.Blue
                    };
                    circle.SetValue((DependencyProperty)Canvas.TopProperty, ((double)region[i + 1]) - Network.R[type] - 4);
                    circle.SetValue((DependencyProperty)Canvas.LeftProperty, ((double)region[i]) - Network.R[type] - 4);
                    canvas.Children.Add((UIElement)circle);
                }

                foreach (Region regio in this.regions)
                {
                    if(regio.Towers != null && regio.id != this.actualRegionindex)
                    {
                        foreach (Tower tower in regio.Towers)
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
                    }
                }
            }), DispatcherPriority.Send, null);
        }
        private double FitnessFunction(ArrayList ActualParameters)
        {
            Point point;
            int type = 0;
            List<Tower> towers = new List<Tower>();
            for (int i = 0; i < ActualParameters.Count; i += 3)
            {
                point = new Point((double)ActualParameters[i], (double)ActualParameters[i + 1]);
                type = int.Parse(ActualParameters[i + 2].ToString());
                int users = 0;
                foreach (User user in this.actualRegion.Users)
                {
                    if (user.isCover) continue;
                    double alpha = ((user.position.X - point.X) * (user.position.X - point.X)) + ((user.position.Y - point.Y) * (user.position.Y - point.Y));
                    if (alpha < R[type] * R[type])
                    {
                        user.isCover = true;
                        users++;
                    }
                }
                towers.Add(new Tower(point, R[type], users));
            }
            this.actualRegion.Towers = towers.ToArray();
            double interferenc = 0.0;
            foreach (int index in this.actualRegion.Neighbor)
            {
                Region neighbor = this.regions[index];
                if (neighbor.Towers != null)
                {
                    foreach (Tower tower in this.actualRegion.Towers)
                    {
                        foreach (Tower neighborTower in neighbor.Towers)
                        {
                            interferenc += rangeAeaCalculator.CalcrangeAea(tower.position, tower.radius, neighborTower.position, neighborTower.radius);
                        }
                    }
                }
            }
            double towerCost = 0.0;
            foreach (Tower tower in this.actualRegion.Towers)
            {
                foreach (Tower ownerTower in this.actualRegion.Towers)
                {
                    if(tower.position != ownerTower.position)
                    {
                        interferenc += rangeAeaCalculator.CalcrangeAea(tower.position, tower.radius, ownerTower.position, ownerTower.radius);
                    }
                }
                towerCost += tower.radius * tower.users == 0 ? 5 : 1;
            }
            double userValue = 1.1 - ((double)this.actualRegion.CoverUser / (double)this.actualRegion.AllUser);
            double T = (this.actualRegion.widhtUp - this.actualRegion.widhtDown) * (this.actualRegion.heightUp - this.actualRegion.heightDown);
            double value = ((userValue * userValue) * interferenc) + ((userValue * userValue) * towerCost);
            //double value = ((this.actualRegion.AllUser - (this.actualRegion.CoverUser)) / 1.5) + ((1.2 - userValue) * interferenc) + ((1.1 - userValue) * towerCost);
            foreach (User user in this.actualRegion.Users)
            {
                user.isCover = false;
            }
            return value;
        }
        private void ParameterSet(int count)
        {
            lbp.Clear();
            ubp.Clear();
            InitialParameters.Clear();
            Integer = new bool[count * 3];
            Region region = this.actualRegion;
            for (int i = 0; i < count; i++)
            {
                InitialParameters.Add((double)(((region.widhtUp - region.widhtDown) / 2) + region.widhtDown));//x
                InitialParameters.Add((double)(((region.heightUp - region.heightDown) / 2) + region.heightDown));//y
                InitialParameters.Add((double)R.Length - 1);//size

                lbp.Add((double)region.widhtDown);//x
                ubp.Add((double)region.widhtUp);//x

                lbp.Add((double)region.heightDown);//y
                ubp.Add((double)region.heightUp);//y

                lbp.Add(0.0);//size
                ubp.Add((double)R.Length - 1);//size

                Integer[i * 3] = false;
                Integer[i * 3 + 1] = false;
                Integer[i * 3 + 2] = true;
            }
        }

        private void Differencie()
        {
            if (this.actualRegion.fitness < this.regions[this.actualRegionindex].fitness)
            {
                this.regions[this.actualRegionindex] = (Region)this.actualRegion.Clone();
            }
        }
    }
}
