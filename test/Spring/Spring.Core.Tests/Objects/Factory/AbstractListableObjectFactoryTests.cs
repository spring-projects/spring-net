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

using System;

using NUnit.Framework;

#endregion

namespace Spring.Objects.Factory {

	/// <summary>
	/// Unit tests for the AbstractListableObjectFactory class.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Rick Evans (.NET)</author>
    public abstract class AbstractListableObjectFactoryTests :
        AbstractObjectFactoryTests {

        /// <summary>
        /// Subclasses must initialize this (via the derived ObjectFactory property).
        /// </summary>
        protected internal virtual IListableObjectFactory ListableObjectFactory
        {
            get
            {
                if (!(ObjectFactory is IListableObjectFactory))
                {
                    throw new SystemException ("IListableObjectFactory required...");
                }
                return (IListableObjectFactory) ObjectFactory;
            }			
        }

        /// <summary>
        /// Subclasses can override this.
        /// </summary>
        [Test]
        public virtual void Count ()
        {
            AssertCount (13);
        }
		
        protected internal void AssertCount (int count)
        {
            string [] defnames = ListableObjectFactory.GetObjectDefinitionNames ();
            Assert.IsTrue (
                defnames.Length == count,
                string.Format ("We should have {0} objects, not {1}.", count, defnames.Length));
        }
		
        [Test]
        public virtual void ObjectCount ()
        {
            AssertTestObjectCount (9);
        }
		
        public virtual void AssertTestObjectCount (int count)
        {
            string [] defnames =
                ListableObjectFactory.GetObjectNamesForType (typeof (TestObject));
            Assert.IsTrue (
                defnames.Length == count,
                string.Format ("We should have {0} objects for class {1}, not {2}.", count, typeof (TestObject).FullName, defnames.Length));
        }
		
        [Test]
        public virtual void GetDefinitionsForNoSuchClass ()
        {
            string[] defnames =
                ListableObjectFactory.GetObjectNamesForType (typeof (string));
            Assert.IsTrue (defnames.Length == 0, "No string definitions");
        }
		
        /// <summary>
        /// Check that count refers to factory class, not
        /// object class (we don't know what type factories may return,
        /// and it may even change over time).
        /// </summary>
        [Test]
        public virtual void GetCountForFactoryClass ()
        {
            int count =
                ListableObjectFactory.GetObjectNamesForType (
                    typeof (IFactoryObject)).Length;
            Assert.IsTrue (
                count == 2,
                string.Format ("Should have 2 factories, not {0}.", count));
        }
		
        [Test]
        public virtual void ContainsObjectDefinition ()
        {
            Assert.IsTrue (ListableObjectFactory.ContainsObjectDefinition ("rod"));
            Assert.IsTrue (ListableObjectFactory.ContainsObjectDefinition ("roderick"));
        }
	}
}
