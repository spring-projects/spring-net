/*
 * Copyright � 2002-2011 the original author or authors.
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

namespace Spring.Objects.Factory.Config;

/// <summary>
/// Unit tests for the ConnectionStringsVariableSource class.
/// </summary>
/// <author>Aleksandar Seovic</author>
[TestFixture]
public sealed class ConnectionStringsVariableSourceTests
{
    [Test]
    public void TestVariablesResolution()
    {
        ConnectionStringsVariableSource vs = new ConnectionStringsVariableSource();

        // existing vars
        Assert.That(vs.ResolveVariable("mySqlDataSource.connectionString"), Is.EqualTo("mySqlServerConnectionString"));
        Assert.That(vs.ResolveVariable("mySqlDataSource.providerName"), Is.EqualTo("System.Data.SqlClient"));
        Assert.That(vs.ResolveVariable("myOracleDataSource.connectionString"), Is.EqualTo("myOracleConnectionString"));
        Assert.That(vs.ResolveVariable("myOracleDataSource.providerName"), Is.EqualTo("System.Data.OracleClient"));

        // non-existant variable
        Assert.That(vs.ResolveVariable("dummy"), Is.Null);
    }
}
