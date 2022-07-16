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

using System.Web;
using System.Web.Caching;

namespace Spring.Globalization
{
    /// <summary>
    /// Resource cache implementation that uses ASP.NET <see cref="Cache"/> to cache resources.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    internal class AspNetResourceCache : AbstractResourceCache
    {
        /// <summary>
        /// Gets the list of resources from cache.
        /// </summary>
        /// <param name="cacheKey">Cache key to use for lookup.</param>
        /// <returns>A list of cached resources for the specified target object and culture.</returns>
        protected override IList<Resource> GetResources(string cacheKey)
        {
            return (IList<Resource>)HttpRuntime.Cache[cacheKey];
        }

        /// <summary>
        /// Puts the list of resources in the cache.
        /// </summary>
        /// <param name="cacheKey">Cache key to use for the specified resources.</param>
        /// <param name="resources">A list of resources to cache.</param>
        protected override void PutResources(string cacheKey, IList<Resource> resources)
        {
            HttpRuntime.Cache[cacheKey] = resources;
        }
    }
}
