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
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Dao;
using Spring.Data.Common;

#endregion

namespace Spring.Data.Objects
{
    /// <summary>
    /// Tests for AdoQuery
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class AdoQueryTests : AbstractAdoQueryTests
    {

        private static string SELECT_ID_WHERE = "select id from custmr";
        private static string[] COLUMN_NAMES = new string[] {"id", "forename"};
	    private static DbType[] COLUMN_TYPES = new DbType[] {DbType.Int32, DbType.String};

        [SetUp]
        public void Setup()
        {
            SetUpMocks();

            
        }


        [Test]
        public void MappingAdoQueryWithContextWithoutParams()
        {
            IDataReader reader = (IDataReader)mocks.DynamicMock(typeof(IDataReader));
            Expect.Call(reader.Read()).Return(true);
            Expect.Call(reader.GetInt32(0)).Return(1);
            Expect.Call(reader.Read()).Return(false);

            Expect.Call(command.ExecuteReader()).Return(reader);

            mocks.ReplayAll();
            IntMappingQueryWithContext queryWithNoContext = new IntMappingQueryWithContext(provider);
            queryWithNoContext.Compile();
            IList list = queryWithNoContext.QueryByNamedParam(null, null);
            Assert.IsTrue(list.Count != 0);
            foreach (int count in list)
            {
                Assert.AreEqual(1, count);
            }

            mocks.VerifyAll();
        }
 
        [Test]
        [ExpectedException(typeof(InvalidDataAccessApiUsageException))]
        public void QueryWithoutEnoughParams()
        {
            SqlParameter sqlParameter1 = new SqlParameter();         
            Expect.Call(command.CreateParameter()).Return(sqlParameter1);
            Expect.Call(provider.CreateParameterNameForCollection(COLUMN_NAMES[0])).Return("@" + COLUMN_NAMES[0]);

            SqlParameter sqlParameter2 = new SqlParameter();
            Expect.Call(command.CreateParameter()).Return(sqlParameter2);
            Expect.Call(provider.CreateParameterNameForCollection(COLUMN_NAMES[1])).Return("@" + COLUMN_NAMES[1]);
            
            mocks.ReplayAll();

            IntMappingAdoQuery query = new IntMappingAdoQuery();
            query.DbProvider = provider;
            query.Sql = SELECT_ID_WHERE;
            query.DeclaredParameters.Add(COLUMN_NAMES[0], COLUMN_TYPES[0]);
            query.DeclaredParameters.Add(COLUMN_NAMES[1], COLUMN_TYPES[1]);
            query.Compile();
            query.Query();

            mocks.VerifyAll();
        }


        public class IntMappingQueryWithContext : MappingAdoQueryWithContext
        {
            private static string sql = "select id from custmr";

            public IntMappingQueryWithContext(IDbProvider dbProvider)
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

        public class IntMappingAdoQuery : MappingAdoQuery
        {
            protected override object MapRow(IDataReader reader, int rowNum)
            {
                return reader.GetInt32(0);
            }
        }
        
    }
}