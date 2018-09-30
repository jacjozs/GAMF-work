using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    /// <summary>
    /// Feladata az Optimization namespace alatt található algoritmusokból választés egy fitnes függvény alapján
    /// </summary>
    public class AlgoSelector
    {
        /// <summary>
        /// A vizsgál fitnesz funkció
        /// </summary>
        public FitnessFunctionDelegate FitnessFunction;
    }
}
