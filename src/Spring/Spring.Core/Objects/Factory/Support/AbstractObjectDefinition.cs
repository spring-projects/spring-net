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

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Spring.Core.TypeResolution;
using Spring.Objects.Factory.Config;
using Spring.Util;
using Spring.Collections.Generic;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Common base class for object definitions, factoring out common
    /// functionality from
    /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/> and
    /// <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public abstract class AbstractObjectDefinition : ObjectMetadataAttributeAccessor, IConfigurableObjectDefinition, ISerializable
    {
        private const string ScopeSingleton = "singleton";
        private const string ScopePrototype = "prototype";

        private ConstructorArgumentValues constructorArgumentValues = new ConstructorArgumentValues();
        private MutablePropertyValues propertyValues = new MutablePropertyValues();
        private EventValues eventHandlerValues = new EventValues();
        private MethodOverrides methodOverrides = new MethodOverrides();
        private string resourceDescription;
        private bool isSingleton = true;
        private bool isPrototype;
        private bool isLazyInit;
        private bool isAbstract;
        private string scope = ScopeSingleton;
        private ObjectRole role = ObjectRole.ROLE_APPLICATION;
        
        private string objectTypeName;
        private Type objectType;
        
        private AutoWiringMode autowireMode = AutoWiringMode.No;
        private DependencyCheckingMode dependencyCheck = DependencyCheckingMode.None;
        internal List<string> dependsOn;
        private bool autowireCandidate = true;
        private bool primary;
        private Dictionary<string, AutowireCandidateQualifier> qualifiers;
        private string initMethodName;
        private string destroyMethodName;
        private string factoryMethodName;
        private string factoryObjectName;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes no
        /// public constructors.
        /// </p>
        /// </remarks>
        protected AbstractObjectDefinition() : this(null, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes no
        /// public constructors.
        /// </p>
        /// </remarks>
        protected AbstractObjectDefinition(ConstructorArgumentValues arguments, MutablePropertyValues properties)
        {
            constructorArgumentValues = arguments ?? constructorArgumentValues ?? new ConstructorArgumentValues();
            propertyValues = properties ?? propertyValues ?? new MutablePropertyValues();
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition"/>
        /// class.
        /// </summary>
        /// <param name="other">
        /// The object definition used to initialise the member fields of this
        /// instance.
        /// </param>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes no
        /// public constructors.
        /// </p>
        /// </remarks>
        protected AbstractObjectDefinition(IObjectDefinition other)
        {
            AssertUtils.ArgumentNotNull(other, "other");
            OverrideFrom(other);

            AbstractObjectDefinition aod = other as AbstractObjectDefinition;
            if (aod != null)
            {
                if (aod.HasObjectType)
                {
                    ObjectType = other.ObjectType;
                }
                else
                {
                    ObjectTypeName = other.ObjectTypeName;
                }
                MethodOverrides = new MethodOverrides(aod.MethodOverrides);
                DependencyCheck = aod.DependencyCheck;
            }
            ParentName = other.ParentName;
            IsAbstract = other.IsAbstract;
//            IsSingleton = other.IsSingleton;
            Scope = other.Scope;
            Role = other.Role;
            IsLazyInit = other.IsLazyInit;
            ConstructorArgumentValues
                = new ConstructorArgumentValues(other.ConstructorArgumentValues);
            PropertyValues = new MutablePropertyValues(other.PropertyValues);
            EventHandlerValues = new EventValues(other.EventHandlerValues);

            InitMethodName = other.InitMethodName;
            DestroyMethodName = other.DestroyMethodName;
            IsAutowireCandidate = other.IsAutowireCandidate;
            IsPrimary = other.IsPrimary;
            CopyQualifiersFrom(aod);
            if (other.DependsOn.Count > 0)
            {
                DependsOn = other.DependsOn;
            }
            FactoryMethodName = other.FactoryMethodName;
            FactoryObjectName = other.FactoryObjectName;
            AutowireMode = other.AutowireMode;
            ResourceDescription = other.ResourceDescription;
        }

        /// <summary>
        /// The name of the parent definition of this object definition, if any.
        /// </summary>
        public abstract string ParentName { get; set; }

        /// <summary>
        /// The property values that are to be applied to the object
        /// upon creation.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Setting the value of this property to <see langword="null"/>
        /// will merely result in a new (and empty)
        /// <see cref="Spring.Objects.MutablePropertyValues"/>
        /// collection being assigned to the property value.
        /// </p>
        /// </remarks>
        /// <value>
        /// The property values (if any) for this object; may be an
        /// empty collection but is guaranteed not to be
        /// <see langword="null"/>.
        /// </value>
        public MutablePropertyValues PropertyValues
        {
            get => propertyValues;
            set => propertyValues = value ?? new MutablePropertyValues();
        }

        /// <summary>
        /// Does this definition have any
        /// <see cref="Spring.Objects.Factory.Support.MethodOverrides"/>?
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this definition has at least one
        /// <see cref="Spring.Objects.Factory.Support.MethodOverride"/>.
        /// </value>
        public bool HasMethodOverrides => !MethodOverrides.IsEmpty;

        /// <summary>
        /// The constructor argument values for this object.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Setting the value of this property to <see langword="null"/>
        /// will merely result in a new (and empty)
        /// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
        /// collection being assigned.
        /// </p>
        /// </remarks>
        /// <value>
        /// The constructor argument values (if any) for this object; may be an
        /// empty collection but is guaranteed not to be
        /// <see langword="null"/>.
        /// </value>
        public ConstructorArgumentValues ConstructorArgumentValues
        {
            get => constructorArgumentValues;
            set => constructorArgumentValues = value ?? new ConstructorArgumentValues();
        }

        /// <summary>
        /// The event handler values for this object.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Setting the value of this property to <see langword="null"/>
        /// will merely result in a new (and empty)
        /// <see cref="Spring.Objects.Factory.Config.EventValues"/>
        /// collection being assigned.
        /// </p>
        /// </remarks>
        /// <value>
        /// The event handler values (if any) for this object; may be an
        /// empty collection but is guaranteed not to be
        /// <see langword="null"/>.
        /// </value>
        public EventValues EventHandlerValues
        {
            get => eventHandlerValues;
            set => eventHandlerValues = value ?? new EventValues();
        }

        /// <summary>
        /// The method overrides (if any) for this object.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Setting the value of this property to <see langword="null"/>
        /// will merely result in a new (and empty)
        /// <see cref="Spring.Objects.Factory.Support.MethodOverrides"/>
        /// collection being assigned to the property value.
        /// </p>
        /// </remarks>
        /// <value>
        /// The method overrides (if any) for this object; may be an
        /// empty collection but is guaranteed not to be
        /// <see langword="null"/>.
        /// </value>
        public MethodOverrides MethodOverrides
        {
            get => methodOverrides;
            set => methodOverrides = value ?? new MethodOverrides();
        }

        /// <summary>
        /// The name of the target scope for the object.
        /// Defaults to "singleton", ootb alternative is "prototype". Extended object factories
        /// might support further scopes.
        /// </summary>
        public virtual string Scope
        {
            get => scope;
            set
            {
                AssertUtils.ArgumentNotNull(value, "Scope");
                scope = value;
                isPrototype = 0 == string.Compare(ScopePrototype, value, StringComparison.OrdinalIgnoreCase);
                isSingleton = !isPrototype; // 0 == string.Compare(SCOPE_SINGLETON, value, true);
            }
        }

        /// <summary>
        /// Get or set the role hint for this object definition
        /// </summary>
        public virtual ObjectRole Role
        {
            get => role;
            set => role = value;
        }

        /// <summary>
        /// Is this definition a <b>singleton</b>, with
        /// a single, shared instance returned on all calls to an enclosing
        /// container (typically an
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/> or
        /// <see cref="Spring.Context.IApplicationContext"/>).
        /// </summary>
        /// <remarks>
        /// <p>
        /// If <see langword="false"/>, an object factory will apply the
        /// <b>prototype</b> design pattern, with each caller requesting an
        /// instance getting an independent instance. How this is defined
        /// will depend on the object factory implementation. <b>singletons</b>
        /// are the commoner type.
        /// </p>
        /// </remarks>
        /// <seealso cref="Spring.Objects.Factory.IObjectFactory"/>
        public virtual bool IsSingleton
        {
            get => isSingleton;
            set
            {
                scope = (value ? ScopeSingleton : ScopePrototype);
                isSingleton = value;
                isPrototype = !value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is prototype, with an independent instance
        /// returned for each call.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is prototype; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsPrototype => isPrototype;

        /// <summary>
        /// Is this object lazily initialized?</summary>
        /// <remarks>
        /// <p>
        /// Only applicable to a singleton object.
        /// </p>
        /// <p>
        /// If <see langword="false"/>, it will get instantiated on startup
        /// by object factories that perform eager initialization of
        /// singletons.
        /// </p>
        /// </remarks>
        public bool IsLazyInit
        {
            get => isLazyInit;
            set => isLazyInit = value;
        }

        /// <summary>
        /// Is this object definition a "template", i.e. not meant to be instantiated
        /// itself but rather just serving as an object definition for configuration 
        /// templates used by <see cref="Spring.Objects.Factory.IObjectFactory.ConfigureObject(object, string)"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this object definition is a "template".
        /// </value>
        public bool IsTemplate => isAbstract || (objectType == null && StringUtils.IsNullOrEmpty(factoryObjectName));

        /// <summary>
        /// Is this object definition "abstract", i.e. not meant to be
        /// instantiated itself but rather just serving as a parent for concrete
        /// child object definitions.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this object definition is "abstract".
        /// </value>
        public bool IsAbstract
        {
            get => isAbstract;
            set => isAbstract = value;
        }

        /// <summary>
        /// The <see cref="System.Type"/> of the object definition (if any).
        /// </summary>
        /// <value>
        /// A resolved object <see cref="System.Type"/>.
        /// </value>
        /// <exception cref="ApplicationException">
        /// If the <see cref="System.Type"/> of the object definition is not a
        /// resolved <see cref="System.Type"/> or <see langword="null"/>.
        /// </exception>
        /// <seealso cref="AbstractObjectDefinition.HasObjectType"/>
        public Type ObjectType
        {
            get
            {
                if (objectType == null)
                {
                    ThrowApplicationException("Object definition does not carry a resolved System.Type");
                    return null;
                }
                return objectType;
            }
            set => objectType = value;
        }

        private static void ThrowApplicationException(string message)
        {
            throw new ApplicationException(message);
        }

        /// <summary>
        /// Is the <see cref="System.Type"/> of the object definition a resolved
        /// <see cref="System.Type"/>?
        /// </summary>
        public bool HasObjectType => objectType != null;

        /// <summary>
        /// Returns the <see cref="System.Type.FullName"/> of the
        /// <see cref="System.Type"/> of the object definition (if any).
        /// </summary>
        public string ObjectTypeName
        {
            get => objectTypeName ?? objectType?.FullName;
            set => objectTypeName = StringUtils.GetTextOrNull(value);
        }

        /// <summary>
        /// A description of the resource that this object definition
        /// came from (for the purpose of showing context in case of errors).
        /// </summary>
        public string ResourceDescription
        {
            get => resourceDescription;
            set => resourceDescription = StringUtils.GetTextOrNull(value);
        }

        /// <summary>
        /// The autowire mode as specified in the object definition.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This determines whether any automagical detection and setting of
        /// object references will happen. The default is
        /// <see cref="Spring.Objects.Factory.Config.AutoWiringMode.No"/>,
        /// which means that no autowiring will be performed.
        /// </p>
        /// </remarks>
        public AutoWiringMode AutowireMode
        {
            get => autowireMode;
            set => autowireMode = value;
        }

        /// <summary>
        /// Gets the resolved autowire mode.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This resolves
        /// <see cref="Spring.Objects.Factory.Config.AutoWiringMode.AutoDetect"/>
        /// to one of 
        /// <see cref="Spring.Objects.Factory.Config.AutoWiringMode.Constructor"/>
        /// or
        /// <see cref="Spring.Objects.Factory.Config.AutoWiringMode.ByType"/>.
        /// </p>
        /// </remarks>
        public AutoWiringMode ResolvedAutowireMode
        {
            get
            {
                if (AutowireMode == AutoWiringMode.AutoDetect)
                {
                    // Work out whether to apply setter autowiring or constructor autowiring.
                    // If it has a no-arg constructor it's deemed to be setter autowiring,
                    // otherwise we'll try constructor autowiring.
                    ConstructorInfo[] constructors =
                        ObjectType.GetConstructors();
                    foreach (ConstructorInfo ctor in constructors)
                    {
                        if (ctor.GetParameters().Length == 0)
                        {
                            return AutoWiringMode.ByType;
                        }
                    }
                    return AutoWiringMode.Constructor;
                }
                else
                {
                    return AutowireMode;
                }
            }
        }

        /// <summary>
        /// The dependency checking mode.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default is
        /// <see cref="Spring.Objects.Factory.Support.DependencyCheckingMode.None"/>.
        /// </p>
        /// </remarks>
        public DependencyCheckingMode DependencyCheck
        {
            get => dependencyCheck;
            set => dependencyCheck = value;
        }

        /// <summary>
        /// The object names that this object depends on.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The object factory will guarantee that these objects get initialized
        /// before this object definition.
        /// </p>
        /// <note>
        /// Dependencies are normally expressed through object properties
        /// or constructor arguments. This property should just be necessary for
        /// other kinds of dependencies such as statics (*ugh*) or database
        /// preparation on startup.
        /// </note>
        /// </remarks>
        public IReadOnlyList<string> DependsOn
        {
            get => dependsOn ?? StringUtils.EmptyStringsList;
            set => dependsOn = value != null && value.Count > 0 ? new List<string>(value) : null;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance a candidate for getting autowired into some other
        /// object.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is autowire candidate; otherwise, <c>false</c>.
        /// </value>
        public bool IsAutowireCandidate
        {
            get => autowireCandidate;
            set => autowireCandidate = value;
        }


        /// <summary>
        /// Set whether this bean is a primary autowire candidate.
        /// If this value is true for exactly one bean among multiple
        /// matching candidates, it will serve as a tie-breaker.
        /// </summary>
        public bool IsPrimary 
        { 
            get => primary;
            set => primary = value;
        }

        /// <summary>
        /// Register a qualifier to be used for autowire candidate resolution,
        /// keyed by the qualifier's type name.
        /// <see cref="AutowireCandidateQualifier"/>
        /// </summary>
        public void AddQualifier(AutowireCandidateQualifier qualifier)
        {
            qualifiers = qualifiers ?? new Dictionary<string, AutowireCandidateQualifier>();
            qualifiers.Add(qualifier.TypeName, qualifier);
        }

        /// <summary>
        /// Return whether this bean has the specified qualifier.
        /// </summary>
        public bool HasQualifier(string typeName)
        {
            return qualifiers != null && qualifiers.ContainsKey(typeName);
        }

        /// <summary>
        /// Return the qualifier mapped to the provided type name.
        /// </summary>
        public AutowireCandidateQualifier GetQualifier(string typeName)
        {
            if (qualifiers != null && qualifiers.TryGetValue(typeName, out var qualifier))
            {
                return qualifier;
            }

            return null;
        }

        /// <summary>
        /// Return all registered qualifiers.
        /// </summary>
        /// <returns>the Set of <see cref="AutowireCandidateQualifier"/> objects.</returns>
        public Set<AutowireCandidateQualifier> GetQualifiers()
        {
            return qualifiers != null
                ? new OrderedSet<AutowireCandidateQualifier>(qualifiers.Values)
                : new OrderedSet<AutowireCandidateQualifier>();
        }

        /// <summary>
        /// Copy the qualifiers from the supplied AbstractBeanDefinition to this bean definition.
        /// </summary>
        /// <param name="source">the AbstractBeanDefinition to copy from</param>
        public void CopyQualifiersFrom(AbstractObjectDefinition source)
        {
            Trace.Assert(source != null, "Source must not be null");
            if (source.qualifiers != null && source.qualifiers.Count > 0)
            {
                qualifiers = qualifiers ?? new Dictionary<string, AutowireCandidateQualifier>();
                foreach (var qualifier in source.qualifiers)
                {
                    if (!qualifiers.ContainsKey(qualifier.Key))
                    {
                        qualifiers.Add(qualifier.Key, qualifier.Value);
                    }
                }
            }
        }

        /// <summary>
        /// The name of the initializer method.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default value is the <see cref="String.Empty"/> constant,
        /// in which case there is no initializer method.
        /// </p>
        /// </remarks>
        public string InitMethodName
        {
            get => initMethodName;
            set => initMethodName = StringUtils.GetTextOrNull(value);
        }

        /// <summary>
        /// Return the name of the destroy method.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default value is the <see cref="String.Empty"/> constant,
        /// in which case there is no destroy method.
        /// </p>
        /// </remarks>
        public string DestroyMethodName
        {
            get => destroyMethodName;
            set => destroyMethodName = StringUtils.GetTextOrNull(value);
        }

        /// <summary>
        /// The name of the factory method to use (if any).
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method will be invoked with constructor arguments, or with no
        /// arguments if none are specified. The <see langword="static"/>
        /// method will be invoked on the specified
        /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition.ObjectType"/>.
        /// </p>
        /// </remarks>
        public string FactoryMethodName
        {
            get => factoryMethodName;
            set => factoryMethodName = StringUtils.GetTextOrNull(value);
        }

        /// <summary>
        /// The name of the factory object to use (if any).
        /// </summary>
        public string FactoryObjectName
        {
            get => factoryObjectName;
            set => factoryObjectName = StringUtils.GetTextOrNull(value);
        }

        /// <summary>
        /// Does this object definition have any constructor argument values?
        /// </summary>
        /// <value>
        /// <see langword="true"/> if his object definition has at least one
        /// element in it's
        /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.ConstructorArgumentValues"/>
        /// property.
        /// </value>
        public bool HasConstructorArgumentValues => constructorArgumentValues != null
                                                            && !constructorArgumentValues.Empty;

        /// <summary>
        /// Resolves the type of the object, resolving it from a specified
        /// object type name if necessary. 
        /// </summary>
        /// <returns>
        /// A resolved <see cref="System.Type"/> instance.
        /// </returns>
        /// <exception cref="System.TypeLoadException">
        /// If the type cannot be resolved.
        /// </exception>
        public Type ResolveObjectType()
        {
            string typeName = ObjectTypeName;
            if (typeName == null)
            {
                return null;
            }
            Type resolvedType = TypeResolutionUtils.ResolveType(typeName);
            ObjectType = resolvedType;
            return resolvedType;
        }

        /// <summary>
        /// Validate this object definition.
        /// </summary>
        /// <exception cref="Spring.Objects.Factory.Support.ObjectDefinitionValidationException">
        /// In the case of a validation failure.
        /// </exception>
        public virtual void Validate()
        {
            if (IsLazyInit && !IsSingleton)
            {
                throw new ObjectDefinitionValidationException(
                    "Lazy initialization is only applicable to singleton objects.");
            }
            if (HasMethodOverrides && StringUtils.HasText(FactoryMethodName))
            {
                throw new ObjectDefinitionValidationException(
                    "Cannot combine static factory method with method overrides: " +
                    "the static factory method must create the instance.");
            }
            if (HasObjectType)
            {
                PrepareMethodOverrides();
            }
        }

        /// <summary>
        /// Validates all <see cref="MethodOverrides"/> 
        /// </summary>
        public virtual void PrepareMethodOverrides()
        {
            // ascertain that the various lookup methods exist...
            foreach (MethodOverride mo in MethodOverrides)
            {
                PrepareMethodOverride(mo);
            }
        }

        /// <summary>
        /// Validate the supplied <paramref name="methodOverride"/>.
        /// </summary>
        /// <param name="methodOverride">
        /// The <see cref="Spring.Objects.Factory.Support.MethodOverride"/>
        /// to be validated.
        /// </param>
        protected void PrepareMethodOverride(MethodOverride methodOverride)
        {
            if (!ReflectionUtils.HasAtLeastOneMethodWithName(ObjectType, methodOverride.MethodName))
            {
                throw new ObjectDefinitionValidationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Invalid method override: no method with name '{0}' on class [{1}].",
                        methodOverride.MethodName, ObjectTypeName));
            }
            //TODO investigate setting overloaded at this point using MethodCountForName...
            //Test SunnyDayReplaceMethod_WithArgumentAcceptingReplacerWithNoTypeFragmentsSpecified
            // will fail if doing this optimization.
        }

        /// <summary>
        /// Override settings in this object definition from the supplied
        /// <paramref name="other"/> object definition.
        /// </summary>
        /// <param name="other">
        /// The object definition used to override the member fields of this instance.
        /// </param>
        public virtual void OverrideFrom(IObjectDefinition other)
        {
            AssertUtils.ArgumentNotNull(other, "other");

            IsAbstract = other.IsAbstract;
            Scope = other.Scope;
            IsLazyInit = other.IsLazyInit;
            ConstructorArgumentValues.AddAll(other.ConstructorArgumentValues);
            PropertyValues.AddAll(other.PropertyValues.PropertyValues);
            EventHandlerValues.AddAll(other.EventHandlerValues);
            if (StringUtils.HasText(other.ObjectTypeName))
            {
                ObjectTypeName = other.ObjectTypeName;
            }
            if (StringUtils.HasText(other.InitMethodName))
            {
                InitMethodName = other.InitMethodName;
            }
            if (StringUtils.HasText(other.DestroyMethodName))
            {
                DestroyMethodName = other.DestroyMethodName;
            }
            if (StringUtils.HasText(other.FactoryObjectName))
            {
                FactoryObjectName = other.FactoryObjectName;
            }
            if (StringUtils.HasText(other.FactoryMethodName))
            {
                FactoryMethodName = other.FactoryMethodName;
            }
            if (other.DependsOn != null && other.DependsOn.Count > 0)
            {
                var deps = new List<string>(other.DependsOn.Count + (DependsOn?.Count).GetValueOrDefault());
                deps.AddRange(other.DependsOn);
                if (DependsOn != null && DependsOn.Count > 0)
                {
                    deps.AddRange(DependsOn);
                }
                DependsOn = deps;
            }
            AutowireMode = other.AutowireMode;
            ResourceDescription = other.ResourceDescription;
            IsPrimary = other.IsPrimary;
            IsAutowireCandidate = other.IsAutowireCandidate;

            if (other is AbstractObjectDefinition aod)
            {
                if (other.ObjectTypeName != null)
                {
                    ObjectTypeName = other.ObjectTypeName;
                }
                if (aod.HasObjectType)
                {
                    ObjectType = other.ObjectType;
                }

                MethodOverrides.AddAll(aod.MethodOverrides);
                DependencyCheck = aod.DependencyCheck;
                CopyQualifiersFrom(aod);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current
        /// <see cref="System.Object"/>. 
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current
        /// <see cref="System.Object"/>. 
        /// </returns>
        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder(string.Format("Class [{0}]", ObjectTypeName));
            buffer.Append("; Abstract = ").Append(IsAbstract);
            buffer.Append("; Parent = ").Append(ParentName);
            buffer.Append("; Scope = ").Append(Scope);
            buffer.Append("; Singleton = ").Append(IsSingleton);
            buffer.Append("; LazyInit = ").Append(IsLazyInit);
            buffer.Append("; Autowire = ").Append(AutowireMode);
            buffer.Append("; Autowire-Candidate = ").Append(IsAutowireCandidate);
            buffer.Append("; Primary = ").Append(IsPrimary);
            buffer.Append("; DependencyCheck = ").Append(DependencyCheck);
            buffer.Append("; InitMethodName = ").Append(InitMethodName);
            buffer.Append("; DestroyMethodName = ").Append(DestroyMethodName);
            buffer.Append("; FactoryMethodName = ").Append(FactoryMethodName);
            buffer.Append("; FactoryObjectName = ").Append(FactoryObjectName);
            if (StringUtils.HasText(ResourceDescription))
            {
                buffer.Append("; defined in = ").Append(ResourceDescription);
            }
            return buffer.ToString();
        }
       
        protected AbstractObjectDefinition(SerializationInfo info, StreamingContext context)
        {
            constructorArgumentValues = (ConstructorArgumentValues) info.GetValue("constructorArgumentValues", typeof(ConstructorArgumentValues));
            propertyValues = (MutablePropertyValues) info.GetValue("propertyValues", typeof(MutablePropertyValues));
            eventHandlerValues= (EventValues) info.GetValue("eventHandlerValues", typeof(EventValues));
            methodOverrides= (MethodOverrides) info.GetValue("methodOverrides", typeof(MethodOverrides));
            resourceDescription = info.GetString("resourceDescription");
            isSingleton = info.GetBoolean("isSingleton");
            isPrototype = info.GetBoolean("isPrototype");
            isLazyInit = info.GetBoolean("isLazyInit");
            isAbstract = info.GetBoolean("isAbstract");
            scope= info.GetString("scope");
            role = (ObjectRole) info.GetValue("role", typeof(ObjectRole));
            
            var objectTypeName = info.GetString("objectTypeName");
            objectType = objectTypeName != null ? Type.GetType(objectTypeName) : null;
            
            autowireMode = (AutoWiringMode) info.GetValue("autowireMode", typeof(AutoWiringMode));
            dependencyCheck= (DependencyCheckingMode) info.GetValue("dependencyCheck", typeof(DependencyCheckingMode));
            dependsOn = (List<string>) info.GetValue("dependsOn", typeof(List<string>));
            autowireCandidate = info.GetBoolean("autowireCandidate");
            primary = info.GetBoolean("primary");
            qualifiers = (Dictionary<string, AutowireCandidateQualifier>) info.GetValue("qualifiers", typeof(Dictionary<string, AutowireCandidateQualifier>));
            initMethodName = info.GetString("initMethodName");
            destroyMethodName = info.GetString("destroyMethodName");
            factoryMethodName = info.GetString("factoryMethodName" );
            factoryObjectName= info.GetString("factoryObjectName");
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("constructorArgumentValues", constructorArgumentValues);
            info.AddValue("propertyValues", propertyValues);
            info.AddValue("eventHandlerValues", eventHandlerValues);
            info.AddValue("methodOverrides", methodOverrides);
            info.AddValue("resourceDescription", resourceDescription);
            info.AddValue("isSingleton", isSingleton);
            info.AddValue("isPrototype", isPrototype);
            info.AddValue("isLazyInit", isLazyInit);
            info.AddValue("isAbstract", isAbstract);
            info.AddValue("scope", scope);
            info.AddValue("role", role);
            info.AddValue("objectTypeName", objectType.AssemblyQualifiedName);
            info.AddValue("autowireMode", autowireMode);
            info.AddValue("dependencyCheck", dependencyCheck);
            info.AddValue("dependsOn", dependsOn);
            info.AddValue("autowireCandidate", autowireCandidate);
            info.AddValue("primary", primary);
            info.AddValue("qualifiers", qualifiers);
            info.AddValue("initMethodName", initMethodName);
            info.AddValue("destroyMethodName", destroyMethodName);
            info.AddValue("factoryMethodName", factoryMethodName);
            info.AddValue("factoryObjectName", factoryObjectName);
        }
    }
}
