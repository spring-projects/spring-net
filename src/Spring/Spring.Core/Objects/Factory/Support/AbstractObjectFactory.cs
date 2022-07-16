/*
 * Copyright � 2002-2011 the original author or authors.
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
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;

using Common.Logging;

using Spring.Collections;
using Spring.Core;
using Spring.Core.TypeConversion;
using Spring.Objects.Factory.Config;
using Spring.Threading;
using Spring.Util;
using System.Threading;
using System.Runtime.Serialization;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Abstract superclass for <see cref="Spring.Objects.Factory.IObjectFactory"/>
    /// implementations.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This class provides singleton / prototype determination, singleton caching,
    /// object definition aliasing, <see cref="Spring.Objects.Factory.IFactoryObject"/>
    /// handling, and object definition merging for child object definitions.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public abstract class AbstractObjectFactory : IConfigurableObjectFactory
    {
        /// <summary>
        /// Marker object to be temporarily registered in the singleton cache,
        /// while instantiating an object (in order to be able to detect circular references).
        /// </summary>
        private static readonly object CurrentlyInCreation = new object();

        /// <summary>
        /// Used as value in hashtable that keeps track of singleton names currently in the
        /// process of being created.  Would not be necessary if we created a case insensitive implementation of
        /// ISet.
        /// </summary>
        private static readonly object EmptyObject = new object();

        /// <summary>
        /// The <see cref="Common.Logging.ILog"/> instance for this class.
        /// </summary>
        [NonSerialized] protected ILog log;

        /// <summary>
        /// Cache of singleton objects created by <see cref="IFactoryObject"/>s: FactoryObject name -> product
        /// </summary>
        private readonly Dictionary<string, object> factoryObjectProductCache = new Dictionary<string, object>();

        /// <summary>
        /// Disposable object instances: object name --> disposable instance
        /// </summary>
        private readonly Dictionary<string, object> disposableObjects = new Dictionary<string, object>();

        /// <summary>
        /// root object definitons: object name --> Root Object Definition
        /// </summary>
        protected ConcurrentDictionary<string, RootObjectDefinition> mergedObjectDefinitions = new ConcurrentDictionary<string, RootObjectDefinition>();

        /// <summary>
        /// Whether to cache object metadata or rather reobtain it for every access
        /// </summary>
        private bool cacheObjectMetadata = true;

        /// <summary>
        /// Names of object that have already been created at least once
        /// </summary>
        private readonly HashSet<string> alreadyCreated = new HashSet<string>();

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.AbstractObjectFactory"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This constructor implicitly creates an
        /// <see cref="Spring.Objects.Factory.Support.AbstractObjectFactory"/>
        /// that treats the names of objects in this factory in a case-sensitive fashion.
        /// </p>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes no public constructors.
        /// </p>
        /// </remarks>
        protected AbstractObjectFactory()
            : this(true)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.AbstractObjectFactory"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes no public constructors.
        /// </p>
        /// </remarks>
        /// <param name="caseSensitive">
        /// <see lang="true"/> if the names of objects in this factory are to be treated in a
        /// case-sensitive fashion.
        /// </param>
        protected AbstractObjectFactory(bool caseSensitive)
        {
            log = LogManager.GetLogger(GetType());
            this.caseSensitive = caseSensitive;

            var  comparer = (caseSensitive) ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            aliasMap = new OrderedDictionary(comparer);
            singletonCache = new OrderedDictionary(comparer);
            singletonLocks = new ConcurrentDictionary<string, Lazy<object>>(comparer);
            singletonsInCreation = new OrderedDictionary(comparer);
            prototypesInCreation = new LogicalThreadContextSetVariable();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            log = LogManager.GetLogger(GetType());
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.AbstractObjectFactory"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes no public constructors.
        /// </p>
        /// </remarks>
        /// <param name="caseSensitive">
        /// <see lang="true"/> if the names of objects in this factory are to be treated in a
        /// case-sensitive fashion.
        /// </param>
        /// <param name="parentFactory">
        /// Any parent object factory; may be <see lang="null"/>.
        /// </param>
        protected AbstractObjectFactory(bool caseSensitive, IObjectFactory parentFactory)
            : this(caseSensitive)
        {
            ParentObjectFactory = parentFactory;
        }

        /// <summary>
        /// Returns, whether this factory treats object names case sensitive or not.
        /// </summary>
        public bool IsCaseSensitive => caseSensitive;

        /// <summary>
        /// Gets the <see cref="ISet"/> of
        /// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>s
        /// that will be applied to objects created by this factory.
        /// </summary>
        public IReadOnlyList<IObjectPostProcessor> ObjectPostProcessors => objectPostProcessors;

        /// <summary>
        /// Gets the set of classes that will be ignored for autowiring.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The elements of this <see cref="Spring.Collections.ISet"/> are
        /// <see cref="System.Type"/>s.
        /// </p>
        /// </remarks>
        public ISet IgnoredDependencyTypes => ignoreDependencyTypes;

        /// <summary>
        /// Returns, whether this object factory instance contains <see cref="IInstantiationAwareObjectPostProcessor"/> objects.
        /// </summary>
        protected bool HasInstantiationAwareObjectPostProcessors => hasInstantiationAwareObjectPostProcessors;

        /// <summary>
        /// Returns, whether this object factory instance contains <see cref="IDestructionAwareObjectPostProcessor"/> objects.
        /// </summary>
        protected bool HasDestructionAwareObjectPostProcessors => hasDestructionAwareObjectPostProcessors;

        /// <summary>
        /// Check whether this factory's bean creation phase already started,
        /// i.e. whether any bean has been marked as created in the meantime.
        /// </summary>
        protected bool HasObjectCreationStarted => alreadyCreated.Count > 0;

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
            return GetObjectInternal(name, requiredType, arguments, false);
        }

        /// <summary>
        /// Apply the property values of the object definition with the supplied
        /// <paramref name="name"/> to the supplied <paramref name="instance"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The object definition can either define a fully self-contained object,
        /// reusing it's property values, or just property values meant to be used
        /// for existing object instances.
        /// </p>
        /// </remarks>
        /// <param name="instance">
        /// The existing object that the property values for the named object will
        /// be applied to.
        /// </param>
        /// <param name="name">
        /// The name of the object definition associated with the property values that are
        /// to be applied.
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        public virtual void ApplyObjectPropertyValues(object instance, string name)
        {
            // explicit no-op...
        }

        /// <summary>
        /// Apply the property values of the object definition with the supplied
        /// <paramref name="name"/> to the supplied <paramref name="instance"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The object definition can either define a fully self-contained object,
        /// reusing it's property values, or just property values meant to be used
        /// for existing object instances.
        /// </p>
        /// </remarks>
        /// <param name="instance">
        /// The existing object that the property values for the named object will
        /// be applied to.
        /// </param>
        /// <param name="name">
        /// The name of the object definition associated with the property values that are
        /// to be applied.
        /// </param>
        /// <param name="definition">
        /// An object definition that should be used to apply property values.
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        public virtual void ApplyObjectPropertyValues(object instance, string name, IObjectDefinition definition)
        {
            // explicit no-op...
        }

        //        /// <summary>
        //        /// Create an object instance for the given object definition.
        //        /// </summary>
        //        /// <remarks>
        //        /// <p>
        //        /// The object definition will already have been merged with the parent
        //        /// definition in case of a child definition.
        //        /// </p>
        //        /// <p>
        //        /// All the other methods in this class invoke this method, although objects
        //        /// may be cached after being instantiated by this method. All object
        //        /// instantiation within this class is performed by this method.
        //        /// </p>
        //        /// </remarks>
        //        /// <param name="name">The name of the object.</param>
        //        /// <param name="definition">
        //        /// The object definition for the object that is to be instantiated.
        //        /// </param>
        //        /// <param name="arguments">
        //        /// The arguments to use if creating a prototype using explicit arguments to
        //        /// a <see lang="static"/>  factory method. If there is no factory method and the
        //        /// supplied <paramref name="arguments"/> array is not <see lang="null"/>,
        //        /// then match the argument values by type and call the object's constructor.
        //        /// </param>
        //        /// <returns>
        //        /// A new instance of the object.
        //        /// </returns>
        //        /// <exception cref="Spring.Objects.ObjectsException">
        //        /// In case of errors.
        //        /// </exception>
        //        protected internal abstract object CreateObject(string name, RootObjectDefinition definition, object[] arguments);


        /// <summary>
        /// Create an object instance for the given object definition.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="definition">
        /// The object definition for the object that is to be instantiated.
        /// </param>
        /// <param name="arguments">
        /// The arguments to use if creating a prototype using explicit arguments to
        /// a static factory method. It is invalid to use a non-<see langword="null"/> arguments value
        /// in any other case.
        /// </param>
        /// <param name="allowEagerCaching">
        /// Whether eager caching of singletons is allowed... typically true for
        /// singlton objects, but never true for inner object definitions.
        /// </param>
        /// <param name="suppressConfigure">
        /// Create instance only - suppress injecting dependencies yet.
        /// </param>
        /// <returns>
        /// A new instance of the object.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        /// <remarks>
        /// <p>
        /// The object definition will already have been merged with the parent
        /// definition in case of a child definition.
        /// </p>
        /// <p>
        /// All the other methods in this class invoke this method, although objects
        /// may be cached after being instantiated by this method. All object
        /// instantiation within this class is performed by this method.
        /// </p>
        /// </remarks>
        protected internal abstract object InstantiateObject(string name, RootObjectDefinition definition, object[] arguments,
                                                        bool allowEagerCaching, bool suppressConfigure);

        /// <summary>
        /// Destroy the target object.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Must destroy objects that depend on the given object before the object itself,
        /// nor throw an exception.
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <param name="target">
        /// The target object instance to destroyed.
        /// </param>
        protected abstract void DestroyObject(string name, object target);

        /// <summary>
        /// Does this object factory contain an object definition with the
        /// supplied <paramref name="name"/>?
        /// </summary>
        /// <remarks>
        /// <p>
        /// Does not consider any hierarchy this factory may participate in.
        /// Invoked by
        /// <see cref="Spring.Objects.Factory.Support.AbstractObjectFactory.ContainsObject"/>
        /// when no cached singleton instance is found.
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The name of the object to look for.
        /// </param>
        /// <returns>
        /// <see lang="true"/> if this object factory contains an object
        /// definition with the supplied <paramref name="name"/>.
        /// </returns>
        public abstract bool ContainsObjectDefinition(string name);

        /// <summary>
        /// Adds the supplied <paramref name="singleton"/> (object) to this factory's
        /// singleton cache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// To be called for eager registration of singletons, e.g. to be able to
        /// resolve circular references.
        /// </p>
        /// <note>
        /// If a singleton has already been registered under the same name as
        /// the supplied <paramref name="name"/>, then the old singleton will
        /// be replaced.
        /// </note>
        /// </remarks>
        /// <param name="name">The name of the object.</param>
        /// <param name="singleton">The singleton object.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="name"/> argument is <see langword="null"/>
        /// or consists wholly of whitespace characters; or if the
        /// <paramref name="singleton"/> is <see langword="null"/>.
        /// </exception>
        protected virtual void AddSingleton(string name, object singleton)
        {
            AssertUtils.ArgumentHasText(name, "The object name must not be empty.");
            AssertUtils.ArgumentNotNull(singleton, "singleton");
            lock (GetSingletonLockFor(name))
            {
                singletonCache[name] = singleton;
                registeredSingletons.Add(name);
            }
        }

        /// <summary>
        /// Return the object name, stripping out the factory dereference prefix if
        /// necessary, and resolving aliases to canonical names.
        /// </summary>
        /// <param name="name">
        /// The transformed name of the object.
        /// </param>
        protected string TransformedObjectName(string name)
        {
            string objectName = ObjectFactoryUtils.TransformedObjectName(name);

            if (aliasMap.Count > 0)
            {
                // handle aliasing...
                string canonicalName = (string) aliasMap[objectName];
                return canonicalName ?? objectName;
            }

            return objectName;
        }

        /// <summary>
        /// Ensures, that the given name is prefixed with <see cref="ObjectFactoryUtils.FactoryObjectPrefix"/>
        /// if it incidentially already starts with this prefix. This avoids troubles when dereferencing
        /// the object name during <see cref="ObjectFactoryUtils.TransformedObjectName"/>
        /// </summary>
        protected string OriginalObjectName(string name)
        {
            string objectName = TransformedObjectName(name);
            if (name.StartsWith(ObjectFactoryUtils.FactoryObjectPrefix))
            {
                objectName = ObjectFactoryUtils.FactoryObjectPrefix + objectName;
            }
            return objectName;
        }

        /// <summary>
        /// Determines whether the specified name is defined as an alias as opposed
        /// to the name of an actual object definition.
        /// </summary>
        /// <param name="name">The object name to check.</param>
        /// <returns>
        /// 	<c>true</c> if the specified name is alias; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsAlias(string name)
        {
            return aliasMap.Contains(name);
        }

        /// <summary>
        /// Return a <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>,
        /// even by traversing parent if the parameter is a child definition.
        /// </summary>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <param name="includingAncestors">
        /// Are ancestors to be included in the merge?
        /// </param>
        /// <remarks>
        /// <p>
        /// Will ask the parent object factory if not found in this instance.
        /// </p>
        /// </remarks>
        /// <returns>
        /// A merged <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
        /// with overridden properties.
        /// </returns>
        public virtual RootObjectDefinition GetMergedObjectDefinition(string name, bool includingAncestors)
        {
            return GetMergedObjectDefinition(name, GetObjectDefinition(name, includingAncestors));
        }

        /// <summary>
        /// Return a <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>,
        /// even by traversing parent if the parameter is a child definition.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="od">The od.</param>
        /// <returns>
        /// A merged <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
        /// with overridden properties.
        /// </returns>
        protected internal virtual RootObjectDefinition GetMergedObjectDefinition(string name, IObjectDefinition od)
        {
            return GetMergedLocalObjectDefinition(name) ?? GetMergedObjectDefinitionInternal(name, od);
        }

        /// <summary>
        /// Return a <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>,
        /// even by traversing parent if the parameter is a child definition.
        /// </summary>
        /// <returns>
        /// A merged <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
        /// with overridden properties.
        /// </returns>
        protected internal virtual RootObjectDefinition GetMergedObjectDefinitionInternal(string name, IObjectDefinition od)
        {
            if (od == null)
            {
                return null;
            }

            // Check with full lock now in order to enforce the same merged instance.
            RootObjectDefinition ValueFactory(string key)
            {
                RootObjectDefinition mod;
                if (od.ParentName == null)
                {
                    mod = CreateRootObjectDefinition(od);
                }
                else
                {
                    IObjectDefinition pod = null;
                    if (!key.Equals(od.ParentName))
                    {
                        pod = GetMergedObjectDefinition(TransformedObjectName(od.ParentName), true);
                    }
                    else
                    {
                        if (ParentObjectFactory is AbstractObjectFactory factory)
                        {
                            pod = factory.GetMergedObjectDefinition(od.ParentName, true);
                        }
                    }

                    if (pod == null)
                    {
                        throw new NoSuchObjectDefinitionException(od.ParentName, $"Parent name '{od.ParentName}' is equal to object name '{key}' - " + "cannot be resolved without an AbstractObjectFactory parent.");
                    }

                    mod = CreateRootObjectDefinition(pod);
                    mod.OverrideFrom(od);
                }

                return mod;
            }

            // Only cache the merged bean definition if we're already about to create an
            // instance of the object, or at least have already created an instance before.
            if (CacheObjectMetadata && IsObjectEligibleForMetadataCaching(name))
            {
                return mergedObjectDefinitions.GetOrAdd(name, ValueFactory);
            }

            return ValueFactory(name);
        }

        /// <summary>
        /// Gets the merged local object definition.
        /// </summary>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>
        /// Merged RootBeanDefinition, traversing the parent bean definition
        /// if the specified bean corresponds to a child bean definition.
        /// </returns>
        protected RootObjectDefinition GetMergedLocalObjectDefinition(string objectName)
        {
            mergedObjectDefinitions.TryGetValue(objectName, out var mbd);
            return mbd; // ?? GetMergedObjectDefinition(objectName, GetObjectDefinition(objectName));
        }

        /// <summary>
        /// Determines whether the metadata for the specified object name is eligible for caching.
        /// </summary>
        /// <param name="beanName">Name of the bean.</param>
        /// <returns>
        /// 	<c>true</c> if [is object eligible for metadata caching] [the specified bean name]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsObjectEligibleForMetadataCaching(string beanName)
        {
            return alreadyCreated.Contains(beanName);
        }

        protected bool CacheObjectMetadata
        {
            get => cacheObjectMetadata;
            set => cacheObjectMetadata = value;
        }

        /// <summary>
        /// Creates the root object definition.
        /// </summary>
        /// <param name="templateDefinition">The template definition to base root definition on.</param>
        /// <returns>Root object definition.</returns>
        protected virtual RootObjectDefinition CreateRootObjectDefinition(IObjectDefinition templateDefinition)
        {
            return new RootObjectDefinition(templateDefinition);
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
        public abstract void RegisterObjectDefinition(string name, IObjectDefinition objectDefinition);

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
        public abstract IObjectDefinition GetObjectDefinition(string name);

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
        public abstract IObjectDefinition GetObjectDefinition(string name, bool includeAncestors);

        /// <summary>
        /// Gets the type for the given FactoryObject.
        /// </summary>
        /// <param name="factoryObject">The factory object instance to check.</param>
        /// <returns>the FactoryObject's object type</returns>
        protected virtual Type GetTypeForFactoryObject(IFactoryObject factoryObject)
        {
            try
            {
                return factoryObject.ObjectType;
            }
            catch (Exception ex)
            {
                log.Warn("FactoryObject threw exception from ObjectType, despite the contract saying " +
                    "that it should return null if the type of its object cannot be determined yet", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets the object type for the given FactoryObject definition, as far as possible.
        /// Only called if there is no singleton instance registered for the target object already.
        /// </summary>
        /// <remarks>
        /// The default implementation creates the FactoryObject via <code>GetObject</code>
        /// to call its <code>ObjectType</code> property. Subclasses are encouraged to optimize
        /// this, typically by just instantiating the FactoryObject but not populating it yet,
        /// trying whether its <code>ObjectType</code> property already returns a type.
        /// If no type found, a full FactoryObject creation as performed by this implementation
        /// should be used as fallback.
        /// </remarks>
        /// <param name="objectName">Name of the object.</param>
        /// <param name="mod">The merged object definition for the object.</param>
        /// <returns>The type for the object if determinable, or <code>null</code> otherwise</returns>
        protected virtual Type GetTypeForFactoryObject(string objectName, RootObjectDefinition mod)
        {
            if (!mod.IsSingleton)
            {
                return null;
            }
            try
            {
                IFactoryObject factoryObject = GetFactoryObject(objectName);
                return GetTypeForFactoryObject(factoryObject);
            }
            catch (ObjectCreationException ex)
            {
                // Can only happen when getting a FactoryObject.
                log.Warn("Ignoring object creation exception on FactoryObject type check", ex);
                return null;
            }
        }

        /// <summary>
        /// Mark the specified bean as already created (or about to be created).
        /// </summary>
        /// <remarks>
        /// This allows the bean factory to optimize its caching for repeated
        /// creation of the specified bean.
        /// </remarks>
        /// <param name="objectName">The name of the object.</param>
        protected void MarkObjectAsCreated(string objectName)
        {
            if (alreadyCreated.Contains(objectName))
            {
                return;
            }

            lock (mergedObjectDefinitions)
            {
                if (!alreadyCreated.Contains(objectName))
                {
                    // Let the bean definition get re-merged now that we're actually creating
                    // the object... just in case some of its metadata changed in the meantime.
                    ClearMergedObjectDefinition(objectName);
                    alreadyCreated.Add(objectName);
                }
            }
        }

        /// <summary>
        /// Perform appropriate cleanup of cached metadata after bean creation failed.
        /// </summary>
        /// <param name="objectName">The name of the object</param>
        protected void CleanupAfterObjectCreationFailure(string objectName)
        {
            lock (mergedObjectDefinitions)
            {
                alreadyCreated.Remove(objectName);
            }
        }

        /// <summary>
        /// Predict the eventual object type (of the processed object instance) for the
        /// specified object.
        /// </summary>
        /// <remarks>
        /// Does not need to handle FactoryObjects specifically, since it is only
        /// supposed to operate on the raw object type.
        /// This implementation is simplistic in that it is not able to
        /// handle factory methods and InstantiationAwareBeanPostProcessors.
        /// It only predicts the object type correctly for a standard object.
        /// To be overridden in subclasses, applying more sophisticated type detection.
        /// </remarks>
        /// <param name="objectName">Name of the object.</param>
        /// <param name="mod">The merged object definition to determine the type for. May be <c>null</c></param>
        /// <returns>The type of the object, or <code>null</code> if not predictable</returns>
        protected virtual Type PredictObjectType(string objectName, RootObjectDefinition mod)
        {
            if (mod == null || StringUtils.HasText(mod.FactoryObjectName))
            {
                return null;
            }
            return ResolveObjectType(mod, objectName);
        }

        /// <summary>
        /// Get the object for the given object instance, either the object
        /// instance itself or its created object in case of an
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/>.
        /// </summary>
        /// <param name="instance">The object instance.</param>
        /// <param name="name">
        /// The name that may include the factory dereference prefix (=the requested name).
        /// </param>
        /// <param name="canonicalName">
        /// The canonical object name
        /// </param>
        /// <param name="rod">the merged object definition</param>
        /// <returns>
        /// The singleton instance of the object.
        /// </returns>
        protected internal virtual object GetObjectForInstance(object instance, string name, string canonicalName, RootObjectDefinition rod)
        {
            // don't let calling code try to dereference the
            // object factory if the object isn't a factory
            if (IsFactoryDereference(name) && !(ObjectUtils.IsAssignable(typeof(IFactoryObject), instance)))
            {
                throw new ObjectIsNotAFactoryException(canonicalName, instance);
            }

            // now we have the object instance, which may be a normal object
            // or an IFactoryObject. If it's an IFactoryObject and the caller wants
            // a reference to the factory there's nothing more to do


            // it's a normal object ?
            if (!ObjectUtils.IsAssignable(typeof(IFactoryObject), instance))
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("Calling code asked for normal instance for name '{0}'.", canonicalName));
                }

                return instance;
            }

            // the user wants the factory itself ?
            if (!ObjectUtils.IsAssignable(typeof(IFactoryObject), instance) || IsFactoryDereference(name))
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(
                        string.Format("Calling code asked for IFactoryObject instance for name '{0}'.",
                                      TransformedObjectName(name)));
                }

                return instance;
            }

            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format("Object with name '{0}' is a factory object.", canonicalName));
            }

            object resultInstance = null;

            if (rod == null)
            {
                factoryObjectProductCache.TryGetValue(canonicalName, out resultInstance);
            }

            if (resultInstance == null)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("Dereferencing Object with name '{0}'", canonicalName));
                }

                // return object instance from factory...
                IFactoryObject factory = (IFactoryObject)instance;

                if (rod == null && ContainsObjectDefinition(canonicalName))
                {
                    rod = GetMergedObjectDefinition(canonicalName, true);
                }

                if (factory.IsSingleton && ContainsSingleton(canonicalName))
                {
                    lock (factoryObjectProductCache)
                    {
                        if (!factoryObjectProductCache.TryGetValue(canonicalName, out resultInstance))
                        {
                            resultInstance = GetObjectFromFactoryObject(factory, canonicalName, rod);
                            if (resultInstance != null)
                            {
                                factoryObjectProductCache[canonicalName] = resultInstance;
                            }
                        }
                    }
                }
                else
                {
                    resultInstance = GetObjectFromFactoryObject(factory, canonicalName, rod);
                }

                if (resultInstance == null)
                {
                    throw new FactoryObjectNotInitializedException(canonicalName,
                                                                   "Factory object returned null object - "
                                                                   + "possible cause: not fully initialized due to "
                                                                   + "circular object reference.");
                }
            }
            else
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("Returning factory product from cache for Object with name '{0}'", canonicalName));
                }
            }
            return resultInstance;
        }

        /// <summary>
        /// Obtain an object to expose from the given IFactoryObject.
        /// </summary>
        /// <param name="factory">The IFactoryObject instance.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <param name="rod">The merged object definition.</param>
        /// <returns>The object obtained from the IFactoryObject</returns>
        /// <exception cref="ObjectCreationException">If IFactoryObject object creation failed.</exception>
        private object GetObjectFromFactoryObject(IFactoryObject factory, string objectName, RootObjectDefinition rod)
        {
            object instance;

            try
            {
                instance = factory.GetObject();
            }
            catch (FactoryObjectNotInitializedException ex)
            {
                throw new ObjectCurrentlyInCreationException(
                    rod.ResourceDescription, objectName, ex);
            }
            catch (Exception ex)
            {
                throw new ObjectCreationException(rod.ResourceDescription, objectName,
                    "FactoryObject threw exception on object creation.", ex);
            }

            // Do not accept a null value for a FactoryBean that's not fully
            // initialized yet: Many FactoryBeans just return null then.
            if (instance == null && IsSingletonCurrentlyInCreation(objectName))
            {
                throw new ObjectCurrentlyInCreationException(rod.ResourceDescription, objectName,
                    "FactoryObject which is currently in creation returned null from GetObject.");
            }

            if (factory is IConfigurableFactoryObject)
            {
                IConfigurableFactoryObject configurableFactory = (IConfigurableFactoryObject)factory;

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("Factory object with name '{0}' is configurable.", TransformedObjectName(objectName)));
                }

                if (configurableFactory.ProductTemplate != null)
                {
                    ApplyObjectPropertyValues(instance, objectName, configurableFactory.ProductTemplate);
                }
            }

            if (instance != null)
            {
                try
                {
                    instance = PostProcessObjectFromFactoryObject(instance, objectName);
                }
                catch (Exception ex)
                {
                    throw new ObjectCreationException(rod.ResourceDescription, objectName,
                                "Post-processing of the FactoryObject's object failed.", ex);
                }
            }

            return instance;
        }

        /// <summary>
        /// Post-process the given object that has been obtained from the FactoryObject.
        /// The resulting object will be exposed for object references.
        /// </summary>
        /// <remarks>The default implementation simply returns the given object
        /// as-is.  Subclasses may override this, for example, to apply
        /// post-processors.</remarks>
        /// <param name="instance">The instance obtained from the IFactoryObject.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>The object instance to expose</returns>
        /// <exception cref="ObjectsException">if any post-processing failed.</exception>
        protected virtual object PostProcessObjectFromFactoryObject(object instance, string objectName)
        {
            return instance;
        }

        /// <summary>
        /// Convenience method to pull an <see cref="Spring.Objects.Factory.IFactoryObject"/>
        /// from this factory.
        /// </summary>
        /// <param name="objectName">
        /// The name of the factory object to be retrieved. If this name is not a valid
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> name, it will be converted
        /// into one.
        /// </param>
        /// <returns>
        /// The <see cref="Spring.Objects.Factory.IFactoryObject"/> associated with the
        /// supplied <paramref name="objectName"/>.
        /// </returns>
        protected IFactoryObject GetFactoryObject(string objectName)
        {
            if (!ObjectFactoryUtils.IsFactoryDereference(objectName))
            {
                objectName = ObjectFactoryUtils.BuildFactoryObjectName(objectName);
            }
            return (IFactoryObject)GetObject(objectName);
        }

        /// <summary>
        /// Is the supplied <paramref name="name"/> a factory object dereference?
        /// </summary>
        protected bool IsFactoryDereference(string name)
        {
            return ObjectFactoryUtils.IsFactoryDereference(name);
        }

        /// <summary>
        /// Determines whether the type of the given object definition matches the
        /// specified target type.
        /// </summary>
        /// <remarks>Allows for lazy load of the actual object type, provided that the
        /// type match can be determined otherwise.
        /// <para>The default implementation simply delegates to the standard
        /// <code>ResolveObjectType</code> method.  Subclasses may override this to use
        /// a differnt strategy.</para>
        /// </remarks>
        /// <param name="objectName">Name of the object (for error handling purposes).</param>
        /// <param name="rod">The merged object definition to determine the type for.</param>
        /// <param name="targetType">Type to match against (never null).</param>
        /// <returns>
        /// 	<c>true</c> if object definition matches tye specified target type; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="CannotLoadObjectTypeException">if we failed to load the type.</exception>"
        protected bool IsObjectTypeMatch(string objectName, RootObjectDefinition rod, Type targetType)
        {
            Type objectType = ResolveObjectType(rod, objectName);
            return (objectType != null && targetType.IsAssignableFrom(objectType));
        }

        /// <summary>
        /// Resolves the type of the object for the specified object definition resolving
        /// an object type name to a Type (if necessary) and storing the resolved Type
        /// in the object definition for further use.
        /// </summary>
        /// <param name="rod">The merged object definition to dertermine the type for.</param>
        /// <param name="objectName">Name of the object (for error handling purposes).</param>
        /// <returns></returns>
        protected Type ResolveObjectType(RootObjectDefinition rod, string objectName)
        {
            try
            {
                if (rod.HasObjectType)
                {
                    return rod.ObjectType;
                }
                return rod.ResolveObjectType();
            }
            catch (TypeLoadException e)
            {
                throw new CannotLoadObjectTypeException(rod.ResourceDescription, objectName, rod.ObjectTypeName, e);
            }
        }

        /// <summary>
        /// Is the object (definition) with the supplied <paramref name="name"/> an
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/>?
        /// </summary>
        /// <param name="name">The name of the object to be checked.</param>
        /// <returns>
        /// <see lang="true"/> the object (definition) with the supplied
        /// <paramref name="name"/> an <see cref="Spring.Objects.Factory.IFactoryObject"/>?
        /// </returns>
        protected bool IsFactoryObject(string name)
        {
            string objectName = TransformedObjectName(name);
            object objectInstance = GetSingleton(objectName);

            if (objectInstance != null)
            {
                return (objectInstance is IFactoryObject);
            }
            else
            {
                RootObjectDefinition definition = GetMergedObjectDefinition(objectName, false);
                if (definition != null)
                {
                    return (definition.HasObjectType && typeof(IFactoryObject).IsAssignableFrom(definition.ObjectType));
                }
                else
                {
                    if (parentObjectFactory != null)
                    {
                        return ((AbstractObjectFactory)parentObjectFactory).IsFactoryObject(name);
                    }
                    else
                    {
                        throw new NoSuchObjectDefinitionException(objectName,
                                                                  "Cannot find definition for object [" + objectName
                                                                  + "]");
                    }
                }
            }
        }

        /// <summary>
        /// Remove the object identified by the supplied <paramref name="name"/>
        /// from this factory's singleton cache.
        /// </summary>
        /// <param name="name">
        /// The name of the object that is to be removed from the singleton
        /// cache.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="name"/> argument is <see langword="null"/> or
        /// consists wholly of whitespace characters.
        /// </exception>
        protected void RemoveSingleton(string name)
        {
            AssertUtils.ArgumentHasText(name, "name");
            lock (GetSingletonLockFor(name))
            {
                singletonCache.Remove(name);
            }
        }

        /// <summary>
        /// Return the names of objects in the singleton cache that match the given
        /// object type (including subclasses).
        /// </summary>
        /// <param name="type">
        /// The class or interface to match, or <see langword="null"/> for all object names.
        /// </param>
        /// <remarks>
        /// <p>
        /// Will <i>not</i> consider <see cref="Spring.Objects.Factory.IFactoryObject"/>s
        /// as the type of their created objects is not known before instantiation.
        /// </p>
        /// <p>
        /// Does not consider any hierarchy this factory may participate in.
        /// </p>
        /// </remarks>
        /// <returns>
        /// The names of objects in the singleton cache that match the given
        /// object type (including subclasses), or an empty array if none.
        /// </returns>
        public virtual IList<string> GetSingletonNames(Type type)
        {
            lock (singletonCache)
            {
                List<string> matches = new List<string>();
                foreach (string name in singletonCache.Keys)
                {
                    object singletonObject = singletonCache[name];
                    if (singletonObject != null && type.IsAssignableFrom(singletonObject.GetType())
                        && !matches.Contains(name))
                    {
                        matches.Add(name);
                    }
                }
                return matches;
            }
        }

        /// <summary>
        /// Determines whether the object with the given name matches the specified type.
        /// </summary>
        /// <remarks>More specifically, check whether a GetObject call for the given name
        /// would return an object that is assignable to the specified target type.
        /// Translates aliases back to the corresponding canonical instance name.
        /// Will ask the parent factory if the instance cannot be found in this factory instance.
        /// </remarks>
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
            string objectName = TransformedObjectName(name);
            Type typeToMatch = (targetType != null ? targetType : typeof(object));

            //Check manually registered singletons.
            object objectInstance = GetSingleton(objectName);
            if (objectInstance != null)
            {
                if (objectInstance is IFactoryObject)
                {
                    if (!IsFactoryDereference(name))
                    {
                        Type type = GetTypeForFactoryObject((IFactoryObject)objectInstance);
                        return (type != null && typeToMatch.IsAssignableFrom(type));
                    }
                    else
                    {
                        return typeToMatch.IsAssignableFrom(objectInstance.GetType());
                    }
                }
                else
                {
                    return !IsFactoryDereference(name) && typeToMatch.IsAssignableFrom(objectInstance.GetType());
                }
            }
            else
            {
                // No singleton instance found -> check object definition
                IObjectFactory parentFactory = ParentObjectFactory;
                if (parentFactory != null && !ContainsObjectDefinition(name))
                {
                    // No object definition found in this factory -> delegate to parent
                    return parentFactory.IsTypeMatch(OriginalObjectName(name), targetType);
                }

                RootObjectDefinition mod = GetMergedObjectDefinition(objectName, false);
                Type objectType = PredictObjectType(objectName, mod);

                if (objectType == null)
                {
                    return false;
                }

                // Check object class whether we're dealing with a FactoryObject
                if (typeof(IFactoryObject).IsAssignableFrom(objectType))
                {
                    if (!IsFactoryDereference(name))
                    {
                        // If it's a FactoryObject, we want to look at what it creates, not the factory class.
                        Type type = GetTypeForFactoryObject(objectName, mod);
                        return (type != null && typeToMatch.IsAssignableFrom(type));
                    }
                    else
                    {
                        return typeToMatch.IsAssignableFrom(objectType);
                    }
                }
                else
                {
                    return !IsFactoryDereference(name) && typeToMatch.IsAssignableFrom(objectType);
                }
            }
        }

        /// <summary>
        /// Determines whether the object with the given name matches the specified type.
        /// </summary>
        /// <remarks>More specifically, check whether a GetObject call for the given name
        /// would return an object that is assignable to the specified target type.
        /// Translates aliases back to the corresponding canonical instance name.
        /// Will ask the parent factory if the instance cannot be found in this factory instance.
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
        /// Determines the <see cref="System.Type"/> of the object with the
        /// supplied <paramref name="name"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// More specifically, checks the <see cref="System.Type"/> of object that
        /// <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/> would return.
        /// For an <see cref="Spring.Objects.Factory.IFactoryObject"/>, returns the
        /// <see cref="System.Type"/> of object that the
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates.
        /// </p>
        /// <p>
        /// Please note that (prototype) objects created via a factory method or
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> objects are handled
        /// slightly differently, in that we <b>don't</b> want to needlessly create
        /// instances of such objects just to determine the <see cref="System.Type"/>
        /// of object that they create.
        /// </p>
        /// </remarks>
        /// <param name="name">The name of the object to query.</param>
        /// <returns>
        /// The <see cref="System.Type"/> of the object or <see langword="null"/>
        /// if not determinable.
        /// </returns>
        public virtual Type GetType(string name)
        {
            string objectName = TransformedObjectName(name);

            // check manually registered singletons...
            object objectInstance = GetSingleton(objectName);

            if (objectInstance != null)
            {
                IFactoryObject factoryObject = objectInstance as IFactoryObject;
                if (factoryObject != null & !IsFactoryDereference(objectName))
                {
                    return GetTypeForFactoryObject(factoryObject);
                }
                else
                {
                    return objectInstance.GetType();
                }
            }
            else
            {
                // No singleton instance found -> check object definition.
                IObjectFactory parentFactory = ParentObjectFactory;
                if (parentFactory != null && !ContainsObjectDefinition(objectName))
                {
                    // No object definition found in this factory -> delegate to parent.
                    return parentFactory.GetType(OriginalObjectName(name));
                }

                RootObjectDefinition mod = GetMergedObjectDefinition(objectName, false);
                Type objectType = PredictObjectType(objectName, mod);

                if (objectType != null && typeof(IFactoryObject).IsAssignableFrom(objectType))
                {
                    if (!IsFactoryDereference(name))
                    {
                        // If it's a FactoryObject, we want to look at what it creates, not the factory class.
                        return GetTypeForFactoryObject(objectName, mod);
                    }
                    else
                    {
                        return objectType;
                    }
                }
                else
                {
                    return (!IsFactoryDereference(name) ? objectType : null);
                }
            }
        }

        /// <summary>
        /// Determines the <see cref="System.Type"/> of the object defined
        /// by the supplied object <paramref name="definition"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This, the default, implementation returns <see lang="null"/>
        /// to indicate that the type cannot be determined. Subclasses are
        /// encouraged to try to determine the actual return
        /// <see cref="System.Type"/> here, matching their strategy of resolving
        /// factory methods in the
        /// <code>Spring.Objects.Factory.Support.AbstractObjectFactory.CreateObject</code>
        /// implementation.
        /// </p>
        /// </remarks>
        /// <param name="objectName">
        /// The name associated with the supplied object <paramref name="definition"/>.
        /// </param>
        /// <param name="definition">
        /// The <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
        /// that the <see cref="System.Type"/> is to be determined for.
        /// </param>
        /// <returns>
        /// The <see cref="System.Type"/> of the object defined by the supplied
        /// object <paramref name="definition"/>; or <see lang="null"/> if the
        /// <see cref="System.Type"/> cannot be determined.
        /// </returns>
        protected virtual Type GetTypeForFactoryMethod(string objectName, RootObjectDefinition definition)
        {
            return null;
        }

        /// <summary>
        /// Returns the names of the objects in the singleton cache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Does not consider any hierarchy this factory may participate in.
        /// </p>
        /// </remarks>
        /// <returns>The names of the objects in the singleton cache.</returns>
        public virtual IList<string> GetSingletonNames()
        {
            lock (singletonCache)
            {
                IEnumerable<string> keys = singletonCache.Keys.Cast<string>();
                return new List<string>(keys);
            }
        }

        /// <summary>
        /// Returns the number of objects in the singleton cache.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Does not consider any hierarchy this factory may participate in.
        /// </p>
        /// </remarks>
        /// <returns>The number of objects in the singleton cache.</returns>
        public virtual int GetSingletonCount()
        {
            lock (singletonCache)
            {
                return singletonCache.Count;
            }
        }

        /// <summary>
        /// Destroys the named singleton object.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Delegates to
        /// <see cref="Spring.Objects.Factory.Support.AbstractObjectFactory.DestroyObject"/>
        /// if a corresponding singleton instance is found.
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The name of the singleton object that is to be destroyed.
        /// </param>
        /// <seealso cref="Spring.Objects.Factory.Support.AbstractObjectFactory.DestroyObject"/>
        protected virtual void DestroySingleton(string name)
        {
            lock (singletonLocks)
            {
                object tempObject = singletonCache[name];
                singletonCache.Remove(name);
                registeredSingletons.Remove(name);

                object singletonInstance = tempObject;
                if (singletonInstance != null)
                {
                    DestroyObject(name, singletonInstance);
                }
            }
        }

        /// <summary>
        /// Check the supplied merged object definition for any possible
        /// validation errors.
        /// </summary>
        /// <param name="mergedObjectDefinition">
        /// The object definition to be checked for validation errors.
        /// </param>
        /// <param name="objectName">
        /// The name of the object associated with the supplied object definition.
        /// </param>
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
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of object validation errors.
        /// </exception>
        protected void CheckMergedObjectDefinition(RootObjectDefinition mergedObjectDefinition, string objectName,
                                                   Type requiredType, params object[] arguments)
        {
            // check if required type can match according to the object definition;
            // this is only possible at this early stage for conventional objects!
            if (mergedObjectDefinition.HasObjectType)
            {
                Type objectType = mergedObjectDefinition.ObjectType;
                if (requiredType != null && StringUtils.IsNullOrEmpty(mergedObjectDefinition.FactoryMethodName)
                    && !typeof(IFactoryObject).IsAssignableFrom(objectType)
                    && !requiredType.IsAssignableFrom(objectType))
                {
                    throw new ObjectNotOfRequiredTypeException(objectName, requiredType, objectType);
                }
            }

            // check validity of the usage of the args parameter; this can
            // only be used for prototypes constructed via a factory method...
            if (arguments != null)
            {
                if (mergedObjectDefinition.IsSingleton)
                {
                    throw new ObjectDefinitionStoreException("Cannot specify arguments in the GetObject () method when "
                                                             + "referring to a singleton object definition.");
                }

                if (mergedObjectDefinition.HasObjectType
                    && typeof(IFactoryObject).IsAssignableFrom(mergedObjectDefinition.ObjectType))
                {
                    throw new ObjectDefinitionStoreException("Cannot specify arguments in the GetObject () method when "
                                                             + "referring to a factory object definition.");
                }

                //MLP lets skip this check for now.
                /*
                else if (StringUtils.IsNullOrEmpty(mergedObjectDefinition.FactoryMethodName))
                {
                    throw new ObjectDefinitionStoreException(
                        "Can only specify arguments in the GetObject () method in " +
                            "conjunction with a factory method.");
                }
                */
            }
        }

        /// <summary>
        /// Remove the merged bean definition for the specified bean,
        /// recreating it on next access.
        /// </summary>
        /// <param name="objectName">The bean name to clear the merged definition for</param>
        protected void ClearMergedObjectDefinition(string objectName)
        {
            mergedObjectDefinitions.TryRemove(objectName, out _);
        }

        /// <summary>
        /// Clear the merged object definition cache, removing entries for objects
        /// which are not considered eligible for full metadata caching yet.
        /// </summary>
        /// <remarks>
        /// Typically triggered after changes to the original object definitions,
        /// e.g. after applying a <see cref="IObjectFactoryPostProcessor" />. Note that metadata
        /// for objects which have already been created at this point will be kept around.
        /// </remarks>
        public virtual void ClearMetadataCache()
        {
            var keys = new List<string>(mergedObjectDefinitions.Keys);
            foreach (var key in keys)
            {
                if (!IsObjectEligibleForMetadataCaching(key))
                {
                    mergedObjectDefinitions.TryRemove(key, out _);
                }
            }
        }

        /// <summary>
        /// Gets the temporary object that is placed
        /// into the singleton cache during object resolution.
        /// </summary>
        protected object TemporarySingletonPlaceHolder => CurrentlyInCreation;

        /// <summary>
        /// Parent object factory, for object inheritance support
        /// </summary>
        private IObjectFactory parentObjectFactory;

        /// <summary>
        /// Dependency types to ignore on dependency check and autowire, as Set of
        /// Type objects: for example, string.  Default is none.
        /// </summary>
        private HybridSet ignoreDependencyTypes = new HybridSet();

        /// <summary>
        /// ObjectPostProcessors to apply in CreateObject
        /// </summary>
        internal List<IObjectPostProcessor> objectPostProcessors = new List<IObjectPostProcessor>();

        /// <summary>
        /// String Resolver applied to Autowired value injections
        /// </summary>
        private SortedSet embeddedValueResolvers = new SortedSet(ObjectOrderComparator.ObjectOrderComparatorInstance);

        /// <summary>
        /// Indicates whether any IInstantiationAwareBeanPostProcessors have been registered
        /// </summary>
        private bool hasInstantiationAwareObjectPostProcessors;

        /// <summary>
        /// Indicates whether any IDestructionAwareBeanPostProcessors have been registered
        /// </summary>
        private bool hasDestructionAwareObjectPostProcessors;

        private bool caseSensitive;
        private OrderedDictionary aliasMap;
        private OrderedDictionary singletonCache;
        private ConcurrentDictionary<string, Lazy<object>> singletonLocks;

        /// <summary>
        /// Set of registered singletons, containing the instance names in registration order
        /// </summary>
        private HashSet<string> registeredSingletons = new HashSet<string>();

        private readonly IDictionary singletonsInCreation;

        private readonly LogicalThreadContextSetVariable prototypesInCreation;

        /// <summary>
        /// Set that holds all inner objects created by this factory that implement the IDisposable
        /// interface, to be destroyed on call to Dispose.
        /// </summary>
        private SynchronizedSet disposableInnerObjects = new SynchronizedSet(new HybridSet());

        /// <summary>
        /// Set that holds all inner objects created by this factory that implement the IDisposable
        /// interface, to be destroyed on call to Dispose.
        /// </summary>
        protected internal ISet DisposableInnerObjects => disposableInnerObjects;

        /// <summary>
        /// The parent object factory, or <see langword="null"/> if there is none.
        /// </summary>
        /// <value>
        /// The parent object factory, or <see langword="null"/> if there is none.
        /// </value>
        public IObjectFactory ParentObjectFactory
        {
            get => parentObjectFactory;
            set => parentObjectFactory = value;
        }

        /// <summary>
        /// Determines whether the local object factory contains a instance of the given name,
        /// ignoring object defined in ancestor contexts.
        /// This is an alternative to <code>ContainsObject</code>, ignoring an object
        /// of the given name from an ancestor object factory.
        /// </summary>
        /// <param name="name">The name of the object to query.</param>
        /// <returns>
        /// 	<c>true</c> if objects with the specified name is defined in the local factory; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsLocalObject(string name)
        {
            string objectName = TransformedObjectName(name);
            return ((ContainsSingleton(objectName) || ContainsObjectDefinition(objectName)) &&
                    (!ObjectFactoryUtils.IsFactoryDereference(name) || IsFactoryObject(objectName)));
        }

        /// <summary>
        /// Is this object a singleton?
        /// </summary>
        /// <see cref="Spring.Objects.Factory.IObjectFactory.IsSingleton"/>
        public bool IsSingleton(string name)
        {
            string objectName = TransformedObjectName(name);
            object objectInstance = GetSingleton(objectName);
            if (objectInstance != null)
            {
                IFactoryObject factoryObject = objectInstance as IFactoryObject;
                if (factoryObject != null)
                {
                    return IsFactoryDereference(name) || factoryObject.IsSingleton;
                }
                else
                {
                    return !IsFactoryDereference(name);
                }
            }
            else
            {
                // No singleton instance found -> check object definition
                IObjectFactory pof = ParentObjectFactory;
                if (pof != null && !ContainsObjectDefinition(objectName))
                {
                    // No object definition found in this factory -> delegate to parent
                    return pof.IsSingleton(OriginalObjectName(name));
                }
                RootObjectDefinition od = GetMergedObjectDefinition(objectName, false);
                if (od == null)
                {
                    throw new NoSuchObjectDefinitionException(objectName);
                }
                // In case of IFactoryObject, return singleton status of created object if not a dereference
                if (od.IsSingleton)
                {
                    if (IsObjectTypeMatch(objectName, od, typeof(IFactoryObject)))
                    {
                        if (IsFactoryDereference(name))
                        {
                            return true;
                        }
                        IFactoryObject factoryObject =
                            (IFactoryObject)GetObject(ObjectFactoryUtils.BuildFactoryObjectName(objectName));
                        return factoryObject.IsSingleton;
                    }
                    else
                    {
                        return !IsFactoryDereference(name);
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified object name is prototype.  That is, will GetObject
        /// always return independent instances?
        /// </summary>
        /// <param name="name">The name of the object to query</param>
        /// <returns>
        /// 	<c>true</c> if the specified object name will always deliver independent instances; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>This method returning false does not clearly indicate a singleton object.
        /// It indicated non-independent instances, which may correspond to a scoped object as
        /// well.  use the IsSingleton property to explicitly check for a shared
        /// singleton instance.
        /// <para>Translates aliases back to the corresponding canonical object name.  Will ask the
        /// parent factory if the object can not be found in this factory instance.
        /// </para>
        /// </remarks>
        /// <exception cref="NoSuchObjectDefinitionException">if there is no object with the given name.</exception>
        public bool IsPrototype(string name)
        {
            string objectName = TransformedObjectName(name);
            IObjectFactory parentFactory = ParentObjectFactory;
            if (parentFactory != null && !ContainsObjectDefinition(objectName))
            {
                // No object definition found in this factory -> delegate to parent
                return parentFactory.IsPrototype(OriginalObjectName(name));
            }

            RootObjectDefinition od = GetMergedObjectDefinition(objectName, false);

            // In case of FactoryObject, return singleton status of created object if not a dereference
            if (od.IsPrototype)
            {
                return (!IsFactoryDereference(name) || IsObjectTypeMatch(objectName, od, typeof(IFactoryObject)));
            }
            else
            {
                // not a prototype, however factory object may still produce a prototype object
                if (IsFactoryDereference(name) && IsObjectTypeMatch(objectName, od, typeof(IFactoryObject)))
                {
                    IFactoryObject factoryObject = GetFactoryObject(objectName);
                    return (!factoryObject.IsSingleton);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Does this object factory or one of its parent factories contain an object with the given name?
        /// </summary>
        /// <remarks>
        /// This method scans the object factory hierarchy starting with the current factory instance upwards.
        /// Use <see cref="ContainsLocalObject"/> if you want to explicitely check just this object factory instance.
        /// </remarks>
        /// <see cref="Spring.Objects.Factory.IObjectFactory.ContainsObject"/>.
        public bool ContainsObject(string name)
        {
            string objectName = TransformedObjectName(name);

            if (ContainsSingleton(objectName) || ContainsObjectDefinition(objectName))
            {
                return (!ObjectFactoryUtils.IsFactoryDereference(name) || IsFactoryObject(name));
            }

            IObjectFactory parent = ParentObjectFactory;
            return (parent != null) && parent.ContainsObject(OriginalObjectName(name));
        }

        /// <summary>
        /// Return the aliases for the given object name, if defined.
        /// </summary>
        /// <see cref="Spring.Objects.Factory.IObjectFactory.GetAliases"/>.
        public IReadOnlyList<string> GetAliases(string name)
        {
            return DoGetAliases(name);
        }

        internal List<string> DoGetAliases(string name)
        {
            string objectName = TransformedObjectName(name);
            // check if object actually exists in this object factory...
            bool isInSingletonCache = ContainsSingleton(objectName);
            if (isInSingletonCache || ContainsObjectDefinition(objectName))
            {
                // if found, gather aliases...
                var matches = new List<string>();
                foreach (DictionaryEntry aliasEntry in aliasMap)
                {
                    if (0 == string.Compare((string) aliasEntry.Value, objectName, !IsCaseSensitive))
                    {
                        matches.Add((string) aliasEntry.Key);
                    }
                }

                return matches;
            }

            // not found, so check parent...
            if (ParentObjectFactory != null)
            {
                return new List<string>(ParentObjectFactory.GetAliases(objectName));
            }

            throw new NoSuchObjectDefinitionException(objectName, ToString());
        }

        /// <summary>
        /// Return an instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>.
        public object this[string name] => GetObject(name);

        /// <summary>
        /// Return an unconfigured(!) instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.CreateObject(string, Type, object[])"/>
        /// <remarks>
        ///  This method will only <b>instantiate</b> the requested object. It does <b>NOT</b> inject any dependencies!
        /// </remarks>
        public object CreateObject(string name, Type requiredType, object[] arguments)
        {
            return GetObjectInternal(name, requiredType, arguments, true);
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
        /// Return an instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>.
        public object GetObject(string name)
        {
            return GetObjectInternal(name, null, null, false);
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
        public abstract T GetObject<T>();

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
            return (T)GetObject(name);
        }

        /// <summary>
        /// Return an instance (possibly shared or independent) of the given object name.
        /// </summary>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetObject(string, Type)"/>
        public object GetObject(string name, Type requiredType)
        {
            return GetObjectInternal(name, requiredType, null, false);
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
            return GetObjectInternal(name, null, arguments, false);

            //            string objectName = TransformedObjectName(name);
            //            object instance = null;
            //
            //            // check if object definition exists
            //            RootObjectDefinition mergedObjectDefinition = null;
            //            mergedObjectDefinition = GetMergedObjectDefinition(objectName, false);
            //            if (mergedObjectDefinition == null)
            //            {
            //                if (ParentObjectFactory != null)
            //                {
            //                    return ParentObjectFactory.GetObject(name, arguments);
            //                }
            //                throw new NoSuchObjectDefinitionException(name, "Cannot find definition for object [" + name + "]");
            //            }
            //
            //            // Override constructor values and configure as a prototype
            //            RootObjectDefinition tmpObjectDefinition = new RootObjectDefinition(mergedObjectDefinition);
            //            tmpObjectDefinition.ConstructorArgumentValues = null;
            //            tmpObjectDefinition.IsSingleton = false;
            //
            //            // create a new instance...
            //            instance = CreateObject(name, tmpObjectDefinition, arguments);
            //
            //            return GetObjectForInstance(name, instance);
        }

        /// <summary>
        /// Return an instance (possibly shared or independent) of the given object name,
        /// optionally injecting dependencies.
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
        /// <param name="suppressConfigure">whether to inject dependencies or not.</param>
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
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.CreateObject(string, Type, object[])"/>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.GetObject(string, Type, object[])"/>
        protected object GetObjectInternal(string name, Type requiredType, object[] arguments, bool suppressConfigure)
        {
            object monitor = new object();
            const int indent = 3;
            bool hasErrors = false;
            var isDebugEnabled = log.IsDebugEnabled;

            try
            {
                string objectName = TransformedObjectName(name);
                Interlocked.Increment(ref nestingCount);

                if (isDebugEnabled)
                {
                    log.Debug(string.Format("{2}GetObjectInternal: obtaining instance for name {0} => canonical name {1}", name, objectName, new string(' ', nestingCount * indent)));
                }

                object instance;

                // those are cases, where singleton cache can be used
                if (arguments == null && !suppressConfigure)
                {
                    // eagerly check singleton cache for manually registered singletons...
                    object sharedInstance = GetSingleton(objectName);
                    if (sharedInstance != null)
                    {
                        if (isDebugEnabled)
                        {
                            if (IsSingletonCurrentlyInCreation(objectName))
                            {
                                log.Debug("Returning eagerly cached instance of singleton object '" + objectName +
                                          "' that is not fully initialized yet - a consequence of a circular reference");
                            }
                            else
                            {
                                log.Debug($"Returning cached instance of singleton object '{objectName}'.");
                            }
                        }

                        instance = GetObjectForInstance(sharedInstance, name, objectName, null);
                        return EnsureObjectIsOfRequiredType(name, instance, requiredType);
                    }
                }

                var set = prototypesInCreation.Value;
                if (set.Contains(name))
                {
                    throw new ObjectCurrentlyInCreationException(name);
                }

                if (!suppressConfigure)
                {
                    MarkObjectAsCreated(objectName);
                }

                // check if object definition exists
                var mergedObjectDefinition = GetMergedObjectDefinition(objectName, false);
                if (mergedObjectDefinition == null)
                {
                    if (ParentObjectFactory != null)
                    {
                        return ParentObjectFactory.GetObject(name, requiredType, arguments);
                    }
                    throw new NoSuchObjectDefinitionException(name, "Cannot find definition for object [" + name + "]");
                }

                if (arguments != null || suppressConfigure)
                {
                    // Clone ObjectDefinition
                    mergedObjectDefinition = CreateRootObjectDefinition(mergedObjectDefinition);
                    mergedObjectDefinition.IsSingleton = false;
                    if (arguments != null)
                    {
                        // Override constructor values and configure as a prototype if arguments are specified
                        mergedObjectDefinition.ConstructorArgumentValues = null;
                    }
                }

                CheckMergedObjectDefinition(mergedObjectDefinition, objectName, requiredType, arguments);

                // return IObjectDefinition instance itself for an abstract object-definition
                if (mergedObjectDefinition.IsAbstract)
                {
                    instance = mergedObjectDefinition;
                }
                else if (mergedObjectDefinition.IsSingleton)
                {
                    // create object instance...
                    object sharedInstance = CreateAndCacheSingletonInstance(objectName, mergedObjectDefinition, arguments);
                    instance = GetObjectForInstance(sharedInstance, name, objectName, mergedObjectDefinition);
                }
                else
                {
                    // it's a prototype, so create a new instance...
                    set.Add(name);
                    try
                    {
                        instance = InstantiateObject(name, mergedObjectDefinition, arguments, true, suppressConfigure);
                    }
                    finally
                    {
                        if (!set.Remove(name))
                        {
                            ThrowNotCurrentlyInCreation(name);
                        }
                    }
                }

                try
                {
                    RegisterDisposableObjectIfNecessary(name, instance, mergedObjectDefinition);
                }
                catch (ObjectDefinitionValidationException ex)
                {
                    throw new ObjectCreationException(mergedObjectDefinition.ResourceDescription, name, "Invalid destruction signature", ex);
                }

                return EnsureObjectIsOfRequiredType(name, instance, requiredType);
            }
            catch
            {
                lock (monitor)
                {
                    if (nestingCount > 0)
                    {
                        nestingCount--;
                    }
                }

                CleanupAfterObjectCreationFailure(name);

                hasErrors = true;
                if (log.IsErrorEnabled)
                {
                    log.Error(string.Format("{1}GetObjectInternal: error obtaining object {0}", name, new string(' ', nestingCount * indent)));
                }

                throw;
            }
            finally
            {
                if (!hasErrors)
                {
                    lock (monitor)
                    {
                        if (nestingCount > 0)
                        {
                            nestingCount--;
                        }
                    }

                    if (isDebugEnabled)
                    {
                        log.Debug(string.Format("{1}GetObjectInternal: returning instance for objectname {0}", name, new string(' ', nestingCount * indent)));
                    }
                }
            }
        }

        protected virtual void RegisterDisposableObjectIfNecessary(string name, object instance, RootObjectDefinition od)
        {
            if (od.IsSingleton && RequiresDestruction(instance, od))
            {
                // Register a DisposableObject implementation that performs all destruction
                // work for the given object: DestructionAwareObjectPostProcessors,
                // DisposableObject interface, custom destroy method.
                RegisterDisposableObject(name,
                        new DisposableObjectAdapter(instance, name, od, objectPostProcessors));

            }
        }

        /// <summary>
        /// Requireses the destruction.
        /// </summary>
        /// <param name="instance">The instance to check.</param>
        /// <param name="od">The corresponding instance definition.</param>
        /// <returns>
        /// Boolean indicating whether destruction is required.
        /// </returns>

        protected bool RequiresDestruction(object instance, RootObjectDefinition od)
        {
            return (instance != null &&
                (instance is IDisposable || od.DestroyMethodName != null || HasDestructionAwareObjectPostProcessors));
        }

        private int nestingCount;

        /// <summary>
        /// Checks, if the passed instance is of the required type.
        /// </summary>
        /// <param name="name">the name of the object</param>
        /// <param name="instance">the actual instance</param>
        /// <param name="requiredType">the type contract the given instance must adhere.</param>
        /// <returns>the object instance passed in via <paramref name="instance"/>(for more fluent usage)</returns>
        /// <exception cref="ObjectNotOfRequiredTypeException">
        /// if <paramref name="instance"/> is null or not assignable to <paramref name="requiredType"/>.
        /// </exception>
        private object EnsureObjectIsOfRequiredType(string name, object instance, Type requiredType)
        {
            // check that any required type matches the type of the actual object instance...
            if (requiredType != null && instance != null && !requiredType.IsAssignableFrom(instance.GetType()))
            {
                throw new ObjectNotOfRequiredTypeException(name, requiredType, instance);
            }

            return instance;
        }

        /// <summary>
        /// Creates a singleton instance for the specified object name and definition.
        /// </summary>
        /// <param name="objectName">
        /// The object name (will be used as the key in the singleton cache key).
        /// </param>
        /// <param name="objectDefinition">The object definition.</param>
        /// <param name="arguments">
        /// The arguments to use if creating a prototype using explicit arguments to
        /// a static factory method. If there is no factory method and the
        /// arguments are not null, then match the argument values by type and
        /// call the object's constructor.
        /// </param>
        /// <returns>The created object instance.</returns>
        protected virtual object CreateAndCacheSingletonInstance(string objectName,
                                                                 RootObjectDefinition objectDefinition,
                                                                 object[] arguments)
        {
            lock (GetSingletonLockFor(objectName))
            {
                object sharedInstance = singletonCache[objectName];
                if (sharedInstance == null)
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(string.Format("Creating shared instance of singleton object '{0}'", objectName));
                    }

                    BeforeSingletonCreation(objectName);
                    try
                    {
                        sharedInstance = InstantiateObject(objectName, objectDefinition, arguments, true, false);
                    }
                    finally
                    {
                        AfterSingletonCreation(objectName);
                    }
                    AddSingleton(objectName, sharedInstance);

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(string.Format("Cached shared instance of singleton object '{0}'", objectName));
                    }
                }
                return sharedInstance;
            }
        }

        /// <summary>
        /// Injects dependencies into the supplied <paramref name="target"/> instance
        /// using the named object definition.
        /// </summary>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.ConfigureObject(object, string)"/>
        public abstract object ConfigureObject(object target, string name);

        /// <summary>
        /// Injects dependencies into the supplied <paramref name="target"/> instance
        /// using the supplied <paramref name="definition"/>.
        /// </summary>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.ConfigureObject(object, string)"/>
        public abstract object ConfigureObject(object target, string name, IObjectDefinition definition);

        /// <summary>
        /// Destroy all cached singletons in this factory.
        /// </summary>
        public virtual void Dispose()
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format("Destroying singletons in factory [{0}].", this));
            }

            prototypesInCreation.Dispose();

            lock (singletonCache)
            {
                // copy the keys into a new set, 'cos we are going to modifying the
                // original collection (_singletonCache) as we destroy each singleton.
                // we also want to traverse the keys in reverse order to destroy correctly
                ArrayList keys = new ArrayList(singletonCache.Keys);
                keys.Reverse();
                foreach (string name in keys)
                {
                    DestroySingleton(name);
                }
            }
        }

        /// <summary>
        /// Ignore the given dependency type for autowiring
        /// </summary>
        /// <seealso cref="Spring.Objects.Factory.Config.IConfigurableObjectFactory.IgnoreDependencyType"/>.
        public void IgnoreDependencyType(Type type)
        {
            IgnoredDependencyTypes.Add(type);
        }

        /// <summary>
        /// Determines whether the specified object name is currently in creation..
        /// </summary>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>
        /// 	<c>true</c> if the specified object name is currently in creation; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCurrentlyInCreation(string objectName)
        {
            return IsSingletonCurrentlyInCreation(objectName) || IsPrototypeCurrentlyInCreation(objectName);
        }

        private static void ThrowNotCurrentlyInCreation(string name)
        {
            throw new InvalidOperationException("Singleton " + name + " isn't currently in creation.");
        }

        private bool IsPrototypeCurrentlyInCreation(string name)
        {
            return prototypesInCreation.Value.Contains(name);
        }

        private void AfterSingletonCreation(string name)
        {
            if (!IsSingletonCurrentlyInCreation(name))
            {
                throw new InvalidOperationException("Singleton " + name + " isn't currently in creation.");
            }
            singletonsInCreation.Remove(name);
        }

        private void BeforeSingletonCreation(string name)
        {
            if (singletonsInCreation.Contains(name))
            {
                throw new ObjectCurrentlyInCreationException(name);
            }
            singletonsInCreation.Add(name, EmptyObject);
        }

        private bool IsSingletonCurrentlyInCreation(string name)
        {
            return singletonsInCreation.Contains(name);
        }

        /// <summary>
        /// Add a String resolver for embedded values such as annotation attributes.
        /// </summary>
        /// <param name="valueResolver">the String resolver to apply to embedded values</param>
        public void AddEmbeddedValueResolver(IStringValueResolver valueResolver)
        {
            embeddedValueResolvers.Add(valueResolver);
        }

        /// <summary>
        /// Resolve the given embedded value, e.g. an annotation attribute.
        /// </summary>
        /// <param name="value">the value to resolve</param>
        /// <returns>the resolved value (may be the original value as-is)</returns>
        public string ResolveEmbeddedValue(string value)
        {
            string result = value;
            foreach (IStringValueResolver resolver in embeddedValueResolvers)
            {
                result = resolver.ParseAndResolveVariables(result);
            }
            return result;
        }

        /// <summary>
        /// Add a new <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>
        /// that will get applied to objects created by this factory.
        /// </summary>
        /// <param name="objectPostProcessor">
        /// The <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>
        /// to register.
        /// </param>
        /// <seealso cref="Spring.Objects.Factory.Config.IConfigurableObjectFactory.AddObjectPostProcessor"/>.
        public void AddObjectPostProcessor(IObjectPostProcessor objectPostProcessor)
        {
            if (objectPostProcessor is IObjectFactoryAware)
            {
                ((IObjectFactoryAware)objectPostProcessor).ObjectFactory = this;
            }

            // ensure the same instance doesn't get registered twice
            if (!objectPostProcessors.Contains(objectPostProcessor))
            {
                objectPostProcessors.Add(objectPostProcessor);
            }
            if (typeof(IInstantiationAwareObjectPostProcessor).IsInstanceOfType(objectPostProcessor))
            {
                hasInstantiationAwareObjectPostProcessors = true;
            }
            if (typeof(IDestructionAwareObjectPostProcessor).IsInstanceOfType(objectPostProcessor))
            {
                hasDestructionAwareObjectPostProcessors = true;
            }
        }

        /// <summary>
        /// Returns the current number of registered
        /// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>s.
        /// </summary>
        /// <value>
        /// The current number of registered
        /// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>s.
        /// </value>
        /// <seealso cref="Spring.Objects.Factory.Config.IConfigurableObjectFactory.ObjectPostProcessorCount"/>.
        public int ObjectPostProcessorCount => objectPostProcessors.Count;

        /// <summary>
        /// Given an object name, create an alias.
        /// </summary>
        /// <seealso cref="Spring.Objects.Factory.Config.IConfigurableObjectFactory.RegisterAlias"/>.
        public void RegisterAlias(string name, string alias)
        {
            AssertUtils.ArgumentHasText(name, "The object name must not be empty.");
            AssertUtils.ArgumentHasText(alias, "The alias must not be empty.");

            if (name == alias)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(
                        $"Ignoring attempt to Register alias '{alias}' for object with name '{name}' because name and alias would be the same value.");
                }

                return;
            }

            if (log.IsDebugEnabled)
            {
                log.Debug($"Registering alias '{alias}' for object with name '{name}'.");
            }

            object registeredName = aliasMap[alias];
            if (registeredName != null)
            {
                throw new ObjectDefinitionStoreException(
                    $"Cannot register alias '{alias}' for object with name '{name}': it's already registered for object name '{registeredName}'.");
            }

            aliasMap[alias] = name;
        }

        /// <summary>
        /// Register the given custom <see cref="System.ComponentModel.TypeConverter"/>
        /// for all properties of the given <see cref="System.Type"/>.
        /// </summary>
        /// <seealso cref="Spring.Objects.Factory.Config.IConfigurableObjectFactory.RegisterCustomConverter"/>.
        public void RegisterCustomConverter(Type requiredType, TypeConverter converter)
        {
            AssertUtils.ArgumentNotNull(requiredType, "requiredType");
            TypeConverterRegistry.RegisterConverter(requiredType, converter);
        }

        /// <summary>
        /// Register the given existing object as singleton in the object factory,
        /// under the given object name.
        /// </summary>
        /// <seealso cref="Spring.Objects.Factory.Config.ISingletonObjectRegistry.RegisterSingleton"/>.
        public virtual void RegisterSingleton(string name, object singletonObject)
        {
            AssertUtils.ArgumentHasText(name, "name", "The singleton object cannot be registered under an empty name.");
            lock (GetSingletonLockFor(name))
            {
                object oldObject = singletonCache[name];
                if (oldObject != null)
                {
                    throw new ObjectDefinitionStoreException(
                        $"Could not register object [{singletonObject}] under object name '{name}': there's already object [{oldObject}] bound.");
                }
                AddSingleton(name, singletonObject);
            }
        }

        /// <summary>
        /// Does this object factory contains a singleton instance with the
        /// supplied <paramref name="name"/>?
        /// </summary>
        /// <seealso cref="Spring.Objects.Factory.Config.ISingletonObjectRegistry.ContainsSingleton(string)"/>
        public bool ContainsSingleton(string name)
        {
            AssertUtils.ArgumentHasText(name, "name");
            lock (GetSingletonLockFor(name))
            {
                return singletonCache.Contains(name);
            }
        }

        /// <summary>
        /// Gets the names of singleton objects registered in this registry.
        /// </summary>
        /// <value>The list of names as String array (never <code>null</code>).</value>
        /// <remarks>
        /// 	<para>
        /// Only checks already instantiated singletons; does not return names
        /// for singleton instance definitions which have not been instantiated yet.
        /// </para>
        /// 	<para>
        /// The main purpose of this method is to check manually registered singletons
        /// <see cref="RegisterSingleton"/>. Can also be used to check which
        /// singletons defined by an object definition have already been created.
        /// </para>
        /// </remarks>
        /// <see cref="RegisterSingleton"/>
        /// <see cref="IObjectDefinitionRegistry.GetObjectDefinitionNames()"/>
        /// <see cref="IListableObjectFactory.GetObjectDefinitionNames()"/>
        public IList<string> SingletonNames
        {
            get
            {
                lock (singletonCache)
                {
                    return
                        StringUtils.DelimitedListToStringArray(
                            StringUtils.CollectionToDelimitedString(registeredSingletons, ","), ",");

                }
            }
        }

        /// <summary>
        /// Gets the number of singleton beans registered in this registry.
        /// </summary>
        /// <value>The number of singleton objects.</value>
        /// <remarks>
        /// 	<para>
        /// Only checks already instantiated singletons; does not count
        /// singleton object definitions which have not been instantiated yet.
        /// </para>
        /// 	<para>
        /// The main purpose of this method is to check manually registered singletons
        /// <see cref="RegisterSingleton"/>.  Can also be used to count the number of
        /// singletons defined by an object definition that have already been created.
        /// </para>
        /// </remarks>
        /// <see cref="RegisterSingleton"/>
        /// <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry.ObjectDefinitionCount"/>
        /// <see cref="Spring.Objects.Factory.IListableObjectFactory.ObjectDefinitionCount"/>
        public int SingletonCount
        {
            get
            {
                lock (singletonCache)
                {
                    return registeredSingletons.Count;
                }
            }
        }
        /// <summary>
        /// Tries to find a cached object for the specified name.
        /// </summary>
        /// <param name="objectName">Teh object name to look for.</param>
        /// <returns>The cached object if found, <see langword="null"/> otherwise.</returns>
        public virtual object GetSingleton(string objectName)
        {
            lock (GetSingletonLockFor(objectName))
            {
                return singletonCache[objectName];
            }
        }


        /// <summary>
        /// Registers the disposable object.
        /// </summary>
        /// <param name="objectName">Name of the instance.</param>
        /// <param name="instance">The instance.</param>
        /// Add the given instance to the list of disposable beans in this registry.
        /// Disposable beans usually correspond to registered singletons,
        /// matching the instance name but potentially being a different instance
        /// (for example, a DisposableBean adapter for a singleton that does not
        /// naturally implement <see cref="IDisposable"/>).
        public void RegisterDisposableObject(string objectName, IDisposable instance)
        {
            if (disposableObjects.ContainsKey(objectName)) return;
            disposableObjects.Add(objectName, instance);
        }



        /// <summary>
        /// Determines whether the given object name is already in use within this factory,
        /// i.e. whether there is a local object or alias registered under this name or
        /// an inner object created with this name.
        /// </summary>
        /// <param name="objectName">Name of the object to check.</param>
        /// <returns>
        /// 	<c>true</c> if is object name in use; otherwise, <c>false</c>.
        /// </returns>
        public bool IsObjectNameInUse(string objectName)
        {
            return IsAlias(objectName) || ContainsLocalObject(objectName);
        }

        /// <summary>
        /// Gets the singleton lock for a given object name.
        /// </summary>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>lock object</returns>
        private object GetSingletonLockFor(string objectName)
        {
            return singletonLocks.GetOrAdd(objectName, key => new Lazy<object>(() => new object())).Value;
        }

        [Serializable]
        private class LogicalThreadContextSetVariable : IDisposable
        {
            private readonly string name = Guid.NewGuid().ToString();

            public HashSet<string> Value
            {
                get
                {
                    if (!(LogicalThreadContext.GetData(name) is HashSet<string> set))
                    {
                        set = new HashSet<string>();
                        LogicalThreadContext.SetData(name, set);
                    }
                    return set;
                }
            }

            public void Dispose()
            {
                LogicalThreadContext.FreeNamedDataSlot(name);
            }
        }

        /// <summary>
        /// Makes a distinction between sort order and object identity.
        /// This is important when used with <see cref="ISet"/>, since most
        /// implementations assume Order == Identity
        /// </summary>
        [Serializable]
        private class ObjectOrderComparator : OrderComparator
        {
            public static readonly ObjectOrderComparator ObjectOrderComparatorInstance = new ObjectOrderComparator();
            /// <summary>
            /// Handle the case when both objects have equal sort order priority. By default returns 0,
            /// but may be overriden for handling special cases.
            /// </summary>
            /// <param name="o1">The first object to compare.</param>
            /// <param name="o2">The second object to compare.</param>
            /// <returns>
            /// -1 if first object is less then second, 1 if it is greater, or 0 if they are equal.
            /// </returns>
            protected override int CompareEqualOrder(object o1, object o2)
            {
                if (ReferenceEquals(o1, o2))
                    return 0;
                if (o1 == null)
                    return 1;
                if (o2 == null)
                    return -1;
                return o1.GetHashCode().CompareTo(o2.GetHashCode());
            }
        }
    }
}
