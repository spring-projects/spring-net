#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using Spring.Aop.Config;
using Spring.Context;
using Spring.Context.Support;
using Spring.Core.IO;
using Spring.Objects;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Transaction.Interceptor;

#endregion

namespace Spring.Transaction.Config
{
    [TestFixture]
    public class TxNamespaceParserTests
    {
        private class ResourceXmlApplicationContext : AbstractXmlApplicationContext
        {
            private readonly IResource[] configurationResources;
            public ResourceXmlApplicationContext(params IResource[] configurationResources) 
                : base()
            {
                this.configurationResources = configurationResources;
            }

            protected override void  LoadObjectDefinitions(XmlObjectDefinitionReader objectDefinitionReader)
            {
                 base.LoadObjectDefinitions(objectDefinitionReader);
                objectDefinitionReader.LoadObjectDefinitions(configurationResources);
            }

            protected override string[] ConfigurationLocations
            {
                get { return null; }
            }

            protected override IResource[] ConfigurationResources
            {
                get { return null; }
            }
        }

        private const string APPCTXCFG_PROLOG = @"<?xml version='1.0' encoding='utf-8' ?>";
        private const string APPCTXCFG_START = APPCTXCFG_PROLOG + @"<objects xmlns='http://www.springframework.net' xmlns:tx='http://www.springframework.net/tx'>";
        private const string APPCTXCFG_END = @"</objects>";

        private IApplicationContext ctx;

        [SetUp]
        public void SetUp()
        {
            //WELLKNOWN: NamespaceParserRegistry.RegisterParser(typeof(TxNamespaceParser));
            //WELLKNOWN: NamespaceParserRegistry.RegisterParser(typeof(AopNamespaceParser));
            ctx = new XmlApplicationContext("assembly://Spring.Data.Tests/Spring.Transaction.Config/TxNamespaceParserTests.xml");
        }

        // TODO (EE)
        [Test]
        public void AppliesTxAttributeDrivenAttributes()
        {
            StringResource appCtxCfg = new StringResource(
                APPCTXCFG_START 
                + "<tx:attribute-driven transaction-manager='otherTxManager' proxy-target-type='true' order='2' />" 
                + APPCTXCFG_END);

            IApplicationContext appCtx = new ResourceXmlApplicationContext(appCtxCfg);
//            DefaultAdvisorAutoProxyCreator daapc = (DefaultAdvisorAutoProxyCreator) appCtx.GetObject(AopNamespaceUtils.AUTO_PROXY_CREATOR_OBJECT_NAME);
//            Assert.AreEqual(2, daapc.Order);
        }

        [Test]
        public void Registered()
        {
            Assert.IsNotNull(NamespaceParserRegistry.GetParser("http://www.springframework.net/tx"));
            Assert.IsTrue(ctx.ContainsObjectDefinition(AopNamespaceUtils.AUTO_PROXY_CREATOR_OBJECT_NAME));

            string className = typeof(ObjectFactoryTransactionAttributeSourceAdvisor).FullName;
            string targetName = className + ObjectDefinitionReaderUtils.GENERATED_OBJECT_NAME_SEPARATOR + "0";

            Assert.IsTrue(ctx.ContainsObjectDefinition(targetName));
        }


        [Test]
        public void InvokeTransactional()
        {
            ITestObject testObject = TestObject;
            CallCountingTransactionManager ptm = ctx["transactionManager"] as CallCountingTransactionManager;
            Assert.IsNotNull(ptm);

            // try with transactional
            Assert.AreEqual(0, ptm.begun,"Should not have started any transactions");
            testObject.GetDescription();
            Assert.AreEqual(1, ptm.begun, "Should have 1 started transaction");
            Assert.AreEqual(1, ptm.commits, "Should have 1 committed transaction");

            // try with non-transaction
            int i = testObject.Age;
            Assert.IsNotNull(i);
            Assert.AreEqual(1, ptm.begun, "Should not have started another transaction");

            // try with exceptional
            try
            {
                testObject.Exceptional(new ArgumentNullException());
                Assert.Fail("Should not get here");
            } catch (Exception)
            {
                Assert.AreEqual(2, ptm.begun, "Should have another started transaction");
                Assert.AreEqual(1, ptm.rollbacks, "Should have 1 rolled back transaction");
            }
        }

        private ITestObject TestObject
        {
            get
            {
                return ctx["testObject"] as
                       ITestObject;
            }
        }

        [Test]
        public void RollbackRules()
        {
            TransactionInterceptor txInterceptor = ctx.GetObject("txRollbackAdvice") as TransactionInterceptor;
            Assert.IsNotNull(txInterceptor);

            MethodInfo getDescriptionMethod = typeof(ITestObject).GetMethod("GetDescription");
            MethodInfo exceptionalMethod = typeof (ITestObject).GetMethod("Exceptional");
            ITransactionAttributeSource txAttrSource = txInterceptor.TransactionAttributeSource;
            ITransactionAttribute txAttr = txAttrSource.ReturnTransactionAttribute(getDescriptionMethod, typeof (ITestObject));
            Assert.IsTrue(txAttr.RollbackOn(new System.ApplicationException()));

            txAttr = txAttrSource.ReturnTransactionAttribute(exceptionalMethod, typeof (ITestObject));
            Assert.IsFalse(txAttr.RollbackOn(new System.ArithmeticException()));
        }
    }
}
