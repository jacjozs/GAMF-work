using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class Firework : BaseOptimizationMethod
    {
        /// <summary>
        /// 
        /// </summary>
        public int m { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double a { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double b { get; set; }
        /// <summary>
        /// Maximum (worst) value of the fitness function.
        /// </summary>
        public double ymax { get; set; }
        /// <summary>
        /// Maximum explosion amplitude.
        /// </summary>
        public double Amax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int mhat { get; set; }
        
        protected override void CreateNextGeneration()
        {

            // Denominator for the spark number calculation.
            var Dens = 0.0;
            for (var j = 0; j < NumberOfElements; j++)
                Dens += ymax - ((BaseElement)Elements[j]).Fitness;
            Dens += double.Epsilon;

            // Denominator for the amplitude calculation.
            var ymin = ((BaseElement)Elements[0]).Fitness;
            for (var j = 1; j < NumberOfElements; j++)
                if (((BaseElement)Elements[j]).Fitness < ymin)
                    ymin = ((BaseElement)Elements[j]).Fitness;
            var Dena = 0.0;
            for (var j = 0; j < NumberOfElements; j++)
                Dena += ((BaseElement)Elements[j]).Fitness - ymin;
            Dena += double.Epsilon;
            var s = new int[NumberOfElements];
            var A = new double[NumberOfElements];

            // Set off n fireworks at the n locations.
            // Calculate the amplitude and sparkcount of each 
            for (var j = 0; j < NumberOfElements; j++)
            {
                // Calculate the number of sparks for the current firework.
                s[j] = (int)Math.Round((ymax - ((BaseElement)Elements[j]).Fitness + double.Epsilon) / Dens);
                if (s[j] < a * m)
                    s[j] = (int)Math.Round(a * m);
                else
                    if (s[j] > b * m)
                    s[j] = (int)Math.Round(b * m);
                // Calculate the amplitude of explosion.
                A[j] = Amax * (((BaseElement)Elements[j]).Fitness - ymin + double.Epsilon) / Dena;
            }

            // Generate the sparks.
            var SparkList = new ArrayList();
            for (int j = 0; j < NumberOfElements; j++) // for each firework
            {
                for (int k = 0; k < s[j]; k++) // Spark generation
                {
                    var dimensions = SelectRandomDimensinons();
                    var parameters = new ArrayList();
                    for (int p = 0; p < InitialParameters.Count; p++)    // places the origin firework's coordiantes into an array
                    {
                        parameters.Add(((BaseElement)Elements[j])[p]);
                        if (dimensions.Contains(p)) // the selected dimensions are modified
                        {
                            // moves the spark from the origin to somewhere within the appropriate radius.
                            parameters[parameters.Count - 1] = (double)parameters[parameters.Count - 1] + A[j] * (RNG.NextDouble() * 2 - 1);
                            // if the spark is out of bounds, move it back.
                            if ((double)parameters[parameters.Count - 1] > (double)UpperParamBounds[p])
                                parameters[parameters.Count - 1] = UpperParamBounds[p];
                            else if ((double)parameters[parameters.Count - 1] < (double)LowerParamBounds[p])
                                parameters[parameters.Count - 1] = LowerParamBounds[p];
                            if (Integer[p])
                            {
                                parameters[parameters.Count - 1] = Math.Round((double)parameters[parameters.Count - 1]);
                            }
                        }
                    }

                    SparkList.Add(GetNewElement(FitnessFunction, parameters));  //adds the modified parameters to a temporary list of sparks
                    //Evaluation++;
                }
            }
            SparkList.AddRange(Elements);
            
            // do the background calculations that let us find the most distant sparks
            var distances = new double[SparkList.Count];
            for (int j = 1; j < SparkList.Count; j++) // for each spark
            {
                for (int k = 0; k < SparkList.Count; k++) // sum the distances from each spark (incuding itself)
                {
                    distances[j] += ((BaseElement)SparkList[j]).DistanceTo(((BaseElement)SparkList[k]));
                }
            }
            var dSum = distances.Sum();
            for (int j = 0; j < SparkList.Count; j++)
            {
                distances[j] /= dSum;
            }

            // select the locations of the new fireworks
            SparkList.Sort();
            Elements[0] = ((BaseElement)SparkList[0]);
            for (int j = 1; j < NumberOfElements; j++)
            {
                Elements[j] = ((BaseElement)SparkList[RNG.Next(SparkList.Count)]);
            }
        }
        
        /// <summary>
        /// Selects a random amount of random dimensions.
        /// </summary>
        List<int> SelectRandomDimensinons()
        {
            var z = (int)Math.Round((InitialParameters.Count - 1) * RNG.NextDouble()) + 1;    //the number of selected dimensions
            var dimensions = new List<int>();
            for (var j = 0; j < InitialParameters.Count; j++)
                dimensions.Add(j);
            var selectedDimesions = new List<int>();
            if (z == InitialParameters.Count)
                return dimensions;
            for (var j = 0; j < z; j++)
            {
                var currentDimension = RNG.Next(dimensions.Count);
                selectedDimesions.Add(dimensions[currentDimension]);
                dimensions.Remove(currentDimension);
            }
            selectedDimesions.Sort();
            return selectedDimesions;
        }
    }
}
