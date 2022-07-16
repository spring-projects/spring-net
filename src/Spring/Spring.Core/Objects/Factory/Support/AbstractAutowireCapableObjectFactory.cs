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
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using Spring.Collections;
using Spring.Core.TypeResolution;
using Spring.Objects.Factory.Config;
using Spring.Util;

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
    [Serializable]
    public abstract class AbstractAutowireCapableObjectFactory : AbstractObjectFactory, IAutowireCapableObjectFactory
    {

        private IInstantiationStrategy instantiationStrategy = new MethodInjectingInstantiationStrategy();

        /// <summary>
        /// Cache of filtered PropertyInfos: object Type -> PropertyInfo array
        /// </summary>
        private readonly ConcurrentDictionary<Type, List<PropertyInfo>> filteredPropertyDescriptorsCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();

        /// <summary>
        /// Dependency interfaces to ignore on dependency check and autowire, as Set of
        /// Class objects. By default, only the IObjectFactoryAware and IObjectNameAware
        /// interfaces are ignored.
        /// </summary>
        private readonly HybridSet ignoredDependencyInterfaces = new HybridSet();

        [NonSerialized]
        private ObjectDefinitionValueResolver cachedValueResolver;

        /// <summary>
        /// The <see cref="System.Reflection.BindingFlags"/> used during the invocation and
        /// searching for of methods.
        /// </summary>
        protected const BindingFlags MethodResolutionFlags =
                BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;

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
            IgnoreDependencyInterface(typeof(IObjectFactoryAware));
            IgnoreDependencyInterface(typeof(IObjectNameAware));
        }

        /// <summary>
        /// The <see cref="Spring.Objects.Factory.Support.IInstantiationStrategy"/>
        /// implementation to be used to instantiate managed objects.
        /// </summary>
        protected IInstantiationStrategy InstantiationStrategy
        {
            get => instantiationStrategy;
            set => instantiationStrategy = value;
        }

        /// <summary>
        /// Predict the eventual object type (of the processed object instance) for the
        /// specified object.
        /// </summary>
        /// <param name="objectName">Name of the object.</param>
        /// <param name="mod">The merged object definition to determine the type for. May be <c>null</c></param>
        /// <returns>
        /// The type of the object, or <code>null</code> if not predictable
        /// </returns>
        protected override Type PredictObjectType(string objectName, RootObjectDefinition mod)
        {
            Type objectType;
            if (mod == null)
            {
                return null;
            }

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
            }
            if (returnTypes.Count == 1)
            {
                // clear return type found: all factory methods return same type...
                return (Type)ObjectUtils.EnumerateFirstElement(returnTypes);
            }

            // ambiguous return types found: return null to indicate "not determinable"...
            return null;
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
                log.Debug($"configuring object '{instance}' using definition '{name}'");
                ApplyPropertyValues(name, definition, new ObjectWrapper(instance), definition.PropertyValues);
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
        /// <param name="definition">
        /// An object definition that should be used to apply property values.
        /// </param>
        public override void ApplyObjectPropertyValues(object instance, string name, IObjectDefinition definition)
        {
            MarkObjectAsCreated(name);
            ApplyPropertyValues(name, new RootObjectDefinition(definition), new ObjectWrapper(instance), definition.PropertyValues);
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
            if (log.IsDebugEnabled)
            {
                log.Debug("Invoking IInstantiationAwareObjectPostProcessors before " +
                          $"the instantiation of '{objectName}'.");
            }

            for (var i = 0; i < objectPostProcessors.Count; i++)
            {
                IObjectPostProcessor processor = objectPostProcessors[i];
                IInstantiationAwareObjectPostProcessor inProc = processor as IInstantiationAwareObjectPostProcessor;
                object theObject = inProc?.PostProcessBeforeInstantiation(objectType, objectName);
                if (theObject != null)
                {
                    return theObject;
                }
            }

            return null;
        }

        /// <summary>
        /// Apply <see cref="Spring.Objects.Factory.Config.IDestructionAwareObjectPostProcessor"/>s
        /// to the given existing object instance, invoking their
        /// <see cref="Spring.Objects.Factory.Config.IDestructionAwareObjectPostProcessor.PostProcessBeforeDestruction"/>
        /// methods.
        /// </summary>
        /// <param name="instance">
        /// The existing object instance.
        /// </param>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <seealso cref="Spring.Objects.Factory.Config.IDestructionAwareObjectPostProcessor.PostProcessBeforeDestruction"/>
        public virtual void ApplyObjectPostProcessBeforeDestruction(object instance, string name)
        {
            log.Debug(m => m("Invoking PostProcessBeforeDestruction after IDisposal of object '" + name + "'"));

            for (var i = 0; i < ObjectPostProcessors.Count; i++)
            {
                IObjectPostProcessor objectProcessor = ObjectPostProcessors[i];
                if (objectProcessor is IDestructionAwareObjectPostProcessor processor)
                {
                    try
                    {
                        processor.PostProcessBeforeDestruction(instance, name);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat(
                            $"Error during execution of {processor.GetType().Name}.PostProcessBeforeDestruction for object {name}", ex);
                    }
                }
            }
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
            if (properties == null || properties.PropertyValues.Count == 0)
            {
                return;
            }
            ObjectDefinitionValueResolver valueResolver = CreateValueResolver();

            MutablePropertyValues deepCopy = new MutablePropertyValues(properties);
            var copiedProperties = deepCopy.PropertyValues;
            for (int i = 0; i < copiedProperties.Count; ++i)
            {
                PropertyValue copiedProperty = copiedProperties[i];
                //(string name, RootObjectDefinition definition, string argumentName, object argumentValue)
                object value = valueResolver.ResolveValueIfNecessary(name, definition, copiedProperty.Name, copiedProperty.Value);
                // object value = ResolveValueIfNecessary(name, definition, copiedProperty.Name, copiedProperty.Value);
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
        /// Create the value resolver strategy to use for resolving raw property values
        /// </summary>
        protected virtual ObjectDefinitionValueResolver CreateValueResolver()
        {
            return cachedValueResolver ?? (cachedValueResolver =new ObjectDefinitionValueResolver(this));
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
        protected string[] UnsatisfiedNonSimpleProperties(RootObjectDefinition definition, IObjectWrapper wrapper)
        {
            ListSet results = new ListSet();
            IPropertyValues pvs = definition.PropertyValues;
            PropertyInfo[] properties = wrapper.GetPropertyInfos();
            foreach (PropertyInfo property in properties)
            {
                string name = property.Name;
                if (property.CanWrite
                    && !IsExcludedFromDependencyCheck(property)
                    && !pvs.Contains(name)
                    && !ObjectUtils.IsSimpleProperty(property.PropertyType))
                {
                    results.Add(name);
                }
            }
            return (string[])CollectionUtils.ToArray(results, typeof(string));
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
            //TODO: fix the calls to GetObject(...) etc. so that they are invalid during container shutdown rather than attempting to resolve
            //  objects and permitting calling code to even get this far; considered too invasive a breaking change for 1.3.2; recommend impl
            //  of this change for the 2.0 release

            base.Dispose();

            //have to clone the collection before iterating it to avoid arbitrary code in the objects' Dispose()
            // that might permit re-entering the DisposableInnerObjects collections during the iteration to destroy them
            // see https://jira.springframework.org/browse/SPRNET-1334

            ISet clone = (ISet)DisposableInnerObjects.Clone();

            foreach (object o in clone)
            {
                DestroyObject(string.Format(CultureInfo.InvariantCulture, "(Inner object of Type '{0}')", o.GetType().FullName), o);
            }

            DisposableInnerObjects.Clear();
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

            if (HasInstantiationAwareObjectPostProcessors)
            {
                for (var i = 0; i < ObjectPostProcessors.Count; i++)
                {
                    IObjectPostProcessor processor = ObjectPostProcessors[i];
                    if (processor is IInstantiationAwareObjectPostProcessor inProc)
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
                if (properties.PropertyValues.Count > 0)
                {
                    throw new ObjectCreationException(definition.ResourceDescription,
                        name, "Cannot apply property values to null instance.");
                }

                // skip property population phase for null instance
                return;
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


            bool hasInstAwareOpps = HasInstantiationAwareObjectPostProcessors;
            bool needsDepCheck = (definition.DependencyCheck != DependencyCheckingMode.None);

            if (hasInstAwareOpps || needsDepCheck)
            {
                List<PropertyInfo> filteredPropInfo = null;
                if (hasInstAwareOpps)
                {
                    for (var i = 0; i < ObjectPostProcessors.Count; i++)
                    {
                        IObjectPostProcessor processor = ObjectPostProcessors[i];
                        if (processor is IInstantiationAwareObjectPostProcessor instantiationAwareObjectPostProcessor)
                        {
                            filteredPropInfo = filteredPropInfo ?? FilterPropertyInfoForDependencyCheck(wrapper);
                            properties = instantiationAwareObjectPostProcessor.PostProcessPropertyValues(properties,
                                filteredPropInfo, wrapper.WrappedInstance, name);
                            if (properties == null)
                            {
                                return;
                            }
                        }
                    }
                }

                if (needsDepCheck)
                {
                    filteredPropInfo = filteredPropInfo ?? FilterPropertyInfoForDependencyCheck(wrapper);
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
            string[] propertyNames = UnsatisfiedNonSimpleProperties(definition, wrapper);
            foreach (string propertyName in propertyNames)
            {
                // look for a matching type
                if (ContainsObject(propertyName))
                {
                    object o = GetObject(propertyName);
                    properties.Add(propertyName, o);

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                                string.Format(CultureInfo.InvariantCulture,
                                              "Added autowiring by name from object name '{0}' via " + "property '{1}' to object named '{1}'.", name,
                                              propertyName));
                    }
                }
                else
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                                string.Format(CultureInfo.InvariantCulture,
                                              "Not autowiring property '{0}' of object '{1}' by name: " + "no matching object found.", propertyName, name));
                    }
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
            string[] propertyNames = UnsatisfiedNonSimpleProperties(definition, wrapper);
            foreach (string propertyName in propertyNames)
            {
                // look for a matching type
                Type requiredType = wrapper.GetPropertyType(propertyName);
                IDictionary<string, object> matchingObjects = FindMatchingObjects(requiredType);
                if (matchingObjects != null && matchingObjects.Count == 1)
                {
                    properties.Add(propertyName, ObjectUtils.EnumerateFirstElement(matchingObjects.Values));

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                                string.Format(CultureInfo.InvariantCulture,
                                              "Autowiring by type from object name '{0}' via property " + "'{1}' to object named '{2}'.", name,
                                              propertyName, ObjectUtils.EnumerateFirstElement(matchingObjects.Keys)));
                    }
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
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                                string.Format(CultureInfo.InvariantCulture, "Not autowiring property '{0}' of object '{1}': no matching object found.",
                                              propertyName, name));
                    }
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

        //        /// <summary>
        //        /// Create an object instance for the given object definition.
        //        /// </summary>
        //        /// <param name="name">The name of the object.</param>
        //        /// <param name="definition">
        //        /// The object definition for the object that is to be instantiated.
        //        /// </param>
        //        /// <param name="arguments">
        //        /// The arguments to use if creating a prototype using explicit arguments to
        //        /// a static factory method. It is invalid to use a non-<see langword="null"/> arguments value
        //        /// in any other case.
        //        /// </param>
        //        /// <returns>
        //        /// A new instance of the object.
        //        /// </returns>
        //        /// <exception cref="Spring.Objects.ObjectsException">
        //        /// In case of errors.
        //        /// </exception>
        //        /// <remarks>
        //        /// <p>
        //        /// Delegates to the
        //        /// <see cref="Spring.Objects.Factory.Support.AbstractAutowireCapableObjectFactory.CreateObject (string,RootObjectDefinition,object[],bool)"/>
        //        /// method version with the <c>allowEagerCaching</c> parameter set to <b>true</b>.
        //        /// </p>
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
        //        protected internal override object CreateObject(string name, RootObjectDefinition definition, object[] arguments)
        //        {
        //            return CreateObject(name, definition, arguments, true, false);
        //        }

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
        /// Suppress injecting dependencies yet.
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
        protected internal override object InstantiateObject(string name, RootObjectDefinition definition, object[] arguments, bool allowEagerCaching, bool suppressConfigure)
        {
            // guarantee the initialization of objects that the current one depends on..
            if (definition.dependsOn != null)
            {
                for (var i = 0; i < definition.dependsOn.Count; i++)
                {
                    string dependant = definition.dependsOn[i];
                    GetObject(dependant);
                }
            }

            var isDebugEnabled = log.IsDebugEnabled;
            if (isDebugEnabled)
            {
                log.Debug($"Creating instance of Object '{name}' with merged definition [{definition}].");
            }

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

            object instance;
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

                var instanceWrapper = CreateObjectInstance(name, definition, arguments);
                instance = instanceWrapper.WrappedInstance;

                // eagerly cache singletons to be able to resolve circular references
                // even when triggered by lifecycle interfaces like IObjectFactoryAware.
                if (allowEagerCaching && definition.IsSingleton)
                {
                    if (isDebugEnabled)
                    {
                        log.Debug($"Eagerly caching object '{name}' to allow for resolving potential circular references");
                    }
                    AddEagerlyCachedSingleton(name, definition, instance);
                    eagerlyCached = true;
                }

                if (!suppressConfigure)
                {
                    instance = ConfigureObject(name, definition, instanceWrapper);
                }
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
            RemoveSingleton(objectName);
        }

        /// <summary>
        /// Creates an <see cref="IObjectWrapper"/> instance from the <see cref="RootObjectDefinition"/> passed in <paramref name="objectDefinition"/>
        /// using constructor <paramref name="arguments"/>
        /// </summary>
        /// <param name="objectName">The name of the object to create - used for error messages.</param>
        /// <param name="objectDefinition">The <see cref="RootObjectDefinition"/> describing the object to be created.</param>
        /// <param name="arguments">optional arguments to pass to the constructor</param>
        /// <returns>An <see cref="IObjectWrapper"/> wrapping the already instantiated object</returns>
        protected IObjectWrapper CreateObjectInstance(string objectName, RootObjectDefinition objectDefinition, object[] arguments)
        {
            // Make sure object class is actually resolved at this point.
            Type objectType = ResolveObjectType(objectDefinition, objectName);
            if (StringUtils.HasText(objectDefinition.FactoryMethodName))
            {
                return InstantiateUsingFactoryMethod(objectName, objectDefinition, arguments);
            }

            //TODO perf optimization when creating the same object

            ConstructorInfo[] ctors = DetermineConstructorsFromObjectPostProcessors(objectType, objectName);
            if (ctors != null ||
                objectDefinition.ResolvedAutowireMode == AutoWiringMode.Constructor ||
                objectDefinition.HasConstructorArgumentValues || !ObjectUtils.IsEmpty(arguments))
            {
                return AutowireConstructor(objectName, objectDefinition, ctors, arguments);
            }

            // No special handling: simply use no-arg constructor.
            return InstantiateObject(objectName, objectDefinition);

            /*
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

            */
        }

        /// <summary>
        /// Instantiates the given object using its default constructor
        /// </summary>
        /// <param name="objectName">Name of the object.</param>
        /// <param name="definition">The definition.</param>
        /// <returns>IObjectWrapper for the new instance</returns>
        protected virtual IObjectWrapper InstantiateObject(string objectName, RootObjectDefinition definition)
        {
            return new ObjectWrapper(InstantiationStrategy.Instantiate(definition, objectName, this));
        }

        /// <summary>
        /// Determines candidate constructors to use for the given object, checking all registered
        /// <see cref="SmartInstantiationAwareObjectPostProcessor"/>
        /// </summary>
        /// <param name="objectType">Raw type of the object.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>the candidate constructors, or <code>null</code> if none specified</returns>
        /// <exception cref="ObjectsException">In case of errors</exception>
        /// <seealso cref="SmartInstantiationAwareObjectPostProcessor.DetermineCandidateConstructors"/>
        protected virtual ConstructorInfo[] DetermineConstructorsFromObjectPostProcessors(Type objectType, string objectName)
        {
            if (HasInstantiationAwareObjectPostProcessors)
            {
                for (var i = 0; i < objectPostProcessors.Count; i++)
                {
                    IObjectPostProcessor objectPostProcessor = objectPostProcessors[i];
                    if (ObjectUtils.IsAssignable(typeof(SmartInstantiationAwareObjectPostProcessor),
                        objectPostProcessor))
                    {
                        SmartInstantiationAwareObjectPostProcessor iop =
                            (SmartInstantiationAwareObjectPostProcessor) objectPostProcessor;
                        ConstructorInfo[] ctors = iop.DetermineCandidateConstructors(objectType, objectName);
                        if (ctors != null)
                        {
                            return ctors;
                        }
                    }
                }
            }
            return null;
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
            ConstructorResolver constructorResolver =
                    new ConstructorResolver(this, this, InstantiationStrategy, CreateValueResolver());
            return constructorResolver.InstantiateUsingFactoryMethod(name, definition, arguments);
        }

        /// <summary>
        /// "autowire constructor" (with constructor arguments by type) behaviour.
        /// </summary>
        /// <param name="name">The name of the object to autowire by type.</param>
        /// <param name="definition">The object definition to update through autowiring.</param>
        /// <param name="ctors">The chosen candidate constructors.</param>
        /// <param name="explicitArgs">The argument values passed in programmatically via the GetObject method,
        /// or <code>null</code> if none (-> use constructor argument values from object definition)</param>
        /// <returns>
        /// An <see cref="Spring.Objects.IObjectWrapper"/> for the new instance.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Also applied if explicit constructor argument values are specified,
        /// matching all remaining arguments with objects from the object factory.
        /// </para>
        /// <para>
        /// This corresponds to constructor injection: in this mode, a Spring.NET
        /// object factory is able to host components that expect constructor-based
        /// dependency resolution.
        /// </para>
        /// </remarks>
        protected IObjectWrapper AutowireConstructor(string name, RootObjectDefinition definition, ConstructorInfo[] ctors, object[] explicitArgs)
        {
            ConstructorResolver constructorResolver =
                new ConstructorResolver(this, this, InstantiationStrategy, CreateValueResolver());
            return constructorResolver.AutowireConstructor(name, definition, ctors, explicitArgs);

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

            IList<PropertyInfo> filteredPropInfo = FilterPropertyInfoForDependencyCheck(wrapper);
            if (HasInstantiationAwareObjectPostProcessors)
            {
                for (var i = 0; i < ObjectPostProcessors.Count; i++)
                {
                    IObjectPostProcessor processor = ObjectPostProcessors[i];
                    if (processor is IInstantiationAwareObjectPostProcessor inProc)
                    {
                        properties = inProc.PostProcessPropertyValues(properties, filteredPropInfo, wrapper.WrappedInstance, name);
                        if (properties == null)
                        {
                            return;
                        }
                    }
                }
            }


            CheckDependencies(name, definition, filteredPropInfo, properties);
        }

        private void CheckDependencies(string name, IConfigurableObjectDefinition definition, IList<PropertyInfo> filteredPropInfo, IPropertyValues properties)
        {
            DependencyCheckingMode dependencyCheck = definition.DependencyCheck;
            IList<PropertyInfo> unsatisfiedDependencies = AutowireUtils.GetUnsatisfiedDependencies(filteredPropInfo, properties, dependencyCheck);

            if (unsatisfiedDependencies.Count > 0)
            {
                throw new UnsatisfiedDependencyException(definition.ResourceDescription, name, unsatisfiedDependencies[0].Name,
                    "Set this property value or disable dependency checking for this object.");
            }
        }

        /// <summary>
        /// Extract a filtered set of PropertyInfos from the given IObjectWrapper, excluding
        /// ignored dependency types.
        /// </summary>
        /// <param name="wrapper">The object wrapper the object was created with.</param>
        /// <returns>The filtered PropertyInfos</returns>
        private List<PropertyInfo> FilterPropertyInfoForDependencyCheck(IObjectWrapper wrapper)
        {
            return filteredPropertyDescriptorsCache.GetOrAdd(wrapper.WrappedType, t =>
            {
                var list = new List<PropertyInfo>(wrapper.GetPropertyInfos());
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    PropertyInfo pi = list[i];
                    if (IsExcludedFromDependencyCheck(pi))
                    {
                        list.RemoveAt(i);
                    }
                }

                return list;
            });
        }

        /// <summary>
        /// Determine whether the given bean property is excluded from dependency checks.
        /// This implementation excludes properties whose type matches an ignored dependency type
        /// or which are defined by an ignored dependency interface.
        /// </summary>
        /// <seealso cref="AbstractObjectFactory.IgnoreDependencyType(Type)"/>
        /// <seealso cref="IgnoreDependencyInterface(Type)"/>
        /// <param name="property">the <see cref="PropertyInfo"/> of the object property</param>
        /// <returns>whether the object property is excluded</returns>
        private bool IsExcludedFromDependencyCheck(PropertyInfo property)
        {
            bool b1 = !property.CanWrite; //AutowireUtils.IsExcludedFromDependencyCheck(pi);
            bool b2 = IgnoredDependencyTypes.Contains(property.PropertyType);
            bool b3 = AutowireUtils.IsSetterDefinedInInterface(property, ignoredDependencyInterfaces);
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
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format(CultureInfo.InvariantCulture, "Calling AfterPropertiesSet() on object with name '{0}'.", name));
                }

                ((IInitializingObject)target).AfterPropertiesSet();
            }
            if (StringUtils.HasText(definition.InitMethodName))
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(
                            string.Format(CultureInfo.InvariantCulture, "Calling custom init method '{0} on object with name '{1}'.",
                                          definition.InitMethodName, name));
                }

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
                log.Error("Couldn't find a method named '" + destroyMethodName + "' on object with name '" + name + "'");
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
                    log.Error("Couldn't invoke destroy method '" + destroyMethodName + "' of object with name '" + name + "'", ex.GetBaseException());
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
            using (new DisposableObjectAdapter(target, name, GetMergedObjectDefinition(name, true), ObjectPostProcessors))
            {
                log.Debug(m => m("Destroying dependant objects for object '{0}", name));
                DestroyDependantObjects(name);
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
            IList<string> dependingObjects = GetDependingObjectNames(name);
            foreach (string doName in dependingObjects)
            {
                DestroySingleton(doName);
            }
        }

        ///// <summary>
        ///// Given a property value, return a value, resolving any references to other
        ///// objects in the factory if necessary.
        ///// </summary>
        ///// <remarks>
        ///// <p>
        ///// The value could be :
        ///// <list type="bullet">
        ///// <item>
        ///// <p>
        ///// An <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>,
        ///// which leads to the creation of a corresponding new object instance.
        ///// Singleton flags and names of such "inner objects" are always ignored: inner objects
        ///// are anonymous prototypes.
        ///// </p>
        ///// </item>
        ///// <item>
        ///// <p>
        ///// A <see cref="Spring.Objects.Factory.Config.RuntimeObjectReference"/>, which must
        ///// be resolved.
        ///// </p>
        ///// </item>
        ///// <item>
        ///// <p>
        ///// An <see cref="Spring.Objects.Factory.Support.IManagedCollection"/>. This is a
        ///// special placeholder collection that may contain
        ///// <see cref="Spring.Objects.Factory.Config.RuntimeObjectReference"/>s or
        ///// collections that will need to be resolved.
        ///// </p>
        ///// </item>
        ///// <item>
        ///// <p>
        ///// An ordinary object or <see langword="null"/>, in which case it's left alone.
        ///// </p>
        ///// </item>
        ///// </list>
        ///// </p>
        ///// </remarks>
        ///// <param name="name">
        ///// The name of the object that is having the value of one of its properties resolved.
        ///// </param>
        ///// <param name="definition">
        ///// The definition of the named object.
        ///// </param>
        ///// <param name="argumentName">
        ///// The name of the property the value of which is being resolved.
        ///// </param>
        ///// <param name="argumentValue">
        ///// The value of the property that is being resolved.
        ///// </param>
        //        protected object ResolveValueIfNecessary(string name, RootObjectDefinition definition, string argumentName, object argumentValue)
        //        {
        //            object resolvedValue = null;
        //
        //            AssertUtils.ArgumentNotNull(resolvedValue, "test");
        //
        //            // we must check the argument value to see whether it requires a runtime
        //            // reference to another object to be resolved.
        //            // if it does, we'll attempt to instantiate the object and set the reference.
        //            if (RemotingServices.IsTransparentProxy(argumentValue))
        //            {
        //                resolvedValue = argumentValue;
        //            }
        //            else if (argumentValue is ObjectDefinitionHolder)
        //            {
        //                // contains an IObjectDefinition with name and aliases...
        //                ObjectDefinitionHolder holder = (ObjectDefinitionHolder)argumentValue;
        //                resolvedValue = ResolveInnerObjectDefinition(name, holder.ObjectName, argumentName, holder.ObjectDefinition, definition.IsSingleton);
        //            }
        //            else if (argumentValue is IObjectDefinition)
        //            {
        //                // resolve plain IObjectDefinition, without contained name: use dummy name...
        //                IObjectDefinition def = (IObjectDefinition)argumentValue;
        //                resolvedValue = ResolveInnerObjectDefinition(name, "(inner object)", argumentName, def, definition.IsSingleton);
        //
        //            }
        //            else if (argumentValue is RuntimeObjectReference)
        //            {
        //                RuntimeObjectReference roref = (RuntimeObjectReference)argumentValue;
        //                resolvedValue = ResolveReference(definition, name, argumentName, roref);
        //            }
        //            else if (argumentValue is ExpressionHolder)
        //            {
        //                ExpressionHolder expHolder = (ExpressionHolder)argumentValue;
        //                object context = null;
        //                IDictionary variables = null;
        //
        //                if (expHolder.Properties != null)
        //                {
        //                    PropertyValue contextProperty = expHolder.Properties.GetPropertyValue("Context");
        //                    context = contextProperty == null
        //                                         ? null
        //                                         : ResolveValueIfNecessary2(name, definition, "Context",
        //                                                                   contextProperty.Value);
        //                    PropertyValue variablesProperty = expHolder.Properties.GetPropertyValue("Variables");
        //                    object vars = (variablesProperty == null
        //                                                   ? null
        //                                                   : ResolveValueIfNecessary2(name, definition, "Variables",
        //                                                                             variablesProperty.Value));
        //                    if (vars is IDictionary)
        //                    {
        //                        variables = (IDictionary)vars;
        //                    }
        //                    else
        //                    {
        //                        if (vars != null) throw new ArgumentException("'Variables' must resolve to an IDictionary");
        //                    }
        //                }
        //
        //                if (variables == null) variables = CollectionsUtil.CreateCaseInsensitiveHashtable();
        //                // add 'this' objectfactory reference to variables
        //                variables.Add(Expression.ReservedVariableNames.CurrentObjectFactory, this);
        //
        //                resolvedValue = expHolder.Expression.GetValue(context, variables);
        //            }
        //            else if (argumentValue is IManagedCollection)
        //            {
        //                resolvedValue =
        //                        ((IManagedCollection)argumentValue).Resolve(name, definition, argumentName,
        //                                                                     new ManagedCollectionElementResolver(ResolveValueIfNecessary2));
        //            }
        //            else if (argumentValue is TypedStringValue)
        //            {
        //                TypedStringValue tsv = (TypedStringValue)argumentValue;
        //                try
        //                {
        //                    Type resolvedTargetType = ResolveTargetType(tsv);
        //                    if (resolvedTargetType != null)
        //                    {
        //                        resolvedValue = TypeConversionUtils.ConvertValueIfNecessary(tsv.TargetType, tsv.Value, null);
        //                    }
        //                    else
        //                    {
        //                        resolvedValue = tsv.Value;
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new ObjectCreationException(definition.ResourceDescription, name,
        //                                                      "Error converted typed String value for " + argumentName, ex);
        //                }
        //
        //            }
        //            else
        //            {
        //                // no need to resolve value...
        //                resolvedValue = argumentValue;
        //            }
        //            return resolvedValue;
        //        }

        ///// <summary>
        ///// Resolve the target type of the passed <see cref="TypedStringValue"/>.
        ///// </summary>
        ///// <param name="value">The <see cref="TypedStringValue"/> who's target type is to be resolved</param>
        ///// <returns>The resolved target type, if any. <see lang="null" /> otherwise.</returns>
        //        protected virtual Type ResolveTargetType(TypedStringValue value)
        //        {
        //            if (value.HasTargetType)
        //            {
        //                return value.TargetType;
        //            }
        //            else
        //            {
        //                return null;
        //            }
        //        }

        ///// <summary>
        ///// Resolves an inner object definition.
        ///// </summary>
        ///// <param name="name">
        ///// The name of the object that surrounds this inner object definition.
        ///// </param>
        ///// <param name="innerObjectName">
        ///// The name of the inner object definition... note: this is a synthetic
        ///// name assigned by the factory (since it makes no sense for inner object
        ///// definitions to have names).
        ///// </param>
        ///// <param name="argumentName">
        ///// The name of the property the value of which is being resolved.
        ///// </param>
        ///// <param name="definition">
        ///// The definition of the inner object that is to be resolved.
        ///// </param>
        ///// <param name="singletonOwner">
        ///// <see langword="true"/> if the owner of the property is a singleton.
        ///// </param>
        ///// <returns>
        ///// The resolved object as defined by the inner object definition.
        ///// </returns>
        //        protected object ResolveInnerObjectDefinition(string name, string innerObjectName, string argumentName, IObjectDefinition definition,
        //                                                      bool singletonOwner)
        //        {
        //            RootObjectDefinition mod = GetMergedObjectDefinition(innerObjectName, definition);
        //            mod.IsSingleton = singletonOwner;
        //            object instance;
        //            object result;
        //            try
        //            {
        //                instance = InstantiateObject(innerObjectName, mod, ObjectUtils.EmptyObjects, false, false);
        //                result = GetObjectForInstance(innerObjectName, instance);
        //            }
        //            catch (ObjectsException ex)
        //            {
        //                throw ObjectCreationException.GetObjectCreationException(ex, name, argumentName, definition.ResourceDescription, innerObjectName);
        //            }
        //            if (singletonOwner && instance is IDisposable)
        //            {
        //                // keep a reference to the inner object instance, to be able to destroy
        //                // it on factory shutdown...
        //                DisposableInnerObjects.Add(instance);
        //            }
        //            return result;
        //        }

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
            if (log.IsDebugEnabled)
            {
                log.Debug(
                        string.Format(CultureInfo.InvariantCulture, "Resolving reference from property '{0}' in object '{1}' to object '{2}'.",
                                      argumentName, name, reference.ObjectName));
            }

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
        ///   The <see cref="System.Type"/> of the objects to look up.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionary"/> of object names and object
        /// instances that match the required <see cref="System.Type"/>, or
        /// <see langword="null"/> if none are found.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        protected abstract IDictionary<string, object> FindMatchingObjects(Type requiredType);

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
        protected abstract IList<string> GetDependingObjectNames(string name);

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
            MarkObjectAsCreated(name);
            RootObjectDefinition definition = GetMergedObjectDefinition(name, true);
            if (definition != null)
            {
                return ConfigureObject(name, definition, new ObjectWrapper(target));
            }

            return target;
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

            if (log.IsDebugEnabled)
            {
                log.Debug($"Configuring object using definition '{name}'");
            }

            PopulateObject(name, definition, wrapper);
            WireEvents(name, definition, wrapper);

            if (ObjectUtils.IsAssignableAndNotTransparentProxy(typeof(IObjectNameAware), instance))
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug($"Setting the name property on the IObjectNameAware object '{name}'.");
                }

                ((IObjectNameAware)instance).ObjectName = name;
            }

            if (ObjectUtils.IsAssignableAndNotTransparentProxy(typeof(IObjectFactoryAware), instance))
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(
                        $"Setting the ObjectFactory property on the IObjectFactoryAware object '{name}'.");
                }

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
                return AutowireConstructor(type.Name, rod, null, null).WrappedInstance;
            }

            object obj = InstantiationStrategy.Instantiate(rod, string.Empty, this);
            PopulateObject(obj.GetType().Name, rod, new ObjectWrapper(obj));
            return obj;
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
            if (log.IsDebugEnabled)
            {
                log.Debug($"Invoking IObjectPostProcessors before initialization of object '{name}'");
            }

            object result = instance;
            for (var i = 0; i < objectPostProcessors.Count; i++)
            {
                IObjectPostProcessor objectProcessor = objectPostProcessors[i];
                result = objectProcessor.PostProcessBeforeInitialization(result, name);
                if (result == null)
                {
                    throw new ObjectCreationException(name,
                        $"PostProcessBeforeInitialization method of IObjectPostProcessor [{objectProcessor}] " +
                        $" returned null for object [{instance}] with name '{name}'.");
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
            if (log.IsDebugEnabled)
            {
                log.Debug($"Invoking IObjectPostProcessors after initialization of object '{name}'");
            }

            object result = instance;
            for (var i = 0; i < objectPostProcessors.Count; i++)
            {
                IObjectPostProcessor objectProcessor = objectPostProcessors[i];
                result = objectProcessor.PostProcessAfterInitialization(result, name);
                if (result == null)
                {
                    throw new ObjectCreationException(name,
                        $"PostProcessAfterInitialization method of IObjectPostProcessor [{objectProcessor}] " +
                        $" returned null for object [{instance}] with name [{name}].");
                }
            }

            return result;
        }

        /// <summary>
        /// Resolve the specified dependency against the objects defined in this factory.
        /// </summary>
        /// <param name="descriptor">The descriptor for the dependency.</param>
        /// <param name="objectName">Name of the object which declares the present dependency.</param>
        /// <param name="autowiredObjectNames">A list that all names of autowired object (used for
        ///     resolving the present dependency) are supposed to be added to.</param>
        /// <returns>
        /// the resolved object, or <code>null</code> if none found
        /// </returns>
        /// <exception cref="ObjectsException">if dependency resolution failed</exception>
        public abstract object ResolveDependency(
            DependencyDescriptor descriptor,
            string objectName,
            IList<string> autowiredObjectNames);
    }

    internal class UnsatisfiedDependencyExceptionData
    {
        public UnsatisfiedDependencyExceptionData(int parameterIndex, Type parameterType, string errorMessage)
        {
            ParameterIndex = parameterIndex;
            ParameterType = parameterType;
            ErrorMessage = errorMessage;
        }

        public int ParameterIndex { get; }

        public Type ParameterType { get; }

        public string ErrorMessage { get; }
    }
}
