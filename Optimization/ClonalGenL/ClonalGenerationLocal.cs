using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class ClonalGenerationLocal : ClonalGeneration
    {
        /// <summary>
        /// Stores the actual value for StepSizeRelative
        /// </summary>
        private double _StepSizeRelative;
        /// <summary>
        /// The local search area parameter, expressed as a multiplier for the entire search area's size
        /// </summary>
        public double StepSizeRelative
        {
            get { return _StepSizeRelative; }
            set
            {
                _StepSizeRelative = value;
                StepSizeAbsoulte = new double[UpperParamBounds.Count];
                for (int dimId = 0; dimId < StepSizeAbsoulte.Length; dimId++)
                {
                    StepSizeAbsoulte[dimId] = (((double)UpperParamBounds[dimId]) - ((double)LowerParamBounds[dimId])) * StepSizeRelative;
                }
            }
        }

        /// <summary>
        /// Absolute step size for each dimension, initialized by setting StepSizeRelative; do not edit directly!
        /// </summary>
        protected double[] StepSizeAbsoulte;

        /// <summary>
        /// Local searches performed in each generation
        /// </summary>
        public int LocalSearchesPerGeneration;

        public ClonalGenerationLocal()
        {
            LocalSearchesPerGeneration = 1;
        }

        /// <summary>
        /// Calls base class functionality, then performs local search around the best element
        /// </summary>
        protected override void CreateNextGeneration()
        {
            base.CreateNextGeneration();

            double randomScale = 0.0;

            for (int n = 0; n < LocalSearchesPerGeneration; n++)
            {
                randomScale = RNG.NextDouble();
                for (int dimId = 0; dimId < InitialParameters.Count; dimId++)
                {
                    // Get a copy of the original parameters of the best element (0-index)
                    ArrayList newParamsPlus = new ArrayList();
                    ArrayList newParamsMinus = new ArrayList();
                    for (int i = 0; i < InitialParameters.Count; i++)
                    {
                        newParamsPlus.Add((double)((BaseElement)Elements[0])[i]);
                        newParamsMinus.Add((double)((BaseElement)Elements[0])[i]);
                    }

                    // Set the new param of the dimId-th dimension to a slightly smaller/greater value
                    newParamsPlus[dimId] = Math.Min((double)((BaseElement)Elements[0])[dimId] + StepSizeAbsoulte[dimId] * randomScale, (double)UpperParamBounds[dimId]);
                    newParamsMinus[dimId] = Math.Max((double)((BaseElement)Elements[0])[dimId] + StepSizeAbsoulte[dimId] * randomScale, (double)LowerParamBounds[dimId]);

                    if (Integer[dimId])
                        newParamsPlus[dimId] = Math.Round((double)newParamsPlus[dimId]);
                    if (Integer[dimId])
                        newParamsMinus[dimId] = Math.Round((double)newParamsMinus[dimId]);

                    // Create the new elements using the modified parameters
                    BaseElement newElementPlus = (BaseElement)GetNewElement(FitnessFunction, newParamsPlus);
                    BaseElement newElementMinus = (BaseElement)GetNewElement(FitnessFunction, newParamsMinus);

                    // Replace the 0-index (best) element with a modified particle if it is better
                    if (newElementPlus.Fitness < ((BaseElement)Elements[0]).Fitness)
                        Elements[0] = newElementPlus;
                    if (newElementMinus.Fitness < ((BaseElement)Elements[0]).Fitness)
                        Elements[0] = newElementMinus;
                }
            }
        }
    }
}
