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

namespace Spring.Collections;

/// <summary>
/// Unit tests for the static methods of the Set class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class StaticSetTests
{
    /// <summary>
    /// The setup logic executed before the execution of each individual test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        one = new ListSet(new object[] { 1, "Foo", 2 });
        two = new ListSet(new object[] { 1, 3 });
    }

    [Test]
    public void Xor()
    {
        ISet actual = Set.ExclusiveOr(one, two);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Count, Is.EqualTo(3));
        Assert.That(actual.ContainsAll(new object[] { "Foo", 2, 3 }), Is.True);
        CheckThatOriginalsHaveNotBeenModified();

        Assert.That(Set.ExclusiveOr(null, null), Is.Null);

        actual = Set.ExclusiveOr(null, two);
        Assert.That(actual, Is.EqualTo(two));

        actual = Set.ExclusiveOr(one, null);
        Assert.That(actual, Is.EqualTo(one));
    }

    [Test]
    public void Minus()
    {
        ISet actual = Set.Minus(one, two);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Count, Is.EqualTo(2));
        Assert.That(actual.ContainsAll(new object[] { "Foo", 2 }), Is.True);
        CheckThatOriginalsHaveNotBeenModified();

        Assert.That(Set.Minus(null, two), Is.Null);
    }

    [Test]
    public void Union()
    {
        ISet actual = Set.Union(one, two);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Count, Is.EqualTo(4));
        Assert.That(actual.ContainsAll(new object[] { 1, "Foo", 2, 3 }), Is.True);
        CheckThatOriginalsHaveNotBeenModified();

        Assert.That(Set.Union(null, null), Is.Null);

        actual = Set.Union(null, two);
        Assert.That(actual, Is.EqualTo(two));

        actual = Set.Union(one, null);
        Assert.That(actual, Is.EqualTo(one));
    }

    [Test]
    public void Intersect()
    {
        ISet actual = Set.Intersect(one, two);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Count, Is.EqualTo(1));
        Assert.That(actual.Contains(1), Is.True);
        CheckThatOriginalsHaveNotBeenModified();

        Assert.That(Set.Intersect(null, null), Is.Null);

        actual = Set.Intersect(null, two);
        Assert.That(actual.Count, Is.EqualTo(0));

        actual = Set.Intersect(one, null);
        Assert.That(actual.Count, Is.EqualTo(0));
    }

    private void CheckThatOriginalsHaveNotBeenModified()
    {
        Assert.That(one, Is.Not.Null, "Set operation modified the original sets.");
        Assert.That(one.Count, Is.EqualTo(3), "Set operation modified the original sets.");
        Assert.That(one.ContainsAll(new object[] { 1, "Foo", 2 }), Is.True, "Set operation modified the original sets.");

        Assert.That(two, Is.Not.Null, "Set operation modified the original sets.");
        Assert.That(two.Count, Is.EqualTo(2), "Set operation modified the original sets.");
        Assert.That(two.ContainsAll(new object[] { 1, 3 }), Is.True, "Set operation modified the original sets.");
    }

    private ISet one;
    private ISet two;
}
