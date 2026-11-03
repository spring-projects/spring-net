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
using Spring.Objects;

namespace Spring.Aop.Support;

/// <summary>
/// Unit tests for the TypeFilters class.
/// </summary>
/// <author>Rod Johnson</author>
/// <author>Choy Rim (.NET)</author>
[TestFixture]
public sealed class TypeFiltersTests
{
    private ITypeFilter exceptionFilter = new RootTypeFilter(typeof(Exception));
    private ITypeFilter itoFilter = new RootTypeFilter(typeof(ITestObject));
    private ITypeFilter hasRootCauseFilter = new RootTypeFilter(typeof(StackOverflowException));

    [Test]
    public void Union()
    {
        Assert.That(exceptionFilter.Matches(typeof(SystemException)), Is.True);
        Assert.That(exceptionFilter.Matches(typeof(TestObject)), Is.False);
        Assert.That(itoFilter.Matches(typeof(Exception)), Is.False);
        Assert.That(itoFilter.Matches(typeof(TestObject)), Is.True);
        ITypeFilter union = TypeFilters.Union(exceptionFilter, itoFilter);
        Assert.That(union.Matches(typeof(SystemException)), Is.True);
        Assert.That(union.Matches(typeof(TestObject)), Is.True);
    }

    [Test]
    public void Intersection()
    {
        Assert.That(exceptionFilter.Matches(typeof(SystemException)), Is.True);
        Assert.That(hasRootCauseFilter.Matches(typeof(StackOverflowException)), Is.True);

        ITypeFilter intersection = TypeFilters.Intersection(exceptionFilter, hasRootCauseFilter);
        Assert.That(intersection.Matches(typeof(SystemException)), Is.False);
        Assert.That(intersection.Matches(typeof(TestObject)), Is.False);
        Assert.That(intersection.Matches(typeof(StackOverflowException)), Is.True);
    }
}