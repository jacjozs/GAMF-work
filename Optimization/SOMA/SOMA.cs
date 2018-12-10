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
        /// [0,1]
        /// </summary>
        public double PRT { get; set; }
        /// <summary>
        /// [0.11,5]
        /// </summary>
        public double ParthLength { get; set; }
        /// <summary>
        /// [0.11, ParthLength]
        /// </summary>
        public double Step { get; set; }
        public double MinDiv { get; set; }
        protected override void CreateNextGeneration()
        {
            ParthLength = 1;
            for (int i = 0; i < NumberOfElements; i++)
            {
                double t = 0;
                while (t <= ParthLength)
                {
                    var parameter = new ArrayList();
                    for (int j = 0; j < InitialParameters.Count; j++)
                    {
                        if (RNG.NextDouble() < PRT)
                            parameter.Add((double)((BaseElement)Elements[i])[j] + ((double)((BaseElement)Elements[0])[j] - (double)((BaseElement)Elements[i])[j]) * t);
                        else
                            parameter.Add((double)((BaseElement)Elements[i])[j] + ((double)((BaseElement)Elements[0])[j] - (double)((BaseElement)Elements[i])[j]) * t * -1);

                        if ((double)parameter[j] > (double)UpperParamBounds[j])
                            parameter[j] = UpperParamBounds[j];
                        else if ((double)parameter[j] < (double)LowerParamBounds[j])
                            parameter[j] = LowerParamBounds[j];
                        if (Integer[j])
                            parameter[j] = Math.Round((double)parameter[j]);
                    }
                    BaseElement newElem = (BaseElement)GetNewElement(FitnessFunction, parameter);
                    if (newElem.Fitness <= ((BaseElement)Elements[i]).Fitness)
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
