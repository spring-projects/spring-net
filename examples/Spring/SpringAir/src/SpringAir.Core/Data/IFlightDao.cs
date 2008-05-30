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
using SpringAir.Domain;

#endregion

namespace SpringAir.Data
{
	/// <summary>
	/// DAO interface for the <see cref="SpringAir.Domain.Flight"/>
	/// domain class.
	/// </summary>
	/// <author>Rick Evans</author>
	/// <version>$Id: IFlightDao.cs,v 1.4 2005/12/21 14:37:02 springboy Exp $</version>
	public interface IFlightDao
	{
		/// <summary>
		/// Gets a list of all of those <see cref="SpringAir.Domain.Flight"/>s
		/// departing from the supplied <paramref name="origin"/> on the supplied
		/// <paramref name="departureDate"/> and going to the supplied
		/// <paramref name="destination"/>.
		/// </summary>
		/// <param name="origin">
		/// The departure airport.
		/// </param>
		/// <param name="destination">
		/// The destination airport.
		/// </param>
		/// <param name="departureDate">
		/// The desired departure date from the supplied <paramref name="origin"/> airport.
		/// </param>
		/// <returns>
		/// A list of all of those <see cref="SpringAir.Domain.Flight"/>s
		/// departing from the supplied <paramref name="origin"/> on the supplied
		/// <paramref name="departureDate"/> and going to the supplied
		/// <paramref name="destination"/>.
		/// </returns>
		FlightCollection GetFlights(Airport origin, Airport destination, DateTime departureDate);
	}
}