using System;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;

namespace Spring.Testing.NUnit
{
	/// <summary>
	/// Summary description for AbstractDependencyInjectionSpringContextTestsTests.
	/// </summary>
	[TestFixture]
	public class AbstractDependencyInjectionSpringContextTestsTests
	{
        private class TestAbstractDependencyInjectionSpringContextTests :AbstractDependencyInjectionSpringContextTests
        {
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
                get { return new string[] {"assembly://Spring.Testing.NUnit.Tests/Spring.Testing.NUnit/TestApplicationContext.xml"}; }
            }
        }

        private TestAbstractDependencyInjectionSpringContextTests fixtureInstance ;

        [TearDown]
        public void TearDown()
        {
            AbstractSpringContextTests.ClearContextCache();            
        }

	    [Test]
        public void RegistersAndUnregistersWithContextRegistryByDefault()
	    {
	        fixtureInstance = new TestAbstractDependencyInjectionSpringContextTests();
            Assert.IsTrue(fixtureInstance.RegisterContextWithContextRegistry);
            fixtureInstance.SetUp();
            Assert.IsTrue( ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name) );
            fixtureInstance.TearDown();
            Assert.IsFalse( ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name) );
	    }

	    [Test]
        public void DoesNotRegisterContextWithContextRegistry()
	    {
	        fixtureInstance = new TestAbstractDependencyInjectionSpringContextTests();
            Assert.IsTrue(fixtureInstance.RegisterContextWithContextRegistry);
            fixtureInstance.RegisterContextWithContextRegistry = false;
            fixtureInstance.SetUp();
            Assert.IsFalse( ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name) );
            fixtureInstance.TearDown();
            Assert.IsFalse( ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name) );
	    }


        [Test]
        public void CachesApplicationContexts()
        {
	        fixtureInstance = new TestAbstractDependencyInjectionSpringContextTests(false);
            fixtureInstance.SetUp();            
            Assert.IsNotNull(fixtureInstance.ApplicationContext);
            Assert.AreEqual(1, fixtureInstance.LoadCount); // context has been loaded
            fixtureInstance.TearDown();
            
            TestAbstractDependencyInjectionSpringContextTests otherFixtureInstance = new TestAbstractDependencyInjectionSpringContextTests(false);
            otherFixtureInstance.SetUp();            
            Assert.IsNotNull(otherFixtureInstance.ApplicationContext);
            Assert.AreEqual(0, otherFixtureInstance.LoadCount); // context was obtained from cache
            Assert.AreSame(fixtureInstance.ApplicationContext, otherFixtureInstance.ApplicationContext);
            otherFixtureInstance.SetDirty(); // dispose
            otherFixtureInstance.TearDown();
            
            otherFixtureInstance = new TestAbstractDependencyInjectionSpringContextTests(false);
            otherFixtureInstance.SetUp();            
            Assert.IsNotNull(otherFixtureInstance.ApplicationContext);
            Assert.AreEqual(1, otherFixtureInstance.LoadCount); // context was reloaded because of SetDirty() above
            Assert.AreNotSame(fixtureInstance.ApplicationContext, otherFixtureInstance.ApplicationContext);
            otherFixtureInstance.TearDown();
        }
	}
}
