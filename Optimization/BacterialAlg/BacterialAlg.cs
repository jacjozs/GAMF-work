using System;
using System.Collections;

namespace Optimization
{
    public class BacterialAlg : BaseOptimizationMethod
    {
        /// <summary>
        /// Génátadások száma
        /// </summary>
        public int Infections { get; set; }
        /// <summary>
        /// egyedengénti maximálasan generált klonok száma
        /// a random generálás maximuma
        /// </summary>
        public int ClonesCount { get; set; }

        protected override void CreateNextGeneration()
        {
            for (int i = 0; i < NumberOfElements; i++)
            {
                Elements[i] = Mutate((BaseElement)Elements[i]);
            }
            Elements.Sort();
            GenTransfer();
        }
        /// <summary>
        /// A megadot elemn végrehajtja a mutáció folyamatát
        /// </summary>
        /// <param name="entity">mutálni kivánt elem</param>
        /// <returns>mutáción átestt elem</returns>
        private BaseElement Mutate(BaseElement entity)
        {
            ArrayList Clones = new ArrayList();
            BaseElement copy = entity;
            int rnd = RNG.Next(1, ClonesCount);
            for (int h = 0; h < rnd; h++)
            {//másolatok létrehozása
                Clones.Add(copy);
            }
            for (int p = 0; p < InitialParameters.Count; p++)
            {//paraméterenkénti modosítás majd kiértékelés
                var parameter = new ArrayList();
                for (int j = 0; j < Clones.Count; j++)
                {//minden klonon végrehajtottt modosítás
                    parameter = ((BaseElement)Clones[j]).Position;
                    parameter[p] = (double)((BaseElement)Clones[j])[p] + 3 * (RNG.NextDouble() * 2 - 1);
                    if ((double)parameter[p] > (double)UpperParamBounds[p])
                        parameter[p] = UpperParamBounds[p];
                    else if ((double)parameter[p] < (double)LowerParamBounds[p])
                        parameter[p] = LowerParamBounds[p];
                    if (Integer[p])
                        parameter[p] = Math.Round((double)parameter[p]);

                    Clones[j] = GetNewElement(FitnessFunction, parameter);
                }//a legjobb értékel rendelkező paraméterének átadása a többi klonak
                Clones.Sort();
                for (int j = 0; j < Clones.Count; j++)
                {//A legjobb értékel modosítot elem paraméterének átadása majd újraszámolása a modosítot klonban
                    ((BaseElement)Clones[j])[p] = ((BaseElement)Clones[0])[p];
                    Clones[j] = GetNewElement(FitnessFunction, ((BaseElement)Clones[j]).Position);
                }
            }//rendezés majd a legjobb klon vissza adása és kilépés
            Clones.Sort();
            return (BaseElement)Clones[0];
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
