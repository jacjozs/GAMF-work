using System;
using System.Collections;

namespace Optimization
{
    public class AntAlg : BaseOptimizationMethod
    {
        /// <summary>
        /// Keresési kör sugara
        /// Default : 10
        /// </summary>
        public double R = 10;
        /// <summary>
        /// Hangya elemek tárolására
        /// </summary>
        private ArrayList Ants;
        /// <summary>
        /// Ez jelzi hogy első futás e vagy sem
        /// </summary>
        private bool isFirst = true;
        /// <summary>
        /// Feromon pontok tárolására
        /// </summary>
        private ArrayList FeromonPoints;
        /// <summary>
        /// A legjobb fitnesz érték
        /// </summary>
        private double BestFitness = double.MaxValue;
        protected override void CreateNextGeneration()
        {
            if(isFirst)
            {
                Ants = new ArrayList();
                for (int i = 0; i < NumberOfElements; i++)
                {
                    Ants.Add(new Ant((BaseElement)Elements[i]));
                }
                FeromonPoints = new ArrayList(); 
                isFirst = false;
            }
            //Azért kell a 5. legjobb eredmény mivel a legjobbat sokkal kisebb esélyel tudják felülmúlni
            if(((BaseElement)Elements[4]).Fitness < BestFitness)//szükséges, hogy a bolyt egyben tartsa
                BestFitness = ((BaseElement)Elements[4]).Fitness;
            BaseElement Elem;
            ArrayList parameter;
            for (int i = 0; i < NumberOfElements; i++)
            {
                //Ha az egyik lokális elem fitnesze nagyobb mint az 5.-é valamint nagyobb a különbségük mint 10 akkor vissza kerül globális keresövé
                if (((Ant)Ants[i]).IsLocal && (((Ant)Ants[i]).Elem.Fitness - BestFitness) >= 10)
                {
                    ((Ant)Ants[i]).IsLocal = false;
                }
                //Ha a hangya már csak lokálisan keres akkor nem nézi meg a feromon pontokat
                if (((Ant)Ants[i]).IsLocal || !FeromonSearch((Ant)Ants[i]))//Feromonok keresése
                {
                    parameter = new ArrayList();
                    for (int p = 0; p < InitialParameters.Count; p++)
                    {//Ha az aktuális egyed már lokálisan keres akkor a legjobb érték felével módosítsa csak a paraméterét
                        parameter.Add((double)((Ant)Ants[i]).Elem[p] + (((Ant)Ants[i]).IsLocal ? (BestFitness / 2) : R) * (RNG.NextDouble() * 2 - 1));
                        if ((double)parameter[p] > (double)UpperParamBounds[p])
                            parameter[p] = UpperParamBounds[p];
                        else if ((double)parameter[p] < (double)LowerParamBounds[p])
                            parameter[p] = LowerParamBounds[p];
                        if (Integer[p])
                            parameter[p] = Math.Round((double)parameter[p]);
                    }
                    Elem = (BaseElement)GetNewElement(FitnessFunction, parameter);
                    //Csak akkor adodik hozzá az új elem ha az nem lokálisan keress vagy pedig kisebb a fitnessze mint az 5. legjobbnak
                    if (!((Ant)Ants[i]).IsLocal || (Elem.Fitness < BestFitness && ((Ant)Ants[i]).IsLocal))
                        ((Ant)Ants[i]).newVectorAdd(Elem);
                }
                //Lokális keresésre váltás akkor történik meg ha az elöző körben legjobb 5. elemnél jobb az értéke
                if (!((Ant)Ants[i]).IsLocal && ((Ant)Ants[i]).Elem.Fitness < BestFitness)
                {
                    FeromonPoints.Add(new Feromon(((Ant)Ants[i]).Vectors));
                    ((Ant)Ants[i]).Vectors.Clear();//Lokális keresés során nincs szükség feromon vonalra a késöbbiekben
                    ((Ant)Ants[i]).IsLocal = true;
                }
                Elements[i] = ((Ant)Ants[i]).Elem;
            }
            Evaporation();//Párolgás
            Elements.Sort();//Rendezés a kimenethez
        }
        /// <summary>
        /// Feromon pontok keresése a közelben valamint érték módosítás
        /// </summary>
        /// <param name="Ant">Az aktuális hangya ami körül keresünk</param>
        /// <returns>Ha sikerült a keresés és modosítás akkor igazat ad vissza</returns>
        private bool FeromonSearch(Ant Ant)
        {
            if(FeromonPoints != null)
            {
                foreach (Feromon Feromon in FeromonPoints)
                {
                    foreach (Vector Vector in Feromon.Vectors)
                    {
                        if (isInRondure(Vector.Coordinates, Ant.Elem.Position) && Vector.Fitness < Ant.Elem.Fitness)
                        {
                            var parameters = new ArrayList();
                            for (int p = 0; p < Vector.Coordinates.Count; p++)
                            {
                                if (RNG.Next(2) % 2 == 0)
                                    parameters.Add((double)(Vector.Coordinates[p]) + (BestFitness / 2));
                                else
                                    parameters.Add((double)(Vector.Coordinates[p]) - (BestFitness / 2));

                                if ((double)parameters[p] > (double)UpperParamBounds[p])
                                    parameters[p] = UpperParamBounds[p];
                                else if ((double)parameters[p] < (double)LowerParamBounds[p])
                                    parameters[p] = LowerParamBounds[p];
                                if (Integer[p])
                                    parameters[p] = Math.Round((double)parameters[p]);
                            }
                            BaseElement Elem = (BaseElement)GetNewElement(FitnessFunction, parameters);
                            if (Elem.Fitness < Ant.Elem.Fitness)
                            {
                                Ant.newVectorAdd(Elem);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Feladata a már nem alkalmas feromon pontok eltávolítása valamint a felgyejzett útvonalak szelektálása
        /// </summary>
        private void Evaporation()
        {
            for (int i = 0; i < FeromonPoints.Count; i++)
            {
                if(((Feromon)FeromonPoints[i]).BestFitness > ((BaseElement)Elements[NumberOfElements - 1]).Fitness)
                {
                    FeromonPoints.RemoveAt(i);
                }
            }
            for (int i = 0; i < NumberOfElements; i++)
            {//Kritikus pont lehet a második feltétel!!!
                if (!((Ant)Ants[i]).IsLocal && ((Ant)Ants[i]).Vectors.Count >= NumberOfElements * 2)//TODO
                {
                    ((Ant)Ants[i]).Vectors.RemoveRange(((Ant)Ants[i]).Vectors.Count / 2, ((Ant)Ants[i]).Vectors.Count / 2);
                }
            }
        }
        /// <summary>
        /// Megmondja, hogy a vizsgált pontok szerepelnek e a másik pont ható sugarában (Kör egyenlet)
        /// </summary>
        /// <param name="XY">Azon pont amire kiváncsik vagyunk hogy benne van e</param>
        /// <param name="UV">Azon kör középontja amit vizsgálunk</param>
        /// <returns>Bent van e a körben vagy nem</returns>
        private bool isInRondure(ArrayList XY, ArrayList UV)
        {
            double value = 0;
            for (int i = 0; i < XY.Count; i++)
            {
                value += Math.Pow(((double)XY[i] - (double)UV[i]), 2);
            }
            if (value <= (R * R))
            {
                return true;
            }
            return false;
        }
        private class Feromon
        {
            /// <summary>
            /// Vector elemeket fog tartalmazni
            /// </summary>
            public ArrayList Vectors { get; set; }
            public double BestFitness { get; set; }

            public Feromon(ArrayList Vectors)
            {
                this.Vectors = new ArrayList();
                foreach (Vector Vector in Vectors)
                {
                    this.Vectors.Add(Vector);
                }
                BestFitness = ((Vector)this.Vectors[this.Vectors.Count - 1]).Fitness;
            }
        }
        private class Ant : IComparable
        {
            public BaseElement Elem { get; set; }
            /// <summary>
            /// Vector elemeket fog tartalmazni
            /// </summary>
            public ArrayList Vectors { get; set; }
            public bool IsLocal { get; set; }
            public Ant(BaseElement Elem)
            {
                this.Elem = Elem;
                Vectors = new ArrayList();
                this.Vectors.Add(new Vector(Elem.Position, Elem.Fitness));
                IsLocal = false;
            }
            /// <summary>
            /// Új vektord hozzáadása valamint az elem felül írása
            /// </summary>
            /// <param name="newElem"></param>
            public void newVectorAdd(BaseElement newElem)
            {
                this.Elem = newElem;
                if(!this.IsLocal)
                    this.Vectors.Add(new Vector(Elem.Position, Elem.Fitness));
            }
            public int CompareTo(object other)
            {
                var otherAnt = (Ant)other;
                if (otherAnt.Elem.Fitness < this.Elem.Fitness)
                    return 1;
                if (otherAnt.Elem.Fitness > this.Elem.Fitness)
                    return -1;
                return 0;
            }
        }
        private class Vector
        {
            /// <summary>
            /// A paraméter elemeket fogja tartalmazni
            /// </summary>
            public ArrayList Coordinates { get; set; }
            /// <summary>
            /// Az aktuális paraméterekhez tartozó Fitnesz érték
            /// </summary>
            public double Fitness { get; set; }
            public Vector(ArrayList Coordinates, double Fitness)
            {
                this.Coordinates = Coordinates;
                this.Fitness = Fitness;
            }
        }
    }
}
