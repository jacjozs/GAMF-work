using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    /// <summary>
    /// Particle Elements Optimization algorithm. Implementation mainly based on
    /// Z.C. Johanyák, P.G. Ailer: Particle Elements Optimization based Tuning for Fuzzy Cruise Control, 
    /// in Proceedings of 15th IEEE International Symposium on Computational Intelligence and Informatics 
    /// (CINTI 2014 ), Ed: Anikó Szakál,19–21 November, 2014, Budapest, Hungary, pp. 21-26.
    /// </summary>
    public class ParticleSwarm : BaseOptimizationMethod
    {
        /// <summary>
        /// The best particle in the current generation.
        /// </summary>
        private Particle BestInCurrentGeneration;
        /// <summary>
        /// The best particle so far.
        /// </summary>
        private Particle GlobalBest;

        /// <summary>
        /// Multiplication factor for the distance to the personal best position.
        /// </summary>
        public double cp { get; set; }
        /// <summary>
        /// Multiplication factor for the distance to the global best position.
        /// </summary>
        public double cg { get; set; }
        /// <summary>
        /// Multiplication factor for the distance to the previous velocity value.
        /// </summary>
        public double c0 { get; set; }
        /// <summary>
        /// Method responsible for the optimization.
        /// </summary>
        /// <returns>An object containing the optimized values of the parameters as well as other characteristics of the optimization process.</returns>

        /// <summary>
        /// Returns the particle created using the initial parameters and the fitness function.
        /// </summary>
        /// <param name="fitnessFunction"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected override IElement GetNewElement(FitnessFunctionDelegate fitnessFunction, ArrayList parameters)
        {
            return new Particle(fitnessFunction, parameters, FitnessEvaluated);
        }

        /// <summary>
        /// Creates the next generation of antibodies.
        /// </summary>
        protected override void CreateNextGeneration()
        {
            // Update the position of the particles
            for (int j = 0; j < NumberOfElements; j++) // for each particle (j)
            {
                var pj = (Particle)Elements[j];
                var TempPosition = new ArrayList();
                for (int k = 0; k < InitialParameters.Count; k++) // for each dimension (k)
                {
                    var p = (double)pj.Position[k] + (double)pj.Velocity[k];
                    // Update the postition
                    if (p < (double)LowerParamBounds[k])
                        p = (double)LowerParamBounds[k];
                    if (p > (double)UpperParamBounds[k])
                        p = (double)UpperParamBounds[k];
                    if (Integer[k])
                    {
                        p = Math.Round(p); // round it if necessary
                                           // Apply the lower and upper bounds.
                        p = Math.Ceiling(Math.Max(p, (double)LowerParamBounds[k]));
                        p = Math.Floor(Math.Min(p, (double)UpperParamBounds[k]));
                    }
                    TempPosition.Add(p);
                }
                pj.Position = TempPosition;
                pj.Update();
                //Evaluation++;
                // Stop if the stopping criteria was the number of evaluations and the allowed number was reached.
                // Stop if the stopping criteria was the the performance treshold and the it was reached.
                if ((StoppingType == StoppingType.EvaluationNumber && Evaluation >= StoppingNumberOfEvaluations) ||
                        (StoppingType == StoppingType.PerformanceTreshold && (pj.Fitness <= StoppingFitnessTreshold)))
                {
                    // Set stopping state.
                    Stop = true;
                    break;
                }
            }
            BestInCurrentGeneration = (Particle)FindBestElement();
            if (GlobalBest == null || BestInCurrentGeneration.Fitness < GlobalBest.Fitness)
            {
                GlobalBest = new Particle(BestInCurrentGeneration);
            }
               
            Elements.Sort();
            UpdateVelocities();
        }

        /// <summary>
        /// Update the velocity values of all particles.
        /// </summary>
        private void UpdateVelocities()
        {
            // Update the velocities
            for (int j = 0; j < NumberOfElements; j++) // for each particle (j)
            {
                var pj = (Particle)Elements[j];
                for (int k = 0; k < InitialParameters.Count; k++) // for each dimension (k)
                    pj.Velocity[k] = c0 * (double)pj.Velocity[k] +
                                                     cp * RNG.NextDouble() * ((double)pj.BestPosition[k] - (double)pj.Position[k]) +
                                                     cg * RNG.NextDouble() * ((double)GlobalBest.Position[k] - (double)pj.Position[k]);
            }
        }

        public void reset()
        {
            this.GlobalBest = null;
        }
    }
}
