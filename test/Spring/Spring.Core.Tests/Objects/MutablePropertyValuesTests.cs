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

using System.Collections.Generic;

using NUnit.Framework;

#endregion

namespace Spring.Objects
{
	/// <summary>
	/// Unit tests for the MutablePropertyValues class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class MutablePropertyValuesTests
    {
        #region SetUp
        /// <summary>
        /// The setup logic executed before the execution of this test fixture.
        /// </summary>
        [OneTimeSetUp]
        public void FixtureSetUp () {}

        /// <summary>
        /// The setup logic executed before the execution of each individual test.
        /// </summary>
        [SetUp]
        public void SetUp () {}
        #endregion

        #region TearDown
        /// <summary>
        /// The tear down logic executed after the execution of each individual test.
        /// </summary>
        [TearDown]
        public void TearDown () {}

        /// <summary>
        /// The tear down logic executed after the entire test fixture has executed.
        /// </summary>
        [OneTimeTearDown]
        public void FixtureTearDown () {}
        #endregion

        [Test]
        public void Instantiation () {
            MutablePropertyValues root = new MutablePropertyValues ();
            root.Add (new PropertyValue ("Name", "Fiona Apple"));
            root.Add (new PropertyValue ("Age", 24));
            MutablePropertyValues props = new MutablePropertyValues (root);
            Assert.AreEqual (2, props.PropertyValues.Count);
        }

        [Test]
        public void InstantiationWithNulls () 
        {
            MutablePropertyValues props = new MutablePropertyValues((Dictionary<string, object>) null);
            Assert.AreEqual (0, props.PropertyValues.Count);
            MutablePropertyValues props2 = new MutablePropertyValues ((IPropertyValues) null);
            Assert.AreEqual (0, props2.PropertyValues.Count);
        }

        [Test]
        public void AddAllInList () 
        {
            MutablePropertyValues props = new MutablePropertyValues ();
            props.AddAll (new PropertyValue [] {
                new PropertyValue ("Name", "Fiona Apple"),
                new PropertyValue ("Age", 24)});
            Assert.AreEqual (2, props.PropertyValues.Count);
        }

        [Test]
        public void AddAllInNullList () 
        {
            MutablePropertyValues props = new MutablePropertyValues ();
            props.Add (new PropertyValue ("Name", "Fiona Apple"));
            props.Add (new PropertyValue ("Age", 24));
            props.AddAll((List<PropertyValue>) null);
            Assert.AreEqual (2, props.PropertyValues.Count);
        }

        [Test]
        public void RemoveByName () 
        {
            MutablePropertyValues props = new MutablePropertyValues ();
            props.Add (new PropertyValue ("Name", "Fiona Apple"));
            props.Add (new PropertyValue ("Age", 24));
            Assert.AreEqual (2, props.PropertyValues.Count);
            props.Remove ("name");
            Assert.AreEqual (1, props.PropertyValues.Count);
        }

        [Test]
        public void RemoveByPropertyValue () 
        {
            MutablePropertyValues props = new MutablePropertyValues ();
            PropertyValue propName = new PropertyValue ("Name", "Fiona Apple");
            props.Add (propName);
            props.Add (new PropertyValue ("Age", 24));
            Assert.AreEqual (2, props.PropertyValues.Count);
            props.Remove (propName);
            Assert.AreEqual (1, props.PropertyValues.Count);
        }

        [Test]
        public void Contains () 
        {
            MutablePropertyValues props = new MutablePropertyValues ();
            props.Add (new PropertyValue ("Name", "Fiona Apple"));
            props.Add (new PropertyValue ("Age", 24));
            // must be case insensitive to be CLS compliant...
            Assert.IsTrue (props.Contains ("nAmE"));
        }

        [Test]
        public void AddAllInMap () 
        {
            MutablePropertyValues props = new MutablePropertyValues ();
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add("Name", "Fiona Apple");
            map.Add ("Age", 24);
            props.AddAll (map);
            Assert.AreEqual (2, props.PropertyValues.Count);
        }

        [Test]
        public void AddAllInNullMap () 
        {
            MutablePropertyValues props = new MutablePropertyValues ();
            props.Add (new PropertyValue ("Name", "Fiona Apple"));
            props.AddAll ((IDictionary<string, object>) null);
            Assert.AreEqual (1, props.PropertyValues.Count);
        }

        [Test]
        public void ChangesSince () 
        {
            Dictionary<string, object> map = new Dictionary<string, object>();
            PropertyValue propName = new PropertyValue("Name", "Fiona Apple");
            map.Add (propName.Name, propName.Value);
            map.Add ("Age", 24);
            MutablePropertyValues props = new MutablePropertyValues (map);
            MutablePropertyValues newProps = new MutablePropertyValues (map);

            // change the name... this is the change we'll be looking for
            newProps.SetPropertyValueAt (new PropertyValue (propName.Name, "Naomi Woolf"), 0);
            IPropertyValues changes = newProps.ChangesSince (props);
            Assert.AreEqual (1, changes.PropertyValues.Count);
            // the name was changed, so its the name property that should be in the changed list
            Assert.IsTrue (changes.Contains ("name"));

            newProps.Add (new PropertyValue ("Commentator", "Naomi Woolf"));
            changes = newProps.ChangesSince (props);
            Assert.AreEqual (2, changes.PropertyValues.Count);
            // the Commentator was added, so its the Commentator property that should be in the changed list
            Assert.IsTrue (changes.Contains ("commentator"));
            // the name was changed, so its the name property that should be in the changed list
            Assert.IsTrue (changes.Contains ("name"));
        }

        [Test]
        public void ChangesSinceWithSelf () 
        {
            Dictionary<string, object> map = new Dictionary<string, object>();
            map.Add("Name", "Fiona Apple");
            map.Add ("Age", 24);
            MutablePropertyValues props = new MutablePropertyValues (map);
            props.Remove ("name");
            // get all of the changes between self and self again (there should be none);
            IPropertyValues changes = props.ChangesSince (props);
            Assert.AreEqual (0, changes.PropertyValues.Count);
        }
	}
}
