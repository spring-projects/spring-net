#region License

/*
 * Copyright 2002-2008 the original author or authors.
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
using Spring.Core;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Descriptor for a specific dependency that is about to be injected.
    /// Wraps a constructor parameter, a method parameter or a field,
    /// allowing unified access to their metadata.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack</author>
    public class DependencyDescriptor
    {
        private MethodParameter methodParameter;

        private readonly bool required;

        private readonly bool eager;


        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyDescriptor"/> class for a method or constructor parameter.
        /// Considers the dependency as 'eager'
        /// </summary>
        /// <param name="methodParameter">The MethodParameter to wrap.</param>
        /// <param name="required">if set to <c>true</c> if the dependency is required.</param>
        public DependencyDescriptor(MethodParameter methodParameter, bool required) : this(methodParameter, required, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyDescriptor"/> class for a method or a constructor parameter.
        /// </summary>
        /// <param name="methodParameter">The MethodParameter to wrap.</param>
        /// <param name="required">if set to <c>true</c> the dependency is required.</param>
        /// <param name="eager">if set to <c>true</c> the dependency is 'eager' in the sense of
        /// eagerly resolving potential target objects for type matching.</param>
        public DependencyDescriptor(MethodParameter methodParameter, bool required, bool eager)
        {
            this.methodParameter = methodParameter;
            this.required = required;
            this.eager = eager;
        }


        /// <summary>
        /// Gets a value indicating whether this dependency is required.
        /// </summary>
        /// <value><c>true</c> if required; otherwise, <c>false</c>.</value>
        public bool Required
        {
            get { return required; }
        }

        /// <summary>
        /// Determine the declared (non-generic) type of the wrapped parameter/field.
        /// </summary>
        /// <value>The type of the dependency (never <code>null</code></value>
        public Type DependencyType
        {
            get { return methodParameter.ParameterType; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="DependencyDescriptor"/> is eager in the sense of
        /// eagerly resolving potential target beans for type matching.
        /// </summary>
        /// <value><c>true</c> if eager; otherwise, <c>false</c>.</value>
        public bool Eager
        {
            get { return this.eager; }
        }


        /// <summary>
        /// Gets the wrapped MethodParameter, if any.
        /// </summary>
        /// <value>The method parameter.</value>
        public MethodParameter MethodParameter
        {
            get { return methodParameter; }
        }
    }
}