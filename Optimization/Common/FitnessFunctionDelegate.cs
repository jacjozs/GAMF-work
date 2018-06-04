using System.Collections;

namespace Optimization
{
	/// <summary>
	/// Delegate for the fitness function. The calculated fitness should be greater or equal to 0, where 0 is the best value.
	/// </summary>
	/// <param name="ActualParameters">Array containing the parameters to be evaluated.</param>
	/// <returns>Fitness value.</returns>
	public delegate double FitnessFunctionDelegate(ArrayList ActualParameters);
}
