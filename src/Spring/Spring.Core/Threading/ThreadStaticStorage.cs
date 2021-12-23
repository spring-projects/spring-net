#region License
// /*
//  * Copyright 2022 the original author or authors.
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *      http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */
#endregion

using System.Collections;
using System.Threading;

namespace Spring.Threading
{
    /// <summary>
    /// Implements <see cref="IThreadStorage"/> by using a <see cref="ThreadStaticAttribute"/> hashtable.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class ThreadStaticStorage : IThreadStorage
    {
        [ThreadStatic]
        private static Hashtable _dataThreadStatic;
        // AsyncLocal for it to work in async NMS lib
        private static AsyncLocal<Hashtable> _dataAsyncLocal = new AsyncLocal<Hashtable>();

        /// <summary>
        /// Allows to switch how context is being held, if true, then it will use AsyncLocal
        /// </summary>
        public static bool UseAsyncLocal { get; set; } = false;

        private static Hashtable Data
        {
            get
            {
                if (UseAsyncLocal)
                {
                    if (_dataAsyncLocal.Value == null) _dataAsyncLocal.Value = new Hashtable();
                    return _dataAsyncLocal.Value;
                }
                else
                {
                    if (_dataThreadStatic == null) _dataThreadStatic = new Hashtable();
                    return _dataThreadStatic;
                }
            }
        }

        /// <summary>
        /// Retrieves an object with the specified name.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns>The object in the call context associated with the specified name or null if no object has been stored previously</returns>
        public object GetData(string name)
        {
            return Data[name];
        }

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item.</param>
        /// <param name="value">The object to store in the call context.</param>
        public void SetData(string name, object value)
        {
            Data[name] = value;
        }

        /// <summary>
        /// Empties a data slot with the specified name.
        /// </summary>
        /// <param name="name">The name of the data slot to empty.</param>
        public void FreeNamedDataSlot(string name)
        {
            Data.Remove(name);
        }
    }
}
