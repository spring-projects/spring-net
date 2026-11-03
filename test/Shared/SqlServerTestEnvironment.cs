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

using System.Data.SqlClient;
using System.Text;

using Testcontainers.MsSql;

namespace Spring.IntegrationTests;

/// <summary>
/// Provisions the SQL Server that the integration test assemblies run against, and publishes where it is.
/// </summary>
/// <remarks>
/// <para>
/// This file is compiled into each SQL Server backed integration test project (a linked source file, not a project of
/// its own), and each assembly's <c>[SetUpFixture]</c> calls <see cref="InitializeAsync"/> and
/// <see cref="DisposeAsync"/>. Every assembly therefore gets its own server, so the assemblies can run in parallel
/// without port clashes.
/// </para>
/// <para>
/// By default a SQL Server container is started with Testcontainers, which needs only a running Docker daemon.
/// When the environment variable <see cref="AdminConnectionStringVariable"/> is set, to an administrator's connection
/// string, no container is started and the same scripts run against that server instead (for a developer without a Linux
/// container engine).
/// </para>
/// <para>
/// Once the scripts have run, the process environment variables below tell the tests where the server is. The Spring XML
/// contexts read them through <c>${NAME}</c> placeholders; C# code uses <see cref="GetConnectionString"/>.
/// </para>
/// <list type="bullet">
/// <item><description><c>SPRING_TEST_SQLSERVER_HOST</c>: the server as <c>host,port</c> (the value of <c>Data Source</c>).</description></item>
/// <item><description><c>SPRING_TEST_SQLSERVER_USER</c> and <c>SPRING_TEST_SQLSERVER_PASSWORD</c>: the <c>springqa</c> login the scripts create.</description></item>
/// <item><description><c>SPRING_TEST_SQLSERVER_CS_&lt;DATABASE&gt;</c>: a complete connection string, as <c>springqa</c>, for each
/// database the scripts created, with the name upper-cased, for example <c>SPRING_TEST_SQLSERVER_CS_SPRING</c>,
/// <c>SPRING_TEST_SQLSERVER_CS_CREDITSANDDEBITS</c> and <c>SPRING_TEST_SQLSERVER_CS_QUARTZ</c>.</description></item>
/// </list>
/// </remarks>
internal static class SqlServerTestEnvironment
{
    /// <summary>
    /// The image the container runs. Pinned, so that a run does not change with the registry's "latest".
    /// </summary>
    public const string Image = "mcr.microsoft.com/mssql/server:2022-CU27-ubuntu-22.04";

    /// <summary>
    /// When set to an administrator's connection string, the tests use that server instead of starting a container.
    /// </summary>
    public const string AdminConnectionStringVariable = "SPRING_TEST_SQLSERVER";

    public const string HostVariable = "SPRING_TEST_SQLSERVER_HOST";

    public const string UserVariable = "SPRING_TEST_SQLSERVER_USER";

    public const string PasswordVariable = "SPRING_TEST_SQLSERVER_PASSWORD";

    public const string ConnectionStringVariablePrefix = "SPRING_TEST_SQLSERVER_CS_";

    /// <summary>
    /// The login the scripts create and the tests connect as.
    /// </summary>
    public const string TestUser = "springqa";

    public const string TestPassword = "springqa";

    /// <summary>
    /// The script, relative to the repository root, that creates the login and the databases every suite shares.
    /// </summary>
    public const string CommonScript = "build-support/create-integration-test-databases-and-users.sql";

    private const string ContainerAdminPassword = "Spring!Adm1nP4ssw0rd";

    private static readonly SemaphoreSlim s_lock = new(1, 1);
    private static MsSqlContainer s_container;
    private static bool s_initialized;

    /// <summary>
    /// Starts (or connects to) the server, runs <see cref="CommonScript"/> and then <paramref name="scripts"/>, and
    /// publishes the environment variables.
    /// </summary>
    /// <param name="scripts">Further scripts to run, in order, as paths relative to the repository root.</param>
    /// <param name="replacements">Text to substitute in every script, for scripts with placeholders.</param>
    public static async Task InitializeAsync(IEnumerable<string> scripts = null, IReadOnlyDictionary<string, string> replacements = null)
    {
        await s_lock.WaitAsync();
        try
        {
            if (s_initialized)
            {
                return;
            }

            try
            {
                string adminConnectionString = Environment.GetEnvironmentVariable(AdminConnectionStringVariable);
                if (string.IsNullOrWhiteSpace(adminConnectionString))
                {
                    s_container = new MsSqlBuilder(Image)
                        .WithPassword(ContainerAdminPassword)
                        .Build();
                    await s_container.StartAsync();
                    adminConnectionString = s_container.GetConnectionString();
                }

                var admin = new SqlConnectionStringBuilder(adminConnectionString)
                {
                    InitialCatalog = "master",
                    TrustServerCertificate = true,
                };

                foreach (string script in new[] { CommonScript }.Concat(scripts ?? []))
                {
                    await RunScriptAsync(admin.ConnectionString, script, replacements);
                }

                await PublishAsync(admin);
                s_initialized = true;
            }
            catch
            {
                await DisposeContainerAsync();
                throw;
            }
        }
        finally
        {
            s_lock.Release();
        }
    }

