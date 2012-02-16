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


using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects;

namespace Spring.Data.Generic
{
    [TestFixture]
    public class GenericAdoTemplateTests
    {
        private AdoTemplate adoTemplate;

        [SetUp]
        public void SetUp()
        {
            IApplicationContext ctx =
                new XmlApplicationContext(
                    "assembly://Spring.Data.Integration.Tests/Spring.Data.Generic/GenericAdoTemplateTests.xml");

            adoTemplate = ctx["adoTemplate"] as AdoTemplate;
        }

        [Test]
        public void CommandDelegateUsage()
        {
            string postalCode = "1010";
            int count = adoTemplate.Execute<int>(delegate(DbCommand command)
                                                     {
                                                         command.CommandText =
                                                             "select count(*) from Customers where PostalCode = @PostalCode";

                                                         DbParameter p = command.CreateParameter();
                                                         p.ParameterName = "@PostalCode";
                                                         p.Value = postalCode;
                                                         command.Parameters.Add(p);

                                                         return (int) command.ExecuteScalar();
                                                     });
            Assert.AreEqual(3, count);
        }

        public void CommandDelegateUsageDownCast()
        {
            string postalCode = "1010";
            int count = adoTemplate.Execute<int>(delegate(DbCommand command)
                                                     {
                                                         SqlCommand sqlCommand = command as SqlCommand;
                                                         command.CommandText =
                                                             "select count(*) from Customers where PostalCode = @PostalCode";

                                                         sqlCommand.Parameters.AddWithValue("@PostalCode", postalCode);

                                                         return (int) command.ExecuteScalar();
                                                     });
            Assert.AreEqual(3, count);
        }

        [Test]
        public void TestCallbacks()
        {
            TestObjectDao testDao = new TestObjectDao();
            testDao.AdoTemplate = adoTemplate;
            IList<TestObject> testObjects = testDao.FindAll();
            Assert.IsNotNull(testObjects);
        }

        [Test]
        public void TestQueryWithCommandCreators()
        {
            IDbCommandCreatorFactory ccf = new IDbCommandCreatorFactory(adoTemplate.DbProvider, CommandType.Text, "select TestObjectNo, Age, Name from TestObjects", null);
            IDbCommandCreator cc = ccf.NewDbCommandCreator(null);
            IList<TestObject> testObjects = adoTemplate.QueryWithCommandCreator(cc,
                                                new TestObjectResultSetExtractor<List<TestObject>>());
            Assert.IsNotNull(testObjects);
            Assert.AreEqual(2, testObjects.Count);
            foreach (TestObject o in testObjects)
            {
                Console.WriteLine(o);
            }
        }
    }

    internal class TestObjectResultSetExtractor<T> : IResultSetExtractor<T> where T : IList<TestObject>, new()
    {
        public T ExtractData(IDataReader reader)
        {
            T testObjectList = new T();
            while(reader.Read())
            {
                TestObject to = new TestObject();
                to.ObjectNumber = reader.GetInt32(0);
                to.Age = reader.GetInt32(1);
                to.Name = reader.GetString(2);
                testObjectList.Add(to);
            }
            return testObjectList;
        }
    }
}
