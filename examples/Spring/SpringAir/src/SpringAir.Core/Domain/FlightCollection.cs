#region Licence

/*
 * Copyright © 2002-2005 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region Imports

using System;
using System.Collections;

#endregion

namespace SpringAir.Domain
{
	/// <summary>
	/// Provides a strongly typed collection for
	/// <see cref="SpringAir.Domain.Flight"/> objects.
	/// </summary>
	/// <author>Bruno Baia</author>
	/// <version>$Id: FlightCollection.cs,v 1.3 2006/01/08 00:49:09 bbaia Exp $</version>
	[Serializable]
	public class FlightCollection : CollectionBase
	{
		/// <summary>
		/// Adds an <see cref="SpringAir.Domain.Flight"/> to the end of this collection.
		/// </summary>
		/// <param name="value">
		/// The <see cref="SpringAir.Domain.Flight"/> to be added to the end of the collection.
		/// </param>
		/// <returns>
		/// The index at which the <see cref="SpringAir.Domain.Flight"/> has been added.
		/// </returns>
		public int Add(Flight value)
		{
			return base.List.Add(value);
		}

		/// <summary>
		/// Gets or sets the <see cref="SpringAir.Domain.Flight"/> at the specified index.
		/// </summary>
		/// <param name="index">
		/// The zero-based index of the <see cref="SpringAir.Domain.Flight"/> to get or set.
		/// </param>
		public Flight this[int index]
		{
			get { return (Flight) base.List[index]; }
			set { base.List[index] = value; }
		}
	}
}