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

using System.Collections.Generic;

using NUnit.Framework;

using Spring.Aop.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Objects.Factory.Xml;

namespace Spring.Aop.Config
{
    /// <summary>
    /// This class contains tests for the custom aop namespace.
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class AopNamespaceParserTests
    {

       private IApplicationContext ctx;

        [SetUp]
        public void Setup()
        {
            // IS WELLKNOWN NOW
            //NamespaceParserRegistry.RegisterParser(typeof(AopNamespaceParser));
            //ctx = new XmlApplicationContext( "assembly://Spring.Aop.Tests/Spring.Aop.Config/AopNamespaceParserTests.xml");
            ctx = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("AopNamespaceParserTests.xml", this.GetType()));
        }



        [Test]
        public void Registered()
        {
            Assert.IsNotNull(NamespaceParserRegistry.GetParser("http://www.springframework.net/aop"));


            IPointcut pointcut = ctx["getDescriptionCalls"] as IPointcut;
            Assert.IsNotNull(pointcut);
            Assert.IsFalse(AopUtils.IsAopProxy(pointcut));


            ITestObject testObject = ctx["testObject"] as ITestObject;
            Assert.IsNotNull(testObject);
            Assert.IsTrue(AopUtils.IsAopProxy(testObject), "Object should be an AOP proxy");

            IAdvised advised = testObject as IAdvised;
            Assert.IsNotNull(advised);
            IList<IAdvisor> advisors = advised.Advisors;
            Assert.IsTrue(advisors.Count > 0, "Advisors should not be empty");


        }

        [Test]
        public void AdviceInvokedCorrectly()
        {
            CountingBeforeAdvice getDescriptionCounter = ctx.GetObject("getDescriptionCounter") as CountingBeforeAdvice;
            Assert.IsNotNull(getDescriptionCounter);

            ITestObject testObject = GetTestObject();

            Assert.AreEqual(0,getDescriptionCounter.GetCalls("GetDescription"),"Incorrect initial getDescription count");

            testObject.GetDescription();

            Assert.AreEqual(1, getDescriptionCounter.GetCalls("GetDescription"), "Incorrect getDescription count");

        }

        private ITestObject GetTestObject()
        {
            return ctx.GetObject("testObject", typeof (ITestObject)) as ITestObject;
        }


    }
}
