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

#region Imports

using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Connection;
using NHibernate.Driver;
using NHibernate.Engine;

using NUnit.Framework;

using Rhino.Mocks;

using Spring.Data.Common;

#endregion

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// This class contains tests for SessionFactoryUtilsTest
    /// </summary>
    /// <author>Marko Lahma</author>
    [TestFixture]
    public class SessionFactoryUtilsTests
    {
#if !NET_1_1
        [Test]
        public void SessionFactoryUtilsWithGetDbProvider()
        {
            MockRepository mockery = new MockRepository();
            ISessionFactoryImplementor sessionFactory = (ISessionFactoryImplementor) mockery.CreateMultiMock(typeof(ISessionFactory), typeof(ISessionFactoryImplementor));

            IDriver driver = (IDriver)mockery.DynamicMock(typeof(IDriver));
            Expect.Call(driver.CreateCommand()).Repeat.AtLeastOnce().Return(new SqlCommand());

            IConnectionProvider cp = (IConnectionProvider) mockery.DynamicMock(typeof (IConnectionProvider));
            Expect.Call(cp.Driver).Repeat.AtLeastOnce().Return(driver);

            Expect.Call(sessionFactory.ConnectionProvider).Repeat.AtLeastOnce().Return(cp);

            mockery.ReplayAll();
            IDbProvider provider = SessionFactoryUtils.GetDbProvider(sessionFactory);

            Assert.AreEqual(typeof(SqlCommand), provider.DbMetadata.CommandType);
            
            mockery.VerifyAll();
        }
#endif
    }

   
}
