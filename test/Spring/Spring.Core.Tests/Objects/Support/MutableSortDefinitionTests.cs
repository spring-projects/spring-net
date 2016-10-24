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

using NUnit.Framework;

#endregion

namespace Spring.Objects.Support
{
    /// <summary>
    /// Unit tests for the MutableSortDefinition class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class MutableSortDefinitionTests
    {
        [Test]
        public void Instantiation () {
            MutableSortDefinition def = new MutableSortDefinition ();
            Assert.IsTrue (def.IgnoreCase);
            Assert.IsTrue (def.Ascending);
            Assert.IsNotNull (def.Property);
        }

        [Test]
        public void InstantiationWithCopy () {
            ISortDefinition copy = new MutableSortDefinition ("Bing!", false, false);
            MutableSortDefinition def = new MutableSortDefinition (copy);
            Assert.IsFalse (def.IgnoreCase);
            Assert.IsFalse (def.Ascending);
            Assert.IsNotNull (def.Property);
            Assert.AreEqual (copy.Property, def.Property);
        }

        [Test]
        public void TestEquals () {
            ISortDefinition copy = new MutableSortDefinition ("Rab", false, false);
            MutableSortDefinition def = new MutableSortDefinition ("Bing!", false, false);
            def.Property = "Rab";
            Assert.AreEqual (copy, def);
            Assert.AreEqual (copy.GetHashCode (), def.GetHashCode ());
            Assert.IsFalse (def.Equals (null));
        }

        [Test]
        public void PropertySetter () {
            MutableSortDefinition def = new MutableSortDefinition ();
            def.Property = null;
            Assert.IsNotNull (def.Property);
            Assert.AreEqual (string.Empty, def.Property);
        }

        [Test]
        public void TogglesOkWhenPropertyIsSetAgain () 
        {
            MutableSortDefinition def = new MutableSortDefinition (true);
            bool originalAscendingValue = def.Ascending;
            def.Property = "Name";
            def.Property = "Name"; // sort order should (must!) be reversed now...
            Assert.IsFalse (def.Ascending == originalAscendingValue);
        }

        [Test]
        public void DoesNotTogglesWhenPropertyIsSetAgain () 
        {
            MutableSortDefinition def = new MutableSortDefinition (true);
            bool originalAscendingValue = def.Ascending;
            def.Property = "Name";
            def.Property = "Age"; // sort order must not be toggled (different property)
            Assert.IsTrue (def.Ascending == originalAscendingValue);
        }
    }
}
