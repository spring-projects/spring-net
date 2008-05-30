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

#region Import

using System.Collections;
using System.Data;

using Spring.Caching;
using Spring.Data;
using Spring.Data.Core;
using SpringAir.Domain;

#endregion

namespace SpringAir.Data.Ado
{
    /// <summary>
    /// AdoTemplate backed implementation of the
    /// <see cref="SpringAir.Data.IAirportDao"/> interface.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id: AirportDao.cs,v 1.13 2007/10/11 17:59:21 markpollack Exp $</version>
    public class AirportDao : AdoDaoSupport, IAirportDao
    {
        private IRowMapper airportMapper = new AirportMapper();

        /// <summary>
        /// Gets all of the <see cref="SpringAir.Domain.Airport"/> instances
        /// that exist in the system.
        /// </summary>
        /// <returns>
        /// All of the <see cref="SpringAir.Domain.Airport"/> instances
        /// that exist in the system; if no <see cref="SpringAir.Domain.Airport"/>
        /// instances can be found will return an empty
        /// <see cref="SpringAir.Domain.AirportCollection"/> (but never <cref lang="null"/>).
        /// </returns>
        [CacheResult("AspNetCache", "'Airports'", TimeToLive = "0:1:0")]
        [CacheResultItems("AspNetCache", "'Airport.Id=' + Id", TimeToLive = "0:1:0")]
        [CacheResultItems("AspNetCache", "'Airport.Code=' + Code", TimeToLive = "0:1:0")]
        public AirportCollection GetAllAirports()
        {
			AirportCollection results = new AirportCollection();
            IList list = AdoTemplate.QueryWithRowMapper(CommandType.Text,
                                                        "SELECT * FROM airport", airportMapper);
            
            //TODO - add support to queries for supplying own collection implementation?   
            // Why not using IResultSetExtrator ?
            foreach (Airport airport in list)
            {
                results.Add(airport);
            }
			return results;
        }

        /// <summary>
        /// Gets the <see cref="SpringAir.Domain.Airport"/> uniquely
        /// identified by the supplied <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        /// The id that uniquely identifies an <see cref="SpringAir.Domain.Airport"/>.
        /// </param>
        /// <returns>
        /// The <see cref="SpringAir.Domain.Airport"/> uniquely
        /// identified by the supplied <paramref name="id"/>; or <cref lang="null"/>
        /// if no such <see cref="SpringAir.Domain.Airport"/> can be found.
        /// </returns>
        [CacheResult("AspNetCache", "'Airport.Id=' + #id", TimeToLive = "0:1:0")]
        public Airport GetAirport(long id)
        {
            return (Airport) AdoTemplate.QueryForObject(CommandType.Text,
                                                        "SELECT * FROM airport WHERE id = @id",
                                                        airportMapper, "id", DbType.Int32, 0, id);
            //
            //return (Airport) QueryForEntity("SELECT * FROM airport WHERE id = @id", id, this.airportMapper);
        }

        /// <summary>
        /// Gets the <see cref="SpringAir.Domain.Airport"/> uniquely
        /// identified by the supplied <paramref name="code"/>.
        /// </summary>
        /// <param name="code">
        /// The code that uniquely identifies an <see cref="SpringAir.Domain.Airport"/>.
        /// </param>
        /// <returns>
        /// The <see cref="SpringAir.Domain.Airport"/> uniquely
        /// identified by the supplied <paramref name="code"/>; or <cref lang="null"/>
        /// if no such <see cref="SpringAir.Domain.Airport"/> can be found.
        /// </returns>
        [CacheResult("AspNetCache", "'Airport.Code=' + #code", TimeToLive = "0:1:0")]
        public Airport GetAirport(string code)
        {
            return (Airport) AdoTemplate.QueryForObject(CommandType.Text,
                                                        "SELECT * FROM airport WHERE code = @code",
                                                        airportMapper, "code", DbType.String, 0, code);
            /*
            IDbCommand command = GetCommand("SELECT * FROM airport WHERE code = @code");
            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = "@code";
            parameter.DbType = DbType.String;
            parameter.Value = code;
            command.Parameters.Add(parameter);
            return (Airport) QueryForEntity(command, this.airportMapper);
             */
        }

