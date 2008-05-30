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
using System.Data;
using DotNetMock.Dynamic;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Data.Common;

#endregion

namespace Spring.Data.Objects
{
    /// <summary>
    /// This class contains tests for AdoQuery subclasses
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id: AdoQueryTests.cs,v 1.3 2007/07/25 08:25:34 markpollack Exp $</version>
    [TestFixture]
    public class AdoQueryTests
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

/*
        [Test]
        public void QueryWithoutContextRhino()
        {
            IDbProvider provider = (IDbProvider) mocks.DynamicMock(typeof (IDbProvider));

            IDbConnection connection = (IDbConnection) mocks.DynamicMock(typeof (IDbConnection));

            Expect.Call(provider.CreateConnection()).Return(connection);

            IDbCommand command = (IDbCommand) mocks.DynamicMock(typeof (IDbCommand));

            IDataReader reader = (IDataReader) mocks.DynamicMock(typeof (IDataReader));
            Expect.Call(reader.Read()).Return(true);
            Expect.Call(reader.GetInt32(0)).Return(1);
            Expect.Call(reader.Read()).Return(false);

            Expect.Call(command.ExecuteReader()).Return(reader);
            Expect.Call(provider.CreateCommand()).Return(command);
            mocks.ReplayAll();

            IntMappingQueryWithNoContext queryWithNoContext = new IntMappingQueryWithNoContext(provider);
            queryWithNoContext.Compile();
            IList list = queryWithNoContext.QueryByNamedParam(null, null);
            Assert.IsTrue(list.Count != 0);
            foreach (int count in list)
            {
                Assert.AreEqual(1, count);
            }

            mocks.VerifyAll();



        }
 */
        [Test]
        public void QueryWithoutContext()
        {

            //Prepare provider
            IDynamicMock mockProvider = new DynamicMock(typeof(IDbProvider));

            //Prepare connection to return from provider
            IDynamicMock mockConnection = new DynamicMock(typeof(IDbConnection));
            IDbConnection connection = (IDbConnection) mockConnection.Object;
            mockProvider.ExpectAndReturn("CreateConnection", connection);

            //Prepare command
            IDynamicMock mockCommand = new DynamicMock(typeof(IDbCommand));

            //Prepare reader to return from command
            IDynamicMock mockReader = new DynamicMock(typeof (IDataReader));
            mockReader.ExpectAndReturn("Read", true);
            mockReader.ExpectAndReturn("GetInt32", 1, 0);
            mockReader.ExpectAndReturn("Read", false);
            IDataReader reader = (IDataReader) mockReader.Object;
            mockCommand.ExpectAndReturn("ExecuteReader", reader);

            //Preppare command to return from provider
            IDbCommand command = (IDbCommand)mockCommand.Object;
            mockProvider.ExpectAndReturn("CreateCommand", command);

            
            IDbProvider dbProvider = (IDbProvider)mockProvider.Object;


            //Test MappingAdoQueryWithContext
            IntMappingQueryWithNoContext queryWithNoContext = new IntMappingQueryWithNoContext(dbProvider);
            queryWithNoContext.Compile();
            //IList list = queryWithNoContext.QueryByNamedParam(null, null);
            IList list = queryWithNoContext.Query();
            Assert.IsTrue(list.Count != 0);
            foreach (int count in list)
            {
                Assert.AreEqual(1, count);
            }







        }

        public class IntMappingQueryWithNoContext : MappingAdoQueryWithContext
        {
            private static string sql = "select id from custmr";

            public IntMappingQueryWithNoContext(IDbProvider dbProvider)
                : base(dbProvider, sql)
            {
                CommandType = CommandType.Text;
            }


            protected override object MapRow(IDataReader reader, int rowNum, IDictionary inParams,
                                             IDictionary callingContext)
            {
                Assert.IsNull(inParams);
                Assert.IsNull(callingContext);
                return reader.GetInt32(0);
            }
        }
        
    }
}