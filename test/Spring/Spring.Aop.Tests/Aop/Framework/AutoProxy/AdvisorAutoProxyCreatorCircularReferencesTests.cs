#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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
using System.Reflection;
using NUnit.Framework;
using Spring.Aop.Framework.DynamicProxy;
using Spring.Aop.Support;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// Tests for auto proxy creation combined with factory object and circular references.
    /// </summary>
    /// <author>Erich Eichinger (.NET)</author>
    /// <version>$Id: AdvisorAutoProxyCreatorCircularReferencesTests.cs,v 1.4 2007/09/07 01:53:01 markpollack Exp $</version>
    [TestFixture]
    public class AdvisorAutoProxyCreatorCircularReferencesTests
    {
        [Test]
        public void TestAutoProxyCreation()
        {
            XmlApplicationContext context = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("advisorAutoProxyCreatorCircularReferencesTests.xml", typeof(AdvisorAutoProxyCreatorCircularReferencesTests)));
            // direct deps of AutoProxyCreator are not eligable for proxying
            Assert.IsFalse(AopUtils.IsAopProxy(context.GetObject("aapc")));
            Assert.IsFalse(AopUtils.IsAopProxy(context.GetObject("testAdvisor")));
            Assert.IsFalse(AopUtils.IsAopProxy(context.GetObject("&testObjectFactory")));
            Assert.IsFalse(AopUtils.IsAopProxy(context.GetObject("someOtherObject")));

            // this one is completely independent
            Assert.IsTrue(AopUtils.IsAopProxy(context.GetObject("independentObject")));

            // products of the factory created at runtime should be proxied
            Assert.IsTrue(AopUtils.IsAopProxy(context.GetObject("testObjectFactory")));
        }
    }

    #region Support Classes

    public class TestAdvisor : StaticMethodMatcherPointcutAdvisor
    {
        private ITestObject testObject;

        public ITestObject TestObject
        {
            get { return this.testObject; }
            set { this.testObject = value; }
        }

        public override bool Matches(MethodInfo method, Type targetType)
        {
            return true;
        }
    }

    public class SomeOtherObject
    {}

    public class IndependentObject
    { }
    
    public class TestObjectFactoryObject : IFactoryObject, IInitializingObject
    {
        private bool initialized = false;
        private ITestObject testObject;
        private SomeOtherObject someOtherObject;

        public TestObjectFactoryObject()
        {
        }

        public SomeOtherObject SomeOtherObject
        {
            get { return this.someOtherObject; }
            set { this.someOtherObject = value; }
        }

        public object GetObject()
        {
            // return product only, if factory has been fully initialized!
            if (!initialized)
            {
                return null;
            }
            else
            {
                return testObject;
            }
        }

        public Type ObjectType
        {
            get
            {
                // return type only if we are ready to deliver our product!
                if (!initialized)
                {
                    return null;
                }
                else
                {
                    return typeof(ITestObject);
                }
            }
        }

        public bool IsSingleton
        {
            get { return true; }
        }

        public void AfterPropertiesSet()
        {
            Assert.IsNotNull(someOtherObject);
            testObject = new TestObject();
            initialized = true;
        }
    }

    #endregion
}
