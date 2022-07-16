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
    /// Defines an interface that resource cache adapters have to implement.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public interface IResourceCache
    {
        /// <summary>
        /// Gets the list of resources from cache.
        /// </summary>
        /// <param name="target">Target to get a list of resources for.</param>
        /// <param name="culture">Resource culture.</param>
        /// <returns>A list of cached resources for the specified target object and culture.</returns>
        IList<Resource> GetResources(object target, CultureInfo culture);

        /// <summary>
        /// Puts the list of resources in the cache.
        /// </summary>
        /// <param name="target">Target to cache a list of resources for.</param>
        /// <param name="culture">Resource culture.</param>
        /// <param name="resources">A list of resources to cache.</param>
        /// <returns>A list of cached resources for the specified target object and culture.</returns>
        void PutResources(object target, CultureInfo culture, IList<Resource> resources);
    }
}
