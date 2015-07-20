#region Licence

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

#region

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;
using Common.Logging;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Objects;

#endregion

namespace Spring.Data
{
    /// <summary>
    ///
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    [TestFixture]
    [Ignore("ORACLE-dependent tests disabled for integration runs")]
    public class OracleAdoTemplateTests
    {
        #region Setup/Teardown

        [SetUp]
        public void CreateAdoTemplate()
        {
            IApplicationContext ctx =
                new XmlApplicationContext(
                    "assembly://Spring.Data.Integration.Tests/Spring.Data/oracleAdoTemplateTests.xml");
            Assert.IsNotNull(ctx);
            dbProvider = ctx["DbProvider"] as IDbProvider;
            Assert.IsNotNull(dbProvider);
            adoOperations = new AdoTemplate(dbProvider);
        }

        #endregion

        private IAdoOperations adoOperations;
        private IDbProvider dbProvider;

        /// <summary>
        /// The shared ILog instance for this class (and derived classes).
        /// </summary>
        protected static readonly ILog log =
            LogManager.GetLogger(typeof (OracleAdoTemplateTests));

        private class TestObjectExtractor : IResultSetExtractor
        {
            #region IResultSetExtractor Members

            public object ExtractData(IDataReader reader)
            {
                IList testObjects = new ArrayList();
                while (reader.Read())
                {
                    TestObject to = new TestObject();
                    //object foo = reader.GetDataTypeName(0);
                    to.ObjectNumber = (int) reader.GetInt64(0);
                    to.Name = reader.GetString(1);
                    testObjects.Add(to);
                }
                return testObjects;
            }

            #endregion
        }

        [Test]
        public void DataSetFillNoParams()
        {
            String sql = "select USER_ID, USER_NAME from USER_TABLE";
            DataSet dataSet = new DataSet();
            adoOperations.DataSetFill(dataSet, CommandType.Text, sql);
            Assert.AreEqual(1, dataSet.Tables.Count);
            Assert.AreEqual(18, dataSet.Tables["Table"].Rows.Count);

            dataSet = new DataSet();
            adoOperations.DataSetFill(dataSet, CommandType.Text, sql, new string[] {"TestObjects"});
            Assert.AreEqual(1, dataSet.Tables.Count);
            Assert.AreEqual(18, dataSet.Tables["TestObjects"].Rows.Count);

            dataSet = new DataSet();
            DataTableMappingCollection mappingCollection =
                new DataTableMappingCollection();
            DataTableMapping testObjectsMapping = mappingCollection.Add("Table", "TestObjects");
            testObjectsMapping.ColumnMappings.Add("USER_ID", "UserID");
            testObjectsMapping.ColumnMappings.Add("USER_NAME", "UserName");
            adoOperations.DataSetFill(dataSet, CommandType.Text, sql, mappingCollection);
            Assert.AreEqual(1, dataSet.Tables.Count);
            Assert.AreEqual(18, dataSet.Tables["TestObjects"].Rows.Count);
            foreach (DataRow testObjectRow in dataSet.Tables["TestObjects"].Rows)
            {
                Assert.IsNotNull(testObjectRow["UserID"]);
                Assert.IsNotNull(testObjectRow["UserName"]);
            }
        }

        [Test]
        public void DataSetFillWithParameters()
        {
            String sql = "select USER_ID, USER_NAME from USER_TABLE where USER_ID < :maxId";
            DataSet dataSet = new DataSet();
            IDbParameters parameters = adoOperations.CreateDbParameters();
            parameters.Add("maxId", OracleType.Int32).Value = 10;
            adoOperations.DataSetFillWithParameters(dataSet, CommandType.Text, sql,
                                                    parameters,
                                                    new string[] {"TestObjects"});
            Assert.AreEqual(1, dataSet.Tables.Count);
            Assert.AreEqual(6, dataSet.Tables["TestObjects"].Rows.Count);
        }

