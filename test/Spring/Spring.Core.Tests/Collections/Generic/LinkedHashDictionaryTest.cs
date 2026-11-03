/*
 * Copyright © 2015-2015 the original author or authors.
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

namespace Spring.Collections.Generic;

/// <summary>
/// This class contains tests for LinkedHashDictionary
/// </summary>
/// <author>Zbynek Vyskovsky, kvr@centrum.cz</author>
[TestFixture]
public class LinkedHashDictionaryTest
{
    private IDictionary<int, string> _testDictionary;

    [SetUp]
    public void Setup()
    {
        _testDictionary = new LinkedHashDictionary<int, string>();
        _testDictionary.Add(0, "a");
        _testDictionary.Add(1, "b");
        _testDictionary.Add(2, "c");
        _testDictionary.Add(3, "d");
        _testDictionary.Add(4, "e");
    }

    [Test]
    public void CanAddEntries()
    {
        Assert.That(_testDictionary.ContainsKey(0), Is.True);
        Assert.That(_testDictionary.ContainsKey(1), Is.True);
        Assert.That(_testDictionary.ContainsKey(2), Is.True);
        Assert.That(_testDictionary.ContainsKey(3), Is.True);
        Assert.That(_testDictionary.ContainsKey(4), Is.True);
    }

    [Test]
    public void AddAtExistingIndexOverwritesEntry()
    {
        _testDictionary.Add(1, "b2");
        Assert.That(_testDictionary[1], Is.EqualTo("b2"));
    }

    [Test]
    public void CanAccessValuesByIndex()
    {
        Assert.That(_testDictionary[0], Is.EqualTo("a"));
        Assert.That(_testDictionary[1], Is.EqualTo("b"));
    }

    /// <summary>
    /// Determines whether this instance [can retreive keys].
    /// </summary>
    [Test]
    public void CanRetreiveKeys()
    {
        int[] keys = _testDictionary.Keys.ToArray();
        Assert.That(keys.Length, Is.EqualTo(5));
        Assert.That(keys[0], Is.EqualTo(0));
        Assert.That(keys[1], Is.EqualTo(1));
        Assert.That(keys[2], Is.EqualTo(2));
        Assert.That(keys[3], Is.EqualTo(3));
        Assert.That(keys[4], Is.EqualTo(4));
    }

    [Test]
    public void CanRemoveEntriesByIndex()
    {
        _testDictionary.Remove(3);
        _testDictionary.Remove(2);
        Assert.That(_testDictionary.Count, Is.EqualTo(3));

        _testDictionary.Remove(1);
        Assert.That(_testDictionary.Count, Is.EqualTo(2));

        _testDictionary.Remove(4);
        _testDictionary.Remove(0);
        Assert.That(_testDictionary.Count, Is.EqualTo(0));
        Assert.That(_testDictionary.Keys.Count, Is.EqualTo(0));
    }
}
