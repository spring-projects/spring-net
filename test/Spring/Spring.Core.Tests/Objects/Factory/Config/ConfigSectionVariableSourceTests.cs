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
/// Unit tests for the ConfigSectionVariableSource class.
/// </summary>
/// <author>Aleksandar Seovic</author>
[TestFixture]
public sealed class ConfigSectionVariableSourceTests
{
    [Test]
    public void TestVariablesResolutionWithSingleSection()
    {
        ConfigSectionVariableSource vs = new ConfigSectionVariableSource();
        vs.SectionName = "DaoConfiguration";

        // existing vars
        Assert.That(vs.ResolveVariable("maxResults"), Is.EqualTo("1000"));
        Assert.That(vs.ResolveVariable("MAXResults"), Is.EqualTo("1000"));

        // non-existant variable
        Assert.That(vs.ResolveVariable("dummy"), Is.Null);
    }

    [Test]
    public void TestVariablesResolutionWithTwoSections()
    {
        ConfigSectionVariableSource vs = new ConfigSectionVariableSource();
        vs.SectionNames = new string[] { "DaoConfiguration", "DatabaseConfiguration" };

        // existing vars
        Assert.That(vs.ResolveVariable("maxResults"), Is.EqualTo("1000"));
        Assert.That(vs.ResolveVariable("MAXResults"), Is.EqualTo("1000"));
        Assert.That(vs.ResolveVariable("connection.string"),
            Is.EqualTo(@"Provider=Microsoft.Jet.OLEDB.4.0; Data Source=c:\Northwind.mdb;User ID=Admin;Password=;"));
        Assert.That(vs.ResolveVariable("Connection.String"),
            Is.EqualTo(@"Provider=Microsoft.Jet.OLEDB.4.0; Data Source=c:\Northwind.mdb;User ID=Admin;Password=;"));

        // non-existant variable
        Assert.That(vs.ResolveVariable("dummy"), Is.Null);
    }

    [Test]
    public void TestVariableResolutionFromApplicationSettingsSchema()
    {
        ConfigSectionVariableSource vs = new ConfigSectionVariableSource();
        vs.SectionName = "applicationSettings/MyApp.Properties.Settings";
        Assert.That(vs.ResolveVariable("maxResults"), Is.EqualTo("1000"));
        Assert.That(vs.ResolveVariable("connection.string"),
            Is.EqualTo(@"Provider=Microsoft.Jet.OLEDB.4.0; Data Source=c:\Northwind.mdb;User ID=Admin;Password=;"));
    }
}
