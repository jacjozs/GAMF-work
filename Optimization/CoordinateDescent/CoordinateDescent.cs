using System;
using System.Collections;

namespace Optimization
{
    public class CoordinateDescent : BaseOptimizationMethod
    {
        /// <summary>
        /// Relatív lépési méret
        /// this > 0 !!!
        /// </summary>
        public double StepSizeRelative { get; set; }
        protected override void CreateNextGeneration()
        {
            //Kivétel kezelés a StepSizeRelative változóra
            if (StepSizeRelative <= 0) throw new ArgumentException("StepSizeRelative cannot be lower than zero", "original");
            ArrayList M1Minus, M2Plus;
            double randomScale = 0.0;
            for (int i = 0; i < NumberOfElements; i++)
            {
                for (int p = 0; p < InitialParameters.Count; p++)
                {
                    M1Minus = new ArrayList();
                    M2Plus = new ArrayList();
                    //Jelenlegi paraméterek fevétele az aktuális egydtől
                    M1Minus.AddRange(((BaseElement)Elements[i]).Position);
                    M2Plus.AddRange(((BaseElement)Elements[i]).Position);
                    //Random szám generálása az ugrási méret módosításához
                    randomScale = RNG.NextDouble();
                    //Paraméterek módosítása
                    M1Minus[p] = (double)M1Minus[p] - ((double)M1Minus[p] - (double)LowerParamBounds[p]) / StepSizeRelative * randomScale;
                    M2Plus[p] = (double)M2Plus[p] + ((double)UpperParamBounds[p] - (double)M2Plus[p]) / StepSizeRelative * randomScale;
                    if (Integer[p]) {
                        M1Minus[p] = Math.Round((double)M1Minus[p]);
                        M2Plus[p] = Math.Round((double)M2Plus[p]);
                    }
                    //Fitnesz számítás
                    BaseElement newM1Minus = (BaseElement)GetNewElement(FitnessFunction, M1Minus);
                    BaseElement newM2Plus = (BaseElement)GetNewElement(FitnessFunction, M2Plus);
                    //Eredmény kiértékelés
                    if (newM1Minus.Fitness < ((BaseElement)Elements[i]).Fitness)
                        Elements[i] = newM1Minus;
                    if (newM2Plus.Fitness < ((BaseElement)Elements[i]).Fitness)
                        Elements[i] = newM2Plus;
                }
            }
        }
    }
}
