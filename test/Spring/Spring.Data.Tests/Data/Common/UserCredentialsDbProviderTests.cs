#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Data;

using FakeItEasy;

using NUnit.Framework;

namespace Spring.Data.Common
{
    /// <summary>
    /// This class contains tests for UserCredentialsDbProvider
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class UserCredentialsDbProviderTests
    {
        [Test]
        public void StaticCredentials()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => dbProvider.ConnectionString).Returns(@"Data Source=MARKT60\SQL2005;Database=Spring;Trusted_Connection=False");

            UserCredentialsDbProvider provider = new UserCredentialsDbProvider();
            provider.TargetDbProvider = dbProvider;
            provider.Username = "User ID=springqa";
            provider.Password = "Password=springqa";
            Assert.AreEqual(connection, provider.CreateConnection());

            A.CallToSet(() => connection.ConnectionString)
                .WhenArgumentsMatch(x => (string) x[0] == "Data Source=MARKT60\\SQL2005;Database=Spring;Trusted_Connection=False;User ID=springqa;Password=springqa")
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NoCredentials()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);

            UserCredentialsDbProvider provider = new UserCredentialsDbProvider();
            provider.TargetDbProvider = dbProvider;
            Assert.AreEqual(connection, provider.CreateConnection());
        }

        [Test]
        public void ThreadBoundCredentials()
        {
            IDbProvider dbProvider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();
            A.CallTo(() => dbProvider.CreateConnection()).Returns(connection);
            A.CallTo(() => dbProvider.ConnectionString).Returns(@"Data Source=MARKT60\SQL2005;Database=Spring;Trusted_Connection=False");

            UserCredentialsDbProvider provider = new UserCredentialsDbProvider();
            provider.TargetDbProvider = dbProvider;
            provider.SetCredentialsForCurrentThread("User ID=springqa", "Password=springqa");
            Assert.AreEqual(connection, provider.CreateConnection());

            A.CallToSet(() => connection.ConnectionString)
                .WhenArgumentsMatch(x => (string) x[0] == "Data Source=MARKT60\\SQL2005;Database=Spring;Trusted_Connection=False;User ID=springqa;Password=springqa")
                .MustHaveHappenedOnceExactly();
        }
    }
}