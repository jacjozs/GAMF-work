using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Optimization
{
    public class AntAlg : BaseOptimizationMethod
    {
        /// <summary>
        /// Keresési kör sugara
        /// </summary>
        private double R = 10;
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
        private Feromon FeromonPoints;
        protected override void CreateNextGeneration()
        {
            if(isFirst)
            {
                Ants = new ArrayList();

                var elem = GetNewElement(FitnessFunction, InitialParameters);
                for (int i = 0; i < NumberOfElements; i++)
                {
                    Ants.Add(new Ant((BaseElement)elem));
                }
                FeromonPoints = new Feromon(((Ant)Ants[0]).Vectors, int.MaxValue);
                isFirst = false;
            }
            for (int i = 0; i < NumberOfElements; i++)
            {
                var parameter = new ArrayList();
                //Feromonok keresése
                if (!FeromonSearch((Ant)Ants[i]))
                {
                    for (int p = 0; p < InitialParameters.Count; p++)
                    {
                        parameter.Add((double)((Ant)Ants[i]).Elem[p] + R * (RNG.NextDouble() * 2 - 1));

                        if ((double)parameter[p] > (double)UpperParamBounds[p])
                            parameter[p] = UpperParamBounds[p];
                        else if ((double)parameter[p] < (double)LowerParamBounds[p])
                            parameter[p] = LowerParamBounds[p];
                        if (Integer[p])
                            parameter[p] = Math.Round((double)parameter[p]);
                    }
                    ((Ant)Ants[i]).newVectorAdd((BaseElement)GetNewElement(FitnessFunction, parameter));
                }
                if(1 >= ((Ant)Ants[i]).Elem.Fitness && FeromonPoints.Steps > ((Ant)Ants[i]).Vectors.Count)
                {
                    FeromonPoints = new Feromon(((Ant)Ants[i]).Vectors);
                    ((Ant)Ants[i]).Vectors.Clear();
                }
                Elements[i] = ((Ant)Ants[i]).Elem;
            }
            Ants.Sort();
            Elements.Sort();
        }
        /// <summary>
        /// Feromon pontok keresése a közelben valamint érték modósítás
        /// </summary>
        /// <param name="Ant">Az aktuális hangya ami körül keresünk</param>
        /// <returns>Ha sikerült a keresés és modosítás akkor igazat ad vissza</returns>
        private bool FeromonSearch(Ant Ant)
        {
            ArrayList Feromons = new ArrayList();
            int Count = 0;
            if(FeromonPoints != null)
            {
                foreach (Vector Vector in FeromonPoints.Vectors)
                {
                    if (isInRondure(Vector.Coordinates, Ant.Elem.Position) && Vector.Fitness < Ant.Elem.Fitness)
                    {
                        //FeromonPoints.SearchVector.Add(Vector);
                        Ant.newVectorAdd((BaseElement)GetNewElement(FitnessFunction, Vector.Coordinates));
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Kör egyenlete Csak 2 dimenziós!!!
        /// </summary>
        /// <param name="XY">Azon pont amire kiváncsik vagyunk hogy benne van e</param>
        /// <param name="UV">Azon kör középontja amit vizsgálunk</param>
        /// <returns></returns>
        private bool isInRondure(ArrayList XY, ArrayList UV)
        {
            double value = Math.Pow(((double)XY[0] - (double)UV[0]), 2) + Math.Pow(((double)XY[1] - (double)UV[1]), 2);
            if (value <= (R * R))
            {
                return true;
            }
            return false;
        }
        private class Feromon : IComparable
        {
            /// <summary>
            /// Vector elemeket fog tartalmazni
            /// </summary>
            public ArrayList Vectors { get; set; }
            public int Steps { get; set; }
            /// <summary>
            /// A keresés során ebben tárolodnak el azok a pontok amik a keresési pont körül vannak
            /// </summary>
            public ArrayList SearchVector { get; set; }
            public Feromon(ArrayList Vectors)
            {
                this.Vectors = new ArrayList();
                foreach (Vector Vector in Vectors)
                {
                    this.Vectors.Add(Vector);
                }
                Steps = this.Vectors.Count;
                SearchVector = new ArrayList();
            }
            public Feromon(ArrayList Vectors, int Steps)
            {
                this.Vectors = new ArrayList();
                foreach (Vector Vector in Vectors)
                {
                    this.Vectors.Add(Vector);
                }
                this.Steps = Steps;
                SearchVector = new ArrayList();
            }
            public int CompareTo(object other)
            {
                var otherFeromon = (Feromon)other;
                if (otherFeromon.Steps < Steps)
                    return 1;
                if (otherFeromon.Steps > Steps)
                    return -1;
                return 0;
            }
        }
        private class Ant : IComparable
        {
            public BaseElement Elem { get; set; }
            /// <summary>
            /// Vector elemeket fog tartalmazni
            /// </summary>
            public ArrayList Vectors { get; set; }
            public Ant(BaseElement Elem)
            {
                this.Elem = Elem;
                Vectors = new ArrayList();
                this.Vectors.Add(new Vector(Elem.Position, Elem.Fitness));
            }
            /// <summary>
            /// Új vektord hozzáadása valamint az elem felül írása
            /// </summary>
            /// <param name="newElem"></param>
            public void newVectorAdd(BaseElement newElem)
            {
                this.Elem = newElem;
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
        private class Vector : IComparable
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
            public int CompareTo(object other)
            {
                var otherVector = (Vector)other;
                if (otherVector.Fitness < Fitness)
                    return 1;
                if (otherVector.Fitness > Fitness)
                    return -1;
                return 0;
            }
        }
    }
}
