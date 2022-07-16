#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System.Collections;
using System.EnterpriseServices;
using System.Reflection;
using System.Reflection.Emit;
using Spring.Core.TypeResolution;
using Spring.Objects.Factory;
using Spring.Proxy;

namespace Spring.EnterpriseServices
{
    /// <summary>
    /// Encapsulates information necessary to create ServicedComponent
    /// wrapper around target class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Instances of this class should be used as elements in the Components
    /// list of the <see cref="EnterpriseServicesExporter"/> class, which will
    /// register them with COM+ Services. For a full description on how to export
    /// and use services with COM+, see the <see cref="EnterpriseServicesExporter"/> reference.
    /// </para>
    /// </remarks>
    /// <seealso cref="EnterpriseServicesExporter"/>
    /// <author>Aleksandar Seovic</author>
    /// <author>Erich Eichinger</author>
    public class ServicedComponentExporter : IInitializingObject, IObjectNameAware
    {
        #region Fields

        private string _objectName;
        private string _targetName;
        private string[] _interfaces;
        private IList _typeAttributes = new ArrayList();
        private IDictionary _memberAttributes = new Hashtable();

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="ServicedComponentExporter"/> class.
        /// </summary>
        public ServicedComponentExporter()
        {}

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets name of the target object that should be exposed as a serviced component.
        /// </summary>
        public string TargetName
        {
            get { return _targetName; }
            set { _targetName = value; }
        }

        /// <summary>
        /// Gets or sets the list of interfaces whose methods should be exported.
        /// </summary>
        /// <remarks>
        /// The default value of this property is all the interfaces
        /// implemented or inherited by the target type.
        /// </remarks>
        /// <value>The interfaces to export.</value>
        public string[] Interfaces
        {
            get { return _interfaces; }
            set { _interfaces = value; }
        }

        /// <summary>
        /// Gets or sets a list of custom attributes
        /// that should be applied to a proxy class.
        /// </summary>
        public IList TypeAttributes
        {
            get { return _typeAttributes; }
            set { _typeAttributes = value; }
        }

        /// <summary>
        /// Gets or sets a dictionary of custom attributes
        /// that should be applied to proxy members.
        /// </summary>
        /// <remarks>
        /// Map key is an expression that members can be matched against. Value is a list
        /// of attributes that should be applied to each member that matches expression.
        /// </remarks>
        public IDictionary MemberAttributes
        {
            get { return _memberAttributes; }
            set { _memberAttributes = value; }
        }

        #endregion

        #region IInitializingObject Members

        /// <summary>
        /// Validate configuration.
        /// </summary>
        public void AfterPropertiesSet()
        {
            ValidateConfiguration();
        }

        #endregion

        #region IObjectNameAware Members

        /// <summary>
        /// Set the name of the object in the object factory
        /// that created this object.
        /// </summary>
        public string ObjectName
        {
            set { _objectName = value; }
        }

        #endregion

        #region Methods

        private void ValidateConfiguration()
        {
            if (TargetName == null)
            {
                throw new ArgumentException("The TargetName property is required.");
            }
        }

        /// <summary>
        /// Creates ServicedComponent wrapper around target class.
        /// </summary>
        /// <param name="module">Dynamic module builder to use</param>
        /// <param name="baseType"></param>
        /// <param name="targetType">Type of the exported object.</param>
        /// <param name="springManagedLifecycle">whether to generate lookups in ContextRegistry for each service method call or use a 'new'ed target instance</param>
        /// <remarks>
        /// if <paramref name="springManagedLifecycle"/> is <c>true</c>, each ServicedComponent method call will look similar to
        /// <code>
        /// class MyServicedComponent {
        ///   void MethodX() {
        ///     ContextRegistry.GetContext().GetObject("TargetName").MethodX();
        ///   }
        /// }
        /// </code>
        /// <br/>
        /// if <paramref name="springManagedLifecycle"/> is <c>false</c>, the instance will be simply created at component activation using 'new':
        /// <code>
        /// class MyServicedComponent {
        ///   TargetType target = new TargetType();
        ///
        ///   void MethodX() {
        ///     target.MethodX();
        ///   }
        /// }
        /// </code>
        /// <br/>
        /// The differences are of course that in the former case, the target lifecycle is entirely managed by Spring, thus avoiding
        /// issues with ServiceComponent activation/deactivation as well as removing the need for default constructors.
        /// </remarks>
        public Type CreateWrapperType(ModuleBuilder module, Type baseType, Type targetType, bool springManagedLifecycle)
        {
            ValidateConfiguration();

            // create wrapper using appropriate proxy builder
            IProxyTypeBuilder proxyBuilder;
            if (springManagedLifecycle)
            {
                proxyBuilder = new SpringManagedServicedComponentProxyTypeBuilder(module, baseType, this);
            }
            else
            {
                proxyBuilder = new SimpleServicedComponentProxyTypeBuilder(module, baseType);
            }

            proxyBuilder.Name = _objectName;
            proxyBuilder.TargetType = targetType;
            if (_interfaces != null && _interfaces.Length > 0)
            {
                proxyBuilder.Interfaces = TypeResolutionUtils.ResolveInterfaceArray(_interfaces);
            }
            proxyBuilder.TypeAttributes = TypeAttributes;
            proxyBuilder.MemberAttributes = MemberAttributes;

            Type componentType = proxyBuilder.BuildProxyType();

            // create and register client-side proxy factory for component

            return componentType;
        }

