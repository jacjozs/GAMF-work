using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    /// <summary>
    /// Differential Evolution Optimization Algorithm
    /// Az ajánlott egyedszám 50-150 közti érték
    /// </summary>
    public class DifferentialEvolution : BaseOptimizationMethod
    {
        /// <summary>
        /// Valószínűségi változó
        /// 0 - 1.0
        /// </summary>
        public double crossf { get; set; }
        /// <summary>
        /// Súlyozási tényező
        /// 0 - 2.0
        /// </summary>
        public double weighf { get; set; }
        protected override void CreateNextGeneration()
        {
            int p0, p1, p2, p3;
            for (int i = 0; i < NumberOfElements; i++)
            {
                //Random elemek
                p0 = i; p1 = i; p2 = i; p3 = i;
                while (p0 == p1)
                {
                    p1 = RNG.Next(NumberOfElements - 1);
                }
                while ((p0 == p2) || (p1 == p2))
                {
                    p2 = RNG.Next(NumberOfElements - 1);
                }
                while ((p0 == p3) || (p1 == p3) || (p2 == p3))
                {
                    p3 = RNG.Next(NumberOfElements - 1);
                }
                //Random paraméter
                int param = RNG.Next(InitialParameters.Count);
                var parameter = new ArrayList();
                for (int p = 0; p < InitialParameters.Count; p++)
                {
                    if(p == param && RNG.NextDouble() < crossf)
                    {
                        parameter.Add((double)((BaseElement)Elements[p3])[p] + weighf * ((double)((BaseElement)Elements[p1])[p] - (double)((BaseElement)Elements[p2])[p]));
                    }
                    else
                    {
                        parameter.Add(((BaseElement)Elements[i])[p]);
                    }
                    if ((double)parameter[p] > (double)UpperParamBounds[p])
                        parameter[p] = UpperParamBounds[p];
                    else if ((double)parameter[p] < (double)LowerParamBounds[p])
                        parameter[p] = LowerParamBounds[p];
                    if (Integer[p])
                        parameter[p] = Math.Round((double)parameter[p]);
                }
                BaseElement newElem = (BaseElement)GetNewElement(FitnessFunction, parameter);
                if (((BaseElement)Elements[i]).Fitness > newElem.Fitness)
                {
                    Elements[i] = newElem;
                }
            }
        }
    }
}
