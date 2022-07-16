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

using Spring.Core;
using System.Reflection;

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

        private PropertyInfo property;

        private FieldInfo field;

        private readonly bool required;

        private readonly bool eager;


        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyDescriptor"/> class for a method or constructor parameter.
        /// Considers the dependency as 'eager'
        /// </summary>
        /// <param name="methodParameter">The MethodParameter to wrap.</param>
        /// <param name="required">if set to <c>true</c> if the dependency is required.</param>
        public DependencyDescriptor(MethodParameter methodParameter, bool required)
            : this(methodParameter, required, true)
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
        /// Create a new descriptor for a property.
        /// Considers the dependency as 'eager'.
        /// <param name="property">property to wrap</param>
        /// <param name="required">required whether the dependency is required</param>
        /// </summary>
        public DependencyDescriptor(PropertyInfo property, bool required)
            : this(property, required, true)
        {
        }

        /// <summary>
        /// Create a new descriptor for a property.
        /// <param name="property">property to wrap</param>
        /// <param name="required ">whether the dependency is required</param>
        /// <param name="eager">whether this dependency is 'eager' in the sense of</param>
        /// eagerly resolving potential target beans for type matching
        /// </summary>
        public DependencyDescriptor(PropertyInfo property, bool required, bool eager)
        {
            this.property = property;
            this.required = required;
            this.eager = eager;
        }

        /// <summary>
        /// Create a new descriptor for a field.
        /// Considers the dependency as 'eager'.
        /// <param name="field">field to wrap</param>
        /// <param name="required">whether the dependency is required</param>
        /// </summary>
        public DependencyDescriptor(FieldInfo field, bool required)
            : this(field, required, true)
        {
        }

        /// <summary>
        /// Create a new descriptor for a field.
        /// <param name="field">field to wrap</param>
        /// <param name="required ">whether the dependency is required</param>
        /// <param name="eager">whether this dependency is 'eager' in the sense of</param>
        /// eagerly resolving potential target beans for type matching
        /// </summary>
        public DependencyDescriptor(FieldInfo field, bool required, bool eager)
        {
            this.field = field;
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
            get
            {
                if (methodParameter != null)
                    return methodParameter.ParameterType;
                if (property != null)
                    return property.PropertyType;
                if (field != null)
                    return field.FieldType;

                return null;
            }
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

        /// <summary>
        /// Gets the Attributes assigned to Field, Property or Paramater
        /// </summary>
        public Attribute[] Attributes
        {
            get
            {
                if (methodParameter != null)
                    return methodParameter.ParameterAttributes;
                if (property != null)
                    return Attribute.GetCustomAttributes(property);
                if (field != null)
                    return Attribute.GetCustomAttributes(field);

                return new Attribute[0];
            }
        }

        /// <summary>
        /// Gets the name of the member info
        /// </summary>
        public string DependencyName
        {
            get
            {
                if (methodParameter != null)
                    return methodParameter.ParameterName();
                if (property != null)
                    return property.Name;
                if (field != null)
                    return field.Name;

                return "";
            }
        }
    }
}
