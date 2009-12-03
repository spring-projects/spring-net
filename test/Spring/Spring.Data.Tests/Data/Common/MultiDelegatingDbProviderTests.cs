#region License

/*
 * Copyright 2002-2009 the original author or authors.
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
using NUnit.Framework;
using Spring.Dao;
using Spring.Objects;

namespace Spring.Data.Common
{
    /// <summary>
    /// Test for MultiDelegatingDbProvider
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    [TestFixture]
    public class MultiDelegatingDbProviderTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CreationWhenNoRequiredPropertiesSet()
        {
            MultiDelegatingDbProvider dbProvider = new MultiDelegatingDbProvider();
            dbProvider.AfterPropertiesSet();
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
            } catch (ArgumentException ex)
            {
                Assert.AreEqual("Key identifying target IDbProvider in TargetDbProviders dictionary property is required to be of type string.  Key = [1], type = [System.Int32]", ex.Message);
            }
        }

        [Test]
        public void CreationWithWrongTypeDictionaryValues()
        {
            try
            {
                MultiDelegatingDbProvider dbProvider = new MultiDelegatingDbProvider();
                IDictionary targetDbProviders = new Hashtable();
                targetDbProviders.Add("foo", 1);
                dbProvider.TargetDbProviders = targetDbProviders;
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
            } catch (InvalidDataAccessApiUsageException exception)
            {
                Assert.AreEqual("No provider name found in thread local storage.  Consider setting the property DefaultDbProvider to fallback to a default value.", exception.Message);
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

            MultiDelegatingDbProvider.CurrentDbProviderName = "db2";
            Assert.AreEqual("connString2", multiDbProvider.ConnectionString);                
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
            Assert.AreEqual("connString1", multiDbProvider.ConnectionString);
        }
    }

}