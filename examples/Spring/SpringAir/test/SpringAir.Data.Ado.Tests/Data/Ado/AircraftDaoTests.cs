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
using NUnit.Framework;
using Spring.Data.Common;
using SpringAir.Domain;

#endregion

namespace SpringAir.Data.Ado
{
    /// <summary>
    /// Integration tests for the ADO.NET-backed AircraftDao class.
    /// </summary>
    /// <author>Rick Evans (.NET)</author>
    /// <version>$Id: AircraftDaoTests.cs,v 1.3 2006/11/28 06:42:27 markpollack Exp $</version>
    [TestFixture]
    public sealed class AircraftDaoTests
    {
        private AircraftDao dao;

        [SetUp]
        public void SetUp()
        {
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("SqlServer-1.1");
            dbProvider.ConnectionString =
                "Server=(local);Integrated Security=no;User ID=springqa;PWD=springqa;initial catalog=SpringAir;";
            dao = new AircraftDao();
            dao.DbProvider = dbProvider;
            
            /*
			ReflectiveDbConnectionFactory connectionfactory = new ReflectiveDbConnectionFactory();
			connectionfactory.ConnectionType = typeof (SqlConnection);
			connectionfactory.ConnectionString =
				"Server=(local)\\Spring;Integrated Security=no;User ID=sa;PWD=spring;initial catalog=SpringAir;";

            dao = new AircraftDao();
			dao.ConnectionFactory = connectionfactory;
			dao.CommandType = typeof (SqlCommand);
            dao.AircraftMapper = new AircraftMapper();
             */
        }

        [Test]
        public void GetAllAircraftSunnyDay()
        {
            IList aircraft = dao.GetAllAircraft();
            Assert.IsNotNull(aircraft, "Must never return a null aircraft list.");
            Assert.AreEqual(5, aircraft.Count);
            foreach (Aircraft plane in aircraft)
            {
                Console.Out.WriteLine(plane);
            }
        }

        [Test]
        public void GetAircraftSunnyDay()
        {
            Aircraft plane = dao.GetAircraft(0);
            Assert.IsNotNull(plane, "Failed to retrive an Aircraft that is definitely in the database.");
            Console.Out.WriteLine(plane);
        }
    }
}
