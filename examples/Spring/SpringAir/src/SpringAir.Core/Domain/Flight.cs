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
using System.Collections.Specialized;
using System.Text;

#endregion

namespace SpringAir.Domain
{
	/// <summary>
	/// A flight.
	/// </summary>
	/// <remarks>
	/// <p>
	/// An aircraft may have many flights, but a particular flight is for just one
	/// aircraft. When a flight is created, there are as many unreserved seats
	/// for that flight as the aircraft model that is servicing the flight has seats.
	/// </p>
	/// </remarks>
	/// <author>Keith Donald</author>
	/// <author>Rick Evans (.NET)</author>
	/// <version>$Id: Flight.cs,v 1.6 2006/01/08 00:48:10 bbaia Exp $</version>
	[Serializable]
	public class Flight : Entity
	{
		private string flightNumber;
		private Airport departureAirport;
		private Airport destinationAirport;
        private Aircraft aircraft;
		private IDictionary cabins = new ListDictionary();
        private DateTime departureTime;
		private string seatPlan;

        #region Constructors

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="SpringAir.Domain.Flight"/> class.
		public Flight()
		{}

		public Flight(
            string flightNumber, Airport departureAirport, Airport destinationAirport, Aircraft aircraft, DateTime departureTime)
            : this (Transient, flightNumber, departureAirport, destinationAirport, aircraft, departureTime, new Cabin[] {})
		{
            foreach (Cabin cabin in aircraft.Cabins)
            {
                this.cabins.Add(cabin.CabinClass, new Cabin(cabin));
            }
			CalculateSeatPlan();
        }

        public Flight(
            long id, string flightNumber, Airport departureAirport, Airport destinationAirport, Aircraft aircraft, DateTime departureTime, params Cabin[] cabins) : base(id)
        {
            this.flightNumber = flightNumber;
            this.departureAirport = departureAirport;
            this.destinationAirport = destinationAirport;
            this.aircraft = aircraft;
            this.departureTime = departureTime;
            foreach (Cabin cabin in cabins)
            {
                this.cabins.Add(cabin.CabinClass, cabin);
            }
            CalculateSeatPlan();
        }
        #endregion

        #region Properties

		public string FlightNumber
		{
			get { return this.flightNumber; }
			set { this.flightNumber = value; }
		}

		public Airport DepartureAirport
		{
			get { return this.departureAirport; }
			set { this.departureAirport = value; }
		}

		public Airport DestinationAirport
		{
			get { return this.destinationAirport; }
			set { this.destinationAirport = value; }
		}

		public Aircraft Aircraft
		{
			get { return this.aircraft; }
			set { this.aircraft = value; }
		}

	    public DateTime DepartureTime
	    {
	        get { return this.departureTime; }
			set { this.departureTime = value; }
	    }

        public string SeatPlan
        {
			get { return this.seatPlan; }
			set { this.seatPlan = value; }
        }

        public Cabin[] Cabins
        {
            get { return (Cabin[]) new ArrayList(cabins.Values).ToArray(typeof(Cabin)); }
        }

        #endregion

        public string ReserveSeat(CabinClass cabinClass)
        {
            if (cabins.Contains(cabinClass))
            {
                return ((Cabin) cabins[cabinClass]).ReserveSeat();
            }
			CalculateSeatPlan();
            throw new ArgumentException("Specified cabin class does not exist on this flight.", "cabinClass");
        }
    
        public string ReserveSeat(CabinClass cabinClass, string seatNumber)
        {
            if (cabins.Contains(cabinClass))
            {
                return ((Cabin) cabins[cabinClass]).ReserveSeat(seatNumber);
            }
			CalculateSeatPlan();
            throw new ArgumentException("Specified cabin class does not exist on this flight.", "cabinClass");
        }

		private void CalculateSeatPlan()
		{
			StringBuilder sb = new StringBuilder();
			string separator = "";
			foreach (Cabin cabin in this.cabins.Values)
			{
				sb.Append(separator)
					.Append(cabin.CabinClass).Append(' ')
					.Append(cabin.SeatPlan);
				separator = ", ";
			}
			this.seatPlan = sb.ToString();
		}
    }
}