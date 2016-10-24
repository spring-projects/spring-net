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
using Rhino.Mocks;
using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Unit tests for the ObjectDefinitionReaderUtils class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class ObjectDefinitionReaderUtilsTests
    {
        private MockRepository mocks;
        private IObjectDefinitionRegistry registry;
        private IObjectDefinition definition;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            registry = mocks.StrictMock<IObjectDefinitionRegistry>();
            definition = mocks.StrictMock<IObjectDefinition>();
        }

        [Test]
        public void RegisterObjectDefinitionSunnyDay()
        {
            registry.RegisterObjectDefinition(null, null);
            LastCall.IgnoreArguments();
            mocks.ReplayAll();

            ObjectDefinitionHolder holder = new ObjectDefinitionHolder(definition, "foo");

            ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, registry);
            mocks.VerifyAll();
        }

        [Test]
        public void RegisterObjectDefinitionSunnyDayWithAliases()
        {
            registry.RegisterObjectDefinition("foo", definition);
            registry.RegisterAlias("foo", "bar");
            registry.RegisterAlias("foo", "baz");
            mocks.ReplayAll();

            ObjectDefinitionHolder holder = new ObjectDefinitionHolder(definition, "foo", new string[] {"bar", "baz"});

            ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, registry);
            
            mocks.VerifyAll();
        }

        [Test]
        public void RegisterObjectDefinitionWithNullDefinition()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectDefinitionReaderUtils.RegisterObjectDefinition(null, registry));
        }

        [Test]
        public void RegisterObjectDefinitionWithNullRegistry()
        {
            ObjectDefinitionHolder holder = new ObjectDefinitionHolder(definition, "foo");
            Assert.Throws<ArgumentNullException>(() => ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, null));
        }

        [Test]
        public void RegisterObjectDefinitionWithAllArgumentsNull()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectDefinitionReaderUtils.RegisterObjectDefinition(null, null));
        }

        [Test]
        public void RegisterObjectDefinitionWithDuplicateAlias()
        {
            registry.RegisterObjectDefinition("foo", definition);

            // we assume that some other object defition has already been associated with this alias...
            registry.RegisterAlias(null, null);
            LastCall.IgnoreArguments().Throw(new ObjectDefinitionStoreException());
            mocks.ReplayAll();

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

            mocks.VerifyAll();
        }

        [Test]
        public void GenerateObjectNameWithNullDefinition()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectDefinitionReaderUtils.GenerateObjectName(null, registry));
        }

        [Test]
        public void GenerateObjectNameWithNullRegistry()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectDefinitionReaderUtils.GenerateObjectName(mocks.StrictMock<IConfigurableObjectDefinition>(), null));
        }
    }
}