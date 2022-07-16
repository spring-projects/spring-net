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

using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Strategy interface for determining whether a specific object definition
    /// qualifies as an autowire candidate for a specific dependency.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Juergen hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface IAutowireCandidateResolver
    {
        /// <summary>
        /// Determines whether the given object definition qualifies as an
        /// autowire candidate for the given dependency.
        /// </summary>
        /// <param name="odHolder">The object definition including object name and aliases.</param>
        /// <param name="descriptor">The descriptor for the target method parameter or field.</param>
        /// <returns>
        /// 	<c>true</c> if the object definition qualifies as autowire candidate; otherwise, <c>false</c>.
        /// </returns>
        bool IsAutowireCandidate(ObjectDefinitionHolder odHolder, DependencyDescriptor descriptor);


        /// <summary>
        /// Determine whether a default value is suggested for the given dependency.
        /// </summary>
        /// <param name="descriptor">The descriptor for the target method parameter or field</param>
        /// <returns>The value suggested (typically an expression String),
        /// or <c>null</c> if none found
        /// </returns>
        Object GetSuggestedValue(DependencyDescriptor descriptor);

    }
}
