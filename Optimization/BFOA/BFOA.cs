using System;
using System.Collections;

namespace Optimization
{
    /// <summary>
    /// BFOA = Bacterial Foraging Optimization Algorithm
    /// </summary>
    public class BFOA : BaseOptimizationMethod
    {
        /// <summary>
        /// Génátadások száma
        /// </summary>
        public int Infections { get; set; }
        /// <summary>
        /// Egyedengénti generált klonok száma
        /// </summary>
        public int ClonesCount { get; set; }
        protected override void CreateNextGeneration()
        {
            for (int i = 0; i < NumberOfElements; i++)
            {
                Cemotoxia(i);
            }
            Elements.Sort();
            GenTransfer();
        }
        /// <summary>
        /// A sejetek mozgását rekonstruáló methodus
        /// Az elemből klónokat hoz létre majd azokat módosítja és a legjobbal felcserélni a megadott elemet
        /// </summary>
        /// <param name="index">Az Elements listában elfoglalt indexe</param>
        private void Cemotoxia(int index)
        {
            int rnd = 0;
            ArrayList Clones = new ArrayList();
            for (int i = 0; i < ClonesCount; i++)//Klónok létrehozása és módosítása
            {
                Clones.Add((BaseElement)Elements[index]);
                var parameter = new ArrayList();
                for (int p = 0; p < InitialParameters.Count; p++)
                {
                    parameter.Add(((BaseElement)Clones[i])[p]);
                    do rnd = RNG.Next(NumberOfElements); while (rnd == index);
                    parameter[p] = (double)parameter[p] + ((double)parameter[p] - (double)((BaseElement)Elements[rnd])[p]) * (RNG.NextDouble() * 2 - 1);
                    if ((double)parameter[p] > (double)UpperParamBounds[p])
                        parameter[p] = UpperParamBounds[p];
                    else if ((double)parameter[p] < (double)LowerParamBounds[p])
                        parameter[p] = LowerParamBounds[p];
                    if (Integer[p])
                        parameter[p] = Math.Round((double)parameter[p]);
                }
                Clones[i] = GetNewElement(FitnessFunction, parameter);
            }
            Clones.Sort();
            Elements[index] = Clones[0];
        }
        /// <summary>
        /// Feladata a génátadás végrehajtása a populáscioban
        /// </summary>
        private void GenTransfer()
        {
            int GoodIndex;//A jó populácioból kiválasztot egyed indexe
            int BadIndex;//A rossz populácioból kiválasztot egyed indexe
            int Chromosome;//Az átadandó kromoszoma indexe (x, y .. koordináták)
            for (int i = 0; i < Infections; i++)
            {
                GoodIndex = RNG.Next(0, NumberOfElements / 2);
                BadIndex = RNG.Next(NumberOfElements / 2, NumberOfElements);
                Chromosome = RNG.Next(0, InitialParameters.Count);
                ((BaseElement)Elements[BadIndex])[Chromosome] = ((BaseElement)Elements[GoodIndex])[Chromosome];
                Elements[BadIndex] = GetNewElement(FitnessFunction, ((BaseElement)Elements[GoodIndex]).Position);
            }
        }
    }
}