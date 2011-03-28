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

using NUnit.Framework;
using Spring.Aop.Interceptor;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Objects.Factory.Xml;

#endregion#region License

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// Tests for ObjectNameAutoProxyCreator functionality
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class ObjectNameAutoProxyCreatorTests
    {
        private IApplicationContext ctx;

        [SetUp]
        public void SetUp()
        {
            string configLocation =
                ReadOnlyXmlTestResource.GetFilePath("objectNameAutoProxyCreatorTests.xml",
                                                    typeof (ObjectNameAutoProxyCreatorTests));
            ctx = new XmlApplicationContext(configLocation);
        }

        [Test]
        public void NoProxy()
        {
            ITestObject testObject = (ITestObject) ctx.GetObject("noproxy");
            Assert.IsFalse(AopUtils.IsAopProxy(testObject), testObject + " is not an AOP proxy");
            Assert.AreEqual("noproxy", testObject.Name);
        }

        [Test]
        public void ProxyWithExactNameMatch()
        {
            ITestObject testObject = (ITestObject) ctx.GetObject("testObject");
            ProxyAssertions(testObject, 1);
            Assert.AreEqual("SimpleTestObject", testObject.Name);
        }

        [Test]
        public void ProxyWithDoubleProxyingInvokesInterceptorsOnceOnly()
        {
            ITestObject testObject = (ITestObject)ctx.GetObject("doubleProxy");
            ProxyAssertions(testObject, 1);
            Assert.AreEqual("doubleProxy", testObject.Name);
        }

        [Test]
        public void ProxyWithWildcardMatchSuffix()
        {
            ITestObject testObject = (ITestObject) ctx.GetObject("SmithFamilyMember");
            ProxyAssertions(testObject, 1);
            Assert.AreEqual("John Smith", testObject.Name);
        }

        [Test]
        public void ProxyWithTwoWildcardsMatch()
        {
            ITestObject testObject = (ITestObject)ctx.GetObject("twoWildcardsTestObject");
            ProxyAssertions(testObject, 1);
            Assert.AreEqual("Damjan Tomic", testObject.Name);
        }

        [Test]
        public void AppliesToCreatedObjectsNotFactoryObject()
        {
            ITestObject testObject = (ITestObject) ctx.GetObject("factoryObject");
            ProxyAssertions(testObject, 1);
        }

        [Test]
        public void FrozenProxy()
        {
            ITestObject testObject = (ITestObject)ctx.GetObject("frozen");
            Assert.IsTrue( ((IAdvised)testObject).IsFrozen);
        }

        [Test]
        public void Introduction()
        {
            object obj = ctx.GetObject("introductionUsingDecorator");
            Assert.IsNotNull(obj as IIsModified);
            ITestObject testObject = (ITestObject) obj;
            NopInterceptor nop = (NopInterceptor)ctx.GetObject("introductionNopInterceptor");
            Assert.AreEqual(0, nop.Count);
            Assert.IsTrue(AopUtils.IsCompositionAopProxy(testObject), testObject + " is not an Composition AOP Proxy");
            int age = 5;
            testObject.Age = age;
            Assert.AreEqual(age, testObject.Age);
            Assert.IsNotNull(testObject as IIsModified);
            Assert.IsTrue(((IIsModified)testObject).IsModified);
            Assert.AreEqual(3, nop.Count);
            Assert.AreEqual("introductionUsingDecorator", testObject.Name);
        }


        private void ProxyAssertions(ITestObject testObject, int nopInterceptorCount)
        {
            NopInterceptor nop = (NopInterceptor) ctx.GetObject("nopInterceptor");
            Assert.AreEqual(0, nop.Count);
            Assert.IsTrue(AopUtils.IsCompositionAopProxy(testObject), testObject + " is not an AOP Proxy");
            int age = 5;
            testObject.Age = age;
            Assert.AreEqual(age, testObject.Age);
            Assert.AreEqual(2*nopInterceptorCount, nop.Count);
        }

        [Test]
        public void DecoratorProxyWithWildcardMatch()
        {
            ITestObject testObject = (ITestObject)ctx.GetObject("decoratorProxy");
            DecoratorProxyAssertions(testObject);
            Assert.AreEqual("decoratorProxy", testObject.Name);
        }

        private void DecoratorProxyAssertions(ITestObject testObject)
        {
            CountingBeforeAdvice cba = (CountingBeforeAdvice) ctx.GetObject("countingBeforeAdvice");
            NopInterceptor nop = (NopInterceptor)ctx.GetObject("nopInterceptor");
            Assert.AreEqual(0, cba.GetCalls());
            Assert.AreEqual(0, nop.Count);
            Assert.IsTrue(AopUtils.IsDecoratorAopProxy(testObject), testObject + " is not an AOP Proxy");
            //extra advice calls are due to test IsDecoratorAopProxy and call to .GetType in impl
            Assert.AreEqual(1, nop.Count);
            Assert.AreEqual(1, cba.GetCalls());
            int age = 5;
            testObject.Age = age;
            Assert.AreEqual(age, testObject.Age);
            Assert.AreEqual(3, nop.Count);
            Assert.AreEqual(3, cba.GetCalls());
        }
    }
}