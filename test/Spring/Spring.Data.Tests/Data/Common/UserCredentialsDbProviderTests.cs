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
using Rhino.Mocks;

#endregion

namespace Spring.Data.Common
{
    /// <summary>
    /// This class contains tests for UserCredentialsDbProvider
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class UserCredentialsDbProviderTests
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [Test]
        public void StaticCredentials()
        {
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                Expect.Call(dbProvider.ConnectionString).Return(
                            @"Data Source=MARKT60\SQL2005;Database=Spring;Trusted_Connection=False");
                Expect.Call(connection.ConnectionString = @"Data Source=MARKT60\SQL2005;Database=Spring;Trusted_Connection=False;User ID=springqa;Password=springqa");
            }

            mocks.ReplayAll();

            UserCredentialsDbProvider provider = new UserCredentialsDbProvider();
            provider.TargetDbProvider = dbProvider;
            provider.Username = "User ID=springqa";
            provider.Password = "Password=springqa";
            Assert.AreEqual(connection, provider.CreateConnection());

            mocks.VerifyAll();
        }

        [Test]
        public void NoCredentials()
        {
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
            }

            mocks.ReplayAll();
            
            UserCredentialsDbProvider provider = new UserCredentialsDbProvider();
            provider.TargetDbProvider = dbProvider;
            Assert.AreEqual(connection, provider.CreateConnection());

            mocks.VerifyAll();
        }

        [Test]
        public void ThreadBoundCredentials()
        {
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection connection = mocks.StrictMock<IDbConnection>();
            using (mocks.Ordered())
            {
                Expect.Call(dbProvider.CreateConnection()).Return(connection);
                Expect.Call(dbProvider.ConnectionString).Return(
                            @"Data Source=MARKT60\SQL2005;Database=Spring;Trusted_Connection=False");
                Expect.Call(connection.ConnectionString = @"Data Source=MARKT60\SQL2005;Database=Spring;Trusted_Connection=False;User ID=springqa;Password=springqa");
            }

            mocks.ReplayAll();

            UserCredentialsDbProvider provider = new UserCredentialsDbProvider();
            provider.TargetDbProvider = dbProvider;
            provider.SetCredentialsForCurrentThread("User ID=springqa", "Password=springqa");
            Assert.AreEqual(connection, provider.CreateConnection());

            mocks.VerifyAll();

        }
    }
}