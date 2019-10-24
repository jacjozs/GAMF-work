using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    /// <summary>
    /// Haromony Search Optimization Algorithm
    /// Az ajánlott egyedszám 20-50 közti érték
    /// </summary>
    public class HaromonySearch : BaseOptimizationMethod
    {
        /// <summary>
        /// Valószínűségi változó
        /// 0.1 - 1.0
        /// Ajánlott: 0.7 - 0.95
        /// </summary>
        public double consid_rate { get; set; }
        /// <summary>
        /// Paraméter módosítási változó
        /// 0.1 - 1.0
        /// Ajánlott: 0.1 - 0.5
        /// </summary>
        public double adjust_rate { get; set; }
        /// <summary>
        /// Ugrási változó
        /// </summary>
        public double range { get; set; }
        protected override void CreateNextGeneration()
        {
            for (int i = 0; i < NumberOfElements; i++)
            {
                var parameter = new ArrayList();
                for (int p = 0; p < InitialParameters.Count; p++)
                {
                    if(RNG.NextDouble() < consid_rate)
                    {
                        parameter.Add(((BaseElement)Elements[RNG.Next(0, Elements.Count)])[p]);
                        if(RNG.NextDouble() < adjust_rate)
                        {
                            parameter[p] = (double)parameter[p] + range * RandomInBounds(-1.0, 1.0);
                        }
                        if ((double)parameter[p] > (double)UpperParamBounds[p])
                            parameter[p] = UpperParamBounds[p];
                        else if ((double)parameter[p] < (double)LowerParamBounds[p])
                            parameter[p] = LowerParamBounds[p];
                        if (Integer[p])
                            parameter[p] = Math.Round((double)parameter[p]);
                    }
                    else
                    {
                        parameter.Add(RandomInBounds((double)LowerParamBounds[p], (double)UpperParamBounds[p]));
                        if (Integer[p])
                            parameter[p] = Math.Round((double)parameter[p]);
                    }
                }
                BaseElement newElem = (BaseElement)GetNewElement(FitnessFunction, parameter);
                if(newElem.Fitness < ((BaseElement)Elements[0]).Fitness)
                {
                    Elements.Add(newElem);
                    Elements.Sort();
                    Elements.RemoveAt(NumberOfElements - 1);
                }
            }
        }
        public double RandomInBounds(double min, double max)
        {
            return min + ((max - min) * RNG.NextDouble());
        }
    }
}
