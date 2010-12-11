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

using Spring.Context;

namespace Spring.Globalization
{
    /// <summary>
    /// Defines an interface that localizers have to implement.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Localizers are used to automatically apply resources to object's members
    /// using reflection.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public interface ILocalizer
    {
        /// <summary>
        /// Gets or sets the resource cache instance.
        /// </summary>
        /// <value>The resource cache instance.</value>
        IResourceCache ResourceCache { get; set; }
        
        /// <summary>
        /// Applies resources of the specified culture to the specified target object.
        /// </summary>
        /// <param name="target">Target object to apply resources to.</param>
        /// <param name="messageSource"><see cref="IMessageSource"/> instance to retrieve resources from.</param>
        /// <param name="culture">Resource culture to use for resource lookup.</param>
        void ApplyResources(object target, IMessageSource messageSource, CultureInfo culture);

        /// <summary>
        /// Applies resources to the specified target object, using current thread's culture to resolve resources.
        /// </summary>
        /// <param name="target">Target object to apply resources to.</param>
        /// <param name="messageSource"><see cref="IMessageSource"/> instance to retrieve resources from.</param>
        void ApplyResources(object target, IMessageSource messageSource);
    }
}