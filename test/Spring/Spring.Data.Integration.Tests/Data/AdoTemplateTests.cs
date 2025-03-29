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

using System.Collections;
using System.Data;
using System.Data.Common;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Data.Support;
using Spring.Objects;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Data;

/// <summary>
/// Simple exercising of the AdoTemplate
/// </summary>
/// <author>Mark Pollack (.NET)</author>
[TestFixture]
public class AdoTemplateTests
{
    IAdoOperations adoOperations;
    IDbProvider dbProvider;
    private IPlatformTransactionManager transactionManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdoTemplateTests"/> class.
    /// </summary>
    public AdoTemplateTests()
    {
    }

    [SetUp]
    public void CreateAdoTemplate()
    {
        IApplicationContext ctx =
            new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/adoTemplateTests.xml");
        Assert.IsNotNull(ctx);
        dbProvider = ctx["DbProvider"] as IDbProvider;
        Assert.IsNotNull(dbProvider);
        adoOperations = new AdoTemplate(dbProvider);
        transactionManager = new AdoPlatformTransactionManager(dbProvider);
    }

    [Test]
    public void FillDataSetWithTwoDataTables()
    {
        PopulateTestObjectsTable();
        PopulateCreditObjectsTable();
        string testObjectsql = ValidateTestObjects(4);
        string creditObjectsql = ValidateCreditObjects(4);

        DataSet dataSet;

        dataSet = new DataSet();
        adoOperations.DataSetFill(dataSet, CommandType.Text, testObjectsql + ";" + creditObjectsql, new string[] { "TestObjects", "CreditObjects" });
        Assert.AreEqual(2, dataSet.Tables.Count);
        Assert.AreEqual(4, dataSet.Tables["TestObjects"].Rows.Count);
        Assert.AreEqual(4, dataSet.Tables["CreditObjects"].Rows.Count);
    }

    [Test]
    public void FillDataSetNoParams()
    {
        PopulateTestObjectsTable();
        string sql = ValidateTestObjects(4);
        DataSet dataSet;

        dataSet = new DataSet();
        adoOperations.DataSetFill(dataSet, CommandType.Text, sql, new string[] { "TestObjects" });
        Assert.AreEqual(1, dataSet.Tables.Count);
        Assert.AreEqual(4, dataSet.Tables["TestObjects"].Rows.Count);

        dataSet = new DataSet();
        DataTableMappingCollection mappingCollection =
            new DataTableMappingCollection();
        DataTableMapping testObjectsMapping = mappingCollection.Add("Table", "TestObjects");
        testObjectsMapping.ColumnMappings.Add("TestObjectNo", "UserID");
        testObjectsMapping.ColumnMappings.Add("Name", "UserName");
        adoOperations.DataSetFill(dataSet, CommandType.Text, sql, mappingCollection);
        Assert.AreEqual(1, dataSet.Tables.Count);
        Assert.AreEqual(4, dataSet.Tables["TestObjects"].Rows.Count);
        foreach (DataRow testObjectRow in dataSet.Tables["TestObjects"].Rows)
        {
            Assert.IsNotNull(testObjectRow["UserID"]);
            Assert.IsNotNull(testObjectRow["Age"]);
            Assert.IsNotNull(testObjectRow["UserName"]);
        }
    }

    [Test]
    public void UpdateDataSet()
    {
        PopulateTestObjectsTable();
        ValidateTestObjects(4);
        DoUpdateDataSet();
        ValidateTestObjects(5);
    }

    [Test]
    public void UpdateDataSetTxRollback()
    {
        PopulateTestObjectsTable();
        ValidateTestObjects(4);
        TransactionTemplate tt = new TransactionTemplate(transactionManager);
        try
        {
            object result = tt.Execute(new DataSetUpdateTransactionCallback(this, true));
            Assert.Fail("Should have thrown exception to rollback transaction.");
        }
        catch (Exception)
        {
            ValidateTestObjects(4);
        }
    }

