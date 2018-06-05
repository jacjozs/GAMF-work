using System;
using System.Collections;
using System.Linq;

namespace Optimization
{
    /// <summary>
    /// Az algoritmus csak 2 és 3 dimenziós környezetben volt tesztelve és ott is még csak lineáris emelkedőn!
    /// Feladatok (fejlesztés)
    /// Egyértelműbb megszakitás!
    /// Kivétel kezelések!
    /// Olcsóbb logikai menet! (Optimalizálás)
    /// Legyszerüsités!
    /// </summary>
    public class NewtonL
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
        NewtonPoint StartPosition { get; set; }
        /// <summary>
        /// Az előző koordináta
        /// </summary>
        NewtonPoint HistoryPosition { get; set; }
        /// <summary>
        /// A lépés egység mérete
        /// </summary>
        public double Unit { get; set; }
        /// <summary>
        /// A maximális méret amin nem mehet túl a keresés
        /// (A "map" szélei)
        /// </summary>
        public double MaxSize { get; set; }
        /// <summary>
        /// Dimenziók száma
        /// </summary>
        public int Dimension { get; set; }
        /// <summary>
        /// Az indulási irány eldöntéséhez szükséges
        /// Itt tárolodnak a jobb és bal oldali pont adatai
        /// </summary>
        public ArrayList Valid { get; set; }
        /// <summary>
        /// Keresés konstruktora ami betölti az datokat
        /// !!!Azt feltételazi a kód hogy a megadott pontok listálya azonos egység távolságra vannak egymástól!!!
        /// </summary>
        /// <param name="parameters">Azon pontok listája amiböl a "map" (térkép) áll</param>
        /// <param name="startP">Indulási pont kordinátáji (x,y,z,...)</param>
        /// <param name="d">Dimenziók száma</param>
        public NewtonL(ArrayList parameters, double[] startP, int d)
        {
            breaking = true;
            Dimension = d;
            Positions = parameters;
            if (d > 2)
                MaxSize = Math.Pow(Positions.Count, 1.0 / d);
            else
                MaxSize = ((NewtonPoint)Positions[Positions.Count - 1]).Points[0];
            StartPosition = new NewtonPoint(startP);
            ///Az első (az 1 indexű) elem X kordinátájának a 0-tól vett távolsága az egység ugrás
            ///Ha az X 0 akkor az Y értékét veszi fel
            ///Feltételezük hogy a map pontjai egyforma távolságra vannak egymástól
            Unit = ((NewtonPoint)Positions[1]).Points[0] == 0 ? ((NewtonPoint)Positions[1]).Points[1] : ((NewtonPoint)Positions[1]).Points[0];

        }
        /// <summary>
        /// Maga a keresési folyamat
        /// </summary>
        /// <returns>A minimum pont koordinátáji</returns>
        public NewtonPoint MinSearch()
        {
            double FullUnit = Unit;
            Valid = new ArrayList();
            //egyforma koordináták száma
            int EqGen = 0;
            while (breaking)
            {
                for (int i = 0; i < Dimension - 1; i++)
                {
                    Valid.Clear();
                    //Mintavételi pontok létrehozása és az aktuális pont értékének a felvétele
                    NewtonPoint M1 = new NewtonPoint(StartPosition);
                    NewtonPoint M2 = new NewtonPoint(StartPosition);
                    //Mintavételi pontok elmozditása az adott síkban
                    M1.Points[i] = M1.Points[i] - FullUnit;
                    M2.Points[i] = M2.Points[i] + FullUnit;

                    //bal oldali pont keresése
                    if (M1.Points[i] >= 0)
                    {
                        NewtonPoint M1Val = PointSearch(M1);
                        if (M1Val != null) Valid.Add(M1Val);
                    }
                    //jobb oldali pont keresése
                    if (M2.Points[i] >= 0)
                    {
                        NewtonPoint M2Val = PointSearch(M2);
                        if (M2Val != null) Valid.Add(M2Val);
                    }

                    //jobb és bal oldali értékek összehasonlítása
                    if (Valid.Count > 1)
                    {
                        ///A Dimension - 1 az az index hely amin az érték található
                        ///2D = 2-1=1 Points[1]=Y,  3D = 3-1=2 Points[2] Z
                        if (((NewtonPoint)Valid[0]).Points[Dimension - 1] < ((NewtonPoint)Valid[1]).Points[Dimension - 1] && ((NewtonPoint)Valid[0]).Points[Dimension - 1] < StartPosition.Points[Dimension - 1])
                            StartPosition = (NewtonPoint)Valid[0];
                        else
                            if (((NewtonPoint)Valid[1]).Points[Dimension - 1] < StartPosition.Points[Dimension - 1])
                            StartPosition = (NewtonPoint)Valid[1];
                    }
                    else
                        if(Valid.Count != 0)
                        StartPosition = (NewtonPoint)Valid[0];
                }
                //ha az előző 3 eredmény ugyan az akkor az eredmény véglegesnek tekintet
                if (HistoryPosition != null && HistoryPosition.Equals(StartPosition)) EqGen++;
                HistoryPosition = new NewtonPoint(StartPosition);
                if (EqGen > 3) break;
            }
            return StartPosition;
        }
        /// <summary>
        /// Megkeresi az adott koordinátában található értéket vagyis hogy hol található érték
        /// 2D = Y, 3D = Z
        /// </summary>
        /// <param name="Point">Az aktuális koordináta</param>
        /// <returns>a talált koordináta</returns>
        NewtonPoint PointSearch(NewtonPoint Point)
        {
            NewtonPoint M11 = new NewtonPoint(Point);
            NewtonPoint M21 = new NewtonPoint(Point);
            while (true)
            {
                var query = from NewtonPoint points in Positions where points.Equals(M11) || points.Equals(M21) select points;
                foreach (var item in query)
                    return item;
                if (M11.Points[Dimension - 1] >= 300 || M11.Points[Dimension - 1] <= 0 || M21.Points[Dimension - 1] >= 300 || M21.Points[Dimension - 1] <= 0) return null;
                M11.Points[Dimension - 1] = M11.Points[Dimension - 1] - Unit;
                M21.Points[Dimension - 1] = M21.Points[Dimension - 1] + Unit;
            }
        }
    }
    /// <summary>
    /// point osztály a kordináták használatára
    /// </summary>
    public class NewtonPoint : IEquatable<NewtonPoint>
    {
        /// <summary>
        /// Koordinátákat tároló tömb
        /// </summary>
        public double[] Points { get; set; }
        public NewtonPoint(double[] point)
        {
            this.Points = point;
        }
        public NewtonPoint(NewtonPoint point)
        {
            Points = new double[point.Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = point.Points[i];
            }
        }
        /// <summary>
        /// Egyenlőség ellenörzése
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Equals(NewtonPoint point)
        {
            for (int i = 0; i < point.Points.Length; i++)
            {
                if (this.Points[i] != point.Points[i]) return false;
            }
            return true;
        }
    }
}
