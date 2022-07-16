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
using System.Reflection;

using NUnit.Framework;

using Spring.Context;
using Spring.Context.Support;
using Spring.Threading;

namespace Spring.Data.Common
{
    /// <summary>
    /// Test for loading of DbProviders
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    [TestFixture]
    public class DbProviderFactoryTests
    {
        #region Helper classes for threading tests

        public class AsyncTestDbProviderFactory : AsyncTestTask
        {
            private readonly string providerName;

            public AsyncTestDbProviderFactory(int iterations, string providerName)
                : base(iterations)
            {
                this.providerName = providerName;
            }

            public override void DoExecute()
            {
                object result = DbProviderFactory.GetDbProvider(providerName);
                Assert.IsNotNull(result);
            }
        }

        #endregion

        private IApplicationContext ctx;

        [SetUp]
        public void SetUp()
        {
            //Other tests in this assembly will have already initialized the internal context that is part of DbProviderFactory
            //Reset it back to null so that tests for specifying additional database providers will be able 're-initialize'
            //the internal Context of DbProviderFactory.
            //Spring.Objects.Factory.Xml.NamespaceParserRegistry.RegisterParser(typeof(Spring.Data.Config.DatabaseNamespaceParser));
            if (DbProviderFactory.ApplicationContext != null)
            {
                FieldInfo fieldInfo = typeof (DbProviderFactory).GetField("ctx", BindingFlags.NonPublic | BindingFlags.Static);
                fieldInfo.SetValue(null, null);
            }
            ctx = new XmlApplicationContext("assembly://Spring.Data.Tests/Spring.Data.Common/DbProviderFactoryTests.xml");
        }

        [Test]
        public void ThreadSafety()
        {
#if NETCOREAPP
            const string providerName = "SqlServer";
#else
            const string providerName = "SqlServer-2.0";
#endif

            AsyncTestTask t1 = new AsyncTestDbProviderFactory(1000, providerName).Start();
            AsyncTestTask t2 = new AsyncTestDbProviderFactory(1000, providerName).Start();
            AsyncTestTask t3 = new AsyncTestDbProviderFactory(1000, providerName).Start();
            AsyncTestTask t4 = new AsyncTestDbProviderFactory(1000, providerName).Start();

            t1.AssertNoException();
            t2.AssertNoException();
            t3.AssertNoException();
            t4.AssertNoException();
        }

        [Test]
        public void AdditionalResourceName()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("Test-SqlServer-2.0");
            Assert.IsNotNull(provider);
        }

        [Test]
        public void BadErrorExpression()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("Test-SqlServer-2.0-BadErrorCodeExpression");
            Assert.IsNotNull(provider);
            string errorCode = provider.ExtractError(new Exception("foo"));
            Assert.AreEqual("156", errorCode);
        }

