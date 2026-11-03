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

using NUnit.Framework;
using Spring.Aop.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Transaction;

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

        [TearDown]
        public void TearDown()
        {
            ctx?.Dispose();
        }

        [Test]
        [Ignore("TODO Connects to database")]
        public void ProxyDataAccessAndServiceLayer()
        {
            Assert.That(AopUtils.IsAopProxy(ctx["DbProvider"]), Is.False);
            Assert.That(AopUtils.IsAopProxy(ctx["SessionFactory"]), Is.False);
            Assert.That(AopUtils.IsAopProxy(ctx["hibernateTransactionManager"]), Is.False);
            Assert.That(AopUtils.IsAopProxy(ctx["transactionManager"]), Is.False);
            //Assert.That(AopUtils.IsAopProxy(ctx["testObjectDaoTransProxy"]), Is.True);
            Assert.That(AopUtils.IsAopProxy(ctx["TestObjectDao"]), Is.True);
            Assert.That(AopUtils.IsAopProxy(ctx["SimpleService"]), Is.True);

            CallCountingTransactionManager ccm = ctx["transactionManager"] as CallCountingTransactionManager;
            Assert.That(ccm, Is.Not.Null);
            Assert.That(ccm.begun, Is.EqualTo(0));
            Assert.That(ccm.commits, Is.EqualTo(0));

            LoggingAroundAdvice caa = ctx["loggingAroundAdvice"] as LoggingAroundAdvice;
            Assert.That(caa, Is.Not.Null);
            Assert.That(caa.numInvoked, Is.EqualTo(0));

            ISimpleService simpleService = ctx["SimpleService"] as ISimpleService;
            Assert.That(simpleService, Is.Not.Null);
            simpleService.DoWork(new TestObject());
            Assert.That(ccm.begun, Is.EqualTo(1));
            Assert.That(ccm.commits, Is.EqualTo(1));
            Assert.That(caa.numInvoked, Is.EqualTo(1));
        }
    }
}
