using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    /// <summary>
    /// https://www.doc.ic.ac.uk/~nd/surprise_96/journal/vol1/hmw/article1.html
    /// </summary>
    public class GeneticAlgorithm : BaseOptimizationMethod
    {
        /// <summary>
        /// Number of parents in each generation.
        /// </summary>
        public int ParentsInEachGeneration;
        /// <summary>
        /// The probability of mutation.
        /// </summary>
        public double MutationProbability;
        /// <summary>
        /// The number of crossovers in each generation.
        /// </summary>
        public int CrossoverPerGeneration;
        
        /// <summary>
		/// Creates the next generation of antibodies.
		/// </summary>
		protected override void CreateNextGeneration()
        {
            //do crosses between the best parent candidates
            for (int i = 0; i < CrossoverPerGeneration; i++)
            {
                //select the 2 (different) individuals
                int i1 = RNG.Next(ParentsInEachGeneration);
                int i2 = i1;
                while (i2 == i1)
                    i2 = RNG.Next(ParentsInEachGeneration);
                //cross them over
                Crossover((BaseElement)Elements[i1], (BaseElement)Elements[i2]);
            }
            //mutate the children
            for(int i = NumberOfElements; i < Elements.Count; i++)
            {
                if (RNG.NextDouble() < MutationProbability)
                    Mutate((BaseElement)Elements[i]);
            }
            //sort
            Elements.Sort();
            //keep only the best ones
            Elements.RemoveRange(NumberOfElements, Elements.Count - NumberOfElements);
        }

        /// <summary>
        /// Create a random child of 2 elements and add it to the array
        /// </summary>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        private void Crossover(BaseElement element1, BaseElement element2)
        {
            var p = new ArrayList();
            for (int dimension = 0; dimension < InitialParameters.Count; dimension++)
            {
                if (RNG.NextDouble() < 0.5)
                    p.Add(element1[dimension]);
                else
                    p.Add(element2[dimension]);
            }
            Elements.Add(GetNewElement(FitnessFunction, p));
            //Evaluation++;
        }

        /// <summary>
        /// Randomize some parameters of the element
        /// </summary>
        /// <param name="element"></param>
        private void Mutate(BaseElement element)
        {
            bool changed = false;

            //for each dimension
            for (int i = 0; i < InitialParameters.Count; i++)
            {
                //for some of them
                if (RNG.NextDouble() < MutationProbability)
                {
                    //randomize the value
                    element[i] = RNG.NextDouble() * ((double)UpperParamBounds[i] - (double)LowerParamBounds[i]) + (double)LowerParamBounds[i];
                    changed = true;
                }
            }

            if (!changed)
                return;
            
            element.Update();
            //Evaluation++;
        }
    }
}
