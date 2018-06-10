using System;
using System.Collections;
using System.Linq;
using System.Windows;

namespace Optimization
{
    /// <summary>
    /// Az algoritmus csak 2 és 3 dimenziós környezetben volt tesztelve és ott is még csak lineáris emelkedőn!
    /// Feladatok: (fejlesztés)
    /// Egyértelműbb megszakitás!
    /// Kivétel kezelések!
    /// Olcsóbb logikai menet! (Optimalizálás)
    /// Leegyszerűsítés!
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
        private NewtonPoint ActualPosition { get; set; }
        /// <summary>
        /// Az utovonal pontjai
        /// </summary>
        public ArrayList HistoryPositions { get; set; }
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
        private ArrayList Valid { get; set; }
        /// <summary>
        /// Keresés konstruktora ami betölti az datokat
        /// !!!Azt feltételazi a kód hogy a megadott pontok listálya azonos egység távolságra vannak egymástól!!!
        /// </summary>
        /// <param name="parameters">Azon pontok listája amiböl a "map" (térkép) áll</param>
        /// <param name="startP">Indulási pont kordinátáji (x,y,z,...)</param>
        /// <param name="D">Dimenziók száma</param>
        public NewtonL(ArrayList parameters, NewtonPoint startP, int D)
        {
            breaking = true;
            Dimension = D;
            Positions = parameters;
            ActualPosition = new NewtonPoint(startP);
            MaxSize = ((NewtonPoint)Positions[Positions.Count - 1]).Points[0];
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
            HistoryPositions = new ArrayList();
            //egyforma koordináták száma
            int EqGen = 0;
            while (breaking)
            {
                for (int i = 0; i < Dimension - 1; i++)
                {
                    HistoryPositions.Add(ActualPosition);
                    Valid.Clear();
                    //FullUnit = (((MaxSize / Unit) - (StartPosition.Points[i] / Unit)) / 5) * Unit;
                    //Mintavételi pontok létrehozása és az aktuális pont értékének a felvétele
                    NewtonPoint M1 = new NewtonPoint(ActualPosition);
                    NewtonPoint M2 = new NewtonPoint(ActualPosition);
                    //Mintavételi pontok elmozditása az adott síkban
                    M1.Points[i] = M1.Points[i] - FullUnit;
                    M2.Points[i] = M2.Points[i] + FullUnit;

                    //bal oldali pont keresése
                    if (M1.Points[i] >= 0 && !(M1.Points[i] > MaxSize || M1.Points[i] < 0))
                    {
                        Valid.Add((from NewtonPoint points in Positions where points.Equals(M1, Dimension - 1) select points).First());
                    }
                    //jobb oldali pont keresése
                    if (M2.Points[i] >= 0 && !(M2.Points[i] > MaxSize || M2.Points[i] < 0))
                    {
                        Valid.Add((from NewtonPoint points in Positions where points.Equals(M2, Dimension - 1) select points).First());
                    }

                    //jobb és bal oldali értékek összehasonlítása
                    if (Valid.Count > 1)
                    {
                        //A Dimension - 1 az az index hely amin az érték található
                        //2D = 2-1=1 Points[1]=Y,  3D = 3-1=2 Points[2]=Z
                        if (((NewtonPoint)Valid[0]).Points[Dimension - 1] < ((NewtonPoint)Valid[1]).Points[Dimension - 1] 
                            && ((NewtonPoint)Valid[0]).Points[Dimension - 1] < ActualPosition.Points[Dimension - 1])
                            ActualPosition = (NewtonPoint)Valid[0];
                        else
                            if (((NewtonPoint)Valid[1]).Points[Dimension - 1] < ActualPosition.Points[Dimension - 1])
                            ActualPosition = (NewtonPoint)Valid[1];
                    }
                    else
                    {
                        if (Valid.Count != 0 && ((NewtonPoint)Valid[0]).Points[Dimension - 1] < ActualPosition.Points[Dimension - 1])
                            ActualPosition = (NewtonPoint)Valid[0];
                    }
                }
                //ha az utolsó 3 eredmény ugyan az akkor az eredmény véglegesnek tekintett
                for (int i = 0; i < 3 && HistoryPositions.Count >= 3; i++)
                {
                    if (HistoryPositions[HistoryPositions.Count - 1] != null && HistoryPositions[HistoryPositions.Count - (i + 1)].Equals(ActualPosition)) EqGen++;
                    if (EqGen == 3) { breaking = false; break; }
                }
                EqGen = 0;
            }
            return ActualPosition;
        }
    }
    /// <summary>
    /// NewtonPoint osztály a koordináták és az összehasonlitás használatára
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
        /// 4 tizedesjegy pontosság
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Equals(NewtonPoint point)
        {
            for (int i = 0; i < point.Points.Length; i++)
            {
                if (Math.Round(this.Points[i], 4) != Math.Round(point.Points[i], 4)) return false;
            }
            return true;
        }
        /// <summary>
        /// Egyenlőség elenörzése szelektálva
        /// Általábban a dimenziók számát kell megadni
        /// 4 tizedesjegy pontosság
        /// </summary>
        /// <param name="point"></param>
        /// <param name="Count">elsőtöl számitot hány elemet akarunk összehasonlítani</param>
        /// <returns></returns>
        public bool Equals(NewtonPoint point, int Count)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Math.Round(this.Points[i], 4) != Math.Round(point.Points[i], 4)) return false;
            }
            return true;
        }
    }
}
