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

namespace SpringAir.Service
{
	/// <summary>
	/// Central service interface for the trip booking use cases of the
	/// SpringAir application.
	/// </summary>
	/// <author>Keith Donald</author>
	/// <author>Rick Evans (.NET)</author>
	/// <version>$Id: IBookingAgent.cs,v 1.7 2005/12/21 14:38:19 springboy Exp $</version>
	public interface IBookingAgent
	{
		/// <summary>
		/// Gets those <see cref="SpringAir.Domain.FlightSuggestions"/>
		/// that are applicable for the supplied <see cref="SpringAir.Domain.Trip"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Guaranteed to not be <see lang="null"/>, although there may not be any
		/// actual suggested <see cref="SpringAir.Domain.Flight"/>s for either the
		/// outbound (or return) legs.
		/// </p>
		/// </remarks>
		/// <param name="trip">
		/// The <see cref="SpringAir.Domain.Trip"/> that contains the criteria
		/// that are to be used to select the flight suggestions.
		/// </param>
		/// <returns>
		/// A <see cref="SpringAir.Domain.FlightSuggestions"/> comprised of all
		/// those <see cref="SpringAir.Domain.Flight"/> instances that
		/// are applicable for the supplied <see cref="SpringAir.Domain.Trip"/>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="trip"/> is <cref lang="null"/>.
		/// </exception>
		FlightSuggestions SuggestFlights(Trip trip);

		/// <summary>
		/// Goes ahead and actually books what up until this point has been a
		/// transient <paramref name="reservation"/>.
		/// </summary>
		/// <param name="reservation">
		/// The <see cref="SpringAir.Domain.Reservation"/> that is to be confirmed.
		/// </param>
		/// <returns>
		/// A confirmation that the supplied <paramref name="reservation"/>
		/// has been accepted by the system.
		/// </returns>
		/// <exception cref="SpringAir.Service.CannotConfirmReservationException">
		/// If the supplied <paramref name="reservation"/> cannot be confirmed
		/// (for whatever reason).
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="reservation"/> is <cref lang="null"/>.
		/// </exception>
		ReservationConfirmation Book(Reservation reservation);

		/// <summary>
		/// Returns a collection of all of those
		/// <see cref="SpringAir.Domain.Airport"/> instances that can be used for
		/// the purposes of booking.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Guaranteed to not be <see lang="null"/>, although the collection
		/// may well be empty (in the unlikely event that every
		/// <see cref="SpringAir.Domain.Airport"/> has been purged from the system).
		/// </p>
		/// </remarks>
		/// <returns>
		/// A collection of all of those <see cref="SpringAir.Domain.Airport"/>
		/// instances that can be used for the purposes of booking.
		/// </returns>
		AirportCollection GetAirportList();
	}
}
