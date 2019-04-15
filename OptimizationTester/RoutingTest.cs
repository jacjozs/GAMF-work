using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OptimizationTester
{
    public class RoutingTest
    {
        private Dictionary<string, double> routingTable { get; set; }
        public Point[] Points { get; set; }
        private Random RNG = new Random();
        public RoutingTest(int pointsCount)
        {
            Points = new Point[pointsCount + 1];
            routingTable = new Dictionary<string, double>();
            for (int i = 0; i < pointsCount + 1; i++)
            {
                Points[i].X = RNG.NextDouble() * 300;
                Points[i].Y = RNG.NextDouble() * 300;
            }
        }
        public double FitnessFunction(ArrayList ActualParameters)
        {
            double result = 0.0;
            int[] pointTmp = Encrypt(ActualParameters);
            //Kiindulási pontból az első pontba
            result += Math.Sqrt(Math.Pow((Points[pointTmp[0]].X - Points[0].X), 2) + Math.Pow((Points[pointTmp[0]].Y - Points[0].Y), 2));
            for (int i = 0; i < pointTmp.Length - 1; i++)//Pontok közi távolságok számítása és összeadása
            {
                int key_1 = pointTmp[i];
                int key_2 = pointTmp[i + 1];
                result += Math.Sqrt(Math.Pow((Points[key_2].X - Points[key_1].X), 2) + Math.Pow((Points[key_2].Y - Points[key_1].Y), 2));
            }
            //Vissza a kiindulási pontba
            result += Math.Sqrt(Math.Pow((Points[0].X - Points[pointTmp.Length - 1].X), 2) + Math.Pow((Points[0].Y - Points[pointTmp.Length - 1].Y), 2));

            return result;
        }
        public int[] Encrypt(ArrayList ActualParameters)
        {
            int[] points = new int[ActualParameters.Count];//Hanyas pont után hanyas jön 1 -> 2 -> 3
            for (int i = 0; i < ActualParameters.Count; i++)
            {
                points[i] = i + 1;//1-től indul!!!
            }
            for (int i = 0; i < ActualParameters.Count; i++)
            {
                int index = (int)((double)ActualParameters[i]);
                int tmp = points[index];
                points[index] = points[i];
                points[i] = tmp;
            }
            return points;
        }
    }
}
