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

using Spring.Context.Support;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;
using Spring.Threading;

#endregion#region License

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// Tests for auto proxy creation by advisor recognition.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Mark Pollack (.NET)</author>
    [TestFixture]
    public class AdvisorAutoProxyCreatorTests
    {
        private static string ADVISOR_APC_OBJECT_NAME = "aapc";

        protected virtual IObjectFactory ObjectFactory
        {
            get
            {
                string configLocation = ReadOnlyXmlTestResource.GetFilePath("advisorAutoProxyCreator.xml", typeof(AdvisorAutoProxyCreatorTests));
                return new XmlApplicationContext(configLocation);
            }
        }

        [Test]
        public void DefaultExclusionPrefix()
        {
            DefaultAdvisorAutoProxyCreator aapc = (DefaultAdvisorAutoProxyCreator)ObjectFactory.GetObject(ADVISOR_APC_OBJECT_NAME);
            Assert.AreEqual(ADVISOR_APC_OBJECT_NAME + DefaultAdvisorAutoProxyCreator.Separator, aapc.AdvisorObjectNamePrefix);
            Assert.IsFalse(aapc.UsePrefix);
        }

        /// <summary>
        /// No pointcuts match the methods on NoSetterProperties therefore 
        /// there should be proxying.
        /// </summary> 
        [Test]
        public void NoProxy()
        {
            IObjectFactory of = ObjectFactory;
            object o = of.GetObject("noSetterPropertiesObject");
            Assert.IsFalse(AopUtils.IsAopProxy(o));
        }

        /// <summary>
        /// A pointcut matches the property (i.e. method set_Age) on TestObject
        /// therefore there should be proxying.
        /// </summary> 
        [Test]
        public void HasProxy()
        {
            IObjectFactory of = ObjectFactory;
            object o = of.GetObject("testObject");
            Assert.IsTrue(AopUtils.IsAopProxy(o), "Expected TestObject to be proxied");
        }
        
        [Test]
        public void RegexpApplied()
        {
            IObjectFactory of = ObjectFactory;
            ITestObject testObject = (ITestObject)of.GetObject("testObject");
            MethodCounter counter = (MethodCounter)of.GetObject("CountingAdvice");
            Assert.AreEqual(0,counter.GetCalls());
            testObject.Spouse = new TestObject("Daniela", 23);
            Assert.AreEqual(0, counter.GetCalls());
            testObject.Name = "foo";
            Assert.AreEqual(1, counter.GetCalls());
            
        }
        
        [Test]
        public void SetLTCValue()
        {
            IObjectFactory of = ObjectFactory;
            ITestObject testObject = (ITestObject)of.GetObject("testObject");
            OrderedLogicalThreadContextCheckAdvisor orderedBeforeLTCSet =
                (OrderedLogicalThreadContextCheckAdvisor)of.GetObject("orderedBeforeLTCSet");
            Assert.AreEqual(0, orderedBeforeLTCSet.CountingBeforeAdvice.GetCalls());

            Assert.IsNull(LogicalThreadContext.GetData(LogicalThreadContextAdvice.ORDERING_SLOT));
            Assert.AreEqual(4, testObject.Age, "Initial value of age for test object is not correct.");
            int newAge = 5;
            testObject.Age = newAge;
            Assert.AreEqual(1, orderedBeforeLTCSet.CountingBeforeAdvice.GetCalls());

            Assert.AreEqual(newAge, testObject.Age, "Assigned value of age for test object is not correct.");
            Assert.IsNotNull(LogicalThreadContext.GetData(LogicalThreadContextAdvice.ORDERING_SLOT));
           
        }
    }
}