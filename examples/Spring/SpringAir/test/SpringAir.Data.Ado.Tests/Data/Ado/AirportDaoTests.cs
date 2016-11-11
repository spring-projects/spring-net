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
using NUnit.Framework;
using Spring.Dao;
using Spring.Data.Common;
using SpringAir.Domain;

#endregion

namespace SpringAir.Data.Ado
{
    /// <summary>
    /// Integration tests for the ADO.NET-backed AirportDao class.
    /// </summary>
    /// <author>Rick Evans (.NET)</author>
    /// <version>$Id: AirportDaoTests.cs,v 1.4 2006/11/28 06:42:27 markpollack Exp $</version>
    [TestFixture]
    public sealed class AirportDaoTests
    {
        private AirportDao dao;

        [SetUp]
        public void SetUp()
        {
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("SqlServer-1.1");
            dbProvider.ConnectionString =
                "Server=(local);Integrated Security=no;User ID=springqa;PWD=springqa;initial catalog=SpringAir;";
           
            dao = new AirportDao();
            dao.DbProvider = dbProvider;
            /*
			ReflectiveDbConnectionFactory connectionfactory = new ReflectiveDbConnectionFactory();
			connectionfactory.ConnectionType = typeof (SqlConnection);
			connectionfactory.ConnectionString =
				"Server=(local)\\Spring;Integrated Security=no;User ID=sa;PWD=spring;initial catalog=SpringAir;";

            dao = new AirportDao();
            dao.CommandType = typeof (SqlCommand);
			dao.ConnectionFactory = connectionfactory;
            dao.AirportMapper = new AirportMapper();
             */
        }

        [Test]
        public void GetAllAirports()
        {
            IList airports = dao.GetAllAirports();
            Assert.IsNotNull(airports, "Must never return a null airport list.");
            Assert.AreEqual(4, airports.Count);
        }

        [Test]
        public void GetAirportById()
        {
            Airport airport = dao.GetAirport(0);
            Assert.IsNotNull(airport, "Failed to retrive an Airport that is definitely in the database.");
        }

        [Test]
        public void GetAirportByCode()
        {
            Airport airport = dao.GetAirport("SFO");
            Assert.IsNotNull(airport, "Failed to retrive an Airport that is definitely in the database.");
        }

        [Test]
        public void GetAirportByCodeNonExistent()
        {
            Assert.Throws<EmptyResultDataAccessException>(() => dao.GetAirport("FOO"));
        }
    }
}