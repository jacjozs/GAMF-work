using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    /// <summary>
    /// Self Organizing Migrating Algorithm
    /// </summary>
    public class SelfOrgMigrating : BaseOptimizationMethod
    {
        /// <summary>
        /// Az az érték hogy mennyinél kell kisebbnek lennie a randomnak, hogy 
        /// az aktuális paraméter módosítás 1 legyen [0,1]
        /// </summary>
        public double PRT { get; set; }
        /// <summary>
        /// Maximális ugrási méret [0.11, 5]
        /// </summary>
        public double PathLength { get; set; }
        /// <summary>
        /// Ugrási méret [0.11, ParthLength]
        /// </summary>
        public double Step { get; set; }
        /// <summary>
        /// Ugrások maximális száma [2, PopSize]
        /// </summary>
        public int PopSize { get; set; }
        /// <summary>
        /// Típus meghatározó
        /// </summary>
        public SelfOrgMigratingType Type { get; set; }

        protected override void CreateNextGeneration()
        {
            switch (Type)
            {
                case SelfOrgMigratingType.AllToOne:
                    this.All_To_One();
                    break;
                case SelfOrgMigratingType.AllToAll:
                    this.PRT = 1;
                    this.All_To_All();
                    break;
                case SelfOrgMigratingType.AllToAllAdaptive:
                    this.PRT = 1;
                    this.All_To_All_Adaptive();
                    break;
                case SelfOrgMigratingType.AllToRand:
                    this.All_To_Rand();
                    break;
                default:
                    this.All_To_All();
                    break;
            }
        }

        private void All_To_One()
        {
            int PRTVector = 0;
            for (int i = 0; i < NumberOfElements; i++)
            {
                double t = 0.0;
                Elements.Sort();
                for (int j = 0; j <= PopSize && t <= PathLength; j++)
                {
                    var parameter = new ArrayList();
                    for (int p = 0; p < InitialParameters.Count; p++)
                    {
                        if (RNG.NextDouble() < PRT) PRTVector = 1;
                        else PRTVector = 0;
                        parameter.Add(((BaseElement)Elements[i])[p]);
                        if (i == 0)
                            parameter[p] = (double)parameter[p] + ((double)parameter[p] - t * (RNG.NextDouble() * 2 - 1)) * PRTVector;
                        else
                            parameter[p] = (double)parameter[p] + ((double)((BaseElement)Elements[0])[p] - (double)parameter[p]) * t * PRTVector;
                        
                        if ((double)parameter[p] > (double)UpperParamBounds[p])
                            parameter[p] = UpperParamBounds[p];
                        else if ((double)parameter[p] < (double)LowerParamBounds[p])
                            parameter[p] = LowerParamBounds[p];
                        if (Integer[p])
                            parameter[p] = Math.Round((double)parameter[p]);
                    }
                    var newElem = GetNewElement(FitnessFunction, parameter);
                    if (((BaseElement)newElem).Fitness <= ((BaseElement)Elements[i]).Fitness)
                    {
                        Elements[i] = newElem;
                    }
                    t += Step;
                }
            }
            Elements.Sort();
        }

        private void All_To_All()
        {
            int PRTVector = 0;
            for (int i = NumberOfElements - 1; i >= 0; i--)
            {
                double t = 0.0;
                int rnd = 0;
                do rnd = RNG.Next(NumberOfElements); while (rnd == i);
                for (int j = 0; j <= PopSize && t <= PathLength; j++)
                {
                    var parameter = new ArrayList();
                    for (int p = 0; p < InitialParameters.Count; p++)
                    {
                        if (RNG.NextDouble() < PRT) PRTVector = 1;
                        else PRTVector = 0;
                        parameter.Add(((BaseElement)Elements[i])[p]);
                        parameter[p] = (double)parameter[p] + ((double)((BaseElement)Elements[rnd])[p] - (double)parameter[p]) * t * PRTVector;
                        if ((double)parameter[p] > (double)UpperParamBounds[p])
                            parameter[p] = UpperParamBounds[p];
                        else if ((double)parameter[p] < (double)LowerParamBounds[p])
                            parameter[p] = LowerParamBounds[p];
                        if (Integer[p])
                            parameter[p] = Math.Round((double)parameter[p]);
                    }
                    var newElem = GetNewElement(FitnessFunction, parameter);
                    if (((BaseElement)newElem).Fitness <= ((BaseElement)Elements[i]).Fitness)
                    {
                        Elements[i] = newElem;
                    }
                    t += Step;
                }
            }
            Elements.Sort();
        }

        private void All_To_All_Adaptive()
        {
            int PRTVector = 0;
            for (int i = NumberOfElements - 1; i >= 0; i--)
            {
                double t = 0.0;
                int rnd = 0;
                do rnd = RNG.Next(NumberOfElements); while (rnd == i);
                for (int j = 0; j <= PopSize && t <= PathLength; j++)
                {
                    var parameter = new ArrayList();
                    for (int p = 0; p < InitialParameters.Count; p++)
                    {
                        if (RNG.NextDouble() < PRT) PRTVector = 1;
                        else PRTVector = 0;
                        parameter.Add(((BaseElement)Elements[i])[p]);
                        parameter[p] = (double)parameter[p] + ((double)((BaseElement)Elements[rnd])[p] - (double)parameter[p]) * t * PRTVector;
                        if ((double)parameter[p] > (double)UpperParamBounds[p])
                            parameter[p] = UpperParamBounds[p];
                        else if ((double)parameter[p] < (double)LowerParamBounds[p])
                            parameter[p] = LowerParamBounds[p];
                        if (Integer[p])
                            parameter[p] = Math.Round((double)parameter[p]);
                    }
                    var newElem = GetNewElement(FitnessFunction, parameter);
                    if (((BaseElement)newElem).Fitness <= ((BaseElement)Elements[i]).Fitness)
                    {
                        Elements[i] = newElem;
                    }
                    t += Step;
                }
            }
            Elements.Sort();
            for (int i = 1; i < NumberOfElements; i++)
            {
                Elements[i] = Elements[0];
            }
        }

        private void All_To_Rand()
        {
            int PRTVector = 0;
            for (int i = 0; i < NumberOfElements; i++)
            {
                double t = 0.0;
                int size = RNG.Next(1, PopSize);
                int rnd = 0;
                do rnd = RNG.Next(NumberOfElements); while (rnd == i);
                for (int j = 0; j <= size && t <= PathLength; j++)
                {
                    var parameter = new ArrayList();
                    for (int p = 0; p < InitialParameters.Count; p++)
                    {
                        if (RNG.NextDouble() < PRT) PRTVector = 1;
                        else PRTVector = 0;
                        parameter.Add(((BaseElement)Elements[i])[p]);
                        parameter[p] = (double)parameter[p] + ((double)((BaseElement)Elements[rnd])[p] - (double)parameter[p]) * t * PRTVector;
                        if ((double)parameter[p] > (double)UpperParamBounds[p])
                            parameter[p] = UpperParamBounds[p];
                        else if ((double)parameter[p] < (double)LowerParamBounds[p])
                            parameter[p] = LowerParamBounds[p];
                        if (Integer[p])
                            parameter[p] = Math.Round((double)parameter[p]);
                    }
                    var newElem = GetNewElement(FitnessFunction, parameter);
                    if (((BaseElement)newElem).Fitness <= ((BaseElement)Elements[i]).Fitness)
                    {
                        Elements[i] = newElem;
                    }
                    t += Step;
                }
            }
            Elements.Sort();
        }
    }
}
