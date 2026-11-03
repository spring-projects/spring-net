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

using System.Reflection;
using NUnit.Framework;
using Spring.Util;

namespace Spring.Aop;

/// <summary>
/// Unit tests for the TrueMethodMatcher class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class TrueMethodMatcherTests
{
    [Test]
    public void Deserialization()
    {
        IMethodMatcher deserializedVersion
            = (IMethodMatcher) SerializationTestUtils.SerializeAndDeserialize(
                TrueMethodMatcher.True);
        Assert.That(Object.ReferenceEquals(TrueMethodMatcher.True, deserializedVersion),
            Is.True,
            "Singleton instance not being deserialized correctly");
    }

    [Test]
    public void IsSerializable()
    {
        Assert.That(SerializationTestUtils.IsSerializable(TrueMethodMatcher.True),
            Is.True,
            "TrueMethodMatcher must be serializable.");
    }

    [Test]
    public void AlwaysMatchesEvenOnNullArguments()
    {
        Assert.That(TrueMethodMatcher.True.Matches(null, null),
            Is.True,
            "Must always match (return true).");
    }

    [Test]
    public void AlwaysMatches()
    {
        Assert.That(TrueMethodMatcher.True.Matches(
                (MethodInfo) MethodBase.GetCurrentMethod(), GetType()),
            Is.True,
            "Must always match (return true).");
    }

    [Test]
    public void IsRuntime()
    {
        Assert.That(TrueMethodMatcher.True.IsRuntime, Is.False, "Must NOT be runtime.");
    }
}