#if !NETCOREAPP
        [Test]
        public void DefaultInstanceWithSqlServer20()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US", false);
            //IApplicationContext ctx = DbProviderFactory.ApplicationContext;
            //Assert.IsNotNull(ctx);
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("tr-TR", false);
            IDbProvider provider = DbProviderFactory.GetDbProvider("SqlServer-2.0");
            AssertIsSqlServer2005(provider);
            provider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            AssertIsSqlServer2005(provider);
            Assert.IsNull(provider.ConnectionString);
            Assert.IsNotNull(provider.CreateCommand());
            Assert.IsNotNull(provider.CreateCommandBuilder());
            Assert.IsNotNull(provider.CreateConnection());
            Assert.IsNotNull(provider.CreateDataAdapter());
            Assert.IsNotNull(provider.CreateParameter());
            Assert.AreEqual("@Foo", provider.CreateParameterName("Foo"));
        }

        [Test]
        public void DefaultInstanceWithOleDb20()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("OleDb-2.0");
            Assert.AreEqual("OleDb, provider V2.0.0.0 in framework .NET V2", provider.DbMetadata.ProductName);
            Assert.IsNotNull(provider.CreateCommand());
            Assert.IsNotNull(provider.CreateCommandBuilder());
            Assert.IsNotNull(provider.CreateConnection());
            Assert.IsNotNull(provider.CreateDataAdapter());
            Assert.IsNotNull(provider.CreateParameter());
            Assert.AreEqual("?", provider.CreateParameterName("Foo"));
        }

        [Test]
        public void DefaultInstanceWithMicrosoftOracleClient20()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("OracleClient-2.0");
            Assert.AreEqual("Oracle, Microsoft provider V2.0.0.0", provider.DbMetadata.ProductName);
            Assert.IsNotNull(provider.CreateCommand());
            Assert.IsNotNull(provider.CreateCommandBuilder());
            Assert.IsNotNull(provider.CreateConnection());
            Assert.IsNotNull(provider.CreateDataAdapter());
            Assert.IsNotNull(provider.CreateParameter());
            Assert.AreEqual(":Foo", provider.CreateParameterName("Foo"));
        }

        [Test]
        public void DefaultInstanceWithOleDb40()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("OleDb-4.0");
            Assert.AreEqual("OleDb, provider V4.0.0.0 in framework .NET V4", provider.DbMetadata.ProductName);
            Assert.IsNotNull(provider.CreateCommand());
            Assert.IsNotNull(provider.CreateCommandBuilder());
            Assert.IsNotNull(provider.CreateConnection());
            Assert.IsNotNull(provider.CreateDataAdapter());
            Assert.IsNotNull(provider.CreateParameter());
            Assert.AreEqual("?", provider.CreateParameterName("Foo"));
        }

        [Test]
        public void DefaultInstanceWithSqlServer40()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("SqlServer-4.0");
            AssertIsSqlServer40(provider);
            Assert.IsNull(provider.ConnectionString);
            Assert.IsNotNull(provider.CreateCommand());
            Assert.IsNotNull(provider.CreateCommandBuilder());
            Assert.IsNotNull(provider.CreateConnection());
            Assert.IsNotNull(provider.CreateDataAdapter());
            Assert.IsNotNull(provider.CreateParameter());
            Assert.AreEqual("@Foo", provider.CreateParameterName("Foo"));
        }

        [Test]
        public void TestSqlServer20Names()
        {
            //Initialize internal application context. factory
            DbProviderFactory.GetDbProvider("SqlServer-2.0");
            IApplicationContext ctx = DbProviderFactory.ApplicationContext;
            var dbProviderNames = ctx.GetObjectNamesForType(typeof (IDbProvider));
            Assert.IsTrue(dbProviderNames.Count > 0);
        }
