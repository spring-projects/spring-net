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
using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;
using Spring.Util;
using IDbMetadata = Spring.Data.Common.IDbMetadata;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// Adapts Spring's <see cref="Data.Common.IDbProvider"/> to Quartz's
/// <see cref="IDbProvider"/>.
/// </summary>
/// <remarks>
/// <para>
/// Spring supplies the connections and commands. The rest of what Quartz needs to know about the database (the SQL dialect:
/// parameter prefix, how a binary parameter is typed) is Quartz's own description of the provider that is named by
/// <c>quartzDbProvider</c>, one of Quartz's built-in provider names such as <c>SqlServer</c>,
/// <c>SystemDataSqlClient</c>, <c>Npgsql</c>, <c>MySqlConnector</c>, <c>SQLite-Microsoft</c>, <c>Firebird</c> or
/// <c>OracleODPManaged</c>. The connection, command, parameter and exception types in it are replaced with those of the
/// Spring provider, so they describe what Spring actually creates.
/// </para>
/// <para>
/// Every call to <see cref="CreateConnection"/> returns a new connection. It is never the connection of a Spring
/// transaction: Quartz closes the connections it gets.
/// </para>
/// </remarks>
public class SpringDbProviderAdapter : IDbProvider
{
    private readonly Data.Common.IDbProvider dbProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpringDbProviderAdapter"/> class.
    /// </summary>
    /// <param name="dbProvider">The Spring db provider.</param>
    /// <param name="quartzDbProvider">The name of the Quartz built-in database provider that describes the SQL dialect.</param>
    public SpringDbProviderAdapter(Data.Common.IDbProvider dbProvider, string quartzDbProvider)
    {
        AssertUtils.ArgumentNotNull(dbProvider, nameof(dbProvider));
        AssertUtils.ArgumentHasText(quartzDbProvider, nameof(quartzDbProvider));
        this.dbProvider = dbProvider;
        QuartzDbProvider = quartzDbProvider;
        Metadata = CreateMetadata(dbProvider.DbMetadata, QuartzDialect.ResolveMetadata(quartzDbProvider, dbProvider.ConnectionString));
    }

    /// <summary>
    /// The name of the Quartz built-in database provider that describes the SQL dialect.
    /// </summary>
    public string QuartzDbProvider { get; }

    /// <summary>
    /// The driver delegate (the SQL dialect of the job store) that Quartz uses for the database provider of that name.
    /// </summary>
    /// <param name="quartzDbProvider">The name of the Quartz built-in database provider.</param>
    /// <exception cref="ArgumentException">if Quartz has no description for the name.</exception>
    public static Type GetDriverDelegateType(string quartzDbProvider)
    {
        return QuartzDialect.GetDriverDelegateType(quartzDbProvider);
    }

    /// <summary>
    /// Creates the command.
    /// </summary>
    public DbCommand CreateCommand()
    {
        return (DbCommand) dbProvider.CreateCommand();
    }

    /// <summary>
    /// Creates the connection.
    /// </summary>
    public DbConnection CreateConnection()
    {
        return (DbConnection) dbProvider.CreateConnection();
    }

    /// <summary>
    /// Gets the connection string.
    /// </summary>
    public string ConnectionString => dbProvider.ConnectionString;

    /// <summary>
    /// Gets the metadata.
    /// </summary>
    public DbMetadata Metadata { get; }

    /// <summary>
    /// Shutdowns this instance.
    /// </summary>
    public void Shutdown()
    {
        // no-op: the Spring provider owns nothing that needs releasing
    }

    /// <summary>
    /// Combines Quartz's description of the dialect with the types of the Spring provider.
    /// </summary>
    internal static DbMetadata CreateMetadata(IDbMetadata spring, DbMetadata quartz)
    {
        return quartz with
        {
            ProductName = spring.ProductName ?? quartz.ProductName,
            ConnectionType = spring.ConnectionType ?? quartz.ConnectionType,
            CommandType = spring.CommandType ?? quartz.CommandType,
            ParameterType = spring.ParameterType ?? quartz.ParameterType,
            ExceptionType = spring.ExceptionType ?? quartz.ExceptionType,
            ParameterDbType = spring.ParameterDbType ?? quartz.ParameterDbType
        };
    }
}

/// <summary>
/// Maps the name of a Quartz built-in database provider to the pieces of Quartz that implement its SQL dialect.
/// </summary>
internal static class QuartzDialect
{
    // The names that Quartz 4.7.2 describes (BuiltInDbMetadataFactory).
    private static readonly string[] s_names =
    [
        "SqlServer",
        "SystemDataSqlClient",
        "MicrosoftDataSqlClient",
        "Npgsql",
        "MySql",
        "MySqlConnector",
        "SQLite",
        "SQLite-Microsoft",
        "Firebird",
        "OracleODPManaged"
    ];

    public static IReadOnlyList<string> Names => s_names;

    /// <summary>
    /// Quartz's description of the provider, without the Spring specific parts.
    /// </summary>
    public static DbMetadata ResolveMetadata(string quartzDbProvider, string connectionString)
    {
        AssertKnown(quartzDbProvider);

        try
        {
            // The public way to Quartz's built-in description: the provider that Quartz itself would use for the name.
            return new DbProvider(quartzDbProvider, connectionString ?? string.Empty).Metadata;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Quartz could not describe the database provider '{quartzDbProvider}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// The driver delegate (the SQL of the job store) that Quartz uses for the provider.
    /// </summary>
    public static Type GetDriverDelegateType(string quartzDbProvider)
    {
        AssertKnown(quartzDbProvider);

        return quartzDbProvider switch
        {
            "SqlServer" or "SystemDataSqlClient" or "MicrosoftDataSqlClient" => typeof(SqlServerDelegate),
            "Npgsql" => typeof(PostgreSQLDelegate),
            "MySql" or "MySqlConnector" => typeof(MySQLDelegate),
            "SQLite" or "SQLite-Microsoft" => typeof(SQLiteDelegate),
            "Firebird" => typeof(FirebirdDelegate),
            _ => typeof(OracleDelegate)
        };
    }

    /// <summary>
    /// Selects the driver delegate of the provider on the store.
    /// </summary>
    public static void UseDriverDelegate(IPersistentStoreBuilder store, string quartzDbProvider)
    {
        Type delegateType = GetDriverDelegateType(quartzDbProvider);
        store.UseDriverDelegate(provider => (IDriverDelegate) ActivatorUtilities.CreateInstance(provider, delegateType));
    }

    private static void AssertKnown(string quartzDbProvider)
    {
        if (!s_names.Contains(quartzDbProvider, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                $"Unknown Quartz database provider '{quartzDbProvider}'. Valid names are: {string.Join(", ", s_names)}.",
                nameof(quartzDbProvider));
        }
    }
}
