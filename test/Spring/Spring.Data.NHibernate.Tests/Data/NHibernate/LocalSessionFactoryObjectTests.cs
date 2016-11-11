#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using System.Collections.Generic;
using System.IO;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Driver;

using Spring.Context.Support;
using Spring.Data.Common;
using Spring.Data.NHibernate.Bytecode;

using NUnit.Framework;

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// This class contains tests for LocalSessionFactoryObject
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class LocalSessionFactoryObjectTests
    {

        [Test]
        public void LocalSessionFactoryObjectWithDbProviderAndProperties()
        {
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            dbProvider.ConnectionString = "Data Source=(local);Database=Spring;Trusted_Connection=false";
            LocalSessionFactoryObject sfo = new LocalSessionFactoryObject();
            sfo.DbProvider = dbProvider;
            sfo.ApplicationContext = new StaticApplicationContext();

            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Add(Environment.Dialect, typeof(MsSql2000Dialect).AssemblyQualifiedName);
            properties.Add(Environment.ConnectionDriver, typeof(SqlClientDriver).AssemblyQualifiedName);
            properties.Add(Environment.ConnectionProvider, typeof(DriverConnectionProvider).AssemblyQualifiedName);

            // since 2.1. "hbm2ddl.keywords = 'keywords'" is the default which causes the SessionFactory to connect to the db upon creation
            // we don't want this in this unit test
            properties.Add(Environment.Hbm2ddlKeyWords, "none");
            sfo.HibernateProperties = properties;
            sfo.AfterPropertiesSet();

            Assert.IsNotNull(sfo.Configuration);
            Assert.AreEqual(sfo.Configuration.Properties[Environment.ConnectionProvider], typeof(DriverConnectionProvider).AssemblyQualifiedName);
            Assert.AreEqual(sfo.Configuration.Properties[Environment.ConnectionDriver], typeof(SqlClientDriver).AssemblyQualifiedName);
            Assert.AreEqual(sfo.Configuration.Properties[Environment.Dialect], typeof(MsSql2000Dialect).AssemblyQualifiedName);

            Assert.Throws<KeyNotFoundException>(() =>
                                                    {
                                                        var x =
                                                          sfo.Configuration.Properties[Environment.ProxyFactoryFactoryClass];
                                                    }, "ProxyFactoryFactory should not be explicitly set!");
            
            Assert.AreNotEqual(typeof(BytecodeProvider), Environment.BytecodeProvider.GetType(), "default IBytecodeProvider should not be Spring's BytecodeProvider!");
        }

        [Test]
        public void LocalSessionFactoryObjectWithInvalidMapping()
        {
            LocalSessionFactoryObject sfo = new LocalSessionFactoryObject();
            sfo.MappingResources = new string[] { "mapping.hbm.xml" };
            Assert.Throws<FileNotFoundException>(() => sfo.AfterPropertiesSet());

        }

        [Test]
        public void LocalSessionFactoryObjectWithInvalidProperties()
        {
            LocalSessionFactoryObject sfo = new LocalSessionFactoryObject();
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Add(Environment.Dialect, typeof(MsSql2000Dialect).AssemblyQualifiedName);
            properties.Add(Environment.ConnectionProvider, "myClass");
            sfo.HibernateProperties = properties;
            Assert.Throws<HibernateException>(() => sfo.AfterPropertiesSet());
        }
    }
}
