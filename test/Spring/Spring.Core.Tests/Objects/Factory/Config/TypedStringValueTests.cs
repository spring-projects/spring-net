/*
 * Copyright 2002-2010 the original author or authors.
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
using Spring.Util;

namespace Spring.Objects.Factory.Config;

/// <summary>
/// Unit tests for the TypedStringValue class.
/// </summary>
/// <author>Juergen Hoeller</author>
/// <author>Rick Evans (.NET)</author>
[TestFixture]
public sealed class TypedStringValueTests
{
    [Test]
    public void Instantiation()
    {
        string expectedNow = DateTime.Now.ToShortDateString();

        TypedStringValue tsv = new TypedStringValue(expectedNow, typeof(DateTime));
        Assert.That(tsv.Value, Is.EqualTo(expectedNow));
        Assert.That(tsv.TargetType, Is.EqualTo(typeof(DateTime)));

        tsv = new TypedStringValue(expectedNow);
        Assert.That(tsv.Value, Is.EqualTo(expectedNow));

        tsv = new TypedStringValue(expectedNow, typeof(DateTime).FullName);
        Assert.That(tsv.Value, Is.EqualTo(expectedNow));
        Assert.That(tsv.TargetTypeName, Is.EqualTo(typeof(DateTime).FullName));
    }

    [Test]
    public void InstantiationWithNullType()
    {
        Assert.Throws<ArgumentNullException>(() => new TypedStringValue(string.Empty, (Type) null));
    }

    [Test]
    public void InstantiationWithNullTypeName()
    {
        Assert.Throws<ArgumentNullException>(() => new TypedStringValue(string.Empty, (string) null));
    }

    [Test]
    public void InstantiationWithEmptyTypeName()
    {
        Assert.Throws<ArgumentNullException>(() => new TypedStringValue(string.Empty, "  "));
    }

    [Test]
    public void SetTargetTypePropertyToNullType()
    {
        TypedStringValue tsv = new TypedStringValue(string.Empty, typeof(DateTime));
        Assert.Throws<ArgumentNullException>(() => tsv.TargetType = null);
    }

    [Test]
    public void SetTargetTypeNamePropertyToEmptyString()
    {
        TypedStringValue tsv = new TypedStringValue(string.Empty, typeof(DateTime));
        Assert.Throws<ArgumentNullException>(() => tsv.TargetTypeName = "  ");
    }

    [Test]
    public void IsSerializable()
    {
        Assert.That(SerializationTestUtils.IsSerializable(new TypedStringValue()),
            Is.True,
            "Must be marked as [Serializable].");
    }

    [Test]
    public void Serialization()
    {
        TypedStringValue value = new TypedStringValue();
        Type expectedType = typeof(string);
        value.TargetType = expectedType;
        const string expectedValue = "rilo-kiley";
        value.Value = expectedValue;

        object foo = SerializationTestUtils.SerializeAndDeserialize(value);
        Assert.That(foo, Is.Not.Null, "Serialization roundtrip must never result in null.");
        TypedStringValue deser = foo as TypedStringValue;
        Assert.That(deser,
            Is.Not.Null,
            "Serialization roundtrip yielded the wrong Type of object.");
        Assert.That(deser.TargetType, Is.EqualTo(expectedType),
            "Serialization roundtrip yielded the wrong TargetType.");
        Assert.That(deser.Value, Is.EqualTo(expectedValue),
            "Serialization roundtrip yielded the wrong Value.");
    }

    [Test]
    public void HasTargetType()
    {
        TypedStringValue tsv = new TypedStringValue(string.Empty, typeof(DateTime));
        Assert.That(tsv.HasTargetType, Is.True);
    }

    [Test]
    public void HasTargetTypeReturnsFalseWhenTargetTypeNotResolved()
    {
        TypedStringValue tsv = new TypedStringValue(string.Empty, typeof(DateTime).FullName);
        Assert.That(tsv.HasTargetType, Is.False);

        tsv = new TypedStringValue(string.Empty, typeof(DateTime));
        Assert.That(tsv.HasTargetType, Is.True);
        tsv.TargetTypeName = typeof(DateTime).FullName;
        Assert.That(tsv.HasTargetType, Is.False);
    }

    [Test]
    public void ResolveTargetType()
    {
        TypedStringValue tsv = new TypedStringValue(string.Empty, typeof(DateTime).FullName);
        Assert.That(tsv.HasTargetType, Is.False);
        Assert.That(tsv.ResolveTargetType(), Is.EqualTo(typeof(DateTime)));
        Assert.That(tsv.HasTargetType, Is.True);
    }
}
