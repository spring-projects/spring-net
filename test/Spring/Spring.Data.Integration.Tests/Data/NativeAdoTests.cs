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
using System.Data.SqlClient;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;

#endregion

namespace Spring.Data
{
    [TestFixture]
    public class NativeAdoTests
    {
        [Test]
        public void SimpleUsage()
        {
            IApplicationContext ctx =
                new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/nativeAdoTests.xml");
            Assert.IsNotNull(ctx);
            ITestObjectDao dao = (ITestObjectDao)ctx["testObjectDao"];
            Assert.IsNotNull(dao);
            dao.Create("John", 45);
        }

        [Test]
        public void Helloworld()
        {
            string connString =
                @"Data Source=SPRINGQA;Initial Catalog=Spring;User ID=springqa;Password=springqa;Trusted_Connection=False";

            SqlConnection conn = new SqlConnection(connString);
            conn.Open();
            //conn.BeginTransaction(IsolationLevel.Unspecified);
            SqlTransaction trans = conn.BeginTransaction();

            Assert.That(trans.IsolationLevel, Is.EqualTo(IsolationLevel.ReadCommitted));
        }
    }
}
