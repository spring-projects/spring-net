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
using Spring.Objects.Factory.Attributes.Collections;

namespace Spring.Objects.Factory.Attributes
{
    [TestFixture]
    public class AutowireCollectionTests
    {
        private XmlApplicationContext _applicationContext;

        [SetUp]
        public void Setup()
        {
            _applicationContext = new XmlApplicationContext(false,
                                                            "assembly://Spring.Core.Tests/Spring.Objects.Factory.Attributes/CollectionObjects.xml");
        }

        [Test]
        public void InjectIntoList()
        {
            var testObj = (AutowireTestList)_applicationContext.GetObject("AutowireTestList");
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("AutowireTestList");

            Assert.That(testObj.foos, Is.Not.Null);
            Assert.That(testObj.foos.Count, Is.EqualTo(2));
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(2));
        }

        [Test]
        public void InjectIntoSet()
        {
            var testObj = (AutowireTestSet)_applicationContext.GetObject("AutowireTestSet");
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("AutowireTestSet");

            Assert.That(testObj.foos, Is.Not.Null);
            Assert.That(testObj.foos.Count, Is.EqualTo(2));
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(2));
        }

        [Test]
        public void InjectIntoDictionary()
        {
            var testObj = (AutowireTestDictionary)_applicationContext.GetObject("AutowireTestDictionary");
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("AutowireTestDictionary");

            Assert.That(testObj.foos, Is.Not.Null);
            Assert.That(testObj.foos.Count, Is.EqualTo(2));
            Assert.That(testObj.foos.ContainsKey("HelloFoo"), Is.True);
            Assert.That(testObj.foos.ContainsKey("CiaoFoo"), Is.True);
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(2));
        }

        [Test]
        public void InjectIntoDictionaryFail()
        {
            Exception ex = null;

            try
            {
                var testObj = (AutowireTestDictionaryFail)_applicationContext.GetObject("AutowireTestDictionaryFail");
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.InnerException.InnerException.Message.Contains("first generic to be a string"), Is.True);
        }

        [Test]
        public void InjectIntoListWithQualifier()
        {
            var testObj = (AutowireTestQualifier)_applicationContext.GetObject("AutowireTestQualifier");
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("AutowireTestQualifier");

            Assert.That(testObj.foos, Is.Not.Null);
            Assert.That(testObj.foos.Count, Is.EqualTo(1));
            Assert.That(testObj.foos[0].GetType(), Is.EqualTo(typeof(CiaoFoo)));
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(1));
        }
    }
}
