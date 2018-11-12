using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows;

namespace Optimization
{
    public class NewtonL : BaseOptimizationMethod
    {
        public double HistoryFitness { get; set; }
        public int Reduct { get; set; }
        protected override void CreateNextGeneration()
        {
            var M1Left = new ArrayList();
            var M2Right = new ArrayList();
            var StartPosition = (BaseElement)Elements[0];
            HistoryFitness = ((BaseElement) Elements[0]).Fitness;
            for (int p = 0; p < InitialParameters.Count; p++)
            {
                M1Left.Clear();
                M2Right.Clear();
                foreach (var item in StartPosition.Position)
                {
                    M1Left.Add(item);
                    M2Right.Add(item);
                }

                M1Left[p] = (double)M1Left[p] + ((double)LowerParamBounds[p] - (double)StartPosition[p]) / Reduct;
                M2Right[p] = (double)M2Right[p] + ((double)UpperParamBounds[p] - (double)StartPosition[p]) / Reduct;

                BaseElement M1 = (BaseElement)GetNewElement(FitnessFunction, M1Left);
                BaseElement M2 = (BaseElement)GetNewElement(FitnessFunction, M2Right);
                if (M1.Fitness > M2.Fitness)
                    StartPosition = M2;
                if (M1.Fitness < M2.Fitness)
                    StartPosition = M1;
                if (Integer[p])
                    StartPosition[p] = Math.Round((double)StartPosition[p]);
                Elements[0] = StartPosition;
                if (HistoryFitness < ((BaseElement) Elements[0]).Fitness)
                    Reduct++;
                else if (HistoryFitness == ((BaseElement) Elements[0]).Fitness)
                    Reduct += 2;
                else
                    HistoryFitness = StartPosition.Fitness;
            }
        }
    }
}
