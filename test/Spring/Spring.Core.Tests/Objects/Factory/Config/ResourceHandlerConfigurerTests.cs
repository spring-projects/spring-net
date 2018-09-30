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

using System.Collections;

using FakeItEasy;

using NUnit.Framework;

using Spring.Core.IO;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Unit tests for the TypeAliasConfigurer class
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class ResourceHandlerConfigurerTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Serialization()
        {
            IDictionary resourceHandlers = new Hashtable();
            resourceHandlers.Add("httpsss", typeof(UrlResource).AssemblyQualifiedName);

            ResourceHandlerConfigurer resourceHandlerConfiguer = new ResourceHandlerConfigurer();
            resourceHandlerConfiguer.ResourceHandlers = resourceHandlers;


            resourceHandlerConfiguer.Order = 1;

            SerializationTestUtils.SerializeAndDeserialize(resourceHandlerConfiguer);
        }

        [Test]
        public void UseInvalidTypeForDictionaryValue()
        {
            IDictionary resourceHandlers = new Hashtable();
            resourceHandlers.Add("httpsss", new Hashtable());

            ResourceHandlerConfigurer resourceHandlerConfiguer = new ResourceHandlerConfigurer();
            resourceHandlerConfiguer.ResourceHandlers = resourceHandlers;

            Assert.Throws<ObjectInitializationException>(() => resourceHandlerConfiguer.PostProcessObjectFactory(A.Fake<IConfigurableListableObjectFactory>()));
        }

        [Test]
        public void UseNonResolvableTypeForDictionaryValue()
        {
            IDictionary resourceHandlers = new Hashtable();
            resourceHandlers.Add("httpsss", "Spring.Core.IO.UrrrrlResource, Spring.Core");

            ResourceHandlerConfigurer resourceHandlerConfiguer = new ResourceHandlerConfigurer();
            resourceHandlerConfiguer.ResourceHandlers = resourceHandlers;


            Assert.Throws<ObjectInitializationException>(() => resourceHandlerConfiguer.PostProcessObjectFactory(A.Fake<IConfigurableListableObjectFactory>()));
        }

        [Test]
        public void SunnyDayScenarioUsingType()
        {


            IDictionary resourceHandlers = new Hashtable();
            resourceHandlers.Add("httpsss", typeof(UrlResource));

            CreateConfigurerAndTestNewProtcol(resourceHandlers);
        }

        [Test]
        public void SunnyDayScenarioUsingTypeString()
        {
            IDictionary typeAliases = new Hashtable();
            typeAliases.Add("httpsss", "Spring.Core.IO.UrlResource, Spring.Core");
            CreateConfigurerAndTestNewProtcol(typeAliases);

        }

        private void CreateConfigurerAndTestNewProtcol(IDictionary resourceHandlers)
        {
            ResourceHandlerConfigurer resourceHandlerConfiguer = new ResourceHandlerConfigurer();
            resourceHandlerConfiguer.ResourceHandlers = resourceHandlers;

            resourceHandlerConfiguer.Order = 1;


            resourceHandlerConfiguer.PostProcessObjectFactory(A.Fake<IConfigurableListableObjectFactory>());

            //todo investigate mocking the typeregistry, for now ask the actual one for information.
            Assert.IsTrue(ResourceHandlerRegistry.IsHandlerRegistered("httpsss"),
                          "ResourceHandlerConfigurer did not register a protocol handler with the ResourceHandlerRegistry");

            Assert.IsTrue(ResourceHandlerRegistry.IsHandlerRegistered("httpsss"), "Custom IResource not registered.");
            Assert.AreEqual(1, resourceHandlerConfiguer.Order);
        }
    }
}
