#region License

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

#endregion

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Spring.Collections.Generic
{
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
            Assert.IsTrue(_testDictionary.ContainsKey(0));
            Assert.IsTrue(_testDictionary.ContainsKey(1));
            Assert.IsTrue(_testDictionary.ContainsKey(2));
            Assert.IsTrue(_testDictionary.ContainsKey(3));
            Assert.IsTrue(_testDictionary.ContainsKey(4));
        }

        [Test]
        public void AddAtExistingIndexOverwritesEntry()
        {
            _testDictionary.Add(1, "b2");
            Assert.AreEqual("b2", _testDictionary[1]);
        }

        [Test]
        public void CanAccessValuesByIndex()
        {
            Assert.AreEqual("a", _testDictionary[0]);
            Assert.AreEqual("b", _testDictionary[1]);
        }

        /// <summary>
        /// Determines whether this instance [can retreive keys].
        /// </summary>
        [Test]
        public void CanRetreiveKeys()
        {
            int[] keys = _testDictionary.Keys.ToArray();
            Assert.AreEqual(5, keys.Length);
            Assert.AreEqual(0, keys[0]);
            Assert.AreEqual(1, keys[1]);
            Assert.AreEqual(2, keys[2]);
            Assert.AreEqual(3, keys[3]);
            Assert.AreEqual(4, keys[4]);
        }

        [Test]
        public void CanRemoveEntriesByIndex()
        {
            _testDictionary.Remove(3);
            _testDictionary.Remove(2);
            Assert.AreEqual(3, _testDictionary.Count);

            _testDictionary.Remove(1);
            Assert.AreEqual(2, _testDictionary.Count);

            _testDictionary.Remove(4);
            _testDictionary.Remove(0);
            Assert.AreEqual(0, _testDictionary.Count);
            Assert.AreEqual(0, _testDictionary.Keys.Count);
        }
        
    }
}
