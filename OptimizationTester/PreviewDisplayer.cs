using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class PreviewDisplayer : IOptimizationMethod
    {
        public FitnessFunctionDelegate FitnessFunction { get; set; }

        public double StoppingFitnessTreshold { get; set; }

        public ArrayList InitialParameters { get; set; }

        public bool[] Integer { get; set; }

        public long StoppingNumberOfEvaluations { get; set; }

        public long StoppingNumberOfGenerations { get; set; }

        public bool Stop { get; set; }

        public StoppingType StoppingType { get; set; }

        public event EventHandlerDelegate GenerationCreated;
        /// <summary>
        /// Lower bound for the parameters.
        /// </summary>
        public ArrayList LowerParamBounds { get; set; }
        /// <summary>
        /// Upper bound for the parameters.
        /// </summary>
        public ArrayList UpperParamBounds { get; set; }

        /// <summary>
        /// Dummy function to fill required parameter role
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="unusedA"></param>
        /// <param name="unusedB"></param>
        public void FitnessEvaluated(object Sender, ArrayList unusedA, double[] unusedB)
        {

        }

        public Result Optimize()
        {
            var Individuals = new ArrayList();
            var Fitnesses = new List<Double>();

            for (int i = (int) LowerParamBounds[0]; i < (int) UpperParamBounds[0]; i++)
            {
                for (int j = (int) LowerParamBounds[1]; j < (int) UpperParamBounds[1]; j++)
                {
                    Individuals.Add(new BaseElement(FitnessFunction, new ArrayList { (double)i, (double)j }, FitnessEvaluated));
                    Fitnesses.Add(FitnessFunction(new ArrayList { (double)i, (double)j }));
                }
            }
            GenerationCreated?.Invoke(this, Individuals, Fitnesses.ToArray());
            return null;
        }
    }
}