#endif

        [Test]
        public void DefaultInstanceWithOracleClient10_20()
        {
            if (Type.GetType("Oracle.DataAccess.Client.OracleConnection, Oracle.DataAccess, Version=2.102.2.20, Culture=neutral, PublicKeyToken=89b483f429c47342") == null)
            {
                Assert.Inconclusive("oracle data access libs not found, skipping test");
            }

            AssertOracleProvider("OracleODP-2.0", "Oracle, Oracle provider V2.102.2.20");
        }

        [Test]
        public void DefaultInstanceWithOracleClient11_20()
        {
            if (Type.GetType("Oracle.DataAccess.Client.OracleConnection, Oracle.DataAccess, Version=2.112.3.0, Culture=neutral, PublicKeyToken=89b483f429c47342") == null)
            {
                Assert.Inconclusive("oracle data access libs not found, skipping test");
            }

            AssertOracleProvider("OracleODP-11-2.0", "Oracle, Oracle provider V2.112.3.0");
        }

        [Test]
        public void DefaultInstanceWithOracleClient12_20()
        {
            if (Type.GetType("Oracle.DataAccess.Client.OracleConnection, Oracle.DataAccess, Version=2.121.1.0, Culture=neutral, PublicKeyToken=89b483f429c47342") == null)
            {
                Assert.Inconclusive("oracle data access libs not found, skipping test");
            }

            AssertOracleProvider("OracleODP-12-2.0", "Oracle, Oracle provider V2.121.1.0");
        }

        [Test]
        public void DefaultInstanceWithOracleClient12_40()
        {
            if (Type.GetType("Oracle.DataAccess.Client.OracleConnection, Oracle.DataAccess, Version=4.121.1.0, Culture=neutral, PublicKeyToken=89b483f429c47342") == null)
            {
                Assert.Inconclusive("oracle data access libs not found, skipping test");
            }

            AssertOracleProvider("OracleODP-12-4.0", "Oracle, Oracle provider V4.121.1.0");
        }

        [Test]
        public void DefaultInstanceWithOracleManagedClient11_40()
        {
            if (Type.GetType("Oracle.ManagedDataAccess.Client.OracleConnection, Oracle.ManagedDataAccess, Version=4.121.1.0, Culture=neutral, PublicKeyToken=89b483f429c47342") == null)
            {
                Assert.Inconclusive("oracle data access libs not found, skipping test");
            }

            AssertOracleProvider("OracleODP-Managed-12-4.0", "Oracle, Oracle Managed provider V4.121.1.0");
        }

        private static void AssertOracleProvider(string providerName, string productName)
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider(providerName);
            Assert.AreEqual(productName, provider.DbMetadata.ProductName);

            var command = provider.CreateCommand();
            Assert.IsNotNull(command);

            // check if parameter has readable BindByName property
            var property = command.GetType().GetProperty("BindByName");
            if (property != null)
            {
                var bindByNameValue = property.GetValue(command, null);
                Assert.That(bindByNameValue, Is.EqualTo(provider.DbMetadata.BindByName), "BindByName had wrong value");
            }

            Assert.IsNotNull(provider.CreateCommandBuilder());
            Assert.IsNotNull(provider.CreateConnection());
            Assert.IsNotNull(provider.CreateDataAdapter());
            Assert.IsNotNull(provider.CreateParameter());
            Assert.AreEqual(":Foo", provider.CreateParameterName("Foo"));
        }

        /*
        [Test]
        public void DefaultInstanceWithMySql()
        {
            DbProviderFactory.DBPROVIDER_ADDITIONAL_RESOURCE_NAME =
                "assembly://Spring.Data.Tests/Spring.Data.Common/AdditionalProviders.xml";
            IDbProvider provider = DbProviderFactory.GetDbProvider("MySqlPersonal");
            Assert.AreEqual("MySQL, MySQL provider 1.0.7.30072", provider.DbMetadata.ProductName);

        }

        */

        private void AssertIsSqlServer2005(IDbProvider provider)
        {
            Assert.AreEqual("Microsoft SQL Server, provider V2.0.0.0 in framework .NET V2.0",
                provider.DbMetadata.ProductName);
            AssertCommonSqlServerErrorCodes(provider);
        }

        private void AssertIsSqlServer40(IDbProvider provider)
        {
            Assert.AreEqual("Microsoft SQL Server, provider V4.0.0.0 in framework .NET V4.0",
                provider.DbMetadata.ProductName);
            AssertCommonSqlServerErrorCodes(provider);
        }

        private static void AssertCommonSqlServerErrorCodes(IDbProvider provider)
        {
            ErrorCodes codes = provider.DbMetadata.ErrorCodes;
            Assert.IsTrue(codes.BadSqlGrammarCodes.Length > 0);
            Assert.IsTrue(codes.DataIntegrityViolationCodes.Length > 0);
            // This had better be a Bad SQL Grammar code
            Assert.IsTrue(Array.IndexOf(codes.BadSqlGrammarCodes, "156") >= 0);
            // This had better NOT be
            Assert.IsFalse(Array.IndexOf(codes.BadSqlGrammarCodes, "1xx56") >= 0);
        }
    }
}
