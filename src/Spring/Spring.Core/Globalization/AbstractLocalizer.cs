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
using System.Threading;

using Spring.Context;
using Spring.Util;

namespace Spring.Globalization
{
    /// <summary>
    /// Abstract base class that all localizers should extend
    /// </summary>
    /// <remarks>
    /// <p>
    /// This class contains the bulk of the localizer logic, including implementation
    /// of the <c>ApplyResources</c> methods that are defined in <see cref="ILocalizer"/>
    /// interface.
    /// </p>
    /// <p>
    /// All specific localizers need to do is inherit this class and implement
    /// <c>GetResources</c> method that will return a list of <see cref="Resource"/>
    /// objects that should be applied to a specified <c>target</c>.
    /// </p>
    /// <p>
    /// Custom implementations can use whatever type of resource storage they want,
    /// such as standard .NET resource sets, custom XML files, database, etc.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public abstract class AbstractLocalizer : ILocalizer
    {
        private IResourceCache resourceCache = new NullResourceCache();

        /// <summary>
        /// Gets or sets the resource cache instance.
        /// </summary>
        /// <value>The resource cache instance.</value>
        public IResourceCache ResourceCache
        {
            get { return resourceCache; }
            set { resourceCache = value; }
        }

        /// <summary>
        /// Applies resources of the specified culture to the specified target object.
        /// </summary>
        /// <param name="target">Target object to apply resources to.</param>
        /// <param name="messageSource"><see cref="IMessageSource"/> instance to retrieve resources from.</param>
        /// <param name="culture">Resource culture to use for resource lookup.</param>
        public void ApplyResources(object target, IMessageSource messageSource, CultureInfo culture)
        {
            AssertUtils.ArgumentNotNull(target, "target");
            AssertUtils.ArgumentNotNull(culture, "culture");

            IList<Resource> resources = GetResources(target, messageSource, culture);
            foreach (Resource resource in resources)
            {
                resource.Target.SetValue(target, null, resource.Value);
            }
        }

        /// <summary>
        /// Applies resources to the specified target object, using current thread's uiCulture to resolve resources.
        /// </summary>
        /// <param name="target">Target object to apply resources to.</param>
        /// <param name="messageSource"><see cref="IMessageSource"/> instance to retrieve resources from.</param>
        public void ApplyResources(object target, IMessageSource messageSource)
        {
            AssertUtils.ArgumentNotNull(target, "target");
            ApplyResources(target, messageSource, Thread.CurrentThread.CurrentUICulture);
        }

        /// <summary>
        /// Returns a list of <see cref="Resource"/> instances that should be applied to the target.
        /// </summary>
        /// <param name="target">Target to get a list of resources for.</param>
        /// <param name="messageSource"><see cref="IMessageSource"/> instance to retrieve resources from.</param>
        /// <param name="culture">Resource locale.</param>
        /// <returns>A list of resources to apply.</returns>
        private IList<Resource> GetResources(object target, IMessageSource messageSource, CultureInfo culture)
        {
            IList<Resource> resources = resourceCache.GetResources(target, culture);

            if (resources == null)
            {
                resources = LoadResources(target, messageSource, culture);
                resourceCache.PutResources(target, culture, resources);
            }

            return resources;
        }

        /// <summary>
        /// Loads resources from the storage and creates a list of <see cref="Resource"/> instances that should be applied to the target.
        /// </summary>
        /// <param name="target">Target to get a list of resources for.</param>
        /// <param name="messageSource"><see cref="IMessageSource"/> instance to retrieve resources from.</param>
        /// <param name="culture">Resource locale.</param>
        /// <returns>A list of resources to apply.</returns>
        protected abstract IList<Resource> LoadResources(object target, IMessageSource messageSource, CultureInfo culture);

    }
}
