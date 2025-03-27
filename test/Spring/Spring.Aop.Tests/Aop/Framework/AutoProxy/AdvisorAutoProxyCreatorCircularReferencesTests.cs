#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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
using System.Collections.Generic;
using System.Reflection;


using NUnit.Framework;

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
    [TestFixture]
    public class AdvisorAutoProxyCreatorCircularReferencesTests
    {
        [Test]
        public void TestAutoProxyCreation()
        {
            XmlApplicationContext context = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("advisorAutoProxyCreatorCircularReferencesTests.xml", typeof(AdvisorAutoProxyCreatorCircularReferencesTests)));
            CountingAfterReturningAdvisor countingAdvisor = (CountingAfterReturningAdvisor)context.GetObject("testAdvisor");

            // direct deps of AutoProxyCreator are not eligable for proxying
            Assert.IsFalse(AopUtils.IsAopProxy(context.GetObject("aapc")));
            Assert.IsFalse(AopUtils.IsAopProxy(countingAdvisor));

            TestObjectFactoryObject testObjectFactory = (TestObjectFactoryObject) context.GetObject("&testObjectFactory");
            Assert.IsFalse(AopUtils.IsAopProxy(testObjectFactory));

            Assert.IsFalse(AopUtils.IsAopProxy(context.GetObject("someOtherObject")));

            // this one is completely independent
            Assert.IsTrue(AopUtils.IsAopProxy(context.GetObject("independentObject")));


            // Asserts SPRNET-1225 - advisor dependencies most not be auto-proxied
            object testObject = context.GetObject("testObjectFactory");
            Assert.IsFalse(AopUtils.IsAopProxy(testObject));

            // Asserts SPRNET-1224 - factory product most be cached
            context.GetObject("testObjectFactory");
            testObjectFactory.GetObjectCounter = 0;
            context.GetObject("testObjectFactory");
            Assert.AreEqual(0, testObjectFactory.GetObjectCounter);

            ICloneable someOtherObject = (ICloneable) context.GetObject("someOtherObject");
            someOtherObject.Clone();
            ICloneable independentObject = (ICloneable) context.GetObject("independentObject");
            independentObject.Clone();
            Assert.AreEqual(1, countingAdvisor.GetCalls());
        }
    }

    #region Support Classes

    public class TestDefaultAdvisorAutoProxyCreator : DefaultAdvisorAutoProxyCreator, IInitializingObject
    {
        private readonly ILog _logger;

        public TestDefaultAdvisorAutoProxyCreator()
        {
            _logger = LogManager.GetLogger(this.GetType().Name + "#" + GetHashCode());
            _logger.Trace("Created instance");
        }

        protected override IList<object> GetAdvicesAndAdvisorsForObject(Type targetType, string targetName, ITargetSource customTargetSource)
        {
            _logger.Trace("GetAdvicesAndAdvisorsForObject begin");
            IList<object> advices = base.GetAdvicesAndAdvisorsForObject(targetType, targetName, customTargetSource);
            _logger.Trace("GetAdvicesAndAdvisorsForObject end");
            return advices;
        }

        public override void AfterPropertiesSet()
        {
            _logger.Trace("AfterPropertiesSet");
            base.AfterPropertiesSet();
        }
    }

    public class CountingAfterReturningAdvisor : StaticMethodMatcherPointcutAdvisor
    {
        private ITestObject testObject;

        public ITestObject TestObject
        {
            get { return this.testObject; }
            set { this.testObject = value; }
        }

        public int GetCalls()
        {
            return ((CountingAfterReturningAdvice) base.Advice).GetCalls();
        }

        public CountingAfterReturningAdvisor()
        {
            LogManager.GetLogger(this.GetType()).Trace("Created instance #" + this.GetHashCode());
            base.Advice = new CountingAfterReturningAdvice();
        }

        public override bool Matches(MethodInfo method, Type targetType)
        {
            return true;
        }
    }

    public class SomeOtherObject : ICloneable
    {
        public SomeOtherObject()
        {
            LogManager.GetLogger(this.GetType()).Trace("Created instance #" + this.GetHashCode());
        }

        public object Clone()
        {
            return this;
        }
    }

    public class IndependentObject : ICloneable
    {
        public IndependentObject()
        {
            LogManager.GetLogger(this.GetType()).Trace("Created instance #" + this.GetHashCode());
        }

        public object Clone()
        {
            return this;
        }
    }
    
    public class TestObjectFactoryObject : IFactoryObject, IInitializingObject
    {
        private bool initialized = false;
        private ITestObject testObject;
        private SomeOtherObject someOtherObject;
        private readonly ILog _logger;

        public int GetObjectCounter = 0;

        public TestObjectFactoryObject()
        {
            _logger = LogManager.GetLogger(this.GetType().Name + "#" + this.GetHashCode());
            _logger.Trace("Created instance");
        }

        public SomeOtherObject SomeOtherObject
        {
            get { return this.someOtherObject; }
            set { this.someOtherObject = value; }
        }

        public object GetObject()
        {
            GetObjectCounter++;
            // return product only, if factory has been fully initialized!
            if (!initialized)
            {
                _logger.Trace("GetObject(): not initialized, returning null");
                return null;
            }
            _logger.Trace("GetObject(): initialized, returning testObject");
            return testObject;
        }

        public Type ObjectType
        {
            get
            {
                // return type only if we are ready to deliver our product!
                if (!initialized)
                {
                    _logger.Trace("get_ObjectType(): not initialized, returning null");
                    return null;
                }
                _logger.Trace("get_ObjectType(): initialized, returning typeof(ITestObject)");
                return typeof(ITestObject);
            }
        }

        public bool IsSingleton
        {
            get { return true; }
        }

        public void AfterPropertiesSet()
        {
            _logger.Trace("AfterPropertiesSet");
            Assert.IsNotNull(someOtherObject);
            testObject = new TestObject();
            initialized = true;
        }
    }

    #endregion
}
