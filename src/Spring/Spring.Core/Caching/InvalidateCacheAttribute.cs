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

using Spring.Expressions;

namespace Spring.Caching
{
    /// <summary>
    /// This attribute should be used to mark method that should 
    /// invalidate one or more cache items when invoked.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This attribute allows application developers to specify that some
    /// cache items should be evicted from cache when the method is invoked, 
    /// but it will not do any eviction by itself.
    /// </p>
    /// <p>
    /// In order to actually evict cache items, an application developer
    /// must apply a <c>Spring.Aspects.Cache.InvalidateCacheAdvice</c> to
    /// all of the members that have this attribute defined.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    [Serializable]
    public sealed class InvalidateCacheAttribute : Attribute
    {
        private string cacheName;
        private string keys;
        private IExpression keysExpression;
        private string condition;
        private IExpression conditionExpression;

        /// <summary>
        /// Creates an attribute instance.
        /// </summary>
        public InvalidateCacheAttribute()
        {
        }

        /// <summary>
        /// Creates an attribute instance.
        /// </summary>
        /// <param name="cacheName">
        /// The name of the cache to use.
        /// </param>
        public InvalidateCacheAttribute(string cacheName)
        {
            this.cacheName = cacheName;
        }

        /// <summary>
        /// Gets or sets the name of the cache to use.
        /// </summary>
        /// <value>
        /// The name of the cache to use.
        /// </value>
        public string CacheName
        {
            get { return cacheName; }
            set { cacheName = value; }
        }

        /// <summary>
        /// Gets or sets a SpEL expression that should be evaluated in order 
        /// to determine the keys for the items that should be evicted.
        /// </summary>
        /// <value>
        /// An expression string that should be evaluated in order 
        /// to determine the keys for the items that should be evicted.
        /// </value>
        public string Keys
        {
            get { return keys; }
            set
            {
                keys = value;
                keysExpression = Expression.Parse(value);
            }
        }

        /// <summary>
        /// Gets an expression instance that should be evaluated in order 
        /// to determine the keys for the items that should be evicted.
        /// </summary>
        /// <value>
        /// An expression instance that should be evaluated in order 
        /// to determine the keys for the items that should be evicted.
        /// </value>
        public IExpression KeysExpression
        {
            get { return keysExpression; }
        }

        /// <summary>
        /// Gets or sets a SpEL expression that should be evaluated in order 
        /// to determine whether items should be evicted.
        /// </summary>
        /// <value>
        /// An expression string that should be evaluated in order to determine 
        /// whether items should be evicted.
        /// </value>
        public string Condition
        {
            get { return condition; }
            set
            {
                condition = value;
                conditionExpression = Expression.Parse(value);
            }
        }

        /// <summary>
        /// Gets an expression instance that should be evaluated in order 
        /// to determine whether items should be evicted.
        /// </summary>
        /// <value>
        /// An expression instance that should be evaluated in order to determine 
        /// whether items should be evicted.
        /// </value>
        public IExpression ConditionExpression
        {
            get { return conditionExpression; }
        }
    }
}
