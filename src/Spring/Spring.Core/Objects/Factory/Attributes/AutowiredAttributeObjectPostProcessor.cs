﻿#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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

using System.Reflection;
using Spring.Collections;
using Spring.Core;
using Spring.Objects.Factory.Config;

using Common.Logging;

using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Objects.Factory.Attributes
{
    /// <summary>
    /// <see cref="IInstantiationAwareObjectPostProcessor"/> implementation
    /// that autowires annotated fields, properties and arbitrary config methods.
    /// Such members to be injected are detected through an attribute: by default,
    /// Spring's <see cref="AutowiredAttribute"/>.
    ///
    /// Only one constructor (at max) of any given bean class may carry this
    /// annotation with the 'required' parameter set to <code>true</code>,
    /// indicating <i>the</i> constructor to autowire when used as a Spring bean.
    /// If multiple <i>non-required</i> constructors carry the annotation, they
    /// will be considered as candidates for autowiring. The constructor with
    /// the greatest number of dependencies that can be satisfied by matching
    /// beans in the Spring container will be chosen. If none of the candidates
    /// can be satisfied, then a default constructor (if present) will be used.
    /// An annotated constructor does not have to be public.
    ///
    /// Fields are injected right after construction of a bean, before any
    /// config methods are invoked. Such a config field does not have to be public.
    ///
    /// Config methods may have an arbitrary name and any number of arguments; each of
    /// those arguments will be autowired with a matching bean in the Spring container.
    /// Bean property setter methods are effectively just a special case of such a
    /// general config method. Config methods do not have to be public.
    ///
    /// Note: A default AutowiredAttributeObjectPostProcessor will be registered
    /// by the "context:annotation-config" and "context:component-scan" XML tags.
    /// Remove or turn off the default annotation configuration there if you intend
    /// to specify a custom AutowiredAnnotationBeanPostProcessor bean definition.
    /// <b>NOTE:</b> Annotation injection will be performed <i>before</i> XML injection;
    /// thus the latter configuration will override the former for properties wired through
    /// both approaches.
    /// </summary>
    public class AutowiredAttributeObjectPostProcessor
        : InstantiationAwareObjectPostProcessorAdapter, IMergedObjectDefinitionPostProcessor, IObjectFactoryAware, IOrdered
    {
        private static readonly ILog logger = LogManager.GetLogger<AutowiredAttributeObjectPostProcessor>();

        private IConfigurableListableObjectFactory objectFactory;

        private readonly HashSet<string> lookupMethodsChecked = new HashSet<string>();

        private readonly SynchronizedHashtable candidateConstructorsCache = new SynchronizedHashtable();

        private readonly Dictionary<string, InjectionMetadata> injectionMetadataCache = new Dictionary<string, InjectionMetadata>();

        private readonly List<Type> autowiredPropertyTypes = new List<Type>();

        /// <summary>
        /// Return the order value of this object, where a higher value means greater in
        ///             terms of sorting.
        /// </summary>
        /// <remarks>
        /// <p>Normally starting with 0 or 1, with <see cref="F:System.Int32.MaxValue"/> indicating
        ///             greatest. Same order values will result in arbitrary positions for the affected
        ///             objects.
        ///             </p><p>Higher value can be interpreted as lower priority, consequently the first object
        ///             has highest priority.
        ///             </p>
        /// </remarks>
        /// <returns>
        /// The order value.
        /// </returns>
        public int Order { get; private set; } = int.MaxValue - 2;

        /// <summary>
        /// Callback that supplies the owning factory to an object instance.
        /// </summary>
        /// <value>
        /// Owning <see cref="T:Spring.Objects.Factory.IObjectFactory"/>
        ///             (may not be <see langword="null"/>). The object can immediately
        ///             call methods on the factory.
        /// </value>
        /// <remarks>
        /// <p>Invoked after population of normal object properties but before an init
        ///             callback like <see cref="T:Spring.Objects.Factory.IInitializingObject"/>'s
        ///             <see cref="M:Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        ///             method or a custom init-method.
        ///             </p>
        /// </remarks>
        /// <exception cref="T:Spring.Objects.ObjectsException">In case of initialization errors.
        ///             </exception>
        public IObjectFactory ObjectFactory
        {
            set => objectFactory = (IConfigurableListableObjectFactory) value;
        }

        public void PostProcessMergedObjectDefinition(RootObjectDefinition objectDefinition, Type objectType, string objectName)
        {
            var metadata = FindAutowiringMetadata(objectName, objectType, null);
            metadata.CheckConfigMembers(objectDefinition);
        }

        public void ResetObjectDefinition(string beanName)
        {
            this.lookupMethodsChecked.Remove(beanName);
            injectionMetadataCache.Remove(beanName);
        }

        /// <summary>
        /// Add a Autowired Attribute Type
        /// </summary>
        public void AddAutowiredType(Type attributeType)
        {
            if (!autowiredPropertyTypes.Contains(attributeType))
            {
                autowiredPropertyTypes.Add(attributeType);
            }
        }

        /// <summary>
        /// Create a new instance of an Autowire Post Processor
        /// with standard attributes of <see cref="AutowiredAttribute"/>
        /// and <see cref="ValueAttribute"/>
        /// </summary>
        public AutowiredAttributeObjectPostProcessor()
        {
            autowiredPropertyTypes.Add(typeof (AutowiredAttribute));
            autowiredPropertyTypes.Add(typeof (ValueAttribute));
        }

        /// <summary>
        /// Determines the candidate constructors to use for the given object.
        /// </summary>
        /// <param name="objectType">The raw Type of the object.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>The candidate constructors, or <code>null</code> if none specified</returns>
        /// <exception cref="ObjectsException">in case of errors</exception>
        public override ConstructorInfo[] DetermineCandidateConstructors(Type objectType, string objectName)
        {
            /* TODO implement Lookup
            // Let's check for lookup methods here..
            if (!lookupMethodsChecked.Contains(objectName))
            {
                try
                {
                    foreach (var method in objectType.GetMethods())
                    {
                        ValueAttribute lookup = method.GetCustomAttribute<ValueAttribute>(true);
                        if (lookup != null)
                        {
                            AssertUtils.State(objectFactory != null, "No ObjectFactory available");
                            var lookupMethodOverride = new LookupMethodOverride(method.Name, lookup.Expression);
                            try
                            {
                                var mbd = (RootObjectDefinition) objectFactory.GetMergedObjectDefinition(objectName);
                                mbd.MethodOverrides.Add(lookupMethodOverride);
                            }
                            catch (NoSuchObjectDefinitionException)
                            {
                                throw new ObjectCreationException(objectName,
                                    "Cannot apply @Lookup to objects without corresponding object definition");
                            }
                        }
                    };
                }
                catch (Exception ex)
                {
                    throw new ObjectCreationException(objectName, "Lookup method resolution failed", ex);
                }

                lookupMethodsChecked.Add(objectName);
            }*/

            // Quick check on the concurrent map first, with minimal locking.
            ConstructorInfo[] candidateConstructors = candidateConstructorsCache.ContainsKey(objectType)
                ? (ConstructorInfo[]) candidateConstructorsCache[objectType]
                : null;
            if (candidateConstructors == null)
            {
                lock (candidateConstructorsCache)
                {
                    candidateConstructors = candidateConstructorsCache.ContainsKey(objectType)
                        ? (ConstructorInfo[]) candidateConstructorsCache[objectType]
                        : null;
                    if (candidateConstructors == null)
                    {
                        ConstructorInfo[] rawCandidates = objectType.GetConstructors();
                        IList<ConstructorInfo> candidates = new List<ConstructorInfo>(rawCandidates.Length);
                        ConstructorInfo requiredConstructor = null;
                        ConstructorInfo defaultConstructor = null;
                        foreach (var candidate in rawCandidates)
                        {
                            if (Attribute.GetCustomAttribute(candidate, typeof (AutowiredAttribute)) is AutowiredAttribute attr)
                            {
                                if (requiredConstructor != null)
                                {
                                    throw new ObjectCreationException("Invalid autowire-marked constructor: " + candidate +
                                                                      ". Found another constructor with 'required' Autowired annotation: " +
                                                                      requiredConstructor);
                                }
                                if (candidate.GetParameters().Length == 0)
                                {
                                    throw new InvalidOperationException("Autowired annotation requires at least one argument: " + candidate);
                                }
                                if (attr.Required)
                                {
                                    if (candidates.Count > 0)
                                    {
                                        throw new ObjectCreationException(
                                            "Invalid autowire-marked constructors: " + candidates +
                                            ". Found another constructor with 'required' Autowired annotation: " +
                                            requiredConstructor);
                                    }
                                    requiredConstructor = candidate;
                                }
                                candidates.Add(candidate);
                            }
                            else if (candidate.GetParameters().Length == 0)
                            {
                                defaultConstructor = candidate;
                            }
                        }
                        if (candidates.Count > 0)
                        {
                            // Add default constructor to list of optional constructors, as fallback.
                            if (requiredConstructor == null && defaultConstructor != null)
                            {
                                candidates.Add(defaultConstructor);
                            }
                            candidateConstructors = candidates.ToArray();
                        }
                        else
                        {
                            candidateConstructors = new ConstructorInfo[0];
                        }
                        candidateConstructorsCache.Add(objectType, candidateConstructors);
                    }
                }
            }
            return (candidateConstructors.Length > 0 ? candidateConstructors : null);
        }

        /// <summary>
        /// Finds autowire candidates and verifies them
        /// </summary>
        /// <param name="objectType">
        /// The <see cref="System.Type"/> of the target object that is to be
        /// instantiated.
        /// </param>
        /// <param name="objectName">
        /// The name of the target object.
        /// </param>
        /// <returns>
        /// The object to expose instead of a default instance of the target
        /// object.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of any errors.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.HasObjectType"/>
        /// <seealso cref="Spring.Objects.Factory.Support.IConfigurableObjectDefinition.FactoryMethodName"/>
        public override object PostProcessBeforeInstantiation(Type objectType, string objectName)
        {
            var objectDefinition = objectFactory.GetObjectDefinition(objectName) as RootObjectDefinition;
            if (objectType != null)
            {
                var metadata = FindAutowiringMetadata(objectName, objectType, null);
                metadata.CheckConfigMembers(objectDefinition);
            }
            return null;
        }

        /// <summary>
        /// Injects autoried annotated properties, fields, methods into objectInstance
        /// </summary>
        /// <param name="pvs"></param>
        /// <param name="pis"></param>
        /// <param name="objectInstance"></param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>The actual property values to apply to the given object (can be the
        /// passed-in PropertyValues instances0 or null to skip property population.</returns>
        public override IPropertyValues PostProcessPropertyValues(IPropertyValues pvs, IList<PropertyInfo> pis,
            object objectInstance, string objectName)
        {
            var metadata = FindAutowiringMetadata(objectName, objectInstance.GetType(), pvs);
            try
            {
                metadata.Inject(objectInstance, objectName, pvs);
            }
            catch (Exception ex)
            {
                throw new ObjectCreationException(objectName, "Injection of autowired dependencies failed", ex);
            }
            return pvs;
        }

        private InjectionMetadata FindAutowiringMetadata(string objectName, Type objectType, IPropertyValues pvs)
        {
            // Fall back to class name as cache key, for backwards compatibility with custom callers.
            var cacheKey = StringUtils.HasLength(objectName) ? objectName : objectType.Name;
            injectionMetadataCache.TryGetValue(cacheKey, out var metadata);

            if (InjectionMetadata.NeedsRefresh(metadata, objectType))
            {
                lock (injectionMetadataCache)
                {
                    if (!injectionMetadataCache.TryGetValue(cacheKey, out metadata)
                        || InjectionMetadata.NeedsRefresh(metadata, objectType))
                    {
                        metadata = BuildAutowiringMetadata(objectType);
                        injectionMetadataCache[cacheKey] = metadata;
                    }
                }
            }

            return metadata;
        }

        private InjectionMetadata BuildAutowiringMetadata(Type objectType)
        {
            var elements = new List<InjectionMetadata.InjectedElement>();

            do
            {
                for (var i = 0; i < autowiredPropertyTypes.Count; i++)
                {
                    var autowiredType = autowiredPropertyTypes[i];
                    var currElements = new List<InjectionMetadata.InjectedElement>();
                    var properties =
                        objectType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                    foreach (var property in properties)
                    {
                        var required = true;
                        var attr = Attribute.GetCustomAttribute(property, autowiredType);
                        if (attr is AutowiredAttribute autowiredAttribute)
                        {
                            required = autowiredAttribute.Required;
                        }

                        if (attr != null && property.DeclaringType == objectType)
                        {
                            currElements.Add(new AutowiredPropertyElement(this, property, required));
                        }
                    }

                    var fields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    foreach (var field in fields)
                    {
                        var required = true;
                        var attr = Attribute.GetCustomAttribute(field, autowiredType);
                        if (attr is AutowiredAttribute autowiredAttribute)
                        {
                            required = autowiredAttribute.Required;
                        }

                        if (attr != null && field.DeclaringType == objectType)
                        {
                            currElements.Add(new AutowiredFieldElement(this, field, required));
                        }
                    }

                    var methods = objectType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    foreach (var method in methods)
                    {
                        var required = true;
                        var attr = Attribute.GetCustomAttribute(method, autowiredType);
                        if (attr is AutowiredAttribute autowiredAttribute)
                        {
                            required = autowiredAttribute.Required;
                        }

                        if (attr != null && method.DeclaringType == objectType)
                        {
                            if (method.IsStatic)
                            {
                                logger.Warn(m => m("Autowired annotation is not supported on static methods: " + method.Name));
                                continue;
                            }

                            if (method.IsGenericMethod)
                            {
                                logger.Warn(m => m("Autowired annotation is not supported on generic methods: " + method.Name));
                                continue;
                            }

                            currElements.Add(new AutowiredMethodElement(this, method, required));
                        }
                    }

                    elements.InsertRange(0, currElements);
                }

                objectType = objectType.BaseType;
            } while (objectType != null && objectType != typeof (object));

            return new InjectionMetadata(objectType, elements);
        }

        /// <summary>
        /// Register the specified bean as dependent on the autowired beans.
        /// </summary>
        private void RegisterDependentObjects(string objectName, List<string> autowiredObjectNames)
        {
            if (objectName == null)
            {
                return;
            }

            var objectDefinition = objectFactory.GetObjectDefinition(objectName) as RootObjectDefinition;
            if (objectDefinition == null)
            {
                return;
            }

            var dependsOn = new List<string>(objectDefinition.DependsOn);
            foreach (var name in autowiredObjectNames)
            {
                var autowiredObjectName = name;
                if (!dependsOn.Contains(autowiredObjectName))
                {
                    dependsOn.Add(autowiredObjectName);
                    logger.Debug(m => m("Autowiring by type from object name '{0}' to object named '{1}'", objectName, autowiredObjectName));
                }
            }
            objectDefinition.DependsOn = dependsOn;
        }

        private object ResolvedCachedArgument(string objectName, object cachedArgument)
        {
            if (cachedArgument is DependencyDescriptor descriptor)
            {
                return objectFactory.ResolveDependency(descriptor, objectName, null);
            }

            if (cachedArgument is RuntimeObjectReference runtimeObjectReference)
            {
                return objectFactory.GetObject(runtimeObjectReference.ObjectName);
            }

            return cachedArgument;
        }

        /// <summary>
        /// Class representing injection information about an annotated field.
        /// </summary>
        private class AutowiredPropertyElement : InjectionMetadata.InjectedElement
        {
            private readonly AutowiredAttributeObjectPostProcessor processor;
            private readonly bool required;
            private bool cached;
            private object cachedFieldValue;

            public AutowiredPropertyElement(AutowiredAttributeObjectPostProcessor processor, PropertyInfo property, bool required)
                : base(property)
            {
                this.processor = processor;
                this.required = required;
            }

            public override void Inject(object instance, string objectName, IPropertyValues pvs)
            {
                var property = (PropertyInfo) Member;
                try
                {
                    object value;
                    if (cached)
                    {
                        value = processor.ResolvedCachedArgument(objectName, cachedFieldValue);
                    }
                    else
                    {
                        var descriptor = new DependencyDescriptor(property, required);
                        var autowiredObjectNames = new List<string>();
                        value = processor.objectFactory.ResolveDependency(descriptor, objectName, autowiredObjectNames);
                        lock (this)
                        {
                            if (!cached)
                            {
                                if (value != null || required)
                                {
                                    cachedFieldValue = descriptor;
                                    processor.RegisterDependentObjects(objectName, autowiredObjectNames);
                                    if (autowiredObjectNames.Count == 1)
                                    {
                                        var autowiredBeanName = autowiredObjectNames[0];
                                        if (processor.objectFactory.ContainsObject(autowiredBeanName))
                                        {
                                            if (processor.objectFactory.IsTypeMatch(autowiredBeanName, property.GetType()))
                                            {
                                                cachedFieldValue = new RuntimeObjectReference(autowiredBeanName);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    cachedFieldValue = null;
                                }
                                cached = true;
                            }
                        }
                    }
                    if (value != null)
                    {
                        property.SetValue(instance, value, null);
                    }
                }
                catch (Exception ex)
                {
                    throw new ObjectCreationException("Could not autowire property: " + property, ex);
                }
            }
        }

        /// <summary>
        /// Class representing injection information about an annotated field.
        /// </summary>
        private class AutowiredFieldElement : InjectionMetadata.InjectedElement
        {
            private readonly AutowiredAttributeObjectPostProcessor processor;
            private readonly bool required;
            private bool cached;
            private object cachedFieldValue;

            public AutowiredFieldElement(AutowiredAttributeObjectPostProcessor processor, FieldInfo field, bool required)
                : base(field)
            {
                this.processor = processor;
                this.required = required;
            }

            public override void Inject(object instance, string objectName, IPropertyValues pvs)
            {
                var field = (FieldInfo) Member;
                try
                {
                    object value;
                    if (cached)
                    {
                        value = processor.ResolvedCachedArgument(objectName, cachedFieldValue);
                    }
                    else
                    {
                        var descriptor = new DependencyDescriptor(field, required);
                        var autowiredObjectNames = new List<string>();
                        value = processor.objectFactory.ResolveDependency(descriptor, objectName, autowiredObjectNames);
                        lock (this)
                        {
                            if (!cached)
                            {
                                if (value != null || required)
                                {
                                    cachedFieldValue = descriptor;
                                    processor.RegisterDependentObjects(objectName, autowiredObjectNames);
                                    if (autowiredObjectNames.Count == 1)
                                    {
                                        var autowiredBeanName = autowiredObjectNames[0] as string;
                                        if (processor.objectFactory.ContainsObject(autowiredBeanName))
                                        {
                                            if (processor.objectFactory.IsTypeMatch(autowiredBeanName, field.GetType()))
                                            {
                                                cachedFieldValue = new RuntimeObjectReference(autowiredBeanName);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    cachedFieldValue = null;
                                }
                                cached = true;
                            }
                        }
                    }
                    if (value != null)
                    {
                        field.SetValue(instance, value);
                    }
                }
                catch (Exception ex)
                {
                    throw new ObjectCreationException("Could not autowire field: " + field, ex);
                }
            }
        }

        ///
        /// Class representing injection information about an annotated method.
        ///
        private class AutowiredMethodElement : InjectionMetadata.InjectedElement
        {
            private readonly AutowiredAttributeObjectPostProcessor processor;
            private readonly bool required;
            private bool cached;
            private volatile object[] cachedMethodArguments;

            public AutowiredMethodElement(AutowiredAttributeObjectPostProcessor processor, MethodInfo method, bool required)
                : base(method)
            {
                this.processor = processor;
                this.required = required;
            }

            public override void Inject(object target, string objectName, IPropertyValues pvs)
            {
                MethodInfo method = Member as MethodInfo;
                try
                {
                    object[] arguments;
                    if (cached)
                    {
                        arguments = ResolveCachedArguments(objectName);
                    }
                    else
                    {
                        Type[] paramTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                        arguments = new object[paramTypes.Length];
                        var descriptors = new DependencyDescriptor[paramTypes.Length];
                        var autowiredBeanNames = new List<string>();
                        for (int i = 0; i < arguments.Length; i++)
                        {
                            MethodParameter methodParam = new MethodParameter(method, i);
                            descriptors[i] = new DependencyDescriptor(methodParam, required);
                            arguments[i] = processor.objectFactory.ResolveDependency(descriptors[i], objectName, autowiredBeanNames);
                            if (arguments[i] == null && !required)
                            {
                                arguments = null;
                                break;
                            }
                        }
                        lock (this)
                        {
                            if (!cached)
                            {
                                if (arguments != null)
                                {
                                    cachedMethodArguments = new object[arguments.Length];
                                    for (int i = 0; i < arguments.Length; i++)
                                    {
                                        cachedMethodArguments[i] = descriptors[i];
                                    }
                                    processor.RegisterDependentObjects(objectName, autowiredBeanNames);
                                    if (autowiredBeanNames.Count == paramTypes.Length)
                                    {
                                        for (int i = 0; i < paramTypes.Length; i++)
                                        {
                                            string autowiredBeanName = autowiredBeanNames[i] as string;
                                            if (processor.objectFactory.ContainsObject(autowiredBeanName))
                                            {
                                                if (processor.objectFactory.IsTypeMatch(autowiredBeanName, paramTypes[i]))
                                                {
                                                    cachedMethodArguments[i] =
                                                        new RuntimeObjectReference(autowiredBeanName);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    cachedMethodArguments = null;
                                }
                                cached = true;
                            }
                        }
                    }
                    if (arguments != null)
                    {
                        method.Invoke(target, arguments);
                    }
                }
                catch (Exception ex)
                {
                    throw new ObjectCreationException("Could not autowire method: " + method, ex);
                }
            }

            private object[] ResolveCachedArguments(string objectName)
            {
                if (cachedMethodArguments == null)
                {
                    return null;
                }
                object[] arguments = new object[cachedMethodArguments.Length];
                for (int i = 0; i < arguments.Length; i++)
                {
                    arguments[i] = processor.ResolvedCachedArgument(objectName, cachedMethodArguments[i]);
                }
                return arguments;
            }
        }
    }
}
