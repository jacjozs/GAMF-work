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
        private double R = 15;
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

        private double MaxSize = double.MaxValue;
        private int count = 0;
        protected override void CreateNextGeneration()
        {
            if(isFirst)
            {
                Ants = new ArrayList();

                //var elem = GetNewElement(FitnessFunction, InitialParameters);
                for (int i = 0; i < NumberOfElements; i++)
                {
                    Ants.Add(new Ant((BaseElement)Elements[i]));
                }
                //FeromonPoints = new Feromon(((Ant)Ants[0]).Vectors, int.MaxValue);
                FeromonPoints = new ArrayList(); 
                isFirst = false;
            }
            
            if (count >= NumberOfElements / 10)
            {
                MaxSize /= NumberOfElements * 2;
                count = 0;
                if (MaxSize < R / 2)
                {
                    R /= 2;
                }
                for (int i = 0; i < NumberOfElements; i++)
                {
                    ((Ant)Ants[i]).IsIn = false;
                }
            }
            for (int i = 0; i < NumberOfElements; i++)
            {
                var parameter = new ArrayList();
                var parameterBack = new ArrayList();
                BaseElement Elem;
                if (!FeromonSearch((Ant)Ants[i]) || MaxSize < R / 2)//Feromonok keresése
                {
                    do
                    {
                        for (int p = 0; p < InitialParameters.Count; p++)
                        {
                            if(((Ant)Ants[i]).IsIn)
                                parameter.Add((double)((Ant)Ants[i]).Elem[p] + (R / 2) * (RNG.NextDouble() * 2 - 1));
                            else
                                parameter.Add((double)((Ant)Ants[i]).Elem[p] + R * (RNG.NextDouble() * 2 - 1));
                            if ((double)parameter[p] > (double)UpperParamBounds[p])
                                parameter[p] = UpperParamBounds[p];
                            else if ((double)parameter[p] < (double)LowerParamBounds[p])
                                parameter[p] = LowerParamBounds[p];
                            if (Integer[p])
                                parameter[p] = Math.Round((double)parameter[p]);
                        }
                        Elem = (BaseElement)GetNewElement(FitnessFunction, parameter);
                        parameter.Clear();
                    } while (Elem.Fitness > MaxSize && ((Ant)Ants[i]).IsIn);
                    ((Ant)Ants[i]).newVectorAdd(Elem);
                }
                if(MaxSize >= ((Ant)Ants[i]).Elem.Fitness && Math.Abs(MaxSize - ((Ant)Ants[i]).Elem.Fitness) > 0.1)
                {
                    if (!((Ant)Ants[i]).IsIn)
                    {
                        count++;
                        ((Ant)Ants[i]).IsIn = true;
                    }
                    if(((Ant)Ants[i]).Vectors.Count > 5)
                    {
                        FeromonPoints.Add(new Feromon(((Ant)Ants[i]).Vectors));
                        ((Ant)Ants[i]).Vectors.Clear();
                    }
                }
                Elements[i] = ((Ant)Ants[i]).Elem;
            }
            Ants.Sort();
            Elements.Sort();
            Evaporation();//Párolgás
        }
        /// <summary>
        /// Feromon pontok keresése a közelben valamint érték modósítás
        /// </summary>
        /// <param name="Ant">Az aktuális hangya ami körül keresünk</param>
        /// <returns>Ha sikerült a keresés és modosítás akkor igazat ad vissza</returns>
        private bool FeromonSearch(Ant Ant)
        {
            if(FeromonPoints != null)
            {
                foreach (Feromon Feromon in FeromonPoints)
                {
                    ArrayList list = new ArrayList();
                    list.Add(Feromon.Vectors[0]);
                    list.Add(Feromon.Vectors[Feromon.Vectors.Count - 1]);
                    if (isInLine(new Vector(Ant.Elem.Position, Ant.Elem.Fitness), list))
                    {
                        foreach (Vector Vector in Feromon.Vectors)
                        {
                            if (isInRondure(Vector.Coordinates, Ant.Elem.Position) && Vector.Fitness < Ant.Elem.Fitness)
                            {
                                var parameters = new ArrayList();
                                for (int i = 0; i < Vector.Coordinates.Count; i++)
                                {
                                    if (RNG.Next(2) % 2 == 0)
                                        parameters.Add((double)(Vector.Coordinates[i]) + 0.1);
                                    else
                                        parameters.Add((double)(Vector.Coordinates[i]) - 0.1);
                                }
                                BaseElement Elem = (BaseElement)GetNewElement(FitnessFunction, parameters);
                                if (!Ant.IsIn && Elem.Fitness < Ant.Elem.Fitness)
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
        /// párolgás
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
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="XY">Azon pont amire kiváncsik vagyunk hogy benne van e</param>
        /// <param name="UV">Azon kör középontja amit vizsgálunk</param>
        /// <returns></returns>
        private bool isInRondure(ArrayList XY, ArrayList UV)
        {
            double value = Math.Pow(((double)XY[0] - (double)UV[0]), 2);
            for (int i = 1; i < XY.Count; i++)
            {
                value += Math.Pow(((double)XY[i] - (double)UV[i]), 2);
            }
            if (value <= (R * R))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="XY"></param>
        /// <param name="UV"></param>
        /// <returns></returns>
        private bool isInLine(Vector XY, ArrayList UV)
        {
            Vector UUVV_1 = (Vector)UV[0];
            Vector UUVV_2 = (Vector)UV[1];
            ArrayList NormalVector = new ArrayList();
            NormalVector.Add(((double)UUVV_1.Coordinates[1] - (double)UUVV_2.Coordinates[1]) * (-1));
            NormalVector.Add((double)UUVV_1.Coordinates[0] - (double)UUVV_2.Coordinates[0]);
            Vector UUVV_3 = new Vector(NormalVector, 0);
            double value = ((double)XY.Coordinates[0] * (double)UUVV_3.Coordinates[0]) + ((double)XY.Coordinates[1] * (double)UUVV_3.Coordinates[1]);
            double value2 = ((double)UUVV_3.Coordinates[0] * (double)UUVV_3.Coordinates[0]) + ((double)UUVV_3.Coordinates[1] * (double)UUVV_3.Coordinates[1]);
            if (value >= value2 - (value2 / 2) && value <= value2 + (value2 / 2))
                return true;
            return false;
        }
        private class Feromon : IComparable
        {
            /// <summary>
            /// Vector elemeket fog tartalmazni
            /// </summary>
            public ArrayList Vectors { get; set; }
            public int Steps { get; set; }
            public double BestFitness { get; set; }

            public Feromon(ArrayList Vectors)
            {
                this.Vectors = new ArrayList();
                foreach (Vector Vector in Vectors)
                {
                    this.Vectors.Add(Vector);
                }
                Steps = this.Vectors.Count;
                BestFitness = ((Vector)this.Vectors[this.Steps - 1]).Fitness;
            }
            public Feromon(ArrayList Vectors, int Steps)
            {
                this.Vectors = new ArrayList();
                foreach (Vector Vector in Vectors)
                {
                    this.Vectors.Add(Vector);
                }
                this.Steps = Steps;
                BestFitness = ((Vector)this.Vectors[this.Steps - 1]).Fitness;
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
            public bool IsIn { get; set; }
            public Ant(BaseElement Elem)
            {
                this.Elem = Elem;
                Vectors = new ArrayList();
                this.Vectors.Add(new Vector(Elem.Position, Elem.Fitness));
                IsIn = false;
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
