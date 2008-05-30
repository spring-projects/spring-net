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
using Spring.Validation;
using SpringAir.Data;
using SpringAir.Domain;

#endregion

namespace SpringAir.Service
{
	#region Production Booking Agent implementation

	/// <summary>
	/// Default implementation of the <see cref="SpringAir.Service.IBookingAgent"/>
	/// service interface.
	/// </summary>
	/// <author>Rick Evans</author>
	/// <version>$Id: DefaultBookingAgent.cs,v 1.9 2008/04/02 23:00:00 markpollack Exp $</version>
	public class DefaultBookingAgent : IBookingAgent
	{
		private IAirportDao airportDao;
		private IFlightDao flightDao;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="SpringAir.Service.DefaultBookingAgent"/> class.
		/// </summary>
		/// <param name="airportDao">
		/// An appropriate implementation of the
		/// <see cref="SpringAir.Data.IAirportDao"/> that this DAO
		/// can use to find <see cref="SpringAir.Domain.Airport"/>
		/// instances with.
		/// </param>
		/// <param name="flightDao">
		/// An appropriate implementation of the
		/// <see cref="SpringAir.Data.IFlightDao"/> that this DAO
		/// can use to find <see cref="SpringAir.Domain.Flight"/>
		/// instances with.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If either of the supplied arguments is <cref lang="null"/>.
		/// </exception>
		public DefaultBookingAgent(IAirportDao airportDao, IFlightDao flightDao)
		{
			#region Sanity Checks

			if (airportDao == null)
			{
				throw new ArgumentNullException("airportDao", "The 'airportDao' argument is required.");
			}
			if (flightDao == null)
			{
				throw new ArgumentNullException("legDao", "The 'legDao' argument is required.");
			}

			#endregion

			this.airportDao = airportDao;
			this.flightDao = flightDao;
		}

		/// <summary>
		/// Gets those <see cref="SpringAir.Domain.FlightSuggestions"/>
		/// that are applicable for the supplied <see cref="SpringAir.Domain.Trip"/>.
		/// </summary>
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
		public FlightSuggestions SuggestFlights(Trip trip)
		{
			#region Sanity Check

			if (trip == null)
			{
				throw new ArgumentNullException("trip", "The 'trip' argument is required.");
			}

			#endregion

			Airport origin = this.airportDao.GetAirport(trip.StartingFrom.AirportCode);
			Airport destination = this.airportDao.GetAirport(trip.ReturningFrom.AirportCode);

			FlightCollection outboundLegs = GetFlights(origin, destination, trip.StartingFrom.Date);
			FlightCollection returnLegs = null;
			if (trip.Mode == TripMode.RoundTrip)
			{
				// swap 'em round, and get the legs for the way back...
				returnLegs = GetFlights(destination, origin, trip.ReturningFrom.Date);
			}
			return new FlightSuggestions(outboundLegs, returnLegs);
		}

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
		/// <seealso cref="SpringAir.Service.IBookingAgent.Book"/>
		public ReservationConfirmation Book(Reservation reservation)
		{
			if (reservation == null)
			{
				throw new ArgumentNullException("reservation", "The 'reservation' argument is required.");
			}
		    //TODO do something in the db...
            return new ReservationConfirmation(Guid.NewGuid().ToString("N").Substring(17, 5).ToUpper(), reservation);
		}

		private FlightCollection GetFlights(Airport origin, Airport destination, DateTime departureDate)
		{
			return this.flightDao.GetFlights(origin, destination, departureDate);
		}

		public AirportCollection GetAirportList()
		{
			return airportDao.GetAllAirports();
		}
	}

	#endregion

	#region Booking Agent Stub

	public class BookingAgentStub : IBookingAgent
	{
		private static readonly FlightCollection NoSuggestions = new FlightCollection();

		private IAirportDao airportDao;
		private IAircraftDao aircraftDao;

		public BookingAgentStub(IAirportDao airportDao, IAircraftDao aircraftDao)
		{
			this.airportDao = airportDao;
			this.aircraftDao = aircraftDao;
		}

		public FlightSuggestions SuggestFlights([Validated("tripValidator")] Trip trip)
		{
			#region Sanity Check

			if (trip == null)
			{
				throw new ArgumentNullException("trip", "The 'trip' argument is required.");
			}

			#endregion

			Aircraft outboundAircraft, returnAircraft;
			bool transAtlantic = trip.StartingFrom.AirportCode == "LHR" || trip.ReturningFrom.AirportCode == "LHR";
			if (transAtlantic)
			{
				outboundAircraft = aircraftDao.GetAircraft(2);
				returnAircraft = aircraftDao.GetAircraft(2);
			}
			else
			{
				outboundAircraft = aircraftDao.GetAircraft(1);
				returnAircraft = aircraftDao.GetAircraft(3);
			}
			Airport from = airportDao.GetAirport(trip.StartingFrom.AirportCode);
			Airport to = airportDao.GetAirport(trip.ReturningFrom.AirportCode);

			FlightCollection outboundFlights = new FlightCollection();
			outboundFlights.Add(new Flight("UA 0123", from, to, outboundAircraft, trip.StartingFrom.Date.AddHours(6.5)));
			outboundFlights.Add(new Flight("AA 2367", from, to, outboundAircraft, trip.StartingFrom.Date.AddHours(10)));
			outboundFlights.Add(new Flight("SW 6534", from, to, outboundAircraft, trip.StartingFrom.Date.AddHours(15.75)));
			outboundFlights.Add(new Flight("CO 0054", from, to, outboundAircraft, trip.StartingFrom.Date.AddHours(19.25)));

			FlightCollection returnFlights = new FlightCollection();
			returnFlights.Add(new Flight("CO 0112", to, from, returnAircraft, trip.ReturningFrom.Date.AddHours(9)));
			returnFlights.Add(new Flight("UA 0230", to, from, returnAircraft, trip.ReturningFrom.Date.AddHours(12.3)));
			returnFlights.Add(new Flight("AA 2234", to, from, returnAircraft, trip.ReturningFrom.Date.AddHours(21.5)));

			if (trip.Mode == TripMode.RoundTrip)
			{
				return new FlightSuggestions(outboundFlights, returnFlights);
			}
			else
			{
				return new FlightSuggestions(outboundFlights, NoSuggestions);
			}
		}

		public ReservationConfirmation Book(Reservation reservation)
		{
			if (reservation == null)
			{
				throw new ArgumentNullException("reservation", "The 'reservation' argument is required.");
			}
			return new ReservationConfirmation(Guid.NewGuid().ToString("N").Substring(17, 5).ToUpper(), reservation);
		}

		public AirportCollection GetAirportList()
		{
			return airportDao.GetAllAirports();
		}
	}

	#endregion
}