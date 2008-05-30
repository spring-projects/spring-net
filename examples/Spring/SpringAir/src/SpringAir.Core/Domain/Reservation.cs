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
    /// A flight reservation for a single <see cref="SpringAir.Domain.Passenger"/>.
    /// </summary>
    /// <author>Keith Donald</author>
    /// <author>Rick Evans (.NET)</author>
    /// <version>$Id: Reservation.cs,v 1.5 2005/10/09 06:18:45 aseovic Exp $</version>
    [Serializable]
    public class Reservation : Entity
    {
        /// <summary>
        /// Holds information about a flight reservation, such as the legs and the cost.
        /// </summary>
        private Itinerary itinerary;

        /// <summary>
        /// The <see cref="SpringAir.Domain.Passenger"/> for whom this reservation
        /// is made.
        /// </summary>
        private Passenger passenger;

        /// <summary>
        /// The mapping between the <see cref="SpringAir.Domain.Flight"/> of a journey
        /// to the seat (number, a <see cref="System.String"/>) that is reserved for
        /// that particular <see cref="SpringAir.Domain.Flight"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// More clearly, which seat a passenger is going to plonk their bum into for
        /// each leg of a journey.
        /// </p>
        /// </remarks>
        //private IDictionary flightSeats;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="SpringAir.Domain.Reservation"/> class.
        /// </summary>
        public Reservation()
        {
        }

        public Reservation(Passenger passenger, Itinerary itinerary)//, IDictionary flightSeats)
        {
            this.passenger = passenger;
            this.itinerary = itinerary;
            //this.flightSeats = flightSeats;
        }

        public Itinerary Itinerary
        {
            get { return this.itinerary; }
            set { this.itinerary = value; }
        }

//        public IDictionary FlightSeats
//        {
//            get { return this.flightSeats; }
//            set { this.flightSeats = value; }
//        }

        public Passenger Passenger
        {
            get { return passenger; }
            set { passenger = value; }
        }

        public override bool Equals(object obj)
        {
            Reservation rhs = obj as Reservation;
            return rhs != null
                   && this.passenger.Id.Equals(rhs.passenger.Id)
                   && this.itinerary == rhs.itinerary;
        }

        public override int GetHashCode()
        {
            return this.passenger.Id.GetHashCode() + this.itinerary.GetHashCode();
        }
    }
}