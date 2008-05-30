#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using DotNetMock.Dynamic;
using NUnit.Framework;
using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Unit tests for the ObjectDefinitionReaderUtils class.
    /// </summary>
    /// <author>Rick Evans</author>
    /// <version>$Id: ObjectDefinitionReaderUtilsTests.cs,v 1.3 2006/04/09 07:24:50 markpollack Exp $</version>
    [TestFixture]
    public sealed class ObjectDefinitionReaderUtilsTests
    {
        [Test]
        public void RegisterObjectDefinitionSunnyDay()
        {
            IDynamicMock mockRegistry = new DynamicMock(typeof (IObjectDefinitionRegistry));
            mockRegistry.Expect("RegisterObjectDefinition");

            IDynamicMock mockDefinition = new DynamicMock(typeof (IObjectDefinition));
            IObjectDefinition definition = (IObjectDefinition) mockDefinition.Object;

            IObjectDefinitionRegistry registry = (IObjectDefinitionRegistry) mockRegistry.Object;
            ObjectDefinitionHolder holder = new ObjectDefinitionHolder(definition, "foo");

            ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, registry);
            mockRegistry.Verify();
            mockDefinition.Verify();
        }

        [Test]
        public void RegisterObjectDefinitionSunnyDayWithAliases()
        {
            IDynamicMock mockRegistry = new DynamicMock(typeof (IObjectDefinitionRegistry));
            mockRegistry.Expect("RegisterObjectDefinition");
            mockRegistry.Expect("RegisterAlias");
            mockRegistry.Expect("RegisterAlias");

            IDynamicMock mockDefinition = new DynamicMock(typeof (IObjectDefinition));
            IObjectDefinition definition = (IObjectDefinition) mockDefinition.Object;

            IObjectDefinitionRegistry registry = (IObjectDefinitionRegistry) mockRegistry.Object;
            ObjectDefinitionHolder holder
                = new ObjectDefinitionHolder(definition, "foo", new string[] {"bar", "baz"});

            ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, registry);
            mockRegistry.Verify();
            mockDefinition.Verify();
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void RegisterObjectDefinitionWithNullDefinition()
        {
            IDynamicMock mockRegistry = new DynamicMock(typeof (IObjectDefinitionRegistry));
            IObjectDefinitionRegistry registry = (IObjectDefinitionRegistry) mockRegistry.Object;
            ObjectDefinitionReaderUtils.RegisterObjectDefinition(null, registry);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void RegisterObjectDefinitionWithNullRegistry()
        {
            IDynamicMock mockDefinition = new DynamicMock(typeof (IObjectDefinition));
            IObjectDefinition definition = (IObjectDefinition) mockDefinition.Object;
            ObjectDefinitionHolder holder = new ObjectDefinitionHolder(definition, "foo");
            ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void RegisterObjectDefinitionWithAllArgumentsNull()
        {
            ObjectDefinitionReaderUtils.RegisterObjectDefinition(null, null);
        }

        [Test]
        public void RegisterObjectDefinitionWithDuplicateAlias()
        {
            IDynamicMock mockRegistry = new DynamicMock(typeof (IObjectDefinitionRegistry));
            mockRegistry.Expect("RegisterObjectDefinition");

            // we assume that some other object defition has already been associated with this alias...
            mockRegistry.ExpectAndThrow("RegisterAlias", new ObjectDefinitionStoreException());
            IDynamicMock mockDefinition = new DynamicMock(typeof (IObjectDefinition));
            IObjectDefinition definition = (IObjectDefinition) mockDefinition.Object;

            IObjectDefinitionRegistry registry = (IObjectDefinitionRegistry) mockRegistry.Object;
            ObjectDefinitionHolder holder
                = new ObjectDefinitionHolder(definition, "foo", new string[] { "bing" });

            try
            {
                ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, registry);
                Assert.Fail("Must have thrown an ObjectDefinitionStoreException store by this point.");
            }
            catch (ObjectDefinitionStoreException)
            {
                // expected...
            }
            mockRegistry.Verify();
            mockDefinition.Verify();
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GenerateObjectNameWithNullDefinition()
        {
            IDynamicMock mockRegistry = new DynamicMock(typeof (IObjectDefinitionRegistry));
            IObjectDefinitionRegistry registry = (IObjectDefinitionRegistry) mockRegistry.Object;
            ObjectDefinitionReaderUtils.GenerateObjectName(null, registry);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GenerateObjectNameWithNullRegistry()
        {
            IDynamicMock mockDefinition = new DynamicMock(typeof (IConfigurableObjectDefinition));
            ObjectDefinitionReaderUtils.GenerateObjectName((IConfigurableObjectDefinition) mockDefinition.Object, null);
        }
    }
}