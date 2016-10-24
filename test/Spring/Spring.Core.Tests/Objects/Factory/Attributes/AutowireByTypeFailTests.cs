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
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Attributes.ByType;

namespace Spring.Objects.Factory.Attributes
{
    [TestFixture]
    public class AutowireByTypeFailTests
    {
        private XmlApplicationContext _applicationContext;

        [SetUp]
        public void Setup()
        {
            _applicationContext = new XmlApplicationContext(false,
                                                            "assembly://Spring.Core.Tests/Spring.Objects.Factory.Attributes/ByTypeFailObjects.xml");
        }

        [Test]
        public void FailFieldInjectionTooManyObjects()
        {
            Exception ex = null;
            try
            {
                var testObj = (AutowireTestFieldNormal)_applicationContext.GetObject("AutowireTestFieldNormal");
            }
            catch (Exception e) { ex = e; }

            Assert.That(ex, Is.Not.Null, "Should throw an exception");
            Assert.That(ex.Message, Does.Contain("Injection of autowired dependencies failed"));
        }

        [Test]
        public void FailPropertyInjectionTooManyObjects()
        {
            Exception ex = null;
            try
            {
                var testObj = (AutowireTestPropertyNormal)_applicationContext.GetObject("AutowireTestPropertyNormal");
            }
            catch (Exception e) { ex = e; }

            Assert.That(ex, Is.Not.Null, "Should throw an exception");
            Assert.That(ex.Message, Does.Contain("Injection of autowired dependencies failed"));
        }

        [Test]
        public void FailMethodInjectionTooManyObjects()
        {
            Exception ex = null;
            try
            {
                var testObj = (AutowireTestMethodNormal)_applicationContext.GetObject("AutowireTestMethodNormal");
            }
            catch (Exception e) { ex = e; }

            Assert.That(ex, Is.Not.Null, "Should throw an exception");
            Assert.That(ex.Message, Does.Contain("Injection of autowired dependencies failed"));
        }

        [Test]
        public void FailConstructorInjectionTooManyObjects()
        {
            var ex = Assert.Throws<UnsatisfiedDependencyException>(() => _applicationContext.GetObject("AutowireTestConstructorNormal"));
            Assert.That(ex.Message, Does.Contain("Error creating object with name"));
        }
    }
}
