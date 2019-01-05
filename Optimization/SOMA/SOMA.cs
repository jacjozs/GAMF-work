using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class SOMA : BaseOptimizationMethod
    {
        /// <summary>
        /// Az az érték hogy mennyinél kell kisebbnek lennie a randomnak, hogy 
        /// az aktuális paraméter módosítás 1 legyen [0,1]
        /// </summary>
        public double PRT { get; set; }
        /// <summary>
        /// Maximális ugrási méret [0.11, 5]
        /// </summary>
        public double ParthLength { get; set; }
        /// <summary>
        /// Ugrási méret [0.11, ParthLength]
        /// </summary>
        public double Step { get; set; }
        /// <summary>
        /// Ugrások maximális száma [2, PopSize]
        /// </summary>
        public int PopSize { get; set; }
        protected override void CreateNextGeneration()
        {
            
            int PRTVector = 0;
            for (int i = 0; i < NumberOfElements; i++)
            {
                double t = 0.0;
                Elements.Sort();
                for (int j = 0; j <= PopSize && t <= ParthLength; j++)
                {
                    var parameter = new ArrayList();
                    for (int p = 0; p < InitialParameters.Count; p++)
                    {
                        if (RNG.NextDouble() < PRT) PRTVector = 1;
                        else PRTVector = 0;
                        parameter.Add(((BaseElement)Elements[i])[p]);
                        parameter[p] = (double)parameter[p] + ((double)((BaseElement)Elements[0])[p] - (double)parameter[p]) * t * PRTVector;
                        if ((double)parameter[p] > (double)UpperParamBounds[p])
                            parameter[p] = UpperParamBounds[p];
                        else if ((double)parameter[p] < (double)LowerParamBounds[p])
                            parameter[p] = LowerParamBounds[p];
                        if (Integer[p])
                            parameter[p] = Math.Round((double)parameter[p]);
                    }
                    var newElem = GetNewElement(FitnessFunction, parameter);
                    if(((BaseElement)newElem).Fitness <= ((BaseElement)Elements[i]).Fitness)
                    {
                        Elements[i] = newElem;
                    }
                    t += Step;
                }

            }
        }
    }
}
