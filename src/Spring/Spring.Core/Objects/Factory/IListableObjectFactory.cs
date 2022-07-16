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

namespace Spring.Objects.Factory
{
	/// <summary>
	/// Extension of the <see cref="Spring.Objects.Factory.IObjectFactory"/> interface
	/// to be implemented by object factories that can enumerate all their object instances,
	/// rather than attempting object lookup by name one by one as requested by clients.
	/// </summary>
	/// <remarks>
	/// <p>
	/// <see cref="Spring.Objects.Factory.IObjectFactory"/> implementations that preload
	/// all their objects (for example, DOM-based XML factories) may implement this
	/// interface. This interface is discussed in
	/// "Expert One-on-One J2EE Design and Development", by Rod Johnson.
	/// </p>
	/// <p>
	/// If this is an <see cref="Spring.Objects.Factory.IHierarchicalObjectFactory"/>,
	/// the return values will not take any
	/// <see cref="Spring.Objects.Factory.IObjectFactory"/> hierarchy into account, but
	/// will relate only to the objects defined in the current factory.
	/// Use the <see cref="Spring.Objects.Factory.ObjectFactoryUtils"/> helper class to
	/// get all objects.
	/// </p>
	/// <p>
	/// With the exception of
	/// <see cref="Spring.Objects.Factory.IListableObjectFactory.ObjectDefinitionCount"/>,
	/// the methods and properties in this interface are not designed for frequent
	/// invocation. Implementations may be slow.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Rick Evans (.NET)</author>
	public interface IListableObjectFactory : IObjectFactory
	{
        /// <summary>
		/// Check if this object factory contains an object definition with the given name.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Does not consider any hierarchy this factory may participate in.
		/// </p>
		/// <note>
		/// Ignores any singleton objects that have been registered by other means
		/// than object definitions.
		/// </note>
		/// </remarks>
		/// <param name="name">The name of the object to look for.</param>
		/// <returns>
		/// <see langword="true"/> if this object factory contains an object
		/// definition with the given name.
		/// </returns>
		bool ContainsObjectDefinition(string name);

		/// <summary>
		/// Return the number of objects defined in the factory.
		/// </summary>
		/// <value>
		/// The number of objects defined in the factory.
		/// </value>
		int ObjectDefinitionCount { get; }


	    /// <summary>
	    /// Return the names of all objects defined in this factory.
	    /// </summary>
	    /// <returns>
	    /// The names of all objects defined in this factory, or an empty array if none
	    /// are defined.
	    /// </returns>
	    IReadOnlyList<string> GetObjectDefinitionNames();

        /// <summary>
        /// Return the names of all objects defined in this factory, if <code>includeAncestors</code> is <code>true</code>
        /// includes all parent factories.
        /// </summary>
        /// <param name="includeAncestors">to include parent factories in result</param>
        /// <returns>
        /// The names of all objects defined in this factory, if <code>includeAncestors</code> is <code>true</code> includes all
        /// objects defined in parent factories, or an empty array if none are defined.
        /// </returns>
        IReadOnlyList<string> GetObjectDefinitionNames(bool includeAncestors);

	    /// <summary>
	    /// Return the names of objects matching the given <see cref="System.Type"/>
	    /// (including subclasses), judging from the object definitions.
	    /// </summary>
	    /// <remarks>
	    /// <p>
	    /// Does consider objects created by <see cref="Spring.Objects.Factory.IFactoryObject"/>s,
	    /// or rather it considers the type of objects created by
	    /// <see cref="Spring.Objects.Factory.IFactoryObject"/> (which means that
	    /// <see cref="Spring.Objects.Factory.IFactoryObject"/>s will be instantiated).
	    /// </p>
	    /// <p>
	    /// Does not consider any hierarchy this factory may participate in.
	    /// </p>
	    /// </remarks>
	    /// <param name="type">
	    /// The <see cref="System.Type"/> (class or interface) to match, or <see langword="null"/>
	    /// for all object names.
	    /// </param>
	    /// <returns>
	    /// The names of all objects defined in this factory, or an empty array if none
	    /// are defined.
	    /// </returns>
	    IReadOnlyList<string> GetObjectNamesForType(Type type);

