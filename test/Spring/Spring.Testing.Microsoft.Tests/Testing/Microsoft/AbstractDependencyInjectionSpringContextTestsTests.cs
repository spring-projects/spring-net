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

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Spring.Context;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Transaction;

namespace Spring.Testing.Microsoft
{
	/// <summary>
	/// Summary description for AbstractDependencyInjectionSpringContextTestsTests.
	/// </summary>
	[TestClass]
	public class AbstractDependencyInjectionSpringContextTestsTests
	{
        //Force loading of this assembly since MsTest runner doesn't run in the 'working' directory and find
        //dynamically loaded assemblies... (you have *got* to be kidding!!!)
        //Investigate use of [DeploymentItem(...)] 
	    private Type t = typeof (CallCountingTransactionManager);
	    private Type t2 = typeof (TestObject);

        private class TestAbstractDependencyInjectionSpringContextTests : AbstractDependencyInjectionSpringContextTests
        {
            public static readonly string[] CONFIGLOCATIONS = new string[] {"assembly://Spring.Testing.Microsoft.Tests/Spring.Testing.Microsoft/TestApplicationContext.xml"};

            public TestAbstractDependencyInjectionSpringContextTests()
            {}

            public TestAbstractDependencyInjectionSpringContextTests(bool registerWithContextRegistry)
            {
                base.RegisterContextWithContextRegistry = registerWithContextRegistry;
            }

            public IConfigurableApplicationContext ApplicationContext
            {
                get { return base.applicationContext; }
            }

            protected override string[] ConfigLocations
            {
                get { return CONFIGLOCATIONS; }
            }

            public new bool HasCachedContext(object key)
            {
                return base.HasCachedContext(key);
            }            
        }

        private TestAbstractDependencyInjectionSpringContextTests fixtureInstance ;

        [TestCleanup]
        public void TearDown()
        {
            AbstractSpringContextTests.ClearContextCache();            
        }

	    [TestMethod]
        public void RegistersWithContextRegistryByDefault()
	    {
	        fixtureInstance = new TestAbstractDependencyInjectionSpringContextTests();
            Assert.IsTrue(fixtureInstance.RegisterContextWithContextRegistry);
            Assert.AreEqual(typeof(CallCountingTransactionManager), t);
            Assert.AreEqual(typeof(TestObject), t2);

	    }

	    [TestMethod]
        public void UnregistersFromContextRegistryWhenDirty()
	    {
	        fixtureInstance = new TestAbstractDependencyInjectionSpringContextTests();
            Assert.IsTrue(fixtureInstance.RegisterContextWithContextRegistry);
            fixtureInstance.TestInitialize();
            Assert.IsTrue( ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name) );
            fixtureInstance.TestCleanup();
            Assert.IsTrue(ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name));
            fixtureInstance.TestInitialize();
            Assert.IsTrue(ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name));
            fixtureInstance.SetDirty();
            fixtureInstance.TestCleanup();
            Assert.IsFalse(ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name));
	    }

	    [TestMethod]
        public void DoesNotRegisterContextWithContextRegistry()
	    {
	        fixtureInstance = new TestAbstractDependencyInjectionSpringContextTests();
            Assert.IsTrue(fixtureInstance.RegisterContextWithContextRegistry);
            fixtureInstance.RegisterContextWithContextRegistry = false;
            fixtureInstance.TestInitialize();
            Assert.IsFalse( ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name) );
            fixtureInstance.TestCleanup();
            Assert.IsFalse( ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name) );
	    }

        [TestMethod]
        public void CachesApplicationContexts()
        {
	        fixtureInstance = new TestAbstractDependencyInjectionSpringContextTests(false);
            fixtureInstance.TestInitialize();        
            Assert.IsNotNull(fixtureInstance.ApplicationContext);
            Assert.AreEqual(1, fixtureInstance.LoadCount); // context has been loaded
            Assert.IsTrue(fixtureInstance.HasCachedContext(TestAbstractDependencyInjectionSpringContextTests.CONFIGLOCATIONS));
            fixtureInstance.TestCleanup();
            
            TestAbstractDependencyInjectionSpringContextTests otherFixtureInstance = new TestAbstractDependencyInjectionSpringContextTests(false);
            otherFixtureInstance.TestInitialize();            
            Assert.IsNotNull(otherFixtureInstance.ApplicationContext);
            Assert.AreEqual(0, otherFixtureInstance.LoadCount); // context was obtained from cache
            Assert.AreSame(fixtureInstance.ApplicationContext, otherFixtureInstance.ApplicationContext);
            otherFixtureInstance.SetDirty(); // purge cache and dispose cached instances
            Assert.IsFalse(fixtureInstance.HasCachedContext(TestAbstractDependencyInjectionSpringContextTests.CONFIGLOCATIONS));
            otherFixtureInstance.TestCleanup();
            otherFixtureInstance = new TestAbstractDependencyInjectionSpringContextTests(false);
            otherFixtureInstance.TestInitialize();          
            Assert.IsNotNull(otherFixtureInstance.ApplicationContext);
            Assert.AreEqual(1, otherFixtureInstance.LoadCount); // context was reloaded because of SetDirty() above
            Assert.AreNotSame(fixtureInstance.ApplicationContext, otherFixtureInstance.ApplicationContext);
            otherFixtureInstance.TestCleanup();
        }
	}
}
