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
    /// Holds information about a flight reservation, such as the flights and the cost.
    /// </summary>
    /// <remarks>
    /// <p>
    /// In this cut of the application, we do not persist these objects to
    /// persistant storage.
    /// </p>
    /// </remarks>
    /// <author>Keith Donald</author>
    /// <author>Rick Evans (.NET)</author>
    /// <version>$Id: Itinerary.cs,v 1.8 2005/12/13 00:42:50 bbaia Exp $</version>
    [Serializable]
    public class Itinerary : Entity
    {
        private decimal price;
        private FlightCollection flights;

        #region Constructors

		/// <summary>
		/// Creates a new instance of the 
		/// <see cref="SpringAir.Domain.Itinerary"/> class.
		/// </summary>
        public Itinerary()
        {}

        /// <summary>
        /// Creates a new instance of the <see cref="SpringAir.Domain.Itinerary"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Use this constructor for a one way journey.
        /// </p>
        /// </remarks>
        /// <param name="flights">
        /// Flights for this itinerary.
        /// </param>
        public Itinerary(FlightCollection flights)
            : this(Transient, 0.0M, flights)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SpringAir.Domain.Itinerary"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Use this constructor for a one way journey.
        /// </p>
        /// </remarks>
        /// <param name="id">
        /// The number that uniquely identifies this instance.
        /// </param>
        /// <param name="price">Price for this itinerary.</param>
        /// <param name="flights">
        /// Flights for this itinerary.
        /// </param>
        public Itinerary(long id, decimal price, FlightCollection flights) : base(id)
        {
            this.price = price;
            this.flights = flights;
        }
        
        #endregion

        #region Properties
        
        public decimal Price
        {
            get { return price; }
            set { price = value; }
        }

        public FlightCollection Flights
        {
            get { return flights; }
			set { flights = value; }
        }

        #endregion
    }
}