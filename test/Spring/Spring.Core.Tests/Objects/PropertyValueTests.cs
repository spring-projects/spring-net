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

namespace Spring.Objects;

/// <summary>
/// Unit tests for the PropertyValue class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class PropertyValueTests
{
    [Test]
    public void InstantiationWithNulls()
    {
        Assert.Throws<ArgumentNullException>(() => new PropertyValue(null, null));
    }

    [Test]
    public void Instantiation()
    {
        PropertyValue val = new PropertyValue("Age", 12);
        Assert.That(val.Name, Is.EqualTo("Age"));
        Assert.That(val.Value, Is.EqualTo(12));
    }

    [Test]
    public void InstantiationWithEmptyStringPropertyName()
    {
        Assert.Throws<ArgumentNullException>(() => new PropertyValue(string.Empty, 12));
    }

    [Test]
    public void InstantiationWithWhitespacePropertyName()
    {
        Assert.Throws<ArgumentNullException>(() => new PropertyValue("  ", 12));
    }

    [Test]
    public void TestEquals()
    {
        PropertyValue one = new PropertyValue("Age", 12);
        PropertyValue two = new PropertyValue("Age", 12);
        Assert.That(two, Is.EqualTo(one));
        Assert.That(two.GetHashCode(), Is.EqualTo(one.GetHashCode()));
        Assert.That(one.Equals(one), Is.True);
        Assert.That(one.Equals(two), Is.True);
        Assert.That(two.Equals(one), Is.True);
        Assert.That(one.Equals(new PropertyValue("Age", 13)), Is.False);
        Assert.That(one.Equals(new PropertyValue("Height", 12)), Is.False);
        Assert.That(one.Equals("Foo"), Is.False);
    }

    [Test]
    public void TestToString()
    {
        PropertyValue one = new PropertyValue("Age", 12);
        Assert.That(one.ToString(), Is.EqualTo("PropertyValue: name='Age'; value=[12]."));
    }
}
