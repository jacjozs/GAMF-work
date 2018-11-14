using System.Collections;
using System.Collections.Generic;

namespace Optimization
{
	/// <summary>
	/// Stores information about the resutls of the optimization. 
	/// </summary>
	public class Result
	{
		/// <summary>
		/// Vector containing the optimized values of the parameters.
		/// </summary>
		public ArrayList OptimizedParameters;
		/// <summary>
		/// Vector containing other information about the optimization process.
		/// </summary>
		public Dictionary<InfoTypes, object> InfoList;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="op">Vector containing the optimized values of the parameters.</param>
		/// <param name="il">Vector containing information about the optimization process.</param>
		public Result(ArrayList op, Dictionary<InfoTypes, object> il)
		{
			OptimizedParameters = op;
			InfoList = il;
		}
	}
}
