using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    /// <summary>
    /// Nelder-Mead Optimization Algorithm
    /// Az ajánlott egyedszám 20-25 közti érték
    /// </summary>
    public class NelderMead : BaseOptimizationMethod
    {
        /// <summary>
        /// Reflected pont távolsága
        /// </summary>
        private double alpha = 1.0;
        /// <summary>
        /// Contracted pont távolsága
        /// </summary>
        private double beta = 0.5;
        /// <summary>
        /// Expanded pont távolsága
        /// </summary>
        private double gamma = 2.0;
        protected override void CreateNextGeneration()
        {
            BaseElement centroid = Centroid();
            BaseElement reflected = Reflected(centroid);
            if(reflected.Fitness < ((BaseElement)Elements[0]).Fitness)
            {
                BaseElement expanded = Expanded(reflected, centroid);
                if(expanded.Fitness < ((BaseElement)Elements[0]).Fitness)
                    ReplaceWorst(expanded);
                else
                    ReplaceWorst(reflected);
            }
            if(IsWorseThanAllButWorst(reflected))
            {
                if(reflected.Fitness <= ((BaseElement)Elements[NumberOfElements - 1]).Fitness)
                    ReplaceWorst(reflected);
                BaseElement contracted = Contracted(centroid);
                if (contracted.Fitness < ((BaseElement)Elements[0]).Fitness)
                    ReplaceWorst(contracted);
                else
                    Shrink();
                return;
            }
            ReplaceWorst(reflected);
        }
        /// <summary>
        /// Centroid pont kiszámítása
        /// </summary>
        /// <returns></returns>
        private BaseElement Centroid()
        {
            var parameter = new ArrayList();
            for (int i = 0; i < InitialParameters.Count; i++)
            {
                parameter.Add(0.0);
            }
            for (int i = 0; i < NumberOfElements - 1; i++)
            {
                for (int p = 0; p < InitialParameters.Count; p++)
                {
                    parameter[p] = (double)parameter[p] + (double)((BaseElement)Elements[i])[p];
                }
            }
            for (int p = 0; p < InitialParameters.Count; p++)
            {
                parameter[p] = (double)parameter[p] / (NumberOfElements - 1);
            }
            return (BaseElement)GetNewElement(FitnessFunction, parameter);
        }
        /// <summary>
        /// Reflected pont kisszámítása
        /// </summary>
        /// <param name="centroid"></param>
        /// <returns></returns>
        private BaseElement Reflected(BaseElement centroid)
        {
            var parameter = new ArrayList();
            var worstParam = ((BaseElement)Elements[NumberOfElements - 1]).Position;
            for (int p = 0; p < InitialParameters.Count; p++)
            {
                parameter.Add(((1.0 + alpha) * (double)centroid[p]) - (alpha * (double)worstParam[p]));
            }
            return (BaseElement)GetNewElement(FitnessFunction, parameter);
        }
        /// <summary>
        /// Expanded pont kiszámítása
        /// </summary>
        /// <param name="reflected"></param>
        /// <param name="centroid"></param>
        /// <returns></returns>
        private BaseElement Expanded(BaseElement reflected, BaseElement centroid)
        {
            var parameter = new ArrayList();
            for (int p = 0; p < InitialParameters.Count; p++)
            {
                parameter.Add((gamma * (double)reflected[p]) + ((1 - gamma) * (double)centroid[p]));
            }
            return (BaseElement)GetNewElement(FitnessFunction, parameter);
        }
        /// <summary>
        /// Contracted pont kiszámítása
        /// </summary>
        /// <param name="centroid"></param>
        /// <returns></returns>
        private BaseElement Contracted(BaseElement centroid)
        {
            var parameter = new ArrayList();
            var worstParam = ((BaseElement)Elements[NumberOfElements - 1]).Position;
            for (int p = 0; p < InitialParameters.Count; p++)
            {
                parameter.Add((beta * (double)worstParam[p]) + ((1 - beta) * (double)centroid[p]));
            }
            return (BaseElement)GetNewElement(FitnessFunction, parameter);
        }
        /// <summary>
        /// Koordináták zsugorítása
        /// </summary>
        private void Shrink()
        {
            for (int i = 0; i < NumberOfElements; i++)
            {
                var parameter = new ArrayList();
                for (int p = 0; p < InitialParameters.Count; p++)
                {
                    parameter.Add(((double)((BaseElement)Elements[i])[p] + (double)((BaseElement)Elements[0])[p]) / 2.0);
                }
                Elements[i] = GetNewElement(FitnessFunction, parameter);
            }
            Elements.Sort();
        }
        /// <summary>
        /// Legrosszab érték beállítása és az egyedek rendezése
        /// </summary>
        /// <param name="newSolution"></param>
        private void ReplaceWorst(BaseElement newSolution)
        {
            Elements[NumberOfElements - 1] = newSolution;
            Elements.Sort();
        }
        /// <summary>
        /// Megnézi, hogy rosszabb e a megadott elem mint a többi egyed, kivéve a legrossza egyet
        /// </summary>
        /// <param name="reflected"></param>
        /// <returns></returns>
        private bool IsWorseThanAllButWorst(BaseElement reflected)
        {
            for (int i = 0; i < NumberOfElements - 1; ++i)
            {
                if (reflected.Fitness <= ((BaseElement)Elements[i]).Fitness)
                    return false;
            }
            return true;
        }
    }
}