        [Test]
        public void DataSetUpdate()
        {
            //'pretend' unique key is the age...
            String sql = "select USER_ID, USER_NAME from USER_TABLE";
            DataSet dataSet = new DataSet();
            adoOperations.DataSetFill(dataSet, CommandType.Text, sql, new string[] {"TestObjects"});

            //Create and add new row.
            DataRow myDataRow = dataSet.Tables["TestObjects"].NewRow();
            myDataRow["USER_ID"] = 101;
            myDataRow["USER_NAME"] = "OldManWinter";
            dataSet.Tables["TestObjects"].Rows.Add(myDataRow);

            IDbParameters parameters = adoOperations.CreateDbParameters();
            //TODO - remembering the -1 isn't all that natural... add string name, dbtype, string sourceCol)
            //or AddSourceCol("age", SqlDbType.Int);  would copy into source col?
            parameters.Add("id", OracleType.Int32, -1, "USER_ID");
            parameters.Add("name", DbType.String, 12, "USER_NAME");


            //Extanious CommandTypes....
            adoOperations.DataSetUpdate(dataSet, "TestObjects",
                                        CommandType.Text,
                                        "insert into USER_TABLE(USER_ID, USER_NAME) values (:id,:name)", parameters,
                                        CommandType.Text, null, null,
                                        CommandType.Text, null, null);


            //TODO - think about api...
            /*
            IDbCommand insertCommand = dbProvider.CreateCommand();
            insertCommand.CommandText = "insert into USER_TABLE(USER_ID, USER_NAME) values (:id,:name)";
            parameters = adoOperations.NewDbParameters();
            //TODO - remembering the -1 isn't all that natural... add string name, dbtype, string sourceCol)
            //or AddSourceCol("age", SqlDbType.Int);  would copy into source col?
            parameters.Add("id", OracleType.Int32, -1, "USER_ID");
            parameters.Add("name", DbType.String, 12, "USER_NAME");

            //TODO - this isn't all that natural...
            ParameterUtils.CopyParameters(insertCommand, parameters);


            adoOperations.DataSetUpdate(dataSet, "TestObjects",
                insertCommand,
                null,
                null);

            */

            //TODO avoid param Utils copy by adding argument...

            //adoOperations.DataSetUpdate(dataSet, "TestObjects",
            //                            insertCommand, parameters,
            //                            null, null,
            //                            null, null);

            // or avoid all ref to command object....

            //adoOperations.DataSetUpdate(dataSet, "TestObjects",
            //                            CommandType type, string sql, parameters,
            //                            null, null,
            //                            null, null);

            //TODO how about breaking up the operations...
        }

        [Test]
        public void ExceptionTranslation()
        {
            try
            {
                adoOperations.ExecuteNonQuery(CommandType.Text, "select foo from bar");
            }
            catch (BadSqlGrammarException)
            {
                // should execute in here in sunny day scenario...
            }
            catch (Exception)
            {
                Assert.Fail("Exception translation not working.");
            }
        }


        [Test]
        public void ExecuteNonQueryText()
        {
            int user_id = 100;

            string user_name = "George0";

            String sql = String.Format("insert into USER_TABLE(USER_ID, USER_NAME) VALUES ({0}, '{1}')",
                                       user_id, user_name);
            adoOperations.ExecuteNonQuery(CommandType.Text, sql);


            sql = "insert into USER_TABLE(USER_ID, USER_NAME) values (:id,:name)";
            IDbParameters parameters = adoOperations.CreateDbParameters();

            int user_id1 = 101;
            string user_name1 = "George1";
            parameters.Add("id", OracleType.Int32).Value = user_id1;
            parameters.Add("name", DbType.String, 12).Value = user_name1;

            adoOperations.ExecuteNonQuery(CommandType.Text, sql, parameters);
        }

        [Test]
        public void ExecuteQueryWithResultSetExtractor()
        {
            IResultSetExtractor rse = new TestObjectExtractor();
            String sql = "select USER_ID, USER_NAME from USER_TABLE";
            IList testObjectList = (IList) adoOperations.QueryWithResultSetExtractor(CommandType.Text, sql, rse);
            Assert.AreEqual(18, testObjectList.Count);
        }

        [Test]
        public void SanityCheck()
        {
            adoOperations.ExecuteNonQuery(CommandType.Text, "select * from DUAL");
        }
    }
}
