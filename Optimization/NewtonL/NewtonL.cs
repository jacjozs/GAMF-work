using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
        public List<NewtonPoint> HistoryPositions { get; set; }
        /// <summary>
        /// A lépés egység mérete
        /// </summary>
        public double[] Unit { get; set; }
        /// <summary>
        /// A jobb  és bal oldali lépés összege
        /// </summary>
        public int MaxStep { get; set; }
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
        /// 7 tizedes pontosági konstans
        /// Az egyenlöség ellenörzéséhez szükséges
        /// </summary>
        private const double _7 = 0.0000001;
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
            int Count = parameters.Count;
            Unit = new double[D - 1];
            breaking = true;
            Dimension = D;
            Positions = new List<Tuple<NewtonPoint, double>>();
            foreach (NewtonPoint item in parameters)
            {
                Fitness = item[D - 1];
                item.Points.RemoveAt(D - 1);
                Positions.Add(Tuple.Create(item, Fitness));
            }
            switch (D)
            {
                case 1:
                case 2:
                    MaxStep = Count;
                    break;
                default:
                    MaxStep = (int)Math.Sqrt(Count);
                    break;

            }

            ///Azon pontok törlése amik ki esnek a "négyzetből"s
            if (Count % MaxStep != 0)
                Positions.RemoveRange((Count - (Count % MaxStep)) - 1, Count % MaxStep);

            ActualPosition = new NewtonPoint(startP);
            ActualFitness = startP[D - 1];
            ActualPosition.Points.RemoveAt(D - 1);
            ///A 0 és az első változott értékel rendelkező koordinátákat kivonjuk egymásból és a kapot abszolult érték a lépések egysége
            ///Feltételezük hogy a map aktuális síkjában (X sík például) a pontok egyforma távolságra vannak egymástól
            for (int i = 0; i < ((D == 1) ? 1 : D - 1); i++)
            {
                double StartElem = Positions[0].Item1[i];
                Unit[i] = Math.Abs(Positions.Find(x => x.Item1[i] > StartElem).Item1[i] - StartElem);
            }
        }
        /// <summary>
        /// Maga a keresési folyamat
        /// </summary>
        /// <returns>A minimum pont koordinátáji</returns>
        public NewtonPoint MinSearch()
        {
            double FullUnit;
            //Oldalankénti lépések száma
            double[] M1FullStep = new double[Dimension - 1];
            double[] M2FullStep = new double[Dimension - 1];
            double[] HistoryFitness = new double[Dimension - 1];
            NewtonPoint M1;
            NewtonPoint M2;
            int Hole = 0;
            //jobb vagy ball oldali keresés volt e
            //ez általábban csak akkor szükséges ha csak az egyik oldalon volt keresés
            bool left, right;
            Valid = new List<double>();
            //ugrás méretének a redukálásának a mérete
            int[] M1Reduct = new int[Dimension - 1];
            int[] M2Reduct = new int[Dimension - 1];
            for (int i = 0; i < Dimension - 1; i++)
            {
                M1Reduct[i] = 1;
                M2Reduct[i] = 1;
            }
            //Redukció mértékének a szorzata
            int Down = 1;
            HistoryPositions = new List<NewtonPoint>();
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
                    M1 = new NewtonPoint(ActualPosition);
                    M2 = new NewtonPoint(ActualPosition);
                    //Bal oldali maximális lépés száma
                    M1FullStep[i] = M1[i] / Unit[i] - M1Reduct[i];
                    if (M1FullStep[i] >= 10)
                        FullUnit = (int)(M1FullStep[i] / 5) * Unit[i]; 
                    else
                        FullUnit = Unit[i];
                    //Mintavételi pontok elmozditása az adott síkban
                    M1[i] = M1[i] - FullUnit;
                    //Jobb oldali maximális lépés száma
                    M2FullStep[i] = (MaxStep - (M2[i] / Unit[i]) - 1) - M2Reduct[i];
                    if (M2FullStep[i] >= 10)
                        FullUnit = (int)(M2FullStep[i] / 5) * Unit[i];
                    else
                        FullUnit = Unit[i];
                    //Mintavételi pontok elmozditása az adott síkban
                    M2[i] = M2[i] + FullUnit;

                    //bal oldali pont keresése
                    double max = Positions.Where(x => x.Item1.Equals(M1, i, true)).Max(x => x.Item1[i]);
                    double min = Positions.Where(x => x.Item1.Equals(M1, i, true)).Min(x => x.Item1[i]);
                    if (M1[i] <= max && M1[i] >= min) 
                    {
                        Valid.Add(Positions.Find(x => x.Item1.Equals(M1)).Item2);
                        left = true;
                    }
                    //jobb oldali pont keresése
                    max = Positions.Where(x => x.Item1.Equals(M2, i, true)).Max(x => x.Item1[i]);
                    min = Positions.Where(x => x.Item1.Equals(M2, i, true)).Min(x => x.Item1[i]);
                    if (M2[i] <= max && M2[i] >= min) 
                    {
                        Valid.Add(Positions.Find(x => x.Item1.Equals(M2)).Item2);
                        right = true;
                    }
                    //jobb és bal oldali értékek összehasonlítása
                    if (Valid.Count > 1)
                    {
                        //Ha az aktuális síkban egy minimum pontban vanna akkor megnöveli a Hole értéket
                        if (Valid[0] > ActualFitness && Valid[1] > ActualFitness)
                        {
                            Hole++;
                            continue;
                        }
                        if (Valid[0] < Valid[1] && Valid[0] < ActualFitness)
                        {
                            ActualFitness = Valid[0];
                            ActualPosition = M1;
                            //túlugrás ellenörzése
                            if (HistoryFitness[i] < ActualFitness)
                            {
                                M2FullStep[i] = M2FullStep[i] / M1FullStep[i];
                            }
                        }
                        else if(Valid[0] > Valid[1] && Valid[1] < ActualFitness)
                        {
                            ActualFitness = Valid[1];
                            ActualPosition = M2;
                            //túlugrás ellenörzése
                            if (M1FullStep[i] > M2FullStep[i])
                            {
                                M1FullStep[i] = M1FullStep[i] / M2FullStep[i];
                            }
                        }
                        //Ha "gödörben" van és két megegyező pont közt ugrál akkor lép ez életbe
                        else if (Math.Abs(Valid[0] - Valid[1]) < _7)
                        {
                            Down++;
                        }
                    }
                    else if (Valid.Count == 1 && Valid[0] < ActualFitness)
                    {
                        ActualFitness = Valid[0];
                        ActualPosition = left ? M1 : (right ? M2 : null);
                    }
                    HistoryFitness[i] = ActualFitness;
                    M1Reduct[i] = M1Reduct[i] * Down;
                    M2Reduct[i] = M2Reduct[i] * Down;
                }
                if (Hole == Dimension - 1)
                {
                    ActualPosition.Points.Add(ActualFitness);
                    return ActualPosition;
                }
                //felesleges elemek eltávolítása a listából
                if (HistoryPositions.Count > 5)
                    HistoryPositions.RemoveRange(0, HistoryPositions.Count - 5);
                //ha az utolsó 5 eredmény ugyan az akkor az eredmény véglegesnek tekintett
                for (int i = 0; i < 5 && HistoryPositions.Count >= 5; i++)
                {
                    if (HistoryPositions[HistoryPositions.Count - (i + 1)].Equals(ActualPosition)) EqGen++;
                    switch (EqGen)
                    {
                        case 3:
                            Down++;
                            break;
                        case 5:
                            breaking = false;
                            break;
                    }
                }
                EqGen = 0;
                Hole = 0;
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
        const double _7 = 0.0000001;
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
            return Equals(objAsPart);
        }
        public bool Equals(NewtonPoint other)
        {
            if(other == null) return false;
            for (int i = 0; i < other.Points.Count; i++)
            {
                if (Math.Abs(Points[i] - other.Points[i]) > _7) return false;
            }
            return true;
        }
        /// <summary>
        /// Szelektív összehasonlítás
        /// </summary>
        /// <param name="other">Aktuális pont</param>
        /// <param name="pos">Az az index amit ki kell hagyni vagy ameddig meg kell nézni</param>
        /// <param name="IsPosit">Ha true akkor kihagyás ha false akkor részleges</param>
        /// <returns></returns>
        public bool Equals(NewtonPoint other, int pos, bool IsPosit)
        {
            if (other == null) return false;
            for (int i = 0; i < other.Points.Count && IsPosit; i++)
            {
                if(i == pos) continue;
                if (Math.Abs(Points[i] - other.Points[i]) > _7) return false;
            }
            for (int i = 0; i < pos && !IsPosit; i++)
            {
                if (Math.Abs(Points[i] - other.Points[i]) > _7) return false;
            }
            return true;
        }
    }
}
