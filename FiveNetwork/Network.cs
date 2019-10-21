using Optimization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FiveNetwork
{
    public class Network
    {
        private int[] R = new int[] { 10, 20, 30, 40, 50 };
        private int actualRegion;
        private ArrayList users;
        private BaseOptimizationMethod mehod;
        private int width, height;
        private ArrayList InitialParameters, lbp, ubp;
        private bool[] Integer;
        private Random RNG = new Random();
        private ArrayList[] FinalElements;
        private List<Region> regions;

        public Network(int width, int height, ArrayList users, BaseOptimizationMethod mehod)
        {
            this.actualRegion = 0;
            this.users = users;
            this.mehod = mehod;
            this.width = width;
            this.height = height;
            this.lbp = new ArrayList();
            this.ubp = new ArrayList();
            this.InitialParameters = new ArrayList();
            this.FinalElements = new ArrayList[this.width / (R[R.Length - 1] * 2) * this.width / (R[R.Length - 1] * 2)];
            this.mehod.FitnessFunction = FitnessFunction;
            this.regions = new List<Region>();
            for (int i = 0; i < this.height / (R[R.Length - 1] * 2); i++)
            {
                for (int j = 0; j < this.width / (R[R.Length - 1] * 2); j++)
                {
                    int heightId = this.actualRegion / (this.height / (R[R.Length - 1] * 2));
                    int widhtId = this.actualRegion % (this.width / (R[R.Length - 1] * 2));
                    int heightDown = heightId * (R[R.Length - 1] * 2);
                    int heightUp = (heightId + 1) * (R[R.Length - 1] * 2);
                    int widhtDown = widhtId * (R[R.Length - 1] * 2);
                    int widhtUp = (widhtId + 1) * (R[R.Length - 1] * 2);
                    List<Point> regionUser = new List<Point>();
                    foreach (Point user in this.users)
                    {
                        if(user.X >= widhtDown && user.X <= widhtUp && user.Y >= heightDown && user.Y <= heightUp)
                        {
                            regionUser.Add(user);
                        }
                    }
                    regions.Add(new Region(this.actualRegion++, regionUser.ToArray(), 0, heightDown, heightUp, widhtDown, widhtUp));
                }
            }
        }

        public ArrayList[] Optimalization()
        {
            for (int count = 1; count < 5; count++)
            {
                ArrayList[] actualElements = new ArrayList[this.width / (R[R.Length - 1] * 2) * this.width / (R[R.Length - 1] * 2)];
                this.actualRegion = 0;
                for (int i = 0; i < this.height / (R[R.Length - 1] * 2); i++)
                {
                    for (int j = 0; j < this.width / (R[R.Length - 1] * 2); j++)
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
                        actualElements[this.actualRegion++] = result.OptimizedParameters;
                    }
                }
                Differencie(actualElements);
            }
            return this.FinalElements;
        }

        private double FitnessFunction(ArrayList ActualParameters)
        {
            Point point;
            int type = 0;
            int userC = 0;
            Region region = (Region)this.regions[this.actualRegion];
            List<Tower> towers = new List<Tower>();
            for (int i = 0; i < ActualParameters.Count; i += 3)
            {
                point = new Point((double)ActualParameters[i], (double)ActualParameters[i + 1]);
                type = int.Parse(ActualParameters[i + 2].ToString());
                towers.Add(new Tower(point, type));
                foreach (Point user in region.Users)
                {
                    double alpha = ((user.X - point.X) * (user.X - point.X)) + ((user.Y - point.Y) * (user.Y - point.Y));
                    if (alpha < R[type] * R[type])
                    {
                        userC++;
                    }
                }
            }
            region.CoverUser = userC;
            region.Towers = towers.ToArray();
            double interferenc = 1.0;
            foreach (Region neighborRegion in this.regions)
            {
                if(neighborRegion.id != this.actualRegion && neighborRegion.Towers != null)
                {
                    foreach (Tower tower in region.Towers)
                    {
                        foreach (Tower neighborTower in neighborRegion.Towers)
                        {
                            interferenc += rangeAeaCalculator.CalcrangeAea(tower.position, tower.radius, neighborTower.position, neighborTower.radius);
                        }
                    }
                }
            }
            return interferenc / (region.AllUser - region.CoverUser);
        }

        private double[] getMetszes(double[] origok, int[] sugarak)
        {

            return null;
        }

        private void ParameterSet(int count)
        {
            lbp.Clear();
            ubp.Clear();
            InitialParameters.Clear();
            Integer = new bool[count * 3];
            Region region = this.regions[this.actualRegion];
            for (int i = 0; i < count; i++)
            {
                InitialParameters.Add((double)RNG.Next(region.widhtDown, region.widhtUp));//x
                InitialParameters.Add((double)RNG.Next(region.heightDown, region.heightUp));//y
                InitialParameters.Add((double)RNG.Next(0, R.Length - 1));//size

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

        private void Differencie(ArrayList[] actualElements)
        {
            for (int i = 0; i < FinalElements.Length; i++)
            {
                ArrayList finalElem = this.FinalElements[i];
                ArrayList actualElem = actualElements[i];
                this.actualRegion = i;
                if(finalElem == null)
                {
                    if(actualElem == null)
                    {
                        continue;
                    }
                    this.FinalElements[i] = new ArrayList();
                    this.FinalElements[i].AddRange(actualElem);
                    continue;
                }

                double finalFitness = this.mehod.FitnessFunction(finalElem);
                double actualFitness = this.mehod.FitnessFunction(actualElem);
                if (finalFitness < actualFitness)
                {
                    this.FinalElements[i].Clear();
                    this.FinalElements[i].AddRange(actualElem);
                }
            }
        }
    }
}
