#region License

/*
 * Copyright 2002-2010 the original author or authors.
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
using System.Collections;
using System.Data;
using System.Data.Common;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Dao;
using Spring.Threading;

namespace Spring.Data.Common
{
    /// <summary>
    /// Test for MultiDelegatingDbProvider
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    [TestFixture]
    public class MultiDelegatingDbProviderTests
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [Test]
        public void CreationWhenNoRequiredPropertiesSet()
        {
            MultiDelegatingDbProvider dbProvider = new MultiDelegatingDbProvider();
            Assert.Throws<ArgumentException>(() => dbProvider.AfterPropertiesSet());
        }

        [Test]
        public void CreationWithWrongTypeDictionaryKeys()
        {
            try
            {
                MultiDelegatingDbProvider dbProvider = new MultiDelegatingDbProvider();
                IDictionary targetDbProviders = new Hashtable();
                targetDbProviders.Add(1, "bar");
                dbProvider.TargetDbProviders = targetDbProviders;
                dbProvider.AfterPropertiesSet();
                Assert.Fail("Should have thrown ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Key identifying target IDbProvider in TargetDbProviders dictionary property is required to be of type string.  Key = [1], type = [System.Int32]", ex.Message);
            }
        }

        [Test]
        public void CreationWithWrongTypeDictionaryValues()
        {
            try
            {
                IDictionary targetDbProviders = new Hashtable();
                targetDbProviders.Add("foo", 1);
                //Exercise the constructor
                MultiDelegatingDbProvider dbProvider = new MultiDelegatingDbProvider(targetDbProviders);
                dbProvider.AfterPropertiesSet();
                Assert.Fail("Should have thrown ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Value in TargetDbProviders dictionary is not of type IDbProvider.  Type = [System.Int32]", ex.Message);
            }
        }

        [Test]
        public void NoDefaultProvided()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            provider.ConnectionString = "connString1";
            MultiDelegatingDbProvider multiDbProvider = new MultiDelegatingDbProvider();
            IDictionary targetDbProviders = new Hashtable();
            targetDbProviders.Add("db1", provider);
            multiDbProvider.TargetDbProviders = targetDbProviders;
            multiDbProvider.AfterPropertiesSet();
            try
            {
                Assert.AreEqual("connString1", multiDbProvider.ConnectionString);
                Assert.Fail("InvalidDataAccessApiUsageException should have been thrown");
            }
            catch (InvalidDataAccessApiUsageException exception)
            {
                Assert.AreEqual("No provider name found in thread local storage.  Consider setting the property DefaultDbProvider to fallback to a default value.", exception.Message);
            }
            finally
            {
                LogicalThreadContext.FreeNamedDataSlot(MultiDelegatingDbProvider.CURRENT_DBPROVIDER_SLOTNAME);
            }
        }

        [Test]
        public void NoMatchingProviderDefinedInThreadLocalStorage()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            provider.ConnectionString = "connString1";
            MultiDelegatingDbProvider multiDbProvider = new MultiDelegatingDbProvider();
            IDictionary targetDbProviders = new Hashtable();
            targetDbProviders.Add("db1", provider);
            multiDbProvider.TargetDbProviders = targetDbProviders;
            multiDbProvider.AfterPropertiesSet();
            try
            {
                MultiDelegatingDbProvider.CurrentDbProviderName = "db2";
                Assert.AreEqual("connString1", multiDbProvider.ConnectionString);
                Assert.Fail("InvalidDataAccessApiUsageException should have been thrown");
            }
            catch (InvalidDataAccessApiUsageException exception)
            {
                Assert.AreEqual("'db2' was not under the thread local key 'dbProviderName' and no default IDbProvider was set.", exception.Message);
            }
            finally
            {
                LogicalThreadContext.FreeNamedDataSlot(MultiDelegatingDbProvider.CURRENT_DBPROVIDER_SLOTNAME);
            }
        }

        [Test]
        public void MatchingProviderDefinedInThreadLocalStorage()
        {
            IDbProvider provider1 = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            provider1.ConnectionString = "connString1";
            IDbProvider provider2 = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            provider2.ConnectionString = "connString2";
            MultiDelegatingDbProvider multiDbProvider = new MultiDelegatingDbProvider();
            IDictionary targetDbProviders = new Hashtable();
            targetDbProviders.Add("db1", provider1);
            targetDbProviders.Add("db2", provider2);
            multiDbProvider.DefaultDbProvider = provider1;
            multiDbProvider.TargetDbProviders = targetDbProviders;
            multiDbProvider.AfterPropertiesSet();

            //an aside, set setter for connection string
            multiDbProvider.ConnectionString = "connString1Reset";
            Assert.AreEqual("connString1Reset", multiDbProvider.ConnectionString);

            MultiDelegatingDbProvider.CurrentDbProviderName = "db2";
            try
            {
                Assert.AreEqual("connString2", multiDbProvider.ConnectionString);
            }
            finally
            {
                LogicalThreadContext.FreeNamedDataSlot(MultiDelegatingDbProvider.CURRENT_DBPROVIDER_SLOTNAME);
            }
        }

        [Test]
        public void FallbackToDefault()
        {
            IDbProvider provider1 = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            provider1.ConnectionString = "connString1";
            IDbProvider provider2 = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            provider2.ConnectionString = "connString2";
            MultiDelegatingDbProvider multiDbProvider = new MultiDelegatingDbProvider();
            IDictionary targetDbProviders = new Hashtable();
            targetDbProviders.Add("db1", provider1);
            targetDbProviders.Add("db2", provider2);
            multiDbProvider.DefaultDbProvider = provider1;
            multiDbProvider.TargetDbProviders = targetDbProviders;
            multiDbProvider.AfterPropertiesSet();

            MultiDelegatingDbProvider.CurrentDbProviderName = "db314";
            try
            {
                Assert.AreEqual("connString1", multiDbProvider.ConnectionString);
            }
            finally
            {
                LogicalThreadContext.FreeNamedDataSlot(MultiDelegatingDbProvider.CURRENT_DBPROVIDER_SLOTNAME);
            }
        }

        [Test]
        public void CreateOperations()
        {
            IDbProvider dbProvider = mocks.StrictMock<IDbProvider>();
            IDbConnection mockConnection = mocks.StrictMock<IDbConnection>();            
            Expect.Call(dbProvider.CreateConnection()).Return(mockConnection).Repeat.Once();

            IDbCommand mockCommand = (IDbCommand)mocks.CreateMock(typeof(IDbCommand));
            Expect.Call(dbProvider.CreateCommand()).Return(mockCommand).Repeat.Once();

            IDbDataParameter mockParameter = (IDbDataParameter) mocks.CreateMock(typeof (IDbDataParameter));
            Expect.Call(dbProvider.CreateParameter()).Return(mockParameter).Repeat.Once();

            IDbDataAdapter mockDataAdapter = (IDbDataAdapter) mocks.CreateMock(typeof (IDbDataAdapter));
            Expect.Call(dbProvider.CreateDataAdapter()).Return(mockDataAdapter).Repeat.Once();

            DbCommandBuilder mockDbCommandBuilder = (DbCommandBuilder) mocks.CreateMock(typeof (DbCommandBuilder));
            Expect.Call(dbProvider.CreateCommandBuilder()).Return(mockDbCommandBuilder).Repeat.Once();

            Expect.Call(dbProvider.CreateParameterName("p1")).Return("@p1").Repeat.Once();
            Expect.Call(dbProvider.CreateParameterNameForCollection("c1")).Return("cc1");

            IDbMetadata mockDbMetaData = (IDbMetadata) mocks.CreateMock(typeof (IDbMetadata));
            Expect.Call(dbProvider.DbMetadata).Return(mockDbMetaData);

            Exception e = new Exception("foo");
            Expect.Call(dbProvider.ExtractError(e)).Return("badsql").Repeat.Once();
            DbException dbException = (DbException) mocks.CreateMock(typeof (DbException));

            
            MultiDelegatingDbProvider multiDbProvider = new MultiDelegatingDbProvider();
            IDictionary targetDbProviders = new Hashtable();
            targetDbProviders.Add("db1", dbProvider);
            multiDbProvider.DefaultDbProvider = dbProvider;
            multiDbProvider.TargetDbProviders = targetDbProviders;
            multiDbProvider.AfterPropertiesSet();


            mocks.ReplayAll();

            Assert.IsNotNull(multiDbProvider.CreateConnection());
            Assert.IsNotNull(multiDbProvider.CreateCommand());
            Assert.IsNotNull(multiDbProvider.CreateParameter());
            Assert.IsNotNull(multiDbProvider.CreateDataAdapter());
            Assert.IsNotNull(multiDbProvider.CreateCommandBuilder() as DbCommandBuilder);
            Assert.AreEqual("@p1", multiDbProvider.CreateParameterName("p1"));
            Assert.AreEqual("cc1", multiDbProvider.CreateParameterNameForCollection("c1"));
            Assert.IsNotNull(multiDbProvider.DbMetadata);
            Assert.AreEqual("badsql", multiDbProvider.ExtractError(e));
            Assert.IsTrue(multiDbProvider.IsDataAccessException(dbException));
            Assert.IsFalse(multiDbProvider.IsDataAccessException(e));
            mocks.VerifyAll();
        }
    }
}