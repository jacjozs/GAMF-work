using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public List<Tuple<NewtonPoint, double>> Positions { get; set; }
        /// <summary>
        /// Az épp aktuális pozició (koordinátája)
        /// </summary>
        private NewtonPoint ActualPosition { get; set; }
        /// <summary>
        /// Az épp aktuális fitness érték
        /// </summary>
        public double ActualFitness { get; set; }
        /// <summary>
        /// Az utovonal pontjai
        /// </summary>
        public ArrayList HistoryPositions { get; set; }
        /// <summary>
        /// A lépés egység mérete
        /// </summary>
        public double Unit { get; set; }
        /// <summary>
        /// A jobb  és bal oldali lépés összege
        /// </summary>
        public double MaxStep { get; set; }
        /// <summary>
        /// Azon tömbök listája amik a minimim és a maximumokat tárolják
        /// List[0] = X koordináta min/max ....
        /// tömb[0] = minimum tömb[1] = maximum
        /// </summary>
        private List<double[]> Sizes { get; set; }
        /// <summary>
        /// Dimenziók száma
        /// </summary>
        public int Dimension { get; set; }
        /// <summary>
        /// Az indulási irány eldöntéséhez szükséges
        /// Itt tárolodnak a jobb és bal oldali pont adatai
        /// </summary>
        private List<double> Valid { get; set; }
        /// <summary>
        /// Keresés konstruktora ami betölti az datokat
        /// !!!Azt feltételazi a kód hogy a megadott pontok listálya azonos egység távolságra vannak egymástól!!!
        /// </summary>
        /// <param name="parameters">Azon pontok listája amiböl a "map" (térkép) áll</param>
        /// <param name="startP">Indulási pont kordinátáji (x,y,z,...)</param>
        /// <param name="D">Dimenziók száma</param>
        public NewtonL(ArrayList parameters, NewtonPoint startP, int D)
        {
            double Fitness = 0.0;
            breaking = true;
            Dimension = D;
            Sizes = new List<double[]>();
            Positions = new List<Tuple<NewtonPoint, double>>();
            foreach (NewtonPoint item in parameters)
            {
                Fitness = item[D - 1];
                item.Points.RemoveAt(D - 1);
                Positions.Add(Tuple.Create(item, Fitness));
            }
            if (D > 3)
                MaxStep = Math.Pow(Positions.Count, (1 / (D - 1)));
            else if(D == 2)
                MaxStep = Positions.Count;
            else
                MaxStep = Math.Sqrt(Positions.Count);
            ///Az épp aktuális dimenzióhoz tartozó szélső értékek
            ///pl 1D = X, 2D = Y ...
            double MaxSize, MinSize;
            for (int i = 0; i < ((D == 1)? 1 : D - 1); i++)
            {
                MinSize = Positions.Min(x => x.Item1[i]);
                MaxSize = Positions.Max(x => x.Item1[i]);
                Sizes.Add(new double[] { MinSize, MaxSize });
            }
            ActualPosition = new NewtonPoint(startP);
            ActualFitness = startP[D - 1];
            ActualPosition.Points.RemoveAt(D - 1);
            ///Az első és a második elem érték elötti tagját kivonjuk egymásból és a kapot abszolult érték a lépések egysége
            ///Feltételezük hogy a map pontjai egyforma távolságra vannak egymástól
            Unit = Math.Abs(Positions[1].Item1[((D == 1) ? 0 : D - 2)] - Positions[0].Item1[((D == 1) ? 0 : D - 2)]);
        }
        /// <summary>
        /// Maga a keresési folyamat
        /// </summary>
        /// <returns>A minimum pont koordinátáji</returns>
        public NewtonPoint MinSearch()
        {
            double FullUnit = Unit;
            //jobb vagy ball oldali keresés volt e
            //ez általábban csak akkor szükséges ha csak az egyik oldalon volt keresés
            bool left, right;
            Valid = new List<double>();
            //ugrás méretének a redukálásának a mérete
            int Down = 0;
            HistoryPositions = new ArrayList();
            //egyforma koordináták száma
            int EqGen = 0;
            while (breaking)
            {
                for (int i = 0; i < ((Dimension == 1) ? 1 : Dimension - 1); i++)
                {
                    HistoryPositions.Add(ActualPosition);
                    Valid.Clear();
                    left = false; right = false;
                    //Mintavételi pontok létrehozása és az aktuális pont értékének a felvétele
                    NewtonPoint M1 = new NewtonPoint(ActualPosition);
                    NewtonPoint M2 = new NewtonPoint(ActualPosition);
                    //Bal oldali maximális lépés száma
                    FullUnit = M1[i] / Unit - (Down / 2);
                    if (FullUnit > 5)
                        FullUnit = (int)(FullUnit / 5) * Unit;
                    else
                        FullUnit = Unit;
                    //Mintavételi pontok elmozditása az adott síkban
                    M1[i] = M1[i] - FullUnit;
                    //Jobb oldali maximális lépés száma
                    FullUnit = (MaxStep - (M2[i] / Unit) - 1) - (Down / 2);
                    if (FullUnit > 5)
                        FullUnit = (int)(FullUnit / 5) * Unit;
                    else
                        FullUnit = Unit;
                    //Mintavételi pontok elmozditása az adott síkban
                    M2[i] = M2[i] + FullUnit;

                    //bal oldali pont keresése
                    if (M1[i] <= Sizes[i][1] && M1[i] >= Sizes[i][0]) 
                    {
                        Valid.Add(Positions.Find(x => x.Item1.Equals(M1)).Item2);
                        left = true;
                    }
                    //jobb oldali pont keresése
                    if (M2[i] <= Sizes[i][1] && M2[i] >= Sizes[i][0]) 
                    {
                        Valid.Add(Positions.Find(x => x.Item1.Equals(M2)).Item2);
                        right = true;
                    }
                    //jobb és bal oldali értékek összehasonlítása
                    if (Valid.Count > 1)
                    {
                        if (Valid[0] < Valid[1] && Valid[0] < ActualFitness)
                        {
                            ActualFitness = Valid[0];
                            ActualPosition = M1;
                            Down++;
                        }
                        else if(Valid[0] > Valid[1] && Valid[1] < ActualFitness)
                        {
                            ActualFitness = Valid[1];
                            ActualPosition = M2;
                            Down++;
                        }
                    }
                    else if(Valid.Count == 1 && Valid[0] < ActualFitness)
                    {
                        ActualFitness = Valid[0];
                        ActualPosition = left ? M1 : (right ? M2 : null);
                        Down++;
                    }
                }
                //felesleges elemek eltávolítása a listából
                if (HistoryPositions.Count > 5)
                    HistoryPositions.RemoveRange(0, HistoryPositions.Count - 5);
                //ha az utolsó 3 eredmény ugyan az akkor az eredmény véglegesnek tekintett
                for (int i = 0; i < 5 && HistoryPositions.Count >= 5; i++)
                {
                    if (HistoryPositions[HistoryPositions.Count - 1] != null && HistoryPositions[HistoryPositions.Count - (i + 1)].Equals(ActualPosition)) EqGen++;
                    if (EqGen == 5) { breaking = false; break; }
                }
                EqGen = 0;
            }
            ActualPosition.Points.Add(ActualFitness);
            return ActualPosition;
        }
    }
    /// <summary>
    /// NewtonPoint osztály a koordináták és az összehasonlitás használatára
    /// </summary>
    public class NewtonPoint : IEquatable<NewtonPoint>
    {
        /// <summary>
        /// Koordinátákat tároló lista
        /// </summary>
        public List<double> Points { get; set; }
        /// <summary>
        /// 10 tizedes pontosági konstans
        /// Az egyenlöség ellenörzéséhez szükséges
        /// </summary>
        const double _10 = 0.0000000001;
        public double this[int index]
        {
            get { return Points[index]; }
            set  { Points[index] = value; }
        }
        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="point"></param>
        public NewtonPoint(double[] point)
        {
            this.Points = point.ToList();
        }
        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="point"></param>
        public NewtonPoint(NewtonPoint point)
        {
            Points = point.Points.ToList();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            NewtonPoint objAsPart = obj as NewtonPoint;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public bool Equals(NewtonPoint other)
        {
            if(other == null) return false;
            for (int i = 0; i < other.Points.Count; i++)
            {
                double size = Math.Abs(this.Points[i] - other.Points[i]);
                if (size > _10) return false;
            }
            return true;
        }
    }
}
