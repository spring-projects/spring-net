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

using System;
using System.Configuration;
using System.Reflection;
using System.Xml;

using NUnit.Framework;

using Spring.Objects;
using Spring.Proxy;
using Spring.Objects.Factory.Support;
using Spring.Util;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    /// Unit tests for the ContextRegistry class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class ContextRegistryTests
    {
        [SetUp]
        public void SetUp()
        {
            ContextRegistry.Clear();
            ResetConfigurationSystem();
        }

        private static void ResetConfigurationSystem()
        {
            if (SystemUtils.MonoRuntime)
            {
                return;
            }
            FieldInfo initStateRef = typeof(ConfigurationManager).GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static);
            object notStarted = Activator.CreateInstance(initStateRef.FieldType);
            initStateRef.SetValue(null, notStarted);
        }

        /// <summary>
        /// This handler simulates an undefined configuration section
        /// </summary>
        private static object GetNullSection(object parent, object context, XmlNode section)
        {
            return null;
        }

        /// <summary>
        /// This handler simulates calls to ContextRegistry during context creation
        /// </summary>
        private static object GetContextRecursive(object parent, object context, XmlNode section)
        {
            return ContextRegistry.GetContext(); // this must fail!
        }

        [Test]
        public void ThrowsInvalidOperationExceptionOnRecursiveCallsToGetContext()
        {
            using (new HookableContextHandler.Guard(GetContextRecursive))
            {
                try
                {
                    ContextRegistry.GetContext("somename");
                    Assert.Fail("Should throw an exception");
                }
                catch (ConfigurationException ex)
                {
                    InvalidOperationException rootCause = ex.GetBaseException() as InvalidOperationException;
                    Assert.IsNotNull(rootCause);
                    Assert.AreEqual("root context is currently in creation.", rootCause.Message.Substring(0, 38));
                }
            }
        }

        [Test]
        public void RegisterRootContext()
        {
            MockApplicationContext ctx = new MockApplicationContext();
            ContextRegistry.RegisterContext(ctx);
            IApplicationContext context = ContextRegistry.GetContext();
            Assert.IsNotNull(context,
                "Root context is null even though a context has been registered.");
            Assert.IsTrue(Object.ReferenceEquals(ctx, context),
                "Root context was not the same as the first context registered (it must be).");
        }

        [Test]
        public void RegisterNamedRootContext()
        {
            const string ctxName = "bingo";
            MockApplicationContext ctx = new MockApplicationContext(ctxName);
            ContextRegistry.RegisterContext(ctx);
            IApplicationContext rootContext = ContextRegistry.GetContext();
            Assert.IsNotNull(rootContext,
                "Root context is null even though a context has been registered.");
            Assert.AreEqual(ctxName, rootContext.Name,
                "Root context name is different even though the root context has been registered under the lookup name.");
        }

        [Test]
        public void RegisterNamedContext()
        {
            const string ctxName = "bingo";
            MockApplicationContext ctx = new MockApplicationContext(ctxName);
            ContextRegistry.RegisterContext(ctx);
            IApplicationContext context = ContextRegistry.GetContext(ctxName);
            Assert.IsNotNull(context,
                "Named context is null even though a context has been registered under the lookup name.");
            Assert.IsTrue(Object.ReferenceEquals(ctx, context),
                "Named context was not the same as the registered context (it must be).");
        }

        [Test]
        public void GetContextWithNullName()
        {
            Assert.Throws<ArgumentException>(() => ContextRegistry.GetContext(null));
        }

        [Test]
        public void GetContextWithEmptyName()
        {
            Assert.Throws<ArgumentException>(() => ContextRegistry.GetContext(""));
        }

        [Test]
        public void GetRootContextNotRegisteredThrowsException()
        {
            Assert.Throws<ApplicationContextException>(() =>
            {
                using (new HookableContextHandler.Guard(GetNullSection))
                {
                    ContextRegistry.GetContext();
                }
            }, "No context registered. Use the 'RegisterContext' method or the 'spring/context' section from your configuration file.");
        }


        [Test]
        public void GetContextByNameNotRegisteredThrowsException()
        {
            Assert.Throws<ApplicationContextException>(
                () => ContextRegistry.GetContext("bingo"),
                "No context registered under name 'bingo'. Use the 'RegisterContext' method or the 'spring/context' section from your configuration file.");
        }

        [Test]
        public void CanBuildProxyForClassWithProtectedConstructor()
        {
            CompositionProxyTypeBuilder typeBuilder = new CompositionProxyTypeBuilder();
            typeBuilder.TargetType = typeof(ClassWithProtectedCtor);
            typeBuilder.BuildProxyType();            
        }

        [Test]
        public void ClearWithDynamicProxies()
        {
            CompositionProxyTypeBuilder typeBuilder = new CompositionProxyTypeBuilder();
            typeBuilder.TargetType = typeof(TestObject);
            Type proxyType = typeBuilder.BuildProxyType();

            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            RootObjectDefinition od1 = new RootObjectDefinition(proxyType, false);
            od1.PropertyValues.Add("Name", "Bruno");
            of.RegisterObjectDefinition("testObject", od1);

            GenericApplicationContext ctx1 = new GenericApplicationContext(of);
            ContextRegistry.RegisterContext(ctx1);

            ITestObject to1 = ContextRegistry.GetContext().GetObject("testObject") as ITestObject;
            Assert.IsNotNull(to1);
            Assert.AreEqual("Bruno", to1.Name);

            DefaultListableObjectFactory of2 = new DefaultListableObjectFactory();
            RootObjectDefinition od2 = new RootObjectDefinition(proxyType, false);
            od2.PropertyValues.Add("Name", "Baia");
            of2.RegisterObjectDefinition("testObject", od2);
            GenericApplicationContext ctx2 = new GenericApplicationContext(of2);

            ContextRegistry.Clear();

            ITestObject to2 = ctx2.GetObject("testObject") as ITestObject;
            Assert.IsNotNull(to2);
            Assert.AreEqual("Baia", to2.Name);
        }

        // TODO : Add support for .NET 1.x
        [Test]
        public void ClearWithConfigurationSection()
        {
            IApplicationContext ctx1 = ContextRegistry.GetContext();
            ContextRegistry.Clear();
            IApplicationContext ctx2 = ContextRegistry.GetContext();

            Assert.AreNotSame(ctx1, ctx2);
        }

        [Test(Description = "SPRNET-105")]
        public void ChokesIfChildContextRegisteredUnderNameOfAnExistingContext()
        {
            MockApplicationContext original = new MockApplicationContext("original");
            ContextRegistry.RegisterContext(original);
            MockApplicationContext duplicate = new MockApplicationContext("original");
            Assert.Throws<ApplicationContextException>(() => ContextRegistry.RegisterContext(duplicate));
        }

        
        [Test]
        public void RemovesContextFromRegistryWhenContextCloses()
        {
            StaticApplicationContext appCtx = new StaticApplicationContext();
            appCtx.Name = "myCtx";
            ContextRegistry.RegisterContext(appCtx);
            Assert.IsTrue(ContextRegistry.IsContextRegistered(appCtx.Name));
            appCtx.Dispose();
            Assert.IsFalse(ContextRegistry.IsContextRegistered(appCtx.Name));
        }

        [TestFixture]
        public class WhenHierarchicalContextsAllHaveDefaultNames
        {
            private MockApplicationContext _parentContext;
            private MockApplicationContext _childContext;
            private MockApplicationContext _grandChildContext;
            private MockApplicationContext _greatGrandChildContext;

            private string _expectedParentName;
            private string _expectedChildName;
            private string _expectedGrandChildName;
            private string _expectedGreatGrandChildName;


            [OneTimeSetUp]
            public void InitializeAllTests()
            {
                _expectedParentName = AbstractApplicationContext.DefaultRootContextName;
                _expectedChildName = string.Format("{0}/{1}", _expectedParentName, AbstractApplicationContext.DefaultRootContextName);
                _expectedGrandChildName = string.Format("{0}/{1}/{2}",_expectedParentName, _expectedChildName, AbstractApplicationContext.DefaultRootContextName);
                _expectedGreatGrandChildName = string.Format("{0}/{1}/{2}/{3}",_expectedParentName, _expectedChildName, _expectedGrandChildName, AbstractApplicationContext.DefaultRootContextName);
            }

            [SetUp]
            public void Setup()
            {
                //ensure prior-registered contexts are removed
                ContextRegistry.Clear();

                _parentContext = new MockApplicationContext();
                _parentContext.MockName = "parent";

                _childContext = new MockApplicationContext(_parentContext);
                _childContext.MockName = "child";
                _childContext.ParentContext = _parentContext;

                _grandChildContext = new MockApplicationContext(_childContext);
                _grandChildContext.MockName = "grandchild";
                _grandChildContext.ParentContext = _childContext;

                _greatGrandChildContext = new MockApplicationContext(_grandChildContext);
                _greatGrandChildContext.MockName = "greatgrandchild";
                _greatGrandChildContext.ParentContext = _grandChildContext;
            }


            [Test]
            public void RegisterContext_ConstructsNestedPathBasedNames_IfRegisterdInHierarchicalOrder()
            {
                ContextRegistry.RegisterContext(_parentContext);
                ContextRegistry.RegisterContext(_childContext);
                ContextRegistry.RegisterContext(_grandChildContext);
                ContextRegistry.RegisterContext(_greatGrandChildContext);

                Assert.AreEqual(_expectedParentName, ContextRegistry.GetContext().Name);
                Assert.AreEqual(_expectedChildName, ContextRegistry.GetContext(_expectedChildName).Name);
                Assert.AreEqual(_expectedGrandChildName, ContextRegistry.GetContext(_expectedGrandChildName).Name);
                Assert.AreEqual(_expectedGreatGrandChildName, ContextRegistry.GetContext(_expectedGreatGrandChildName).Name);
            }

            [Test]
            public void RegisterContext_ConstructsNestedPathBasedNames_IfRegisteringAMixOfDefaultAndExplicitNamedContexts()
            {
                //modify the expected names for the decendent contexts for this one test
                string childContextInitialName = AbstractApplicationContext.DefaultRootContextName + "_CUSTOM";
                _expectedChildName = string.Format("{0}/{1}", _expectedParentName, childContextInitialName);
                _expectedGrandChildName = string.Format("{0}/{1}/{2}", _expectedParentName, _expectedChildName, AbstractApplicationContext.DefaultRootContextName);
                _expectedGreatGrandChildName = string.Format("{0}/{1}/{2}/{3}", _expectedParentName, _expectedChildName, _expectedGrandChildName, AbstractApplicationContext.DefaultRootContextName);

                //setup custom child instance for this one test
                _childContext = new MockApplicationContext(_expectedChildName);
                _childContext.MockName = "child";
                _childContext.ParentContext = _parentContext;
                _grandChildContext.ParentContext = _childContext;

                //register contexts in conflict with hierarchical order
                ContextRegistry.RegisterContext(_parentContext);
                ContextRegistry.RegisterContext(_childContext);
                ContextRegistry.RegisterContext(_grandChildContext);
                ContextRegistry.RegisterContext(_greatGrandChildContext);


                Assert.AreEqual(_expectedParentName, ContextRegistry.GetContext(_expectedParentName).Name);
                Assert.AreEqual(_expectedChildName, ContextRegistry.GetContext(_expectedChildName).Name);
                Assert.AreEqual(_expectedGrandChildName, ContextRegistry.GetContext(_expectedGrandChildName).Name);
                Assert.AreEqual(_expectedGreatGrandChildName, ContextRegistry.GetContext(_expectedGreatGrandChildName).Name);
            }
        }

    }
}
