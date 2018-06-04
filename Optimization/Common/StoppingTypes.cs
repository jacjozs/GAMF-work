namespace Optimization
{
	/// <summary>
	/// Enumerates the possible stopping types of an optimization algorithm.
	/// </summary>
	public enum StoppingType
	{
		EvaluationNumber, // Number of allowed performance evaluations.
		GenerationNumber, // number of allowed generations.
		PerformanceTreshold // Treshold for the performance.
	}
}