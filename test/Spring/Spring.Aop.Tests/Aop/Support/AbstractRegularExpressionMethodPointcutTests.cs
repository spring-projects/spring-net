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

using System.Reflection;
using Spring.Util;
using NUnit.Framework;

namespace Spring.Aop.Support;

/// <summary>
/// Unit tests for the AbstractRegularExpressionMethodPointcut class.
/// </summary>
/// <author>Rod Johnson</author>
/// <author>Dmitriy Kopylenko</author>
/// <author>Simon White (.NET)</author>
public abstract class AbstractRegularExpressionMethodPointcutTests
{
    private AbstractRegularExpressionMethodPointcut pointcut;

    [SetUp]
    protected void SetUp()
    {
        pointcut = GetRegexpMethodPointcut();
    }

    protected abstract AbstractRegularExpressionMethodPointcut GetRegexpMethodPointcut();

    [Test]
    public void NoPatternSupplied()
    {
        NoPatternSuppliedTests(pointcut);
    }

    [Test]
    public void SerializationWithNoPatternSupplied()
    {
        pointcut = (AbstractRegularExpressionMethodPointcut) SerializationTestUtils.SerializeAndDeserialize(pointcut);
        NoPatternSuppliedTests(pointcut);
    }

    protected void NoPatternSuppliedTests(AbstractRegularExpressionMethodPointcut rpc)
    {
        Assert.That(rpc.Matches(typeof(object).GetMethod("GetHashCode"), typeof(int)), Is.False);
        Assert.That(rpc.Matches(typeof(object).GetMethod("GetType"), typeof(Type)), Is.False);
        Assert.That(rpc.Patterns.Length, Is.EqualTo(0));
    }

    [Test]
    public void ExactMatch()
    {
        pointcut.Pattern = "System.Object.GetHashCode";
        ExactMatchTests(pointcut);
        pointcut = (AbstractRegularExpressionMethodPointcut) SerializationTestUtils.SerializeAndDeserialize(pointcut);
        ExactMatchTests(pointcut);
    }

    protected void ExactMatchWithGenericTypeTests(AbstractRegularExpressionMethodPointcut rpc)
    {
        // assumes rpc.setPattern("System.Collections.Generic.List<string>");
        Assert.That(rpc.Matches(typeof(System.Collections.Generic.List<string>).GetMethod("Add"), typeof(int)), Is.True);
        Assert.That(rpc.Matches(typeof(System.Collections.Generic.List<string>).GetMethod("GetType"), typeof(Type)), Is.False);
    }

    [Test]
    public void ExactMatchWithGenericType()
    {
        pointcut.Pattern = "System.Collections.Generic.List<string>.Add";
        ExactMatchWithGenericTypeTests(pointcut);
        pointcut = (AbstractRegularExpressionMethodPointcut) SerializationTestUtils.SerializeAndDeserialize(pointcut);
        ExactMatchWithGenericTypeTests(pointcut);
    }

    [Test]
    public void WildcardWithGenericType()
    {
        pointcut.Pattern = ".*List<string>.Add";
        Assert.That(pointcut.Matches(typeof(System.Collections.Generic.List<string>).GetMethod("Add"), typeof(int)), Is.True);
        Assert.That(pointcut.Matches(typeof(System.Collections.Generic.List<string>).GetMethod("GetType"), typeof(Type)), Is.False);
    }

    [Test]
    public void WildcardForOneClassWithGenericType()
    {
        pointcut.Pattern = "System.Collections.*";
        Assert.That(pointcut.Matches(typeof(System.Collections.Generic.List<string>).GetMethod("Add"), typeof(int)), Is.True);
        Assert.That(pointcut.Matches(typeof(System.Collections.Generic.List<string>).GetMethod("GetType"), typeof(Type)), Is.False);
    }

    protected void ExactMatchTests(AbstractRegularExpressionMethodPointcut rpc)
    {
        // assumes rpc.setPattern("java.lang.Object.hashCode");
        Assert.That(rpc.Matches(typeof(object).GetMethod("GetHashCode"), typeof(int)), Is.True);
        Assert.That(rpc.Matches(typeof(object).GetMethod("GetType"), typeof(Type)), Is.False);
    }

    [Test]
    public void Wildcard()
    {
        pointcut.Pattern = ".*Object.GetHashCode";
        Assert.That(pointcut.Matches(typeof(object).GetMethod("GetHashCode"), typeof(int)), Is.True);
        Assert.That(pointcut.Matches(typeof(object).GetMethod("GetType"), typeof(Type)), Is.False);
    }

    [Test]
    public void WildcardForOneClass()
    {
        pointcut.Pattern = "System.Object.*";
        Assert.That(pointcut.Matches(typeof(object).GetMethod("GetHashCode"), typeof(int)), Is.True);
        Assert.That(pointcut.Matches(typeof(object).GetMethod("GetType"), typeof(Type)), Is.True);
    }

    [Test]
    public void MatchesObjectClass()
    {
        pointcut.Pattern = "Object.*";
        Assert.That(pointcut.Matches(typeof(Exception).GetMethod("GetHashCode"), typeof(TargetException)), Is.True);
        // Doesn't match
        Assert.That(pointcut.Matches(typeof(Exception).GetMethod("ToString"), typeof(Exception)), Is.False);
    }
}
