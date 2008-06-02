#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

	}
}
