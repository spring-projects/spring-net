/*
 * Copyright 2002-2026 the original author or authors.
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

using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Quartz;
using Quartz.Configuration;
using Quartz.Impl.AdoJobStore;
using QuartzDbProvider = Quartz.Impl.AdoJobStore.Common.IDbProvider;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Tests for the persistent store that <see cref="SchedulerFactoryObject.DbProvider"/> configures, and for
/// <see cref="SpringDbProviderAdapter"/>. None of them connects to a database.
/// </summary>
[TestFixture]
public class SpringDbProviderAdapterTests
{
    private const string ConnectionString = "Server=nowhere;Database=quartz;User Id=u;Password=p;";

    private static Spring.Data.Common.IDbProvider CreateSpringProvider()
    {
        Spring.Data.Common.IDbProvider provider = Spring.Data.Common.DbProviderFactory.GetDbProvider("System.Data.SqlClient");
        provider.ConnectionString = ConnectionString;
        return provider;
    }

    /// <summary>
    /// Connections and commands are the ones of the Spring provider, and every call makes a new connection.
    /// </summary>
    [Test]
    public void TestAdapter_ConnectionsAndCommandsComeFromTheSpringProvider()
    {
        SpringDbProviderAdapter adapter = new SpringDbProviderAdapter(CreateSpringProvider(), "SystemDataSqlClient");

        using DbConnection first = adapter.CreateConnection();
        using DbConnection second = adapter.CreateConnection();
        using DbCommand command = adapter.CreateCommand();

        Assert.That(first, Is.Not.SameAs(second));
        Assert.That(first.GetType().FullName, Is.EqualTo("System.Data.SqlClient.SqlConnection"));
        Assert.That(first.ConnectionString, Does.Contain("Database=quartz"));
        Assert.That(first.State, Is.EqualTo(System.Data.ConnectionState.Closed), "Quartz opens the connection itself");
        Assert.That(command.GetType().FullName, Is.EqualTo("System.Data.SqlClient.SqlCommand"));
        Assert.That(adapter.ConnectionString, Is.EqualTo(ConnectionString));
        Assert.That(() => adapter.Shutdown(), Throws.Nothing);
    }

    /// <summary>
    /// The metadata is Quartz's description of the dialect with the types of the Spring provider.
    /// </summary>
    [Test]
    public void TestAdapter_MetadataIsTheQuartzDialectWithTheSpringTypes()
    {
        Spring.Data.Common.IDbProvider springProvider = CreateSpringProvider();
        SpringDbProviderAdapter adapter = new SpringDbProviderAdapter(springProvider, "SystemDataSqlClient");

        var metadata = adapter.Metadata;

        Assert.That(metadata.ParameterNamePrefix, Is.EqualTo("@"), "from the Quartz dialect");
        Assert.That(metadata.UseParameterNamePrefixInParameterCollection, Is.True);
        Assert.That(metadata.ParameterDbTypePropertyName, Is.EqualTo("SqlDbType"));
        Assert.That(metadata.DbBinaryTypeName, Is.EqualTo("VarBinary"));
        Assert.That(metadata.ConnectionType, Is.EqualTo(springProvider.DbMetadata.ConnectionType), "from the Spring provider");
        Assert.That(metadata.CommandType, Is.EqualTo(springProvider.DbMetadata.CommandType));
        Assert.That(metadata.ParameterType, Is.EqualTo(springProvider.DbMetadata.ParameterType));
        Assert.That(metadata.ExceptionType, Is.EqualTo(springProvider.DbMetadata.ExceptionType));
    }

    /// <summary>
    /// A name that Quartz does not describe is refused with the list of the names that it does.
    /// </summary>
    [Test]
    public void TestAdapter_UnknownDialect()
    {
        var exception = Assert.Throws<ArgumentException>(() => new SpringDbProviderAdapter(CreateSpringProvider(), "Informix"));

        Assert.That(exception.Message, Does.Contain("Informix").And.Contain("SqlServer").And.Contain("Npgsql"));
    }

    /// <summary>
    /// With a database provider, the scheduler is configured with the persistent store: the Spring connections, the driver
    /// delegate of the dialect, the options that make it accept an enlisted transaction, and the typed options.
    /// </summary>
    [Test]
    public void TestSchedulerFactoryObject_ConfiguresThePersistentStore()
    {
        TestSchedulerFactoryObject factory = new TestSchedulerFactoryObject(TestSchedulerFactoryObject.CreateFakeScheduler());
        try
        {
            factory.DbProvider = CreateSpringProvider();
            factory.QuartzDbProvider = "SystemDataSqlClient";
            factory.TablePrefix = "SPRING_";
            factory.MisfireThreshold = TimeSpan.FromSeconds(90);
            factory.AfterPropertiesSet();

            AdoJobStoreOptions options = factory.GetOptions<AdoJobStoreOptions>();
            Assert.That(options.AcceptEnlistedTransactions, Is.True);
            Assert.That(options.UseDbLocks, Is.True);
            Assert.That(options.TablePrefix, Is.EqualTo("SPRING_"));
            Assert.That(options.MisfireThreshold, Is.EqualTo(TimeSpan.FromSeconds(90)));
            Assert.That(options.DataSource, Is.Not.Null.And.Not.Empty);
            Assert.That(factory.GetOptions<ClusteringOptions>().Enabled, Is.False);

            using ServiceProvider services = factory.BuilderServices.BuildServiceProvider();
            Assert.That(services.GetRequiredService<QuartzDbProvider>(), Is.TypeOf<SpringDbProviderAdapter>());
            Assert.That(services.GetRequiredService<IDriverDelegate>(), Is.TypeOf<SqlServerDelegate>());
        }
        finally
        {
            factory.DisposeServices();
            factory.Dispose();
        }
    }

    /// <summary>
    /// Clustering is turned on with its check-in interval.
    /// </summary>
    [Test]
    public void TestSchedulerFactoryObject_Clustered()
    {
        TestSchedulerFactoryObject factory = new TestSchedulerFactoryObject(TestSchedulerFactoryObject.CreateFakeScheduler());
        try
        {
            factory.DbProvider = CreateSpringProvider();
            factory.QuartzDbProvider = "SystemDataSqlClient";
            factory.Clustered = true;
            factory.ClusterCheckinInterval = TimeSpan.FromSeconds(3);
            factory.AfterPropertiesSet();

            ClusteringOptions clustering = factory.GetOptions<ClusteringOptions>();
            Assert.That(clustering.Enabled, Is.True);
            Assert.That(clustering.CheckinInterval, Is.EqualTo(TimeSpan.FromSeconds(3)));
        }
        finally
        {
            factory.DisposeServices();
            factory.Dispose();
        }
    }

    /// <summary>
    /// Every Quartz dialect has a driver delegate.
    /// </summary>
    [TestCase("SqlServer", typeof(SqlServerDelegate))]
    [TestCase("SystemDataSqlClient", typeof(SqlServerDelegate))]
    [TestCase("MicrosoftDataSqlClient", typeof(SqlServerDelegate))]
    [TestCase("Npgsql", typeof(PostgreSQLDelegate))]
    [TestCase("MySql", typeof(MySQLDelegate))]
    [TestCase("MySqlConnector", typeof(MySQLDelegate))]
    [TestCase("SQLite", typeof(SQLiteDelegate))]
    [TestCase("SQLite-Microsoft", typeof(SQLiteDelegate))]
    [TestCase("Firebird", typeof(FirebirdDelegate))]
    [TestCase("OracleODPManaged", typeof(OracleDelegate))]
    public void TestDialect_SelectsTheDriverDelegate(string dialect, Type expectedDelegate)
    {
        Assert.That(SpringDbProviderAdapter.GetDriverDelegateType(dialect), Is.EqualTo(expectedDelegate));
    }

    /// <summary>
    /// A dialect that Quartz does not know has no driver delegate.
    /// </summary>
    [Test]
    public void TestDialect_Unknown()
    {
        Assert.Throws<ArgumentException>(() => SpringDbProviderAdapter.GetDriverDelegateType("Informix"));
    }
}
