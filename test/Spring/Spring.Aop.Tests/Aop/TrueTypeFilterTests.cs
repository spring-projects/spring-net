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
using Spring.Util;

namespace Spring.Aop;

/// <summary>
/// Unit tests for the TrueTypeFilter class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class TrueTypeFilterTests
{
    [Test]
    public void Deserialization()
    {
        ITypeFilter deserializedVersion
            = (ITypeFilter) SerializationTestUtils.SerializeAndDeserialize(
                TrueTypeFilter.True);
        Assert.That(Object.ReferenceEquals(TrueTypeFilter.True, deserializedVersion),
            Is.True,
            "Singleton instance not being deserialized correctly");
    }

    [Test]
    public void IsSerializable()
    {
        Assert.That(SerializationTestUtils.IsSerializable(TrueTypeFilter.True),
            Is.True,
            "TrueClassFilter must be serializable.");
    }

    [Test]
    public void AlwaysMatchesEvenOnNullArgument()
    {
        Assert.That(TrueTypeFilter.True.Matches(null),
            Is.True,
            "Must always match (return true).");
    }

    [Test]
    public void AlwaysMatches()
    {
        Assert.That(TrueTypeFilter.True.Matches(GetType()),
            Is.True,
            "Must always match (return true).");
    }

    [Test]
    public void ToStringAlwaysTrue()
    {
        Assert.That(TrueTypeFilter.True.ToString(), Is.EqualTo("TrueTypeFilter.True"));
    }
}
