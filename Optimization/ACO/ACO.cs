using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Optimization
{
    public class ACO : BaseOptimizationMethod
    {
        private ArrayList FeromonVectors;

        private List<Ant> Ants;
        /// <summary>
        /// Párolgás mérete 0 < P <= 1
        /// </summary>
        private double P = 0.1;
        private double R = 20;

        private double alpha = 0.9;
        private double beta = 0.2;

        protected override void CreateNextGeneration()
        {
            if (FeromonVectors == null) FeromonVectors = new ArrayList();
            if (Ants == null)
            {
                Ants = new List<Ant>();
                for (int i = 0; i < NumberOfElements; i++)
                {
                    Ants.Add(new Ant((BaseElement)Elements[i]));
                }
            }
            bool isFermMove = false;
            for (int i = 0; i < NumberOfElements; i++)
            {
                var parameter = new ArrayList();
                parameter = searchFeromon(Ants[i]);
                isFermMove = parameter.Count > 0;
                if (!isFermMove)
                {
                    for (int p = 0; p < InitialParameters.Count; p++)
                    {
                        parameter.Add(Ants[i].elem[p]);
                        parameter[p] = (double)parameter[p] + (R * (RNG.NextDouble() * 2 - 1));
                        if ((double)parameter[p] > (double)UpperParamBounds[p])
                            parameter[p] = UpperParamBounds[p];
                        else if ((double)parameter[p] < (double)LowerParamBounds[p])
                            parameter[p] = LowerParamBounds[p];
                        if (Integer[p])
                            parameter[p] = Math.Round((double)parameter[p]);
                    }
                }
                BaseElement newPos = (BaseElement)GetNewElement(FitnessFunction, parameter);
                if (newPos.Fitness < Ants[i].elem.Fitness)
                {
                    Ants[i].elem = newPos;
                    Ants[i].addNewPoint(parameter);
                    var feromonPoints = new ArrayList();
                    foreach (ArrayList point in Ants[i].points)
                    {
                        feromonPoints.Add(new Feromon(point, 0));
                    }
                    if (Ants[i].fermonIndexs[0] == -1)
                    {
                        Ants[i].fermonIndexs[0] = FeromonVectors.Count;
                        FeromonVectors.Add(feromonPoints);
                    }
                    else
                    {
                        FeromonVectors[Ants[i].fermonIndexs[0]] = feromonPoints;
                    }
                }
                if (isFermMove)
                {
                    Ants[i].elem = newPos;
                    Ants[i].addNewPoint(parameter);
                }
                Elements[i] = Ants[i].elem;
            }
            Elements.Sort();
            updateFeromons();
        }
        private ArrayList searchFeromon(Ant ant)
        {
            double hossz;
            int i = 0;
            ArrayList param = new ArrayList();
            foreach (ArrayList points in FeromonVectors)
            {
                if (i != ant.fermonIndexs[0])
                {
                    foreach (Feromon feromon in points)
                    {
                        if (isInRondure(feromon.point, ant.elem.Position))
                        {
                            hossz = pointToPoint(feromon.point, ant.elem.Position);
                            foreach (double point in ant.elem.Position)
                            {
                                param.Add(point + (Math.Pow(feromon.amount, alpha) * Math.Pow((1 / hossz), beta)) / (Math.Pow((1 / hossz), beta)));
                            }
                            return param;
                        }
                    }
                }
                i++;
            }
            return param;
        }
        private void updateFeromons()
        {
            double deltaT;
            double hossz, teljesHossz;
            for (int i = 0; i < FeromonVectors.Count; i++)
            {
                for (int j = 0; j < ((ArrayList)FeromonVectors[i]).Count; j++)
                {
                    deltaT = 0.0; teljesHossz = 0.0;
                    foreach (Ant ant in Ants)
                    {
                        int value = ant.fermonIndexs.Where(x => x == i).DefaultIfEmpty(-1).First();
                        if (value >= 0)
                        {
                            foreach (ArrayList antPoints in ant.points)
                            {
                                hossz = 0.0;
                                foreach (double point in antPoints)
                                {
                                    hossz += Math.Pow((point), 2);
                                }
                                teljesHossz += Math.Sqrt(hossz);
                            }
                            deltaT += 1 / teljesHossz;
                        }
                    }
                    ((Feromon)((ArrayList)FeromonVectors[i])[j]).amount = (1 - P) * ((Feromon)((ArrayList)FeromonVectors[i])[j]).amount + deltaT;
                }
            }
        }

        /// <summary>
        /// Megmondja, hogy a vizsgált pontok szerepelnek e a másik pont ható sugarában (Kör egyenlet)
        /// </summary>
        /// <param name="XY">Azon pont amire kiváncsik vagyunk hogy benne van e</param>
        /// <param name="UV">Azon kör középontja amit vizsgálunk</param>
        /// <returns>Bent van e a körben vagy nem</returns>
        private bool isInRondure(ArrayList XY, ArrayList UV)
        {
            double value = 0;
            for (int i = 0; i < XY.Count; i++)
            {
                value += Math.Pow(((double)XY[i] - (double)UV[i]), 2);
            }
            if (value <= (Math.Pow(R, 2)))
            {
                return true;
            }
            return false;
        }
        private double pointToPoint(ArrayList XY, ArrayList UV)
        {
            double hossz = 0;
            ArrayList vector = new ArrayList();
            for (int i = 0; i < XY.Count; i++)
            {
                vector.Add((double)XY[i] - (double)UV[i]);
            }
            foreach (double point in vector)
            {
                hossz += Math.Pow((point), 2);
            }
            return Math.Sqrt(hossz);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="XY">Az irány vektor</param>
        /// <param name="startXY">A kiindulási pont</param>
        /// <param name="R">Darabolási sugár (megegyezik a keresésivel)</param>
        /// <returns></returns>
        private ArrayList lineCuting(ArrayList XY, ArrayList startXY)
        {
            ArrayList pointList = new ArrayList();
            double hossz = 0;
            foreach (double point in XY)
            {
                hossz += Math.Pow((point), 2);
            }
            hossz = Math.Sqrt(hossz);
            ArrayList stepSizes = new ArrayList();
            foreach (double point in XY)
            {
                stepSizes.Add(point / hossz);
            }
            ArrayList stepPoint;
            ArrayList actualPoint = new ArrayList();
            foreach (var point in startXY)
            {
                actualPoint.Add(point);
            }
            for (int i = 0; i < hossz / R; i++)
            {
                stepPoint = new ArrayList();
                for (int j = 0; j < startXY.Count; j++)
                {
                    stepPoint.Add((double)actualPoint[j] + (double)stepSizes[j]);
                    actualPoint[j] = stepPoint[stepPoint.Count - 1];
                }
                pointList.Add(stepPoint);
            }
            return pointList;
        }
        private class Feromon
        {
            public ArrayList point { get; set; }
            public double amount { get; set; }

            public Feromon(ArrayList point, double amount)
            {
                this.point = point;
                this.amount = amount;
            }
        }
        private class Ant : IComparable
        {
            public BaseElement elem { get; set; }
            public ArrayList points { get; set; }
            /// <summary>
            /// Feromon indexek
            /// A 0. a saját
            /// </summary>
            public List<int> fermonIndexs { get; set; }
            /// <summary>
            /// Paraméteres konstruktor
            /// </summary>
            /// <param name="elem"></param>
            /// <param name="startPos"></param>
            public Ant(BaseElement elem)
            {
                this.elem = elem;
                this.fermonIndexs = new List<int>();
                this.fermonIndexs.Add(-1);
                this.points = new ArrayList();
            }
            public void addNewPoint(ArrayList point)
            {
                this.points.Add(point);
            }
            public int CompareTo(object other)
            {
                var otherAnt = (Ant)other;
                if (otherAnt.elem.Fitness < this.elem.Fitness)
                    return 1;
                if (otherAnt.elem.Fitness > this.elem.Fitness)
                    return -1;
                return 0;
            }
        }
    }
}