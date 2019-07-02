using Optimization;
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

        public Dictionary<int, Dictionary<int, User>> RouteTable;

        public List<Point> Points = new List<Point>();

        public List<ulong> lines = new List<ulong>();//Max 15 points

        public int bitLength = 0;

        public ulong stepMask = 0;

        public int pCount;

        private Random RNG = new Random();

        public RoutingTest(int pointsCount)
        {
            pCount = pointsCount;
            Dictionary<int, User> list;
            RouteTable = new Dictionary<int, Dictionary<int, User>>();
            int dem = 0;
            for (int i = 0; i < pointsCount; i++)
            {
                Point pos = new Point(RNG.NextDouble() * 300, RNG.NextDouble() * 300);
                Points.Add(pos);
            }
            for (int i = 0; i < pointsCount; i++)
            {
                list = new Dictionary<int, User>();
                for (int j = 0; j < pointsCount; j++)
                {
                    dem = RNG.Next(1, 5);
                    
                    if (j == 0) dem = 1;
                    if (RouteTable.ContainsKey(j))
                    {
                        list.Add(j, new User { pos = i, posTo = j, km = RouteTable[j][i].km, dem = dem, position = Points[i] });
                    }
                    else if (j != i)
                    {
                        if (i != 0)
                            dem = RouteTable[0][j].dem;
                        list.Add(j, new User { pos = i, posTo = j, km = getKm(Points[i], Points[j]), dem = dem, position = Points[i] });
                    }
                    
                }
                RouteTable.Add(i, list);
            }
            lines = getCombinations((byte)(pointsCount - 1));
        }

        public double getKm(Point from, Point to)
        {
            return Math.Sqrt(Math.Pow(from.X - to.X, 2) + Math.Pow(from.Y - to.Y, 2));
        }
        public double FitnessFunction(ArrayList ActualParameters)
        {
            int[] pointTmp = Encrypt(ActualParameters);
            double result = RouteTable[0][pointTmp[0]].km / RouteTable[0][pointTmp[0]].dem;
            for (int i = 1; i < pointTmp.Length; i++)
            {
                result += RouteTable[pointTmp[i - 1]][pointTmp[i]].km / RouteTable[pointTmp[i - 1]][pointTmp[i]].dem;
            }
            result += RouteTable[pointTmp[pointTmp.Length - 1]][0].km;
            return result;
        }

        public int[] Encrypt(ArrayList ActualParameters)
        {
            int index = (int)Math.Round((double)ActualParameters[0]);
            ulong line = lines[index];
            int[] result = new int[pCount - 1];
            for (int i = 0; i < pCount - 1; i++)
            {
                result[i] = (int)(line >> (bitLength * i) & stepMask);
            }
            return result;
        }

        private List<ulong> getCombinations(byte pointsCount)
        {
            List<ulong> combinations = new List<ulong>();
            ulong line = 0;
            for (int i = 8; i < 9 * 8; i = i * 2)
            {
                int tmp = i / pointsCount;
                bitLength = (int)(Math.Log(pointsCount, 2)) + 1;
                if (tmp >= bitLength)
                {
                    for (byte j = 0; j < bitLength; j++)
                    {
                        stepMask |= (byte)(1 << j);//Léptetési Maszk
                    }
                    for (int j = 1; j < pointsCount + 1; j++)
                    {
                        line |= (ulong)j << (bitLength * (j - 1));//Start kombináció
                    }
                    break;
                }
            }

            if(bitLength == 0 || combinations == null)
            {
                throw new ArgumentException("The \"pointsCount\" parameter too big or too small");
            }
            
            for (int i = 0; i < pointsCount; i++)
            {
                for (int j = 1; j < pointsCount; j++)
                {
                    ulong right = (ulong)(line & (stepMask << (bitLength * (j - 1))));//Jobb oldali érték kivétele
                    ulong left = (ulong)(line & (stepMask << (bitLength * j)));//Bal oldali érték kivétele

                    line &= (ulong)(~(stepMask << (bitLength * (j - 1))));//Jobb oldali nullázás
                    line &= (ulong)(~(stepMask << (bitLength * j)));//Bal oldal nullázás

                    line |= (ulong)(right << bitLength);//Jobb érték beillesztés
                    line |= (ulong)(left >> bitLength);//Bal érték beillesztés

                    combinations.Add(line);
                }
            }
            return combinations;
        }
    }
}
