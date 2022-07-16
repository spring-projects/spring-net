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

namespace Spring.Globalization
{
    /// <summary>
    /// Resource cache implementation that doesn't cache resources.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class NullResourceCache : AbstractResourceCache
    {
        /// <summary>
        /// Gets the list of resources from cache.
        /// </summary>
        /// <param name="cacheKey">Cache key to use for lookup.</param>
        /// <returns>Always returns <c>null</c>.</returns>
        protected override IList<Resource> GetResources(string cacheKey)
        {
            return null;
        }

        /// <summary>
        /// Puts the list of resources in the cache.
        /// </summary>
        /// <param name="cacheKey">Cache key to use for the specified resources.</param>
        /// <param name="resources">A list of resources to cache.</param>
        protected override void PutResources(string cacheKey, IList<Resource> resources)
        {}
    }
}