	    /// <summary>
	    /// Return the names of objects matching the given <see cref="System.Type"/>
	    /// (including subclasses), judging from the object definitions.
	    /// </summary>
	    /// <remarks>
	    /// <p>
	    /// Does consider objects created by <see cref="Spring.Objects.Factory.IFactoryObject"/>s,
	    /// or rather it considers the type of objects created by
	    /// <see cref="Spring.Objects.Factory.IFactoryObject"/> (which means that
	    /// <see cref="Spring.Objects.Factory.IFactoryObject"/>s will be instantiated).
	    /// </p>
	    /// <p>
	    /// Does not consider any hierarchy this factory may participate in.
	    /// </p>
	    /// </remarks>
	    /// <typeparam name="T">
	    /// The <see cref="System.Type"/> (class or interface) to match, or <see langword="null"/>
	    /// for all object names.
	    /// </typeparam>
	    /// <returns>
	    /// The names of all objects defined in this factory, or an empty array if none
	    /// are defined.
	    /// </returns>
	    IReadOnlyList<string> GetObjectNames<T>();

	    /// <summary>
	    /// Return the names of objects matching the given <see cref="System.Type"/>
	    /// (including subclasses), judging from the object definitions.
	    /// </summary>
	    /// <remarks>
	    /// <p>
	    /// Does consider objects created by <see cref="Spring.Objects.Factory.IFactoryObject"/>s,
	    /// or rather it considers the type of objects created by
	    /// <see cref="Spring.Objects.Factory.IFactoryObject"/> (which means that
	    /// <see cref="Spring.Objects.Factory.IFactoryObject"/>s will be instantiated).
	    /// </p>
	    /// <p>
	    /// Does not consider any hierarchy this factory may participate in.
	    /// Use <see cref="ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(Spring.Objects.Factory.IListableObjectFactory,System.Type,bool,bool)"/>
	    /// to include beans in ancestor factories too.
	    /// &lt;p&gt;Note: Does &lt;i&gt;not&lt;/i&gt; ignore singleton objects that have been registered
	    /// by other means than bean definitions.
	    /// </p>
	    /// </remarks>
	    /// <param name="type">
	    /// The <see cref="System.Type"/> (class or interface) to match, or <see langword="null"/>
	    /// for all object names.
	    /// </param>
	    /// <param name="includePrototypes">
	    /// Whether to include prototype objects too or just singletons (also applies to
	    /// <see cref="Spring.Objects.Factory.IFactoryObject"/>s).
	    /// </param>
	    /// <param name="includeFactoryObjects">
	    /// Whether to include <see cref="Spring.Objects.Factory.IFactoryObject"/>s too
	    /// or just normal objects.
	    /// </param>
	    /// <returns>
	    /// The names of all objects defined in this factory, or an empty array if none
	    /// are defined.
	    /// </returns>
	    IReadOnlyList<string> GetObjectNamesForType(Type type, bool includePrototypes, bool includeFactoryObjects);

		/// <summary>
		/// Return the names of objects matching the given <see cref="System.Type"/>
		/// (including subclasses), judging from the object definitions.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Does consider objects created by <see cref="Spring.Objects.Factory.IFactoryObject"/>s,
		/// or rather it considers the type of objects created by
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/> (which means that
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/>s will be instantiated).
		/// </p>
		/// <p>
		/// Does not consider any hierarchy this factory may participate in.
		/// Use <see cref="ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(Spring.Objects.Factory.IListableObjectFactory,System.Type,bool,bool)"/>
		/// to include beans in ancestor factories too.
		/// &lt;p&gt;Note: Does &lt;i&gt;not&lt;/i&gt; ignore singleton objects that have been registered
		/// by other means than bean definitions.
		/// </p>
		/// </remarks>
		/// <typeparam name="T">
		/// The <see cref="System.Type"/> (class or interface) to match, or <see langword="null"/>
		/// for all object names.
		/// </typeparam>
		/// <param name="includePrototypes">
		///     Whether to include prototype objects too or just singletons (also applies to
		///     <see cref="Spring.Objects.Factory.IFactoryObject"/>s).
		/// </param>
		/// <param name="includeFactoryObjects">
		///     Whether to include <see cref="Spring.Objects.Factory.IFactoryObject"/>s too
		///     or just normal objects.
		/// </param>
		/// <returns>
		/// The names of all objects defined in this factory, or an empty array if none
		/// are defined.
		/// </returns>
		IReadOnlyList<string> GetObjectNames<T>(bool includePrototypes, bool includeFactoryObjects);

