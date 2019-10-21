using System.Collections;
using System.Threading.Tasks;

namespace Optimization
{
	public interface IOptimizationMethod
	{
		ArrayList InitialParameters { get; set; }
		FitnessFunctionDelegate FitnessFunction { get; set; }
		/// <summary>
		/// Shows if a dimension can only take up integer values.
		/// </summary>
		bool[] Integer { get; set; }
		/// <summary>
		/// Fired after a new generation is created. 
		/// </summary>
		event EventHandlerDelegate GenerationCreated;
        /// <summary>
        /// Stop handler
        /// </summary>
        event StopHandlerDelegate StopHandler;
        /// <summary>
        /// Defines which stopping criteria should be applied.
        /// </summary>
        StoppingType StoppingType { get; set; }
		/// <summary>
		/// The allowed number of generations/steps.
		/// </summary>
		long StoppingNumberOfGenerations { get; set; }
		/// <summary>
		/// Allowed number of fitness evaluations.
		/// </summary>
		long StoppingNumberOfEvaluations { get; set; }
		/// <summary>
		/// Affinity (fitness) treshold.
		/// </summary>
		double StoppingFitnessTreshold { get; set; }
		Result Optimize();
        Result Optimize(ArrayList Elements);
        bool Stop { get; set; }
        /// <summary>
        /// Lower bound for the parameters.
        /// </summary>
        ArrayList LowerParamBounds { get; set; }
        /// <summary>
        /// Upper bound for the parameters.
        /// </summary>
        ArrayList UpperParamBounds { get; set; }
    }
}
