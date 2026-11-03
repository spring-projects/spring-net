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

using NUnit.Framework;

namespace Spring.Threading;

/// <summary>
/// Any <see cref="IThreadStorage"/> implementation must pass these tests.
/// </summary>
/// <author>Erich Eichinger</author>
public abstract class CommonThreadStorageTests
{
    protected abstract IThreadStorage CreateStorage();

    protected virtual void ThreadSetup()
    {
    }

    protected virtual void ThreadCleanup()
    {
    }

    [SetUp]
    public void SetUp()
    {
        ThreadSetup();
    }

    [TearDown]
    public void TearDown()
    {
        // cleanup storage
        var storage = CreateStorage();
        storage.FreeNamedDataSlot("key");
        storage.FreeNamedDataSlot("KEY");
        storage.FreeNamedDataSlot("KeY");

        ThreadCleanup();
    }

    [Test]
    public void IsCaseSensitive()
    {
        var storage = CreateStorage();

        var value1 = new object();
        var value2 = new object();
        var value3 = new object();

        storage.SetData("key", value1);
        storage.SetData("KEY", value2);
        storage.SetData("KeY", value3);

        Assert.That(storage.GetData("key"), Is.SameAs(value1));
        Assert.That(storage.GetData("KEY"), Is.SameAs(value2));
        Assert.That(storage.GetData("KeY"), Is.SameAs(value3));
    }

    [Test]
    public void AllowReplaceData()
    {
        var storage = CreateStorage();

        var value1 = new object();
        var value2 = new object();

        storage.SetData("key", value1);
        Assert.That(storage.GetData("key"), Is.SameAs(value1));
        storage.SetData("key", value2);
        Assert.That(storage.GetData("key"), Is.SameAs(value2));
        storage.SetData("key", null);
        Assert.That(storage.GetData("key"), Is.Null);
    }

    [Test]
    public void UnknownKeyReturnsNull()
    {
        var storage = CreateStorage();

        Assert.That(storage.GetData("key"), Is.Null);
    }

    [Test]
    public void FreeNamedDataSlotRemovesData()
    {
        var storage = CreateStorage();

        var value1 = new object();
        storage.SetData("key", value1);
        Assert.That(storage.GetData("key"), Is.SameAs(value1));
        storage.FreeNamedDataSlot("key");
        Assert.That(storage.GetData("key"), Is.Null);
    }

    [Test]
    public void UsesDistinguishedStorageOnDifferentThreads()
    {
        var storage = CreateStorage();

        var value1 = new object();
        storage.SetData("key", value1);

        var helper = new ThreadTestHelper(this, storage);
        helper.Execute();

        Assert.That(helper.value, Is.Not.SameAs(value1));
        Assert.That(helper.value, Is.Null);
    }

    private class ThreadTestHelper
    {
        private CommonThreadStorageTests outer;
        private IThreadStorage storage;

        public object value;

        public ThreadTestHelper(CommonThreadStorageTests outer, IThreadStorage storage)
        {
            this.outer = outer;
            this.storage = storage;
        }

        public void Execute()
        {
            var t = new Thread(new ThreadStart(Run));
            t.Start();
            t.Join();
        }

        private void Run()
        {
            outer.ThreadSetup();
            value = storage.GetData("key");
        }
    }
}