		/// <summary>
		/// Return the object instances that match the given object
		/// <see cref="System.Type"/> (including subclasses), judging from either object
		/// definitions or the value of
		/// <see cref="Spring.Objects.Factory.IFactoryObject.ObjectType"/> in the case of
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/>s.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This version of the <see cref="IListableObjectFactory.GetObjectsOfType(Type,bool,bool)"/>
		/// method matches all kinds of object definitions, be they singletons, prototypes, or
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/>s. Typically, the results
		/// of this method call will be the same as a call to
		/// <code>IListableObjectFactory.GetObjectsOfType(type,true,true)</code> .
		/// </p>
		/// </remarks>
		/// <param name="type">
		/// The <see cref="System.Type"/> (class or interface) to match.
		/// </param>
		/// <returns>
		/// A <see cref="System.Collections.IDictionary"/> of the matching objects,
		/// containing the object names as keys and the corresponding object instances
		/// as values.
		/// </returns>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the objects could not be created.
		/// </exception>
		IReadOnlyDictionary<string, object> GetObjectsOfType(Type type);

        /// <summary>
		/// Return the object instances that match the given object
		/// <see cref="System.Type"/> (including subclasses), judging from either object
		/// definitions or the value of
		/// <see cref="Spring.Objects.Factory.IFactoryObject.ObjectType"/> in the case of
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/>s.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This version of the <see cref="IListableObjectFactory.GetObjectsOfType(Type,bool,bool)"/>
		/// method matches all kinds of object definitions, be they singletons, prototypes, or
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/>s. Typically, the results
		/// of this method call will be the same as a call to
		/// <code>IListableObjectFactory.GetObjectsOfType(type,true,true)</code> .
		/// </p>
		/// </remarks>
		/// <typeparam name="T">
		/// The <see cref="System.Type"/> (class or interface) to match.
		/// </typeparam>
		/// <returns>
		/// A <see cref="System.Collections.IDictionary"/> of the matching objects,
		/// containing the object names as keys and the corresponding object instances
		/// as values.
		/// </returns>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the objects could not be created.
		/// </exception>
        IReadOnlyDictionary<string, T> GetObjects<T>();

		/// <summary>
		/// Return the object instances that match the given object
		/// <see cref="System.Type"/> (including subclasses), judging from either object
		/// definitions or the value of
		/// <see cref="Spring.Objects.Factory.IFactoryObject.ObjectType"/> in the case of
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/>s.
		/// </summary>
		/// <param name="type">
		/// The <see cref="System.Type"/> (class or interface) to match.
		/// </param>
		/// <param name="includePrototypes">
		/// Whether to include prototype objects too or just singletons (also applies to
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/>s).
		/// </param>
		/// <param name="includeFactoryObjects">
		/// Whether to include <see cref="Spring.Objects.Factory.IFactoryObject"/>s too
		/// or just normal objects.
		/// </param>
		/// <returns>
		/// A <see cref="System.Collections.IDictionary"/> of the matching objects,
		/// containing the object names as keys and the corresponding object instances
		/// as values.
		/// </returns>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the objects could not be created.
		/// </exception>
		IReadOnlyDictionary<string, object> GetObjectsOfType(Type type, bool includePrototypes, bool includeFactoryObjects);

	    /// <summary>
	    /// Return the object instances that match the given object
	    /// <see cref="System.Type"/> (including subclasses), judging from either object
	    /// definitions or the value of
	    /// <see cref="Spring.Objects.Factory.IFactoryObject.ObjectType"/> in the case of
	    /// <see cref="Spring.Objects.Factory.IFactoryObject"/>s.
	    /// </summary>
	    /// <typeparam name="T">
	    /// The <see cref="System.Type"/> (class or interface) to match.
	    /// </typeparam>
	    /// <param name="includePrototypes">
	    ///   Whether to include prototype objects too or just singletons (also applies to
	    ///   <see cref="Spring.Objects.Factory.IFactoryObject"/>s).
	    /// </param>
	    /// <param name="includeFactoryObjects">
	    ///   Whether to include <see cref="Spring.Objects.Factory.IFactoryObject"/>s too
	    ///   or just normal objects.
	    /// </param>
	    /// <returns>
	    /// A <see cref="System.Collections.IDictionary"/> of the matching objects,
	    /// containing the object names as keys and the corresponding object instances
	    /// as values.
	    /// </returns>
	    /// <exception cref="Spring.Objects.ObjectsException">
	    /// If the objects could not be created.
	    /// </exception>
	    IReadOnlyDictionary<string, T> GetObjects<T>(bool includePrototypes, bool includeFactoryObjects);
	}
}
