using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
	public class Particle : BaseElement
	{
		/// <summary>
		/// Velocity vector.
		/// </summary>
		public ArrayList Velocity { get; set; }
		/// <summary>
		/// Best position of the current particle.
		/// </summary>
		public ArrayList BestPosition { get; set; }
		/// <summary>
		/// The fitness value corresponding to the best position of the curent particle.
		/// </summary>
		public double BestFitness { get; set; }

		/// <summary>
		/// Initializes the fields of a particle and calculates the fitness of the position vector.
		/// </summary>
		/// <param name="ffd">Function used for the fitness calculation.</param>
		/// <param name="p">The new parameter tuple.</param>
		public Particle(FitnessFunctionDelegate ffd, ArrayList pos, EventHandlerDelegate fitnessEvaluated) : base(ffd, pos, fitnessEvaluated)
		{
			// Initial velocity is 0.0 in each dimension.
			Velocity = new ArrayList();
			for (int i = 0; i < Position.Count; i++)
			{
				Velocity.Add(0.0);
			}
			BestFitness=Double.MaxValue;
		}
        
		/// <summary>
		/// Initializes the fields of a particle with another particle.
		/// </summary>
		/// <param name="ffd">Function used for the fitness calculation.</param>
		/// <param name="p">The new parameter tuple.</param>
		public Particle(Particle particle) : base(particle.FitnessFunction, new ArrayList(particle.Position), (EventHandlerDelegate)particle.GetFitnessEvaluatedInvocationList()[0])
		{
			FitnessFunction = particle.FitnessFunction;
			Fitness = particle.Fitness;
            BestPosition = new ArrayList();
			foreach (var d in particle.BestPosition)
			{
				BestPosition.Add(d);
			}
			BestFitness = particle.BestFitness;
            Velocity = new ArrayList();
			foreach (var v in particle.Velocity)
			{
				Velocity.Add(v);
			}
		}
        
        /// <summary>
		/// Update, then store the current position and fitness if it is better one so far.
		/// </summary>
		public override void Update()
		{
            base.Update();
			if (BestPosition == null || Fitness < BestFitness)  // if the best position is not yet set OR if the new fitness is less than the best fitness so far
			{
				// The current postition becomes the best one
				BestPosition = new ArrayList();
				foreach (object t in Position)
				{
					BestPosition.Add(t);
				}
				// and the new fitness is the best
				BestFitness = Fitness;
			}
		}
	}
}