    /// <summary>
    /// Stops the container, if one was started.
    /// </summary>
    public static async Task DisposeAsync()
    {
        await s_lock.WaitAsync();
        try
        {
            s_initialized = false;
            await DisposeContainerAsync();
        }
        finally
        {
            s_lock.Release();
        }
    }

    /// <summary>
    /// Gets the connection string, as the <c>springqa</c> login, for a database a script created.
    /// </summary>
    public static string GetConnectionString(string database)
    {
        string name = ConnectionStringVariablePrefix + database.ToUpperInvariant();
        return Environment.GetEnvironmentVariable(name)
               ?? throw new InvalidOperationException(
                   $"No connection string for database '{database}': '{name}' is not set. "
                   + "Run the test from an assembly whose [SetUpFixture] calls SqlServerTestEnvironment.InitializeAsync.");
    }

    /// <summary>
    /// Gets the <c>host,port</c> of the server.
    /// </summary>
    public static string GetHost()
    {
        return Environment.GetEnvironmentVariable(HostVariable)
               ?? throw new InvalidOperationException($"'{HostVariable}' is not set; SqlServerTestEnvironment has not been initialized.");
    }

    private static async Task PublishAsync(SqlConnectionStringBuilder admin)
    {
        var databases = new List<string>();
        using (var connection = new SqlConnection(admin.ConnectionString))
        {
            await connection.OpenAsync();
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name";
            using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                databases.Add(reader.GetString(0));
            }
        }

        Environment.SetEnvironmentVariable(HostVariable, admin.DataSource);
        Environment.SetEnvironmentVariable(UserVariable, TestUser);
        Environment.SetEnvironmentVariable(PasswordVariable, TestPassword);

        foreach (string database in databases)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = admin.DataSource,
                InitialCatalog = database,
                UserID = TestUser,
                Password = TestPassword,
                PersistSecurityInfo = true,
                TrustServerCertificate = true,
            };
            Environment.SetEnvironmentVariable(ConnectionStringVariablePrefix + database.ToUpperInvariant(), builder.ConnectionString);
        }
    }

    private static async Task RunScriptAsync(string connectionString, string repositoryRelativePath, IReadOnlyDictionary<string, string> replacements)
    {
        string path = ResolveRepositoryFile(repositoryRelativePath);

        // The scripts are UTF-16 or UTF-8 (SSMS output); ReadAllText follows the byte order mark.
        string text = File.ReadAllText(path);
        if (replacements is not null)
        {
            foreach (KeyValuePair<string, string> replacement in replacements)
            {
                text = text.Replace(replacement.Key, replacement.Value);
            }
        }

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // One connection for the whole script: the "USE [database]" statements the scripts begin with stay in effect for
        // the batches that follow, which is what sqlcmd and SSMS do.
        int batchNumber = 0;
        foreach (string batch in SplitBatches(text))
        {
            batchNumber++;
            try
            {
                using SqlCommand command = connection.CreateCommand();
                command.CommandText = batch;
                command.CommandTimeout = 120;
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException e)
            {
                throw new InvalidOperationException(
                    $"Script '{repositoryRelativePath}' failed in batch {batchNumber}: {e.Message}{Environment.NewLine}{batch}", e);
            }
        }
    }

    /// <summary>
    /// Splits a script into batches at the lines that consist of <c>GO</c>, which is a client command, not T-SQL.
    /// </summary>
    internal static IEnumerable<string> SplitBatches(string script)
    {
        var batch = new StringBuilder();
        foreach (string line in script.Split('\n'))
        {
            if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                if (batch.ToString().Trim().Length > 0)
                {
                    yield return batch.ToString();
                }

                batch.Clear();
            }
            else
            {
                batch.Append(line).Append('\n');
            }
        }

        if (batch.ToString().Trim().Length > 0)
        {
            yield return batch.ToString();
        }
    }

    private static string ResolveRepositoryFile(string repositoryRelativePath)
    {
        string relativePath = repositoryRelativePath.Replace('/', Path.DirectorySeparatorChar);
        DirectoryInfo current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            string candidate = Path.Combine(current.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException(
            $"Could not find '{repositoryRelativePath}' in any directory above '{AppContext.BaseDirectory}'. "
            + "The integration tests must run from a checkout of the repository.");
    }

    private static async Task DisposeContainerAsync()
    {
        MsSqlContainer container = s_container;
        s_container = null;
        if (container is not null)
        {
            await container.DisposeAsync();
        }
    }
}
