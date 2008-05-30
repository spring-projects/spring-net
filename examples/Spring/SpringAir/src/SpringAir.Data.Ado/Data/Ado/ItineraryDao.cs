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
using SpringAir.Domain;

#endregion

namespace SpringAir.Data.Ado
{
    /// <summary>
    /// ADO.NET backed implementation of the
    /// <see cref="SpringAir.Data.IItineraryDao"/> interface.
    /// </summary>
    /// <author>Rick Evans</author>
    /// <version>$Id: ItineraryDao.cs,v 1.3 2005/10/02 14:06:50 springboy Exp $</version>
    public class ItineraryDao : AbstractDao, IItineraryDao
    {
        private IAircraftDao aircraftDao;
        private IAirportDao airportDao;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="SpringAir.Data.Ado.ItineraryDao"/> class.
        /// </summary>
        /// <param name="aircraftDao">
        /// An appropriate implementation of the
        /// <see cref="SpringAir.Data.IAircraftDao"/> that this DAO
        /// can use to find <see cref="SpringAir.Domain.Aircraft"/> with.
        /// </param>
        /// <param name="airportDao">
        /// An appropriate implementation of the
        /// <see cref="SpringAir.Data.IAirportDao"/> that this DAO
        /// can use to find <see cref="SpringAir.Domain.Airport"/>
        /// instances with.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If either of the supplied arguments is <cref lang="null"/>.
        /// </exception>
        public ItineraryDao(IAircraftDao aircraftDao, IAirportDao airportDao)
        {
            #region Sanity Checks

            if (aircraftDao == null)
            {
                throw new ArgumentNullException("aircraftDao", "The 'aircraftDao' argument is required.");
            }
            if (airportDao == null)
            {
                throw new ArgumentNullException("airportDao", "The 'airportDao' argument is required.");
            }

            #endregion

            this.aircraftDao = aircraftDao;
            this.airportDao = airportDao;
        }

        /// <summary>
        /// Gets a <see cref="System.Collections.IList"/> comprised of all
        /// those <see cref="SpringAir.Domain.Itinerary"/> instances that
        /// are applicable for the supplied <see cref="SpringAir.Domain.Trip"/>.
        /// </summary>
        /// <param name="trip">
        /// The <see cref="SpringAir.Domain.Trip"/> that contains the criteria
        /// that are to be used to select a list of applicable
        /// <see cref="SpringAir.Domain.Itinerary"/> instances.
        /// </param>
        /// <returns>
        /// A <see cref="System.Collections.IList"/> comprised of all
        /// those <see cref="SpringAir.Domain.Itinerary"/> instances that
        /// are applicable for the supplied <see cref="SpringAir.Domain.Trip"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="trip"/> is <cref lang="null"/>.
        /// </exception>
        public IList GetItinerariesFor(Trip trip)
        {
            #region Sanity Check

            if (trip == null)
            {
                throw new ArgumentNullException("trip", "The 'trip' argument is required.");
            }

            #endregion

            IList itineraries = new ArrayList();

            return itineraries;
        }
    }
}