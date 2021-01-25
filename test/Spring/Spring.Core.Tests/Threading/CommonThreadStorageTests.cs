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

using System.Threading;
using NUnit.Framework;

#endregion

namespace Spring.Threading
{
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

            Assert.AreSame(value1, storage.GetData("key"));
            Assert.AreSame(value2, storage.GetData("KEY"));
            Assert.AreSame(value3, storage.GetData("KeY"));
        }

        [Test]
        public void AllowReplaceData()
        {
            var storage = CreateStorage();

            var value1 = new object();
            var value2 = new object();

            storage.SetData("key", value1);
            Assert.AreSame(value1, storage.GetData("key"));
            storage.SetData("key", value2);
            Assert.AreSame(value2, storage.GetData("key"));
            storage.SetData("key", null);
            Assert.AreSame(null, storage.GetData("key"));
        }

        [Test]
        public void UnknownKeyReturnsNull()
        {
            var storage = CreateStorage();

            Assert.AreSame(null, storage.GetData("key"));
        }

        [Test]
        public void FreeNamedDataSlotRemovesData()
        {
            var storage = CreateStorage();

            var value1 = new object();
            storage.SetData("key", value1);
            Assert.AreSame(value1, storage.GetData("key"));
            storage.FreeNamedDataSlot("key");
            Assert.AreSame(null, storage.GetData("key"));
        }

        [Test]
        public void UsesDistinguishedStorageOnDifferentThreads()
        {
            var storage = CreateStorage();

            var value1 = new object();
            storage.SetData("key", value1);

            var helper = new ThreadTestHelper(this, storage);
            helper.Execute();

            Assert.AreNotSame(value1, helper.value);
            Assert.IsNull(helper.value);
        }

        #region Test Utility Classes

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

        #endregion Test Utility Classes
    }
}