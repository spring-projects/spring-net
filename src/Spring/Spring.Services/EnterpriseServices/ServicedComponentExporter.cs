#region License

/*
 * Copyright 2002-2007 the original author or authors.
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

#if (!NET_1_0 && !MONO)

#region Imports

using System;
using System.Collections;
using System.EnterpriseServices;
using System.Reflection.Emit;

using Spring.Core.TypeResolution;
using Spring.Objects.Factory;
using Spring.Proxy;
using Spring.Util;

#endregion

namespace Spring.EnterpriseServices
{
    /// <summary>
    /// Encapsulates information necessary to create ServicedComponent
    /// wrapper around target class.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Instances of this class should be used as elements in the Components
    /// list of the <see cref="EnterpriseServicesExporter"/> class, which will
    /// register them with COM+ Services.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
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
        /// <param name="objectFactory">Object factory to get target from.</param>
        internal Type CreateWrapperType(ModuleBuilder module, IObjectFactory objectFactory)
        {
            // create wrapper using appropriate proxy builder
            IProxyTypeBuilder proxyBuilder = new ServicedComponentProxyTypeBuilder(module);
            proxyBuilder.Name = _objectName;
            proxyBuilder.TargetType = objectFactory.GetType(TargetName);
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

        private sealed class ServicedComponentProxyTypeBuilder : CompositionProxyTypeBuilder
        {
            #region Fields

            private ModuleBuilder module;

            #endregion

            #region Constructor(s) / Destructor

            public ServicedComponentProxyTypeBuilder(ModuleBuilder module)
            {
                this.module = module;

                BaseType = typeof(ServicedComponent);
            }

            #endregion

            #region Protected Methods

            protected override TypeBuilder CreateTypeBuilder(string name, Type baseType)
            {
                return module.DefineType(name,
                    System.Reflection.TypeAttributes.BeforeFieldInit | System.Reflection.TypeAttributes.Public,
                    baseType);
            }

            #endregion
        }

        #endregion
    }
}

#endif // (!NET_1_0)
