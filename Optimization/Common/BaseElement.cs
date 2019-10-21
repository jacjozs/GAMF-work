using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class BaseElement : IElement, IComparable, ICloneable
    {
        /// <summary>
		/// Fired after the fitness of an antibody is evaluated.
		/// </summary>
		public event EventHandlerDelegate FitnessEvaluated;
        /// <summary>
		/// Position vector. A tuple of parameters. 
		/// </summary>
		public ArrayList Position { get; set; }
        /// <summary>
		/// Fitness (performance) of a particle. Greater than or equal to 0, where 0 is the best value.
		/// </summary>
		public double Fitness { get; set; }
        /// <summary>
        /// Fitness function used by the optimization.
        /// </summary>
        protected FitnessFunctionDelegate FitnessFunction;

        /// <summary>
		/// Initializes the fields of an element and calculates the fitness of the position vector.
		/// </summary>
		/// <param name="fitnessFunction">Function used for the affinity calculation.</param>
		/// <param name="parameters">The new parameter tuple.</param>
		public BaseElement(FitnessFunctionDelegate fitnessFunction, ArrayList parameters, EventHandlerDelegate fitnessEvaluated)
        {
            Position = new ArrayList();
            foreach (var d in parameters)
            {
                Position.Add(d);
            }
            FitnessFunction = fitnessFunction;
            FitnessEvaluated += fitnessEvaluated;
            Update();
        }

        public Delegate[] GetFitnessEvaluatedInvocationList()
        {
            return FitnessEvaluated.GetInvocationList();
        }

        /// <summary>
		/// Gets or sets the value of a parameter from the position vector.
		/// </summary>
		/// <param name="index">Original number (index) of the vector element.</param>
		/// <returns>The actual value of the given element of the position vector.</returns>
        public object this[int index]
        {
            get { return Position[index]; }
            set
            {
                Position[index] = value;
                // If one of the position vector values is modified, the affinity of the element has to be recalculated.
                Update();
            }
        }

        /// <summary>
		/// Recalculates the fitness of the individual.
		/// </summary>
        public virtual void Update()
        {
            Fitness = FitnessFunction(Position);
            FitnessEvaluated?.Invoke(this, null, null);
        }

        /// <summary>
		/// Compares the fitness of an element to the fitness of the current element. 
		/// </summary>
		/// <param name="other">The element we want to compare to.</param>
		/// <returns>1 if the fitness of the examined element is less (better) than the fitness of the current element. -1 if the fitness of the examined element is greater than the fitness of the current element. 0 otherwise.</returns>
        public int CompareTo(object other)
        {
            var otherElement = (BaseElement)other;
            if (otherElement.Fitness < Fitness)
                return 1;
            if (otherElement.Fitness > Fitness)
                return -1;
            return 0;
        }

        /// <summary>
        /// Returns the Manhattan distance between this and an other element
        /// </summary>
        /// <param name="otherElement"></param>
        /// <returns></returns>
        public double DistanceTo(BaseElement otherElement)
        {
            var dist = 0.0;
            for (int i = 0; i < Position.Count - 1; i++)
            {
                dist += Math.Abs((double)Position[i] - (double)otherElement[i]);
            }
            return dist;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
