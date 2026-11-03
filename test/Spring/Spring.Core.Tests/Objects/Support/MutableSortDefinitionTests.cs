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

using NUnit.Framework;

namespace Spring.Objects.Support;

/// <summary>
/// Unit tests for the MutableSortDefinition class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class MutableSortDefinitionTests
{
    [Test]
    public void Instantiation()
    {
        MutableSortDefinition def = new MutableSortDefinition();
        Assert.That(def.IgnoreCase, Is.True);
        Assert.That(def.Ascending, Is.True);
        Assert.That(def.Property, Is.Not.Null);
    }

    [Test]
    public void InstantiationWithCopy()
    {
        ISortDefinition copy = new MutableSortDefinition("Bing!", false, false);
        MutableSortDefinition def = new MutableSortDefinition(copy);
        Assert.That(def.IgnoreCase, Is.False);
        Assert.That(def.Ascending, Is.False);
        Assert.That(def.Property, Is.Not.Null);
        Assert.That(def.Property, Is.EqualTo(copy.Property));
    }

    [Test]
    public void TestEquals()
    {
        ISortDefinition copy = new MutableSortDefinition("Rab", false, false);
        MutableSortDefinition def = new MutableSortDefinition("Bing!", false, false);
        def.Property = "Rab";
        Assert.That(def, Is.EqualTo(copy));
        Assert.That(def.GetHashCode(), Is.EqualTo(copy.GetHashCode()));
        Assert.That(def.Equals(null), Is.False);
    }

    [Test]
    public void PropertySetter()
    {
        MutableSortDefinition def = new MutableSortDefinition();
        def.Property = null;
        Assert.That(def.Property, Is.Not.Null);
        Assert.That(def.Property, Is.Empty);
    }

    [Test]
    public void TogglesOkWhenPropertyIsSetAgain()
    {
        MutableSortDefinition def = new MutableSortDefinition(true);
        bool originalAscendingValue = def.Ascending;
        def.Property = "Name";
        def.Property = "Name"; // sort order should (must!) be reversed now...
        Assert.That(def.Ascending == originalAscendingValue, Is.False);
    }

    [Test]
    public void DoesNotTogglesWhenPropertyIsSetAgain()
    {
        MutableSortDefinition def = new MutableSortDefinition(true);
        bool originalAscendingValue = def.Ascending;
        def.Property = "Name";
        def.Property = "Age"; // sort order must not be toggled (different property)
        Assert.That(def.Ascending == originalAscendingValue, Is.True);
    }
}
