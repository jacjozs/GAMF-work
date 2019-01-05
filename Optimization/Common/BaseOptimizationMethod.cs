using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class BaseOptimizationMethod : IOptimizationMethod
    {
        /// <summary>
        /// Initial parameters to optimize.
        /// </summary>
        public ArrayList InitialParameters { get; set; }
        /// <summary>
        /// The function used for the optimization.
        /// </summary>
        public FitnessFunctionDelegate FitnessFunction { get; set; }
        /// <summary>
        /// Shows if a dimension can only take up integer values.
        /// </summary>
        public bool[] Integer { get; set; }
        /// <summary>
        /// Fired after a new generation is created. 
        /// </summary>
        public event EventHandlerDelegate GenerationCreated;
        /// <summary>
        /// Defines which stopping criteria should be applied.
        /// </summary>
        public StoppingType StoppingType { get; set; }
        /// <summary>
        /// The allowed number of generations/steps.
        /// </summary>
        public long StoppingNumberOfGenerations { get; set; }
        /// <summary>
        /// Allowed number of fitness evaluations.
        /// </summary>
        public long StoppingNumberOfEvaluations { get; set; }
        /// <summary>
        /// Affinity (fitness) treshold.
        /// </summary>
        public double StoppingFitnessTreshold { get; set; }
        public bool Stop { get; set; }
        /// <summary>
        /// Lower bound for the parameters.
        /// </summary>
        public ArrayList LowerParamBounds { get; set; }
        /// <summary>
        /// Upper bound for the parameters.
        /// </summary>
        public ArrayList UpperParamBounds { get; set; }
        /// <summary>
        /// The list of elementes.
        /// </summary>
        protected ArrayList Elements;
        /// <summary>
        /// Actual number of affinity evaluations.
        /// </summary>
        public long Evaluation { get; set; }
        /// <summary>
        /// Actual number of generations.
        /// </summary>
        public long Generation { get; set; }
        /// <summary>
        /// Sets whether the generations should be slowed down
        /// </summary>
        public bool Slow { get; set; }
        /// <summary>
        /// Object for random number generation.
        /// </summary>
        protected Random RNG;
        /// <summary>
        /// The number of elements.
        /// </summary>
        public int NumberOfElements;
        /// <summary>
        /// Returned informations
        /// </summary>
        public Dictionary<InfoTypes, object> Info;

        /// <summary>
        /// Method responsible for the optimization.
        /// </summary>
        /// <returns>An object containing the optimized values of the parameters as well as other characteristics of the optimization process.</returns>
        public Result Optimize()
        {
            // Create a list object for elementes. 
            Elements = new ArrayList();
            // Create a random number generator object.
            RNG = new Random();
            // The ordinal number of the actual generation.
            Generation = 1;
            // Define a logical variable that keeps track of the current state.
            Stop = false;
            // Start execturion timer
            var timer = new System.Diagnostics.Stopwatch();
            timer.Reset();
            timer.Start();
            // Put the first element into the pool.
            Elements.Add(GetNewElement(FitnessFunction, InitialParameters));
            // Initializing the first element takes an evaluation, so we initialize to one
            Evaluation = 1;
            // Check if the stopping condition was met
            CheckStop();
            // Create an object for the returned information.
            Info = new Dictionary<InfoTypes, object>();
            // Add the affinity (performance) corresponding to the initial parameter values.
            Info.Add(InfoTypes.InitialFitness, ((BaseElement)Elements[0]).Fitness);
            // Create the rest of the initial pool (first generation) of elementes creating elementes at random positions.
            CreateRandomElements(NumberOfElements - 1);
            // Raise GenerationCreated event if there are any subscribers.
            GenerationCreated?.Invoke(this, Elements, GetFitnesses());
            ArrayList BestAffinities = new ArrayList();
            BestAffinities.Add(((BaseElement)Elements[0]).Fitness);
            CheckStop();

            //Begin the optimization process
            while (!Stop)
            {
                if (Slow)
                    System.Threading.Thread.Sleep(100);
                Generation++;
                // Create next generation.
                CreateNextGeneration();
                BestAffinities.Add(((BaseElement)Elements[0]).Fitness);
                // Raise GenerationCreated event if there are any subscribers.
                GenerationCreated?.Invoke(this, Elements, GetFitnesses());
                // Stop if the stopping criteria is the number of generations and the number of allowed generations is reached
                CheckStop();
            }
            // Stop timer
            timer.Stop();
            // Create an array for the parameters (position) of the best antibody. 
            var Best = new ArrayList();
            for (int dim = 0; dim < InitialParameters.Count; dim++)
            {
                Best.Add(((BaseElement)Elements[0])[dim]);
            }
            // Add the affinity (performance) value of the best antibody to the returned information.
            Info.Add(InfoTypes.FinalFitness, ((BaseElement)Elements[0]).Fitness);
            // Add munber of generations.
            Info.Add(InfoTypes.Generations, Generation);
            // Add number of affinity evaluations.
            Info.Add(InfoTypes.Evaluations, Evaluation);
            // Define returned values.
            Info.Add(InfoTypes.Affinities, BestAffinities);
            // Add execution time to result info
            Info.Add(InfoTypes.ExecutionTime, timer.ElapsedMilliseconds);
            var res = new Result(Best, Info);
            return res;
        }

        /// <summary>
        /// Increment Evaluation variable, to be called when particle is updated
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="unusedA"></param>
        /// <param name="unusedB"></param>
        public void FitnessEvaluated(object Sender, ArrayList unusedA, double[] unusedB)
        {
            Evaluation++;
        }

        /// <summary>
        /// Returns the element created using the initial parameters and the fitness function.
        /// </summary>
        /// <param name="fitnessFunction"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected virtual IElement GetNewElement(FitnessFunctionDelegate fitnessFunction, ArrayList parameters)
        {
            return new BaseElement(fitnessFunction, parameters, FitnessEvaluated);
        }

        /// <summary>
        /// Adds a given number of random elements to the Elements array
        /// </summary>
        /// <param name="number"></param>
        protected virtual void CreateRandomElements(int number)
        {
            for (int i = 0; i < number; i++)
            {
                // Create a temporary position vector.
                var TempPosition = new ArrayList();
                // Initialize its values with random numbers from the allowed zone.
                for (int j = 0; j < InitialParameters.Count; j++)     //for each dimension
                {
                    TempPosition.Add(RNG.NextDouble() * ((double)UpperParamBounds[j] - (double)LowerParamBounds[j]) + (double)LowerParamBounds[j]);     // generate a random position 
                    if (Integer[j])
                    {
                        TempPosition[j] = Math.Round((double)TempPosition[j]);  // round it if necessary
                                                                                // Apply the lower and upper bounds.
                        TempPosition[j] = Math.Ceiling(Math.Max((double)TempPosition[j], (double)LowerParamBounds[j]));
                        TempPosition[j] = Math.Floor(Math.Min((double)TempPosition[j], (double)UpperParamBounds[j]));
                    }
                }
                // Create a new element with the temporary position vector and add it to the pool.
                Elements.Add(GetNewElement(FitnessFunction, TempPosition));
                //Evaluation++;
                CheckStop();
                if (Stop == true)
                {
                    // Sort elements in ascending order conform their affinity (the first will be the best one).
                    Elements.Sort();
                    // Set stopping state.
                    return;
                }
            }
            // Sort the list (pool) of elementes.
            Elements.Sort();
        }
        
        /// <summary>
        /// Creates the next generation of elements.
        /// </summary>
        protected virtual void CreateNextGeneration()
        {
            
        }
        /// <summary>
        /// Sets the stop flag to true if the stopping criteria was the number of evaluations and the allowed number was reached or if the stopping criteria was the the performance treshold and the it was reached.
        /// </summary>
        /// <returns></returns>
        protected void CheckStop()
        {
            if (
                (StoppingType == StoppingType.EvaluationNumber && Evaluation >= StoppingNumberOfEvaluations) ||
                (StoppingType == StoppingType.PerformanceTreshold && ((BaseElement)Elements[0]).Fitness <= StoppingFitnessTreshold) ||
                (StoppingType == StoppingType.GenerationNumber && Generation >= StoppingNumberOfGenerations)
            )
                Stop = true;
        }

        /// <summary>
        /// Collects the fitnesses from each antibody into an array.
        /// </summary>
        /// <returns>The array of fitnesses.</returns>
        private double[] GetFitnesses()
        {
            var fitnesses = new double[Elements.Count];
            for (int i = 0; i < Elements.Count; i++)
            {
                fitnesses[i] = ((BaseElement)Elements[i]).Fitness;
            }
            return fitnesses;
        }

        /// <summary>
        /// Finds the element that is in the best position.
        /// </summary>
        /// <returns>The best element.</returns>
        public BaseElement FindBestElement()
        {
            var BestIndex = 0;
            for (int i = 1; i < Elements.Count; i++)
                if (((BaseElement)Elements[i]).Fitness < ((BaseElement)Elements[BestIndex]).Fitness)
                    BestIndex = i;
            return (BaseElement)Elements[BestIndex];
        }

        /// <summary>
		/// Finds the element that is in the worst position.
		/// </summary>
		/// <returns>The best element.</returns>
		public BaseElement FindWorstElement()
        {
            var WorstIndex = 0;
            for (int i = 1; i < Elements.Count; i++)
                if (((BaseElement)Elements[i]).Fitness > ((BaseElement)Elements[WorstIndex]).Fitness)
                    WorstIndex = i;
            return (BaseElement)Elements[WorstIndex];
        }

    }
}
