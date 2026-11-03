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

using System.Collections;
using System.Data;
using System.Data.Common;
using FakeItEasy;
using NUnit.Framework;
using Spring.Dao;
using Spring.Threading;

namespace Spring.Data.Common;

/// <summary>
/// Test for MultiDelegatingDbProvider
/// </summary>
/// <author>Mark Pollack (.NET)</author>
[TestFixture]
public class MultiDelegatingDbProviderTests
{
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
            Assert.That(ex.Message, Is.EqualTo("Key identifying target IDbProvider in TargetDbProviders dictionary property is required to be of type string.  Key = [1], type = [System.Int32]"));
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
            Assert.That(ex.Message, Is.EqualTo("Value in TargetDbProviders dictionary is not of type IDbProvider.  Type = [System.Int32]"));
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
            Assert.That(multiDbProvider.ConnectionString, Is.EqualTo("connString1"));
            Assert.Fail("InvalidDataAccessApiUsageException should have been thrown");
        }
        catch (InvalidDataAccessApiUsageException exception)
        {
            Assert.That(exception.Message, Is.EqualTo("No provider name found in thread local storage.  Consider setting the property DefaultDbProvider to fallback to a default value."));
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
            Assert.That(multiDbProvider.ConnectionString, Is.EqualTo("connString1"));
            Assert.Fail("InvalidDataAccessApiUsageException should have been thrown");
        }
        catch (InvalidDataAccessApiUsageException exception)
        {
            Assert.That(exception.Message, Is.EqualTo("'db2' was not under the thread local key 'dbProviderName' and no default IDbProvider was set."));
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
        Assert.That(multiDbProvider.ConnectionString, Is.EqualTo("connString1Reset"));

        MultiDelegatingDbProvider.CurrentDbProviderName = "db2";
        try
        {
            Assert.That(multiDbProvider.ConnectionString, Is.EqualTo("connString2"));
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
            Assert.That(multiDbProvider.ConnectionString, Is.EqualTo("connString1"));
        }
        finally
        {
            LogicalThreadContext.FreeNamedDataSlot(MultiDelegatingDbProvider.CURRENT_DBPROVIDER_SLOTNAME);
        }
    }

    [Test]
    public void CreateOperations()
    {
        IDbProvider dbProvider = A.Fake<IDbProvider>();
        IDbConnection mockConnection = A.Fake<IDbConnection>();
        A.CallTo(() => dbProvider.CreateConnection()).Returns(mockConnection).Once();

        IDbCommand mockCommand = A.Fake<IDbCommand>();
        A.CallTo(() => dbProvider.CreateCommand()).Returns(mockCommand).Once();

        IDbDataParameter mockParameter = A.Fake<IDbDataParameter>();
        A.CallTo(() => dbProvider.CreateParameter()).Returns(mockParameter).Once();

        IDbDataAdapter mockDataAdapter = A.Fake<IDbDataAdapter>();
        A.CallTo(() => dbProvider.CreateDataAdapter()).Returns(mockDataAdapter).Once();

        DbCommandBuilder mockDbCommandBuilder = A.Fake<DbCommandBuilder>();
        A.CallTo(() => dbProvider.CreateCommandBuilder()).Returns(mockDbCommandBuilder).Once();

        A.CallTo(() => dbProvider.CreateParameterName("p1")).Returns("@p1").Once();
        A.CallTo(() => dbProvider.CreateParameterNameForCollection("c1")).Returns("cc1");

        IDbMetadata mockDbMetaData = A.Fake<IDbMetadata>();
        A.CallTo(() => dbProvider.DbMetadata).Returns(mockDbMetaData);

        Exception e = new Exception("foo");
        A.CallTo(() => dbProvider.ExtractError(e)).Returns("badsql").Once();
        DbException dbException = A.Fake<DbException>();

        MultiDelegatingDbProvider multiDbProvider = new MultiDelegatingDbProvider();
        IDictionary targetDbProviders = new Hashtable();
        targetDbProviders.Add("db1", dbProvider);
        multiDbProvider.DefaultDbProvider = dbProvider;
        multiDbProvider.TargetDbProviders = targetDbProviders;
        multiDbProvider.AfterPropertiesSet();

        Assert.That(multiDbProvider.CreateConnection(), Is.Not.Null);
        Assert.That(multiDbProvider.CreateCommand(), Is.Not.Null);
        Assert.That(multiDbProvider.CreateParameter(), Is.Not.Null);
        Assert.That(multiDbProvider.CreateDataAdapter(), Is.Not.Null);
        Assert.That(multiDbProvider.CreateCommandBuilder() as DbCommandBuilder, Is.Not.Null);
        Assert.That(multiDbProvider.CreateParameterName("p1"), Is.EqualTo("@p1"));
        Assert.That(multiDbProvider.CreateParameterNameForCollection("c1"), Is.EqualTo("cc1"));
        Assert.That(multiDbProvider.DbMetadata, Is.Not.Null);
        Assert.That(multiDbProvider.ExtractError(e), Is.EqualTo("badsql"));
        Assert.That(multiDbProvider.IsDataAccessException(dbException), Is.True);
        Assert.That(multiDbProvider.IsDataAccessException(e), Is.False);
    }
}
