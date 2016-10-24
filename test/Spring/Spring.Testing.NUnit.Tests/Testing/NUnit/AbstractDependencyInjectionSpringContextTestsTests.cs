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
        private class TestAbstractDependencyInjectionSpringContextTests : AbstractDependencyInjectionSpringContextTests
        {
            public static readonly string[] CONFIGLOCATIONS = new string[]
            {
#if NUNIT_3
                "assembly://Spring.Testing.NUnit3.Tests/Spring.Testing.NUnit/TestApplicationContext.xml"
#else
                "assembly://Spring.Testing.NUnit.Tests/Spring.Testing.NUnit/TestApplicationContext.xml"
#endif
            };

            public TestAbstractDependencyInjectionSpringContextTests()
            {
            }

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

        private TestAbstractDependencyInjectionSpringContextTests fixtureInstance;

        [TearDown]
        public void TearDown()
        {
            AbstractSpringContextTests.ClearContextCache();
        }

        [Test]
        public void RegistersWithContextRegistryByDefault()
        {
            fixtureInstance = new TestAbstractDependencyInjectionSpringContextTests();
            Assert.IsTrue(fixtureInstance.RegisterContextWithContextRegistry);
        }

        [Test]
        public void UnregistersFromContextRegistryWhenDirty()
        {
            fixtureInstance = new TestAbstractDependencyInjectionSpringContextTests();
            Assert.IsTrue(fixtureInstance.RegisterContextWithContextRegistry);
            fixtureInstance.SetUp();
            Assert.IsTrue(ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name));
            fixtureInstance.TearDown();
            Assert.IsTrue(ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name));
            fixtureInstance.SetUp();
            Assert.IsTrue(ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name));
            fixtureInstance.SetDirty();
            fixtureInstance.TearDown();
            Assert.IsFalse(ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name));
        }

        [Test]
        public void DoesNotRegisterContextWithContextRegistry()
        {
            fixtureInstance = new TestAbstractDependencyInjectionSpringContextTests();
            Assert.IsTrue(fixtureInstance.RegisterContextWithContextRegistry);
            fixtureInstance.RegisterContextWithContextRegistry = false;
            fixtureInstance.SetUp();
            Assert.IsFalse(ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name));
            fixtureInstance.TearDown();
            Assert.IsFalse(ContextRegistry.IsContextRegistered(fixtureInstance.ApplicationContext.Name));
        }

        [Test]
        public void CachesApplicationContexts()
        {
            fixtureInstance = new TestAbstractDependencyInjectionSpringContextTests(false);
            fixtureInstance.SetUp();
            Assert.IsNotNull(fixtureInstance.ApplicationContext);
            Assert.AreEqual(1, fixtureInstance.LoadCount); // context has been loaded
            Assert.IsTrue(fixtureInstance.HasCachedContext(TestAbstractDependencyInjectionSpringContextTests.CONFIGLOCATIONS));
            fixtureInstance.TearDown();

            TestAbstractDependencyInjectionSpringContextTests otherFixtureInstance = new TestAbstractDependencyInjectionSpringContextTests(false);
            otherFixtureInstance.SetUp();
            Assert.IsNotNull(otherFixtureInstance.ApplicationContext);
            Assert.AreEqual(0, otherFixtureInstance.LoadCount); // context was obtained from cache
            Assert.AreSame(fixtureInstance.ApplicationContext, otherFixtureInstance.ApplicationContext);
            otherFixtureInstance.SetDirty(); // purge cache and dispose cached instances
            Assert.IsFalse(fixtureInstance.HasCachedContext(TestAbstractDependencyInjectionSpringContextTests.CONFIGLOCATIONS));
            otherFixtureInstance.TearDown();

            otherFixtureInstance = new TestAbstractDependencyInjectionSpringContextTests(false);
            otherFixtureInstance.SetUp();
            Assert.IsNotNull(otherFixtureInstance.ApplicationContext);
            Assert.AreEqual(1, otherFixtureInstance.LoadCount); // context was reloaded because of SetDirty() above
            Assert.AreNotSame(fixtureInstance.ApplicationContext, otherFixtureInstance.ApplicationContext);
            otherFixtureInstance.TearDown();
        }

        private class TestAbstractDependencyInjectionSpringContextTestsExceptions : AbstractDependencyInjectionSpringContextTests
        {
            private static readonly string[] CONFIGLOCATIONS = new string[]
            {
                "assembly://Spring.Testing.NUnit.Tests/Spring.Testing.NUnit/TestApplicationContext.xml"
            };

            public TestAbstractDependencyInjectionSpringContextTestsExceptions()
            {
            }

            protected override string[] ConfigLocations
            {
                get { return CONFIGLOCATIONS; }
            }

            protected override void OnSetUp()
            {
                throw new Exception("SetUp Exception");
            }

            protected override void OnTearDown()
            {
                throw new Exception("TearDown Exception");
            }
        }

       [Test]
        public void ThrowsSetUpException()
        {
            var testFixture = new TestAbstractDependencyInjectionSpringContextTestsExceptions();
            Assert.Throws<Exception>(() => testFixture.SetUp());
        }

        [Test]
        public void ThrowsTearDownException()
        {
            var testFixture = new TestAbstractDependencyInjectionSpringContextTestsExceptions();
            Assert.Throws<Exception>(() => testFixture.TearDown());
        }
    }
}