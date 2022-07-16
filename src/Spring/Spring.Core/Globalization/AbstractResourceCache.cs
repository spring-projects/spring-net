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

using System.Globalization;

namespace Spring.Globalization
{
    /// <summary>
    /// Abstract base class that all resource cache implementations should extend.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public abstract class AbstractResourceCache : IResourceCache
    {
        /// <summary>
        /// Gets the list of resources from the cache.
        /// </summary>
        /// <param name="target">Target to get a list of resources for.</param>
        /// <param name="culture">Resource culture.</param>
        /// <returns>A list of cached resources for the specified target object and culture.</returns>
        public IList<Resource> GetResources(object target, CultureInfo culture)
        {
            return GetResources(CreateCacheKey(target, culture));
        }

        /// <summary>
        /// Puts the list of resources in the cache.
        /// </summary>
        /// <param name="target">Target to cache a list of resources for.</param>
        /// <param name="culture">Resource culture.</param>
        /// <param name="resources">A list of resources to cache.</param>
        /// <returns>A list of cached resources for the specified target object and culture.</returns>
        public void PutResources(object target, CultureInfo culture, IList<Resource> resources)
        {
            PutResources(CreateCacheKey(target, culture), resources);
        }

        /// <summary>
        /// Crates resource cache key for the specified target object and culture.
        /// </summary>
        /// <param name="target">Target object to apply resources to.</param>
        /// <param name="culture">Resource culture to use for resource lookup.</param>
        protected virtual string CreateCacheKey(object target, CultureInfo culture)
        {
            return target.GetType().FullName + "." + culture.Name + ".resources";
        }

        /// <summary>
        /// Gets the list of resources from cache.
        /// </summary>
        /// <param name="cacheKey">Cache key to use for lookup.</param>
        /// <returns>A list of cached resources for the specified target object and culture.</returns>
        protected abstract IList<Resource> GetResources(string cacheKey);

        /// <summary>
        /// Puts the list of resources in the cache.
        /// </summary>
        /// <param name="cacheKey">Cache key to use for the specified resources.</param>
        /// <param name="resources">A list of resources to cache.</param>
        /// <returns>A list of cached resources for the specified target object and culture.</returns>
        protected abstract void PutResources(string cacheKey, IList<Resource> resources);

    }
}
