using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OptimizationTester.ICA
{
    public class RoutingTest
    {

        private Dictionary<int, Dictionary<int, User>> rout = new Dictionary<int, Dictionary<int, User>>();

        private Random RNG = new Random();
        public RoutingTest(int pointsCount)
        {
            Dictionary<int, User> list;
            int dem = 0;
            for (int i = 0; i < pointsCount; i++)
            {
                list = new Dictionary<int, User>();
                for (int j = 0; j < pointsCount; j++)
                {
                    dem = RNG.Next(1, 5);
                    if (j == 0) dem = 1;
                    if (rout.ContainsKey(j))
                    {
                        list.Add(j, new User { pos = i, posTo = j, km = rout[j][i].km, dem = dem });
                    }
                    else if (j != i)
                    {
                        if (i != 0)
                            dem = rout[0][j].dem;
                        list.Add(j, new User { pos = i, posTo = j, km = RNG.NextDouble() * 300, dem = dem });
                    }
                }
                rout.Add(i, list);
            }
        }
        public double FitnessFunction(ArrayList ActualParameters)
        {
            int[] pointTmp = Encrypt(ActualParameters);
            double result = rout[0][pointTmp[0]].km / rout[0][pointTmp[0]].dem;
            for (int i = 1; i < pointTmp.Length - 1; i++)
            {
                result += rout[pointTmp[i - 1]][pointTmp[i]].km / rout[pointTmp[i - 1]][pointTmp[i]].dem;
            }
            result += rout[pointTmp[pointTmp.Length - 1]][0].km;
            //result = 1 - (1 / result);
            return result;
        }
        public int[] Encrypt(ArrayList ActualParameters)
        {
            int[] points = new int[ActualParameters.Count + 1];//Hanyas pont után hanyas jön 1 -> 2 -> 3
            for (int i = 0; i < ActualParameters.Count + 1; i++)
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
