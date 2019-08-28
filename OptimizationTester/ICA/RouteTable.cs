using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OptimizationTester.ICA
{
    public class RouteTable
    {
        /// <summary>
        /// Routing tábla
        /// </summary>
        public Dictionary<int, Dictionary<int, User>> routeTable;
        /// <summary>
        /// Poziciók
        /// </summary>
        public List<Point> Points = new List<Point>();
        public List<int> pathList;
        /// <summary>
        /// Pontok száma
        /// </summary>
        private int pCount;
        /// <summary>
        /// Random szám generátor
        /// </summary>
        private Random RNG = new Random();
        /// <summary>
        /// Konstruktor ami a pontok száma alapján legenerálja
        /// A random útvonalú routing táblát a kereséshez
        /// </summary>
        /// <param name="pointsCount">pontok száma</param>
        public RouteTable(int pointsCount)
        {
            pCount = pointsCount;
            Dictionary<int, User> list;
            routeTable = new Dictionary<int, Dictionary<int, User>>();
            pathList = new List<int>();
            int dem = 0;
            for (int i = 0; i < pointsCount + 1; i++)
            {
                Point pos = new Point(RNG.NextDouble() * 150, RNG.NextDouble() * 150);
                Points.Add(pos);
            }
            for (int i = 0; i < pointsCount + 1; i++)
            {
                list = new Dictionary<int, User>();
                for (int j = 0; j < pointsCount + 1; j++)
                {
                    dem = RNG.Next(1, 5);

                    if (j == 0) dem = 1;
                    if (routeTable.ContainsKey(j))
                    {
                        list.Add(j, new User { pos = i, posTo = j, km = routeTable[j][i].km, dem = dem, position = Points[i] });
                    }
                    else if (j != i)
                    {
                        if (i != 0)
                            dem = routeTable[0][j].dem;
                        list.Add(j, new User { pos = i, posTo = j, km = getKm(Points[i], Points[j]), dem = dem, position = Points[i] });
                    }

                }
                routeTable.Add(i, list);
                pathList.Add(i);
            }
        }
        /// <summary>
        /// Két pont közti távolságót számolja ki
        /// (Mivel nem a pontok sima kordináták ezért azok vektori távolságát számoljuk)
        /// </summary>
        /// <param name="from">kiindulási pont</param>
        /// <param name="to">érkezési pont</param>
        /// <returns>két pont közti vektori távolság</returns>
        public double getKm(Point from, Point to)
        {
            return Math.Sqrt(Math.Pow(from.X - to.X, 2) + Math.Pow(from.Y - to.Y, 2));
        }
        /// <summary>
        /// Fitness function
        /// </summary>
        /// <param name="ActualParameters">Paraméterek</param>
        /// <returns>Fitness értéke</returns>
        public double FitnessFunction(ArrayList ActualParameters)
        {
            ArrayList pointTmp = Encrypt(ActualParameters);
            double result = routeTable[0][(int)pointTmp[0]].km / routeTable[0][(int)pointTmp[0]].dem;
            for (int i = 1; i < pointTmp.Count; i++)
            {
                result += routeTable[(int)pointTmp[i - 1]][(int)pointTmp[i]].km / routeTable[(int)pointTmp[i - 1]][(int)pointTmp[i]].dem;
            }
            result += routeTable[(int)pointTmp[(int)pointTmp.Count - 1]][0].km;
            return result;
        }
        /// <summary>
        /// Az algoritmusok által generált paraméterek dekódolása és azok átalakítása
        /// a pozíciók sorenbe helyezése
        /// </summary>
        /// <param name="ActualParameters">A generált paraméterek</param>
        /// <returns>A dekodólt pontokból álló lista</returns>
        public ArrayList Encrypt(ArrayList ActualParameters)
        {
            return this.Encrypt_0(ActualParameters);
        }

        public ArrayList Encrypt_0(ArrayList ActualParameters)
        {
            ArrayList result = new ArrayList();

            for (int i = 0; i < ActualParameters.Count; i++)
            {
                int value;
                if (ActualParameters[i] is double)
                {
                    value = Math.Abs((int)Math.Round((double)ActualParameters[i] % this.pathList.Count));
                }
                else
                {
                    value = Math.Abs(((int)ActualParameters[i] % this.pathList.Count));
                }
                if (!result.Contains(this.pathList[value]))
                {
                    result.Add(this.pathList[value]);
                }
                else
                {
                    result.Add(getCloseFreeValue(result, value));
                }
            }
           return result;
        }
        public ArrayList Encrypt_1(ArrayList ActualParameters)
        {
            ArrayList result = new ArrayList();
            for (int i = 0; i < ActualParameters.Count; i++)
            {
                int value;
                if (ActualParameters[i] is double)
                {
                    value = (int)Math.Round(((double)ActualParameters[i] + (double)ActualParameters[(i + 1) % pCount]) % pCount) + 1;
                }
                else
                {
                    value = (((int)ActualParameters[i] + (int)ActualParameters[(i + 1) % pCount]) % pCount) + 1;
                }
                if (!result.Contains(value) && this.pathList.Contains(value))
                {
                    result.Add(value);
                }
                else
                {
                    result.Add(getCloseFreeValue(result, value));
                }
            }
            return result;
        }
        public ArrayList Encrypt_2(ArrayList ActualParameters)
        {
            ArrayList result = new ArrayList();
            ulong line = 0;
            for (int i = 0; i < ActualParameters.Count; i++)
            {
                if(ActualParameters[i] is double)
                {
                    line |= BitConverter.ToUInt64(BitConverter.GetBytes((double)ActualParameters[i]), 0);
                }
                else
                {
                    line |= BitConverter.ToUInt64(BitConverter.GetBytes((int)ActualParameters[i]), 0);
                }
            }
            for (int i = 0; i < 64; i++)
            {
                if ((line & ((ulong)1 << i)) > 0 && !result.Contains(i % this.pathList.Count + 1) && this.pathList.Contains(i % this.pathList.Count + 1))
                {
                    result.Add(i % this.pathList.Count + 1);
                    if (result.Count == this.pathList.Count) break;
                }
            }
            if (this.pathList.Count != result.Count)
            {
                checkNULL(result);
            }
            return result;
        }
        /// <summary>
        /// A megkapot listába beilleszteni a hiányzó pontokat
        /// Növekvő sorrenben kerülnek bele
        /// </summary>
        /// <param name="result">A vizsgált lista</param>
        private void checkNULL(ArrayList result)
        {
            for (int i = 1; i < this.pathList.Count + 1; i++)
            {
                if (!result.Contains(i))
                    result.Add(i);
                if (result.Count == this.pathList.Count) break;
            }
        }
        /// <summary>
        /// Megkeresi a legközelebbi szabad értéket
        /// </summary>
        /// <param name="result"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private int getCloseFreeValue(ArrayList result, int value)
        {
            for (int i = 1; i < this.pathList.Count; i++)
            {
                int value_1 = ((value + i) % this.pathList.Count);//plus
                int value_2 = Math.Abs((value - i) % this.pathList.Count);//minus
                if (!result.Contains(this.pathList[value_1]))
                {
                    return this.pathList[value_1];
                }
                if (!result.Contains(this.pathList[value_2]))
                {
                    return this.pathList[value_2];
                }
            }
            return -1;
        }
    }
}
