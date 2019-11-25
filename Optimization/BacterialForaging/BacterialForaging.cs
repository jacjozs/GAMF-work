using System;
using System.Collections;

namespace Optimization
{
    /// <summary>
    /// Bacterial Foraging Optimization Algorithm
    /// TODO fenn áll a túlszámolás, ezért nem javasólt a fitnesz értékes megállási feltétel
    /// </summary>
    public class BacterialForaging : BaseOptimizationMethod
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
            for (int i = 0; i < NumberOfElements; i++) Cemotoxia(i); 
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
            if(((BaseElement)Elements[index]).Fitness > ((BaseElement)Clones[0]).Fitness)
                Elements[index] = Clones[0];
        }
        /// <summary>
        /// Feladata a génátadás végrehajtása a populáscioban
        /// </summary>
        private void GenTransfer()
        {
            int GoodIndex;//A jó populácioból kiválasztot egyed indexe
            int BadIndex;//A rossz populácioból kiválasztot egyed indexe
            int Chromosome_1, Chromosome_2;//Az átadandó kromoszoma indexe (x, y .. koordináták)
            for (int i = 0; i < Infections; i++)
            {
                GoodIndex = RNG.Next(0, NumberOfElements / 2);//Jó elem indexének kiválasztása
                BadIndex = RNG.Next(NumberOfElements / 2, NumberOfElements);// Rossze elem indexének kiválasztása
                Chromosome_1 = RNG.Next(0, InitialParameters.Count);// Random kromoszoma kiválasztása
                Chromosome_2 = RNG.Next(0, InitialParameters.Count);// Random kromoszoma kiválasztása
                double goodValue = (double)((BaseElement)Elements[GoodIndex])[Chromosome_1];
                var parameter = new ArrayList();
                for (int p = 0; p < InitialParameters.Count; p++)
                {
                    if (p == Chromosome_2)
                        parameter.Add(goodValue);
                    else
                        parameter.Add(((BaseElement)Elements[BadIndex])[p]);
                    if ((double)parameter[p] > (double)UpperParamBounds[p])
                        parameter[p] = UpperParamBounds[p];
                    else if ((double)parameter[p] < (double)LowerParamBounds[p])
                        parameter[p] = LowerParamBounds[p];
                    if (Integer[p])
                        parameter[p] = Math.Round((double)parameter[p]);
                }
                Elements[BadIndex] = GetNewElement(FitnessFunction, parameter);// Így kapott paraméter lista számítás
            }
        }
    }
}