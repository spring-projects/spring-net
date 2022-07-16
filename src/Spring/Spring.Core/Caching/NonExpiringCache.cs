#region License

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

#endregion

using System.Collections;

namespace Spring.Caching
{
    /// <summary>
    /// A simple <see cref="ICache"/> implementation backed by a dictionary that
    /// never expires cache items.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class NonExpiringCache : AbstractCache
    {
        private readonly object syncRoot = new object();
        private readonly IDictionary<object, object> itemStore = new Dictionary<object, object>();

        /// <summary>
        /// Gets the number of items in the cache.
        /// </summary>
        public override int Count
        {
            get
            {
                lock (syncRoot)
                {
                    return itemStore.Count;
                }
            }
        }

        /// <summary>
        /// Gets a collection of all cache item keys.
        /// </summary>
        public override ICollection Keys
        {
            get
            {
                lock (syncRoot)
                {
                    return (ICollection) itemStore.Keys;
                }
            }
        }

        /// <summary>
        /// Retrieves an item from the cache.
        /// </summary>
        /// <param name="key">
        /// Item key.
        /// </param>
        /// <returns>
        /// Item for the specified <paramref name="key"/>, or <c>null</c>.
        /// </returns>
        public override object Get(object key)
        {
            lock (syncRoot)
            {
                object value;
                itemStore.TryGetValue(key, out value);
                return value;
            }
        }

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">
        /// Item key.
        /// </param>
        public override void Remove(object key)
        {
            lock (syncRoot)
            {
                itemStore.Remove(key);
            }
        }

        /// <summary>
        /// Removes collection of items from the cache.
        /// </summary>
        /// <param name="keys">
        /// Collection of keys to remove.
        /// </param>
        public override void RemoveAll(ICollection keys)
        {
            lock (syncRoot)
            {
                foreach (object key in keys)
                {
                    itemStore.Remove(key);
                }
            }
        }

        /// <summary>
        /// Removes all items from the cache.
        /// </summary>
        public override void Clear()
        {
            lock (syncRoot)
            {
                itemStore.Clear();
            }
        }

        /// <summary>
        /// Inserts an item into the cache.
        /// </summary>
        /// <param name="key">
        /// Item key.
        /// </param>
        /// <param name="value">
        /// Item value.
        /// </param>
        /// <param name="timeToLive">
        /// Item's time-to-live (TTL) in milliseconds.
        /// </param>
        protected override void DoInsert(object key, object value, TimeSpan timeToLive)
        {
            lock (syncRoot)
            {
                itemStore[key] = value;
            }
        }
    }
}
