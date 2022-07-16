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

using System.ComponentModel;
using Spring.Util;

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
	public interface IConfigurableObjectFactory : IHierarchicalObjectFactory, ISingletonObjectRegistry
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
        /// Add a String resolver for embedded values such as annotation attributes.
        /// </summary>
        /// <param name="valueResolver">the String resolver to apply to embedded values</param>
        void AddEmbeddedValueResolver(IStringValueResolver valueResolver);

        /// <summary>
        /// Resolve the given embedded value, e.g. an annotation attribute.
        /// </summary>
        /// <param name="value">the value to resolve</param>
        /// <returns>the resolved value (may be the original value as-is)</returns>
        string ResolveEmbeddedValue(string value);
	}
}
