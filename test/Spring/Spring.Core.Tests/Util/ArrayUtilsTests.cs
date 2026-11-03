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

namespace Spring.Util;

/// <summary>
/// Unit tests for the ArrayUtils class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class ArrayUtilsTests
{
    [Test]
    public void AreEqual()
    {
        object[] one = new string[] { "Foo", "Bar", "Baz" };
        object[] two = new string[] { "Foo", "Bar", "Baz" };
        Assert.That(ArrayUtils.AreEqual(one, two), Is.True);
        object[] three = new string[] { "Foo", "Ben", "Baz" };
        Assert.That(ArrayUtils.AreEqual(one, three), Is.False);
    }

    [Test]
    public void AreEqualWithBadArguments()
    {
        Assert.That(ArrayUtils.AreEqual(null, null), Is.True);
        object[] one = new string[] { "Foo", "Bar", "Baz" };
        object[] two = null;
        Assert.That(ArrayUtils.AreEqual(one, two), Is.False);
        object[] three = new string[] { "Foo", "Bar" };
        Assert.That(ArrayUtils.AreEqual(one, three), Is.False);
    }

    [Test]
    public void HasLengthTests()
    {
        Assert.That(ArrayUtils.HasLength(null), Is.False);
        Assert.That(ArrayUtils.HasLength(new byte[0]), Is.False);
        Assert.That(ArrayUtils.HasLength(new byte[1]), Is.True);
    }

    [Test]
    public void ConcatsArrays()
    {
        byte[] array1 = new byte[] { 0, 1, 2, 3 };
        byte[] array2 = new byte[] { 4, 5, 6, 7 };
        byte[] result = (byte[]) ArrayUtils.Concat(array1, array2);
        Assert.That(result, Is.EqualTo(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }));
    }

    [Test]
    public void ConcatsNullArrays()
    {
        byte[] array = new byte[] { 0, 1, 2, 3 };
        Assert.That(ArrayUtils.Concat(array, null), Is.EqualTo(new byte[] { 0, 1, 2, 3 }));
        Assert.That(ArrayUtils.Concat(null, array), Is.EqualTo(new byte[] { 0, 1, 2, 3 }));
    }
}
