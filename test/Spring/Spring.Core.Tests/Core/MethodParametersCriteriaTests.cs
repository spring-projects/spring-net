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

namespace Spring.Core;

/// <summary>
/// Unit tests for the MethodParametersCriteria class.
/// </summary>
/// <author>Rick Evans</author>
/// <author>Bruno Baia</author>
[TestFixture]
public sealed class MethodParametersCriteriaTests
{
    [Test]
    public void IsNotSatisfiedWithNull()
    {
        MethodParametersCriteria criteria = new MethodParametersCriteria();
        Assert.That(criteria.IsSatisfied(null), Is.False, "Was satisified with null.");
    }

    [Test]
    public void IsSatisfiedWithNoParametersByDefault()
    {
        MethodParametersCriteria criteria = new MethodParametersCriteria();
        MethodInfo method = GetType().GetMethod("Foo");
        Assert.That(criteria.IsSatisfied(method), Is.True, "Was not satisified with a method that takes no parameters by default.");
    }

    [Test]
    public void IsNotSatisfiedWhenOnlyFinalParamMatches()
    {
        MethodParametersCriteria criteria = new MethodParametersCriteria(
            new Type[] { typeof(object), typeof(object), typeof(TestObject) });
        MethodInfo method = GetType().GetMethod("BoJangles");
        Assert.That(criteria.IsSatisfied(method), Is.False, "Was satisified with a method that only matches on the final parameter.");
    }

    [Test]
    public void IsSatisfied()
    {
        MethodParametersCriteria criteria = new MethodParametersCriteria(
            new Type[] { typeof(string), typeof(DBNull), typeof(TestObject) });
        MethodInfo method = GetType().GetMethod("BoJangles");
        Assert.That(criteria.IsSatisfied(method), Is.True, "Was not satisified with a method that takes a whole buncha parameters.");

        method = GetType().GetMethod("BadBobbyBoJangles");
        Assert.That(criteria.IsSatisfied(method), Is.False, "Was satisified with a (bad) method that takes a whole buncha parameters.");
    }

    [Test]
    public void IsNotSatisfiedIsPolymorphic()
    {
        // i.e. derived types satisfy the criteria if a base type or interface is
        // specified as one of the parameter types
        MethodParametersCriteria criteria
            = new MethodParametersCriteria(new Type[] { typeof(TestObject) });
        MethodInfo method = GetType().GetMethod("Diddly");
        Assert.That(criteria.IsSatisfied(method), Is.False, "Was not satisified with a method that takes a base class as a parameter.");
    }

    [Test]
    public void IsSatisfiedWithParamsParameters()
    {
        MethodParametersCriteria criteria = new MethodParametersCriteria(new Type[] { typeof(int), typeof(string[]) });
        MethodInfo method = GetType().GetMethod("ParamsParameters");
        Assert.That(criteria.IsSatisfied(method), Is.True, "Was not satisified with a method that takes a parameter array ('params') as a parameter.");
    }

    // some methods for testing signatures...
    public void Foo()
    {
    }

    public DateTime BoJangles(string one, DBNull two, TestObject three)
    {
        return DateTime.Now;
    }

    public void BadBobbyBoJangles(string one, DBNull two, string bing)
    {
    }

    public void Diddly(ITestObject three)
    {
    }

    public void ParamsParameters(int foo, params string[] strs)
    {
    }
}
