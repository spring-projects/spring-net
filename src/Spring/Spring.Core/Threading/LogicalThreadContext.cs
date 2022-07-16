#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using Spring.Util;

#endregion

namespace Spring.Threading
{
    /// <summary>
    /// An abstraction to safely store "ThreadStatic" data.
    /// </summary>
    /// <remarks>
    /// You may switch the storage strategy by calling <see cref="SetStorage(IThreadStorage)"/>.<p/>
    /// <b>NOTE:</b> Access to the underlying storage is not synchronized for performance reasons.
    /// You should call <see cref="SetStorage(IThreadStorage)"/> only once at application startup!
    /// </remarks>
    /// <author>Erich Eichinger</author>
    public sealed class LogicalThreadContext
    {
        /// <summary>
        /// Holds the current <see cref="IThreadStorage"/> strategy.
        /// </summary>
        /// <remarks>
        /// Access to this variable is not synchronized on purpose for performance reasons.
        /// Setting a different <see cref="IThreadStorage"/> strategy should happen only once
        /// at application startup.
        /// </remarks>
        private static IThreadStorage threadStorage =
#if NETSTANDARD
            new ThreadStaticStorage();
#else
            new CallContextStorage();
#endif

        /// <summary>
        /// Set the new <see cref="IThreadStorage"/> strategy.
        /// </summary>
        public static void SetStorage(IThreadStorage storage)
        {
            AssertUtils.ArgumentNotNull(storage, "storage");
            threadStorage = storage;
        }

        private LogicalThreadContext()
        {
            throw new NotSupportedException("must not be instantiated");
        }

        /// <summary>
        /// Retrieves an object with the specified name.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns>The object in the context associated with the specified name or null if no object has been stored previously</returns>
        public static object GetData(string name)
        {
            return threadStorage.GetData(name);
        }

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item.</param>
        /// <param name="value">The object to store in the current thread's context.</param>
        public static void SetData(string name, object value)
        {
            threadStorage.SetData(name, value);
        }

        /// <summary>
        /// Empties a data slot with the specified name.
        /// </summary>
        /// <param name="name">The name of the data slot to empty.</param>
        public static void FreeNamedDataSlot(string name)
        {
            threadStorage.FreeNamedDataSlot(name);
        }
    }
}
