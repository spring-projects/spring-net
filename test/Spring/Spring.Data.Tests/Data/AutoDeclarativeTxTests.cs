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
using Spring.Aop.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Transaction;

#endregion

namespace Spring.Data
{
	/// <summary>
	/// Test case that uses the approach of automatically creating 
	/// declarative transaction interceptors for objects identified via means
	/// of transaction attributes.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	[TestFixture]
	public class AutoDeclarativeTxTests 
	{
        private IApplicationContext ctx;

        [SetUp]
        public void SetUp()
        {
            //LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter();
            ctx =
                new XmlApplicationContext("assembly://Spring.Data.Tests/Spring.Data/AutoDeclarativeTxTests.xml");
            
        }



        [Test]
        public void CoordinatorDeclarativeWithAttributes()
        {
            ITestCoord coord = ctx["testCoordinator"] as ITestCoord;
            Assert.IsNotNull(coord);
            CallCountingTransactionManager ccm = ctx["transactionManager"] as CallCountingTransactionManager;
            Assert.IsNotNull(ccm);
            LoggingAroundAdvice advice = (LoggingAroundAdvice)ctx["consoleLoggingAroundAdvice"];
            Assert.IsNotNull(advice);

            ITestObjectMgr testObjectMgr = ctx["testObjectManager"] as ITestObjectMgr;
            Assert.IsNotNull(testObjectMgr);
            //Proxied due to NameMatchMethodPointcutAdvisor
            Assert.IsTrue(AopUtils.IsAopProxy(coord));

            //Proxied due to DefaultAdvisorAutoProxyCreator
            Assert.IsTrue(AopUtils.IsAopProxy(testObjectMgr));


            TestObject to1 = new TestObject("Jack", 7);
            TestObject to2 = new TestObject("Jill", 6);

            Assert.AreEqual(0, ccm.begun);
            Assert.AreEqual(0, ccm.commits);
            Assert.AreEqual(0, advice.numInvoked);
            
            coord.WorkOn(to1,to2);
            
            Assert.AreEqual(1, ccm.begun);
            Assert.AreEqual(1, ccm.commits);
            Assert.AreEqual(1, advice.numInvoked);
        }

	}
}
