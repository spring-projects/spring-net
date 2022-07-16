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
using System.Web;
using System.Web.Caching;
using Common.Logging;
using Spring.Util;

namespace Spring.Caching
{
    /// <summary>
    /// An <see cref="ICache"/> implementation backed by ASP.NET Cache (see <see cref="HttpRuntime.Cache"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Because ASP.NET Cache uses strings as cache keys, you need to ensure
    /// that the key object type has properly implemented <b>ToString</b> method.
    /// </para>
    /// <para>
    /// Despite the shared underlying <see cref="HttpRuntime.Cache"/>, it is possible to use more than
    /// one instance of <see cref="AspNetCache"/> without interfering each other.
    /// </para>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <author>Erich Eichinger</author>
    public class AspNetCache : AbstractCache
    {
        #region Internal Abstractions

        /// <summary>
        /// Abstracts the underlying runtime cache
        /// </summary>
        public interface IRuntimeCache : IEnumerable
        {
            /// <summary>
            /// Insert an item into the cache.
            /// </summary>
            void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration,
                   TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback);

            /// <summary>
            /// Removes an item from the cache.
            /// </summary>
            /// <param name="key">The key of the item to remove</param>
            /// <returns>The object that has been removed from the cache</returns>
            object Remove(string key);

            /// <summary>
            /// Retrieve an item with the specified key from the cache.
            /// </summary>
            /// <param name="key">The key of the item to be retrieved</param>
            /// <returns>The item, if found. <value>null</value> otherwise</returns>
            object Get(string key);
        }

        /// <summary>
        /// Actually delegates all calls to the underlying <see cref="HttpRuntime.Cache"/>
        /// </summary>
        private class RuntimeCache : IRuntimeCache
        {
            private readonly Cache _runtimeCache = HttpRuntime.Cache;

            public object Remove(string key)
            {
                return _runtimeCache.Remove(key);
            }

            public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback )
            {
                _runtimeCache.Insert(key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
            }

            public object Get(string key)
            {
                return _runtimeCache.Get(key);
            }

            public IEnumerator GetEnumerator()
            {
                return _runtimeCache.GetEnumerator();
            }
        }

        #endregion

        #region Fields

        // logger instance for this class
        private static readonly ILog Log = LogManager.GetLogger(typeof(AspNetCache));
        // the concrete cache implementation
        private readonly IRuntimeCache _cache;
        // the (unique!) name of this particular cache instance.
        private readonly string _cacheName;

        // hold default values for this cache instance.
        private CacheItemPriority _priority = CacheItemPriority.Default;
        private bool _slidingExpiration = false;

        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="AspNetCache"/>
        /// </summary>
        public AspNetCache()
            :this(new RuntimeCache())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AspNetCache"/>
        /// with the specified <see cref="IRuntimeCache"/> implementation.
        /// </summary>
        /// <param name="runtimeCache"></param>
        public AspNetCache(IRuntimeCache runtimeCache)
        {
            _cache = runtimeCache;
            // noop
            _cacheName = typeof(AspNetCache).FullName + "[" + this.GetHashCode() + "].";
        }

        #region Configurable Properties

        /// <summary>
        /// Gets/Sets a flag, whether to use sliding expiration policy.
        /// </summary>
        public bool SlidingExpiration
        {
            get { return _slidingExpiration; }
            set { _slidingExpiration = value; }
        }

        /// <summary>
        /// Gets/Sets a default priority to be applied to all items inserted into this cache.
        /// </summary>
        public CacheItemPriority Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        #endregion

        /// <summary>
        /// Gets a collection of all cache item keys.
        /// </summary>
        public override ICollection Keys
        {
            get
            {
                List<object> keys = new List<object>();

                foreach (DictionaryEntry entry in _cache)
                {
                    string key = (string) entry.Key;
                    if (key.StartsWith(_cacheName))
                    {
                        keys.Add(key.Substring(_cacheName.Length));
                    }
                }

                return keys;
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
            return key == null ? null : _cache.Get(GenerateKey(key));
        }

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">
        /// Item key.
        /// </param>
        public override void Remove(object key)
        {
            if (key != null)
            {
                if (Log.IsDebugEnabled) Log.Debug(string.Format("removing item '{0}' from cache '{1}'", key, this._cacheName));
                _cache.Remove(GenerateKey(key));
            }
        }

        /// <summary>
        /// Inserts an item into the cache.
        /// </summary>
        /// <remarks>
        /// Items inserted using this method have default <see cref="Priority"/> and default <see cref="SlidingExpiration"/>
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
        protected override void DoInsert(object key, object value, TimeSpan timeToLive)
        {
            this.Insert( key, value, timeToLive, _slidingExpiration, _priority );
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
        /// Item's time-to-live.
        /// </param>
        /// <param name="slidingExpiration">
        /// Flag specifying whether the item's time-to-live should be reset
        /// when the item is accessed.
        /// </param>
        public void Insert(object key, object value, TimeSpan timeToLive, bool slidingExpiration)
        {
            this.Insert(key, value, timeToLive, slidingExpiration, _priority);
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
        /// Item's time-to-live.
        /// </param>
        /// <param name="slidingExpiration">
        /// Flag specifying whether the item's time-to-live should be reset
        /// when the item is accessed.
        /// </param>
        /// <param name="itemPriority">
        /// Item priority.
        /// </param>
        public void Insert(object key, object value, TimeSpan timeToLive, bool slidingExpiration, CacheItemPriority itemPriority)
        {
            AssertUtils.ArgumentNotNull(key, "key");
            AssertUtils.State( TimeSpan.Zero <= timeToLive, "timeToLive" );

            if (Log.IsDebugEnabled) Log.Debug(string.Format("adding item '{0}' to cache '{1}'", key, this._cacheName));

            if (TimeSpan.Zero < timeToLive)
            {
                if (slidingExpiration)
                {
                    TimeSpan slidingExpirationSpan = timeToLive;
                    _cache.Insert(GenerateKey(key), value, null, Cache.NoAbsoluteExpiration, slidingExpirationSpan, itemPriority, null);
                }
                else
                {
                    DateTime absoluteExpiration = DateTime.UtcNow.Add(timeToLive);
                    _cache.Insert(GenerateKey(key), value, null, absoluteExpiration, Cache.NoSlidingExpiration, itemPriority, null);
                }
            }
            else
            {
                _cache.Insert(GenerateKey(key), value, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, itemPriority, null);
            }
        }

        /// <summary>
        /// Generate a key to be used for the underlying <see cref="Cache"/> implementation.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GenerateKey( object key )
        {
            return _cacheName + key;
        }
    }
}
