/*
 * Copyright 2004 the original author or authors.
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

using System.Reflection;
using NUnit.Framework;
using Spring.Objects;

namespace Spring.Core.TypeResolution;

/// <summary>
/// Unit tests for the TypeResolutionUtils class.
/// </summary>
[TestFixture]
public sealed class TypeResolutionUtilsTests
{
    [Test]
    public void ResolveFromAssemblyQualifiedName()
    {
        Type testObjectType = TypeResolutionUtils.ResolveType("Spring.Objects.TestObject, Spring.Core.Tests");
        Assert.That(testObjectType, Is.Not.Null);
        Assert.That(testObjectType.Equals(typeof(TestObject)), Is.True);
    }

    [Test]
    public void ResolveFromBadAssemblyQualifiedName()
    {
        Assert.Throws<TypeLoadException>(() => TypeResolutionUtils.ResolveType("Spring.Objects.TestObject, Spring.Core.FooTests"));
    }

    [Test]
    public void ResolveFromShortName()
    {
        Type testObjectType = TypeResolutionUtils.ResolveType("Spring.Objects.TestObject");
        Assert.That(testObjectType, Is.Not.Null);
        Assert.That(testObjectType.Equals(typeof(TestObject)), Is.True);
    }

    [Test]
    public void ResolveFromBadShortName()
    {
        Assert.Throws<TypeLoadException>(() => TypeResolutionUtils.ResolveType("Spring.Objects.FooBarTestObject"));
    }

    [Test]
    public void ResolveInterfaceArrayFromStringArray()
    {
        Type[] expected = new Type[] { typeof(IFoo) };
        string[] input = new string[] { typeof(IFoo).AssemblyQualifiedName };
        IList<Type> actual = TypeResolutionUtils.ResolveInterfaceArray(input);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Count, Is.EqualTo(expected.Length));
        Assert.That(actual[0], Is.EqualTo(expected[0]));
    }

    [Test]
    public void ResolveInterfaceArrayFromStringArrayWithNonInterfaceTypes()
    {
        string[] input = new string[] { GetType().AssemblyQualifiedName };
        Assert.Throws<ArgumentException>(() => TypeResolutionUtils.ResolveInterfaceArray(input));
    }

    [Test]
    public void MethodMatch()
    {
        MethodInfo absquatulateMethod = typeof(TestObject).GetMethod("Absquatulate");
        Assert.That(TypeResolutionUtils.MethodMatch("*", absquatulateMethod), Is.True, "Should match '*'");
        Assert.That(TypeResolutionUtils.MethodMatch("*tulate", absquatulateMethod), Is.True, "Should match '*tulate'");
        Assert.That(TypeResolutionUtils.MethodMatch("Absqua*", absquatulateMethod), Is.True, "Should match 'Absqua*'");
        Assert.That(TypeResolutionUtils.MethodMatch("*quatul*", absquatulateMethod), Is.True, "Should match '*quatul*'");
        Assert.That(TypeResolutionUtils.MethodMatch("Absquatulate", absquatulateMethod), Is.True, "Should match 'Absquatulate'");
        Assert.That(TypeResolutionUtils.MethodMatch("Absquatulate()", absquatulateMethod), Is.True, "Should match 'Absquatulate()'");
        Assert.That(TypeResolutionUtils.MethodMatch("Absquatulate()", absquatulateMethod), Is.True, "Should match 'Absquatulate()'");
        Assert.That(TypeResolutionUtils.MethodMatch("Absquatulate(string)", absquatulateMethod), Is.False, "Should not match 'Absquatulate(string)'");

        MethodInfo addPeriodicElementMethod = typeof(TestObject).GetMethod("AddPeriodicElement");
        Assert.That(TypeResolutionUtils.MethodMatch("AddPeriodicElement", addPeriodicElementMethod), Is.True, "Should match 'AddPeriodicElement'");
        Assert.That(TypeResolutionUtils.MethodMatch("AddPeriodicElement()", addPeriodicElementMethod), Is.False, "Should not match 'AddPeriodicElement()'");
        Assert.That(TypeResolutionUtils.MethodMatch("AddPeriodicElement(string)", addPeriodicElementMethod), Is.False, "Should not match 'AddPeriodicElement(string)'");
        Assert.That(TypeResolutionUtils.MethodMatch("AddPeriodicElement(string, string)", addPeriodicElementMethod), Is.True, "Should match 'AddPeriodicElement(string, string)'");
    }

    internal interface IFoo
    {
        bool Spanglish(string foo, object[] args);
    }
}
