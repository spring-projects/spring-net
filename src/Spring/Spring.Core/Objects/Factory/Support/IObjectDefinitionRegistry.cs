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

using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Interface for registries that hold object definitions, i.e.
    /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
    /// and
    /// <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>
    /// instances.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Typically implemented by object factories that work with the
	/// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition"/>
	/// hierarchy internally.
	/// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
	public interface IObjectDefinitionRegistry
    {
        /// <summary>
        /// Determine whether the given object name is already in use within this registry,
        /// i.e. whether there is a local object or alias registered under this name.
        /// </summary>
	    bool IsObjectNameInUse(string objectName);

	    /// <summary>
	    /// Return the number of objects defined in the registry.
	    /// </summary>
	    /// <value>
	    /// The number of objects defined in the registry.
	    /// </value>
	    int ObjectDefinitionCount { get; }

        /// <summary>
        /// Return the names of all objects defined in this registry.
        /// </summary>
        /// <returns>
        /// The names of all objects defined in this registry, or an empty array
        /// if none defined
        /// </returns>
        IReadOnlyList<string> GetObjectDefinitionNames();

        /// <summary>
        /// Return the names of all objects defined in this registry.
        /// If <code>includeAncestors</code> is <code>true</code> it includes all objects in the defined parent factories.
        /// </summary>
        /// <param name="includeAncestors">to include parent factories in result</param>
        /// <returns>
        /// The names of all objects defined in this registry, if <code>includeAncestors</code> is <code>true</code> it includes
        /// all objects in the defined parent factories, or an empty array if none defined
        /// </returns>
        IReadOnlyList<string> GetObjectDefinitionNames(bool includeAncestors);

        /// <summary>
        /// Check if this registry contains a object definition with the given name.
        /// </summary>
        /// <param name="name">
        /// The name of the object to look for.
        /// </param>
        /// <returns>
        /// True if this object factory contains an object definition with the
        /// given name.
        /// </returns>
        bool ContainsObjectDefinition (string name);

        /// <summary>
        /// Returns the
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>
        /// for the given object name.
        /// </summary>
        /// <param name="name">
        /// The name of the object to find a definition for.
        /// </param>
        /// <returns>
        /// The <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/> for
        /// the given name (never null).
        /// </returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If the object definition cannot be resolved.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        IObjectDefinition GetObjectDefinition (string name);

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
        void RegisterObjectDefinition (string name, IObjectDefinition definition);

	    /// <summary>
	    /// Return the aliases for the given object name, if defined.
	    /// </summary>
	    /// <param name="name">the object name to check for aliases
	    /// </param>
	    /// <remarks>
	    /// <p>
	    /// Will ask the parent factory if the object cannot be found in this
	    /// factory instance.
	    /// </p>
	    /// </remarks>
	    /// <returns>
	    /// The aliases, or an empty array if none.
	    /// </returns>
	    /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
	    /// If there's no such object definition.
	    /// </exception>
	    IReadOnlyList<string> GetAliases (string name);

        /// <summary>
        /// Given a object name, create an alias. We typically use this method to
        /// support names that are illegal within XML ids (used for object names).
        /// </summary>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <param name="theAlias">
        /// The alias that will behave the same as the object name.
        /// </param>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there is no object with the given name.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectDefinitionStoreException">
        /// If the alias is already in use.
        /// </exception>
        void RegisterAlias (string name, string theAlias);
	}
}
