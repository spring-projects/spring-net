#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using System;
using System.Reflection;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Extension of the <see cref="IInstantiationAwareObjectPostProcessor"/> interface,
    /// adding a callback for predicting the eventual type of a processed object.
    /// </summary>
    /// <remarks>This interface is a special purpose interface, mainly for
    /// internal use within the framework. In general, application-provided
    /// post-processors should simply implement the plain <see cref="IObjectPostProcessor"/>
    /// interface or derive from the <see cref="InstantiationAwareObjectPostProcessorAdapter"/>
    /// class.  New methods might be added to this interface even in point releases.
    /// </remarks>
    /// <seealso cref="InstantiationAwareObjectPostProcessorAdapter"/>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface SmartInstantiationAwareObjectPostProcessor : IInstantiationAwareObjectPostProcessor
    {
        /// <summary>
        /// Predicts the type of the object to be eventually returned from this
        /// processors <see cref="IInstantiationAwareObjectPostProcessor.PostProcessBeforeInstantiation"/> callback.
        /// </summary>
        /// <param name="objectType">The raw Type of the object.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>The type of the object, or <code>null</code> if not predictable.</returns>
        /// <exception cref="ObjectsException">in case of errors</exception>
        Type PredictObjectType(Type objectType, string objectName);


        /// <summary>
        /// Determines the candidate constructors to use for the given object.
        /// </summary>
        /// <param name="objectType">The raw Type of the object.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>The candidate constructors, or <code>null</code> if none specified</returns>
        /// <exception cref="ObjectsException">in case of errors</exception>
        ConstructorInfo[] DetermineCandidateConstructors(Type objectType, string objectName);

    }
}