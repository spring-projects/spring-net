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
	/// <see cref="SpringAir.Domain.Airport"/> objects.
	/// </summary>
	/// <author>Bruno Baia</author>
	/// <version>$Id: AirportCollection.cs,v 1.2 2005/12/21 14:43:47 springboy Exp $</version>
	[Serializable]
	public class AirportCollection : CollectionBase
	{
		/// <summary>
		/// Adds an <see cref="SpringAir.Domain.Airport"/> to the end of this collection.
		/// </summary>
		/// <param name="value">
		/// The <see cref="SpringAir.Domain.Airport"/> to be added to the end of the collection.
		/// </param>
		/// <returns>
		/// The index at which the <see cref="SpringAir.Domain.Airport"/> has been added.
		/// </returns>
		public int Add(Airport value)
		{
			return base.List.Add(value);
		}

		/// <summary>
		/// Gets or sets the <see cref="SpringAir.Domain.Airport"/> at the specified index.
		/// </summary>
		/// <param name="index">
		/// The zero-based index of the <see cref="SpringAir.Domain.Airport"/> to get or set.
		/// </param>
		public Airport this[int index]
		{
			get { return (Airport) base.List[index]; }
			set { base.List[index] = value; }
		}
	}
}