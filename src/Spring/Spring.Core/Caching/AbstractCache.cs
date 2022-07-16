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
    /// An abstract <see cref="ICache"/> implementation that can
    /// be used as base class for concrete implementations.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <author>Erich Eichinger</author>
    public abstract class AbstractCache : ICache
    {
        #region Fields

        private bool _enforceTimeToLive = false;
        private TimeSpan _timeToLive = TimeSpan.Zero;

        #endregion

        #region Properties

        /// <summary>
        /// Gets/Set the Default time-to-live (TTL) for items inserted into this cache.
        /// Used by <see cref="Insert(object,object)"/>
        /// </summary>
        public TimeSpan TimeToLive
        {
            get { return _timeToLive; }
            set { _timeToLive = value; }
        }

        /// <summary>
        /// Gets/Sets a value, whether the this cache instance's <see cref="TimeToLive"/>
        /// shall be applied to all items, regardless of their individual TTL
        /// when <see cref="Insert(object,object,TimeSpan)"/> is called.
        /// </summary>
        public bool EnforceTimeToLive
        {
            get { return _enforceTimeToLive; }
            set { _enforceTimeToLive = value; }
        }

        #endregion

        #region ICache Implementation

        #region

        /// <summary>
        /// Gets the number of items in the cache.
        /// </summary>
        /// <remarks>
        /// May be overridden by subclasses for cache-specific efficient implementation.
        /// </remarks>
        public virtual int Count
        {
            get
            {
                return Keys.Count;
            }
        }

        /// <summary>
        /// Gets a collection of all cache item keys.
        /// </summary>
        public abstract ICollection Keys { get; }



        /// <summary>
        /// Retrieves an item from the cache.
        /// </summary>
        /// <param name="key">
        /// Item key.
        /// </param>
        /// <returns>
        /// Item for the specified <paramref name="key"/>, or <c>null</c>.
        /// </returns>
        public abstract object Get(object key);

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">
        /// Item key.
        /// </param>
        public abstract void Remove(object key);

        /// <summary>
        /// Removes collection of items from the cache.
        /// </summary>
        /// <param name="keys">
        /// Collection of keys to remove.
        /// </param>
        public virtual void RemoveAll(ICollection keys)
        {
            foreach (object key in keys)
            {
                Remove(key);
            }
        }

        /// <summary>
        /// Removes all items from the cache.
        /// </summary>
        public virtual void Clear()
        {
            RemoveAll(this.Keys);
        }


        /// <summary>
        /// Inserts an item into the cache.
        /// </summary>
        /// <remarks>
        /// Items inserted using this method use the default
        /// </remarks>
        /// <param name="key">
        /// Item key.
        /// </param>
        /// <param name="value">
        /// Item value.
        /// </param>
        public virtual void Insert(object key, object value)
        {
            Insert(key, value, TimeToLive);
        }

        /// <summary>
        /// Inserts an item into the cache.
        /// </summary>
        /// <remarks>
        /// If <paramref name="timeToLive"/> equals <see cref="TimeSpan.MinValue"/>,
        /// or <see cref="EnforceTimeToLive"/> is <value>true</value>, this cache
        /// instance's <see cref="TimeToLive"/> value will be applied.
        /// </remarks>
        /// <param name="key">
        /// Item key.
        /// </param>
        /// <param name="value">
        /// Item value.
        /// </param>
        /// <param name="timeToLive">
        /// Item's time-to-live (TTL).
        /// </param>
        public virtual void Insert(object key, object value, TimeSpan timeToLive)
        {
            if (_enforceTimeToLive || (timeToLive < TimeSpan.Zero))
            {
                timeToLive = _timeToLive;
            }
            DoInsert(key, value, timeToLive);
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Actually does the cache implementation specific insert operation into the cache.
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
        /// Item's time-to-live (TTL).
        /// </param>
        protected abstract void DoInsert(object key, object value, TimeSpan timeToLive);

        #endregion
    }
}
