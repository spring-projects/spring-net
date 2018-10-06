#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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

using System.Data.SqlClient;

using FakeItEasy;

using NHibernate.Connection;
using NHibernate.Driver;
using NHibernate.Engine;

using NUnit.Framework;

using Spring.Data.Common;

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// This class contains tests for SessionFactoryUtilsTest
    /// </summary>
    /// <author>Marko Lahma</author>
    [TestFixture]
    public class SessionFactoryUtilsTests
    {
        [Test]
        public void SessionFactoryUtilsWithGetDbProvider()
        {
            ISessionFactoryImplementor sessionFactory = A.Fake<ISessionFactoryImplementor>();

            DriverBase driver = A.Fake<DriverBase>();
            A.CallTo(() => driver.CreateCommand()).Returns(new SqlCommand());

            IConnectionProvider cp = A.Fake<IConnectionProvider>();
            A.CallTo(() => cp.Driver).Returns(driver);

            A.CallTo(() => sessionFactory.ConnectionProvider).Returns(cp);

            IDbProvider provider = SessionFactoryUtils.GetDbProvider(sessionFactory);

            Assert.AreEqual(typeof(SqlCommand), provider.DbMetadata.CommandType);
        }
    }
}