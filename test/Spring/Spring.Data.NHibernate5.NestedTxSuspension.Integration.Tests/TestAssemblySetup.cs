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
/// before the first of them.
/// </summary>
[SetUpFixture]
public class TestAssemblySetup
{
    [OneTimeSetUp]
    public async Task SetUp()
    {
        await SqlServerTestEnvironment.InitializeAsync(
        [
            "test/Spring/Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests/Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests_database.sql",
        ]);
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await SqlServerTestEnvironment.DisposeAsync();
    }
}
