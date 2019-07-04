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
        /// <summary>
        /// Routing tábla
        /// </summary>
        public Dictionary<int, Dictionary<int, User>> RouteTable;
        /// <summary>
        /// Poziciók
        /// </summary>
        public List<Point> Points = new List<Point>();
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
        public RoutingTest(int pointsCount)
        {
            pCount = pointsCount;
            Dictionary<int, User> list;
            RouteTable = new Dictionary<int, Dictionary<int, User>>();
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
        /// <param name="ActualParameters"></param>
        /// <returns></returns>
        public double FitnessFunction(ArrayList ActualParameters)
        {
            ArrayList pointTmp = Encrypt(ActualParameters);
            double result = RouteTable[0][(int)pointTmp[0]].km / RouteTable[0][(int)pointTmp[0]].dem;
            for (int i = 1; i < pCount; i++)
            {
                result += RouteTable[(int)pointTmp[i - 1]][(int)pointTmp[i]].km / RouteTable[(int)pointTmp[i - 1]][(int)pointTmp[i]].dem;
            }
            result += RouteTable[(int)pointTmp[(int)pointTmp.Count - 1]][0].km;
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
            long line = 0;
            foreach (double item in ActualParameters)
            {
                line |= BitConverter.ToInt64(BitConverter.GetBytes(item), 0);
            }
            ArrayList result = new ArrayList();
            for (int i = 0; i < 64; i++)
            {
                if((line & (1 << i)) > 0 && !result.Contains(i % pCount + 1))
                {
                    result.Add(i % pCount + 1);
                    if (result.Count == pCount) break;
                }
            }
            if(pCount != result.Count)
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
            for (int i = 1; i < pCount + 1; i++)
            {
                if(!result.Contains(i))
                    result.Add(i);
                if (result.Count == pCount) break;
            }
        }
    }
}
