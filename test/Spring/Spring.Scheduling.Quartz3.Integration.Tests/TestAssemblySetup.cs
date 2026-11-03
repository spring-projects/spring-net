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

using NUnit.Framework;
using Spring.IntegrationTests;

/// <summary>
/// Starts the SQL Server the tests of this assembly run against (see <see cref="SqlServerTestEnvironment"/>), once,
/// before the first of them, with a quartz database that holds the Quartz.NET tables.
/// </summary>
[SetUpFixture]
public class TestAssemblySetup
{
    private const string DataDirectory = "test/Spring/Spring.Scheduling.Quartz3.Integration.Tests/Data/";

    [OneTimeSetUp]
    public async Task SetUp()
    {
        await SqlServerTestEnvironment.InitializeAsync(
            [
                DataDirectory + "Spring.Scheduling.Quartz3.Integration.Tests_database.sql",
                DataDirectory + "tables_sqlServer.sql",
            ],
            new Dictionary<string, string> { ["[enter_db_name_here]"] = "[quartz]" });
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await SqlServerTestEnvironment.DisposeAsync();
    }
}
