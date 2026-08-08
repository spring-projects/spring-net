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

namespace Spring.Objects.Factory.Support;

/// <summary>
/// Regression tests for GH-239: instances created from inner object definitions and
/// implementing <see cref="IDisposable"/> must not accumulate in the factory-lifetime
/// disposal registry when the owning instance is created repeatedly.
/// </summary>
[TestFixture]
public sealed class DisposableInnerObjectTrackingTests
{
    private sealed class ExposingObjectFactory : DefaultListableObjectFactory
    {
        public int DisposableInnerObjectCount => DisposableInnerObjects.Count;
    }

    public sealed class DisposableInner : IDisposable
    {
        public bool Disposed;

        public void Dispose()
        {
            Disposed = true;
        }
    }

    public sealed class Owner
    {
        public DisposableInner Inner { get; set; }
    }

    private static RootObjectDefinition OwnerDefinitionWithDisposableInner(bool singleton)
    {
        RootObjectDefinition owner = new RootObjectDefinition(typeof(Owner));
        owner.IsSingleton = singleton;
        owner.PropertyValues.Add("Inner", new RootObjectDefinition(typeof(DisposableInner)));
        return owner;
    }

    [Test]
    public void ConfigureObjectDoesNotAccumulateDisposableInnerObjects()
    {
        ExposingObjectFactory factory = new ExposingObjectFactory();
        factory.RegisterObjectDefinition("owner", OwnerDefinitionWithDisposableInner(true));

        for (int i = 0; i < 5; i++)
        {
            Owner owner = new Owner();
            factory.ConfigureObject(owner, "owner");
            Assert.That(owner.Inner, Is.Not.Null);
        }

        Assert.That(factory.DisposableInnerObjectCount, Is.EqualTo(0),
            "ConfigureObject() configures externally managed instances and may run once per " +
            "web request; it must not register inner objects for factory-shutdown disposal.");
    }

    [Test]
    public void SingletonWithDisposableInnerObjectIsRegisteredExactlyOnce()
    {
        ExposingObjectFactory factory = new ExposingObjectFactory();
        factory.RegisterObjectDefinition("owner", OwnerDefinitionWithDisposableInner(true));

        Owner first = (Owner) factory.GetObject("owner");
        Owner second = (Owner) factory.GetObject("owner");

        Assert.That(second, Is.SameAs(first));
        Assert.That(factory.DisposableInnerObjectCount, Is.EqualTo(1));
    }

    [Test]
    public void PrototypeWithDisposableInnerObjectIsNotRegistered()
    {
        ExposingObjectFactory factory = new ExposingObjectFactory();
        factory.RegisterObjectDefinition("owner", OwnerDefinitionWithDisposableInner(false));

        factory.GetObject("owner");
        factory.GetObject("owner");

        Assert.That(factory.DisposableInnerObjectCount, Is.EqualTo(0));
    }

    [Test]
    public void RegisteredInnerObjectOfSingletonIsDisposedWithTheFactory()
    {
        ExposingObjectFactory factory = new ExposingObjectFactory();
        factory.RegisterObjectDefinition("owner", OwnerDefinitionWithDisposableInner(true));

        Owner owner = (Owner) factory.GetObject("owner");
        Assert.That(owner.Inner.Disposed, Is.False);

        factory.Dispose();

        Assert.That(owner.Inner.Disposed, Is.True);
    }
}
