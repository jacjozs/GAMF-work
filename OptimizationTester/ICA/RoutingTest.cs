using Optimization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OptimizationTester.ICA
{
    public class RoutingTest
    {
        private TextBox tbResults;
        public IOptimizationMethod optimizer;
        public RouteTable fullTable;
        private ArrayList pathLines;
        private List<Car> cars;
        private Canvas cvPage;
        private Random RNG = new Random();
        public RoutingTest(IOptimizationMethod optimizer, RouteTable routeTable, Canvas cvPage, TextBox textBox)
        {
            this.optimizer = optimizer;
            this.fullTable = routeTable;
            this.cvPage = cvPage;
            this.tbResults = textBox;
            this.pathLines = new ArrayList();
            this.cars = new List<Car>();
            int pointsCount = routeTable.Points.Count;//Összes pont száma
            int pathPmax = 1;
            for (int i = 0; i < 9; i++)
            {
                int pointC = RNG.Next(pointsCount) + 1;//Aktuális utvonal pontjainak a száma
                while (pointC <= 3)
                {
                    pointC = RNG.Next(pointsCount) + 1;
                }
                if (pointC > pathPmax)
                {
                    pathPmax = pointC;
                }
                ArrayList path = new ArrayList();
                for (int j = 1; j < pointC; j++)
                {
                    int rnd = RNG.Next(pointsCount);
                    while (path.Contains(rnd) || rnd == 0)
                    {
                        rnd = RNG.Next(pointsCount);
                    }
                    path.Add(rnd);
                }
                pathLines.Add(path);
            }
            for (int i = 0; i < 5; i++)
            {
                this.cars.Add(new Car() { index = i + 1, MaxPathPoint = (int)((RNG.Next(pathPmax) + 1) * 3), CurrentDailyPath = 0, MaxDailyPath = (RNG.Next(pathPmax) + 1) * 2, MaxPathDistance = (RNG.Next(pathPmax) + 1) * 150 });
                Car car = this.cars[cars.Count - 1];
                car.MaxPathDistanceRes = car.MaxPathDistance;
            }
        }

        public void Optimize()
        {
            Result result = null;
            StreamWriter dok = new StreamWriter(this.optimizer.GetType().ToString() + ".txt");
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                foreach (Car car in cars)
                {
                    tbResults.Text += car.index + ". Car MaxDailyPath: " + car.MaxDailyPath + " MaxPathPoint: " + car.MaxPathPoint + " MaxPathDistance: " + car.MaxPathDistance + "\n";
                    dok.WriteLine(car.index + ". Car MaxDailyPath: " + car.MaxDailyPath + " MaxPathPoint: " + car.MaxPathPoint + " MaxPathDistance: " + car.MaxPathDistance + "\n");
                }
                dok.WriteLine();
                tbResults.Text += "\n";
                for (int i = 0; i < this.fullTable.routeTable.Count; i++)
                {
                    Dictionary<int, User> user = this.fullTable.routeTable[i];
                    tbResults.Text += i + " |";
                    dok.Write(i + " |");
                    foreach (int key in user.Keys)
                    {
                        tbResults.Text += "=>" + key + " : " + $"{user[key].km,10:F2}" + "-" + user[key].dem + "|";
                        dok.Write("=>" + key + " : " + $"{user[key].km,10:F2}" + "-" + user[key].dem + "|");
                    }
                    dok.WriteLine();
                    tbResults.Text += "\n";
                }
                tbResults.Text += "\n";
                dok.WriteLine();
            }), DispatcherPriority.Send, null);
            double avgAllFinalFitness = 0.0, avgAllInitFitness = 0.0, avgAllTime = 0.0;
            for (int i = 0; i < 3; i++)
            {
                double avgTime = 0.0, avgFitness = 0.0, initAvgFitness = 0.0;
                double avgEarchTime = 0.0;

                foreach (Car car in this.cars)
                {
                    car.reset();
                }
                double sumAvgEarchTime = 0.0;
                ArrayList pathLinesTmp = new ArrayList();
                pathLinesTmp.AddRange(pathLines);
                double pathDistance = 0.0;
                avgTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                int g = 0;
                while (pathLinesTmp.Count > 0)
                {
                    ArrayList path = (ArrayList)pathLinesTmp[g % pathLinesTmp.Count];
                    Car car = this.searchCar(path.Count, pathDistance);
                    if (car != null)
                    {
                        avgEarchTime = DateTime.Now.Millisecond;
                        ArrayList pathTmp = new ArrayList();
                        foreach (int point in path)
                        {
                            pathTmp.Add((double)point);
                        }
                        this.optimizer.InitialParameters = pathTmp;
                        if(optimizer is ParticleSwarm)
                        {
                            ((ParticleSwarm)this.optimizer).reset();
                        }
                        if(optimizer is ABC)
                        {
                            ((ABC)this.optimizer).reset();
                        }

                        fullTable.pathList = path.Cast<int>().ToList();
                        this.optimizer.FitnessFunction = fullTable.FitnessFunction;
                        ArrayList finalParam = new ArrayList();
                        car.Process = Task<Result>.Factory.StartNew(() => optimizer.Optimize());
                        result = car.Process.Result;
                        double finalFitness = (double)result.InfoList[InfoTypes.FinalFitness];
                        if (car.MaxPathDistance < finalFitness)
                        {
                            pathDistance = finalFitness;
                            continue;
                        }
                        else
                        {
                            car.MaxPathDistance -= finalFitness;
                            if (car.MaxPathDistance < 0)
                            {
                                car.MaxPathDistance = 0.0;
                            }
                            pathDistance = 0.0;
                        }
                        g++;
                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            tbResults.Text += i + " - " + g + "  " + "Car ID: " + car.index + " Initial positions: " + List(fullTable.Encrypt(pathTmp)) + "\r\n" +
                                "Initial fitness: " + $"{result.InfoList[InfoTypes.InitialFitness],10:F5}" + "\r\n" +
                                "Final positions: " + List(fullTable.Encrypt(result.OptimizedParameters)) + "\r\n" +
                                "Final fitness: " + $"{finalFitness,10:F5}" + "\r\n\n";
                            ShowRoutingAntibodies(fullTable.Encrypt(result.OptimizedParameters));
                        }), DispatcherPriority.Send, null);
                        dok.Write("\n" + i + " - " + g + "  " + "Car ID: " + car.index + " Initial positions: " + List(fullTable.Encrypt(pathTmp)) + "\r\n" +
                                "Initial fitness: " + $"{result.InfoList[InfoTypes.InitialFitness],10:F5}" + "\r\n" +
                                "Final positions: " + List(fullTable.Encrypt(result.OptimizedParameters)) + "\r\n" +
                                "Final fitness: " + $"{finalFitness,10:F5}" + "\r\n\n");
                        sumAvgEarchTime += avgEarchTime - DateTime.Now.Millisecond;
                        avgFitness += finalFitness;
                        initAvgFitness += (double)result.InfoList[InfoTypes.InitialFitness];
                        car.Process = null;
                        car.CurrentDailyPath++;
                    }
                    else
                    {
                        Debug.WriteLine("Nincs alkalmas jármű! fent maradt utak száma: " + pathLinesTmp.Count);
                        break;
                    }
                }
                avgAllFinalFitness += avgFitness / g;
                avgAllInitFitness += initAvgFitness / g;
                sumAvgEarchTime /= pathLines.Count;
                avgTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds - avgTime;
                avgAllTime += avgTime;
            }
            Application.Current.Dispatcher.Invoke((Action)(() => {
                tbResults.Text += "Initial Avg fitness: " + $"{avgAllInitFitness / 3,10:F5} \r\n";
                tbResults.Text += "Final Avg fitness: " + $"{avgAllFinalFitness / 3,10:F5} \r\n";
                tbResults.Text += "Final Avg time: " + $"{avgAllTime / 3,10:F3} second \r\n\n";
                tbResults.Text += "Final Optimum ratio: " + $"{100 * (1 - (avgAllFinalFitness / 3) / (avgAllInitFitness / 3)),10:F3} % \r\n\n";
            }), DispatcherPriority.Send, null);
            dok.Write("\n\nInitial Avg fitness: " + $"{avgAllInitFitness / 3,10:F5} \r\n");
            dok.Write("Final Avg fitness: " + $"{avgAllFinalFitness / 3,10:F5} \r\n");
            dok.Write("Final Avg time: " + $"{avgAllTime / 3,10:F3} second \r\n\n");
            dok.Write("Final Optimum ratio: " + $"{100 * (1 - (avgAllFinalFitness / 3) / (avgAllInitFitness / 3)),10:F3} % \r\n\n");
            dok.Close();
        }
        public string List(ArrayList AL)
        {
            var s = "";
            for (var i = 0; i < AL.Count; i++)
            {
                s = s + $"{AL[i],8:F2}";
                if (i != AL.Count - 1)
                {
                    s = s + "; ";
                }
            }
            return s;
        }
        void ShowRoutingAntibodies(ArrayList finalParam)
        {
            //Routing.Points
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                //clear the canvas
                cvPage.Children.Clear();
                Point startP = fullTable.routeTable[0][(int)finalParam[0]].position;
                Point endP = fullTable.routeTable[(int)finalParam[0]][0].position;
                var startLine = new Line//Kiindulási vonal
                {
                    X1 = startP.X + 5,
                    Y1 = startP.Y + 5,
                    X2 = endP.X + 5,
                    Y2 = endP.Y + 5,
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(Colors.Red)
                };
                var border = new Border();

                cvPage.Children.Add(startLine);
                for (int i = 0; i < fullTable.Points.Count; i++)
                {
                    Color color = Colors.Black;
                    if (i == 0)
                    {
                        color = Colors.Blue;
                    }
                    var circle = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = new SolidColorBrush(color)
                    };
                    circle.SetValue(Canvas.TopProperty, fullTable.Points[i].Y);
                    circle.SetValue(Canvas.LeftProperty, fullTable.Points[i].X);
                    cvPage.Children.Add(circle);

                    border = new Border()
                    {
                        Child = new TextBlock() { Text = i.ToString() },
                    };
                    border.SetValue(Canvas.TopProperty, fullTable.Points[i].Y + 6);
                    border.SetValue(Canvas.LeftProperty, fullTable.Points[i].X + 6);
                    cvPage.Children.Add(border);
                }
                for (int i = 1; i < finalParam.Count; i++)
                {
                    startP = fullTable.routeTable[(int)finalParam[i - 1]][0].position;
                    endP = fullTable.routeTable[(int)finalParam[i]][0].position;
                    var line = new Line
                    {
                        X1 = startP.X + 5,
                        Y1 = startP.Y + 5,
                        X2 = endP.X + 5,
                        Y2 = endP.Y + 5,
                        StrokeThickness = 1,
                        Stroke = new SolidColorBrush(Colors.Red)
                    };
                    cvPage.Children.Add(line);
                }
                startP = fullTable.routeTable[(int)finalParam[(int)finalParam.Count - 1]][0].position;
                endP = fullTable.routeTable[0][(int)finalParam[(int)finalParam.Count - 1]].position;
                var end = new Line
                {
                    X1 = startP.X + 5,
                    Y1 = startP.Y + 5,
                    X2 = endP.X + 5,
                    Y2 = endP.Y + 5,
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(Colors.Red)
                };
                cvPage.Children.Add(end);
            }), DispatcherPriority.Send, null);
        }
        private Car searchCar(int pathP, double pathDistance)
        {
            foreach (Car car in this.cars)
            {
                if (car.MaxDailyPath > car.CurrentDailyPath && car.MaxPathPoint >= pathP && car.MaxPathDistance >= pathDistance)
                {
                    return car;
                }
            }
            return null;
        }
    }
}
