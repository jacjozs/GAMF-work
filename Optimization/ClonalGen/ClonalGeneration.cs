using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Optimization
{
	/// <summary>
	/// Real-value encoded version of the CLONALG algorithm. Implementation mainly based on
	/// Z.C. Johanyák: Clonal Selection Based Parameter Optimization for Sparse Fuzzy Systems, 
	/// in Proceedings of IEEE 16th International Conference on Intelligent Engineering Systems 
	/// (INES 2012), June 13–15, 2012, Lisbon, Portugal, pp. 369-373, DOI: 10.1109/INES.2012.6249861
	/// </summary>
	public class ClonalGeneration : BaseOptimizationMethod
	{
		/// <summary>
		/// Number of antibodies selected for cloning.
		/// </summary>
		public int NumberSelectedForCloning;
		/// <summary>
		/// Clone number coefficient. ( ]0,1] )
		/// </summary>
		public double Beta;
		/// <summary>
		/// Mutation coefficient. ( ]0,1] )
		/// </summary>
		public double Ro;
		/// <summary>
		/// The number of new antibodies created at the end of each generation.
		/// </summary>
		public int RandomAntibodiesPerGeneration;
        
		protected override void CreateNextGeneration()
		{
			// Calculate the number of clones.
			var Nclones = (int)Math.Round(NumberOfElements * Beta);

			// Determine best and worst affinity (needed for the probability of hypermutation) 
			// for the antibodies selected to be cloned.
			var BestAffinity = ((BaseElement)Elements[0]).Fitness;
			var WorstAffinity = ((BaseElement)Elements[NumberOfElements - 1]).Fitness;
			// Determine performance index [0,1] (relative affinity), 
			// 0 - worst possible performance, 1 - best possible performance.
			var PI = new double[NumberOfElements];  
			for (int i = 0; i < NumberOfElements; i++)
			{ // PI = (worst-current)/(worst-best).
				PI[i] = (WorstAffinity-((BaseElement)Elements[i]).Fitness) / (WorstAffinity-BestAffinity);
			}
			// Calculate the probability of maturation.
			var p = new double[NumberSelectedForCloning];
			for (int i = 0; i < NumberSelectedForCloning; i++)
			{
				p[i] = 1 / Math.Pow(Math.E, Ro * PI[i]);
			}

			// Create clones of the best n antibodies.
			// Number of dimensions (patameters) of an antibody.
			var dimno = InitialParameters.Count;
            for (int antibody = 0; antibody < NumberSelectedForCloning; antibody++)
			{
				for (int clone = 0; clone < Nclones; clone++)
				{
					// Create a temporary position vector.
					var TempPosition = new ArrayList();
                    for (var i = 0; i < InitialParameters.Count; i++) 
						TempPosition.Add(((BaseElement)Elements[antibody])[i]);
					for (int dim = 0; dim < dimno;dim++)
					{
						// Do hypermutation if condition is fulfilled.
						if (RNG.NextDouble() < p[antibody])
						{
                            TempPosition[dim] = (double)LowerParamBounds[dim] + RNG.NextDouble() * ((double)UpperParamBounds[dim] - (double)LowerParamBounds[dim]);
							// Round if necessary
							if (Integer[dim])
							{
								TempPosition[dim]=Math.Round((double)TempPosition[dim]);
								// Apply the lower and upper bounds.
								TempPosition[dim] = Math.Ceiling(Math.Max((double)TempPosition[dim], (double)LowerParamBounds[dim]));
								TempPosition[dim] = Math.Floor(Math.Min((double)TempPosition[dim], (double)UpperParamBounds[dim]));
							}
						}
					}
					// Create clone with the defined position.
					var Clone = GetNewElement(FitnessFunction, TempPosition);
					//Evaluation++;
                    //Add clone to the pool of antibodies.
                    Elements.Add(Clone);
					// Stop if the stopping criteria was the number of evaluations and the allowed number was reached.
					// Stop if the stopping criteria was the the performance treshold and the it was reached.
					if ((StoppingType == StoppingType.EvaluationNumber && Evaluation >= StoppingNumberOfEvaluations)|| (StoppingType == StoppingType.PerformanceTreshold && ((BaseElement)Elements[0]).Fitness <= StoppingFitnessTreshold))
					{
                        // Sort antibodies in ascending order conform their affinity (the first will be the best one).
                        Elements.Sort();
						// Set stopping state.
						Stop = true;
						return;
					}
				}
			}
            // Sort antibodies in ascending order conform their affinity (the first will be the best one).
            Elements.Sort();
			// List();
			// Keep the (N-d) best ones 
			// First to be removed: N-d
			// Number of removed elements: Elements.Count-(N-d)
			Elements.RemoveRange(NumberOfElements - RandomAntibodiesPerGeneration, Elements.Count - NumberOfElements + RandomAntibodiesPerGeneration);
			// Create d new antibodies at random positions, add the to the pool, and sort the resulting pool.
			CreateRandomElements(RandomAntibodiesPerGeneration);
		}
	}
}