        #endregion

        #region ServicedComponentProxyTypeBuilder inner class definition

        private class ServicedComponentTargetProxyMethodBuilder : TargetProxyMethodBuilder
        {
            public ServicedComponentTargetProxyMethodBuilder(TypeBuilder typeBuilder, IProxyTypeGenerator proxyGenerator, bool explicitImplementation) : base(typeBuilder, proxyGenerator, explicitImplementation)
            {}

            /// <summary>
            /// Suppress output to avoid Spring.Core dependency
            /// </summary>
            protected override void CallAssertUnderstands(ILGenerator il, MethodInfo method, string targetName)
            {
//                base.CallAssertUnderstands(il, method, targetRef, targetName);
            }
        }

        private class ServicedComponentProxyTypeBuilder : CompositionProxyTypeBuilder
        {
            /// <summary>
            /// Implements default constructor for the proxy class.
            /// </summary>
            protected override void ImplementConstructors(TypeBuilder builder)
            {
                MethodAttributes attributes = MethodAttributes.Public |
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                    MethodAttributes.RTSpecialName;
                ConstructorBuilder cb = builder.DefineConstructor(attributes,
                  CallingConventions.Standard, Type.EmptyTypes);

                ILGenerator il = cb.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, builder.BaseType.GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Ret);
            }

            protected override IProxyMethodBuilder CreateTargetProxyMethodBuilder(TypeBuilder typeBuilder)
            {
                return new ServicedComponentTargetProxyMethodBuilder(typeBuilder, this,
                                                                     this.ExplicitInterfaceImplementation);
            }
        }

        private sealed class SimpleServicedComponentProxyTypeBuilder : ServicedComponentProxyTypeBuilder
        {
            private readonly ModuleBuilder module;

            #region Constructor(s) / Destructor

            public SimpleServicedComponentProxyTypeBuilder(ModuleBuilder module, Type baseType)
            {
                this.module = module;
                BaseType = baseType;
            }

            #endregion

            #region Protected Methods

            protected override TypeBuilder CreateTypeBuilder(string name, Type baseType)
            {
                return module.DefineType(name, System.Reflection.TypeAttributes.Public, baseType);
            }

            #endregion
        }

        private sealed class SpringManagedServicedComponentProxyTypeBuilder : ServicedComponentProxyTypeBuilder
        {
            #region Fields

            private delegate object GetTargetDelegate(ServicedComponent component, string name);
            private static readonly MethodInfo ServicedComponentExporter_GetTarget =  new GetTargetDelegate(ServicedComponentHelper.GetObject).Method;

            #endregion

            #region Fields

            private readonly ServicedComponentExporter exporter;
            private readonly ModuleBuilder module;

            #endregion

            #region Constructor(s) / Destructor

            public SpringManagedServicedComponentProxyTypeBuilder(ModuleBuilder module, Type baseType, ServicedComponentExporter exporter)
            {
                this.module = module;
                this.exporter = exporter;
                BaseType = baseType;
            }

            #endregion

            #region Protected Methods

            protected override TypeBuilder CreateTypeBuilder(string name, Type baseType)
            {
                return module.DefineType(name, System.Reflection.TypeAttributes.Public, baseType);
            }

            protected override void DeclareTargetInstanceField(TypeBuilder builder)
            {
                //base.DeclareTargetInstanceField(builder);
            }

            #endregion

            #region IProxyTypeGenerator Members

            /// <summary>
            /// Generates the IL instructions that pushes
            /// the target instance on which calls should be delegated to.
            /// </summary>
            /// <param name="il">The IL generator to use.</param>
            public override void PushTarget( ILGenerator il )
            {
                FieldInfo getObjectRef = BaseType.GetField("getObject", BindingFlags.NonPublic|BindingFlags.Static);
                il.Emit(OpCodes.Ldsfld, getObjectRef);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit( OpCodes.Ldstr, this.exporter.TargetName );
                il.Emit( OpCodes.Callvirt,  getObjectRef.FieldType.GetMethod("Invoke"));
            }
            #endregion

        }

        #endregion
    }
}
