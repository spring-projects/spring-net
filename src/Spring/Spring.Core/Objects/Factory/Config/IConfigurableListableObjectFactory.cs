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

namespace Spring.Objects.Factory.Config
{
	/// <summary>
    /// SPI interface to be implemented by most if not all listable object factories.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Allows for framework-internal plug'n'play, e.g. in
    /// <see cref="Spring.Context.Support.AbstractApplicationContext"/>.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
	public interface IConfigurableListableObjectFactory
        : IListableObjectFactory,
          IConfigurableObjectFactory,
          IAutowireCapableObjectFactory
    {
        /// <summary>
        /// Return the registered
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/> for the
        /// given object, allowing access to its property values and constructor
        /// argument values.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <returns>
        /// The registered
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>.
        /// </returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there is no object with the given name.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of errors.
        /// </exception>
        IObjectDefinition GetObjectDefinition(string name);

        /// <summary>
        /// Return the registered
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/> for the
        /// given object, allowing access to its property values and constructor
        /// argument values.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="includeAncestors">Whether to search parent object factories.</param>
        /// <returns>
        /// The registered
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>.
        /// </returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there is no object with the given name.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of errors.
        /// </exception>
        IObjectDefinition GetObjectDefinition(string name, bool includeAncestors);

        /// <summary>
        /// Register a new object definition with this registry.
        /// Must support
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
        /// and <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the object instance to register.
        /// </param>
        /// <param name="definition">
        /// The definition of the object instance to register.
        /// </param>
        /// <remarks>
        /// <p>
        /// Must support
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/> and
        /// <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object definition is invalid.
        /// </exception>
        void RegisterObjectDefinition(string name, IObjectDefinition definition);

        /// <summary>
        /// Injects dependencies into the supplied <paramref name="target"/> instance
        /// using the supplied <paramref name="definition"/>.
        /// </summary>
        /// <param name="target">
        /// The object instance that is to be so configured.
        /// </param>
        /// <param name="name">
        /// The name of the object definition expressing the dependencies that are to
        /// be injected into the supplied <parameref name="target"/> instance.
        /// </param>
        /// <param name="definition">
        /// An object definition that should be used to configure object.
        /// </param>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.ConfigureObject(object, string)"/>
        object ConfigureObject(object target, string name, IObjectDefinition definition);

        /// <summary>
        /// Ensure that all non-lazy-init singletons are instantiated, also
        /// considering <see cref="Spring.Objects.Factory.IFactoryObject"/>s.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Typically invoked at the end of factory setup, if desired.
        /// </p>
        /// <p>
        /// As this is a startup method, it should destroy already created singletons if
        /// it fails, to avoid dangling resources. In other words, after invocation
        /// of that method, either all or no singletons at all should be
        /// instantiated.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If one of the singleton objects could not be created.
        /// </exception>
        void PreInstantiateSingletons ();

        /// <summary>
        ///  Register a special dependency type with corresponding autowired value.
        /// </summary>
        /// <remarks>
        /// This is intended for factory/context references that are supposed
        /// to be autowirable but are not defined as objects in the factory:
        /// e.g. a dependency of type ApplicationContext resolved to the
        /// ApplicationContext instance that the object is living in.
        /// <para>
        /// Note there are no such default types registered in a plain IObjectFactory,
        /// not even for the IObjectFactory interface itself.
        /// </para>
        /// </remarks>
        /// <param name="dependencyType">Type of the dependency to register.
        /// This will typically be a base interface such as IObjectFactory, with extensions of it resolved
        /// as well if declared as an autowiring dependency (e.g. IListableObjectFactory),
        /// as long as the given value actually implements the extended interface.
        /// </param>
        /// <param name="autowiredValue">The autowired value.  This may also be an
        /// implementation o the <see cref="IObjectFactory"/> interface,
        ///  which allows for lazy resolution of the actual target value.</param>
        void RegisterResolvableDependency(Type dependencyType, object autowiredValue);

        /// <summary>
        /// Determines whether the specified object qualifies as an autowire candidate,
        /// to be injected into other objects which declare a dependency of matching type.
        /// This method checks ancestor factories as well.
        /// </summary>
        /// <param name="objectName">Name of the object to check.</param>
        /// <param name="descriptor">The descriptor of the dependency to resolve.</param>
        /// <returns>
        /// 	<c>true</c> if the object should be considered as an autowire candidate; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="NoSuchObjectDefinitionException">if there is no object with the given name.</exception>
	    bool IsAutowireCandidate(string objectName, DependencyDescriptor descriptor);


	    /// <summary>
	    /// Clear the merged object definition cache, removing entries for objects
	    /// which are not considered eligible for full metadata caching yet.
	    /// </summary>
	    /// <remarks>
	    /// Typically triggered after changes to the original object definitions,
	    /// e.g. after applying a <see cref="IObjectFactoryPostProcessor" />. Note that metadata
	    /// for objects which have already been created at this point will be kept around.
	    /// </remarks>
	    /// <seealso cref="GetObjectDefinition(string)"/>
	    void ClearMetadataCache();

	    /// <summary>
	    /// Freeze all object definitions, signalling that the registered object definitions
		/// will not be modified or post-processed any further.
	    /// </summary>
	    /// <remarks>
	    /// This allows the factory to aggressively cache object definition metadata.
	    /// </remarks>
	    void FreezeConfiguration();

	    /// <summary>
	    /// Return whether this factory's object definitions are frozen,
	    /// i.e. are not supposed to be modified or post-processed any further.
	    /// </summary>
	    bool ConfigurationFrozen { get; }
    }
}
