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

using System;
using System.ComponentModel;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Configuration interface to be implemented by most if not all object
	/// factories.
	/// </summary>
	/// <remarks>
	/// <p>
    /// Provides the means to configure an object factory in addition to the
    /// object factory client methods in the
    /// <see cref="Spring.Objects.Factory.IObjectFactory"/> interface.
	/// </p>
	/// <p>
    /// Allows for framework-internal plug'n'play even when needing access to object
    /// factory configuration methods.
	/// </p>
	/// <p>
	/// When disposed, it will destroy all cached singletons in this factory. Call
	/// <see cref="System.IDisposable.Dispose()"/> when you want to shutdown
	/// the factory.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	/// <version>$Id: IConfigurableObjectFactory.cs,v 1.14 2007/07/16 21:06:21 markpollack Exp $</version>
	public interface IConfigurableObjectFactory : IHierarchicalObjectFactory
	{
		/// <summary>
		/// Set the parent of this object factory.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Note that the parent shouldn't be changed: it should only be set outside
		/// a constructor if it isn't available when an object of this class is
		/// created.
		/// </p>
		/// </remarks>
		new IObjectFactory ParentObjectFactory { set; }

		/// <summary>
		/// Ignore the given dependency type for autowiring.
		/// </summary>
		/// <remarks>
		/// <p>
		/// To be invoked during factory configuration.
		/// </p>
		/// <p>
		/// This will typically be used for dependencies that are resolved
		/// in other ways, like <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// through <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>.
		/// </p>
		/// </remarks>
		/// <param name="type">
		/// The <see cref="System.Type"/> to be ignored.
		/// </param>
		void IgnoreDependencyType(Type type);


        /// <summary>
        /// Determines whether the specified object name is currently in creation..
        /// </summary>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>
        /// 	<c>true</c> if the specified object name is currently in creation; otherwise, <c>false</c>.
        /// </returns>
	    bool IsCurrentlyInCreation(string objectName);

		/// <summary>
		/// Add a new <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>
		/// that will get applied to objects created by this factory.
		/// </summary>
		/// <remarks>
		/// <p>
		/// To be invoked during factory configuration.
		/// </p>
		/// </remarks>
		/// <param name="processor">
		/// The <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>
		/// to register.
		/// </param>
		void AddObjectPostProcessor(IObjectPostProcessor processor);

		/// <summary>
		/// Returns the current number of registered
		/// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>s.
		/// </summary>
		/// <value>
		/// The current number of registered
		/// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>s.
		/// </value>
		int ObjectPostProcessorCount 
		{
			get;
		}

		/// <summary>
		/// Given an object name, create an alias.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is typically used to support names that are illegal within
		/// XML ids (which are used for object names).
		/// </p>
		/// <p>
		/// Typically invoked during factory configuration, but can also be
		/// used for runtime registration of aliases. Therefore, a factory
		/// implementation should synchronize alias access.
		/// </p>
		/// </remarks>
		/// <param name="name">The name of the object.
		/// </param>
		/// <param name="theAlias">
		/// The alias that will behave the same as the object name.
		/// </param>
		/// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
		/// If there is no object with the given name.
		/// </exception>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the alias is already in use.
		/// </exception>
		void RegisterAlias(string name, string theAlias);

		/// <summary>
		/// Register the given existing object as singleton in the object factory,
		/// under the given object name.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Typically invoked during factory configuration, but can also be
		/// used for runtime registration of singletons. Therefore, a factory
		/// implementation should synchronize singleton access; it will have
		/// to do this anyway if it supports lazy initialization of singletons.
		/// </p>
		/// </remarks>
		/// <param name="name">
		/// The name of the object.
		/// </param>
		/// <param name="singleton">The existing object.</param>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the singleton could not be registered.
		/// </exception>
		void RegisterSingleton(string name, object singleton);

		/// <summary>
		/// Register the given custom <see cref="System.ComponentModel.TypeConverter"/>
		/// for all properties of the given <see cref="System.Type"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// To be invoked during factory configuration.
		/// </p>
		/// </remarks>
		/// <param name="requiredType">
		/// The required <see cref="System.Type"/> of the property.
		/// </param>
		/// <param name="converter">
		/// The <see cref="System.ComponentModel.TypeConverter"/> to register.
		/// </param>
		void RegisterCustomConverter(Type requiredType, TypeConverter converter);

		/// <summary>
		/// Does this object factory contains a singleton instance with the
		/// supplied <paramref name="name"/>?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Only checks already instantiated singletons; does not return
		/// <see langword="true"/> for singleton object definitions that have
		/// not been instantiated yet.
		/// </p>
		/// <p>
		/// The main purpose of this method is to check manually registered
		/// singletons (<see cref="RegisterSingleton(string, object)"/>). This
		/// method can also be used to check whether a singleton defined by an
		/// object definition has already been created.
		/// </p>
		/// <p>
		/// To check whether an object factory contains an object definition
		/// with a given name, use the
		/// <see cref="Spring.Objects.Factory.IListableObjectFactory.ContainsObjectDefinition(string)"/>
		/// method. Calling both
		/// <see cref="Spring.Objects.Factory.IListableObjectFactory.ContainsObjectDefinition(string)"/>
		/// and <see cref="ContainsSingleton(string)"/> definitively answers
		/// the question of whether a specific object factory contains a
		/// singleton object with the given name.
		/// </p>
		/// <p>
		/// Use the
		/// <see cref="Spring.Objects.Factory.IObjectFactory.ContainsObject(string)"/>
		/// method for general checks as to whether a factory knows about an
		/// object with a given name (regrdless of whether the object in
		/// question is a manually registed singleton instance or created by
		/// an object definition)... this also has the happy bonus of also
		/// checking any ancestor factories.
		/// </p>
		/// </remarks>
		/// <param name="name">
		/// The name of the (singleton) object to look for.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if this object factory contains a singleton
		/// instance with the given <paramref name="name"/>.
		/// </returns>
		/// <seealso cref="Spring.Objects.Factory.IObjectFactory.ContainsObject(string)"/>
		/// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.ContainsObjectDefinition(string)"/>
		bool ContainsSingleton(string name);
	}
}
