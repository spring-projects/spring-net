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

using System;

using FakeItEasy;

using NUnit.Framework;
using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Unit tests for the ObjectDefinitionReaderUtils class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class ObjectDefinitionReaderUtilsTests
    {
        private IObjectDefinitionRegistry registry;
        private IObjectDefinition definition;

        [SetUp]
        public void SetUp()
        {
            registry = A.Fake<IObjectDefinitionRegistry>();
            definition = A.Fake<IObjectDefinition>();
        }

        [Test]
        public void RegisterObjectDefinitionSunnyDay()
        {
            ObjectDefinitionHolder holder = new ObjectDefinitionHolder(definition, "foo");

            ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, registry);

            A.CallTo(() => registry.RegisterObjectDefinition(null, null)).WithAnyArguments().MustHaveHappened();
        }

        [Test]
        public void RegisterObjectDefinitionSunnyDayWithAliases()
        {
            ObjectDefinitionHolder holder = new ObjectDefinitionHolder(definition, "foo", new string[] {"bar", "baz"});

            ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, registry);

            A.CallTo(() => registry.RegisterObjectDefinition("foo", definition)).MustHaveHappened();
            A.CallTo(() => registry.RegisterAlias("foo", "bar")).MustHaveHappened();
            A.CallTo(() => registry.RegisterAlias("foo", "baz")).MustHaveHappened();
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
            A.CallTo(() => registry.RegisterAlias(null, null)).WithAnyArguments().Throws<ObjectDefinitionStoreException>();

            ObjectDefinitionHolder holder
                = new ObjectDefinitionHolder(definition, "foo", new string[] {"bing"});

            Assert.Throws<ObjectDefinitionStoreException>(() => ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, registry));
        }

        [Test]
        public void GenerateObjectNameWithNullDefinition()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectDefinitionReaderUtils.GenerateObjectName(null, registry));
        }

        [Test]
        public void GenerateObjectNameWithNullRegistry()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectDefinitionReaderUtils.GenerateObjectName(A.Fake<IConfigurableObjectDefinition>(), null));
        }
    }
}