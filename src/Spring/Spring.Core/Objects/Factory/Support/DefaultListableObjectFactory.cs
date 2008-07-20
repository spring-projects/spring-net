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
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using Common.Logging;
using Spring.Collections;
using Spring.Core;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Concrete implementation of the
    /// <see cref="Spring.Objects.Factory.IListableObjectFactory"/> and
    /// <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry"/>
    /// interfaces.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This class is a full-fledged object factory based on object definitions
    /// that is usable straight out of the box.
    /// </p>
    /// <p>
    /// Can be used as an object factory in and of itself, or as a superclass
    /// for custom object factory implementations. Note that readers for
    /// specific object definition formats are typically implemented separately
    /// rather than as object factory subclasses.
    /// </p>
    /// <p>
    /// For an alternative implementation of the
    /// <see cref="Spring.Objects.Factory.IListableObjectFactory"/> interface,
    /// have a look at the
    /// <see cref="Spring.Objects.Factory.Support.StaticListableObjectFactory"/>
    /// class, which manages existing object instances rather than creating new
    /// ones based on object definitions.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    /// <seealso cref="Spring.Objects.Factory.Xml.XmlObjectDefinitionReader"/>
    [Serializable]
    public class DefaultListableObjectFactory :
        AbstractAutowireCapableObjectFactory,
        IConfigurableListableObjectFactory,
        IObjectDefinitionRegistry
    {
        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.DefaultListableObjectFactory"/> class.
        /// </summary>
        public DefaultListableObjectFactory() : this(true, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.DefaultListableObjectFactory"/> class.
        /// </summary>
        /// <param name="caseSensitive">Flag specifying whether to make this object factory case sensitive or not.</param>
        public DefaultListableObjectFactory(bool caseSensitive) : this(caseSensitive, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.DefaultListableObjectFactory"/> class.
        /// </summary>
        /// <param name="parentFactory">The parent object factory.</param>
        public DefaultListableObjectFactory(IObjectFactory parentFactory)
            : this(true, parentFactory)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.DefaultListableObjectFactory"/> class.
        /// </summary>
        /// <param name="caseSensitive">Flag specifying whether to make this object factory case sensitive or not.</param>
        /// <param name="parentFactory">The parent object factory.</param>
        public DefaultListableObjectFactory(bool caseSensitive, IObjectFactory parentFactory)
            : base(caseSensitive, parentFactory)
        {
            if (caseSensitive)
            {
                objectDefinitionMap = new Hashtable();
            }
            else
            {
                objectDefinitionMap = new CaseInsensitiveHashtable(); //CollectionsUtil.CreateCaseInsensitiveHashtable();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Should object definitions registered under the same name as an
        /// existing object definition be allowed?
        /// </summary>
        /// <remarks>
        /// <p>
        /// If <see langword="true"/>, then the new object definition will
        /// replace (override) the existing object definition. If
        /// <see langword="false"/>, an exception will be thrown when
        /// an attempt is made to register an object definition under the same
        /// name as an already existing object definition.
        /// </p>
        /// <p>
        /// The default is <see langword="true"/>.
        /// </p>
        /// </remarks>
        /// <value>
        /// <see langword="true"/> is the registration of an object definition
        /// under the same name as an existing object definition is allowed.
        /// </value>
        public bool AllowObjectDefinitionOverriding
        {
            get { return allowObjectDefinitionOverriding; }
            set { allowObjectDefinitionOverriding = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Find object instances that match the <paramref name="requiredType"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Called by autowiring. If a subclass cannot obtain information about object
        /// names by <see cref="System.Type"/>, a corresponding exception should be thrown.
        /// </p>
        /// </remarks>
        /// <param name="requiredType">
        /// The type of the objects to look up.
        /// </param>
        /// <returns>
        /// An <see cref="System.Collections.IDictionary"/> of object names and object
        /// instances that match the <paramref name="requiredType"/>, or
        /// <see langword="null"/> if none is found.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        protected override IDictionary FindMatchingObjects(Type requiredType)
        {
            return ObjectFactoryUtils.ObjectsOfTypeIncludingAncestors(
                this, requiredType, true, true);
        }

        /// <summary>
        /// Return the names of the objects that depend on the given object.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Called by the
        /// <see cref="Spring.Objects.Factory.Support.AbstractObjectFactory.DestroyObject"/>
        /// so that dependant objects are able to be disposed of first.
        /// </p>
        /// </remarks>
        /// <param name="objectName">
        /// The name of the object to find depending objects for.
        /// </param>
        /// <returns>
        /// The array of names of depending objects, or the empty string array if none.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        protected override string[] GetDependingObjectNames(string objectName)
        {
            ArrayList dependingObjectNames = new ArrayList();
            string[] allObjectDefinitionNames = GetObjectDefinitionNames();
            foreach (string name in allObjectDefinitionNames)
            {
                if (ContainsObjectDefinition(name))
                {
                    RootObjectDefinition rod
                        = GetMergedObjectDefinition(name, false);
                    if (rod.DependsOn != null)
                    {
                        IList dependsOn = new ArrayList(rod.DependsOn);
                        if (dependsOn.Contains(objectName))
                        {
                            #region Instrumentation

                            if (log.IsDebugEnabled)
                            {
                                log.Debug(string.Format(
                                              CultureInfo.InvariantCulture,
                                              "Found depending object '{0}' for object '{1}'.",
                                              name, objectName));
                            }

                            #endregion

                            dependingObjectNames.Add(name);
                        }
                    }
                }
            }
            return (string[]) dependingObjectNames.ToArray(typeof(string));
        }

        /// <summary>
        /// Check whether the specified object matches the supplied <paramref name="type"/>.
        /// </summary>
        /// <param name="objectName">The name of the object to check.</param>
        /// <param name="type">
        /// The <see cref="System.Type"/> to check for.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the object matches the supplied <paramref name="type"/>,
        /// or if the supplied <paramref name="type"/> is <see langword="null"/>.
        /// </returns>
        private bool IsObjectTypeMatch(string objectName, Type type)
        {
            if (type == null)
            {
                return true;
            }
            Type objectType = GetType(objectName);
            return (objectType != null && type.IsAssignableFrom(objectType));
        }

        private bool IsObjectDefinitionTypeMatch(string name, Type checkedType)
        {
            if (checkedType == null)
            {
                return true;
            }
            RootObjectDefinition rod = GetMergedObjectDefinition(name, false);
            return (rod.HasObjectType && checkedType.IsAssignableFrom(rod.ObjectType));
        }
/*
        /// <summary>
        /// Merges the object definitions.
        /// </summary>
        /// <param name="name">Object definition name.</param>
        /// <param name="parentDefinition">The parent definition.</param>
        /// <param name="childDefinition">The child definition.</param>
        /// <returns>Merged object definition.</returns>
        protected override RootObjectDefinition MergeObjectDefinitions(string name, IObjectDefinition parentDefinition,
                                                                       IObjectDefinition childDefinition)
        {
            RootObjectDefinition rootDefinition = base.MergeObjectDefinitions(name, parentDefinition, childDefinition);
            RegisterObjectDefinition(name, rootDefinition);
            return rootDefinition;
        }
*/
        #endregion

        #region Fields

        /// <summary>
        /// The <see cref="Common.Logging.ILog"/> instance for this class.
        /// </summary>
        private readonly ILog log = LogManager.GetLogger(typeof(DefaultListableObjectFactory));

        /// <summary>
        /// Whether to allow re-registration of a different definition with the
        /// same name.
        /// </summary>
        private bool allowObjectDefinitionOverriding = true;

        /// <summary>
        /// The mapping of object definition objects, keyed by object name.
        /// </summary>
        private readonly IDictionary objectDefinitionMap;

        /// <summary>
        /// List of object definition names, in registration order.
        /// </summary>
        private readonly IList objectDefinitionNames = new ArrayList();

        #endregion

        #region IObjectDefinitionRegistry Members

        /// <summary>
        /// Return the number of objects defined in this registry.
        /// </summary>
        /// <value>
        /// The number of objects defined in this registry.
        /// </value>
        /// <seealso cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry.ObjectDefinitionCount"/>
        public int ObjectDefinitionCount
        {
            get { return objectDefinitionMap.Count; }
        }

        /// <summary>
        /// Check if this registry contains a object definition with the given
        /// name.
        /// </summary>
        /// <param name="name">
        /// The name of the object to look for.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this object factory contains an object
        /// definition with the given name.
        /// </returns>
        /// <seealso cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry.ContainsObjectDefinition(string)"/>
        public override bool ContainsObjectDefinition(string name)
        {
            return objectDefinitionMap.Contains(name);
        }

        /// <summary>
        /// Register a new object definition with this registry.
        /// </summary>
        /// <param name="name">
        /// The name of the object instance to register.
        /// </param>
        /// <param name="objectDefinition">
        /// The definition of the object instance to register.
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the object definition is invalid.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry.RegisterObjectDefinition(string, IObjectDefinition)"/>
        public void RegisterObjectDefinition(
            string name, IObjectDefinition objectDefinition)
        {
            if (objectDefinition is AbstractObjectDefinition)
            {
                try
                {
                    ((AbstractObjectDefinition) objectDefinition).Validate();
                }
                catch (ObjectDefinitionValidationException ex)
                {
                    throw new ObjectDefinitionStoreException(
                        objectDefinition.ResourceDescription,
                        name,
                        "Validation of object definition failed.",
                        ex);
                }
            }
            object oldObjectDefinition = objectDefinitionMap[name];
            if (oldObjectDefinition != null)
            {
                if (!AllowObjectDefinitionOverriding)
                {
                    throw new ObjectDefinitionStoreException(
                        string.Format(
                            "Cannot register object definition [{0}] for object '{1}': there's already [{2}] bound.",
                            objectDefinition, name, oldObjectDefinition));
                }
                else
                {
                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                            string.Format(
                                "Overriding object definition for object '{0}': replacing [{1}] with [{2}].",
                                name, oldObjectDefinition, objectDefinition));
                    }

                    #endregion
                }
            }
            else
            {
                objectDefinitionNames.Add(name);
            }
            objectDefinitionMap[name] = objectDefinition;
        }

        #endregion

        #region IConfigurableListableObjectFactory Members

        /// <summary>
        /// Ensure that all non-lazy-init singletons are instantiated, also
        /// considering <see cref="Spring.Objects.Factory.IFactoryObject"/>s.
        /// </summary>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If one of the singleton objects could not be created.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.Config.IConfigurableListableObjectFactory.PreInstantiateSingletons"/>
        public void PreInstantiateSingletons()
        {
            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug("Pre-instantiating singletons in factory [" + this + "]");
            }

            #endregion

            try
            {
                int definitionCount = objectDefinitionNames.Count;
                for (int i = 0; i < definitionCount; i++)
                {
                    string name = (string) objectDefinitionNames[i];
                    if (!ContainsSingleton(name) && ContainsObjectDefinition(name))
                    {
                        RootObjectDefinition definition
                            = GetMergedObjectDefinition(name, false);
                        if (!definition.IsAbstract
                            && definition.IsSingleton
                            && !definition.IsLazyInit)
                        {
                            Type objectType = ResolveObjectType(definition, name);
                            if (objectType != null
                                && typeof(IFactoryObject).IsAssignableFrom(definition.ObjectType))
                            {
                                IFactoryObject factoryObject = (IFactoryObject) GetObject(
                                                                                    ObjectFactoryUtils.
                                                                                        BuildFactoryObjectName(name));
                                if (factoryObject.IsSingleton)
                                {
                                    GetObject(name);
                                }
                            }
                            else
                            {
                                GetObject(name);
                            }
                        }
                    }
                }
            }
            catch (ObjectsException)
            {
                // destroy already created singletons to avoid dangling resources...
                try
                {
                    Dispose();
                }
                catch (Exception ex)
                {
                    log.Error(
                        "PreInstantiateSingletons failed but couldn't destroy any already-created singletons.",
                        ex);
                }
                throw;
            }
        }

        /// <summary>
        /// Return the registered
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/> for the
        /// given object, allowing access to its property values and constructor
        /// argument values.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <returns>
        /// The registered <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>, 
        /// or <c>null</c>, if specified object definitions does not exist.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="name"/> is <c>null</c> or empty string.
        /// </exception>
        /// <seealso cref="IConfigurableListableObjectFactory.GetObjectDefinition(string)"/>
        public override IObjectDefinition GetObjectDefinition(string name)
        {
            return GetObjectDefinition(name, false);
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
        /// The registered <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>, 
        /// or <c>null</c>, if specified object definitions does not exist.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="name"/> is <c>null</c> or empty string.
        /// </exception>
        /// <seealso cref="IConfigurableListableObjectFactory.GetObjectDefinition(string)"/>
        public override IObjectDefinition GetObjectDefinition(string name, bool includeAncestors)
        {
            if (StringUtils.IsNullOrEmpty(name))
            {
                throw new ArgumentException(
                    "Cannot get an object definition with a null or zero length object name string.");
            }

            name = TransformedObjectName(name);
            IObjectDefinition definition = (IObjectDefinition) objectDefinitionMap[name];
            if (definition == null)
            {
                if (!includeAncestors || ParentObjectFactory == null)
                {
                    return null;
                }
                else if (ParentObjectFactory is AbstractObjectFactory)
                {
                    definition =
                        ((AbstractObjectFactory) ParentObjectFactory).GetObjectDefinition(name, includeAncestors);
                }
            }
            return definition;
        }

        #endregion

        #region IListableObjectFactory Members

        /// <summary>
        /// Return the names of all objects defined in this factory.
        /// </summary>
        /// <returns>
        /// The names of all objects defined in this factory, or an empty array if none
        /// are defined.
        /// </returns>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectDefinitionNames()"/>
        public string[] GetObjectDefinitionNames()
        {
            return (string[]) ((ArrayList) objectDefinitionNames).ToArray(typeof(string));
        }

        /// <summary>
        /// Return the names of objects matching the given <see cref="System.Type"/>
        /// (including subclasses), judging from the object definitions.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> (class or interface) to match, or <see langword="null"/>
        /// for all object names.
        /// </param>
        /// <returns>
        /// The names of all objects defined in this factory, or an empty array if none
        /// are defined.
        /// </returns>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectDefinitionNames()"/>
        public string[] GetObjectDefinitionNames(Type type)
        {
            ArrayList matches = new ArrayList();
            foreach (string name in objectDefinitionNames)
            {
                if (IsObjectDefinitionTypeMatch(name, type))
                {
                    matches.Add(name);
                }
            }
            return (string[]) matches.ToArray(typeof(string));
        }

        /// <summary>
        /// Return the names of objects matching the given <see cref="System.Type"/>
        /// (including subclasses), judging from the object definitions.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> (class or interface) to match, or <see langword="null"/>
        /// for all object names.
        /// </param>
        /// <returns>
        /// The names of all objects defined in this factory, or an empty array if none
        /// are defined.
        /// </returns>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectNamesForType(Type)"/>
        public string[] GetObjectNamesForType(Type type)
        {
            return GetObjectNamesForType(type, true, true);
        }

        /// <summary>
        /// Return the names of objects matching the given <see cref="System.Type"/>
        /// (including subclasses), judging from the object definitions.
        /// </summary>
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
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectNamesForType(Type, bool, bool)"/>
        public string[] GetObjectNamesForType(
            Type type, bool includePrototypes, bool includeFactoryObjects)
        {
            IList objectNames = DoGetObjectNamesForType(type, includePrototypes, includeFactoryObjects);
            return (string[]) ArrayList.Adapter(objectNames).ToArray(typeof(string));
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
        /// <returns>
        /// A <see cref="System.Collections.IDictionary"/> of the matching objects,
        /// containing the object names as keys and the corresponding object instances
        /// as values.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the objects could not be created.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectsOfType(Type)"/>
        public IDictionary GetObjectsOfType(Type type)
        {
            return GetObjectsOfType(type, true, true);
        }

        /// <summary>
        /// Return the object instances that match the given object
        /// <see cref="System.Type"/> (including subclasses).
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
        /// An <see cref="System.Collections.IDictionary"/> of the matching objects,
        /// containing the object names as keys and the corresponding object instances
        /// as values.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If any of the objects could not be created.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectsOfType(Type, bool, bool)"/>
        public IDictionary GetObjectsOfType(
            Type type, bool includePrototypes, bool includeFactoryObjects)
        {
            IDictionary result = new Hashtable();
            IList objectNames = DoGetObjectNamesForType(type, includePrototypes, includeFactoryObjects);
            foreach (string objectName in objectNames)
            {
                try
                {
                    result.Add(objectName, GetObject(objectName));
                }
                catch (ObjectCreationException ex)
                {
                    if (ex.InnerException != null
                        && ex.GetBaseException().GetType().Equals(typeof(ObjectCurrentlyInCreationException)))
                    {
                        // ignoring this is ok... it indicates a circular reference when autowiring
                        // constructors; we want to find matches other than the currently
                        // created object itself...
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(string.Format(
                                          CultureInfo.InvariantCulture,
                                          "Ignoring match to currently created object '{0}'.",
                                          objectName), ex);
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return result;
        }

        // TODO: (ee) What's this method for? It's never used.
/*
        protected IList DoGetObjectNamesForTypeNew(Type type, bool includePrototypes, bool includeFactoryObjects)
        {
            IList result = new ArrayList();
            string[] objectNames = GetObjectDefinitionNames();
            foreach (string objectNam in objectNames)
            {
                string objectName = objectNam;
                if (!IsAlias(objectName))
                {
                    RootObjectDefinition mod = GetMergedObjectDefinition(objectName, false);
                    // Only check object definition if it is complete
                    if (!mod.IsAbstract &&
                        (mod.HasObjectType || !mod.IsLazyInit))
                    {
                        // In case of FactoryObject, match object created by FactoryObject
                    }
                    try
                    {
                        bool isFactoryObject = IsObjectTypeMatch(objectName, mod, typeof(IFactoryObject));
                        bool matchFound = (includePrototypes || mod.IsSingleton) && IsTypeMatch(objectName, type);
                        if (!matchFound && isFactoryObject)
                        {
                            objectName = ObjectFactoryUtils.BuildFactoryObjectName(objectName);
                            matchFound = (includePrototypes || mod.IsSingleton) && IsTypeMatch(objectName, type);
                        }
                        if (matchFound)
                        {
                            result.Add(objectName);
                        }
                    }
                    catch (CannotLoadObjectTypeException ex)
                    {
                        // Probably contains a placeholder; lets ignore it for type matching purposes.
                        if (log.IsDebugEnabled)
                        {
                            log.Debug("Ignoring object class loading failure for object '" + objectName + "'", ex);
                        }
                    }
                }
            }
            // check singletons too, to catch manually registered singletons...
            string[] singletonNames = GetSingletonNames();
            foreach (string objectNam in singletonNames)
            {
                string objectName = objectNam;
                // only check if manually registered...
                if (!ContainsObjectDefinition(objectName))
                {
                    // in the case of an IFactoryObject, match the object created by the IFactoryObject...
                    if (IsFactoryObject(objectName))
                    {
                        if ((includePrototypes || IsSingleton(objectName)) && IsTypeMatch(objectName, type))
                        {
                            result.Add(objectName);
                            continue;
                        }
                        objectName = ObjectFactoryUtils.BuildFactoryObjectName(objectName);
                    }
                    if (IsTypeMatch(objectName, type))
                    {
                        result.Add(objectName);
                    }
                }
            }
            return result;
        }
*/

        /// <summary>
        /// Return the object instances that match the given object
        /// <see cref="System.Type"/> (including subclasses).
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
        /// An <see cref="System.Collections.IDictionary"/> of the matching objects,
        /// containing the object names as keys and the corresponding object instances
        /// as values.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If any of the objects could not be created.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IListableObjectFactory.GetObjectsOfType(Type, bool, bool)"/>
        protected IList DoGetObjectNamesForType(
            Type type, bool includePrototypes, bool includeFactoryObjects)
        {
            bool isFactoryType = (type != null && typeof(IFactoryObject).IsAssignableFrom(type));
            IList result = new ArrayList();
            if (type != null)
            {
                string[] objectNames = GetObjectDefinitionNames();
                foreach (string objectName in objectNames)
                {
                    RootObjectDefinition rod = GetMergedObjectDefinition(objectName, false);
                    // only check complete object definitions...
                    if (!rod.IsAbstract && rod.HasObjectType)
                    {
                        // return the return type of an object created via a factory method...
                        if (StringUtils.HasText(rod.FactoryMethodName))
                        {
                            Type methodType = GetTypeForFactoryMethod(objectName, rod);
                            if (methodType != null
                                && type.IsAssignableFrom(methodType))
                            {
                                result.Add(objectName);
                            }
                        }
                            // in the case of an IFactoryObject, match the object created by the IFactoryObject...
                        else if (typeof(IFactoryObject).IsAssignableFrom(rod.ObjectType) && !isFactoryType)
                        {
                            if (includeFactoryObjects && (includePrototypes || IsSingleton(objectName)) &&
                                IsObjectTypeMatch(objectName, type))
                            {
                                result.Add(objectName);
                            }
                        }
                        else
                        {
                            string factoryObjectName = objectName;
                            // if type to match is an IFactoryObject, match the IFactoryObject itself;
                            // else, match the object instance...
                            if (isFactoryType)
                            {
                                factoryObjectName = ObjectFactoryUtils.BuildFactoryObjectName(objectName);
                            }
                            if ((includePrototypes || rod.IsSingleton) && //MLP
                                (type.IsAssignableFrom(rod.ObjectType)))
                            {
                                result.Add(factoryObjectName);
                            }
                        }
                    }
                }

                // check singletons too, to catch manually registered singletons...
                string[] singletonNames = GetSingletonNames();
                foreach (string objectName in singletonNames)
                {
                    // only check if manually registered...
                    if (!ContainsObjectDefinition(objectName))
                    {
                        // in the case of an IFactoryObject, match the object created by the IFactoryObject...
                        if (IsFactoryObject(objectName) && !isFactoryType)
                        {
                            if (includeFactoryObjects && (includePrototypes || IsSingleton(objectName)) &&
                                IsObjectTypeMatch(objectName, type))
                            {
                                result.Add(objectName);
                            }
                        }
                        else
                        {
                            string factoryObjectName = objectName;
                            // if type to match is an IFactoryObject, match the IFactoryObject itself...
                            // else, match the object instance...
                            if (isFactoryType)
                            {
                                factoryObjectName = ObjectFactoryUtils.BuildFactoryObjectName(objectName);
                            }
                            if (IsObjectTypeMatch(factoryObjectName, type))
                            {
                                result.Add(factoryObjectName);
                            }
                        }
                    }
                }
            }
            return result;
        }

        #endregion
    }
}
