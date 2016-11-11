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

namespace Spring.Objects
{
    /// <summary>
    /// Unit tests for the PropertyValue class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class PropertyValueTests
    {
        [Test]
        public void InstantiationWithNulls()
        {
            Assert.Throws<ArgumentNullException>(() => new PropertyValue(null, null));
        }

        [Test]
        public void Instantiation()
        {
            PropertyValue val = new PropertyValue("Age", 12);
            Assert.AreEqual("Age", val.Name);
            Assert.AreEqual(12, val.Value);
        }

        [Test]
        public void InstantiationWithEmptyStringPropertyName()
        {
            Assert.Throws<ArgumentNullException>(() => new PropertyValue(string.Empty, 12));
        }

        [Test]
        public void InstantiationWithWhitespacePropertyName()
        {
            Assert.Throws<ArgumentNullException>(() => new PropertyValue("  ", 12));
        }

        [Test]
        public void TestEquals()
        {
            PropertyValue one = new PropertyValue("Age", 12);
            PropertyValue two = new PropertyValue("Age", 12);
            Assert.AreEqual(one, two);
            Assert.AreEqual(one.GetHashCode(), two.GetHashCode());
            Assert.AreEqual(one, one);
            Assert.IsFalse(one.Equals("Foo"));
        }

        [Test]
        public void TestToString()
        {
            PropertyValue one = new PropertyValue("Age", 12);
            Assert.AreEqual("PropertyValue: name='Age'; value=[12].", one.ToString());
        }
    }
}