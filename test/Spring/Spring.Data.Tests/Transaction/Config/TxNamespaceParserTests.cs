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

namespace Spring.Transaction.Config;

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

        protected override void LoadObjectDefinitions(XmlObjectDefinitionReader objectDefinitionReader)
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
//            Assert.That(daapc.Order, Is.EqualTo(2));
    }

    [Test]
    public void Registered()
    {
        Assert.That(NamespaceParserRegistry.GetParser("http://www.springframework.net/tx"), Is.Not.Null);
        Assert.That(ctx.ContainsObjectDefinition(AopNamespaceUtils.AUTO_PROXY_CREATOR_OBJECT_NAME), Is.True);

        string className = typeof(ObjectFactoryTransactionAttributeSourceAdvisor).FullName;
        string targetName = className + ObjectDefinitionReaderUtils.GENERATED_OBJECT_NAME_SEPARATOR + "0";

        Assert.That(ctx.ContainsObjectDefinition(targetName), Is.True);
    }

    [Test]
    public void InvokeTransactional()
    {
        ITestObject testObject = TestObject;
        CallCountingTransactionManager ptm = ctx["transactionManager"] as CallCountingTransactionManager;
        Assert.That(ptm, Is.Not.Null);

        // try with transactional
        Assert.That(ptm.begun, Is.EqualTo(0), "Should not have started any transactions");
        testObject.GetDescription();
        Assert.That(ptm.begun, Is.EqualTo(1), "Should have 1 started transaction");
        Assert.That(ptm.commits, Is.EqualTo(1), "Should have 1 committed transaction");

        // try with non-transaction
        int i = testObject.Age;
        Assert.That((object) i, Is.Not.Null);
        Assert.That(ptm.begun, Is.EqualTo(1), "Should not have started another transaction");

        // try with exceptional
        try
        {
            testObject.Exceptional(new ArgumentNullException());
            Assert.Fail("Should not get here");
        }
        catch (Exception)
        {
            Assert.That(ptm.begun, Is.EqualTo(2), "Should have another started transaction");
            Assert.That(ptm.rollbacks, Is.EqualTo(1), "Should have 1 rolled back transaction");
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
        Assert.That(txInterceptor, Is.Not.Null);

        MethodInfo getDescriptionMethod = typeof(ITestObject).GetMethod("GetDescription");
        MethodInfo exceptionalMethod = typeof(ITestObject).GetMethod("Exceptional");
        ITransactionAttributeSource txAttrSource = txInterceptor.TransactionAttributeSource;
        ITransactionAttribute txAttr = txAttrSource.ReturnTransactionAttribute(getDescriptionMethod, typeof(ITestObject));
        Assert.That(txAttr.RollbackOn(new System.ApplicationException()), Is.True);

        txAttr = txAttrSource.ReturnTransactionAttribute(exceptionalMethod, typeof(ITestObject));
        Assert.That(txAttr.RollbackOn(new System.ArithmeticException()), Is.False);
    }
}
