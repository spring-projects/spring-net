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
using System.Collections;
using NUnit.Framework;

#endregion

namespace Spring.Threading
{
    /// <summary>
    /// Test behaviour of LogicalThreadContext
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class LogicalThreadContextTest
    {
        private class MockStorage : IThreadStorage
        {
            internal Hashtable data = new Hashtable();

            public object GetData(string name)
            {
                return data[name];
            }

            public void SetData(string name, object value)
            {
                data[name] = value;
            }

            public void FreeNamedDataSlot(string name)
            {
                data.Remove(name);
            }
        }

        [Test]
        public void StorageMustNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => LogicalThreadContext.SetStorage(null));
        }

        [Test]
        public void StorageMayBeSetMoreThanOnce()
        {
            LogicalThreadContext.SetStorage(new MockStorage());
            LogicalThreadContext.SetStorage(new MockStorage());
            LogicalThreadContext.SetStorage(new MockStorage());
        }

        [Test]
        public void StorageIsUsedByFacadeMethods()
        {
            MockStorage storage = new MockStorage();
            LogicalThreadContext.SetStorage(storage);

            object value = new object();

            LogicalThreadContext.SetData("key", value);
            Assert.AreSame( value, storage.data["key"] );

            object data = LogicalThreadContext.GetData("key");
            Assert.AreSame(value, data);

            LogicalThreadContext.FreeNamedDataSlot("key");

            Assert.IsFalse( storage.data.ContainsKey("key") );
        }
    }
}
