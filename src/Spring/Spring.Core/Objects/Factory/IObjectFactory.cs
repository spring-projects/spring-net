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

namespace Spring.Objects.Factory
{
	/// <summary>
	/// The root interface for accessing a Spring.NET IoC container.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is the basic client view of a Spring.NET IoC container; further interfaces
	/// such as <see cref="Spring.Objects.Factory.IListableObjectFactory"/> and
	/// <see cref="Spring.Objects.Factory.Config.IConfigurableObjectFactory"/>
	/// are available for specific purposes such as enumeration and configuration.
	/// </para>
	/// <para>
	/// This is the root interface to be implemented by objects that can hold a number
	/// of object definitions, each uniquely identified by a <see cref="System.String"/>
	/// name. An independent instance of any of these objects can be obtained
	/// (the Prototype design pattern), or a single shared instance can be obtained
	/// (a superior alternative to the Singleton design pattern, in which the instance is a
	/// singleton in the scope of the factory). Which type of instance
	/// will be returned depends on the object factory configuration - the API is the same.
	/// The Singleton approach is more useful and hence more common in practice.
	/// </para>
	/// <para>
	/// The point of this approach is that the IObjectFactory is a central registry of
	/// application components, and centralizes the configuring of application components
	/// (no more do individual objects need to read properties files, for example).
	/// See chapters 4 and 11 of "Expert One-on-One J2EE Design and Development" for a
	/// discussion of the benefits of this approach.
	/// </para>
	/// <para>
	/// Normally an IObjectFactory will load object definitions stored in a configuration
	/// source (such as an XML document), and use the <see cref="Spring.Objects"/>
	/// namespace to configure the objects. However, an implementation could simply return
	/// .NET objects it creates as necessary directly in .NET code. There are no
	/// constraints on how the definitions could be stored: LDAP, RDBMS, XML, properties
	/// file etc. Implementations are encouraged to support references amongst objects,
	/// to either Singletons or Prototypes.
	/// </para>
	/// <para>
	/// In contrast to the methods in
	/// <see cref="Spring.Objects.Factory.IListableObjectFactory"/>, all of the methods
	/// in this interface will also check parent factories if this is an
	/// <see cref="Spring.Objects.Factory.IHierarchicalObjectFactory"/>. If an object is
	/// not found in this factory instance, the immediate parent is asked. Objects in
	/// this factory instance are supposed to override objects of the same name in any
	/// parent factory.
	/// </para>
	/// <para>
	/// Object factories are supposed to support the standard object lifecycle interfaces
	/// as far as possible. The maximum set of initialization methods and their standard
	/// order is:
	/// </para>
	/// <para>
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// <see cref="Spring.Objects.Factory.IObjectNameAware"/>'s
	/// <see cref="Spring.Objects.Factory.IObjectNameAware.ObjectName"/> property.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>'s
	/// <see cref="Spring.Objects.Factory.IObjectFactoryAware.ObjectFactory"/> property.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// <see cref="Spring.Context.IApplicationContextAware.ApplicationContext"/>
	/// (only applicable if running within an <see cref="Spring.Context.IApplicationContext"/>).
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// The
	/// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessBeforeInitialization"/>
	/// method of
	/// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>s.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
	/// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/> method.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// A custom init-method definition.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// The
	/// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessAfterInitialization"/>
	/// method of
	/// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>s.
	/// </description>
	/// </item>
	/// </list>
	/// </para>
	/// <p/>
	/// <para>
	/// On shutdown of an object factory, the following lifecycle methods apply:
	/// </para>
	/// <para>
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// <see cref="System.IDisposable"/>'s
	/// <see cref="System.IDisposable.Dispose"/> method.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// A custom destroy-method definition.
	/// </description>
	/// </item>
	/// </list>
	/// </para>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	public interface IObjectFactory : IDisposable
	{
        /// <summary>
        /// Determine whether this object factory treats object names case-sensitive or not.
        /// </summary>
        bool IsCaseSensitive { get; }

        /// <summary>
		/// Is this object a singleton?
		/// </summary>
		/// <remarks>
		/// <para>
		/// That is, will <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>
		/// always return the same object?
		/// </para>
		/// <para>
		/// Will ask the parent factory if the object cannot be found in this factory
		/// instance.
		/// </para>
		/// </remarks>
		/// <param name="name">The name of the object to query.</param>
		/// <returns>True if the named object is a singleton.</returns>
		/// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
		/// If there's no such object definition.
		/// </exception>
		bool IsSingleton(string name);


        /// <summary>
        /// Determines whether the specified object name is prototype.  That is, will GetObject
        /// always return independent instances?
        /// </summary>
        /// <remarks>This method returning false does not clearly indicate a singleton object.
        /// It indicated non-independent instances, which may correspond to a scoped object as 
        /// well.  use the IsSingleton property to explicitly check for a shared 
        /// singleton instance.
        /// <para>Translates aliases back to the corresponding canonical object name.  Will ask the
        /// parent factory if the object can not be found in this factory instance.
        /// </para>
        /// </remarks>
        /// 
        /// <param name="name">The name of the object to query</param>
        /// <returns>
        /// 	<c>true</c> if the specified object name will always deliver independent instances; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="NoSuchObjectDefinitionException">if there is no object with the given name.</exception>
	    bool IsPrototype(string name);

		/// <summary>
		/// Does this object factory contain an object with the given name?
		/// </summary>
		/// <remarks>
		/// <para>
		/// The concrete lookup strategy depends on the implementation. E.g. <see cref="IHierarchicalObjectFactory"/>s
		/// will also search their parent factory if a name isn't found .
		/// </para>
		/// </remarks>
		/// <param name="name">The name of the object to query.</param>
		/// <returns>True if an object with the given name is defined.</returns>
		bool ContainsObject(string name);

	    /// <summary>
	    /// Return the aliases for the given object name, if defined.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// Will ask the parent factory if the object cannot be found in this factory
	    /// instance.
	    /// </para>
	    /// </remarks>
	    /// <param name="name">The object name to check for aliases.</param>
	    /// <returns>The aliases, or an empty array if none.</returns>
	    /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
	    /// If there's no such object definition.
	    /// </exception>
	    IList<string> GetAliases(string name);

#if !MONO
		/// <summary>
		/// Return an instance (possibly shared or independent) of the given object name.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method allows an object factory to be used as a replacement for the
		/// Singleton or Prototype design pattern.
		/// </para>
		/// <para>
		/// Note that callers should retain references to returned objects. There is no
		/// guarantee that this method will be implemented to be efficient. For example,
		/// it may be synchronized, or may need to run an RDBMS query.
		/// </para>
		/// <para>
		/// Will ask the parent factory if the object cannot be found in this factory
		/// instance.
		/// </para>
		/// <para>
		/// This is the indexer for the <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// interface.
		/// </para>
		/// </remarks>
		/// <param name="name">The name of the object to return.</param>
		/// <returns>The instance of the object.</returns>
		/// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
		/// If there's no such object definition.
		/// </exception>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the object could not be created.
		/// </exception>
#endif
        object this[string name] { get; }

        /// <summary>
        /// Return an instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method allows an object factory to be used as a replacement for the
        /// Singleton or Prototype design pattern.
        /// </para>
        /// <para>
        /// Note that callers should retain references to returned objects. There is no
        /// guarantee that this method will be implemented to be efficient. For example,
        /// it may be synchronized, or may need to run an RDBMS query.
        /// </para>
        /// <para>
        /// Will ask the parent factory if the object cannot be found in this factory
        /// instance.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <returns>The instance of the object.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectDefinitionStoreException">
        /// If there is more than a single object of the requested type defined in the factory.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object could not be created.
        /// </exception>
        T GetObject<T>();

		/// <summary>
		/// Return an instance (possibly shared or independent) of the given object name.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method allows an object factory to be used as a replacement for the
		/// Singleton or Prototype design pattern.
		/// </para>
		/// <para>
		/// Note that callers should retain references to returned objects. There is no
		/// guarantee that this method will be implemented to be efficient. For example,
		/// it may be synchronized, or may need to run an RDBMS query.
		/// </para>
		/// <para>
		/// Will ask the parent factory if the object cannot be found in this factory
		/// instance.
		/// </para>
		/// </remarks>
		/// <param name="name">The name of the object to return.</param>
		/// <returns>The instance of the object.</returns>
		/// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
		/// If there's no such object definition.
		/// </exception>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the object could not be created.
		/// </exception>
		object GetObject(string name);
		
        /// <summary>
		/// Return an instance (possibly shared or independent) of the given object name.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method allows an object factory to be used as a replacement for the
		/// Singleton or Prototype design pattern.
		/// </para>
		/// <para>
		/// Note that callers should retain references to returned objects. There is no
		/// guarantee that this method will be implemented to be efficient. For example,
		/// it may be synchronized, or may need to run an RDBMS query.
		/// </para>
		/// <para>
		/// Will ask the parent factory if the object cannot be found in this factory
		/// instance.
		/// </para>
		/// </remarks>
		/// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="name">The name of the object to return.</param>
		/// <returns>The instance of the object.</returns>
		/// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
		/// If there's no such object definition.
		/// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectNotOfRequiredTypeException">
        /// If the object is not of the required type.
		/// </exception>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the object could not be created.
		/// </exception>
		T GetObject<T>(string name);

        /// <summary>
        /// Return an instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method allows an object factory to be used as a replacement for the
        /// Singleton or Prototype design pattern.
        /// </para>
        /// <para>
        /// Note that callers should retain references to returned objects. There is no
        /// guarantee that this method will be implemented to be efficient. For example,
        /// it may be synchronized, or may need to run an RDBMS query.
        /// </para>
        /// <para>
        /// Will ask the parent factory if the object cannot be found in this factory
        /// instance.
        /// </para>
        /// </remarks>
        /// <param name="name">The name of the object to return.</param>
        /// <param name="arguments">
        /// The arguments to use if creating a prototype using explicit arguments to
        /// a static factory method. If there is no factory method and the
        /// arguments are not null, then match the argument values by type and
        /// call the object's constructor.
        /// </param>
        /// <returns>The instance of the object.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object could not be created.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        object GetObject(string name, object[] arguments);
        
        /// <summary>
        /// Return an instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method allows an object factory to be used as a replacement for the
        /// Singleton or Prototype design pattern.
        /// </para>
        /// <para>
        /// Note that callers should retain references to returned objects. There is no
        /// guarantee that this method will be implemented to be efficient. For example,
        /// it may be synchronized, or may need to run an RDBMS query.
        /// </para>
        /// <para>
        /// Will ask the parent factory if the object cannot be found in this factory
        /// instance.
        /// </para>
        /// </remarks>
        /// <param name="name">The name of the object to return.</param>
        /// <param name="arguments">
        /// The arguments to use if creating a prototype using explicit arguments to
        /// a static factory method. If there is no factory method and the
        /// arguments are not null, then match the argument values by type and
        /// call the object's constructor.
        /// </param>
        /// <returns>The instance of the object.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object could not be created.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectNotOfRequiredTypeException">
        /// If the object is not of the required type.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        T GetObject<T>(string name, object[] arguments);
	    
        /// <summary>
        /// Return an instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <param name="name">The name of the object to return.</param>
        /// <param name="requiredType">
        /// The <see cref="System.Type"/> the object may match. Can be an interface or
        /// superclass of the actual class. For example, if the value is the
        /// <see cref="System.Object"/> class, this method will succeed whatever the
        /// class of the returned instance.
        /// </param>
        /// <param name="arguments">
        /// The arguments to use if creating a prototype using explicit arguments to
        /// a <see lang="static"/> factory method. If there is no factory method and the
        /// supplied <paramref name="arguments"/> array is not <see lang="null"/>, then
        /// match the argument values by type and call the object's constructor.
        /// </param>
        /// <returns>The instance of the object.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object could not be created.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectNotOfRequiredTypeException">
        /// If the object is not of the required type.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetObject(string, Type)"/>
        object GetObject(string name, Type requiredType, object[] arguments);

		/// <summary>
		/// Return an instance (possibly shared or independent) of the given object name.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Provides a measure of type safety by throwing an exception if the object is
		/// not of the required <see cref="System.Type"/>.
		/// </para>
		/// <para>
		/// This method allows an object factory to be used as a replacement for the
		/// Singleton or Prototype design pattern.
		/// </para>
		/// <para>
		/// Note that callers should retain references to returned objects. There is no
		/// guarantee that this method will be implemented to be efficient. For example,
		/// it may be synchronized, or may need to run an RDBMS query.
		/// </para>
		/// <para>
		/// Will ask the parent factory if the object cannot be found in this factory
		/// instance.
		/// </para>
		/// </remarks>
		/// <param name="name">The name of the object to return.</param>
		/// <param name="requiredType">
		/// <see cref="System.Type"/> the object may match. Can be an interface or
		/// superclass of the actual class. For example, if the value is the
		/// <see cref="System.Object"/> class, this method will succeed whatever the
		/// class of the returned instance.
		/// </param>
		/// <returns>The instance of the object.</returns>
		/// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
		/// If there's no such object definition.
		/// </exception>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the object could not be created.
		/// </exception>
		/// <exception cref="Spring.Objects.Factory.ObjectNotOfRequiredTypeException">
		/// If the object is not of the required type.
		/// </exception>
		object GetObject(string name, Type requiredType);

		/// <summary>
		/// Determine the type of the object with the given name.
		/// </summary>
		/// <remarks>
		/// <para>
		/// More specifically, checks the type of object that
		/// <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/> would return.
		/// For an <see cref="Spring.Objects.Factory.IFactoryObject"/>, returns the type
		/// of object that the <see cref="Spring.Objects.Factory.IFactoryObject"/> creates.
		/// </para>
		/// </remarks>
		/// <param name="name">The name of the object to query.</param>
		/// <returns>
        /// The type of the object or <cref lang="null"/> if not determinable.
        /// </returns>
		Type GetType(string name);



        /// <summary>
        /// Determines whether the object with the given name matches the specified type.
        /// </summary>
        /// <remarks>More specifically, check whether a GetObject call for the given name
        /// would return an object that is assignable to the specified target type.
        /// Translates aliases back to the corresponding canonical bean name.
        /// Will ask the parent factory if the bean cannot be found in this factory instance.
        /// </remarks>
        /// <param name="name">The name of the object to query.</param>
        /// <param name="targetType">Type of the target to match against.</param>
        /// <returns>
        /// 	<c>true</c> if the object type matches; otherwise, <c>false</c>
        /// if it doesn't match or cannot be determined yet.
        /// </returns>
        /// <exception cref="NoSuchObjectDefinitionException">Ff there is no object with the given name
        /// </exception>
	    bool IsTypeMatch(string name, Type targetType);

        /// <summary>
        /// Determines whether the object with the given name matches the specified type.
        /// </summary>
        /// <remarks>More specifically, check whether a GetObject call for the given name
        /// would return an object that is assignable to the specified target type.
        /// Translates aliases back to the corresponding canonical bean name.
        /// Will ask the parent factory if the bean cannot be found in this factory instance.
        /// </remarks>
        /// <param name="name">The name of the object to query.</param>
        /// <typeparam name="T">Type of the target to match against.</typeparam>
        /// <returns>
        /// 	<c>true</c> if the object type matches; otherwise, <c>false</c>
        /// if it doesn't match or cannot be determined yet.
        /// </returns>
        /// <exception cref="NoSuchObjectDefinitionException">Ff there is no object with the given name
        /// </exception>
	    bool IsTypeMatch<T>(string name);

        /// <summary>
        /// Return an unconfigured(!) instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <param name="name">The name of the object to return.</param>
        /// <param name="requiredType">
        /// The <see cref="System.Type"/> the object may match. Can be an interface or
        /// superclass of the actual class. For example, if the value is the
        /// <see cref="System.Object"/> class, this method will succeed whatever the
        /// class of the returned instance.
        /// </param>
        /// <param name="arguments">
        /// The arguments to use if creating a prototype using explicit arguments to
        /// a <see lang="static"/> factory method. If there is no factory method and the
        /// supplied <paramref name="arguments"/> array is not <see lang="null"/>, then
        /// match the argument values by type and call the object's constructor.
        /// </param>
        /// <returns>The unconfigured(!) instance of the object.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object could not be created.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectNotOfRequiredTypeException">
        /// If the object is not of the required type.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetObject(string, Type, object[])"/>
        /// <remarks>
        ///  This method will only <b>instantiate</b> the requested object. It does <b>NOT</b> inject any dependencies!
        /// </remarks>
        object CreateObject(string name, Type requiredType, object[] arguments);

        /// <summary>
        /// Return an unconfigured(!) instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <param name="name">The name of the object to return.</param>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> the object may match. Can be an interface or
        /// superclass of the actual class. For example, if the value is the
        /// <see cref="System.Object"/> class, this method will succeed whatever the
        /// class of the returned instance.
        /// </typeparam>
        /// <param name="arguments">
        /// The arguments to use if creating a prototype using explicit arguments to
        /// a <see lang="static"/> factory method. If there is no factory method and the
        /// supplied <paramref name="arguments"/> array is not <see lang="null"/>, then
        /// match the argument values by type and call the object's constructor.
        /// </param>
        /// <returns>The unconfigured(!) instance of the object.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object could not be created.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectNotOfRequiredTypeException">
        /// If the object is not of the required type.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetObject(string, Type, object[])"/>
        /// <remarks>
        ///  This method will only <b>instantiate</b> the requested object. It does <b>NOT</b> inject any dependencies!
        /// </remarks>
        T CreateObject<T>(string name, object[] arguments);

		/// <summary>
		/// Injects dependencies into the supplied <paramref name="target"/> instance
		/// using the named object definition.
		/// </summary>
		/// <remarks>
		/// <para>
		/// In addition to being generally useful, typically this method is used to provide
		/// dependency injection functionality for objects that are instantiated outwith the
		/// control of a developer. A case in point is the way that the current (1.1)
		/// ASP.NET classes instantiate web controls... the instantiation takes place within
		/// a private method of a compiled page, and thus cannot be hooked into the
		/// typical Spring.NET IOC container lifecycle for dependency injection.
		/// </para>
		/// </remarks>
		/// <example>
		/// The following code snippet assumes that the instantiated factory instance
		/// has been configured with an object definition named
		/// '<i>ExampleNamespace.BusinessObject</i>' that has been configured to set the
		/// <c>Dao</c> property of any <c>ExampleNamespace.BusinessObject</c> instance
		/// to an instance of an appropriate implementation...
		/// <code language="C#">
		/// namespace ExampleNamespace
		/// {
		///     public class BusinessObject
		///     {
		///         private IDao _dao;
		/// 		
		///         public BusinessObject() {}
		/// 
		///         public IDao Dao
		///         {
		///			    get { return _dao;	}
		///             set { _dao = value; }
		///         }
		///     }
		/// }
		/// </code>
		/// with the corresponding driver code looking like so...
		/// <code language="C#">
		/// IObjectFactory factory = GetAnIObjectFactoryImplementation();
		/// BusinessObject instance = new BusinessObject();
		/// factory.ConfigureObject(instance, "object_definition_name");
		/// // at this point the dependencies for the 'instance' object will have been resolved...
		/// </code>
		/// </example>
		/// <param name="target">
		/// The object instance that is to be so configured.
		/// </param>
		/// <param name="name">
		/// The name of the object definition expressing the dependencies that are to
		/// be injected into the supplied <parameref name="target"/> instance.
		/// </param>
		/// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
		/// If there is no object definition for the supplied <paramref name="name"/>.
		/// </exception>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If any of the target object's dependencies could not be created.
		/// </exception>
		object ConfigureObject(object target, string name);
	}
}