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

using System;

namespace SpringAir.Domain
{
	/// <summary>
	/// Flight suggestions.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Maintains two lists... the first is a list of outbound flight legs, the
	/// second a list of return flight legs (only applicable on a round trip journey).
	/// </p>
	/// </remarks>
	/// <author>Rick Evans</author>
	/// <version>$Id: FlightSuggestions.cs,v 1.8 2006/03/13 11:57:44 aseovic Exp $</version>
	[Serializable]
	public class FlightSuggestions
	{
		private FlightCollection outboundFlights;
		private FlightCollection returnFlights;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="SpringAir.Domain.FlightSuggestions"/> class.
		public FlightSuggestions()
		{
			this.outboundFlights = new FlightCollection();
			this.returnFlights = new FlightCollection();
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="SpringAir.Domain.FlightSuggestions"/> class.
		/// </summary>
		/// <param name="outboundFlights">
		/// The collection of outbound <see cref="SpringAir.Domain.Flight"/> objects.
		/// </param>
		public FlightSuggestions(FlightCollection outboundFlights)
			: this(outboundFlights, new FlightCollection())
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="SpringAir.Domain.FlightSuggestions"/> class.
		/// </summary>
		/// <param name="outboundFlights">
		/// The collection of outbound <see cref="SpringAir.Domain.Flight"/> objects.
		/// </param>
		/// <param name="returnFlights">
		/// The collection of return <see cref="SpringAir.Domain.Flight"/> objects.
		/// </param>
		public FlightSuggestions(FlightCollection outboundFlights, FlightCollection returnFlights)
		{
			this.outboundFlights = FlightsNotNull(outboundFlights);
			this.returnFlights = FlightsNotNull(returnFlights);
		}

		/// <summary>
		/// The collection of outbound <see cref="SpringAir.Domain.Flight"/> objects.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Guaranteed to not be <see lang="null"/>.
		/// </p>
		/// </remarks>
		/// <value>
		/// The collection of outbound <see cref="SpringAir.Domain.Flight"/> objects.
		/// </value>
		public FlightCollection OutboundFlights
		{
			get { return outboundFlights; }
			set { outboundFlights = FlightsNotNull(value); }
		}

		/// <summary>
		/// The collection of return <see cref="SpringAir.Domain.Flight"/> objects.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Guaranteed to not be <see lang="null"/>.
		/// </p>
		/// </remarks>
		/// <value>
		/// The collection of return <see cref="SpringAir.Domain.Flight"/> objects.
		/// </value>
		public FlightCollection ReturnFlights
		{
			get { return returnFlights; }
			set { returnFlights = FlightsNotNull(value); }
		}

	    /// <summary>
	    /// Does this <see cref="SpringAir.Domain.FlightSuggestions"/> instance
	    /// contain any outbound <see cref="SpringAir.Domain.Flight"/>s?
	    /// </summary>
	    /// <returns>
	    /// <see lang="true"/> if this <see cref="SpringAir.Domain.FlightSuggestions"/>
	    /// instance contains any outbound <see cref="SpringAir.Domain.Flight"/>s.
	    /// </returns>
	    public bool HasOutboundFlights
	    {
	        get { return this.outboundFlights.Count > 0; }
	    }

	    /// <summary>
	    /// Does this <see cref="SpringAir.Domain.FlightSuggestions"/> instance
	    /// contain any return <see cref="SpringAir.Domain.Flight"/>s?
	    /// </summary>
	    /// <returns>
	    /// <see lang="true"/> if this <see cref="SpringAir.Domain.FlightSuggestions"/>
	    /// instance contains any return <see cref="SpringAir.Domain.Flight"/>s.
	    /// </returns>
	    public bool HasReturnFlights
	    {
	        get { return this.returnFlights.Count > 0; }
	    }

	    /// <summary>
		/// Gets the <see cref="SpringAir.Domain.Flight"/> specified at the
		/// supplied <paramref name="flightIndex"/> in this
		/// <see cref="SpringAir.Domain.FlightSuggestions"/> instances collection
		/// of <see cref="SpringAir.Domain.FlightSuggestions.OutboundFlights"/>.
		/// </summary>
		/// <param name="flightIndex">
		/// The index of the desired <see cref="SpringAir.Domain.Flight"/>.
		/// </param>
		/// <returns>
		/// The <see cref="SpringAir.Domain.Flight"/>.
		/// </returns>
		public Flight GetOutboundFlight(int flightIndex) 
		{
			return GetFlight(this.outboundFlights, flightIndex);
		}

		/// <summary>
		/// Gets the <see cref="SpringAir.Domain.Flight"/> specified at the
		/// supplied <paramref name="flightIndex"/> in this
		/// <see cref="SpringAir.Domain.FlightSuggestions"/> instances collection
		/// of <see cref="SpringAir.Domain.FlightSuggestions.ReturnFlights"/>.
		/// </summary>
		/// <param name="flightIndex">
		/// The index of the desired <see cref="SpringAir.Domain.Flight"/>.
		/// </param>
		/// <returns>
		/// The <see cref="SpringAir.Domain.Flight"/>.
		/// </returns>
		public Flight GetReturnFlight(int flightIndex) 
		{
			return GetFlight(this.returnFlights, flightIndex);
		}

		private Flight GetFlight(FlightCollection flights, int flightIndex) 
		{
			return flights[flightIndex];
		}

		private static FlightCollection FlightsNotNull(FlightCollection flights)
		{
			return flights == null ? new FlightCollection() : flights;
		}
	}
}