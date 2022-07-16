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
using System.Globalization;
using System.Resources;
using Common.Logging;
using Spring.Context;
using Spring.Context.Support;
using Spring.Expressions;

namespace Spring.Globalization.Localizers
{
    /// <summary>
    /// Loads a list of resources that should be applied from the .NET <see cref="ResourceSet"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This <see cref="ILocalizer"/> implementation will iterate over all resource managers
    /// within the message source and return a list of all the resources whose name starts with '$this'.
    /// </p>
    /// <p>
    /// All other resources will be ignored, but you can retrieve them by calling one of
    /// <c>GetMessage</c> methods on the message source directly.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class ResourceSetLocalizer : AbstractLocalizer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ResourceSetLocalizer));

        private static readonly IList ignoreList =
            new string[] {"$this.DefaultModifiers", "$this.TrayAutoArrange", "$this.TrayLargeIcon"};

        /// <summary>
        /// Loads resources from the storage and creates a list of <see cref="Resource"/> instances that should be applied to the target.
        /// </summary>
        /// <remarks>
        /// This feature is not currently supported on version 1.0 of the .NET platform.
        /// </remarks>
        /// <param name="target">Target to get a list of resources for.</param>
        /// <param name="messageSource"><see cref="IMessageSource"/> instance to retrieve resources from.</param>
        /// <param name="culture">Resource locale.</param>
        /// <returns>A list of resources to apply.</returns>
        protected override IList<Resource> LoadResources(object target, IMessageSource messageSource, CultureInfo culture)
        {
            IList<Resource> resources = new List<Resource>();

            if (messageSource is ResourceSetMessageSource)
            {
                for (int i = 0; i < ((ResourceSetMessageSource) messageSource).ResourceManagers.Count; i++)
                {
                    ResourceManager rm = ((ResourceSetMessageSource) messageSource).ResourceManagers[i] as ResourceManager;
                    ResourceSet invariantResources = null;
                    try
                    {
                        invariantResources = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
                    }
                    catch (MissingManifestResourceException mmrex)
                    {
                        // ignore but log missing ResourceSet
                        log.Debug("No ResourceSet available for invariant culture", mmrex);
                    }

                    if (invariantResources != null)
                    {
                        foreach (DictionaryEntry resource in invariantResources)
                        {
                            string resourceName = (string)resource.Key;
                            if (resourceName.StartsWith("$this") && !ignoreList.Contains(resourceName))
                            {
                                // redirect resource resolution if necessary
                                object resourceValue = rm.GetObject(resourceName, culture);
                                if (resourceValue is String && ((String)resourceValue).StartsWith("$messageSource"))
                                {
                                    resourceValue = messageSource.GetResourceObject(((String)resourceValue).Substring(15), culture);
                                }
                                resources.Add(new Resource(Expression.ParsePrimary(resourceName.Substring(6)), resourceValue));
                            }
                        }
                    }
                }
            }
            return resources;
        }
    }
}
