#region License

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

#endregion

#region Imports

using System.Reflection;
using NUnit.Framework;
using Spring.Objects;

#endregion

namespace Spring.Core;

/// <summary>
/// Unit tests for the MethodArgumentsCriteria class.
/// </summary>
/// <author>Rick Evans</author>
/// <author>Bruno Baia</author>
[TestFixture]
public sealed class MethodArgumentsCriteriaTests
{
    [Test]
    public void IsNotSatisfiedWithNull()
    {
        MethodArgumentsCriteria criteria = new MethodArgumentsCriteria();
        Assert.IsFalse(criteria.IsSatisfied(null), "Was satisified with null.");
    }

    [Test]
    public void IsSatisfiedWithNoParametersByDefault()
    {
        MethodArgumentsCriteria criteria = new MethodArgumentsCriteria();
        MethodInfo method = GetType().GetMethod("Foo");
        Assert.IsTrue(criteria.IsSatisfied(method), "Was not satisified with a method that takes no parameters by default.");
    }

    [Test]
    public void IsSatisfied()
    {
        MethodArgumentsCriteria criteria = new MethodArgumentsCriteria(
            new Object[] { "Bruno", DateTime.Now, new TestObject("Bruno", 29) });
        MethodInfo method = GetType().GetMethod("BoJangles");
        Assert.IsTrue(criteria.IsSatisfied(method), "Was not satisified with a method that takes a whole buncha parameters.");

        method = GetType().GetMethod("BadBobbyBoJangles");
        Assert.IsFalse(criteria.IsSatisfied(method), "Was satisified with a (bad) method that takes a whole buncha parameters.");
    }

    [Test]
    public void IsSatisfiedIsPolymorphic()
    {
        // i.e. derived types satisfy the criteria if a base type or interface is
        // specified as one of the parameter types
        MethodArgumentsCriteria criteria
            = new MethodArgumentsCriteria(new Object[] { new TestObject("Bruno", 29) });
        MethodInfo method = GetType().GetMethod("Diddly");
        Assert.IsTrue(criteria.IsSatisfied(method), "Was not satisified with a method that takes a base class as a parameter.");
    }

    [Test]
    public void IsSatisfiedWithParamsArguments()
    {
        MethodArgumentsCriteria criteria = new MethodArgumentsCriteria(new Object[] { 29, "Bruno" });
        MethodInfo method = GetType().GetMethod("ParamsArguments");
        Assert.IsTrue(criteria.IsSatisfied(method), "Was not satisified with a method that takes a parameter array ('params') as a parameter.");

        criteria = new MethodArgumentsCriteria(new Object[] { 29, new string[] { "Bruno", "Rick" } });
        method = GetType().GetMethod("ParamsArguments");
        Assert.IsTrue(criteria.IsSatisfied(method), "Was not satisified with a method that takes a parameter array ('params') as a parameter.");

        criteria = new MethodArgumentsCriteria(new Object[] { 29 });
        method = GetType().GetMethod("ParamsArguments");
        Assert.IsTrue(criteria.IsSatisfied(method), "Was not satisified with a method that takes a parameter array ('params') as a parameter.");

        criteria = new MethodArgumentsCriteria(new Object[] { 29, "Bruno", "Rick", "James" });
        method = GetType().GetMethod("ParamsArguments");
        Assert.IsTrue(criteria.IsSatisfied(method), "Was not satisified with a method that takes a parameter array ('params') as a parameter.");

        criteria = new MethodArgumentsCriteria(new Object[] { 29, new string[] { "Bruno", "Rick" }, "James" });
        method = GetType().GetMethod("ParamsArguments");
        Assert.IsFalse(criteria.IsSatisfied(method), "Was not satisified with a method that takes a parameter array ('params') as a parameter.");
    }

    // some methods for testing signatures...
    public void Foo()
    {
    }

    public DateTime BoJangles(string one, DateTime two, TestObject three)
    {
        return DateTime.Now;
    }

    public void BadBobbyBoJangles(string one, DateTime two, string bing)
    {
    }

    public void Diddly(ITestObject three)
    {
    }

    public void ParamsArguments(int foo, params string[] strs)
    {
    }
}