    private void PopulateTestObjectsTable()
    {
        adoOperations.ExecuteNonQuery(CommandType.Text, "truncate table TestObjects");
        int age = 18;
        int counter = 0;
        for (int i = 0; i < 4; i++)
        {
            String sql =
                String.Format("insert into TestObjects(Age, Name) VALUES ({0}, '{1}')", age++, "George" + counter++);
            adoOperations.ExecuteNonQuery(CommandType.Text, sql);
        }
    }

    private void PopulateCreditObjectsTable()
    {
        adoOperations.ExecuteNonQuery(CommandType.Text, "truncate table Credits");
        for (int i = 0; i < 4; i++)
        {
            String sql = String.Format("insert into Credits(CreditAmount) VALUES ('{0}')", i + 10);
            adoOperations.ExecuteNonQuery(CommandType.Text, sql);
        }
    }

    private string ValidateTestObjects(int count)
    {
        String sql = "select TestObjectNo, Age, Name from TestObjects";
        DataSet dataSet = new DataSet();
        adoOperations.DataSetFill(dataSet, CommandType.Text, sql);
        Assert.AreEqual(1, dataSet.Tables.Count);
        Assert.AreEqual(count, dataSet.Tables["Table"].Rows.Count);
        return sql;
    }

    private string ValidateCreditObjects(int count)
    {
        String sql = "select CreditID, CreditAmount from Credits";
        DataSet dataSet = new DataSet();
        adoOperations.DataSetFill(dataSet, CommandType.Text, sql);
        Assert.AreEqual(1, dataSet.Tables.Count);
        Assert.AreEqual(count, dataSet.Tables["Table"].Rows.Count);
        return sql;
    }

    public void DoUpdateDataSet()
    {
        String sql = "select TestObjectNo, Age, Name from TestObjects";
        DataSet dataSet = new DataSet();
        adoOperations.DataSetFill(dataSet, CommandType.Text, sql, new string[] { "TestObjects" });

        //Create and add new row.
        DataRow myDataRow = dataSet.Tables["TestObjects"].NewRow();
        myDataRow["Age"] = 101;
        myDataRow["Name"] = "OldManWinter";
        dataSet.Tables["TestObjects"].Rows.Add(myDataRow);

        IDbCommand insertCommand = dbProvider.CreateCommand();
        insertCommand.CommandText = "insert into TestObjects(Age,Name) values (@Age,@Name)";
        IDbParameters parameters = adoOperations.CreateDbParameters();
        parameters.Add("Name", DbType.String, 12, "Name");
        //TODO - remembering the -1 isn't all that natural... add string name, dbtype, string sourceCol)
        //or AddSourceCol("Age", SqlDbType.Int);  would copy into source col?
        parameters.Add("Age", SqlDbType.Int, -1, "Age");

        //TODO - this isn't all that natural...
        ParameterUtils.CopyParameters(insertCommand, parameters);

        //insertCommand.Parameters.Add()

        adoOperations.DataSetUpdate(dataSet, "TestObjects",
            insertCommand,
            null,
            null);

        //TODO avoid param Utils copy by adding argument...

        //adoOperations.DataSetUpdate(dataSet, "TestObjects",
        //                            insertCommand, parameters,
        //                            null, null,
        //                            null, null);

        //adoOperations.DataSetUpdate(dataSet, "TestObjects",
        //                            CommandType type, string sql, parameters,
        //                            null, null,
        //                            null, null);

        //TODO how about breaking up the operations...
    }

    [Test]
    public void ExecuteQueryWithResultSetExtractor()
    {
        PopulateTestObjectsTable();
        IResultSetExtractor rse = new TestObjectExtractor();
        String sql = "select TestObjectNo, Age, Name from TestObjects";
        IList testObjectList = (IList) adoOperations.QueryWithResultSetExtractor(CommandType.Text, sql, rse);
        Assert.AreEqual(4, testObjectList.Count);
    }

