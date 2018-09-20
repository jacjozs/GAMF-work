using System;
using System.Collections;
using System.Collections.Generic;

namespace Optimization
{
    public class AntAlg : BaseOptimizationMethod
    {
        private List<AntClass> Ants;

        private ArrayList FitneszLines;

        protected override void CreateNextGeneration()
        {
            if(FitneszLines == null)
            {
                FitneszLines = new ArrayList();
            }
            if(Ants == null)
            {
                Ants = new List<AntClass>();
                for (int i = 0; i < NumberOfElements; i++)
                {
                    Ants.Add(new AntClass((BaseElement)Elements[i]));
                }
            }
            for (int i = 0; i < NumberOfElements; i++)
            {
                double oldfitness = Ants[i].Ant.Fitness;
                var parameter = new ArrayList();
                for (int p = 0; p < InitialParameters.Count; p++)
                {
                    parameter.Add(Ants[i].Ant.Position[p]);
                    parameter[p] = (double)parameter[p] + 10 * (RNG.NextDouble() * 2 - 1);

                    if ((double)parameter[p] > (double)UpperParamBounds[p])
                        parameter[p] = UpperParamBounds[p];
                    else if ((double)parameter[p] < (double)LowerParamBounds[p])
                        parameter[p] = LowerParamBounds[p];
                    if (Integer[p])
                        parameter[p] = Math.Round((double)parameter[p]);
                }

                Ants[i].lines.Add(parameter);
                Ants[i].Ant = (BaseElement)GetNewElement(FitnessFunction, parameter);
                if(Ants[i].Ant.Fitness < oldfitness)
                {
                    FitneszLines.Add(Ants[i].lines);
                }
            }
            for (int i = 0; i < NumberOfElements; i++)
            {
                Elements[i] = Ants[i].Ant;
            }
            Elements.Sort();
        }
    }

    class AntClass
    {
        public BaseElement Ant { get; set; }
        public List<ArrayList> lines { get; set; }

        public AntClass(BaseElement Ant)
        {
            this.Ant = Ant;
            lines = new List<ArrayList>();
            lines.Add(Ant.Position);
        }
    }
}
