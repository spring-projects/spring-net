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
using System.Data;
using Spring.Data;
using Spring.Data.Core;
using Spring.Data.Common;
using Spring.Util;
using SpringAir.Domain;

#endregion

namespace SpringAir.Data.Ado
{
    /// <summary>
    /// Spring AdoTemlate backed implementation of the
    /// <see cref="SpringAir.Data.IFlightDao"/> interface.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id: FlightDao.cs,v 1.6 2007/08/03 20:28:47 markpollack Exp $</version>
    public class FlightDao : AdoDaoSupport, IFlightDao
    {
        #region Queries

        private const string FlightsQuery =
            "SELECT flight.id, flight.flight_number, flight.aircraft_id " +
            "FROM flight INNER JOIN ( " +
            "SELECT total_seats.id flight_id, (total_seats.tot_seats - reserved_seats.rsrvd_seats) free_seats " +
            "FROM " +
            "(SELECT flight.id, (row_count * seats_per_row) tot_seats " +
            "FROM aircraft INNER JOIN flight ON flight.aircraft_id = aircraft.id " +
            "GROUP BY flight.id, row_count, seats_per_row) total_seats " +
            "INNER JOIN " +
            "(SELECT flight.id, count(flight_id) rsrvd_seats " +
            "FROM flight LEFT JOIN reserved_seat ON flight.id = reserved_seat.flight_id " +
            "GROUP BY flight.id, flight_id) reserved_seats " +
            "ON total_seats.id = reserved_seats.id) available_flights " +
            "ON flight.id = available_flights.flight_id " +
            "WHERE available_flights.free_seats > 0 " +
            //"AND DATEADD(dd, DATEDIFF(dd, 0, flight.departure_date), 0) = DATEADD(dd, DATEDIFF(dd, 0, @departureDate), 0) " +
            "AND flight.departure_airport_id = @departureAirport " +
            "AND flight.destination_airport_id = @destinationAirport";

        #endregion

        private IAircraftDao aircraftDao;

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="SpringAir.Data.Ado.FlightDao"/> class.
        /// </summary>
        /// <param name="aircraftDao">
        /// The DAO used to retrieve information about the <see cref="SpringAir.Domain.Aircraft"/>
        /// associated with a <see cref="SpringAir.Domain.Flight"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="aircraftDao"/> is <see lang="null"/>.
        /// </exception>
        public FlightDao(IAircraftDao aircraftDao)
        {
			if(aircraftDao == null) 
			{
				throw new ArgumentNullException("aircraftDao");
			}
            this.aircraftDao = aircraftDao;
        }

        #endregion

        #region Methods

        public FlightCollection GetFlights(Airport origin, Airport destination, DateTime departureDate)
        {
            #region Sanity Checks
            AssertUtils.ArgumentNotNull(origin, "origin");
            AssertUtils.ArgumentNotNull(destination, "destination");
            #endregion

            FlightCollection flights = new FlightCollection();
            
            
            IDbParametersBuilder builder = new DbParametersBuilder(DbProvider);
            builder.Create().Name("departureDate").Type(DbType.Date).Value(departureDate);
            builder.Create().Name("departureAirport").Type(DbType.Int32).Value(origin.Id);
            builder.Create().Name("destinationAirport").Type(DbType.Int32).Value(destination.Id);

#if NET_2_0            
            AdoTemplate.QueryWithRowCallbackDelegate(CommandType.Text, FlightsQuery, 
                                                 delegate(IDataReader dataReader)
                                                 {
                                                     int flightId = dataReader.GetInt32(0);
                                                     string flightNumber = dataReader.GetString(1);
                                                     Aircraft aircraft = aircraftDao.GetAircraft(dataReader.GetInt32(2));

                                                     //TODO: Load cabins from the database
                                                     Cabin[] cabins = aircraft.Cabins;

                                                     Flight flight = new Flight(flightId, flightNumber, origin, destination, aircraft, departureDate, cabins);
                                                     flights.Add(flight); 
                                                 }, 
                                             
                                             
                                             builder.GetParameters());
#else
      AdoTemplate.QueryWithRowCallback(CommandType.Text, FlightsQuery,
                                        new FlightRowCallback(flights, aircraftDao, origin,destination,departureDate ),
                                        builder.GetParameters());      
#endif
            
            
            /*
            IDbCommand command = GetCommand(FlightsQuery);
            using(new ConnectionManager(command.Connection))
            {
                SetFlightQueryParameters(command, departureDate, origin.Id, destination.Id);
                using (SqlDataReader reader = (SqlDataReader) command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        int flightId = reader.GetInt32(0);
                        string flightNumber = reader.GetString(1);
                        Aircraft aircraft = aircraftDao.GetAircraft(reader.GetInt32(2));

                        //TODO: Load cabins from the database
                        Cabin[] cabins = aircraft.Cabins;

                        Flight flight = new Flight(flightId, flightNumber, origin, destination, aircraft, departureDate, cabins);
                        flights.Add(flight);
                    }
                }
            }
             */
            return flights;
        }

        private static void SetFlightQueryParameters(
            IDbCommand command, DateTime departureDate, long departureAirportsId, long destinationAirportsId)
        {
            IDbDataParameter pDepartureDate = command.CreateParameter();
            pDepartureDate.ParameterName = "@departureDate";
            pDepartureDate.DbType = DbType.Date;
            pDepartureDate.Value = departureDate;

            IDbDataParameter pDepartureAirport = command.CreateParameter();
            pDepartureAirport.ParameterName = "@departureAirport";
            pDepartureAirport.DbType = DbType.Int32;
            pDepartureAirport.Value = departureAirportsId;

            IDbDataParameter pDestinationAirport = command.CreateParameter();
            pDestinationAirport.ParameterName = "@destinationAirport";
            pDestinationAirport.DbType = DbType.Int32;
            pDestinationAirport.Value = destinationAirportsId;

            command.Parameters.Add(pDepartureDate);
            command.Parameters.Add(pDepartureAirport);
            command.Parameters.Add(pDestinationAirport);
        }

        #endregion
    }

    /// <summary>
    /// Without anonymous delegates this type of callback can be tedious if it is not reused.
    /// An alternative is to use a transaction aware IDbProvider and do you own
    /// resource management in order to avoid the back-forth-copying.
    /// </summary>
    internal class FlightRowCallback : IRowCallback
    {
        FlightCollection flights;
        IAircraftDao aircraftDao;
        Airport origin, destination;
        DateTime departureDate;
        public FlightRowCallback(FlightCollection flights, IAircraftDao aircraftDao, 
                                 Airport origin, Airport destination, DateTime departureDate)
        {
            this.flights = flights;
            this.aircraftDao = aircraftDao;
            this.origin = origin;
            this.destination = destination;
            this.departureDate = departureDate;
        }

        public void ProcessRow(IDataReader dataReader)
        {
            int flightId = dataReader.GetInt32(0);
            string flightNumber = dataReader.GetString(1);
            Aircraft aircraft = aircraftDao.GetAircraft(dataReader.GetInt32(2));

            //TODO: Load cabins from the database
            Cabin[] cabins = aircraft.Cabins;

            Flight flight = new Flight(flightId, flightNumber, origin, destination, aircraft, departureDate, cabins);
            flights.Add(flight); 
        }
        
        
    }
}