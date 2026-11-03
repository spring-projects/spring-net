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

namespace Spring.Core;

/// <summary>
/// Unit tests for the MethodGenericArgumentsCountCriteria class.
/// </summary>
/// <author>Bruno Baia</author>
[TestFixture]
public sealed class MethodGenericArgumentsCountCriteriaTests
{
    [Test]
    public void Instantiation()
    {
        MethodGenericArgumentsCountCriteria criteria = new MethodGenericArgumentsCountCriteria();
        Assert.That(criteria.ExpectedGenericArgumentCount, Is.EqualTo(0));
        criteria = new MethodGenericArgumentsCountCriteria(10);
        Assert.That(criteria.ExpectedGenericArgumentCount, Is.EqualTo(10));
    }

    [Test]
    public void InstantiationBailsWithGenericArgumentCountSetToLessThanZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MethodGenericArgumentsCountCriteria(-10));
    }

    [Test]
    public void BailsWhenExpectedGenericArgumentCountSetToLessThanZero()
    {
        MethodGenericArgumentsCountCriteria criteria = new MethodGenericArgumentsCountCriteria();
        Assert.Throws<ArgumentOutOfRangeException>(() => criteria.ExpectedGenericArgumentCount = -12);
    }

    [Test]
    public void IsSatisfied()
    {
        MethodGenericArgumentsCountCriteria criteria = new MethodGenericArgumentsCountCriteria();
        MethodInfo method = GetType().GetMethod("NoGenericArgument", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(criteria.IsSatisfied(method), Is.True);

        criteria = new MethodGenericArgumentsCountCriteria(1);
        method = GetType().GetMethod("OneGenericArgument", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(criteria.IsSatisfied(method), Is.True);

        criteria = new MethodGenericArgumentsCountCriteria(2);
        method = GetType().GetMethod("TwoGenericArguments", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(criteria.IsSatisfied(method), Is.True);
    }

    [Test]
    public void IsNotSatisfiedWithNull()
    {
        MethodGenericArgumentsCountCriteria criteria = new MethodGenericArgumentsCountCriteria();
        Assert.That(criteria.IsSatisfied(null), Is.False);
    }

    // some methods for testing signatures...
    public void NoGenericArgument()
    {
    }

    public void OneGenericArgument<T>()
    {
    }

    public void TwoGenericArguments<T, U>()
    {
    }
}