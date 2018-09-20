using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class BeeAlg : BaseOptimizationMethod
    {
        /// <summary>
        /// Felderitő méhek rádiusza
        /// </summary>
        public double ExploratoryRadius { get; set; }
        /// <summary>
        /// Keresőméhek rádiusza
        /// a kereső méhek a felderitők általt talált pont körül keresnek
        /// </summary>
        public double SearchRadius { get; set; }
        /// <summary>
        /// Azon méhek száma akik találtak megfelelő pontot
        /// </summary>
        private int Elite { get; set; }
        /// <summary>
        /// Felderitő méhek által megtehető maximális utak száma 1 ciklus alatt
        /// </summary>
        public int MaxStep { get; set; }
        /// <summary>
        /// Elit méhek listály
        /// BaseElement
        /// </summary>
        public ArrayList EliteBees { get; set; }
        /// <summary>
        /// Elit méhek által talált pontokhoz (virágokhoz) tartozó kereső méhek száma
        /// </summary>
        public int[] Flowers { get; set; }

        protected override void CreateNextGeneration()
        {

            int count = RNG.Next(1, 10);
            for (int i = 0; i < count; i++)
            {//felderitő méhek keresése, A kiindulási pont lesz a kaptár ahonnan keresnek a felderítők
                Exploratory();
            }
            EliteBees.Sort();
            Elite = EliteBees.Count;

            UpdateFollowerSizes();
            // kereső méhek listája
            var ExploratoryBees = new ArrayList();
            // A kiindulási pont fitness értéke
            double OldFitness;
            // radiuszon belüli keresés és validálás
            for (int k = 0; k < Flowers.Length; k++)
            {
                OldFitness = ((BaseElement)EliteBees[k]).Fitness;
                for (int i = 0; i < Flowers[k]; i++)
                {
                    var parameter = new ArrayList();
                    for (int p = 0; p < InitialParameters.Count; p++)
                    {
                        parameter.Add(((BaseElement) EliteBees[k])[p]);
                        // a kereső méhet a kezdőpontól a megfelelő sugáron belülre mozgatja.
                        parameter[p] = (double)((BaseElement)EliteBees[k])[p] + SearchRadius * (RNG.NextDouble() * 2 - 1);

                        if ((double)parameter[p] > (double)UpperParamBounds[p])
                            parameter[p] = UpperParamBounds[p];
                        else if ((double)parameter[p] < (double)LowerParamBounds[p])
                            parameter[p] = LowerParamBounds[p];
                        if (Integer[p])
                            parameter[p] = Math.Round((double)parameter[p]);
                    }

                    ExploratoryBees.Add(GetNewElement(FitnessFunction, parameter));
                    if (((BaseElement)ExploratoryBees[i]).Fitness < OldFitness)
                    {
                        EliteBees[k] = ExploratoryBees[i];
                    }
                }
            }
            ExploratoryBees.AddRange(Elements);
            ExploratoryBees.Sort();
            Elements[0] = ((BaseElement)ExploratoryBees[0]);
            for (int j = 1; j < NumberOfElements; j++)
            {//új radom poziciokat válogatunk az elemek helyére
                Elements[j] = (BaseElement)ExploratoryBees[RNG.Next(ExploratoryBees.Count)];
            }
            EliteBees.Sort();
            //Minden 20. generáció után az Elit méhek számát megfelezűk
            if (Generation % 20 == 0 && EliteBees.Count > 2)
                EliteBees.RemoveRange((Elite / 2) - 1, EliteBees.Count - (Elite / 2));
        }
        /// <summary>
        /// A felderítő méhek által talált virágokhoz kirendelni  keresőket
        /// </summary>
        private void UpdateFollowerSizes()
        {//A felderitő méhek által talált virágokhoz mennyi keresőt kell rendelni
            Flowers = new int[Elite];
            for (int i = 0; i < Flowers.Length; i++)
            {
                Flowers[i] = (int)Math.Round(double.Parse((((NumberOfElements - Elite) / Elite) * ((Elite - i) / 2) == 0 ? 1 : ((Elite - i) / 2)).ToString()));
            }
        }
        /// <summary>
        /// Felderítő méhek keresése
        /// </summary>
        private void Exploratory()
        {
            double Radius = ExploratoryRadius + (ExploratoryRadius * (Generation / 10));
            BaseElement Start = (BaseElement)GetNewElement(FitnessFunction, InitialParameters);
            double OldFitness = Start.Fitness;
            for (int i = 0; i < MaxStep; i++)
            {
                for (int p = 0; p < Start.Position.Count; p++)
                {
                    Start.Position[p] = (double)Start.Position[p] + Radius > (double)UpperParamBounds[p]? (double)UpperParamBounds[p] : Radius * i * (RNG.NextDouble() * 2 - 1);
                    if ((double)Start.Position[p] > (double)UpperParamBounds[p])
                        Start.Position[p] = UpperParamBounds[p];
                    else if ((double)Start.Position[p] < (double)LowerParamBounds[p])
                        Start.Position[p] = LowerParamBounds[p];
                    if (Integer[p])
                        Start.Position[p] = Math.Round((double)Start.Position[p]);
                }

                Start = (BaseElement)GetNewElement(FitnessFunction, Start.Position);
                //Ha az új pozicó jobb mint a kezdeti akkor azt hozzá adja az Elitekhez
                if (Start.Fitness < OldFitness)
                {
                    EliteBees.Add(Start);
                    return;
                }
            }
        }
    }
}
