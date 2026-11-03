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

using NUnit.Framework;
using Spring.Core;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Xml;

namespace Spring.Context.Support;

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
            SPRNET1231FactoryObject testFactory = (SPRNET1231FactoryObject) factory.GetObject("testFactory");
            Assert.That(testFactory.count, Is.EqualTo(0));
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
            Assert.That(ex, Is.Not.Null);
            //Pass
        }
    }

    [Test]
    public void NoConfigLocation()
    {
        Assert.Throws<ArgumentException>(() => new XmlApplicationContext());
    }

    [Test]
    public void SingleConfigLocation()
    {
        XmlApplicationContext ctx =
            new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/simpleContext.xml");
        Assert.That(ctx.ContainsObject("someMessageSource"), Is.True);
        ctx.Dispose();
    }

    [Test]
    public void MultipleConfigLocations()
    {
        XmlApplicationContext ctx =
            new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/contextB.xml",
                "assembly://Spring.Core.Tests/Spring.Context.Support/contextC.xml",
                "assembly://Spring.Core.Tests/Spring.Context.Support/contextA.xml");
        Assert.That(ctx.ContainsObject("service"), Is.True);
        Assert.That(ctx.ContainsObject("logicOne"), Is.True);
        Assert.That(ctx.ContainsObject("logicTwo"), Is.True);
        Service service = (Service) ctx.GetObject("service");
        ctx.Refresh();
        Assert.That(service.ProperlyDestroyed, Is.True);
        service = (Service) ctx.GetObject("service");
        ctx.Dispose();
        Assert.That(service.ProperlyDestroyed, Is.True);
    }

    [Test]
    public void ContextWithInvalidValueType()
    {
        try
        {
            XmlApplicationContext ctx =
                new XmlApplicationContext(false,
                    "assembly://Spring.Core.Tests/Spring.Context.Support/invalidValueType.xml");
            Assert.Fail("Should have thrown ObjectCreationException for context");
        }
        catch (ObjectCreationException ex)
        {
            Assert.That(ex.Message.IndexOf((typeof(TypeMismatchException).Name)) != -1, Is.True);
            Assert.That(ex.Message.IndexOf(("UseCodeAsDefaultMessage")) != -1, Is.True);
        }
    }

    [Test]
    [Ignore("Need to add Spring.TypeLoadException")]
    public void ContextWithInvalidLazyType()
    {
        XmlApplicationContext ctx =
            new XmlApplicationContext(false,
                "assembly://Spring.Core.Tests/Spring.Context.Support/invalidType.xml");
        Assert.That(ctx.ContainsObject("someMessageSource"), Is.True);
        ctx.GetObject("someMessageSource");
    }

    [Test]
    public void CaseInsensitiveContext()
    {
        XmlApplicationContext ctx =
            new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml");
        Assert.That(ctx.ContainsObject("goran"), Is.True);
        Assert.That(ctx.ContainsObject("Goran"), Is.True);
        Assert.That(ctx.ContainsObject("GORAN"), Is.True);
        Assert.That(ctx.GetObject("GORAN"), Is.EqualTo(ctx.GetObject("goran")));
    }

    [Test]
    public void GetObjectOnUnknownIdThrowsNoSuchObjectDefinition()
    {
        XmlApplicationContext ctx =
            new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml");
        string DOES_NOT_EXIST = "does_not_exist";
        Assert.That(ctx.ContainsObject(DOES_NOT_EXIST), Is.False);
        Assert.Throws<NoSuchObjectDefinitionException>(() => ctx.GetObject(DOES_NOT_EXIST));
    }

    [Test]
    public void FactoryObjectsAreNotInstantiatedBeforeObjectFactoryPostProcessorsAreApplied()
    {
        XmlApplicationContext ctx = new XmlApplicationContext("Spring/Context/Support/SPRNET-192.xml");
        LogFactoryObject logFactory = (LogFactoryObject) ctx["&log"];
        Assert.That(logFactory.LogName, Is.EqualTo("foo"));
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

        Assert.That(ctx.ContainsObject("abstractObjectProcessorPrototype"), Is.True);
        Assert.That(ctx.ContainsObject("abstractFactoryProcessorPrototype"), Is.True);

        Assert.That(CountingObjectPostProcessor.Count, Is.EqualTo(0));
        Assert.That(CoutingObjectFactoryPostProcessor.Count, Is.EqualTo(0));
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

        XmlApplicationContext xmlContext = new XmlApplicationContext(new string[] { objDefLocation });

        object objGoran = xmlContext.GetObject("goran");
        Assert.That(objGoran is TestObject, Is.True);
        TestObject fooGet = objGoran as TestObject;

        TestObject fooConfigure = new TestObject();
        xmlContext.ConfigureObject(fooConfigure, "goran");
        Assert.That(fooGet, Is.Not.Null);
        Assert.That(fooConfigure.Name, Is.EqualTo(fooGet.Name));
        Assert.That(fooConfigure.Age, Is.EqualTo(fooGet.Age));
        Assert.That(fooConfigure.ObjectName, Is.EqualTo(fooGet.ObjectName));
        Assert.That(fooGet.ObjectName, Is.Not.Null);
        Assert.That(fooGet.ApplicationContext, Is.EqualTo(xmlContext));
        Assert.That(fooConfigure.ApplicationContext, Is.EqualTo(xmlContext));
    }

    [Test]
    public void ContextLifeCycle()
    {
        IApplicationContext ctx =
            new XmlApplicationContext("assembly://Spring.Core.Tests/Spring.Context/contextlifecycle.xml");
        IConfigurableApplicationContext configCtx = ctx as IConfigurableApplicationContext;
        Assert.That(configCtx, Is.Not.Null);
        ContextListenerObject clo;
        using (configCtx)
        {
            clo = configCtx["contextListenerObject"] as ContextListenerObject;
            Assert.That(clo, Is.Not.Null);
            Assert.That(clo.AppListenerContextRefreshed,
                Is.True,
                "Object did not receive context refreshed event via IApplicationListener");
            Assert.That(clo.CtxRefreshed, Is.True, "Object did not receive context refreshed event via direct wiring");
        }

        Assert.That(clo.AppListenerContextClosed,
            Is.True,
            "Object did not receive context closed event via IApplicationContextListener");
        Assert.That(clo.CtxClosed, Is.True, "Object did not receive context closed event via direct wiring.");
    }

    [Test]
    public void RefreshDisposesExistingObjectFactory_SPRNET479()
    {
        string tmp = typeof(DestroyTester).FullName;
        Console.WriteLine(tmp);

        IApplicationContext ctx =
            new XmlApplicationContext("assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml");

        DestroyTester destroyTester = (DestroyTester) ctx.GetObject("destroyTester");
        DisposeTester disposeTester = (DisposeTester) ctx.GetObject("disposeTester");
        Assert.That(destroyTester.IsDestroyed, Is.False);
        Assert.That(disposeTester.IsDisposed, Is.False);

        ((IConfigurableApplicationContext) ctx).Refresh();

        Assert.That(destroyTester.IsDestroyed, Is.True);
        Assert.That(disposeTester.IsDisposed, Is.True);
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

        Assert.That(ctx.ContainsObject("service"), Is.True);
        Assert.That(ctx.ContainsObject("logicOne"), Is.True);
        Assert.That(ctx.ContainsObject("logicTwo"), Is.True);
        ctx.Dispose();
    }

    [Test]
    public void GenericApplicationContextConstructorTests()
    {
        IApplicationContext ctx = new XmlApplicationContext("assembly://Spring.Core.Tests/Spring.Context/contextlifecycle.xml");
        GenericApplicationContext genericCtx = new GenericApplicationContext(ctx);
        genericCtx = new GenericApplicationContext("test", true, ctx);
    }

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

        /// <summary>
        /// no op
        /// </summary>
        /// <param name="factory">factory to post process</param>
        public void PostProcessObjectFactory(IConfigurableListableObjectFactory factory)
        {
        }
    }
}

public class SingletonTestingObjectPostProcessor : IObjectPostProcessor, IApplicationContextAware
{
    private IApplicationContext applicationContext;

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

    public IApplicationContext ApplicationContext
    {
        set { this.applicationContext = value; }
    }
}
