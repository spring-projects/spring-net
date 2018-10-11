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

#region Imports

using System;
using System.Collections.Generic;

using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Describes a configurable object instance, which has property values,
    /// constructor argument values, and further information supplied by concrete
    /// implementations.
    /// </summary>
    /// <author>Rick Evans</author>
    public interface IConfigurableObjectDefinition : IObjectDefinition
    {
        /// <summary>
        /// Return the property values to be applied to a new instance of the object.
        /// </summary>
        new MutablePropertyValues PropertyValues { get; set; }

        /// <summary>
        /// Return the constructor argument values for this object.
        /// </summary>
        new ConstructorArgumentValues ConstructorArgumentValues { get; set; }
		
        /// <summary>
        /// The method overrides (if any) for this object.
        /// </summary>
        /// <value>
        /// The method overrides (if any) for this object; may be an
        /// empty collection but is guaranteed not to be
        /// <see langword="null"/>.
        /// </value>
        MethodOverrides MethodOverrides { get; set; }

        /// <summary>
        /// Return the event handlers for any events exposed by this object.
        /// </summary>
        new EventValues EventHandlerValues { get; set; }

        /// <summary>
        /// Get or set the role hint for this object definition
        /// </summary>
        new ObjectRole Role { get; set; }

        /// <summary>
        /// Return a description of the resource that this object definition
        /// came from (for the purpose of showing context in case of errors).
        /// </summary>
        new string ResourceDescription { get; set; }

        /// <summary>
        /// Is this object definition "abstract", i.e. not meant to be instantiated
        /// itself but rather just serving as parent for concrete child object
        /// definitions.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this object definition is "abstract".
        /// </value>
        new bool IsAbstract { get; set; }

        /// <summary>
        /// Returns the <see cref="System.Type"/> of the object definition (if any).
        /// </summary>
        /// <value>
        /// A resolved object <see cref="System.Type"/>.
        /// </value>
        /// <exception cref="ApplicationException">
        /// If the <see cref="System.Type"/> of the object definition is not a
        /// resolved <see cref="System.Type"/> or <see langword="null"/>.
        /// </exception>
        new Type ObjectType { get; set; }

        /// <summary>
        /// Returns the <see cref="System.Type.FullName"/> of the
        /// <see cref="System.Type"/> of the object definition (if any).
        /// </summary>
        new string ObjectTypeName { get; set; }
        
        /// <summary>
        /// Return whether this a <b>Singleton</b>, with a single, shared instance
        /// returned on all calls.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If <see langword="false"/>, an object factory will apply the <b>Prototype</b>
        /// design pattern, with each caller requesting an instance getting an
        /// independent instance. How this is defined will depend on the
        /// object factory implementation. <b>Singletons</b> are the commoner type.
        /// </p>
        /// </remarks>
        new bool IsSingleton { get; set; }

        /// <summary>
        /// Is this object lazily initialized?</summary>
        /// <remarks>
        /// <p>
        /// Only applicable to a singleton object.
        /// </p>
        /// <p>
        /// If <see langword="false"/>, it will get instantiated on startup by object factories
        /// that perform eager initialization of singletons.
        /// </p>
        /// </remarks>
        new bool IsLazyInit { get; set; }

        /// <summary>
        /// The autowire mode as specified in the object definition.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This determines whether any automagical detection and setting of
        /// object references will happen. Default is
        /// <see cref="Spring.Objects.Factory.Config.AutoWiringMode.No"/>,
        /// which means there's no autowire.
        /// </p>
        /// </remarks>
        new AutoWiringMode AutowireMode { get; set; }

        /// <summary>
        /// The dependency check code.
        /// </summary>
        DependencyCheckingMode DependencyCheck { get; set; }

        /// <summary>
        /// The object names that this object depends on.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The object factory will guarantee that these objects get initialized
        /// before.
        /// </p>
        /// <p>
        /// Note that dependencies are normally expressed through object properties
        /// or constructor arguments. This property should just be necessary for
        /// other kinds of dependencies like statics (*ugh*) or database
        /// preparation on startup.
        /// </p>
        /// </remarks>
        new IReadOnlyList<string> DependsOn { get; set; }

        /// <summary>
        /// The name of the initializer method.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default is <see langword="null"/>, in which case there is no initializer method.
        /// </p>
        /// </remarks>
        new string InitMethodName { get; set; }

        /// <summary>
        /// Return the name of the destroy method.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default is <see langword="null"/>, in which case there is no destroy method.
        /// </p>
        /// </remarks>
        new string DestroyMethodName { get; set; }

        /// <summary>
        /// The name of the factory method to use (if any).
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method will be invoked with constructor arguments, or with no
        /// arguments if none are specified. The static method will be invoked on
        /// the specified <see cref="Spring.Objects.Factory.Config.IObjectDefinition.ObjectType"/>.
        /// </p>
        /// </remarks>
        new string FactoryMethodName { get; set; }

        /// <summary>
        /// The name of the factory object to use (if any).
        /// </summary>
        new string FactoryObjectName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance a candidate for getting autowired into some other
        /// object.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is autowire candidate; otherwise, <c>false</c>.
        /// </value>
        new bool IsAutowireCandidate { get; set; }
    }
}