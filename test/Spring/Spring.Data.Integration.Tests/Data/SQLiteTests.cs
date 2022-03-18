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

#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OracleClient;
using NUnit.Framework;
using Spring.Data.Common;
using Spring.Data.Generic;
using Spring.Data.Objects;

#endregion

namespace Spring.Data
{
    /// <summary>
    /// This class contains misc tests for specific providers
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class SQLiteTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SqlServerTest()
        {
            string errorCode = "544";
            string[] errorCodes = new string[4] { "544", "2627", "8114", "8115" };
            //Array.IndexOf()
            //Array.Sort(errorCodes);
            foreach (string code in errorCodes)
            {
                Console.WriteLine(code);
            }
            //if (Array.BinarySearch(errorCodes, errorCode) >= 0)
            if (Array.IndexOf(errorCodes, errorCode) >= 0)
            {
                Console.WriteLine("yes");
            }
            else
            {
                Assert.Fail("did not find error code");
            }
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            dbProvider.ConnectionString =
                @"Data Source=SPRINGQA;Initial Catalog=Spring;Persist Security Info=True;User ID=springqa;Password=springqa";
            AdoTemplate adoTemplate = new AdoTemplate(dbProvider);
            try
            {
                adoTemplate.ExecuteNonQuery(CommandType.Text, "insert into Vacations (FirstName,LastName) values ('Jack','Doe')");
            }
            catch (Exception e)
            {
                Console.Write(e);
                throw;
            }
        }
        [Test]
        [Ignore("ORACLE-dependent tests disabled for integration runs")]
        public void OracleTest()
        {
            //Data Source=XE;User ID=hr;Unicode=True
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("System.Data.OracleClient");
            dbProvider.ConnectionString = "Data Source=XE;User ID=hr;Password=hr;Unicode=True";
            AdoTemplate adoTemplate = new AdoTemplate(dbProvider);
            decimal count = (decimal)adoTemplate.ExecuteScalar(CommandType.Text, "select count(*) from emp");
            Assert.AreEqual(14, count);

            EmpProc empProc = new EmpProc(dbProvider);
            IDictionary dict = empProc.GetEmployees();
            foreach (DictionaryEntry entry in dict)
            {
                Console.WriteLine("Key = " + entry.Key + ", Value = " + entry.Value);
            }
            IList employeeList = dict["employees"] as IList;
            foreach (Employee employee in employeeList)
            {
                Console.WriteLine(employee);
            }

        }

