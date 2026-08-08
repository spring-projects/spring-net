/*
 * Copyright 2002-2026 the original author or authors.
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
using Spring.Objects.Factory.Config;
using Spring.TestSupport;

namespace Spring.Objects.Factory.Support;

/// <summary>
/// Regression tests for GH-239: inner objects of 'request'- and 'session'-scoped owners
/// are created anew on every request/session and must not be tracked in the
/// factory-lifetime disposal registry.
/// </summary>
[TestFixture]
public sealed class WebObjectFactoryDisposableInnerObjectTests
{
    private sealed class TestWebObjectFactory : WebObjectFactory
    {
        public TestWebObjectFactory(string contextPath, bool caseSensitive)
            : base(contextPath, caseSensitive)
        {
        }

        public int DisposableInnerObjectCount => DisposableInnerObjects.Count;

        public void RegisterDisposableInnerObjectForTest(string ownerName, object instance)
        {
            RegisterDisposableInnerObject(ownerName, instance);
        }
    }

    private sealed class DisposableInner : IDisposable
    {
        public void Dispose()
        {
        }
    }

    private TestWebObjectFactory CreateFactoryWithScopedDefinitions()
    {
        TestWebObjectFactory factory;
        using (new VirtualEnvironmentMock("/somedir/some.file", null, null, "/", true))
        {
            factory = new TestWebObjectFactory("/somedir/", false);
        }

        RegisterDefinition(factory, "applicationScopedOwner", ObjectScope.Application);
        RegisterDefinition(factory, "requestScopedOwner", ObjectScope.Request);
        RegisterDefinition(factory, "sessionScopedOwner", ObjectScope.Session);
        return factory;
    }

    private static void RegisterDefinition(WebObjectFactory factory, string name, ObjectScope scope)
    {
        RootWebObjectDefinition definition = new RootWebObjectDefinition(
            typeof(object), new ConstructorArgumentValues(), new MutablePropertyValues());
        definition.Scope = scope.ToString();
        factory.RegisterObjectDefinition(name, definition);
    }

    [Test]
    public void InnerObjectsOfRequestAndSessionScopedOwnersAreNotTracked()
    {
        TestWebObjectFactory factory = CreateFactoryWithScopedDefinitions();

        factory.RegisterDisposableInnerObjectForTest("requestScopedOwner", new DisposableInner());
        factory.RegisterDisposableInnerObjectForTest("sessionScopedOwner", new DisposableInner());

        Assert.That(factory.DisposableInnerObjectCount, Is.EqualTo(0));
    }

    [Test]
    public void InnerObjectsOfApplicationScopedOwnersAreStillTracked()
    {
        TestWebObjectFactory factory = CreateFactoryWithScopedDefinitions();

        factory.RegisterDisposableInnerObjectForTest("applicationScopedOwner", new DisposableInner());

        Assert.That(factory.DisposableInnerObjectCount, Is.EqualTo(1));
    }

    [Test]
    public void InnerObjectsOfUnknownOwnersAreStillTracked()
    {
        TestWebObjectFactory factory = CreateFactoryWithScopedDefinitions();

        factory.RegisterDisposableInnerObjectForTest("(inner object)", new DisposableInner());

        Assert.That(factory.DisposableInnerObjectCount, Is.EqualTo(1));
    }
}
