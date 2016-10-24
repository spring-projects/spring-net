#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Attributes.ByType;

namespace Spring.Objects.Factory.Attributes
{
    [TestFixture]
    public class AutowireByTypeNormalTests
    {
        private XmlApplicationContext _applicationContext;

        [SetUp]
        public void Setup()
        {
            _applicationContext = new XmlApplicationContext(false,
                                                            "assembly://Spring.Core.Tests/Spring.Objects.Factory.Attributes/ByTypeNormalObjects.xml");
        }

        [Test]
        public void InjectOnField()
        {
            var testObj = (AutowireTestFieldNormal) _applicationContext.GetObject("AutowireTestFieldNormal");
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("AutowireTestFieldNormal");

            Assert.That(testObj.hello, Is.Not.Null);
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(1));
        }

        [Test]
        public void InjectOnProperty()
        {
            var testObj = (AutowireTestPropertyNormal) _applicationContext.GetObject("AutowireTestPropertyNormal");
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("AutowireTestPropertyNormal");

            Assert.That(testObj.Hello, Is.Not.Null);
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(1));
        }

        [Test]
        public void InjectOnMethod()
        {
            var testObj = (AutowireTestMethodNormal) _applicationContext.GetObject("AutowireTestMethodNormal");
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("AutowireTestMethodNormal");

            Assert.That(testObj.hello, Is.Not.Null);
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(1));
        }

        [Test]
        public void InjectOnConstructor()
        {
            var testObj = (AutowireTestConstructorNormal)_applicationContext.GetObject("AutowireTestConstructorNormal");
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("AutowireTestConstructorNormal");

            Assert.That(testObj.hello, Is.Not.Null);
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(0));
        }
    }
}
