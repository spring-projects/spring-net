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
	/// Defines a contract that all cache implementations have to fulfill.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
    /// <author>Erich Eichinger</author>
	public interface ICache
	{
        /// <summary>
        /// Gets the number of items in the cache.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a collection of all cache item keys.
        /// </summary>
        ICollection Keys { get; }

        /// <summary>
        /// Retrieves an item from the cache.
        /// </summary>
        /// <param name="key">
        /// Item key.
        /// </param>
        /// <returns>
        /// Item for the specified <paramref name="key"/>, or <c>null</c>.
        /// </returns>
	    object Get(object key);

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">
        /// Item key.
        /// </param>
        void Remove(object key);

	    /// <summary>
	    /// Removes collection of items from the cache.
	    /// </summary>
	    /// <param name="keys">
	    /// Collection of keys to remove.
	    /// </param>
	    void RemoveAll(ICollection keys);

        /// <summary>
        /// Removes all items from the cache.
        /// </summary>
        void Clear();

        /// <summary>
        /// Inserts an item into the cache.
        /// </summary>
        /// <remarks>
        /// Items inserted using this method have no expiration time
        /// and default cache priority.
        /// </remarks>
        /// <param name="key">
        /// Item key.
        /// </param>
        /// <param name="value">
        /// Item value.
        /// </param>
	    void Insert(object key, object value);

        /// <summary>
        /// Inserts an item into the cache.
        /// </summary>
        /// <remarks>
        /// Items inserted using this method have default cache priority.
        /// </remarks>
        /// <param name="key">
        /// Item key.
        /// </param>
        /// <param name="value">
        /// Item value.
        /// </param>
        /// <param name="timeToLive">
        /// Item's time-to-live.
        /// </param>
        void Insert(object key, object value, TimeSpan timeToLive);
	}
}
