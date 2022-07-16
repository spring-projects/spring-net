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

namespace Spring.Objects.Factory
{
    /// <summary>
    /// Unit tests for the AbstractListableObjectFactory class.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Rick Evans (.NET)</author>
    public abstract class AbstractListableObjectFactoryTests :
        AbstractObjectFactoryTests
    {
        /// <summary>
        /// Subclasses must initialize this (via the derived ObjectFactory property).
        /// </summary>
        protected internal virtual IListableObjectFactory ListableObjectFactory
        {
            get
            {
                if (!(ObjectFactory is IListableObjectFactory))
                {
                    throw new SystemException("IListableObjectFactory required...");
                }

                return (IListableObjectFactory) ObjectFactory;
            }
        }

        /// <summary>
        /// Subclasses can override this.
        /// </summary>
        [Test]
        public virtual void Count()
        {
            AssertCount(19);
        }

        protected internal void AssertCount(int count)
        {
            var defnames = ListableObjectFactory.GetObjectDefinitionNames(true);
            Assert.IsTrue(
                defnames.Count == count,
                $"We should have {count} objects, not {defnames.Count}.");
        }

        [Test]
        public virtual void ObjectCount()
        {
            AssertTestObjectCount(12);
        }

        public virtual void AssertTestObjectCount(int count)
        {
            var defnames = ListableObjectFactory.GetObjectNamesForType(typeof(TestObject));
            Assert.IsTrue(
                defnames.Count == count,
                $"We should have {count} objects for class {typeof(TestObject).FullName}, not {defnames.Count}.");
        }

        [Test]
        public virtual void GetDefinitionsForNoSuchClass()
        {
            var defnames = ListableObjectFactory.GetObjectNamesForType(typeof(string));
            Assert.IsTrue(defnames.Count == 0, "No string definitions");
        }

        /// <summary>
        /// Check that count refers to factory class, not
        /// object class (we don't know what type factories may return,
        /// and it may even change over time).
        /// </summary>
        [Test]
        public virtual void GetCountForFactoryClass()
        {
            int count = ListableObjectFactory.GetObjectNamesForType(typeof(IFactoryObject)).Count;
            Assert.IsTrue(
                count == 2,
                $"Should have 2 factories, not {count}.");
        }

        [Test]
        public virtual void ContainsObjectDefinition()
        {
            Assert.IsTrue(ListableObjectFactory.ContainsObjectDefinition("rod"));
            Assert.IsTrue(ListableObjectFactory.ContainsObjectDefinition("roderick"));
        }
    }
}
