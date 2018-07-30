using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows;

namespace Optimization
{
    /// <summary>
    /// Az algoritmus csak 2 és 3 dimenziós környezetben volt tesztelve és ott is még csak lineáris emelkedőn!
    /// Feladatok: (fejlesztés)
    /// Egyértelműbb megszakitás!
    /// Kivétel kezelések!
    /// Olcsóbb logikai menet! (Optimalizálás)
    /// Leegyszerűsítés!
    /// </summary>
    public class NewtonL : BaseOptimizationMethod
    {
        protected override void CreateNextGeneration()
        {
            var M1Left = new ArrayList();
            var M2Right = new ArrayList();
            Elements.Sort();
            Elements.RemoveRange(0, Elements.Count - 1);
            var StartPosition = (BaseElement)Elements[0];
            double LeftUnit, RightUnit;
            for (int p = 0; p < InitialParameters.Count; p++)
            {
                M1Left.Clear();
                M2Right.Clear();
                foreach (var item in ((BaseElement)StartPosition).Position)
                {
                    M1Left.Add(item);
                    M2Right.Add(item);
                }
                LeftUnit = ((double)LowerParamBounds[p] - (double)StartPosition[p]) / 50;
                RightUnit = ((double)UpperParamBounds[p] - (double)StartPosition[p]) / 50;

                M1Left[p] = (double)M1Left[p] + LeftUnit;
                M2Right[p] = (double)M2Right[p] + RightUnit;

                BaseElement Leftt = (BaseElement)GetNewElement(FitnessFunction, M1Left);
                BaseElement Rightt = (BaseElement)GetNewElement(FitnessFunction, M2Right);
                if (Leftt.Fitness > Rightt.Fitness)
                    StartPosition = Rightt;
                if (Leftt.Fitness < Rightt.Fitness)
                    StartPosition = Leftt;
                if (Integer[p])
                    StartPosition[p] = Math.Round((double)StartPosition[p]);
                Elements[0] = StartPosition;
            }
        }
    }
}
