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

using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using NUnit.Framework;
using Spring.Objects.Factory.Config;
using Spring.TestSupport;
using Spring.Util;

namespace Spring.Objects.Factory.Support;

/// <summary>
/// Tests for the 'request'- and 'session'-scoped table accessors and the
/// end-of-request/end-of-session hooks of <see cref="WebObjectFactory"/>.
/// </summary>
[TestFixture]
public sealed class WebObjectFactoryScopedObjectTests
{
    private sealed class TestWebObjectFactory : WebObjectFactory
    {
        public TestWebObjectFactory(string contextPath, bool caseSensitive)
            : base(contextPath, caseSensitive)
        {
        }

        public IDictionary RequestTable => Request;

        public IDictionary SessionTable => Session;

        public void AddEagerlyCachedSingletonForTest(string objectName, IObjectDefinition definition, object instance)
        {
            AddEagerlyCachedSingleton(objectName, definition, instance);
        }

        public void RemoveEagerlyCachedSingletonForTest(string objectName, IObjectDefinition definition)
        {
            RemoveEagerlyCachedSingleton(objectName, definition);
        }
    }

    public sealed class DisposableProbe : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }

    private VirtualEnvironmentMock _environment;

    [SetUp]
    public void SetUp()
    {
        // the factory must be created, and request/session scoped objects used, within a valid HttpContext
        _environment = new VirtualEnvironmentMock("/somedir/some.file", null, null, "/", true);
        TestWebContext.Create("/apppath", "testpage.aspx");
        ResetEventHandlerRegistration();
    }

    [TearDown]
    public void TearDown()
    {
        ResetEventHandlerRegistration();
        TestWebContext.Release();
        _environment.Dispose();
    }

    private static void AddSession()
    {
        HttpSessionStateContainer container = new HttpSessionStateContainer(
            "test-session", new SessionStateItemCollection(), new HttpStaticObjectsCollection(), 20, true,
            HttpCookieMode.UseCookies, SessionStateMode.InProc, false);
        SessionStateUtility.AddHttpSessionStateToContext(HttpContext.Current, container);
    }

    private static RootWebObjectDefinition CreateDefinition(Type type, ObjectScope scope)
    {
        RootWebObjectDefinition definition = new RootWebObjectDefinition(
            type, new ConstructorArgumentValues(), new MutablePropertyValues());
        definition.Scope = scope.ToString();
        return definition;
    }

    /// <summary>
    /// Forgets that <see cref="WebObjectFactory"/> has hooked up its end-of-request/session
    /// handlers, so that every test starts with the first factory that registers them.
    /// </summary>
    private static void ResetEventHandlerRegistration()
    {
        typeof(WebObjectFactory)
            .GetField("s_eventHandlersRegistered", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, false);

        RemoveWebObjectFactoryHandlers("s_requestEvent");
        RemoveWebObjectFactoryHandlers("s_sessionEvent");
    }

    private static void RemoveWebObjectFactoryHandlers(string fieldName)
    {
        FieldInfo field = typeof(VirtualEnvironment).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        Delegate handlers = (Delegate) field.GetValue(null);
        if (handlers == null)
        {
            return;
        }

        foreach (Delegate handler in handlers.GetInvocationList())
        {
            if (handler.Method.DeclaringType == typeof(WebObjectFactory))
            {
                handlers = Delegate.Remove(handlers, handler);
            }
        }

        field.SetValue(null, handlers);
    }

    private static void RaiseEndRequest()
    {
        FieldInfo field = typeof(VirtualEnvironment).GetField("s_requestEvent", BindingFlags.NonPublic | BindingFlags.Static);
        ((VirtualEnvironment.RequestEventHandler) field.GetValue(null))(HttpContext.Current);
    }

    private static void RaiseEndSession()
    {
        FieldInfo field = typeof(VirtualEnvironment).GetField("s_sessionEvent", BindingFlags.NonPublic | BindingFlags.Static);
        ((VirtualEnvironment.SessionEventHandler) field.GetValue(null))(HttpContext.Current.Session, CacheItemRemovedReason.Removed);
    }

    [Test]
    public void RequestTableIsCreatedAndReturnedOnFirstAccess()
    {
        TestWebObjectFactory factory = new TestWebObjectFactory("/somedir/", false);

        Assert.That(HttpContext.Current.Items["spring.objects"], Is.Null);

        IDictionary first = factory.RequestTable;

        Assert.That(first, Is.Not.Null);
        Assert.That(HttpContext.Current.Items["spring.objects"], Is.SameAs(first));
        Assert.That(factory.RequestTable, Is.SameAs(first));
    }

    [Test]
    public void SessionTableIsCreatedAndReturnedOnFirstAccess()
    {
        AddSession();
        TestWebObjectFactory factory = new TestWebObjectFactory("/somedir/", false);

        Assert.That(HttpContext.Current.Session["spring.objects"], Is.Null);

        IDictionary first = factory.SessionTable;

        Assert.That(first, Is.Not.Null);
        Assert.That(HttpContext.Current.Session["spring.objects"], Is.SameAs(first));
        Assert.That(factory.SessionTable, Is.SameAs(first));
    }

    [Test]
    public void EagerlyCachedRequestScopedSingletonCanBeAddedAndRemovedAsFirstAccess()
    {
        TestWebObjectFactory factory = new TestWebObjectFactory("/somedir/", false);
        RootWebObjectDefinition definition = CreateDefinition(typeof(object), ObjectScope.Request);
        object instance = new object();

        factory.AddEagerlyCachedSingletonForTest("requestScoped", definition, instance);
        Assert.That(factory.RequestTable["requestScoped"], Is.SameAs(instance));

        // a request that starts with a removal must not fail either
        HttpContext.Current.Items.Remove("spring.objects");
        factory.RemoveEagerlyCachedSingletonForTest("requestScoped", definition);
        Assert.That(factory.RequestTable.Contains("requestScoped"), Is.False);
    }

    [Test]
    public void EagerlyCachedSessionScopedSingletonCanBeAddedAndRemovedAsFirstAccess()
    {
        AddSession();
        TestWebObjectFactory factory = new TestWebObjectFactory("/somedir/", false);
        RootWebObjectDefinition definition = CreateDefinition(typeof(object), ObjectScope.Session);
        object instance = new object();

        factory.AddEagerlyCachedSingletonForTest("sessionScoped", definition, instance);
        Assert.That(factory.SessionTable["sessionScoped"], Is.SameAs(instance));

        HttpContext.Current.Session.Remove("spring.objects");
        factory.RemoveEagerlyCachedSingletonForTest("sessionScoped", definition);
        Assert.That(factory.SessionTable.Contains("sessionScoped"), Is.False);
    }

    [Test]
    public void FactoryThatCreatedARequestScopedObjectIsNotPinnedByTheEndRequestHandler()
    {
        WeakReference weakFactory = CreateFactoryAndRequestScopedObject();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.That(weakFactory.IsAlive, Is.False, "the factory is rooted by the static VirtualEnvironment.EndRequest event");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateFactoryAndRequestScopedObject()
    {
        TestWebObjectFactory factory = new TestWebObjectFactory("/somedir/", false);
        factory.RegisterObjectDefinition("requestScoped", CreateDefinition(typeof(DisposableProbe), ObjectScope.Request));

        Assert.That(factory.GetObject("requestScoped"), Is.InstanceOf<DisposableProbe>());

        return new WeakReference(factory);
    }

    [Test]
    public void FactoryThatCreatedASessionScopedObjectIsNotPinnedByTheEndSessionHandler()
    {
        AddSession();
        WeakReference weakFactory = CreateFactoryAndSessionScopedObject();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.That(weakFactory.IsAlive, Is.False, "the factory is rooted by the static VirtualEnvironment.EndSession event");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateFactoryAndSessionScopedObject()
    {
        TestWebObjectFactory factory = new TestWebObjectFactory("/somedir/", false);
        factory.RegisterObjectDefinition("sessionScoped", CreateDefinition(typeof(DisposableProbe), ObjectScope.Session));

        Assert.That(factory.GetObject("sessionScoped"), Is.InstanceOf<DisposableProbe>());

        return new WeakReference(factory);
    }

    [Test]
    public void RequestScopedObjectsAreDisposedAtEndOfRequest()
    {
        TestWebObjectFactory factory = new TestWebObjectFactory("/somedir/", false);
        factory.RegisterObjectDefinition("requestScoped", CreateDefinition(typeof(DisposableProbe), ObjectScope.Request));

        DisposableProbe probe = (DisposableProbe) factory.GetObject("requestScoped");
        Assert.That(factory.GetObject("requestScoped"), Is.SameAs(probe));
        Assert.That(probe.Disposed, Is.False);

        RaiseEndRequest();

        Assert.That(probe.Disposed, Is.True);
    }

    [Test]
    public void SessionScopedObjectsAreDisposedAtEndOfSession()
    {
        AddSession();
        TestWebObjectFactory factory = new TestWebObjectFactory("/somedir/", false);
        factory.RegisterObjectDefinition("sessionScoped", CreateDefinition(typeof(DisposableProbe), ObjectScope.Session));

        DisposableProbe probe = (DisposableProbe) factory.GetObject("sessionScoped");
        Assert.That(factory.GetObject("sessionScoped"), Is.SameAs(probe));
        Assert.That(probe.Disposed, Is.False);

        RaiseEndSession();

        Assert.That(probe.Disposed, Is.True);
    }

    [Test]
    public void ObjectsOfEveryFactoryAreDisposedAtEndOfRequest()
    {
        TestWebObjectFactory first = new TestWebObjectFactory("/somedir/", false);
        first.RegisterObjectDefinition("requestScoped", CreateDefinition(typeof(DisposableProbe), ObjectScope.Request));
        TestWebObjectFactory second = new TestWebObjectFactory("/somedir/", false);
        second.RegisterObjectDefinition("otherRequestScoped", CreateDefinition(typeof(DisposableProbe), ObjectScope.Request));

        DisposableProbe firstProbe = (DisposableProbe) first.GetObject("requestScoped");
        DisposableProbe secondProbe = (DisposableProbe) second.GetObject("otherRequestScoped");

        RaiseEndRequest();

        Assert.That(firstProbe.Disposed, Is.True);
        Assert.That(secondProbe.Disposed, Is.True);
    }
}