    [Test]
    public void ExecuteNonQueryText()
    {
        int age = 18;
        int counter = 0;
        String sql = String.Format("insert into TestObjects(Age, Name) VALUES ({0}, '{1}')",
            age++, "George" + counter++);
        adoOperations.ExecuteNonQuery(CommandType.Text, sql);

        sql = "insert into TestObjects(Age,Name) values (@Age,@Name)";

        //One liners are hard due to no standard 'fallback' to use of '?' for property
        //placeholders.  Providers that use named parameters must always use named
        //parameters in SQL string.

        //NamedParameterAdoOperations ...

        //More portable IDbDataParameterCollection implemenation.
        // -> IDbParameters
        //Functionality of .NET 2.0 DbParameterCollection + common helper methods that
        //are commonly found (still) in subclasses.

        //How to create parameter collections?
        //1. Get as much milage/portabiliyt out of basic provider interface and
        //    IDbDataParameter/IDataParameterCollection
        //    DbParameter/DbParameterCollection
        //    a. Must use only base DbType (can't cast as no shared base enumeration)
        //    b. Must use parameter prefix
        //    c. CLR null and DBNull.Value mapping.
        //    c. IsNullable is not writable in IDbDataParameter interface  (1.1 only)
        //    c2. SourceColumnNullMapping?
        //    d. No convenient Add( parameter data ) methods in
        //       base Parameter Collection classes
        //       despite prevalence in provider implementations.
        //    d1. re-use of parameters - parameters are aware if they have been added to
        //        a collection of another command object.
        //    e. verbose

        IDbParameters parametersCreated = new DbParameters(dbProvider);

        IDbDataParameter p = dbProvider.CreateParameter();
        p.ParameterName = "@Name";
        p.DbType = DbType.String;
        p.Size = 12;
        p.Value = "George" + counter++;
        parametersCreated.AddParameter(p);

        IDbDataParameter p2 = dbProvider.CreateParameter();
        p2.ParameterName = "@Age";
        p2.DbType = DbType.Int32;
        p2.Value = age++;
        parametersCreated.AddParameter(p2);

        adoOperations.ExecuteNonQuery(CommandType.Text, sql, parametersCreated);

        //2.  Use IDbParameters abstraction.
        //    e. less verbose...
        IDbParameters parameters = adoOperations.CreateDbParameters();
        parameters.Add("Name", DbType.String, 12).Value = "George" + counter++;
        parameters.Add("Age", SqlDbType.Int).Value = age++;

        //Better to use date example...people like to pick provider specific subtype..
        //parameters.AddWithValue("Age", age++);

        //parameters get 'cloned' before association with command, output values
        //are re-associated, and so the parameter collection is re-usable.
        adoOperations.ExecuteNonQuery(CommandType.Text, sql, parameters);
    }

    private class TestObjectExtractor : IResultSetExtractor
    {
        public object ExtractData(IDataReader reader)
        {
            IList testObjects = new ArrayList();
            while (reader.Read())
            {
                TestObject to = new TestObject();
                to.ObjectNumber = reader.GetInt32(0);
                to.Age = reader.GetInt32(1);
                to.Name = reader.GetString(2);
                testObjects.Add(to);
            }

            return testObjects;
        }
    }
}

internal class DataSetUpdateTransactionCallback : ITransactionCallback
{
    private bool throwException;
    private AdoTemplateTests adoTemplateTests;

    public DataSetUpdateTransactionCallback(AdoTemplateTests adoTemplateTests, bool throwException)
    {
        this.throwException = throwException;
        this.adoTemplateTests = adoTemplateTests;
    }

    /// <summary>
    /// Gets called by TransactionTemplate.Execute within a
    /// transaction context.
    /// </summary>
    /// <param name="status">The associated transaction status.</param>
    /// <returns>A result object or <c>null</c>.</returns>
    public object DoInTransaction(ITransactionStatus status)
    {
        if (throwException)
        {
            throw new ArgumentException("Explicitly thrown exception");
        }

        adoTemplateTests.DoUpdateDataSet();
        return null;
    }
}
