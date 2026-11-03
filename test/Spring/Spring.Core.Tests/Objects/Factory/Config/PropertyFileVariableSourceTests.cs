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
using Spring.Core.IO;

namespace Spring.Objects.Factory.Config;

/// <summary>
/// Unit tests for the PropertyFileVariableSource class.
/// </summary>
/// <author>Aleksandar Seovic</author>
[TestFixture]
public sealed class PropertyFileVariableSourceTests
{
    [Test]
    public void TestVariablesResolutionWithSingleLocation()
    {
        PropertyFileVariableSource vs = new PropertyFileVariableSource();
        vs.Location =
            new AssemblyResource(
                "assembly://Spring.Core.Tests/Spring.Data.Spring.Objects.Factory.Config/one.properties");

        // existing vars
        Assert.That(vs.ResolveVariable("name"), Is.EqualTo("Aleks Seovic"));
        Assert.That(vs.ResolveVariable("age"), Is.EqualTo("32"));

        // non-existant variable
        Assert.That(vs.ResolveVariable("dummy"), Is.Null);
    }

    [Test]
    public void TestMissingResourceLocation()
    {
        PropertyFileVariableSource vs = new PropertyFileVariableSource();
        vs.IgnoreMissingResources = true;
        vs.Locations = new IResource[]
        {
            new AssemblyResource(
                "assembly://Spring.Core.Tests/Spring.Data.Spring.Objects.Factory.Config/non-existent.properties"),
            new AssemblyResource(
                "assembly://Spring.Core.Tests/Spring.Data.Spring.Objects.Factory.Config/one.properties"),
        };

        // existing vars
        Assert.That(vs.ResolveVariable("name"), Is.EqualTo("Aleks Seovic"));
        Assert.That(vs.ResolveVariable("age"), Is.EqualTo("32"));
    }

    [Test]
    public void TestVariablesResolutionWithTwoLocations()
    {
        PropertyFileVariableSource vs = new PropertyFileVariableSource();
        vs.Locations = new IResource[]
        {
            new AssemblyResource(
                "assembly://Spring.Core.Tests/Spring.Data.Spring.Objects.Factory.Config/one.properties"),
            new AssemblyResource(
                "assembly://Spring.Core.Tests/Spring.Data.Spring.Objects.Factory.Config/two.properties")
        };

        // existing vars
        Assert.That(vs.ResolveVariable("name"), Is.EqualTo("Aleksandar Seovic")); // should be overriden by the second file
        Assert.That(vs.ResolveVariable("age"), Is.EqualTo("32"));
        Assert.That(vs.ResolveVariable("family"), Is.EqualTo("Marija,Ana,Nadja"));

        // non-existant variable
        Assert.That(vs.ResolveVariable("dummy"), Is.Null);
    }
}
