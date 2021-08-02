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
using Spring.Transaction;

#endregion

namespace Spring.Data.NHibernate.Config
{
    /// <summary>
    /// This class contains tests for 
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class AopConfiguration
    {
        private IApplicationContext ctx;

        [SetUp]
        public void Setup()
        {
            string aopConfigPath =
                string.Format(
                    "assembly://{0}/Spring.Data.NHibernate.Config/AopConfiguration.xml"
                    , this.GetType().Assembly.GetName().Name);
            ctx = new XmlApplicationContext(aopConfigPath);
        }

        [Test]
        [Ignore("TODO Connects to database")]
        public void ProxyDataAccessAndServiceLayer()
        {
            Assert.IsFalse(AopUtils.IsAopProxy( ctx["DbProvider"] ));
            Assert.IsFalse(AopUtils.IsAopProxy( ctx["SessionFactory"]   ));
            Assert.IsFalse(AopUtils.IsAopProxy(ctx["hibernateTransactionManager"]));
            Assert.IsFalse(AopUtils.IsAopProxy(ctx["transactionManager"]));
            //Assert.IsTrue(AopUtils.IsAopProxy(ctx["testObjectDaoTransProxy"]));
            Assert.IsTrue(AopUtils.IsAopProxy(ctx["TestObjectDao"]));
            Assert.IsTrue(AopUtils.IsAopProxy(ctx["SimpleService"]));

            CallCountingTransactionManager ccm = ctx["transactionManager"] as CallCountingTransactionManager;
            Assert.IsNotNull(ccm);
            Assert.AreEqual(0, ccm.begun);
            Assert.AreEqual(0, ccm.commits);

            LoggingAroundAdvice caa = ctx["loggingAroundAdvice"] as LoggingAroundAdvice;
            Assert.IsNotNull(caa);
            Assert.AreEqual(0, caa.numInvoked);
            
            ISimpleService simpleService = ctx["SimpleService"] as ISimpleService;
            Assert.IsNotNull(simpleService);
            simpleService.DoWork(new TestObject());
            Assert.AreEqual(1, ccm.begun);
            Assert.AreEqual(1, ccm.commits);
            Assert.AreEqual(1, caa.numInvoked);
        }

        
    }
}