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

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Describes an object instance, which has property values, constructor
	/// argument values, and further information supplied by concrete implementations.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This is just a minimal interface: the main intention is to allow
	/// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
	/// (like PropertyPlaceholderConfigurer) to access and modify property values.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	public interface IObjectDefinition
	{
		/// <summary>
		/// Return the property values to be applied to a new instance of the object.
		/// </summary>
		MutablePropertyValues PropertyValues { get; }

		/// <summary>
		/// Return the constructor argument values for this object.
		/// </summary>
		ConstructorArgumentValues ConstructorArgumentValues { get; }

		/// <summary>
		/// Return the event handlers for any events exposed by this object.
		/// </summary>
		EventValues EventHandlerValues { get; }

		/// <summary>
		/// Return a description of the resource that this object definition
		/// came from (for the purpose of showing context in case of errors).
		/// </summary>
		string ResourceDescription { get; }

        /// <summary>
        /// Is this object definition a "template", i.e. not meant to be instantiated
        /// itself but rather just serving as an object definition for configuration 
        /// templates used by <see cref="Spring.Objects.Factory.IObjectFactory.ConfigureObject(object, string)"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this object definition is a "template".
        /// </value>
        bool IsTemplate { get; }

		/// <summary>
		/// Is this object definition "abstract", i.e. not meant to be instantiated
		/// itself but rather just serving as parent for concrete child object
		/// definitions.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this object definition is "abstract".
		/// </value>
		bool IsAbstract { get; }

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
		bool IsSingleton { get; }

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
        bool IsLazyInit { get; }

        /// <summary>
        /// The name of the parent definition of this object definition, if any.
        /// </summary>
        string ParentName { get; set; }

        /// <summary>
        /// The target scope for this object.
        /// </summary>
        string Scope { get; set; }

        /// <summary>
        /// Get the role hint for this object definition
        /// </summary>
        ObjectRole Role { get; }

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
		Type ObjectType { get; }

        /// <summary>
        /// Returns the <see cref="System.Type.FullName"/> of the
        /// <see cref="System.Type"/> of the object definition.
        /// </summary>
        /// <remarks>Note that this does not have to be the actual type name used at runtime,
        /// in case of a child definition overrding/inheriting the the type name from its
        /// parent.  It can be modifed during object factory post-processing, typically 
        /// replacing the original class name with a parsed variant of it.
        /// Hence, do not consider this to be the definitive bean type at runtime
        /// but rather only use it for parsing purposes at the individual object
        /// definition level. 
        /// </remarks>
        string ObjectTypeName { get; set;}

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
        AutoWiringMode AutowireMode { get; }

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
        IReadOnlyList<string> DependsOn { get; }

        /// <summary>
        /// The name of the initializer method.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default is <see langword="null"/>, in which case there is no initializer method.
        /// </p>
        /// </remarks>
        string InitMethodName { get; }

        /// <summary>
        /// Return the name of the destroy method.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default is <see langword="null"/>, in which case there is no destroy method.
        /// </p>
        /// </remarks>
        string DestroyMethodName { get; }

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
        string FactoryMethodName { get; }

        /// <summary>
        /// The name of the factory object to use (if any).
        /// </summary>
        string FactoryObjectName { get; }

        /// <summary>
        /// Gets a value indicating whether this instance a candidate for getting autowired into some other
        /// object.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is autowire candidate; otherwise, <c>false</c>.
        /// </value>
	    bool IsAutowireCandidate { get; }

        /// <summary>
        /// Return whether this bean is a primary autowire candidate.
        /// If this value is true for exactly one bean among multiple
        /// matching candidates, it will serve as a tie-breaker.
        /// </summary>
	    bool IsPrimary { get; }
	}
}