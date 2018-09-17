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

#region Imports

using System.Data;

using NUnit.Framework;

#endregion

namespace Spring.Data.Common
{
    /// <summary>
    /// This class contains tests for DbParameters
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class DbParametersTests
    {
        [SetUp]
        public void Setup()
        {
        }

        // TODO separate providers for OracleClient
#if !NETCOREAPP
        [Test]
        public void OracleClient()
        {
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("System.Data.OracleClient");
            DbParameters dbParameters = new DbParameters(dbProvider);
            dbParameters.Add("p1", DbType.String);
            IDataParameter parameter = dbParameters[0];
            Assert.AreEqual("p1", parameter.ParameterName);
            dbParameters.SetValue("p1", "foo");
            object springParameter = dbParameters.GetValue("p1");
            Assert.IsNotNull(springParameter);
            Assert.AreEqual("foo", springParameter);
        }
#endif

        [Test]
        public void SqlClient()
        {
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            DbParameters dbParameters = new DbParameters(dbProvider);
            dbParameters.Add("p1", DbType.String);
            IDataParameter parameter = dbParameters[0];
            Assert.AreEqual("@p1", parameter.ParameterName);
            dbParameters.SetValue("p1", "foo");
            object springParameter = dbParameters.GetValue("p1");
            Assert.IsNotNull(springParameter);
            Assert.AreEqual("foo", springParameter);
        }
    }
}