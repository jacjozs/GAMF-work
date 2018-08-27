using System;
using System.Collections;

namespace Optimization.BacterialAlg
{
    public class BacterialAlg : BaseOptimizationMethod
    {
        protected override void CreateNextGeneration()
        {
            BaseElement copy;
            for (int i = 0; i < NumberOfElements; i++)
            {
                ArrayList Clones = new ArrayList();
                copy = (BaseElement)Elements[i];
                int rnd = RNG.Next(1, 20);
                for (int h = 0; h < rnd; h++)
                {
                    Clones.Add(copy);
                }
                for (int p = 0; p < InitialParameters.Count; p++)
                {
                    var parameter = new ArrayList();
                    for (int j = 0; j < Clones.Count; j++)
                    {
                        parameter = ((BaseElement)Clones[j]).Position;
                        parameter[p] = (double)((BaseElement)Clones[j])[p] + 5 * (RNG.NextDouble() * 2 - 1);
                        if ((double)parameter[p] > (double)UpperParamBounds[p])
                            parameter[p] = UpperParamBounds[p];
                        else if ((double)parameter[p] < (double)LowerParamBounds[p])
                            parameter[p] = LowerParamBounds[p];
                        if (Integer[p])
                            parameter[p] = Math.Round((double)parameter[p]);

                        Clones[j] = GetNewElement(FitnessFunction, parameter);
                    }
                    Clones.Sort();
                    for (int j = 0; j < Clones.Count; j++)
                    {
                        ((BaseElement)Clones[j])[p] = ((BaseElement)Clones[0])[p];
                    }
                }
                Clones.Sort();
                Elements[i] = Clones[0];
            }
        }
    }
}
