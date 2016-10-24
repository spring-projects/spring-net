#region License

/*
 * Copyright 2005 the original author or authors.
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
using Spring.Core;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Xml;

namespace Spring.Context.Support
{
    /// <summary>
    /// Test creation of application context from XML.
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public sealed class XmlApplicationContextTests
    {
        [Test(Description = "http://jira.springframework.org/browse/SPRNET-1231")]
        public void SPRNET1231_DoesNotInvokeFactoryMethodDuringObjectFactoryPostProcessing()
        {
            string configLocation = TestResourceLoader.GetAssemblyResourceUri(this.GetType(), "XmlApplicationContextTests-SPRNET1231.xml");
            XmlApplicationContext ctx = new XmlApplicationContext(configLocation);

        }

        private class SPRNET1231ObjectFactoryPostProcessor : IObjectFactoryPostProcessor
        {
            public void PostProcessObjectFactory(IConfigurableListableObjectFactory factory)
            {
                SPRNET1231FactoryObject testFactory = (SPRNET1231FactoryObject)factory.GetObject("testFactory");
                Assert.AreEqual(0, testFactory.count);
            }
        }

        private class SPRNET1231FactoryObject
        {
            public int count;

            public ITestObject GetProduct()
            {
                count++;
                return new TestObject("test" + count, count);
            }
        }


        [Test]
        public void InnerObjectWithPostProcessing()
        {
           try
            {
                XmlApplicationContext ctx = new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/innerObjectsWithPostProcessor.xml");           
                ctx.GetObject("hasInnerObjects");
                Assert.Fail("should throw ObjectCreationException");
            }
            catch (ObjectCreationException e)
            {
                NoSuchObjectDefinitionException ex = e.InnerException as NoSuchObjectDefinitionException;
                Assert.IsNotNull(ex);
                //Pass   
            }
        }

        [Test]
        public void NoConfigLocation()
        {
           Assert.Throws<ArgumentException>(() =>  new XmlApplicationContext());
        }

        [Test]
        public void SingleConfigLocation()
        {
            XmlApplicationContext ctx =
                new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/simpleContext.xml");
            Assert.IsTrue(ctx.ContainsObject("someMessageSource"));
            ctx.Dispose();
        }

        [Test]
        public void MultipleConfigLocations()
        {
            XmlApplicationContext ctx =
                new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/contextB.xml",
                                          "assembly://Spring.Core.Tests/Spring.Context.Support/contextC.xml",
                                          "assembly://Spring.Core.Tests/Spring.Context.Support/contextA.xml");
            Assert.IsTrue(ctx.ContainsObject("service"));
            Assert.IsTrue(ctx.ContainsObject("logicOne"));
            Assert.IsTrue(ctx.ContainsObject("logicTwo"));
            Service service = (Service) ctx.GetObject("service");
            ctx.Refresh();
            Assert.IsTrue(service.ProperlyDestroyed);
            service = (Service) ctx.GetObject("service");
            ctx.Dispose();
            Assert.IsTrue(service.ProperlyDestroyed);
        }

        [Test]
        public void ContextWithInvalidValueType()
        {
            try
            {
                XmlApplicationContext ctx =
                    new XmlApplicationContext(false,
                                              "assembly://Spring.Core.Tests/Spring.Context.Support/invalidValueType.xml");
                Assert.Fail("Should have thrown ObjectCreationException for context", ctx);
            }
            catch (ObjectCreationException ex)
            {
                Assert.IsTrue(ex.Message.IndexOf((typeof (TypeMismatchException).Name)) != -1);
                Assert.IsTrue(ex.Message.IndexOf(("UseCodeAsDefaultMessage")) != -1);
            }
        }

        [Test]
        [Ignore("Need to add Spring.TypeLoadException")]
        public void ContextWithInvalidLazyType()
        {
            XmlApplicationContext ctx =
                new XmlApplicationContext(false,
                                          "assembly://Spring.Core.Tests/Spring.Context.Support/invalidType.xml");
            Assert.IsTrue(ctx.ContainsObject("someMessageSource"));
            ctx.GetObject("someMessageSource");
        }

        [Test]
        public void CaseInsensitiveContext()
        {
            XmlApplicationContext ctx =
                new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml");
            Assert.IsTrue(ctx.ContainsObject("goran"));
            Assert.IsTrue(ctx.ContainsObject("Goran"));
            Assert.IsTrue(ctx.ContainsObject("GORAN"));
            Assert.AreEqual(ctx.GetObject("goran"), ctx.GetObject("GORAN"));
        }
        
        [Test]
        public void GetObjectOnUnknownIdThrowsNoSuchObjectDefinition()
        {
            XmlApplicationContext ctx =
                new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml");
            string DOES_NOT_EXIST = "does_not_exist";
            Assert.IsFalse(ctx.ContainsObject(DOES_NOT_EXIST));
            Assert.Throws<NoSuchObjectDefinitionException>(() => ctx.GetObject(DOES_NOT_EXIST));
        }

        [Test]
        public void FactoryObjectsAreNotInstantiatedBeforeObjectFactoryPostProcessorsAreApplied()
        {
            XmlApplicationContext ctx = new XmlApplicationContext("Spring/Context/Support/SPRNET-192.xml");
            LogFactoryObject logFactory = (LogFactoryObject) ctx["&log"];
            Assert.AreEqual("foo", logFactory.LogName);
        }

        /// <summary>
        /// Make sure that if an IObjectPostProcessor is defined as abstract
        /// the creation of an IApplicationContext will not try to instantiate it.
        /// </summary>
        [Test]
        public void ContextWithPostProcessors()
        {
            CountingObjectPostProcessor.Count = 0;
            CoutingObjectFactoryPostProcessor.Count = 0;

            IApplicationContext ctx =
                new XmlApplicationContext("assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml");

            Assert.IsTrue(ctx.ContainsObject("abstractObjectProcessorPrototype"));
            Assert.IsTrue(ctx.ContainsObject("abstractFactoryProcessorPrototype"));

            Assert.AreEqual(0, CountingObjectPostProcessor.Count);
            Assert.AreEqual(0, CoutingObjectFactoryPostProcessor.Count);
        }

        /// <summary>
        /// Make sure that ConfigureObject() completly configures target 
        /// object (goes through whole lifecycle of object creation and 
        /// applies all processors).
        /// </summary>
        [Test]
        public void ConfigureObject()
        {
            const string objDefLocation = "assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml";

            XmlApplicationContext xmlContext = new XmlApplicationContext(new string[] {objDefLocation});

            object objGoran = xmlContext.GetObject("goran");
            Assert.IsTrue(objGoran is TestObject);
            TestObject fooGet = objGoran as TestObject;

            TestObject fooConfigure = new TestObject();
            xmlContext.ConfigureObject(fooConfigure, "goran");
            Assert.IsNotNull(fooGet);
            Assert.AreEqual(fooGet.Name, fooConfigure.Name);
            Assert.AreEqual(fooGet.Age, fooConfigure.Age);
            Assert.AreEqual(fooGet.ObjectName, fooConfigure.ObjectName);
            Assert.IsNotNull(fooGet.ObjectName);
            Assert.AreEqual(xmlContext, fooGet.ApplicationContext);
            Assert.AreEqual(xmlContext, fooConfigure.ApplicationContext);
        }


        [Test]
        public void ContextLifeCycle()
        {
            IApplicationContext ctx =
                new XmlApplicationContext("assembly://Spring.Core.Tests/Spring.Context/contextlifecycle.xml");
            IConfigurableApplicationContext configCtx = ctx as IConfigurableApplicationContext;
            Assert.IsNotNull(configCtx);
            ContextListenerObject clo;
            using (configCtx)
            {
                clo = configCtx["contextListenerObject"] as ContextListenerObject;
                Assert.IsNotNull(clo);
                Assert.IsTrue(clo.AppListenerContextRefreshed,
                              "Object did not receive context refreshed event via IApplicationListener");
                Assert.IsTrue(clo.CtxRefreshed, "Object did not receive context refreshed event via direct wiring");
            }
            Assert.IsTrue(clo.AppListenerContextClosed,
                          "Object did not receive context closed event via IApplicationContextListener");
            Assert.IsTrue(clo.CtxClosed, "Object did not receive context closed event via direct wiring.");
        }

        [Test]
        public void RefreshDisposesExistingObjectFactory_SPRNET479()
        {
            string tmp = typeof (DestroyTester).FullName;
            Console.WriteLine(tmp);

            IApplicationContext ctx =
                new XmlApplicationContext("assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml");

            DestroyTester destroyTester = (DestroyTester) ctx.GetObject("destroyTester");
            DisposeTester disposeTester = (DisposeTester) ctx.GetObject("disposeTester");
            Assert.IsFalse(destroyTester.IsDestroyed);
            Assert.IsFalse(disposeTester.IsDisposed);

            ((IConfigurableApplicationContext) ctx).Refresh();

            Assert.IsTrue(destroyTester.IsDestroyed);
            Assert.IsTrue(disposeTester.IsDisposed);
        }

        [Test]
        public void GenericApplicationContextWithXmlObjectDefinitions()
        {
            GenericApplicationContext ctx = new GenericApplicationContext();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(ctx);
            reader.LoadObjectDefinitions("assembly://Spring.Core.Tests/Spring.Context.Support/contextB.xml");
            reader.LoadObjectDefinitions("assembly://Spring.Core.Tests/Spring.Context.Support/contextC.xml");
            reader.LoadObjectDefinitions("assembly://Spring.Core.Tests/Spring.Context.Support/contextA.xml");
            ctx.Refresh();

            Assert.IsTrue(ctx.ContainsObject("service"));
            Assert.IsTrue(ctx.ContainsObject("logicOne"));
            Assert.IsTrue(ctx.ContainsObject("logicTwo"));
            ctx.Dispose();

        }

        [Test]
        public void GenericApplicationContextConstructorTests()
        {
            IApplicationContext ctx = new XmlApplicationContext("assembly://Spring.Core.Tests/Spring.Context/contextlifecycle.xml");
            GenericApplicationContext genericCtx = new GenericApplicationContext(ctx);
            genericCtx = new GenericApplicationContext("test", true, ctx);
        }

        #region Helper classes

        public class DisposeTester : IDisposable
        {
            private bool isDisposed = false;

            public bool IsDisposed
            {
                get { return isDisposed; }
            }

            public void Dispose()
            {
                if (isDisposed) throw new InvalidOperationException("must not be disposed twice");
                isDisposed = true;
            }
        }

        public class DestroyTester
        {
            private bool isDestroyed = false;

            public bool IsDestroyed
            {
                get { return isDestroyed; }
            }

            public void DestroyMe()
            {
                if (isDestroyed) throw new InvalidOperationException("must not be destroyed twice");
                isDestroyed = true;
            }
        }

        /// <summary>
        /// Utility class to keep track of object construction.
        /// </summary>
        public class CountingObjectPostProcessor : IObjectPostProcessor
        {
            private static int count;

            /// <summary>
            /// Property Count (int)
            /// </summary>
            public static int Count
            {
                get { return count; }
                set { count = value; }
            }

            /// <summary>
            /// Create an instance and increment the counter
            /// </summary>
            public CountingObjectPostProcessor()
            {
                count++;
            }

            #region IObjectPostProcessor Members

            /// <summary>
            /// No op implementation
            /// </summary>
            /// <param name="obj">object to process</param>
            /// <param name="objectName">name of object</param>
            /// <returns>processed object</returns>
            public object PostProcessAfterInitialization(object obj, string objectName)
            {
                return obj;
            }

            /// <summary>
            /// No op implementation
            /// </summary>
            /// <param name="obj">object to process</param>
            /// <param name="name">name of object</param>
            /// <returns>processed object</returns>
            public object PostProcessBeforeInitialization(object obj, string name)
            {
                return obj;
            }

            #endregion
        }


        /// <summary>
        /// Utility class to keep track of object construction.
        /// </summary>
        public class CoutingObjectFactoryPostProcessor : IObjectFactoryPostProcessor
        {
            private static int count;

            /// <summary>
            /// Property Count (int)
            /// </summary>
            public static int Count
            {
                get { return count; }
                set { count = value; }
            }

            /// <summary>
            /// Create an instance and increment the counter
            /// </summary>
            public CoutingObjectFactoryPostProcessor()
            {
                count++;
            }

            #region IObjectFactoryPostProcessor Members

            /// <summary>
            /// no op
            /// </summary>
            /// <param name="factory">factory to post process</param>
            public void PostProcessObjectFactory(IConfigurableListableObjectFactory factory)
            {
            }

            #endregion
        }

        #endregion
    }
    public class SingletonTestingObjectPostProcessor : IObjectPostProcessor, IApplicationContextAware
    {
        private IApplicationContext applicationContext;
        #region IObjectPostProcessor Members

        public object PostProcessBeforeInitialization(object instance, string name)
        {
            return instance;
        }

        public object PostProcessAfterInitialization(object instance, string objectName)
        {
            Console.WriteLine("post process " + objectName);
            if (this.applicationContext.IsSingleton(objectName))
            {
                return instance;
            }
            return instance;
        }

        #endregion

        #region IApplicationContextAware Members

        public IApplicationContext ApplicationContext
        {
            set { this.applicationContext = value; }
        }

        #endregion
    }
}