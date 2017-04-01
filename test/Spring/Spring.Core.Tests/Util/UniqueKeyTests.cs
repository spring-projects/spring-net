#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using System;
using NUnit.Framework;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// Tests <see cref="UniqueKey"/> functionality.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class UniqueKeyTests
    {
        private class TestObject
        {}

        [Test]
        public void MustNotCallObjectSignatureWithType()
        {
            Type myType = typeof(TestObject);
            Assert.Throws<ArgumentException>(() => UniqueKey.GetInstanceScopedString( myType, "PartialKey"));
        }

        [Test]
        public void CreateTypeScopedKeyString()
        {
            string typeScopedKey = UniqueKey.GetTypeScopedString(typeof(TestObject), "PartialKey");
            string expectedKey = string.Format("{0}.{1}", typeof(TestObject).FullName, "PartialKey");
            Assert.AreEqual(expectedKey, typeScopedKey);
        }

        [Test]
        public void CreateTypeScopedKey()
        {
            UniqueKey typeScopedKey = UniqueKey.GetTypeScoped(typeof(TestObject), "PartialKey");
            UniqueKey expectedKey = UniqueKey.GetTypeScoped(typeof(TestObject), "PartialKey");

            string expectedKeyString = string.Format("{0}.{1}", typeof(TestObject).FullName, "PartialKey");

            // I know testing implementation details is not the best strategy,
            // but I want to receive an error if this fails (oakinger)
            Assert.AreEqual(expectedKeyString, expectedKey.ToString());
            Assert.AreEqual(expectedKeyString.GetHashCode(), expectedKey.GetHashCode());

            // different instances...
            Assert.AreNotSame(expectedKey, typeScopedKey);
            // but equal
            Assert.AreEqual(expectedKey, typeScopedKey);
            Assert.AreEqual(expectedKey.GetHashCode(), typeScopedKey.GetHashCode());
        }

        [Test]
        public void CreateInstanceScopedKeyString()
        {
            TestObject testObject = new TestObject();
            string typeScopedKey = UniqueKey.GetInstanceScopedString(testObject, "PartialKey");
            string expectedKey = string.Format("{0}[{1}].{2}", typeof(TestObject).FullName, testObject.GetHashCode(), "PartialKey");
            Assert.AreEqual(expectedKey, typeScopedKey);
        }

        [Test]
        public void CreateInstanceScopedKey()
        {
            TestObject testObject = new TestObject();
            UniqueKey instanceScopedKey = UniqueKey.GetInstanceScoped(testObject, "PartialKey");
            UniqueKey expectedKey = UniqueKey.GetInstanceScoped(testObject, "PartialKey");

            string expectedKeyString = string.Format("{0}[{1}].{2}", typeof(TestObject).FullName, testObject.GetHashCode(), "PartialKey");

            // I know testing implementation details is not the best strategy,
            // but I want to receive an error if this fails (oakinger)
            Assert.AreEqual(expectedKeyString, expectedKey.ToString());
            Assert.AreEqual(expectedKeyString.GetHashCode(), expectedKey.GetHashCode());

            // different instances...
            Assert.AreNotSame(expectedKey, instanceScopedKey);
            // but equal
            Assert.AreEqual(expectedKey, instanceScopedKey);
            Assert.AreEqual(expectedKey.GetHashCode(), instanceScopedKey.GetHashCode());
        }
    }
}