        [Test]
        [Ignore("ODBC-dependent tests disabled for integration runs")]
        public void Test()
        {
            //IDbProvider dbProvider = DbProviderFactory.GetDbProvider("SybaseAse1.15");
            // dbProvider.ConnectionString = "Data Source='MARKT60';Port='5000';UID='sa';PWD='';Database='pubs2';";


            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("Odbc-2.0");
            dbProvider.ConnectionString =
                "Driver={Adaptive Server Enterprise};server=MARKT60;port=5000;Database=pubs2;uid=sa;pwd=;";
            Assert.IsNotNull(dbProvider);
            AdoTemplate adoTemplate = new AdoTemplate(dbProvider);
            IList<string> authorList =
                adoTemplate.QueryWithRowMapperDelegate<string>(CommandType.Text, "select au_lname from authors",
                                                               delegate(IDataReader dataReader, int rowNum) { return dataReader.GetString(0); });
            foreach (string s in authorList)
            {
                Console.WriteLine(s);
            }
            Assert.IsTrue(authorList.Count > 0);
        }
        /*
                [Test]
                public void StoredProc()
                {
                    //IDbProvider dbProvider = DbProviderFactory.GetDbProvider("SybaseAse1.15");
                    //dbProvider.ConnectionString = "Data Source='MARKT60';Port='5000';UID='sa';PWD='';Database='pubs2';";


                    //IDbProvider dbProvider = DbProviderFactory.GetDbProvider("Odbc-2.0");
                    //dbProvider.ConnectionString =
                    //    "Driver={Adaptive Server Enterprise};server=MARKT60;port=5000;Database=pubs2;uid=sa;pwd=;";

                    IDbProvider dbProvider = DbProviderFactory.GetDbProvider("SybaseAse-15");
                    dbProvider.ConnectionString = "Data Source='MARKT60';Port='5000';UID='sa';PWD='';Database='pubs2';";
                    HelloProc proc = new HelloProc(dbProvider);
                    IDictionary dict = proc.GetResults();

                    Assert.AreEqual("Go Sybase", dict["@inoutParam"]);
                    Assert.AreEqual("Hello mango", dict["@outParam"]);
                    Assert.AreEqual(101, (int) dict["RETURN_VALUE"]);
                    foreach (DictionaryEntry entry in dict)
                    {
                        Console.WriteLine("Key = " + entry.Key + ", Value = " + entry.Value);
                    }
                }

                [Test]
                public void StordProcAdoTemplate()
                {
                    IDbProvider dbProvider = DbProviderFactory.GetDbProvider("SybaseAse-15");
                    dbProvider.ConnectionString = "Data Source='MARKT60';Port='5000';UID='sa';PWD='';Database='pubs2';";
                    AdoTemplate adoTemplate = new AdoTemplate(dbProvider);
                    IDbParameters parameters = new DbParameters(dbProvider);
                    parameters.Add("inParam", AseDbType.VarChar, 32).Value = "mango";
                    parameters.AddInOut("inoutParam", AseDbType.VarChar, 64).Value = "Sybase";
                    parameters.AddOut("outParam", AseDbType.VarChar, 64);
                    parameters.AddReturn("retValue", AseDbType.Integer);
                    adoTemplate.ExecuteNonQuery(CommandType.StoredProcedure, "sp_hello", parameters);


                    Assert.AreEqual("Go Sybase", parameters["@inoutParam"].Value);
                    Assert.AreEqual("Hello mango", parameters[2].Value);
                    Assert.AreEqual(101, (int) parameters[3].Value);
                }
        */
        [Test]
        [Ignore("SYBASE-ASE-dependent tests disabled for integration runs")]
        public void DeriveParams()
        {
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("SybaseAse-15");
            dbProvider.ConnectionString = "Data Source='MARKT60';Port='5000';UID='sa';PWD='';Database='pubs2';";
            AdoTemplate adoTemplate = new AdoTemplate(dbProvider);
            IDataParameter[] derivedParameters = adoTemplate.DeriveParameters("@sp_hello", true);
            if (derivedParameters.Length == 0)
            {
                Console.WriteLine("Derived Parameters = 0!!!!");
            }
            for (int i = 0; i < derivedParameters.Length; i++)
            {
                Console.WriteLine("Parameter " + i + ", Name = " + derivedParameters[i].ParameterName + ", Value = " +
                                  derivedParameters[i].Value);
            }
        }

        /*
                [Test]
                public void RawDeriveParams()
                {
                    using (
                        AseConnection conn =
                            new AseConnection("Data Source='MARKT60';Port='5000';UID='sa';PWD='';Database='pubs2';"))
                    {
                        using (AseCommand cmd = new AseCommand("@sp_hello", conn))
                        {
                            conn.Open();
                            cmd.CommandType = CommandType.StoredProcedure;
                            AseCommandBuilder.DeriveParameters(cmd);
                            Console.WriteLine("Number of parameters = " + cmd.Parameters.Count);
                        }
                    }
                }
        */
        [Test]
        [Ignore("SYBASE-ASE-dependent tests disabled for integration runs")]
        public void QueryWithMapper()
        {
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("SybaseAse-15");
            dbProvider.ConnectionString = "Data Source='MARKT60';Port='5000';UID='sa';PWD='';Database='pubs2';";
            PubDao pubDao = new PubDao(dbProvider);
            IList<Sale> sales = pubDao.GetSales("5023");
            Assert.AreEqual(50, sales.Count);
        }

        [Test]
        [Ignore("ODBC-dependent tests disabled for integration runs")]
        public void QueryWithMapperODBC()
        {
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("Odbc-2.0");
            dbProvider.ConnectionString =
                "Driver={Adaptive Server Enterprise};server=MARKT60;port=5000;Database=pubs2;uid=sa;pwd=;";
            PubDao pubDao = new PubDao(dbProvider);
            IList<Sale> sales = pubDao.GetSales("5023");
            Assert.AreEqual(50, sales.Count);
        }

