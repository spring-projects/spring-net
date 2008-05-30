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
using System.Reflection;

using Common.Logging;
using Spring.Collections;
using Spring.Core;
using Spring.Core.TypeConversion;
using Spring.Core.TypeResolution;
using Spring.Expressions;
using Spring.Objects;
using Spring.Objects.Factory.Config;
using Spring.Objects.Support;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Abstract <see cref="Spring.Objects.Factory.IObjectFactory"/> superclass
    /// that implements default object creation.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Provides object creation, initialization and wiring, supporting
    /// autowiring and constructor resolution. Handles runtime object
    /// references, managed collections, and object destruction.
    /// </p>
    /// <p>
    /// The main template method to be implemented by subclasses is
    /// <see cref="Spring.Objects.Factory.Support.AbstractAutowireCapableObjectFactory.FindMatchingObjects"/>,
    /// used for autowiring by type. Note that this class does not implement object
    /// definition registry capabilities
    /// (<see cref="Spring.Objects.Factory.Support.DefaultListableObjectFactory"/>
    /// does).
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    /// <version>$Id: AbstractAutowireCapableObjectFactory.cs,v 1.90 2008/05/29 12:13:27 oakinger Exp $</version>
    [Serializable]
    public abstract class AbstractAutowireCapableObjectFactory : AbstractObjectFactory, IAutowireCapableObjectFactory
    {
        #region Constants

        /// <summary>
        /// The <see cref="System.Reflection.BindingFlags"/> used during the invocation and
        /// searching for of methods.
        /// </summary>
        protected const BindingFlags MethodResolutionFlags =
                BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;

        #endregion

        /// <summary>
        /// The <see cref="Common.Logging.ILog"/> instance for this class.
        /// </summary>
        private readonly ILog log = LogManager.GetLogger(typeof(AbstractAutowireCapableObjectFactory));

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.AbstractAutowireCapableObjectFactory"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes no public constructors.
        /// </p>
        /// </remarks>
        /// <param name="caseSensitive">Flag specifying whether to make this object factory case sensitive or not.</param>
        protected AbstractAutowireCapableObjectFactory(bool caseSensitive)
            : this(caseSensitive, null)
        { }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.AbstractAutowireCapableObjectFactory"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes no public constructors.
        /// </p>
        /// </remarks>
        /// <param name="caseSensitive">Flag specifying whether to make this object factory case sensitive or not.</param>
        /// <param name="parentFactory">The parent object factory, or <see langword="null"/> if none.</param>
        protected AbstractAutowireCapableObjectFactory(bool caseSensitive, IObjectFactory parentFactory)
            : base(caseSensitive, parentFactory)
        {
            this.IgnoreDependencyInterface(typeof(IObjectFactoryAware));
            this.IgnoreDependencyInterface(typeof(IObjectNameAware));
        }

        #endregion

        #region Properties

        /// <summary>
        /// The <see cref="Spring.Objects.Factory.Support.IInstantiationStrategy"/>
        /// implementation to be used to instantiate managed objects.
        /// </summary>
        protected IInstantiationStrategy InstantiationStrategy
        {
            get { return instantiationStrategy; }
            set { instantiationStrategy = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Predict the eventual object type (of the processed object instance) for the
        /// specified object.
        /// </summary>
        /// <param name="objectName">Name of the object.</param>
        /// <param name="mod">The merged object definition to determine the type for.</param>
        /// <returns>
        /// The type of the object, or <code>null</code> if not predictable
        /// </returns>
        protected override Type PredictObjectType(string objectName, RootObjectDefinition mod)
        {
            Type objectType;
            if (StringUtils.HasText(mod.FactoryMethodName))
            {
                objectType = GetTypeForFactoryMethod(objectName, mod);
            }
            else
            {
                objectType = ResolveObjectType(mod, objectName);
            }
            return objectType;
        }

        /// <summary>
        /// Determines the <see cref="System.Type"/> of the object defined
        /// by the supplied object <paramref name="definition"/>. 
        /// </summary>
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
        protected override Type GetTypeForFactoryMethod(string objectName, RootObjectDefinition definition)
        {
            if (StringUtils.HasText(definition.FactoryObjectName) && definition.IsSingleton && !definition.IsLazyInit)
            {
                return GetObject(objectName).GetType();
            }

            Type factoryType = null;
            bool isStatic = true;

            if (StringUtils.HasText(definition.FactoryObjectName))
            {
                // check declared factory method return type on factory type...
                factoryType = GetType(definition.FactoryObjectName);
                isStatic = false;
            }
            else
            {
                factoryType = ResolveObjectType(definition, objectName);
            }
            if (factoryType == null)
            {
                return null;
            }

            // If all factory methods have the same return type, return that type.
            // Can't clearly figure out exact method due to type converting / autowiring!
            int minNrOfArgs = definition.ConstructorArgumentValues.GenericArgumentValues.Count;
            MethodInfo[] candidates = factoryType.GetMethods();
            ISet returnTypes = new HybridSet();
            foreach (MethodInfo factoryMethod in candidates)
            {
#if NET_2_0
                GenericArgumentsHolder genericArgsInfo = new GenericArgumentsHolder(definition.FactoryMethodName);
                if (factoryMethod.IsStatic == isStatic && factoryMethod.Name.Equals(genericArgsInfo.GenericMethodName)
                    && ReflectionUtils.GetParameterTypes(factoryMethod).Length >= minNrOfArgs
                    && factoryMethod.GetGenericArguments().Length == genericArgsInfo.GetGenericArguments().Length)
                {
                    if (genericArgsInfo.ContainsGenericArguments)
                    {
                        string[] unresolvedGenericArgs = genericArgsInfo.GetGenericArguments();
                        Type[] genericArgs = new Type[unresolvedGenericArgs.Length];
                        for (int j = 0; j < unresolvedGenericArgs.Length; j++)
                        {
                            genericArgs[j] = TypeResolutionUtils.ResolveType(unresolvedGenericArgs[j]);
                        }
                        returnTypes.Add(factoryMethod.MakeGenericMethod(genericArgs).ReturnType);
                    }
                    else
                    {
                        returnTypes.Add(factoryMethod.ReturnType);
                    }
                }
#else
                if (factoryMethod.IsStatic == isStatic && factoryMethod.Name.Equals(definition.FactoryMethodName)
                    && ReflectionUtils.GetParameterTypes(factoryMethod).Length >= minNrOfArgs)
                {
                    returnTypes.Add(factoryMethod.ReturnType);
                }
#endif
            }
            if (returnTypes.Count == 1)
            {
                // clear return type found: all factory methods return same type...
                return (Type)ObjectUtils.EnumerateFirstElement(returnTypes);
            }
            else
            {
                // ambiguous return types found: return null to indicate "not determinable"...
                return null;
            }
        }

        /// <summary>
        /// Apply the property values of the object definition with the supplied
        /// <paramref name="name"/> to the supplied <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">
        /// The existing object that the property values for the named object will
        /// be applied to.
        /// </param>
        /// <param name="name">
        /// The name of the object definition associated with the property values that are
        /// to be applied.
        /// </param>
        public override void ApplyObjectPropertyValues(object instance, string name)
        {
            RootObjectDefinition definition = GetMergedObjectDefinition(name, true);
            if (definition != null)
            {
                log.Debug(string.Format("configuring object '{0}' using definition '{1}'", instance, name));
                ApplyPropertyValues(name, definition, new ObjectWrapper(instance), definition.PropertyValues);
            }
        }

        /// <summary>
        /// Apply any
        /// <see cref="Spring.Objects.Factory.Config.IInstantiationAwareObjectPostProcessor"/>s.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The returned instance may be a wrapper around the original.
        /// </p>
        /// </remarks>
        /// <param name="objectType">
        /// The <see cref="System.Type"/> of the object that is to be
        /// instantiated.
        /// </param>
        /// <param name="objectName">
        /// The name of the object that is to be instantiated.
        /// </param>
        /// <returns>
        /// An instance to use in place of the original instance.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        protected object ApplyObjectPostProcessorsBeforeInstantiation(Type objectType, string objectName)
        {
            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format("Invoking IInstantiationAwareObjectPostProcessors before " + "the instantiation of '{0}'.", objectName));
            }

            #endregion

            foreach (IObjectPostProcessor processor in ObjectPostProcessors)
            {
                IInstantiationAwareObjectPostProcessor inProc = processor as IInstantiationAwareObjectPostProcessor;
                if (inProc != null)
                {
                    object theObject = inProc.PostProcessBeforeInstantiation(objectType, objectName);
                    if (theObject != null)
                    {
                        return theObject;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Apply the given property values, resolving any runtime references
        /// to other objects in this object factory.
        /// </summary>
        /// <param name="name">
        /// The object name passed for better exception information.
        /// </param>
        /// <param name="definition">
        /// The definition of the named object.
        /// </param>
        /// <param name="wrapper">
        /// The <see cref="Spring.Objects.IObjectWrapper"/> wrapping the target object.
        /// </param>
        /// <param name="properties">
        /// The new property values.
        /// </param>
        /// <remarks>
        /// <p>
        /// Must use deep copy, so that we don't permanently modify this property.
        /// </p>
        /// </remarks>
        protected void ApplyPropertyValues(string name, RootObjectDefinition definition, IObjectWrapper wrapper, IPropertyValues properties)
        {
            if (properties == null || properties.PropertyValues.Length == 0)
            {
                return;
            }
            MutablePropertyValues deepCopy = new MutablePropertyValues(properties);
            PropertyValue[] copiedProperties = deepCopy.PropertyValues;
            for (int i = 0; i < copiedProperties.Length; ++i)
            {
                PropertyValue copiedProperty = copiedProperties[i];
                object value = ResolveValueIfNecessary(name, definition, copiedProperty.Name, copiedProperty.Value);
                PropertyValue propertyValue = new PropertyValue(copiedProperty.Name, value, copiedProperty.Expression);
                // update mutable copy...
                deepCopy.SetPropertyValueAt(propertyValue, i);
            }
            // set the (possibly resolved) deep copy properties...
            try
            {
                wrapper.SetPropertyValues(deepCopy);
            }
            catch (ObjectsException ex)
            {
                // improve the message by showing the context...
                throw new ObjectCreationException(definition.ResourceDescription, name, "Error setting property values: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Return an array of object-type property names that are unsatisfied.
        /// </summary>
        /// <remarks>
        /// <p>
        /// These are probably unsatisfied references to other objects in the
        /// factory. Does not include simple properties like primitives or
        /// <see cref="System.String"/>s.
        /// </p>
        /// </remarks>
        /// <returns>
        /// An array of object-type property names that are unsatisfied.
        /// </returns>
        /// <param name="definition">
        /// The definition of the named object.
        /// </param>
        /// <param name="wrapper">
        /// The <see cref="Spring.Objects.IObjectWrapper"/> wrapping the target object.
        /// </param>
        protected string[] UnsatisfiedObjectProperties(RootObjectDefinition definition, IObjectWrapper wrapper)
        {
            ArrayList result = new ArrayList();
            ISet ignoredTypes = IgnoredDependencyTypes;
            PropertyInfo[] properties = wrapper.GetPropertyInfos();
            foreach (PropertyInfo property in properties)
            {
                string name = property.Name;
                if (property.CanWrite && !ignoredTypes.Contains(property.PropertyType) && !result.Contains(name)
                    && !ObjectUtils.IsSimpleProperty(property.PropertyType))
                {
                    result.Add(name);
                }
            }
            return (string[])result.ToArray(typeof(string));
        }

        /// <summary>
        /// Destroy all cached singletons in this factory.
        /// </summary>
        /// <remarks>
        /// <p>
        /// To be called on shutdown of a factory.
        /// </p>
        /// </remarks>
        public override void Dispose()
        {
            base.Dispose();
            foreach (object o in _disposableInnerObjects)
            {
                DestroyObject(string.Format(CultureInfo.InvariantCulture, "(Inner object of Type '{0}')", o.GetType().FullName), o);
            }
            _disposableInnerObjects.Clear();
        }

        /// <summary>
        /// Populate the object instance in the given
        /// <see cref="Spring.Objects.IObjectWrapper"/> with the property values from the
        /// object definition.
        /// </summary>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <param name="definition">
        /// The definition of the named object.
        /// </param>
        /// <param name="wrapper">
        /// The <see cref="Spring.Objects.IObjectWrapper"/> wrapping the target object.
        /// </param>
        protected void PopulateObject(string name, RootObjectDefinition definition, IObjectWrapper wrapper)
        {
            // Give any InstantiationAwareBeanPostProcessors the opportunity to modify the
            // state of the bean before properties are set. This can be used, for example,
            // to support styles of field injection.
            bool continueWithPropertyPopulation = true;

            if (HasInstantiationAwareBeanPostProcessors)
            {
                foreach (IObjectPostProcessor processor in ObjectPostProcessors)
                {
                    IInstantiationAwareObjectPostProcessor inProc = processor as IInstantiationAwareObjectPostProcessor;
                    if (inProc != null)
                    {
                        if (!inProc.PostProcessAfterInstantiation(wrapper.WrappedInstance, name))
                        {
                            continueWithPropertyPopulation = false;
                            break;
                        }
                    }
                }
            }
            if (!continueWithPropertyPopulation)
            {
                return;
            }

            IPropertyValues properties = definition.PropertyValues;

            if (wrapper == null)
            {
                if (properties.PropertyValues.Length > 0)
                {
                    throw new ObjectCreationException(definition.ResourceDescription,
                        name, "Cannot apply property values to null instance.");
                }
                else
                {
                    // skip property population phase for null instance
                    return;
                }
            }

            if (definition.ResolvedAutowireMode == AutoWiringMode.ByName || definition.ResolvedAutowireMode == AutoWiringMode.ByType)
            {
                MutablePropertyValues mpvs = new MutablePropertyValues(properties);
                // add property values based on autowire by name if it's applied
                if (definition.ResolvedAutowireMode == AutoWiringMode.ByName)
                {
                    AutowireByName(name, definition, wrapper, mpvs);
                }
                // add property values based on autowire by type if it's applied
                if (definition.ResolvedAutowireMode == AutoWiringMode.ByType)
                {
                    AutowireByType(name, definition, wrapper, mpvs);
                }
                properties = mpvs;
            }
            //DependencyCheck(name, definition, wrapper, properties);


            bool hasInstAwareOpps = HasInstantiationAwareBeanPostProcessors;
            bool needsDepCheck = (definition.DependencyCheck != DependencyCheckingMode.None);


            if (hasInstAwareOpps || needsDepCheck)
            {
                PropertyInfo[] filteredPropInfo = FilterPropertyInfoForDependencyCheck(wrapper);
                if (hasInstAwareOpps)
                {
                    foreach (IObjectPostProcessor processor in ObjectPostProcessors)
                    {
                        IInstantiationAwareObjectPostProcessor instantiationAwareObjectPostProcessor =
                            processor as IInstantiationAwareObjectPostProcessor;
                        if (instantiationAwareObjectPostProcessor != null)
                        {
                            properties =
                                instantiationAwareObjectPostProcessor.PostProcessPropertyValues(properties, filteredPropInfo, wrapper.WrappedInstance,
                                                                 name);
                            if (properties == null)
                            {
                                return;
                            }
                        }
                    }
                }

                if (needsDepCheck)
                {
                    CheckDependencies(name, definition, filteredPropInfo, properties);
                }

            }

            ApplyPropertyValues(name, definition, wrapper, properties);
        }

        /// <summary>
        /// Wires up any exposed events in the object instance in the given
        /// <see cref="Spring.Objects.IObjectWrapper"/> with any event handler
        /// values from the <paramref name="definition"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <param name="definition">
        /// The definition of the named object.
        /// </param>
        /// <param name="wrapper">
        /// The <see cref="Spring.Objects.IObjectWrapper"/> wrapping the target object.
        /// </param>
        protected void WireEvents(string name, IConfigurableObjectDefinition definition, IObjectWrapper wrapper)
        {
            foreach (string eventName in definition.EventHandlerValues.Events)
            {
                foreach (IEventHandlerValue handlerValue
                        in definition.EventHandlerValues[eventName])
                {
                    object handler = null;
                    if (handlerValue.Source is RuntimeObjectReference)
                    {
                        RuntimeObjectReference roref = (RuntimeObjectReference)handlerValue.Source;
                        handler = ResolveReference(definition, name, eventName, roref);
                    }
                    else if (handlerValue.Source is Type)
                    {
                        // a static Type event is being wired up; simply pass on the Type
                        handler = handlerValue.Source;
                    }
                    else if (handlerValue.Source is string)
                    {
                        // a static Type event is being wired up; we need to resolve the Type
                        handler = TypeResolutionUtils.ResolveType(handlerValue.Source as string);
                    }
                    else
                    {
                        throw new FatalObjectException("Currently, only references to other objects and Types are " + "supported as event sources.");
                    }
                    handlerValue.Wire(handler, wrapper.WrappedInstance);
                }
            }
        }

        /// <summary>
        /// Fills in any missing property values with references to
        /// other objects in this factory if autowire is set to
        /// <see cref="Spring.Objects.Factory.Config.AutoWiringMode.ByName"/>.
        /// </summary>
        /// <param name="name">
        /// The object name to be autowired by <see cref="System.Type"/>.
        /// </param>
        /// <param name="definition">
        /// The definition of the named object to update through autowiring.
        /// </param>
        /// <param name="wrapper">
        /// The <see cref="Spring.Objects.IObjectWrapper"/> wrapping the target object (and
        /// from which we can rip out information concerning the object).
        /// </param>
        /// <param name="properties">
        /// The property values to register wired objects with.
        /// </param>
        protected void AutowireByName(string name, RootObjectDefinition definition, IObjectWrapper wrapper, MutablePropertyValues properties)
        {
            string[] propertyNames = UnsatisfiedObjectProperties(definition, wrapper);
            foreach (string propertyName in propertyNames)
            {
                // look for a matching type
                if (ContainsObject(propertyName))
                {
                    object o = GetObject(propertyName);
                    properties.Add(propertyName, o);

                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                                string.Format(CultureInfo.InvariantCulture,
                                              "Added autowiring by name from object name '{0}' via " + "property '{1}' to object named '{1}'.", name,
                                              propertyName));
                    }

                    #endregion
                }
                else
                {
                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                                string.Format(CultureInfo.InvariantCulture,
                                              "Not autowiring property '{0}' of object '{1}' by name: " + "no matching object found.", propertyName, name));
                    }

                    #endregion
                }
            }
        }

        /// <summary>
        /// Defines "autowire by type" (object properties by type) behavior.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is like PicoContainer default, in which there must be exactly one object
        /// of the property type in the object factory. This makes object factories simple
        /// to configure for small namespaces, but doesn't work as well as standard Spring
        /// behavior for bigger applications.
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The object name to be autowired by <see cref="System.Type"/>.
        /// </param>
        /// <param name="definition">
        /// The definition of the named object to update through autowiring.
        /// </param>
        /// <param name="wrapper">
        /// The <see cref="Spring.Objects.IObjectWrapper"/> wrapping the target object (and
        /// from which we can rip out information concerning the object).
        /// </param>
        /// <param name="properties">
        /// The property values to register wired objects with.
        /// </param>
        protected void AutowireByType(string name, RootObjectDefinition definition, IObjectWrapper wrapper, MutablePropertyValues properties)
        {
            string[] propertyNames = UnsatisfiedObjectProperties(definition, wrapper);
            foreach (string propertyName in propertyNames)
            {
                // look for a matching type
                Type requiredType = wrapper.GetPropertyType(propertyName);
                IDictionary matchingObjects = FindMatchingObjects(requiredType);
                if (matchingObjects != null && matchingObjects.Count == 1)
                {
                    properties.Add(propertyName, ObjectUtils.EnumerateFirstElement(matchingObjects.Values));

                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                                string.Format(CultureInfo.InvariantCulture,
                                              "Autowiring by type from object name '{0}' via property " + "'{1}' to object named '{2}'.", name,
                                              propertyName, ObjectUtils.EnumerateFirstElement(matchingObjects.Keys)));
                    }

                    #endregion
                }
                else if (matchingObjects != null && matchingObjects.Count > 1)
                {
                    throw new UnsatisfiedDependencyException(string.Empty, name, propertyName,
                                                             string.Format(CultureInfo.InvariantCulture,
                                                                           "There are {0} objects of Type [{1}] for autowire by "
                                                                           + "type, when there should have been just 1 to be able to "
                                                                           + "autowire property '{2}' of object '{3}'.", matchingObjects.Count,
                                                                           requiredType, propertyName, name));
                }
                else
                {
                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                                string.Format(CultureInfo.InvariantCulture, "Not autowiring property '{0}' of object '{1}': no matching object found.",
                                              propertyName, name));
                    }

                    #endregion
                }
            }
        }

        /// <summary>
        /// Ignore the given dependency type for autowiring
        /// </summary>
        /// <remarks>
        /// This will typically be used by application contexts to register
        /// dependencies that are resolved in other ways, like IOjbectFactory through
        /// IObjectFactoryAware or IApplicationContext through IApplicationContextAware.
        /// By default, IObjectFactoryAware and IObjectName interfaces are ignored.
        /// For further types to ignore, invoke this method for each type.
        /// </remarks>
        /// <seealso cref="Spring.Objects.Factory.Config.IConfigurableObjectFactory.IgnoreDependencyType"/>.
        public void IgnoreDependencyInterface(Type type)
        {
            ignoredDependencyInterfaces.Add(type);
        }

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
        /// <returns>
        /// A new instance of the object.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        /// <remarks>
        /// <p>
        /// Delegates to the
        /// <see cref="Spring.Objects.Factory.Support.AbstractAutowireCapableObjectFactory.CreateObject (string,RootObjectDefinition,object[],bool)"/>
        /// method version with the <c>allowEagerCaching</c> parameter set to <b>true</b>.
        /// </p>
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
        protected override object CreateObject(string name, RootObjectDefinition definition, object[] arguments)
        {
            return CreateObject(name, definition, arguments, true);
        }

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
        protected virtual object CreateObject(string name, RootObjectDefinition definition, object[] arguments, bool allowEagerCaching)
        {
            // guarantee the initialization of objects that the current one depends on..
            if (definition.DependsOn != null && definition.DependsOn.Length > 0)
            {
                foreach (string dependant in definition.DependsOn)
                {
                    GetObject(dependant);
                }
            }

            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format(CultureInfo.InvariantCulture, "Creating instance of Object '{0}' with merged definition [{1}].", name, definition));
            }

            #endregion

            // Make sure object type is actually resolved at this point.
            ResolveObjectType(definition, name);

            try
            {
                definition.PrepareMethodOverrides();
            }
            catch (ObjectDefinitionValidationException ex)
            {
                throw new ObjectDefinitionStoreException(definition.ResourceDescription, name,
                                                         "Validation of method overrides failed.  " + ex.Message, ex);
            }

            // return IObjectDefinition instance itself for an abstract object-definition
            if (definition.IsTemplate)
            {
                return definition;
            }



            object instance = null;


            IObjectWrapper instanceWrapper = null;
            bool eagerlyCached = false;
            try
            {
                // Give IInstantiationAwareObjectPostProcessors a chance to return a proxy instead of the target instance....
                if (definition.HasObjectType)
                {
                    instance = ApplyObjectPostProcessorsBeforeInstantiation(definition.ObjectType, name);
                    if (instance != null)
                    {
                        return instance;
                    }
                }


                instanceWrapper = CreateObjectInstance(name, definition, arguments);
                instance = instanceWrapper.WrappedInstance;

                // eagerly cache singletons to be able to resolve circular references
                // even when triggered by lifecycle interfaces like IObjectFactoryAware.
                if (allowEagerCaching && definition.IsSingleton)
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Eagerly caching object '" + name + "' to allow for resolving potential circular references");
                    }
                    AddEagerlyCachedSingleton(name, definition, instance);
                    eagerlyCached = true;
                }

                instance = ConfigureObject(name, definition, instanceWrapper);
            }
            catch (ObjectCreationException)
            {
                if (eagerlyCached)
                {
                    RemoveEagerlyCachedSingleton(name, definition);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (eagerlyCached)
                {
                    RemoveEagerlyCachedSingleton(name, definition);
                }
                throw new ObjectCreationException(definition.ResourceDescription, name, "Initialization of object failed : " + ex.Message, ex);
            }
            return instance;
        }

        /// <summary>
        /// Add the created, but yet unpopulated singleton to the singleton cache
        /// to be able to resolve circular references
        /// </summary>
        /// <param name="objectName">the name of the object to add to the cache.</param>
        /// <param name="objectDefinition">the definition used to create and populated the object.</param>
        /// <param name="rawSingletonInstance">the raw object instance.</param>
        /// <remarks>
        /// Derived classes may override this method to select the right cache based on the object definition.
        /// </remarks>
        protected virtual void AddEagerlyCachedSingleton(string objectName, IObjectDefinition objectDefinition, object rawSingletonInstance)
        {
            base.AddSingleton(objectName, rawSingletonInstance);
        }

        /// <summary>
        /// Remove the specified singleton from the singleton cache that has 
        /// been added before by a call to <see cref="AddEagerlyCachedSingleton"/>
        /// </summary>
        /// <param name="objectName">the name of the object to remove from the cache.</param>
        /// <param name="objectDefinition">the definition used to create and populated the object.</param>
        /// <remarks>
        /// Derived classes may override this method to select the right cache based on the object definition.
        /// </remarks>
        protected virtual void RemoveEagerlyCachedSingleton(string objectName, IObjectDefinition objectDefinition)
        {
            base.RemoveSingleton(objectName);
        }

        /// <summary>
        /// Creates an <see cref="IObjectWrapper"/> instance from the <see cref="RootObjectDefinition"/> passed in <paramref name="definition"/>
        /// using constructor <paramref name="arguments"/>
        /// </summary>
        /// <param name="name">The name of the object to create - used for error messages.</param>
        /// <param name="definition">The <see cref="RootObjectDefinition"/> describing the object to be created.</param>
        /// <param name="arguments">optional arguments to pass to the constructor</param>
        /// <returns>An <see cref="IObjectWrapper"/> wrapping the already instantiated object</returns>
        protected IObjectWrapper CreateObjectInstance(string name, RootObjectDefinition definition, object[] arguments)
        {
            IObjectWrapper instanceWrapper;
            if (StringUtils.HasText(definition.FactoryMethodName))
            {
                instanceWrapper = InstantiateUsingFactoryMethod(name, definition, arguments);
            }
            //Handle case when arguments are passed in explicitly.
            else if (arguments != null && arguments.Length > 0)
            {
                instanceWrapper = AutowireConstructor(name, definition, arguments);
            }
            else if (definition.ResolvedAutowireMode == AutoWiringMode.Constructor ||
                     definition.HasConstructorArgumentValues)
            {
                instanceWrapper = AutowireConstructor(name, definition);
            }
            else
            {
                instanceWrapper = new ObjectWrapper(InstantiationStrategy.Instantiate(definition, name, this));
                InitObjectWrapper(instanceWrapper);
            }
            return instanceWrapper;
        }

        /// <summary>
        /// Instantiate an object instance using a named factory method.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The method may be static, if the <paramref name="definition"/>
        /// parameter specifies a class, rather than a
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> instance, or an
        /// instance variable on a factory object itself configured using Dependency
        /// Injection.
        /// </p>
        /// <p>
        /// Implementation requires iterating over the static or instance methods
        /// with the name specified in the supplied <paramref name="definition"/>
        /// (the method may be overloaded) and trying to match with the parameters.
        /// We don't have the types attached to constructor args, so trial and error
        /// is the only way to go here.
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The name associated with the supplied <paramref name="definition"/>.
        /// </param>
        /// <param name="definition">
        /// The definition describing the instance that is to be instantiated.
        /// </param>
        /// <param name="arguments">
        /// Any arguments to the factory method that is to be invoked.
        /// </param>
        /// <returns>
        /// The result of the factory method invocation (the instance).
        /// </returns>
        protected virtual IObjectWrapper InstantiateUsingFactoryMethod(string name, RootObjectDefinition definition, object[] arguments)
        {
            ConstructorArgumentValues cargs = definition.ConstructorArgumentValues;
            ConstructorArgumentValues resolvedValues = new ConstructorArgumentValues();
            int expectedArgCount = 0;

            // we don't have arguments passed in programmatically, so we need to resolve the
            // arguments specified in the constructor arguments held in the object definition...
            if (arguments == null || arguments.Length == 0)
            {
                expectedArgCount = cargs.ArgumentCount;
                ResolveConstructorArguments(name, definition, resolvedValues);
            }
            else
            {
                // if we have constructor args, don't need to resolve them...
                expectedArgCount = arguments.Length;
            }
            ObjectWrapper wrapper = new ObjectWrapper();
            InitObjectWrapper(wrapper);
            bool isStatic = true;
            Type factoryClass = null;
            if (StringUtils.HasText(definition.FactoryObjectName))
            {
                // it's an instance method on the factory object's class...
                factoryClass = GetObject(definition.FactoryObjectName).GetType();
                isStatic = false;
            }
            else
            {
                // it's a static factory method on the object class...
                factoryClass = definition.ObjectType;
            }

#if NET_2_0
            GenericArgumentsHolder genericArgsInfo = new GenericArgumentsHolder(definition.FactoryMethodName);

            MethodInfo[] factoryMethods = FindMethods(genericArgsInfo.GenericMethodName, expectedArgCount, isStatic, factoryClass);
            UnsatisfiedDependencyExceptionData unsatisfiedDependencyExceptionData = null;
            // try all matching methods to see if they match the constructor arguments...
            for (int i = 0; i < factoryMethods.Length; i++)
            {
                unsatisfiedDependencyExceptionData = null;
                MethodInfo factoryMethod = factoryMethods[i];

                if (genericArgsInfo.ContainsGenericArguments)
                {
                    string[] unresolvedGenericArgs = genericArgsInfo.GetGenericArguments();
                    if (factoryMethod.GetGenericArguments().Length != unresolvedGenericArgs.Length)
                        continue;

                    Type[] genericArgs = new Type[unresolvedGenericArgs.Length];
                    for (int j = 0; j < unresolvedGenericArgs.Length; j++)
                    {
                        genericArgs[j] = TypeResolutionUtils.ResolveType(unresolvedGenericArgs[j]);
                    }
                    factoryMethod = factoryMethod.MakeGenericMethod(genericArgs);
                }
#else
            MethodInfo[] factoryMethods = FindMethods(definition.FactoryMethodName, expectedArgCount, isStatic, factoryClass);
            UnsatisfiedDependencyExceptionData unsatisfiedDependencyExceptionData = null;            
            // try all matching methods to see if they match the constructor arguments...
            foreach(MethodInfo factoryMethod in factoryMethods)
            {
#endif
                if (arguments == null || arguments.Length == 0)
                {
                    // try to create the required arguments...
                    arguments = CreateArgumentArray(name, definition, resolvedValues, factoryMethod, out unsatisfiedDependencyExceptionData);
                    if (arguments == null)
                    {
                        // if we failed to match this method, keep
                        // trying new overloaded factory methods...
                        continue;
                    }
                }
                // if we get here, we found a factory method...

                if (ReflectionUtils.GetMethodByArgumentValues(new MethodInfo[] { factoryMethod }, arguments) == null)
                {
                    continue;
                }


                object objectInstance = InstantiationStrategy.Instantiate(definition, name, this, factoryMethod, arguments);
                wrapper.WrappedInstance = objectInstance;

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format(CultureInfo.InvariantCulture, "Object '{0}' instantiated via factory method [{1}].", name, factoryMethod));
                }

                #endregion

                return wrapper;
            }



            // if we get here, we didn't match any method...
            throw new ObjectDefinitionStoreException(
                    string.Format(CultureInfo.InvariantCulture, "Cannot find matching factory method '{0} on Type [{1}].", definition.FactoryMethodName,
                                  factoryClass));
        }

        /// <summary>
        /// Returns an array of all of those
        /// <see cref="System.Reflection.MethodInfo">methods</see> exposed on the
        /// <paramref name="searchType"/> that match the supplied criteria.
        /// </summary>
        /// <param name="methodName">
        /// Methods that have this name (can be in the form of a regular expression).
        /// </param>
        /// <param name="expectedArgumentCount">
        /// Methods that have exactly this many arguments.
        /// </param>
        /// <param name="isStatic">
        /// Methods that are static / instance.
        /// </param>
        /// <param name="searchType">
        /// The <see cref="System.Type"/> on which the methods (if any) are to be found.
        /// </param>
        /// <returns>
        /// An array of all of those
        /// <see cref="System.Reflection.MethodInfo">methods</see> exposed on the
        /// <paramref name="searchType"/> that match the supplied criteria.
        /// </returns>
        private static MethodInfo[] FindMethods(string methodName, int expectedArgumentCount, bool isStatic, Type searchType)
        {
            ComposedCriteria methodCriteria = new ComposedCriteria();
            methodCriteria.Add(new MethodNameMatchCriteria(methodName));
            methodCriteria.Add(new MethodParametersCountCriteria(expectedArgumentCount));
            BindingFlags methodFlags = BindingFlags.Public | BindingFlags.IgnoreCase | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            MemberInfo[] methods =
                    searchType.FindMembers(MemberTypes.Method, methodFlags, new MemberFilter(new CriteriaMemberFilter().FilterMemberByCriteria),
                                           methodCriteria);
            return (MethodInfo[])ArrayList.Adapter(methods).ToArray(typeof(MethodInfo));
        }

        /// <summary>
        /// Create an array of arguments to invoke a constructor or static factory method,
        /// given the resolved constructor arguments values.
        /// </summary>
        /// <remarks>When return value is null the out parameter UnsatisfiedDependencyExceptionData will contain
        /// information for use in throwing a UnsatisfiedDependencyException by the caller.  This avoids using
        /// exceptions for flow control as in the original implementation.</remarks>
        private object[] CreateArgumentArray(string name, RootObjectDefinition definition,
            ConstructorArgumentValues resolvedValues, MethodBase methodOrCtor, out UnsatisfiedDependencyExceptionData unsatisfiedDependencyExceptionData)
        {
            string methodType = (methodOrCtor is ConstructorInfo) ? "constructor" : "factory method";
            unsatisfiedDependencyExceptionData = null;
            ParameterInfo[] argTypes = methodOrCtor.GetParameters();
            object[] args = new object[argTypes.Length];
            ISet alreadyUsedValues = new HybridSet();
            for (int j = 0; j < argTypes.Length; ++j)
            {
                Type parameterType = argTypes[j].ParameterType;
                string parameterName = argTypes[j].Name;
                ConstructorArgumentValues.ValueHolder valueHolder = null;
                if (resolvedValues.GetNamedArgumentValue(parameterName) != null)
                {
                    valueHolder = resolvedValues.GetArgumentValue(parameterName, parameterType, alreadyUsedValues);
                }
                else
                {
                    valueHolder = resolvedValues.GetArgumentValue(j, parameterType, alreadyUsedValues);
                }
                if (valueHolder != null)
                {
                    try
                    {
                        args[j] = TypeConversionUtils.ConvertValueIfNecessary(parameterType, valueHolder.Value, null);
                        alreadyUsedValues.Add(valueHolder);
                    }
                    catch (TypeMismatchException ex)
                    {
                        string errorMessage = String.Format(CultureInfo.InvariantCulture,
                                                           "Could not convert {0} argument value [{1}] to required type [{2}] : {3}",
                                                           methodType, valueHolder.Value,
                                                           parameterType, ex.Message);
                        unsatisfiedDependencyExceptionData = new UnsatisfiedDependencyExceptionData(j, parameterType, errorMessage);


                        return null;
                    }
                }
                else
                {
                    if (definition.ResolvedAutowireMode != AutoWiringMode.Constructor)
                    {
                        string errorMessage = String.Format(CultureInfo.InvariantCulture,
                                                          "Ambiguous {0} argument types - " +
                                                          "Did you specify the correct object references as {0} arguments?",
                                                          methodType);
                        unsatisfiedDependencyExceptionData = new UnsatisfiedDependencyExceptionData(j, parameterType, errorMessage);

                        return null;
                    }
                    IDictionary matchingObjects = FindMatchingObjects(parameterType);
                    if (matchingObjects == null || matchingObjects.Count != 1)
                    {
                        string errorMessage = String.Format(CultureInfo.InvariantCulture,
                                                           "There are '{0}' objects of type [{1}] for autowiring "
                                                           +
                                                           "{2}. There should have been exactly 1 to be able to "
                                                           +
                                                           "autowire the '{3}' argument on the {2} of object '{4}'.",
                                                           (matchingObjects == null
                                                                ? 0
                                                                : matchingObjects.Count),
                                                           parameterType, methodType,
                                                           parameterName, name);
                        unsatisfiedDependencyExceptionData = new UnsatisfiedDependencyExceptionData(j, parameterType, errorMessage);

                        return null;
                    }
                    DictionaryEntry entry = (DictionaryEntry)ObjectUtils.EnumerateFirstElement(matchingObjects);
                    args[j] = entry.Value;

                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                                string.Format(CultureInfo.InvariantCulture,
                                              "Autowiring '{0}' argument by type from object name '{1}' via {2} to "
                                              + "object named '{3}'.", parameterName, name, methodType, entry.Key));
                    }

                    #endregion
                }
            }

            return args;
        }

        /// <summary>
        /// Explicitly construct the object using the supplied constructor arguments.
        /// Constructor arguments are matched by type.
        /// </summary>
        /// <param name="name">
        /// The name of the object to autowire by type.
        /// </param>
        /// <param name="definition">
        /// The object definition to update through autowiring.
        /// </param>
        /// <param name="args">Array of constructor argument values.</param>
        /// <returns>
        /// An <see cref="Spring.Objects.IObjectWrapper"/> for the new instance.
        /// </returns>
        protected IObjectWrapper AutowireConstructor(string name, RootObjectDefinition definition, object[] args)
        {
            ConstructorArgumentValues resolvedValues = new ConstructorArgumentValues();
            for (int i = 0; i < args.Length; i++)
            {
                //This is assigning ctor arguments by type.
                resolvedValues.AddGenericArgumentValue(args[i]);
            }
            return AutowireConstructor(name, definition, resolvedValues);
        }

        /// <summary>
        /// "autowire constructor" (with constructor arguments by type) behaviour.
        /// </summary>
        /// <remarks>Passes an empty collection of constructor argument values
        /// to overloaded method.
        /// </remarks>
        /// <param name="name">
        /// The name of the object to autowire by type.
        /// </param>
        /// <param name="definition">
        /// The object definition to update through autowiring.
        /// </param>
        /// <returns>
        /// An <see cref="Spring.Objects.IObjectWrapper"/> for the new instance.
        /// </returns>
        protected IObjectWrapper AutowireConstructor(string name, RootObjectDefinition definition)
        {
            return AutowireConstructor(name, definition, new ConstructorArgumentValues());
        }

        /// <summary>
        /// "autowire constructor" (with constructor arguments by type) behaviour.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Also applied if explicit constructor argument values are specified,
        /// matching all remaining arguments with objects from the object factory.
        /// </p>
        /// <p>
        /// This corresponds to constructor injection: in this mode, a Spring.NET
        /// object factory is able to host components that expect constructor-based
        /// dependency resolution.
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The name of the object to autowire by type.
        /// </param>
        /// <param name="definition">
        /// The object definition to update through autowiring.
        /// </param>
        /// <param name="argumentValues">
        /// The collection on constructor argument values.
        /// </param>
        /// <returns>
        /// An <see cref="Spring.Objects.IObjectWrapper"/> for the new instance.
        /// </returns>
        protected IObjectWrapper AutowireConstructor(string name, RootObjectDefinition definition, ConstructorArgumentValues argumentValues)
        {
            int minNrOfArgs = ResolveConstructorArguments(name, definition, argumentValues);
            ConstructorInfo[] constructors = AutowireUtils.GetConstructors(definition, minNrOfArgs);
            if (constructors == null || constructors.Length == 0)
            {
                throw new ObjectCreationException(definition.ResourceDescription, name,
                                                  string.Format(CultureInfo.InvariantCulture,
                                                                "'{0}' constructor arguments specified but no matching constructor found "
                                                                + "in object '{1}' (hint: specify argument indexes, names, or "
                                                                + "types to avoid ambiguities).", minNrOfArgs, name));
            }
            ObjectWrapper wrapper = new ObjectWrapper();
            InitObjectWrapper(wrapper);
            ConstructorInfo constructorToUse = null;
            object[] argsToUse = null;
            int weighting = Int32.MaxValue;
            UnsatisfiedDependencyExceptionData unsatisfiedDependencyExceptionData = null;
            for (int i = 0; i < constructors.Length; ++i)
            {
                unsatisfiedDependencyExceptionData = null;
                ConstructorInfo constructor = constructors[i];
                if (constructorToUse != null &&
                    constructorToUse.GetParameters().Length > constructor.GetParameters().Length)
                {
                    // already found greedy constructor that can be satisfied, so
                    // don't look any further, there are only less greedy constructors left...
                    break;
                }

                object[] args = CreateArgumentArray(name, definition, argumentValues, constructor, out unsatisfiedDependencyExceptionData);
                if (args == null)
                {
                    if (i == constructors.Length - 1 && constructorToUse == null)
                    {
                        throw new UnsatisfiedDependencyException(definition.ResourceDescription,
                                                                    name,
                                                                    unsatisfiedDependencyExceptionData.ParameterIndex,
                                                                    unsatisfiedDependencyExceptionData.ParameterType,
                                                                    unsatisfiedDependencyExceptionData.ErrorMessage);
                    }
                    // try next constructor...
                    continue;
                }

                int typeDiffWeight = AutowireUtils.GetTypeDifferenceWeight(constructor.GetParameters(), args);
                if (typeDiffWeight < weighting)
                {
                    constructorToUse = constructor;
                    argsToUse = args;
                    weighting = typeDiffWeight;
                }
            }

            if (constructorToUse == null)
            {
                throw new ObjectCreationException(definition.ResourceDescription, name, "Could not resolve matching constructor.");
            }
            wrapper.WrappedInstance = InstantiationStrategy.Instantiate(definition, name, this, constructorToUse, argsToUse);

            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format(CultureInfo.InvariantCulture, "Object '{0}' instantiated via constructor [{1}].", name, constructorToUse));
            }

            #endregion

            return wrapper;
        }

        /// <summary>
        /// Resolves the <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
        /// of the supplied <paramref name="definition"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// 'Resolve' can be taken to mean that all of the <paramref name="definition"/>s
        /// constructor arguments is resolved into a concrete object that can be plugged
        /// into one of the <paramref name="definition"/>s constructors. Runtime object
        /// references to other objects in this (or a parent) factory are resolved,
        /// type conversion is performed, etc.
        /// </p>
        /// <p>
        /// These resolved values are plugged into the supplied
        /// <paramref name="resolvedValues"/> object, because we wouldn't want to touch
        /// the <paramref name="definition"/>s constructor arguments in case it (or any of
        /// its constructor arguments) is a prototype object definition.
        /// </p>
        /// <p>
        /// This method is also used for handling invocations of static factory methods.
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The name of the object that is being resolved by this factory.
        /// </param>
        /// <param name="definition">
        /// The definition associated with the above <paramref name="name"/>.
        /// </param>
        /// <param name="resolvedValues">
        /// Where the resolved constructor arguments will be placed.
        /// </param>
        /// <returns>
        /// The minimum number of arguments that any constructor for the supplied
        /// <paramref name="definition"/> must have.
        /// </returns>
        private int ResolveConstructorArguments(string name, RootObjectDefinition definition, ConstructorArgumentValues resolvedValues)
        {
            int minNrOfArgs = 0;
            if (definition.ConstructorArgumentValues != null)
            {
                minNrOfArgs = definition.ConstructorArgumentValues.ArgumentCount;
                foreach (DictionaryEntry de in definition.ConstructorArgumentValues.IndexedArgumentValues)
                {
                    int index = (int)de.Key;
                    if (index < 0)
                    {
                        throw new ObjectCreationException(definition.ResourceDescription, name, "Invalid constructor argument index: " + index);
                    }
                    if (index > minNrOfArgs)
                    {
                        minNrOfArgs = index + 1;
                    }
                    string argName = "constructor argument with index " + index;
                    ConstructorArgumentValues.ValueHolder valueHolder = (ConstructorArgumentValues.ValueHolder)de.Value;
                    object resolvedValue = ResolveValueIfNecessary(name, definition, argName, valueHolder.Value);
                    resolvedValues.AddIndexedArgumentValue(index, resolvedValue,
                        StringUtils.HasText(valueHolder.Type)
                            ? TypeResolutionUtils.ResolveType(valueHolder.Type).AssemblyQualifiedName
                            : null);
                }
                foreach (ConstructorArgumentValues.ValueHolder valueHolder in definition.ConstructorArgumentValues.GenericArgumentValues)
                {
                    string argName = "constructor argument";
                    object resolvedValue = ResolveValueIfNecessary(name, definition, argName, valueHolder.Value);
                    resolvedValues.AddGenericArgumentValue(resolvedValue,
                        StringUtils.HasText(valueHolder.Type)
                            ? TypeResolutionUtils.ResolveType(valueHolder.Type).AssemblyQualifiedName
                            : null);
                }
                foreach (DictionaryEntry namedArgumentEntry in definition.ConstructorArgumentValues.NamedArgumentValues)
                {
                    string argumentName = (string)namedArgumentEntry.Key;
                    string syntheticArgumentName = "constructor argument with name " + argumentName;
                    ConstructorArgumentValues.ValueHolder valueHolder = (ConstructorArgumentValues.ValueHolder)namedArgumentEntry.Value;
                    object resolvedValue = ResolveValueIfNecessary(name, definition, syntheticArgumentName, valueHolder.Value);
                    resolvedValues.AddNamedArgumentValue(argumentName, resolvedValue);
                }
            }
            return minNrOfArgs;
        }

        /// <summary>
        /// Perform a dependency check that all properties exposed have been set, if desired.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Dependency checks can be objects (collaborating objects), simple (primitives
        /// and <see cref="System.String"/>), or all (both).
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <param name="definition">
        /// The definition of the named object.
        /// </param>
        /// <param name="wrapper">
        /// The <see cref="Spring.Objects.IObjectWrapper"/> wrapping the target object.
        /// </param>
        /// <param name="properties">
        /// The property values to be checked.
        /// </param>
        /// <exception cref="Spring.Objects.Factory.UnsatisfiedDependencyException">
        /// If all of the checked dependencies were not satisfied.
        /// </exception>
        protected void DependencyCheck(string name, IConfigurableObjectDefinition definition, IObjectWrapper wrapper, IPropertyValues properties)
        {
            DependencyCheckingMode dependencyCheck = definition.DependencyCheck;
            if (dependencyCheck == DependencyCheckingMode.None)
            {
                return;
            }

            PropertyInfo[] filteredPropInfo = FilterPropertyInfoForDependencyCheck(wrapper);
            if (HasInstantiationAwareBeanPostProcessors)
            {
                foreach (IObjectPostProcessor processor in ObjectPostProcessors)
                {
                    IInstantiationAwareObjectPostProcessor inProc = processor as IInstantiationAwareObjectPostProcessor;
                    if (inProc != null)
                    {
                        properties =
                            inProc.PostProcessPropertyValues(properties, filteredPropInfo, wrapper.WrappedInstance, name);
                        if (properties == null)
                        {
                            return;
                        }
                    }
                }
            }


            CheckDependencies(name, definition, filteredPropInfo, properties);
        }

        private static void CheckDependencies(string name, IConfigurableObjectDefinition definition, PropertyInfo[] filteredPropInfo, IPropertyValues properties)
        {
            DependencyCheckingMode dependencyCheck = definition.DependencyCheck;
            foreach (PropertyInfo property in filteredPropInfo)
            {
                if (property.CanWrite && properties.GetPropertyValue(property.Name) == null)
                {
                    bool isSimple = ObjectUtils.IsSimpleProperty(property.PropertyType);
                    bool unsatisfied = (dependencyCheck == DependencyCheckingMode.All) || (isSimple && dependencyCheck == DependencyCheckingMode.Simple)
                                       || (!isSimple && dependencyCheck == DependencyCheckingMode.Objects);
                    if (unsatisfied)
                    {
                        throw new UnsatisfiedDependencyException(definition.ResourceDescription, name, property.Name,
                            "Set this property value or disable dependency checking for this object.");
                    }
                }
            }
        }

        /// <summary>
        /// Extract a filtered set of PropertyInfos from the given IObjectWrapper, excluding
        /// ignored dependency types.
        /// </summary>
        /// <param name="wrapper">The object wrapper the object was created with.</param>
        /// <returns>The filtered PropertyInfos</returns>
        private PropertyInfo[] FilterPropertyInfoForDependencyCheck(IObjectWrapper wrapper)
        {
            lock (filteredPropertyDescriptorsCache)
            {
                PropertyInfo[] filtered = (PropertyInfo[])filteredPropertyDescriptorsCache[wrapper.WrappedType];
                if (filtered == null)
                {

                    ArrayList list = new ArrayList(wrapper.GetPropertyInfos());
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        PropertyInfo pi = (PropertyInfo)list[i];
                        if (IsExcludedFromDependencyCheck(pi))
                        {
                            list.RemoveAt(i);
                        }
                    }

                    filtered = (PropertyInfo[])list.ToArray(typeof(PropertyInfo));
                    filteredPropertyDescriptorsCache.Add(wrapper.WrappedType, filtered);
                }
                return filtered;
            }

        }

        private bool IsExcludedFromDependencyCheck(PropertyInfo pi)
        {
            bool b1 = !pi.CanWrite; //AutowireUtils.IsExcludedFromDependencyCheck(pi);
            bool b2 = IgnoredDependencyTypes.Contains(pi.PropertyType);
            bool b3 = AutowireUtils.IsSetterDefinedInInterface(pi, ignoredDependencyInterfaces);
            return b1 || b2 || b3;
            /*
            return AutowireUtils.IsExcludedFromDependencyCheck(pi) ||
                   IgnoredDependencyTypes.Contains(pi.PropertyType) ||
                   AutowireUtils.IsSetterDefinedInInterface(pi, ignoredDependencyInterfaces);
             */
        }

        /// <summary>
        /// Give an object a chance to react now all its properties are set,
        /// and a chance to know about its owning object factory (this object).
        /// </summary>
        /// <remarks>
        /// <p>
        /// This means checking whether the object implements
        /// <see cref="Spring.Objects.Factory.IInitializingObject"/> and  / or
        /// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>, and invoking the
        /// necessary callback(s) if it does.
        /// </p>
        /// <p>
        /// Custom init methods are resolved in a <b>case-insensitive</b> manner.
        /// </p>
        /// </remarks>
        /// <param name="target">
        /// The new object instance we may need to initialise.
        /// </param>
        /// <param name="name">
        /// The name the object has in the factory. Used for logging output.
        /// </param>
        /// <param name="definition">
        /// The definition of the target object instance.
        /// </param>
        protected virtual void InvokeInitMethods(object target, string name, IConfigurableObjectDefinition definition)
        {
            if (ObjectUtils.IsAssignableAndNotTransparentProxy(typeof(IInitializingObject), target))
            {
                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format(CultureInfo.InvariantCulture, "Calling AfterPropertiesSet() on object with name '{0}'.", name));
                }

                #endregion

                ((IInitializingObject)target).AfterPropertiesSet();
            }
            if (StringUtils.HasText(definition.InitMethodName))
            {
                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(
                            string.Format(CultureInfo.InvariantCulture, "Calling custom init method '{0} on object with name '{1}'.",
                                          definition.InitMethodName, name));
                }

                #endregion

                try
                {
                    MethodInfo targetMethod = target.GetType().GetMethod(definition.InitMethodName, MethodResolutionFlags, null, Type.EmptyTypes, null);
                    if (targetMethod == null)
                    {
                        throw new ObjectCreationException(definition.ResourceDescription, name,
                                                          "Could not find the named initialization method '" + definition.InitMethodName + "'.");
                    }
                    targetMethod.Invoke(target, ObjectUtils.EmptyObjects);
                }
                catch (TargetInvocationException ex)
                {
                    throw new ObjectCreationException(definition.ResourceDescription, name,
                                                      "Initialization method '" + definition.InitMethodName + "' threw exception", ex.GetBaseException());
                }
                catch (Exception ex)
                {
                    throw new ObjectCreationException(definition.ResourceDescription, name,
                                                      "Invocation of initialization method '" + definition.InitMethodName + "' failed", ex);
                }
            }
        }

        /// <summary>
        /// Invoke the specified custom destroy method on the given object.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation invokes a no-arg method if found, else checking
        /// for a method with a single boolean argument (passing in "true",
        /// assuming a "force" parameter), else logging an error.
        /// </p>
        /// <p>
        /// Can be overridden in subclasses for custom resolution of destroy
        /// methods with arguments.
        /// </p>
        /// <p>
        /// Custom destroy methods are resolved in a <b>case-insensitive</b> manner.
        /// </p>
        /// </remarks>
        protected virtual void InvokeCustomDestroyMethod(string name, object target, string destroyMethodName)
        {
            bool usingForcingVersion = false;
            MethodInfo targetMethod = target.GetType().GetMethod(destroyMethodName, MethodResolutionFlags, null, Type.EmptyTypes, null);
            if (targetMethod == null)
            {
                // #%&^! try to find the method with a boolean "force" parameter
                targetMethod = target.GetType().GetMethod(destroyMethodName, MethodResolutionFlags, null, new Type[] { typeof(bool) }, null);
                if (targetMethod != null)
                {
                    usingForcingVersion = true;
                }
            }
            if (targetMethod == null)
            {
                #region Instrumentation

                log.Error("Couldn't find a method named '" + destroyMethodName + "' on object with name '" + name + "'");

                #endregion
            }
            else
            {
                object[] args = usingForcingVersion ? new object[] { true } : ObjectUtils.EmptyObjects;
                try
                {
                    targetMethod.Invoke(target, args);
                }
                catch (TargetInvocationException ex)
                {
                    #region Instrumentation

                    log.Error("Couldn't invoke destroy method '" + destroyMethodName + "' of object with name '" + name + "'", ex.GetBaseException());

                    #endregion
                }
                catch (Exception ex)
                {
                    LogExceptionRaisedByCustomDestroyMethodInvocation(destroyMethodName, name, ex);
                }
            }
        }

        private void LogExceptionRaisedByCustomDestroyMethodInvocation(string destroyMethodName, string name, Exception ex)
        {
            log.Error(
                    string.Format(CultureInfo.InvariantCulture, "Couldn't invoke destroy method '{0}' of object with name '{1}'.", destroyMethodName, name),
                    ex);
        }

        /// <summary>
        /// Destroy the target object.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Must destroy objects that depend on the given object before the object itself.
        /// Should not throw any exceptions.
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <param name="target">
        /// The target object instance to destroyed.
        /// </param>
        protected override void DestroyObject(string name, object target)
        {
            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug("Destroying dependant objects for object '" + name + "'");
            }

            #endregion

            DestroyDependantObjects(name);
            if (target is IDisposable)
            {
                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format(CultureInfo.InvariantCulture, "Calling Dispose () on object with name '{0}'.", name));
                }

                #endregion

                try
                {
                    ((IDisposable)target).Dispose();
                }
                catch (Exception ex)
                {
                    #region Instrumentation

                    log.Error("Destroy() on object with name '" + name + "' threw an exception.", ex);

                    #endregion
                }
            }
            RootObjectDefinition rootDefinition = GetMergedObjectDefinition(name, false);
            if (rootDefinition != null && StringUtils.HasText(rootDefinition.DestroyMethodName))
            {
                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug("Calling custom destroy method '" + rootDefinition.DestroyMethodName + "' on object with name '" + name + "'.");
                }

                #endregion

                InvokeCustomDestroyMethod(name, target, rootDefinition.DestroyMethodName);
            }
        }

        /// <summary>
        /// Destroys all of the objects registered as dependant on the 
        /// object (definition) identified by the supplied <paramref name="name"/>. 
        /// </summary>
        /// <param name="name">
        /// The name of the root object (definition) that is itself being destroyed.
        /// </param>
        private void DestroyDependantObjects(string name)
        {
            string[] dependingObjects = GetDependingObjectNames(name);
            foreach (string doName in dependingObjects)
            {
                DestroySingleton(doName);
            }
        }

        /// <summary>
        /// Given a property value, return a value, resolving any references to other
        /// objects in the factory if necessary.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The value could be :
        /// <list type="bullet">
        /// <item>
        /// <p>
        /// An <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>,
        /// which leads to the creation of a corresponding new object instance.
        /// Singleton flags and names of such "inner objects" are always ignored: inner objects
        /// are anonymous prototypes.
        /// </p>
        /// </item>
        /// <item>
        /// <p>
        /// A <see cref="Spring.Objects.Factory.Config.RuntimeObjectReference"/>, which must
        /// be resolved.
        /// </p>
        /// </item>
        /// <item>
        /// <p>
        /// An <see cref="Spring.Objects.Factory.Support.IManagedCollection"/>. This is a
        /// special placeholder collection that may contain
        /// <see cref="Spring.Objects.Factory.Config.RuntimeObjectReference"/>s or
        /// collections that will need to be resolved.
        /// </p>
        /// </item>
        /// <item>
        /// <p>
        /// An ordinary object or <see langword="null"/>, in which case it's left alone.
        /// </p>
        /// </item>
        /// </list>
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The name of the object that is having the value of one of its properties resolved.
        /// </param>
        /// <param name="definition">
        /// The definition of the named object.
        /// </param>
        /// <param name="argumentName">
        /// The name of the property the value of which is being resolved.
        /// </param>
        /// <param name="argumentValue">
        /// The value of the property that is being resolved.
        /// </param>
        protected object ResolveValueIfNecessary(string name, RootObjectDefinition definition, string argumentName, object argumentValue)
        {
            object resolvedValue = null;
            // we must check the argument value to see whether it requires a runtime
            // reference to another object to be resolved.
            // if it does, we'll attempt to instantiate the object and set the reference.
            if (argumentValue is ObjectDefinitionHolder)
            {
                // contains an IObjectDefinition with name and aliases...
                ObjectDefinitionHolder holder = (ObjectDefinitionHolder)argumentValue;
                resolvedValue = ResolveInnerObjectDefinition(name, holder.ObjectName, argumentName, holder.ObjectDefinition, definition.IsSingleton);
            }
            else if (argumentValue is IObjectDefinition)
            {
                // resolve plain IObjectDefinition, without contained name: use dummy name... 
                IObjectDefinition def = (IObjectDefinition)argumentValue;
                resolvedValue = ResolveInnerObjectDefinition(name, "(inner object)", argumentName, def, definition.IsSingleton);

            }
            else if (argumentValue is RuntimeObjectReference)
            {
                RuntimeObjectReference roref = (RuntimeObjectReference)argumentValue;
                resolvedValue = ResolveReference(definition, name, argumentName, roref);
            }
            else if (argumentValue is ExpressionHolder)
            {
                ExpressionHolder expHolder = (ExpressionHolder)argumentValue;
                object context = null;
                IDictionary variables = null;

                if (expHolder.Properties != null)
                {
                    PropertyValue contextProperty = expHolder.Properties.GetPropertyValue("Context");
                    context = contextProperty == null
                                         ? null
                                         : ResolveValueIfNecessary(name, definition, "Context",
                                                                   contextProperty.Value);
                    PropertyValue variablesProperty = expHolder.Properties.GetPropertyValue("Variables");
                    object vars = (variablesProperty == null
                                                   ? null
                                                   : ResolveValueIfNecessary(name, definition, "Variables",
                                                                             variablesProperty.Value));
                    if (vars is IDictionary)
                    {
                        variables = (IDictionary)vars;
                    }
                    else
                    {
                        if (vars != null) throw new ArgumentException("'Variables' must resolve to an IDictionary");
                    }
                }

                if (variables == null) variables = CollectionsUtil.CreateCaseInsensitiveHashtable();
                // add 'this' objectfactory reference to variables
                variables.Add(Expression.ReservedVariableNames.CurrentObjectFactory, this);

                resolvedValue = expHolder.Expression.GetValue(context, variables);
            }
            else if (argumentValue is IManagedCollection)
            {
                resolvedValue =
                        ((IManagedCollection)argumentValue).Resolve(name, definition, argumentName,
                                                                     new ManagedCollectionElementResolver(ResolveValueIfNecessary));
            }
            else if (argumentValue is TypedStringValue)
            {
                TypedStringValue tsv = (TypedStringValue)argumentValue;
                try
                {
                    Type resolvedTargetType = ResolveTargetType(tsv);
                    if (resolvedTargetType != null)
                    {
                        resolvedValue = TypeConversionUtils.ConvertValueIfNecessary(tsv.TargetType, tsv.Value, null);
                    }
                    else
                    {
                        resolvedValue = tsv.Value;
                    }
                }
                catch (Exception ex)
                {
                    throw new ObjectCreationException(definition.ResourceDescription, name,
                                                      "Error converted typed String value for " + argumentName, ex);
                }

            }
            else
            {
                // no need to resolve value...
                resolvedValue = argumentValue;
            }
            return resolvedValue;
        }

        /// <summary>
        /// Resolve the target type of the passed <see cref="TypedStringValue"/>.
        /// </summary>
        /// <param name="value">The <see cref="TypedStringValue"/> who's target type is to be resolved</param>
        /// <returns>The resolved target type, if any. <see lang="null" /> otherwise.</returns>
        protected virtual Type ResolveTargetType(TypedStringValue value)
        {
            if (value.HasTargetType)
            {
                return value.TargetType;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Resolves an inner object definition.
        /// </summary>
        /// <param name="name">
        /// The name of the object that surrounds this inner object definition.
        /// </param>
        /// <param name="innerObjectName">
        /// The name of the inner object definition... note: this is a synthetic
        /// name assigned by the factory (since it makes no sense for inner object
        /// definitions to have names).
        /// </param>
        /// <param name="argumentName">
        /// The name of the property the value of which is being resolved.
        /// </param>
        /// <param name="definition">
        /// The definition of the inner object that is to be resolved.
        /// </param>
        /// <param name="singletonOwner">
        /// <see langword="true"/> if the owner of the property is a singleton.
        /// </param>
        /// <returns>
        /// The resolved object as defined by the inner object definition.
        /// </returns>
        protected object ResolveInnerObjectDefinition(string name, string innerObjectName, string argumentName, IObjectDefinition definition,
                                                      bool singletonOwner)
        {
            RootObjectDefinition mod = GetMergedObjectDefinition(innerObjectName, definition);
            mod.IsSingleton = singletonOwner;
            object instance;
            object result;
            try
            {
                instance = CreateObject(innerObjectName, mod, ObjectUtils.EmptyObjects, false);
                result = GetObjectForInstance(innerObjectName, instance);
            }
            catch (ObjectsException ex)
            {
                throw ObjectCreationException.GetObjectCreationException(ex, name, argumentName, definition.ResourceDescription, innerObjectName);
            }
            if (singletonOwner && instance is IDisposable)
            {
                // keep a reference to the inner object instance, to be able to destroy
                // it on factory shutdown...
                _disposableInnerObjects.Add(instance);
            }
            return result;
        }

        /// <summary>
        /// Resolve a reference to another object in the factory.
        /// </summary>
        /// <param name="name">
        /// The name of the object that is having the value of one of its properties resolved.
        /// </param>
        /// <param name="definition">
        /// The definition of the named object.
        /// </param>
        /// <param name="argumentName">
        /// The name of the property the value of which is being resolved.
        /// </param>
        /// <param name="reference">
        /// The runtime reference containing the value of the property.
        /// </param>
        /// <returns>A reference to another object in the factory.</returns>
        protected object ResolveReference(IConfigurableObjectDefinition definition, string name, string argumentName, RuntimeObjectReference reference)
        {
            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug(
                        string.Format(CultureInfo.InvariantCulture, "Resolving reference from property '{0}' in object '{1}' to object '{2}'.",
                                      argumentName, name, reference.ObjectName));
            }

            #endregion

            try
            {
                if (reference.IsToParent)
                {
                    if (null == ParentObjectFactory)
                    {
                        throw new ObjectCreationException(definition.ResourceDescription, name,
                                                          string.Format(
                                                                  "Can't resolve reference to '{0}' in parent factory: " + "no parent factory available.",
                                                                  reference.ObjectName));
                    }
                    return ParentObjectFactory.GetObject(reference.ObjectName);
                }
                return GetObject(reference.ObjectName);
            }
            catch (ObjectsException ex)
            {
                throw ObjectCreationException.GetObjectCreationException(ex, name, argumentName, definition.ResourceDescription, reference.ObjectName);
            }
        }

        /// <summary>
        /// Find object instances that match the required <see cref="System.Type"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Called by autowiring. If a subclass cannot obtain information about object
        /// names by <see cref="System.Type"/>, a corresponding exception should be thrown.
        /// </p>
        /// </remarks>
        /// <param name="requiredType">
        /// The <see cref="System.Type"/> of the objects to look up.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionary"/> of object names and object
        /// instances that match the required <see cref="System.Type"/>, or
        /// <see langword="null"/> if none are found.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        protected abstract IDictionary FindMatchingObjects(Type requiredType);

        /// <summary>
        /// Return the names of the objects that depend on the given object.
        /// Called by DestroyObject, to be able to destroy depending objects first.
        /// </summary>
        /// <param name="name">
        /// The name of the object to find depending objects for.
        /// </param>
        /// <returns>
        /// The array of names of depending objects, or the empty string array if none.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        protected abstract string[] GetDependingObjectNames(string name);

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
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.ConfigureObject(object, string)"/>
        public override object ConfigureObject(object target, string name)
        {
            RootObjectDefinition definition = GetMergedObjectDefinition(name, true);
            if (definition != null)
            {
                return ConfigureObject(name, definition, new ObjectWrapper(target));
            }

            return null;
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
        public override object ConfigureObject(object target, string name, IObjectDefinition definition)
        {
            return ConfigureObject(name, new RootObjectDefinition(definition), new ObjectWrapper(target));
        }

        /// <summary>
        /// Configures object instance by injecting dependencies, satisfying Spring lifecycle
        /// interfaces and applying object post-processors.
        /// </summary>
        /// <param name="name">
        /// The name of the object definition expressing the dependencies that are to
        /// be injected into the supplied <parameref name="target"/> instance.
        /// </param>
        /// <param name="definition">
        /// An object definition that should be used to configure object.
        /// </param>
        /// <param name="wrapper">
        /// A wrapped object instance that is to be so configured.
        /// </param>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory.ConfigureObject(object, string)"/>
        protected virtual object ConfigureObject(string name, RootObjectDefinition definition, IObjectWrapper wrapper)
        {
            object instance = wrapper.WrappedInstance;

            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format("configuring object '{0}' using definition '{1}'", instance, name));
            }

            #endregion

            PopulateObject(name, definition, wrapper);
            WireEvents(name, definition, wrapper);

            if (ObjectUtils.IsAssignableAndNotTransparentProxy(typeof(IObjectNameAware), instance))
            {
                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format(CultureInfo.InvariantCulture, "Setting the name property on the IObjectNameAware object '{0}'.", name));
                }

                #endregion

                ((IObjectNameAware)instance).ObjectName = name;
            }

            if (ObjectUtils.IsAssignableAndNotTransparentProxy(typeof(IObjectFactoryAware), instance))
            {
                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(
                            string.Format(CultureInfo.InvariantCulture, "Setting the ObjectFactory property on the IObjectFactoryAware object '{0}'.",
                                          name));
                }

                #endregion

                ((IObjectFactoryAware)instance).ObjectFactory = this;
            }

            instance = ApplyObjectPostProcessorsBeforeInitialization(instance, name);
            InvokeInitMethods(instance, name, definition);
            instance = ApplyObjectPostProcessorsAfterInitialization(instance, name);

            return instance;
        }


        /// <summary>
        /// Applies the <code>PostProcessAfterInitialization</code> callback of all
        /// registered IObjectPostProcessors, giving them a chance to post-process
        /// the object obtained from IFactoryObjects (for example, to auto-proxy them)
        /// </summary>
        /// <param name="instance">The instance obtained from the IFactoryObject.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>The object instance to expose</returns>
        /// <exception cref="ObjectsException">if any post-processing failed.</exception>
        protected override object PostProcessObjectFromFactoryObject(object instance, string objectName)
        {
            return ApplyObjectPostProcessorsAfterInitialization(instance, objectName);
        }

        #endregion

        #region IAutowireCapableObjectFactory Members

        /// <summary>
        /// Create a new object instance of the given class with the specified
        /// autowire strategy.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the object to instantiate.
        /// </param>
        /// <param name="autowireMode">
        /// The desired autowiring mode.
        /// </param>
        /// <param name="dependencyCheck">
        /// Whether to perform a dependency check for objects (not applicable to
        /// autowiring a constructor, thus ignored there).
        /// </param>
        /// <returns>The new object instance.</returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the wiring fails.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.Config.AutoWiringMode"/>
        public virtual object Autowire(Type type, AutoWiringMode autowireMode, bool dependencyCheck)
        {
            RootObjectDefinition rod = new RootObjectDefinition(type, autowireMode, dependencyCheck);
            if (rod.ResolvedAutowireMode == AutoWiringMode.Constructor)
            {
                return AutowireConstructor(type.Name, rod).WrappedInstance;
            }
            else
            {
                object obj = InstantiationStrategy.Instantiate(rod, string.Empty, this);
                PopulateObject(obj.GetType().Name, rod, new ObjectWrapper(obj));
                return obj;
            }
        }

        /// <summary>
        /// Autowire the object properties of the given object instance by name or
        /// <see cref="System.Type"/>.
        /// </summary>
        /// <param name="instance">
        /// The existing object instance.
        /// </param>
        /// <param name="autowireMode">
        /// The desired autowiring mode.
        /// </param>
        /// <param name="dependencyCheck">
        /// Whether to perform a dependency check for the object.
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the wiring fails.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="autowireMode"/> is not one of the
        /// <seealso cref="Spring.Objects.Factory.Config.AutoWiringMode.ByName"/> or
        /// <seealso cref="Spring.Objects.Factory.Config.AutoWiringMode.ByType"/>
        /// values.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.Config.AutoWiringMode"/>
        public virtual void AutowireObjectProperties(object instance, AutoWiringMode autowireMode, bool dependencyCheck)
        {
            if (autowireMode != AutoWiringMode.ByName && autowireMode != AutoWiringMode.ByType)
            {
                throw new ArgumentException("Just AutoWiringMode.ByName and AutoWiringMode.ByType allowed.");
            }
            RootObjectDefinition rod = new RootObjectDefinition(instance.GetType(), autowireMode, dependencyCheck);
            PopulateObject(instance.GetType().Name, rod, new ObjectWrapper(instance));
        }

        /// <summary>
        /// Apply <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>s
        /// to the given existing object instance, invoking their
        /// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessBeforeInitialization"/>
        /// methods.
        /// </summary>
        /// <param name="instance">
        /// The existing object instance.
        /// </param>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <returns>
        /// The object instance to use, either the original or a wrapped one.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If any post-processing failed.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessBeforeInitialization"/>
        public virtual object ApplyObjectPostProcessorsBeforeInitialization(object instance, string name)
        {
            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug("Invoking IObjectPostProcessors before initialization of object '" + name + "'");
            }

            #endregion

            object result = instance;
            foreach (IObjectPostProcessor objectProcessor in ObjectPostProcessors)
            {
                result = objectProcessor.PostProcessBeforeInitialization(result, name);
                if (result == null)
                {
                    throw new ObjectCreationException(name,
                            string.Format(CultureInfo.InvariantCulture,
                                          "PostProcessBeforeInitialization method of IObjectPostProcessor [{0}] "
                                          + " returned null for object [{1}] with name '{2}'.", objectProcessor, instance, name));
                }
            }
            return result;
        }

        /// <summary>
        /// Apply <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>s
        /// to the given existing object instance, invoking their
        /// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessAfterInitialization"/>
        /// methods.
        /// </summary>
        /// <param name="instance">
        /// The existing object instance.
        /// </param>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <returns>
        /// The object instance to use, either the original or a wrapped one.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If any post-processing failed.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.Config.IObjectPostProcessor.PostProcessAfterInitialization"/>
        public virtual object ApplyObjectPostProcessorsAfterInitialization(object instance, string name)
        {
            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug("Invoking IObjectPostProcessors after initialization of object '" + name + "'");
            }

            #endregion

            object result = instance;
            foreach (IObjectPostProcessor objectProcessor in ObjectPostProcessors)
            {
                result = objectProcessor.PostProcessAfterInitialization(result, name);
                if (result == null)
                {
                    throw new ObjectCreationException(name,
                            string.Format(CultureInfo.InvariantCulture,
                                          "PostProcessAfterInitialization method of IObjectPostProcessor [{0}] "
                                          + " returned null for object [{1}] with name [{2}].", objectProcessor, instance, name));
                }
            }
            return result;
        }

        #endregion

        #region Fields

        /// <summary>
        /// Set that holds all inner objects created by this factory that implement the IDisposable
        /// interface, to be destroyed on call to Dispose.
        /// </summary>
        private ISet _disposableInnerObjects = new SynchronizedSet(new HybridSet());

        private IInstantiationStrategy instantiationStrategy = new MethodInjectingInstantiationStrategy();

        /// <summary>
        /// Cache of unfinished IFactoryObject instances: IFactoryObject name --> IObjectWrapper */
        /// </summary>
        private IDictionary factoryObjectInstanceCache = new Hashtable();

        /// <summary>
        /// Cache of filtered PropertyInfos: object Type -> PropertyInfo array 
        /// </summary>
        private IDictionary filteredPropertyDescriptorsCache = new Hashtable();

        /// <summary>
        /// Dependency interfaces to ignore on dependency check and autowire, as Set of
        /// Class objects. By default, only the IObjectFactoryAware and IObjectNameAware 
        /// interfaces are ignored.
        /// </summary>
        private ISet ignoredDependencyInterfaces = new HybridSet();

        #endregion
    }

    internal class UnsatisfiedDependencyExceptionData
    {
        private int parameterIndex;
        private Type parameterType;
        private string errorMessage;

        public UnsatisfiedDependencyExceptionData(int parameterIndex, Type parameterType, string errorMessage)
        {
            this.parameterIndex = parameterIndex;
            this.parameterType = parameterType;
            this.errorMessage = errorMessage;
        }

        public int ParameterIndex
        {
            get { return parameterIndex; }
        }

        public Type ParameterType
        {
            get { return parameterType; }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
        }
    }
}
