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

        private Dictionary<int, Dictionary<int, User>> rout;

        public List<byte> lines = new List<byte>();//Max 15 points

        public int bitLength = 0;

        public byte stepMask = 0;

        public int pCount;

        private Random RNG = new Random();
        public RoutingTest(int pointsCount)
        {
            pCount = pointsCount;
            Dictionary<int, User> list;
            rout = new Dictionary<int, Dictionary<int, User>>();
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
            lines = getCombinations((byte)(pointsCount - 1));
        }
        public double FitnessFunction(ArrayList ActualParameters)
        {
            int[] pointTmp = Encrypt(ActualParameters);
            double result = rout[0][pointTmp[0]].km / rout[0][pointTmp[0]].dem;
            for (int i = 1; i < pointTmp.Length; i++)
            {
                result += rout[pointTmp[i - 1]][pointTmp[i]].km / rout[pointTmp[i - 1]][pointTmp[i]].dem;
            }
            result += rout[pointTmp[pointTmp.Length - 1]][0].km;
            return result;
        }

        private int[] Encrypt(ArrayList ActualParameters)
        {
            int index = (int)Math.Round((double)ActualParameters[0]);
            byte line = lines[index];
            int[] result = new int[pCount - 1];
            for (int i = 0; i < pCount - 1; i++)
            {
                result[i] = line >> (bitLength * i) & stepMask;
            }
            return result;
        }

        private List<byte> getCombinations(byte pointsCount)
        {
            List<byte> combinations = new List<byte>();
            byte line = 0;
            for (int i = 8; i < 9 * 8; i = i * 2)
            {
                int tmp = i / pointsCount;
                bitLength = (int)(Math.Log(pointsCount, 2)) + 1;
                if (tmp >= bitLength)
                {
                    for (byte j = 0; j < bitLength; j++)
                    {
                        stepMask |= (byte)(1 << j);
                    }
                    for (int j = 1; j < pointsCount + 1; j++)
                    {
                        line |= (byte)(j << (bitLength * (j - 1)));
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
                    byte right = (byte)(line & (stepMask << (bitLength * (j - 1))));
                    byte left = (byte)(line & (stepMask << (bitLength * j)));
                    line &= (byte)(~(stepMask << (bitLength * j)));
                    line &= (byte)(~(stepMask << (bitLength * (j - 1))));
                    line |= (byte)(right << bitLength);
                    line |= (byte)(left >> bitLength);
                    combinations.Add(line);
                }
            }
            return combinations;
        }
    }
}
