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
using System.Globalization;
using System.Reflection;
using System.Threading;
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
            private string providerName;

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
            //Reset it back to null so that tests for specifiying additional database providers will be able 're-initialize'
            //the internal Context of DbProviderFactory.
			//Spring.Objects.Factory.Xml.NamespaceParserRegistry.RegisterParser(typeof(Spring.Data.Config.DatabaseNamespaceParser));
            if (DbProviderFactory.ApplicationContext != null)
            {
               FieldInfo fieldInfo = typeof (DbProviderFactory).GetField("ctx", BindingFlags.NonPublic | BindingFlags.Static);
               fieldInfo.SetValue(null, null);
            }           
            ctx = new XmlApplicationContext("assembly://Spring.Data.Tests/Spring.Data.Common/DbProviderFactoryTests.xml");
        }

#if NET_2_0   
     
        [Test]
        public void ThreadSafety()
        {
            AsyncTestTask t1 = new AsyncTestDbProviderFactory(1000, "SqlServer-2.0").Start();
            AsyncTestTask t2 = new AsyncTestDbProviderFactory(1000, "SqlServer-2.0").Start();
            AsyncTestTask t3 = new AsyncTestDbProviderFactory(1000, "SqlServer-2.0").Start();
            AsyncTestTask t4 = new AsyncTestDbProviderFactory(1000, "SqlServer-2.0").Start();
            
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
            Assert.AreEqual("156",errorCode);
        }

        [Test]
        public void DefaultInstanceWithSqlServer20()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
            //IApplicationContext ctx = DbProviderFactory.ApplicationContext;            
            //Assert.IsNotNull(ctx);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR", false);
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
        public void DefaultInstanceWithMicrsoftOracleClient20()
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
#endif

#if NET_4_0
       
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
#endif

        //[Test]   
        //Comment in for specific testing with oracle as can't put oracle client in public code repository
        public void DefaultInstanceWithOracleClient20()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("OracleODP-2.0");
            Assert.AreEqual("Oracle, Oracle provider V2.102.2.20", provider.DbMetadata.ProductName);
            Assert.IsNotNull(provider.CreateCommand());
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
                "assembly://Spring.Data.Tests/Spring.Data.Common/AdditonalProviders.xml";
            IDbProvider provider = DbProviderFactory.GetDbProvider("MySqlPersonal");
            Assert.AreEqual("MySQL, MySQL provider 1.0.7.30072", provider.DbMetadata.ProductName);

        }
         
        */

        [Test]
        public void TestSqlServer20Names()
        {
            //Initialize internal application context. factory
            DbProviderFactory.GetDbProvider("SqlServer-2.0");
            IApplicationContext ctx = DbProviderFactory.ApplicationContext;
            string[] dbProviderNames = ctx.GetObjectNamesForType(typeof(IDbProvider));
            Assert.IsTrue(dbProviderNames.Length > 0);           

        }

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
