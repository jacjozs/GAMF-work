using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.NewtonL
{
    /// <summary>
    /// Feladatok (fejlesztés)
    /// Megszakitás!
    /// Kivétel kezelések!
    /// Olcsóbb logikai menet (Optimalizálás)
    /// </summary>
    class NewtonL
    {
        /// <summary>
        /// Folyamat megszakító
        /// </summary>
        private bool breaking;
        /// <summary>
        /// A "map" pontjai (koordinátáji)
        /// </summary>
        public ArrayList Positions { get; set; }
        /// <summary>
        /// Az épp aktuális pozició (koordinátája)
        /// </summary>
        public point StartPosition { get; set; }
        /// <summary>
        /// A lépés mérete
        /// </summary>
        public int Unit { get; set; }
        /// <summary>
        /// Az indulási irány eldöntéséhez szükséges
        /// Itt tárolodnak a jobb és bal oldali pont adatai
        /// </summary>
        public ArrayList Valid { get; set; }
        /// <summary>
        /// Keresési folyamat
        /// </summary>
        /// <param name="parameters">Azon pontok listája amiböl a "map" (térkép) áll</param>
        /// <param name="startP">Indulási pont kordinátáji (x,y,z,...)</param>
        /// <param name="dimension">Dimenziók száma</param>
        public NewtonL(ArrayList parameters, int[] startP, int dimension)
        {
            breaking = true;
            //UnitL = Bal oldali, UnitR = jobb oldali
            int UnitL = 1, UnitR = 1;
            Valid = new ArrayList();
            Positions = parameters;
            StartPosition = new point(startP);
            while (breaking)
            {
                for (int i = 0; i < dimension - 1; i++)
                {
                    //összehasonlítási elemek törlése
                    Valid.Clear();
                    //jobb és bal oldali lépések kiszámolása majd a minimumal való tovább számolás
                    UnitL = ((StartPosition.Point[i] / 5) == 0) ? 1 : StartPosition.Point[i] / 5;
                    UnitR = ((100 - StartPosition.Point[i] / 5) == 0) ? 1 : (100 - StartPosition.Point[i]) / 5;
                    Unit = new int[] { UnitL, UnitR }.Min();
                    //jobb és ball oldali elemek kikeresése
                    var query = from point points in Positions where points.Point[i] == StartPosition.Point[i] - Unit || points.Point[i] == StartPosition.Point[i] + Unit select points;
                    //[0] ball oldali elem, [1] jobb oldali elem
                    foreach (var item in query)
                        Valid.Add(item);
                    //Összehasonlitások
                    if (((point)Valid[0]).Point[i + 1] < ((point)Valid[1]).Point[i + 1] && ((point)Valid[0]).Point[i + 1] < StartPosition.Point[i + 1])
                        StartPosition = (point)Valid[0];
                    else
                        if (((point)Valid[1]).Point[i + 1] < StartPosition.Point[i + 1])
                        StartPosition = (point)Valid[1];
                }
            }
        }
    }
    /// <summary>
    /// point osztály a kordináták használatára
    /// </summary>
    class point
    {
        public int[] Point { get; set; }
        public point(int[] point)
        {
            this.Point = point;
        }
    }
}
