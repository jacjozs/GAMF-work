using System;
using System.Collections;
using System.Linq;

namespace Optimization
{
    /// <summary>
    /// ABC = Artificial Bee Colony
    /// </summary>
    public class ABC : BaseOptimizationMethod
    {
        /// <summary>
        /// A felderítő méhek száma
        /// </summary>
        public int Elite { get; set; }
        /// <summary>
        /// Felderitő méhek által megtehető maximális utak száma 1 ciklus alatt
        /// </summary>
        public int MaxStep { get; set; }
        /// <summary>
        /// Elit méhek listály
        /// BaseElement
        /// </summary>
        private ArrayList EliteBees { get; set; }
        /// <summary>
        /// Elit méhek által talált pontokhoz (virágokhoz) tartozó kereső méhek száma
        /// </summary>
        private int[] Flowers { get; set; }

        protected override void CreateNextGeneration()
        {
            //Kivétel kezelés
            if (NumberOfElements < Elite) throw new ArgumentException("Elite parameter cannot be greater than NumberOfElements", "original");
            if (EliteBees == null) EliteBees = new ArrayList();
            for (int i = 0; i < Elite; i++) Exploratory((BaseElement)Elements[i], i);//Felderítés 
            if (EliteBees.Count > Elite)
            {// Ha több felderített pont van mint az előírt akkor a legrosszabbakat törli
                EliteBees.Sort();
                EliteBees.RemoveRange(Elite, EliteBees.Count - Elite);
            }
            UpdateFollowerSizes();//Kereső méhek kiosztása
            int EliteIndex = Elite;
            int rnd = 0, BeeGroupI = 0;
            for (int k = 0; k < Flowers.Length; k++)
            {
                double OldFitness = ((BaseElement)EliteBees[EliteBees.Count - 1]).Fitness;
                for (int i = 0; i < Flowers[k]; i++)
                {
                    BeeGroupI = EliteIndex + i;//A keresési csoporton belüli méh indexe
                    var parameter = new ArrayList();
                    for (int p = 0; p < InitialParameters.Count; p++)
                    {
                        parameter.Add(((BaseElement)EliteBees[k])[p]);
                        do rnd = RNG.Next(NumberOfElements); while (rnd == i);
                        // a kereső méhet a kezdőpontól a megfelelő sugáron belülre mozgatja.
                        parameter[p] = (double)parameter[p] + ((double)parameter[p] - (double)((BaseElement)Elements[rnd])[p]) * (RNG.NextDouble() * 2 - 1);
                        if ((double)parameter[p] > (double)UpperParamBounds[p])
                            parameter[p] = UpperParamBounds[p];
                        else if ((double)parameter[p] < (double)LowerParamBounds[p])
                            parameter[p] = LowerParamBounds[p];
                        if (Integer[p])
                            parameter[p] = Math.Round((double)parameter[p]);
                    }
                    Elements[BeeGroupI] = GetNewElement(FitnessFunction, parameter);
                    if (((BaseElement)Elements[BeeGroupI]).Fitness < OldFitness) EliteBees[k] = Elements[BeeGroupI];
                }
                EliteIndex += Flowers[k];//Tovább lépés a következő kereső csoportra
            }
            if (EliteBees.Count >= (NumberOfElements - Elite) / 2)
            {
                EliteBees.Sort();
                EliteBees.RemoveRange((EliteBees.Count / 2) - 1, EliteBees.Count / 2);
            }
            Elements.Sort();
        }
        /// <summary>
        /// A felderítő méhek által talált virágokhoz kirendelni  keresőket
        /// </summary>
        private void UpdateFollowerSizes()
        {//A felderitő méhek által talált virágokhoz mennyi keresőt kell rendelni
            Flowers = new int[EliteBees.Count];
            var count = NumberOfElements - Elite;
            EliteBees.Sort();
            for (int i = 0; EliteBees.Count != 0 && count / EliteBees.Count > 0; i++)
            {
                Flowers[i % EliteBees.Count] += (int)Math.Round((double)count / EliteBees.Count);
                count -= Flowers[i % EliteBees.Count];
            }
            if (Flowers.Length >= 1)
            {
                Flowers[0] += count;//A fennmaradt egyedeket a legjobb kereséséhez állitja be
                Flowers[0] += (NumberOfElements - Elite) - Flowers.Sum();//Az osztás miatt keletkezett veszteségeket a legjobbnak adja
            }
        }
        /// <summary>
        /// Felderítő méhek keresése
        /// </summary>
        private void Exploratory(BaseElement bee, int index)
        {
            BaseElement old = (BaseElement)GetNewElement(FitnessFunction, bee.Position);
            int rnd = 0, Step = 0;
            while (MaxStep != Step)//Ismétlés ameddig el nem éri a maximálos lépés számot
            {
                for (int p = 0; p < InitialParameters.Count; p++)
                {
                    do rnd = RNG.Next(NumberOfElements); while (rnd == index);//Egy olyan elem keresése ami nem egyenlő a kiválasztot elemel
                    bee.Position[p] = (double)bee[p] + ((double)bee[p] - (double)((BaseElement)Elements[rnd])[p]) * (RNG.NextDouble() * 2 - 1);
                    if ((double)bee.Position[p] > (double)UpperParamBounds[p])
                        bee.Position[p] = UpperParamBounds[p];
                    else if ((double)bee.Position[p] < (double)LowerParamBounds[p])
                        bee.Position[p] = LowerParamBounds[p];
                    if (Integer[p])
                        bee.Position[p] = Math.Round((double)bee.Position[p]);
                }

                bee = (BaseElement)GetNewElement(FitnessFunction, bee.Position);
                //Ha az új pozicó jobb mint a kezdeti akkor azt hozzá adja az Elitekhez
                if (bee.Fitness < old.Fitness)
                {
                    Elements[index] = bee;
                    EliteBees.Add(bee);
                    return;
                }
                else Elements[index] = old;
                Step++;
            }
        }
    }
}