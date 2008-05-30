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

using System.Collections;
using System.Data;

using Spring.Data;
using Spring.Data.Core;
using Spring.Data.Support;
using Spring.Caching;
using SpringAir.Domain;

#endregion

namespace SpringAir.Data.Ado
{
    /// <summary>
    /// AdoTemplate based backed implementation of the
    /// <see cref="SpringAir.Data.IAircraftDao"/> interface.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id: AircraftDao.cs,v 1.10 2007/10/11 17:59:21 markpollack Exp $</version>
    public class AircraftDao : AdoDaoSupport, IAircraftDao
    {
        #region Queries

        private const string AllAircraft =
            "SELECT id, model, row_count, seats_per_row," +
                "(SELECT seat_count FROM aircraft_cabin_seat WHERE cabin_class_id = 0 AND aircraft_id = id) coach_seats," +
                "(SELECT seat_count FROM aircraft_cabin_seat WHERE cabin_class_id = 1 AND aircraft_id = id) business_seats," +
                "(SELECT seat_count FROM aircraft_cabin_seat WHERE cabin_class_id = 2 AND aircraft_id = id) first_class_seats " +
                "FROM aircraft";

        private const string SingleAircraftById = AllAircraft + " WHERE id = @id";

        #endregion

        private IRowMapper aircraftMapper = new AircraftMapper();

        /// <summary>
        /// Gets all of the <see cref="SpringAir.Domain.Aircraft"/> instances
        /// that exist in the system.
        /// </summary>
        /// <returns>
        /// All of the <see cref="SpringAir.Domain.Aircraft"/> instances
        /// that exist in the system; if no <see cref="SpringAir.Domain.Aircraft"/>
        /// instances can be found will return an empty
        /// <see cref="System.Collections.IList"/> (but never <cref lang="null"/>).
        /// </returns>
        [CacheResult("AspNetCache", "'Aircrafts'", TimeToLive = "0:1:0")]
        [CacheResultItems("AspNetCache", "'Aircraft.Id=' + Id", TimeToLive = "0:1:0")]
        public IList GetAllAircraft()
        {
            return AdoTemplate.QueryWithRowMapper(CommandType.Text, AllAircraft, aircraftMapper);
        }

        /// <summary>
        /// Gets the <see cref="SpringAir.Domain.Aircraft"/> uniquely
        /// identified by the supplied <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        /// The id that uniquely identifies an <see cref="SpringAir.Domain.Aircraft"/>.
        /// </param>
        /// <returns>
        /// The <see cref="SpringAir.Domain.Aircraft"/> uniquely
        /// identified by the supplied <paramref name="id"/>; or <cref lang="null"/>
        /// if no such <see cref="SpringAir.Domain.Airport"/> can be found.
        /// </returns>
        [CacheResult("AspNetCache", "'Aircraft.Id=' + #id", TimeToLive = "0:1:0")]
        public Aircraft GetAircraft(long id)
        {
            return (Aircraft) AdoTemplate.QueryForObject(CommandType.Text, SingleAircraftById, aircraftMapper,
                                                         "id", DbType.Int32, 0, id);
        }

        public IRowMapper AircraftMapper
        {
            set { aircraftMapper = value; }
        }
    }

    #region AircraftDao Stub

    public class AircraftDaoStub : IAircraftDao
    {
        private IList aircraftList = new ArrayList();

        public AircraftDaoStub()
        {
            // create stub aircraft
            aircraftList.Add(new Aircraft(1, "Boeing 737",
                                          new Cabin(CabinClass.Business, 1, 4, "ABCD"),
                                          new Cabin(CabinClass.Coach, 5, 29, "ABCDEF")
                ));
            aircraftList.Add(new Aircraft(2, "Boeing 747",
                                          new Cabin(CabinClass.First, 1, 15, "ABCD"),
                                          new Cabin(CabinClass.Business, 16, 30, "ABCDEF"),
                                          new Cabin(CabinClass.Coach, 31, 85, "ABCDEFGHI")
                ));
            aircraftList.Add(new Aircraft(3, "Airbus 320",
                                          new Cabin(CabinClass.Business, 1, 6, "ABCD"),
                                          new Cabin(CabinClass.Coach, 7, 32, "ABCDEF")
                ));
        }

        [CacheResult("AspNetCache", "'Aircrafts'", TimeToLive = "0:1:0")]
        [CacheResultItems("AspNetCache", "'Aircraft.Id=' + Id", TimeToLive = "0:1:0")]
        public IList GetAllAircraft()
        {
            return aircraftList;
        }

        [CacheResult("AspNetCache", "'Aircraft.Id=' + #id", TimeToLive = "0:1:0")]
        public Aircraft GetAircraft(long id)
        {
            return (Aircraft) aircraftList[(int) (id - 1)];
        }
    }

    #endregion
}