        public IRowMapper AirportMapper
        {
            set { airportMapper = value; }
        }
    }

    #region AirportDao Stub

    /// <summary>
    /// Stub implementation of the <see cref="SpringAir.Data.IAirportDao"/> interface.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <version>$Id: AirportDao.cs,v 1.13 2007/10/11 17:59:21 markpollack Exp $</version>
    public class AirportDaoStub : IAirportDao
    {
        private AirportCollection airports = new AirportCollection();

        public AirportDaoStub()
        {
            airports.Add(new Airport(1, "TPA", "Tampa", "Tampa International Airport"));
            airports.Add(new Airport(2, "MCO", "Orlando", "Orlando International Airport"));
            airports.Add(new Airport(3, "SFO", "San Francisco", "San Francisco International Airport"));
            airports.Add(new Airport(4, "ORD", "Chicago", "O'Hare International Airport"));
            airports.Add(new Airport(5, "LGA", "New York", "La Guardia International Airport"));
            airports.Add(new Airport(6, "LHR", "London", "London Heathrow"));
        }

        /// <summary>
        /// Gets all of the <see cref="SpringAir.Domain.Airport"/> instances
        /// that exist in the system.
        /// </summary>
        /// <returns>
        /// All of the <see cref="SpringAir.Domain.Airport"/> instances
        /// that exist in the system; if no <see cref="SpringAir.Domain.Airport"/>
        /// instances can be found will return an empty
        /// <see cref="SpringAir.Domain.AirportCollection"/> (but never <cref lang="null"/>).
        /// </returns>
        [CacheResult("AspNetCache", "'Airports'", TimeToLive = "0:1:0")]
        [CacheResultItems("AspNetCache", "'Airport.Id=' + Id", TimeToLive = "0:1:0")]
        [CacheResultItems("AspNetCache", "'Airport.Code=' + Code", TimeToLive = "0:1:0")]
        public AirportCollection GetAllAirports()
        {
			return airports;
        }

        /// <summary>
        /// Gets the <see cref="SpringAir.Domain.Airport"/> uniquely
        /// identified by the supplied <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        /// The id that uniquely identifies an <see cref="SpringAir.Domain.Airport"/>.
        /// </param>
        /// <returns>
        /// The <see cref="SpringAir.Domain.Airport"/> uniquely
        /// identified by the supplied <paramref name="id"/>; or <cref lang="null"/>
        /// if no such <see cref="SpringAir.Domain.Airport"/> can be found.
        /// </returns>
        [CacheResult("AspNetCache", "'Airport.Id=' +#id", TimeToLive = "0:1:0")]
        public Airport GetAirport(long id)
        {
            return airports[(int) id - 1];
        }

        /// <summary>
        /// Gets the <see cref="SpringAir.Domain.Airport"/> uniquely
        /// identified by the supplied <paramref name="code"/>.
        /// </summary>
        /// <param name="code">
        /// The code that uniquely identifies an <see cref="SpringAir.Domain.Airport"/>.
        /// </param>
        /// <returns>
        /// The <see cref="SpringAir.Domain.Airport"/> uniquely
        /// identified by the supplied <paramref name="code"/>; or <cref lang="null"/>
        /// if no such <see cref="SpringAir.Domain.Airport"/> can be found.
        /// </returns>
        [CacheResult("AspNetCache", "'Airport.Code=' +#code", TimeToLive = "0:1:0")]
        public Airport GetAirport(string code)
        {
			foreach(Airport airport in airports)
			{
				if (airport.Code == code)
					return airport;
			}
            return null;
        }
    }

    #endregion
}