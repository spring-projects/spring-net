/*
 * Copyright © 2002-2011 the original author or authors.
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

using Microsoft.Win32;
using NUnit.Framework;

#pragma warning disable CA1416 // is only supported on windows

namespace Spring.Objects.Factory.Config;

/// <summary>
/// Unit tests for the RegistryVariableSource class.
/// </summary>
/// <author>Aleksandar Seovic</author>
[Platform("Win")]
public sealed class RegistryVariableSourceTests
{
    private RegistryKey key;

    [SetUp]
    public void SetUp()
    {
        key = Registry.CurrentUser.CreateSubKey("RegistryVariableSourceTests");
        key.SetValue("name", "Aleks Seovic");
        key.SetValue("computer_name", "%COMPUTERNAME% is the name of my computer", RegistryValueKind.ExpandString);
        key.SetValue("age", 32, RegistryValueKind.DWord);
        key.SetValue("family", new string[] { "Marija", "Ana", "Nadja" });
        key.SetValue("bday", new byte[] { 24, 8, 74 });
        key.Flush();
    }

    [TearDown]
    public void TearDown()
    {
        Registry.CurrentUser.DeleteSubKey("RegistryVariableSourceTests");
    }

    [Test]
    public void TestVariablesResolution()
    {
        RegistryVariableSource rvs = new RegistryVariableSource();
        rvs.Key = key;

        // existing vars
        Assert.That(rvs.ResolveVariable("name"), Is.EqualTo("Aleks Seovic"));
        Assert.That(rvs.ResolveVariable("computer_name"),
            Is.EqualTo(Environment.GetEnvironmentVariable("COMPUTERNAME") + " is the name of my computer"));
        Assert.That(rvs.ResolveVariable("age"), Is.EqualTo("32"));
        // multi_sz
        Assert.That(rvs.ResolveVariable("family"), Is.EqualTo("Marija,Ana,Nadja"));
        // binary
        Assert.That(rvs.ResolveVariable("bday"), Is.EqualTo(null));

        // non-existant variable
        Assert.That(rvs.ResolveVariable("xyz"), Is.Null);
    }
}
