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

using System.Collections;
using Spring.Objects.Factory.Config;
using Spring.Util;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Static factory that permits the registration of existing singleton instances.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Does not have support for prototype objects, aliases, and post startup object
    /// configuration.
    /// </p>
    /// <p>
    /// Serves as a simple example implementation of the <see cref="IListableObjectFactory"/>
    /// interface, that manages existing object instances as opposed to creating new ones
    /// based on object definitions.
    /// </p>
    /// <p>
    /// The <see cref="Spring.Objects.Factory.Support.StaticListableObjectFactory.ConfigureObject(object, string)"/>
    /// method is not supported by this class; this class deals exclusively with
    /// existing singleton instances, thus the methods mentioned previously make little sense in this context.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Simon White (.NET)</author>
    [Serializable]
    public class StaticListableObjectFactory : IListableObjectFactory
    {
        /// <summary>
        /// Map from object name to object instance.
        /// </summary>
        private Dictionary<string, object> objects = new Dictionary<string, object>();

        /// <summary>
        /// Determine whether this object factory treats object names case-sensitive or not.
        /// </summary>
        public bool IsCaseSensitive => true;

        /// <summary>
        /// Return the number of objects defined in the factory.
        /// </summary>
        /// <value>
        /// The number of objects defined in the factory.
        /// </value>
        public int ObjectDefinitionCount => objects.Count;

        /// <summary>
        /// Return an instance of the given object name.
        /// </summary>
        /// <param name="name">The name of the object to return.</param>
        /// <returns>The instance of the object.</returns>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>
        public object this[string name] => GetObject(name);

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
        public bool IsTypeMatch<T>(string name)
        {
            return IsTypeMatch(name, typeof(T));
        }

        /// <summary>
        /// This method is not supported by <see cref="StaticListableObjectFactory"/>.
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public object CreateObject(string name, Type requiredType, object[] arguments)
        {
            throw new NotSupportedException("StaticListableObjectFactory does not support this method.");
        }

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
        public T CreateObject<T>(string name, object[] arguments)
        {
            return (T)CreateObject(name, typeof(T), arguments);
        }

        /// <summary>
        /// Return an instance of the given object name.
        /// </summary>
        /// <param name="name">The name of the object to return.</param>
        /// <returns>The instance of the object.</returns>
        /// <exception cref="System.NotSupportedException">
        /// <see cref="Spring.Objects.Factory.Config.IConfigurableFactoryObject"/> is not currently supported.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>
        public object GetObject(string name)
        {
            object instance = objects[name];
            if (instance is IFactoryObject)
            {
                if (instance is IConfigurableFactoryObject)
                {
                    throw new NotSupportedException();
                }
                try
                {
                    return ((IFactoryObject)instance).GetObject();
                }
                catch (Exception ex)
                {
                    throw new ObjectCreationException(name,
                        "IFactoryObject threw an exception on object creation", ex);
                }
            }
            if (instance == null)
            {
                throw new NoSuchObjectDefinitionException(name, GrabDefinedObjectsString());
            }
            return instance;
        }

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
        public T GetObject<T>(string name)
        {
            return (T)GetObject(name, typeof(T));
        }

        /// <summary>
        /// Return an instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method allows an object factory to be used as a replacement for the
        /// Singleton or Prototype design pattern.
        /// </p>
        /// <p>
        /// Note that callers should retain references to returned objects. There is no
        /// guarantee that this method will be implemented to be efficient. For example,
        /// it may be synchronized, or may need to run an RDBMS query.
        /// </p>
        /// <p>
        /// Will ask the parent factory if the object cannot be found in this factory
        /// instance.
        /// </p>
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
        public object GetObject(string name, object[] arguments)
        {
            throw new NotSupportedException("StaticListableObjectFactory does not support this method.");
        }

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
        public T GetObject<T>(string name, object[] arguments)
        {
            return (T)GetObject(name, typeof(T), arguments);
        }

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
        public object GetObject(string name, Type requiredType, object[] arguments)
        {
            throw new NotSupportedException("StaticListableObjectFactory does not support this method.");
        }

        /// <summary>
        /// Return an instance of the given object name.
        /// </summary>
        /// <param name="name">The name of the object to return.</param>
        /// <param name="requiredType">
        /// <see cref="System.Type"/> the object may match. Can be an interface or
        /// superclass of the actual class. For example, if the value is the
        /// <see cref="System.Object"/> class, this method will succeed whatever the
        /// class of the returned instance.
        /// </param>
        /// <returns>The instance of the object.</returns>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetObject(string, System.Type)"/>
        public object GetObject(string name, Type requiredType)
        {
            object instance = GetObject(name);
            if (!requiredType.IsAssignableFrom(instance.GetType()))
            {
                throw new ObjectNotOfRequiredTypeException(name, requiredType, instance);
            }
            return instance;
        }

        /// <summary>
        /// Does this object factory contain an object with the given name?
        /// </summary>
        /// <param name="name">The name of the object to query.</param>
        /// <returns>True if an object with the given name is defined.</returns>
        public bool ContainsObject(string name)
        {
            return objects.ContainsKey(name);
        }

        /// <summary>
        /// Is this object a singleton?
        /// </summary>
        /// <remarks>
        /// <p>
        /// That is, will <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>
        /// or <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string, Type)"/>
        /// always return the same object?
        /// </p>
        /// </remarks>
        /// <param name="name">The name of the object to query.</param>
        /// <returns>True if the named object is a singleton.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        public bool IsSingleton(string name)
        {
            bool isSingleton = true;
            object instance = GetObject(name);
            // in case of IFactoryObject, return singleton status of created object
            if (instance is IFactoryObject)
            {
                isSingleton = ((IFactoryObject)instance).IsSingleton;
            }
            return isSingleton;
        }


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
        public bool IsPrototype(string name)
        {
            bool isPrototype = true;
            object instance = GetObject(name);
            if (instance is IFactoryObject)
            {
                isPrototype = !((IFactoryObject)instance).IsSingleton;
            }
            return isPrototype;

        }

        /// <summary>
        /// Determine the type of the object with the given name.
        /// </summary>
        /// <remarks>
        /// <p>
        /// More specifically, checks the type of object that
        /// <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/> would return.
        /// For an <see cref="Spring.Objects.Factory.IFactoryObject"/>, returns the type
        /// of object that the <see cref="Spring.Objects.Factory.IFactoryObject"/> creates.
        /// </p>
        /// </remarks>
        /// <param name="name">The name of the object to query.</param>
        /// <returns>
        /// The <see cref="System.Type"/> of the object or <see langword="null"/> if
        /// not determinable.
        /// </returns>
        public Type GetType(string name)
        {
            string objectName = ObjectFactoryUtils.TransformedObjectName(name);
            object instance = objects[objectName];
            if (instance == null)
            {
                throw new NoSuchObjectDefinitionException(name, GrabDefinedObjectsString());
            }
            if (instance is IFactoryObject && !ObjectFactoryUtils.IsFactoryDereference(name))
            {
                return ((IFactoryObject)instance).ObjectType;
            }
            return instance.GetType();
        }


        /// <summary>
        /// Determines whether the object with the given name matches the specified type.
        /// </summary>
        /// <param name="name">The name of the object to query.</param>
        /// <param name="targetType">Type of the target to match against.</param>
        /// <returns>
        /// 	<c>true</c> if the object type matches; otherwise, <c>false</c>
        /// if it doesn't match or cannot be determined yet.
        /// </returns>
        /// <exception cref="NoSuchObjectDefinitionException">Ff there is no object with the given name
        /// </exception>
        public bool IsTypeMatch(string name, Type targetType)
        {
            Type type = GetType(name);
            return (targetType == null || (type != null && targetType.IsAssignableFrom(type)));
        }

        private string GrabDefinedObjectsString()
        {
            return "Defined objects are [" +
                   StringUtils.CollectionToDelimitedString(objects.Keys, ",") + "]";
        }

        /// <summary>
        /// Return the aliases for the given object name, if defined.
        /// </summary>
        /// <param name="name">The object name to check for aliases.</param>
        /// <returns>The aliases, or an empty array if none.</returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no such object definition.
        /// </exception>
        public IReadOnlyList<string> GetAliases(string name)
        {
            return StringUtils.EmptyStrings;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <returns>
        /// The registered
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// Always, as object definitions are not supported by this <see cref="IObjectFactory"/>
        /// implementation.
        /// </exception>
        public IObjectDefinition GetObjectDefinition(string name)
        {
            throw new NotSupportedException("StaticListableObjectFactory does not contain object definitions.");
        }

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
        public IObjectDefinition GetObjectDefinition(string name, bool includeAncestors)
        {
            throw new NotSupportedException("StaticListableObjectFactory does not contain object definitions.");
        }


        /// <summary>
        /// Return the names of all objects defined in this factory.
        /// </summary>
        /// <returns>
        /// The names of all objects defined in this factory, or an empty array if none
        /// are defined.
        /// </returns>
        public IReadOnlyList<string> GetObjectDefinitionNames()
        {
            List<string> names = new List<string>(objects.Keys);
            return names;
        }

        /// <summary>
        /// Return the names of all objects defined in this factory, if <code>includeAncestors</code> is <code>true</code>
        /// includes all parent factories.
        /// </summary>
        /// <param name="includeAncestors">to include parent factories in result</param>
        /// <returns>
        /// The names of all objects defined in this factory, if <code>includeAncestors</code> is <code>true</code> includes all
        /// objects defined in parent factories, or an empty array if none are defined.
        /// </returns>
        public IReadOnlyList<string> GetObjectDefinitionNames(bool includeAncestors)
        {
            throw new NotSupportedException("StaticListableObjectFactory does not contain object definitions.");
        }

        /// <summary>
        /// Return the names of objects matching the given <see cref="System.Type"/>
        /// (including subclasses), judging from the object definitions.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> (class or interface) to match, or
        /// <see langword="null"/> for all object names.
        /// </param>
        /// <remarks>
        /// <p>
        /// Will not consider <see cref="Spring.Objects.Factory.IFactoryObject"/>s,
        /// as the type of their created objects is not known before instantiation.
        /// </p>
        /// </remarks>
        /// <returns>
        /// The names of all objects defined in this factory, or an empty array if none
        /// are defined.
        /// </returns>
        public IList<string> GetObjectDefinitionNames(Type type)
        {
            List<string> matches = new List<string>();
            foreach (string name in objects.Keys)
            {
                Type t = objects[name].GetType();
                if (type.IsAssignableFrom(t))
                {
                    matches.Add(name);
                }
            }
            return matches;
        }

        /// <summary>
        /// Return the names of objects matching the given <see cref="System.Type"/>
        /// (including subclasses), judging from the object definitions.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> (class or interface) to match, or
        /// <see langword="null"/> for all object names.
        /// </param>
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
        /// <returns>
        /// The names of all objects defined in this factory, or an empty array if none
        /// are defined.
        /// </returns>
        public IReadOnlyList<string> GetObjectNamesForType(Type type)
        {
            return GetObjectNamesForType(type, true, true);
        }

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
        public IReadOnlyList<string> GetObjectNames<T>()
        {
            return GetObjectNamesForType(typeof(T));
        }

        /// <summary>
        /// Return the names of objects matching the given <see cref="System.Type"/>
        /// (including subclasses), judging from the object definitions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Since this implementation of the
        /// <see cref="Spring.Objects.Factory.IListableObjectFactory"/>
        /// interface does not support the notion of ptototype objects, the
        /// <paramref name="includePrototypes"/> parameter is ignored.
        /// </p>
        /// </remarks>
        /// <param name="type">
        /// The <see cref="System.Type"/> (class or interface) to match, or <see langword="null"/>
        /// for all object names.
        /// </param>
        /// <param name="includePrototypes">
        /// Whether to include prototype objects too or just singletons (also applies to
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/>s). Ignored.
        /// </param>
        /// <param name="includeFactoryObjects">
        /// Whether to include <see cref="Spring.Objects.Factory.IFactoryObject"/>s too
        /// or just normal objects.
        /// </param>
        /// <returns>
        /// The names of all objects defined in this factory, or an empty array if none
        /// are defined.
        /// </returns>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectNamesForType(Type, bool, bool)"/>
        public IReadOnlyList<string> GetObjectNamesForType(Type type, bool includePrototypes, bool includeFactoryObjects)
        {
            bool isFactoryType = (type != null && typeof(IFactoryObject).IsAssignableFrom(type));
            List<string> matches = new List<string>();
            foreach (string name in objects.Keys)
            {
                object instance = objects[name];
                if (instance is IFactoryObject && !isFactoryType)
                {
                    if (includeFactoryObjects)
                    {
                        Type objectType = ((IFactoryObject)instance).ObjectType;
                        if (objectType != null && type.IsAssignableFrom(objectType))
                        {
                            matches.Add(name);
                        }
                    }
                }
                else
                {
                    if (type.IsInstanceOfType(instance))
                    {
                        matches.Add(name);
                    }
                }
            }
            return matches;
        }

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
        public IReadOnlyList<string> GetObjectNames<T>(bool includePrototypes, bool includeFactoryObjects)
        {
            return GetObjectNamesForType(typeof(T), includePrototypes, includeFactoryObjects);
        }

        /// <summary>
        /// Tests whether this object factory contains an object definition for the
        /// specified object name.
        /// </summary>
        /// <param name="name">The object name to query.</param>
        /// <returns>
        /// <b>True</b> if an object defintion is contained within this object factory.
        /// </returns>
        public bool ContainsObjectDefinition(string name)
        {
            return objects.ContainsKey(name);
        }

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
        public IReadOnlyDictionary<string, object> GetObjectsOfType(Type type)
        {
            return GetObjectsOfType(type, true, true);
        }

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
        public IReadOnlyDictionary<string, T> GetObjects<T>()
        {
            Dictionary<string, T> collector = new Dictionary<string, T>();
            DoGetObjectsOfType(typeof(T), true, true, collector);
            return collector;
        }

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
        public IReadOnlyDictionary<string, object> GetObjectsOfType(Type type, bool includePrototypes, bool includeFactoryObjects)
        {
            Dictionary<string, object> collector = new Dictionary<string, object>();
            DoGetObjectsOfType(type, includeFactoryObjects, includePrototypes, collector);
            return collector;
        }

        private void DoGetObjectsOfType(Type type, bool includeFactoryObjects, bool includePrototypes, IDictionary collector)
        {
            bool isFactoryType = (type != null && typeof(IFactoryObject).IsAssignableFrom(type));
            foreach (string name in objects.Keys)
            {
                object instance = objects[name];
                if (instance is IFactoryObject && includeFactoryObjects)
                {
                    IFactoryObject factory = (IFactoryObject)instance;
                    Type objectType = factory.ObjectType;
                    if ((objectType == null && factory.IsSingleton) ||
                        ((factory.IsSingleton || includePrototypes) &&
                         objectType != null && type.IsAssignableFrom(objectType)))
                    {
                        object createdObject = GetObject(name);
                        if (type.IsInstanceOfType(createdObject))
                        {
                            collector[name] = createdObject;
                        }
                    }
                }
                else if (type.IsAssignableFrom(instance.GetType()))
                {
                    if (isFactoryType)
                    {
                        collector[ObjectFactoryUtils.BuildFactoryObjectName(name)] = instance;
                    }
                    else
                    {
                        collector[name] = instance;
                    }
                }
            }
        }

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
        public IReadOnlyDictionary<string, T> GetObjects<T>(bool includePrototypes, bool includeFactoryObjects)
        {
            Dictionary<string, T> collector = new Dictionary<string, T>();
            DoGetObjectsOfType(typeof(T), includeFactoryObjects, includePrototypes, collector);
            return collector;
        }

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
        public T GetObject<T>()
        {
            var objectNamesForType = GetObjectNamesForType(typeof(T));
            if ((objectNamesForType == null) || (objectNamesForType.Count == 0))
            {
                throw new NoSuchObjectDefinitionException(typeof(T).FullName, "Requested Type not Defined in the Context.");
            }

            if (objectNamesForType.Count > 1)
            {
                throw new ObjectDefinitionStoreException(string.Format("More than one definition for {0} found in the Context.", typeof(T).FullName));
            }

            return (T)GetObject(objectNamesForType[0]);
        }

        /// <summary>
        /// Add a new singleton object.
        /// </summary>
        /// <param name="name">
        /// The name to be associated with the object name.
        /// </param>
        /// <param name="instance">The singleton object.</param>
        public void AddObject(string name, object instance)
        {
            objects[name] = instance;
        }

        /// <summary>
        /// Injects dependencies into the supplied <paramref name="target"/> instance
        /// using the named object definition.
        /// </summary>
        /// <param name="target">
        /// The object instance that is to be so configured.
        /// </param>
        /// <param name="name">
        /// The name of the object definition expressing the dependencies that are to
        /// be injected into the supplied <parameref name="target"/> instance.
        /// </param>
        /// <exception cref="System.NotSupportedException">
        /// This feature is not currently supported.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.ConfigureObject(object, string)"/>
        public object ConfigureObject(object target, string name)
        {
            throw new NotSupportedException();
        }

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
        public object ConfigureObject(object target, string name, IObjectDefinition definition)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Defines a method to release allocated unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}