        [Test]
        [Ignore("SYBASE-ASE-dependent tests disabled for integration runs")]
        public void QueryRawODBC()
        {
            using (
                OdbcConnection conn =
                    new OdbcConnection(
                        "Driver={Adaptive Server Enterprise};server=MARKT60;port=5000;Database=pubs2;uid=sa;pwd=;"))
            {
                using (OdbcCommand cmd = new OdbcCommand("history_proc @stor_id", conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("stor_id", OdbcType.NVarChar).Value = "5023";
                    int i = 0;
                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            i++;
                        }
                    }
                    Assert.AreEqual(50, i);
                }
            }
        }
    }

    public class PubDao : AdoDaoSupport
    {
        public PubDao(IDbProvider provider)
        {
            DbProvider = provider;
        }

        public IList<Sale> GetSales(string storeId)
        {
            return AdoTemplate.QueryWithRowMapperDelegate<Sale>(CommandType.StoredProcedure, "history_proc",
                                                                delegate(IDataReader dataReader, int rowNum)
                                                                {
                                                                    Sale sale = new Sale();
                                                                    sale.Date = dataReader.GetDateTime(0);
                                                                    sale.OrderNumber = dataReader.GetString(1);
                                                                    sale.Quantity = dataReader.GetInt32(2);
                                                                    sale.Title = dataReader.GetString(3);
                                                                    sale.Discount = dataReader.GetFloat(4);
                                                                    sale.Price = dataReader.GetFloat(5);
                                                                    sale.Total = dataReader.GetFloat(6);
                                                                    return sale;
                                                                }, "stor_id",
                                                                DbType.String, 0, storeId);
        }
    }

    public class Sale
    {
        private DateTime date;
        private string orderNumber;
        private int quantity;
        private string title;
        private float discount;
        private float price;
        private float total;


        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

        public float Discount
        {
            get { return discount; }
            set { discount = value; }
        }

        public string OrderNumber
        {
            get { return orderNumber; }
            set { orderNumber = value; }
        }

        public int Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public float Price
        {
            get { return price; }
            set { price = value; }
        }

        public float Total
        {
            get { return total; }
            set { total = value; }
        }
    }
    /*
        public class HelloProc : StoredProcedure
        {
            public HelloProc(IDbProvider provider) : base(provider, "sp_hello")
            {
                DeclaredParameters.Add("inParam", AseDbType.VarChar, 32).Value = "mango";
                DeclaredParameters.AddInOut("inoutParam", AseDbType.VarChar, 64).Value = "Sybase";
                DeclaredParameters.AddOut("outParam", AseDbType.VarChar, 64);
                DeclaredParameters.AddReturn("retValue", AseDbType.Integer);
                Compile();
            }

            public IDictionary GetResults()
            {
                return Query("mango", "Sybase");
            }
        }
    */
    public class EmpProc : StoredProcedure
    {
        public EmpProc(IDbProvider provider)
            : base(provider, "TEST.Get1CurOut")
        {
            //DeriveParameters();
            DeclaredParameters.AddOut("P_CURSOR1", OracleType.Cursor);
            AddRowMapper("employees", new EmployeeRowMapper());
            Compile();
        }

        public IDictionary GetEmployees()
        {
            for (int i = 0; i < DeclaredParameters.Count; i++)
            {
                Console.WriteLine("decarled parameter name = " + DeclaredParameters[i].ParameterName + ", type = " + DeclaredParameters[i].DbType);
            }
            return Query();
        }
    }

    internal class EmployeeRowMapper : IRowMapper
    {
        public object MapRow(IDataReader reader, int rowNum)
        {
            Employee emp = new Employee();
            emp.Number = reader.GetDecimal(0);
            emp.Name = reader.GetString(1);
            return emp;
        }
    }

    internal class Employee
    {
        private decimal number;
        private string name;

        public decimal Number
        {
            get { return number; }
            set { number = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public override string ToString()
        {
            return "Number = " + number + ", Name = " + name;
        }
    }
}
