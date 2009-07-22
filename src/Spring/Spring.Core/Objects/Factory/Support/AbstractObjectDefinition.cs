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
using System.Globalization;
using System.Reflection;
using System.Text;
using Spring.Core;
using Spring.Core.TypeResolution;
using Spring.Objects.Factory.Config;
using Spring.Util;

#endregion

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
    public abstract class AbstractObjectDefinition : IConfigurableObjectDefinition
    {
        private static readonly string SCOPE_SINGLETON = "singleton";
        private static readonly string SCOPE_PROTOTYPE = "prototype";

        #region Constructor (s) / Destructor

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
            constructorArgumentValues =
                (arguments != null) ? arguments : new ConstructorArgumentValues();
            propertyValues =
                (properties != null) ? properties : new MutablePropertyValues();
            eventHandlerValues = new EventValues();
            DependsOn = StringUtils.EmptyStrings;
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
            this.OverrideFrom(other);

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
            DependsOn = new string[other.DependsOn.Length];
            IsAutowireCandidate = other.IsAutowireCandidate;
            Array.Copy(other.DependsOn, DependsOn, other.DependsOn.Length);
            FactoryMethodName = other.FactoryMethodName;
            FactoryObjectName = other.FactoryObjectName;
            AutowireMode = other.AutowireMode;
            ResourceDescription = other.ResourceDescription;
        }

        #endregion

        #region Properties

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
            get { return propertyValues; }
            set { propertyValues = value == null ? new MutablePropertyValues() : value; }
        }

        /// <summary>
        /// Does this definition have any
        /// <see cref="Spring.Objects.Factory.Support.MethodOverrides"/>?
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this definition has at least one
        /// <see cref="Spring.Objects.Factory.Support.MethodOverride"/>.
        /// </value>
        public bool HasMethodOverrides
        {
            get { return !MethodOverrides.IsEmpty; }
        }

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
            get { return constructorArgumentValues; }
            set { constructorArgumentValues = value == null ? new ConstructorArgumentValues() : value; }
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
            get { return eventHandlerValues; }
            set { eventHandlerValues = value == null ? new EventValues() : value; }
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
            get { return methodOverrides; }
            set { methodOverrides = value == null ? new MethodOverrides() : value; }
        }

        /// <summary>
        /// The name of the target scope for the object.
        /// Defaults to "singleton", ootb alternative is "prototype". Extended object factories
        /// might support further scopes.
        /// </summary>
        public virtual string Scope
        {
            get { return scope; }
            set
            {
                AssertUtils.ArgumentNotNull(value, "Scope");
                this.scope = value;
                this.isPrototype = 0 == string.Compare(SCOPE_PROTOTYPE, value, true);
                this.isSingleton = !isPrototype; // 0 == string.Compare(SCOPE_SINGLETON, value, true);
            }
        }

        /// <summary>
        /// Get or set the role hint for this object definition
        /// </summary>
        public virtual ObjectRole Role
        {
            get { return role; }
            set { role = value; }
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
            get { return isSingleton; }
            set
            {
                scope = (value ? SCOPE_SINGLETON : SCOPE_PROTOTYPE);
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
        public virtual bool IsPrototype
        {
            get { return isPrototype; }
        }

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
            get { return isLazyInit; }
            set { isLazyInit = value; }
        }

        /// <summary>
        /// Is this object definition a "template", i.e. not meant to be instantiated
        /// itself but rather just serving as an object definition for configuration 
        /// templates used by <see cref="Spring.Objects.Factory.IObjectFactory.ConfigureObject(object, string)"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this object definition is a "template".
        /// </value>
        public bool IsTemplate
        {
            get
            {
                return (
                           isAbstract ||
                           (objectType == null && StringUtils.IsNullOrEmpty(factoryObjectName))
                       );
            }
        }

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
            get { return isAbstract; }
            set { isAbstract = value; }
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
                if (!HasObjectType)
                {
                    throw new ApplicationException(
                        "Object definition does not carry a resolved System.Type");
                }
                return (Type) objectType;
            }
            set { objectType = value; }
        }

        /// <summary>
        /// Is the <see cref="System.Type"/> of the object definition a resolved
        /// <see cref="System.Type"/>?
        /// </summary>
        public bool HasObjectType
        {
            get { return objectType is Type; }
        }

        /// <summary>
        /// Returns the <see cref="System.Type.FullName"/> of the
        /// <see cref="System.Type"/> of the object definition (if any).
        /// </summary>
        public string ObjectTypeName
        {
            get
            {
                if (objectType is Type)
                {
                    return ((Type) objectType).FullName;
                }
                else
                {
                    return objectType as string;
                }
            }
            set { objectType = StringUtils.GetTextOrNull(value); }
        }


        /// <summary>
        /// A description of the resource that this object definition
        /// came from (for the purpose of showing context in case of errors).
        /// </summary>
        public string ResourceDescription
        {
            get { return resourceDescription; }
            set { resourceDescription = StringUtils.GetTextOrNull(value); }
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
            get { return autowireMode; }
            set { autowireMode = value; }
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
            get { return dependencyCheck; }
            set { dependencyCheck = value; }
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
        public string[] DependsOn
        {
            get { return dependsOn; }
            set { dependsOn = value == null ? StringUtils.EmptyStrings : value; }
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
            get { return autowireCandidate; }
            set { autowireCandidate = value;}
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
            get { return initMethodName; }
            set { initMethodName = StringUtils.GetTextOrNull(value); }
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
            get { return destroyMethodName; }
            set { destroyMethodName = StringUtils.GetTextOrNull(value); }
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
            get { return factoryMethodName; }
            set { factoryMethodName = StringUtils.GetTextOrNull(value); }
        }

        /// <summary>
        /// The name of the factory object to use (if any).
        /// </summary>
        public string FactoryObjectName
        {
            get { return factoryObjectName; }
            set { factoryObjectName = StringUtils.GetTextOrNull(value); }
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
        public virtual bool HasConstructorArgumentValues
        {
            get
            {
                return ConstructorArgumentValues != null
                       && !ConstructorArgumentValues.Empty;
            }
        }

        #endregion

        #region Methods

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
            this.ObjectType = resolvedType;
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
            foreach (MethodOverride mo in MethodOverrides.Overrides)
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
            if (ArrayUtils.HasLength(other.DependsOn))
            {
                ArrayList deps = new ArrayList(other.DependsOn);
                if (ArrayUtils.HasLength(DependsOn))
                {
                    deps.AddRange(DependsOn);
                }
                DependsOn = (string[]) deps.ToArray(typeof(string));
            }
            AutowireMode = other.AutowireMode;
            ResourceDescription = other.ResourceDescription;

            AbstractObjectDefinition aod = other as AbstractObjectDefinition;
            if (aod != null)
            {
                if (aod.HasObjectType)
                {
                    ObjectType = other.ObjectType;
                }

                MethodOverrides.AddAll(aod.MethodOverrides);
                DependencyCheck = aod.DependencyCheck;
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

        #endregion

        #region Fields

        private ConstructorArgumentValues constructorArgumentValues = new ConstructorArgumentValues();
        private MutablePropertyValues propertyValues = new MutablePropertyValues();
        private EventValues eventHandlerValues = new EventValues();
        private MethodOverrides methodOverrides = new MethodOverrides();
        private string resourceDescription = null;
        private bool isSingleton = true;
        private bool isPrototype = false;
        private bool isLazyInit = false;
        private bool isAbstract = false;
        private string scope = SCOPE_SINGLETON;
        private ObjectRole role = ObjectRole.ROLE_APPLICATION;
        private object objectType;
        private AutoWiringMode autowireMode = AutoWiringMode.No;
        private DependencyCheckingMode dependencyCheck = DependencyCheckingMode.None;
        private string[] dependsOn;
        private bool autowireCandidate = true;
        private string initMethodName = null;
        private string destroyMethodName = null;
        private string factoryMethodName = null;
        private string factoryObjectName = null;

        #endregion
    }
}