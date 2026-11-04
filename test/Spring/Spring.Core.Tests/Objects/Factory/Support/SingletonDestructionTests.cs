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

using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Spring.Objects.Factory.Support;

/// <summary>
/// Tests that a factory destroys the singletons it created exactly once, and keeps no reference to a
/// singleton after it has destroyed it (GH-239).
/// </summary>
[TestFixture]
public sealed class SingletonDestructionTests
{
    private sealed class ExposingObjectFactory : DefaultListableObjectFactory
    {
        public void Destroy(string name) => DestroySingleton(name);
    }

    public sealed class DisposableSingleton : IDisposable
    {
        public int DisposeCount;

        public void Dispose()
        {
            DisposeCount++;
        }
    }

    public sealed class DestroyMethodSingleton
    {
        public int CleanupCount;

        public void Cleanup()
        {
            CleanupCount++;
        }
    }

    private static void RegisterSingletons(DefaultListableObjectFactory factory)
    {
        factory.RegisterObjectDefinition("disposable", new RootObjectDefinition(typeof(DisposableSingleton)));

        RootObjectDefinition withDestroyMethod = new RootObjectDefinition(typeof(DestroyMethodSingleton));
        withDestroyMethod.DestroyMethodName = "Cleanup";
        factory.RegisterObjectDefinition("withDestroyMethod", withDestroyMethod);
    }

    [Test]
    public void DisposingTheFactoryDestroysEachDestroyableSingletonExactlyOnce()
    {
        DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
        RegisterSingletons(factory);
        DisposableSingleton disposable = (DisposableSingleton) factory.GetObject("disposable");
        DestroyMethodSingleton withDestroyMethod = (DestroyMethodSingleton) factory.GetObject("withDestroyMethod");
        factory.GetObject("disposable");
        factory.GetObject("withDestroyMethod");

        Assert.That(disposable.DisposeCount, Is.EqualTo(0));
        Assert.That(withDestroyMethod.CleanupCount, Is.EqualTo(0));

        factory.Dispose();

        Assert.That(disposable.DisposeCount, Is.EqualTo(1));
        Assert.That(withDestroyMethod.CleanupCount, Is.EqualTo(1));

        factory.Dispose();

        Assert.That(disposable.DisposeCount, Is.EqualTo(1));
        Assert.That(withDestroyMethod.CleanupCount, Is.EqualTo(1));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Disposable, WeakReference WithDestroyMethod) CreateAndDestroy(
        ExposingObjectFactory factory, bool disposeFactory)
    {
        DisposableSingleton disposable = (DisposableSingleton) factory.GetObject("disposable");
        DestroyMethodSingleton withDestroyMethod = (DestroyMethodSingleton) factory.GetObject("withDestroyMethod");

        if (disposeFactory)
        {
            factory.Dispose();
        }
        else
        {
            factory.Destroy("disposable");
            factory.Destroy("withDestroyMethod");
        }

        Assert.That(disposable.DisposeCount, Is.EqualTo(1));
        Assert.That(withDestroyMethod.CleanupCount, Is.EqualTo(1));
        return (new WeakReference(disposable), new WeakReference(withDestroyMethod));
    }

    [TestCase(true, TestName = "DestroyedSingletonsAreCollectableAfterTheFactoryIsDisposed")]
    [TestCase(false, TestName = "DestroyedSingletonsAreCollectableAfterDestroySingleton")]
    public void DestroyedSingletonsAreCollectable(bool disposeFactory)
    {
        ExposingObjectFactory factory = new ExposingObjectFactory();
        RegisterSingletons(factory);

        var (disposable, withDestroyMethod) = CreateAndDestroy(factory, disposeFactory);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.That(disposable.IsAlive, Is.False, "The factory still references the destroyed IDisposable singleton.");
        Assert.That(withDestroyMethod.IsAlive, Is.False, "The factory still references the destroyed destroy-method singleton.");
        GC.KeepAlive(factory);
    }
}
