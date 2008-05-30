#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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
using System.IO;
using NHibernate;
using NHibernate.Cfg;
using NUnit.Framework;
using Spring.Data.Common;

#endregion

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// This class contains tests for LocalSessionFactoryObject
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id: LocalSessionFactoryObjectTests.cs,v 1.1 2008/01/29 20:07:18 markpollack Exp $</version>
    [TestFixture]
    public class LocalSessionFactoryObjectTests
    {

        [Test]
        public void LocalSessionFactoryObjectWithDbProviderAndProperties()
        {
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            dbProvider.ConnectionString = "badConnectionString";
            LocalSessionFactoryObject sfo = new LocalSessionFactoryObject();
            sfo.DbProvider = dbProvider;
            IDictionary properties = new Hashtable();
            properties.Add("hibernate.connection.driver_class", "NHibernate.Driver.SqlClientDriver");
            properties.Add("hibernate.connection.provider", "NHibernate.Connection.DriverConnectionProvider");
            sfo.HibernateProperties = properties;
            sfo.AfterPropertiesSet();

            Assert.IsNotNull(sfo.Configuration);
            Assert.AreEqual(sfo.Configuration.Properties[Environment.ConnectionProvider].ToString(),
                            "NHibernate.Connection.DriverConnectionProvider");
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LocalSessionFactoryObjectWithInvalidMapping()
        {
            LocalSessionFactoryObject sfo = new LocalSessionFactoryObject();
            sfo.MappingResources = new string[] { "mapping.hbm.xml"};
            sfo.AfterPropertiesSet();

        }

        [Test]
        [ExpectedException(typeof(HibernateException))]
        public void LocalSessionFactoryObjectWithInvalidProperties()
        {
            LocalSessionFactoryObject sfo = new LocalSessionFactoryObject();
            IDictionary properties = new Hashtable();
            properties.Add(Environment.ConnectionProvider, "myClass");
            sfo.HibernateProperties = properties;
            sfo.AfterPropertiesSet();
        }
    }

   
}