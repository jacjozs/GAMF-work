using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
	public interface IElement
	{
		/// <summary>
		/// Position vector. A tuple of parameters. 
		/// </summary>
		ArrayList Position { get; set; }
		/// <summary>
		/// Gets or sets the value of a parameter from the position vector.
		/// </summary>
		/// <param name="index">Orinal number (index) of the vector element.</param>
		/// <returns>The actual value of the given element of the position vector.</returns>
		object this[int index] { get; set; }
		/// <summary>
		/// Recalculates the fitness of the element.
		/// </summary>
		void Update();

	}
}
