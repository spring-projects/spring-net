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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using NUnit.Framework;
using Spring.Data.Common;
using Spring.Data.Generic;
using Spring.Objects;

#endregion

namespace Spring.Data.Objects.Generic
{
    /// <summary>
    /// This calss contains tests for 
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class StoredProcedureTests
    {
        private IDbProvider _dbProvider;

        [SetUp]
        public void Setup()
        {
            _dbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            _dbProvider.ConnectionString =
                @"Data Source=SPRINGQA;Database=Spring;User ID=springqa;Password=springqa;Trusted_Connection=False";

            IDbCommand command = _dbProvider.CreateCommand();
            command.Connection = _dbProvider.CreateConnection();

            ClearTestData(command);
            CreateTestData(command);
        }

        [TearDown]
        public void TearDown()
        {
            IDbCommand command = _dbProvider.CreateCommand();
            command.Connection = _dbProvider.CreateConnection();

            ClearTestData(command);
        }

        private void CreateTestData(IDbCommand command)
        {
            command.Connection.Open();

            command.CommandText = "insert into TestObjects(Name,Age) values ('Jack', 10)";
            command.ExecuteNonQuery();

            command.CommandText = "insert into TestObjects(Name,Age) values ('Jill', 20)";
            command.ExecuteNonQuery();

            command.CommandText = "insert into Vacations(FirstName,LastName,EmployeeId,StartDate,EndDate) values ('Jack', 'Doe', 200, '1/1/2010', '1/15/2010')";
            command.ExecuteNonQuery();

            command.CommandText = "insert into Vacations(FirstName,LastName,EmployeeId,StartDate,EndDate) values ('Jack', 'Doe', 200, '2/1/2010', '2/15/2010')";
            command.ExecuteNonQuery();

            command.Connection.Close();
        }

        private void ClearTestData(IDbCommand command)
        {
            command.Connection.Open();

            command.CommandText = "truncate table TestObjects";
            command.ExecuteNonQuery();

            command.CommandText = "truncate table Vacations";
            command.ExecuteNonQuery();

            command.Connection.Close();
        }


        [Test]
        public void TestReflection()
        {
            IRowMapper<TestObject> rm = new TestObjectRowMapper();

            NamedResultSetProcessor<TestObject> rsp = new NamedResultSetProcessor<TestObject>("Test", rm);

            IDictionary dict = new Hashtable();
            dict.Add("Test", rsp);
            DoWork(dict);

        }

        private void DoWork(IDictionary dict)
        {
            BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.IgnoreCase;
            object rsp = dict["Test"];
            Assert.IsNotNull(rsp);

            Type closedType = rsp.GetType();
            foreach (Type parameter in closedType.GetGenericArguments())
            {
                Console.WriteLine("closed type param = " + parameter.ToString());
            }

            Console.WriteLine("closed type = " + closedType);

            // typeof(NamedResultSetProcessor<>);
            Type[] genericArgumentType = closedType.GetGenericArguments();
            foreach (Type type in genericArgumentType)
            {
                Console.WriteLine("arg type= " + type);
            }
            Assert.AreEqual(1, genericArgumentType.Length);
            //Type closedGenericType = openType.MakeGenericType(genericArgumentType);

            PropertyInfo propertyInfo = closedType.GetProperty("Name", BINDING_FLAGS);
            Assert.IsNotNull(propertyInfo);
            object returnValue = propertyInfo.GetValue(rsp, null);

            Assert.AreEqual("Test", returnValue.ToString());

            PropertyInfo extractorPropInfo = closedType.GetProperty("RowMapper", BINDING_FLAGS);

            Assert.IsNotNull(extractorPropInfo);

            object rowmapper = extractorPropInfo.GetValue(rsp, null);

            Type rowMapperclosedType = rowmapper.GetType();

            Console.WriteLine("rowmapper closed type= " + rowMapperclosedType);

            MethodInfo methodInfo = rowMapperclosedType.GetMethod("MapRow", BINDING_FLAGS);

            //MethodInfo genMethodInfo = methodInfo.MakeGenericMethod(genericArgumentType);
            object retVal = methodInfo.Invoke(rowmapper, new object[] { null, null });
            Console.WriteLine("return val = " + retVal);


        }

        [Test]
        public void SingleTableStoredProcedure_ReturnsResult()
        {
            TestObjectStoredProc sp = new TestObjectStoredProc(_dbProvider);
            IList<TestObject> testObjectList = sp.GetByName("Jack");

            Assert.That(testObjectList, Is.Not.Null);
            Assert.That(testObjectList, Has.Count.EqualTo(1));
        }


        [Test]
        public void MultipleTableStoredProcedure_ReturnsResult()
        {
            TestObjectandVacationStoredProc vsp = new TestObjectandVacationStoredProc(_dbProvider);

            IDictionary outParams = vsp.ExecStoreProc("Jack");

            IList<TestObject> testObjectList = testObjectList = outParams["testObjectRowMapper"] as IList<TestObject>;
            Assert.IsNotNull(testObjectList);
            Assert.AreEqual(1, testObjectList.Count);

            IList<Vacation> vacationList = outParams["vacationRowMapper"] as IList<Vacation>;
            Assert.IsNotNull(vacationList);
            Assert.AreEqual(2, vacationList.Count);

        }

    }

    public class TestObjectandVacationStoredProc : StoredProcedure
    {
        public TestObjectandVacationStoredProc(IDbProvider dbProvider)
            : base(dbProvider, "SelectTestObjectAndVacations")
        {
            DeriveParameters();
            AddRowMapper("testObjectRowMapper", new TestObjectRowMapper());
            AddRowMapper("vacationRowMapper", new VacationRowMapper<Vacation>());
            Compile();
        }

        public System.Collections.IDictionary ExecStoreProc(string name)
        {
            return Query<TestObject, Vacation>(name);
        }
    }


    public class TestObjectStoredProc : StoredProcedure
    {
        public TestObjectStoredProc(IDbProvider dbProvider)
            : base(dbProvider, "SelectByName")
        {
            DeriveParameters();
            AddRowMapper("testObjectRowMapper", new TestObjectRowMapper());
            Compile();
        }

        public IList<TestObject> GetByName(string name)
        {
            System.Collections.IDictionary outParams = Query<TestObject>(name);
            return outParams["testObjectRowMapper"] as IList<TestObject>;
        }

    }
}
