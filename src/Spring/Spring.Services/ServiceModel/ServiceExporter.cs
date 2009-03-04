#if NET_3_0
#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
using System.Reflection;
using System.Reflection.Emit;
using System.ServiceModel;
using System.Net.Security;

using Spring.Util;
using Spring.Context;
using Spring.Core.TypeResolution;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Support;
using Spring.Proxy;
using Spring.ServiceModel.Support;

#endregion

namespace Spring.ServiceModel
{
    /// <summary>
    /// Exports an object as a WCF service.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class ServiceExporter : IFactoryObject, IInitializingObject, IObjectFactoryAware, IObjectNameAware
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(ServiceExporter));

        #endregion

        #region Fields

        private string _targetName;
        private Type _contractInterface;
        private IList _typeAttributes = new ArrayList();
        private IDictionary _memberAttributes = new Hashtable();

        private string _name;
        private string _namespace;
        private string _configurationName;
        private Type _callbackContract;
        private ProtectionLevel _protectionLevel;
        private SessionMode _sessionMode;

        /// <summary>
        /// The name of the object in the factory.
        /// </summary>
        protected string objectName;

        /// <summary>
        /// The owning factory.
        /// </summary>
        protected IObjectFactory objectFactory;

        /// <summary>
        /// The generated WCF service wrapper type.
        /// </summary>
        private Type proxyType;

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the <see cref="ServiceExporter"/> class.
        /// </summary>
        public ServiceExporter()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the target object definition.
        /// </summary>
        public string TargetName
        {
            get { return _targetName; }
            set { _targetName = value; }
        }

        /// <summary>
        /// Gets or sets the service contract interface type.
        /// </summary>
        /// <remarks>
        /// If not set, uses the unique interface implemented or inherited by the target type. 
        /// An error will be thrown if the target type implements more than one interface.
        /// </remarks>
        /// <value>The service contract interface type.</value>
        public Type ContractInterface
        {
            get { return _contractInterface; }
            set { _contractInterface = value; }
        }

        /// <summary>
        /// Gets or sets a list of custom attributes 
        /// that should be applied to the WCF service class.
        /// </summary>
        public IList TypeAttributes
        {
            get { return _typeAttributes; }
            set { _typeAttributes = value; }
        }

        /// <summary>
        /// Gets or sets a dictionary of custom attributes 
        /// that should be applied to the WCF service members.
        /// </summary>
        /// <remarks>
        /// Dictionary key is an expression that members can be matched against. 
        /// Value is a list of attributes that should be applied 
        /// to each member that matches expression.
        /// </remarks>
        public IDictionary MemberAttributes
        {
            get { return _memberAttributes; }
            set { _memberAttributes = value; }
        }

        /// <summary>
        /// Gets or sets the name for the &lt;portType&gt; element in 
        /// Web Services Description Language (WSDL).
        /// </summary>
        /// <value>
        /// The default value is the name of the class or interface to which the 
        /// System.ServiceModel.ServiceContractAttribute is applied.
        /// </value>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the namespace of the &lt;portType&gt; element in 
        /// Web Services Description Language (WSDL).
        /// </summary>
        /// <value>
        /// The WSDL namespace of the &lt;portType&gt; element. The default value is "http://tempuri.org".
        /// </value>
        public string Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        /// <summary>
        /// Gets or sets the name used to locate the service in an application configuration file.
        /// </summary>
        /// <value>
        /// The name used to locate the service element in an application configuration file. 
        /// The default is the name of the service implementation class.
        /// </value>
        public string ConfigurationName
        {
            get { return _configurationName; }
            set { _configurationName = value; }
        }

        /// <summary>
        /// Gets or sets the type of callback contract when the contract is a duplex contract.
        /// </summary>
        /// <value>
        /// A <see cref="System.Type"/> that indicates the callback contract. The default is null.
        /// </value>
        public Type CallbackContract
        {
            get { return _callbackContract; }
            set { _callbackContract = value; }
        }

        /// <summary>
        /// Specifies whether the binding for the contract must support the value of
        /// the ProtectionLevel property.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Net.Security.ProtectionLevel"/> values. 
        /// The default is <see cref="System.Net.Security.ProtectionLevel.None"/>.
        /// </value>
        public ProtectionLevel ProtectionLevel
        {
            get { return _protectionLevel; }
            set { _protectionLevel = value; }
        }

        /// <summary>
        /// Gets or sets whether sessions are allowed, not allowed or required.
        /// </summary>
        /// <value>
        /// A <see cref="System.ServiceModel.SessionMode"/> that indicates whether sessions are allowed, 
        /// not allowed, or required.
        /// </value>
        public SessionMode SessionMode
        {
            get { return _sessionMode; }
            set { _sessionMode = value; }
        }

        #endregion

        #region IObjectFactoryAware Members

        /// <summary>
        /// Callback that supplies the owning factory to an object instance.
        /// </summary>
        /// <value>
        /// Owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// (may not be <see langword="null"/>). The object can immediately
        /// call methods on the factory.
        /// </value>
        /// <remarks>
        /// <p>
        /// Invoked after population of normal object properties but before an init
        /// callback like <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// method or a custom init-method.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of initialization errors.
        /// </exception>
        public virtual IObjectFactory ObjectFactory
        {
            set { this.objectFactory = value; }
        }

        #endregion

        #region IInitializingObject Members

        /// <summary>
        /// Publish the object 
        /// </summary>
        public void AfterPropertiesSet()
        {
            ValidateConfiguration();
            GenerateProxy();
        }

        #endregion

        #region IFactoryObject Members

        /// <summary>
        /// Return an instance (possibly shared or independent) of the object
        /// managed by this factory.
        /// </summary>
        /// <remarks>
        /// <note type="caution">
        /// If this method is being called in the context of an enclosing IoC container and
        /// returns <see langword="null"/>, the IoC container will consider this factory
        /// object as not being fully initialized and throw a corresponding (and most
        /// probably fatal) exception.
        /// </note>
        /// </remarks>
        /// <returns>
        /// An instance (possibly shared or independent) of the object managed by
        /// this factory.
        /// </returns>
        public object GetObject()
        {
            return proxyType;
        }

        /// <summary>
        /// Return the <see cref="System.Type"/> of object that this
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
        /// <see langword="null"/> if not known in advance.
        /// </summary>
        public Type ObjectType
        {
            get { return typeof(Type); }
        }

        /// <summary>
        /// Is the object managed by this factory a singleton or a prototype?
        /// </summary>
        public bool IsSingleton
        {
            get { return false; }
        }

        #endregion

        #region IObjectNameAware Members

        /// <summary>
        /// Set the name of the object in the object factory that created this object.
        /// </summary>
        /// <value>
        /// The name of the object in the factory.
        /// </value>
        /// <remarks>
        /// <p>
        /// Invoked after population of normal object properties but before an init
        /// callback like <see cref="IInitializingObject.AfterPropertiesSet"/>'s
        /// <see cref="IInitializingObject"/>
        /// method or a custom init-method.
        /// </p>
        /// </remarks>
        public string ObjectName
        {
            set { this.objectName = value; }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        protected virtual void ValidateConfiguration()
        {
            if (TargetName == null)
            {
                throw new ArgumentException("The TargetName property is required.");
            }
            if (ContractInterface != null && !ContractInterface.IsInterface)
            {
                throw new ArgumentException("ContractInterface must be an interface.");
            }
        }

        /// <summary>
        /// Generates the WCF service wrapper type.
        /// </summary>
        protected virtual void GenerateProxy()
        {
            IProxyTypeBuilder builder = new ConfigurableServiceProxyTypeBuilder(TargetName, this.objectName, objectFactory, 
                Name, Namespace, ConfigurationName, CallbackContract, ProtectionLevel, SessionMode);

            if (ContractInterface != null)
            {
                builder.Interfaces = new Type[] { ContractInterface };
            }
            builder.TypeAttributes = TypeAttributes;
            builder.MemberAttributes = MemberAttributes;

            if (builder.Interfaces.Length > 1)
            {
                throw new ArgumentException(String.Format(
                    "ServiceExporter cannot export service type '{0}' as a WCF service because it implements multiple interfaces. Specify the contract interface to expose via the ContractInterface property.",
                    builder.TargetType));
            }

            proxyType = builder.BuildProxyType();
        }

        #endregion

        #region ConfigurableServiceProxyTypeBuilder inner class implementation

        /// <summary>
        /// Builds a WCF service type.
        /// </summary>
        private sealed class ConfigurableServiceProxyTypeBuilder : ServiceProxyTypeBuilder
        {
            private CustomAttributeBuilder serviceContractAttribute;

            public ConfigurableServiceProxyTypeBuilder(string targetName, string objectName, IObjectFactory objectFactory,  
                string name, string ns, string configurationName, Type callbackContract, ProtectionLevel protectionLevel, SessionMode sessionMode)
                : base(targetName, objectName, objectFactory)
            {

                if (!StringUtils.HasText(configurationName))
                {
                    name = this.Interfaces[0].Name;
                    configurationName = this.Interfaces[0].FullName;
                }

                // Creates a ServiceContractAttribute from configuration info
                this.serviceContractAttribute = CreateServiceContractAttribute(name, ns, configurationName, callbackContract, protectionLevel, sessionMode);
            }

            private static CustomAttributeBuilder CreateServiceContractAttribute(string name, string ns, string configurationName,
                Type callbackContract, ProtectionLevel protectionLevel, SessionMode sessionMode)
            {
                ReflectionUtils.CustomAttributeBuilderBuilder scbb =
                    new ReflectionUtils.CustomAttributeBuilderBuilder(typeof(ServiceContractAttribute));
                if (StringUtils.HasText(name))
                {
                    scbb.AddPropertyValue("Name", name);
                }
                if (StringUtils.HasText(ns))
                {
                    scbb.AddPropertyValue("Namespace", ns);
                }
                if (StringUtils.HasText(configurationName))
                {
                    scbb.AddPropertyValue("ConfigurationName", configurationName);
                }
                if (callbackContract != null)
                {
                    scbb.AddPropertyValue("CallbackContract", callbackContract);
                }
                if (protectionLevel != ProtectionLevel.None)
                {
                    scbb.AddPropertyValue("ProtectionLevel", protectionLevel);
                }
                if (sessionMode != SessionMode.Allowed)
                {
                    scbb.AddPropertyValue("SessionMode", sessionMode);
                }
                return scbb.Build();
            }

            protected override IList GetTypeAttributes(Type type)
            {
                IList attrs = base.GetTypeAttributes(type);

                bool containsServiceContractAttribute = false;

                for (int i = 0; i < attrs.Count; i++)
                {
                    if (IsAttributeMatchingType(attrs[i], typeof(ServiceContractAttribute)))
                    {
                        // Override existing ServiceContractAttribute
                        containsServiceContractAttribute = true;
                        attrs[i] = serviceContractAttribute;
                    }
                }

                // Add missing ServiceContractAttribute
                if (!containsServiceContractAttribute)
                {
                    attrs.Add(serviceContractAttribute);
                }

                return attrs;
            }

            protected override IList GetMethodAttributes(MethodInfo method)
            {
                IList attrs = base.GetMethodAttributes(method);

                bool containsOperationContractAttribute = false;
                foreach (object attr in attrs)
                {
                    if (IsAttributeMatchingType(attr, typeof(OperationContractAttribute)))
                    {
                        containsOperationContractAttribute = true;
                        break;
                    }
                }

                // Creates default OperationContractAttribute if not set yet
                if (!containsOperationContractAttribute)
                {
                    attrs.Add(ReflectionUtils.CreateCustomAttribute(typeof(OperationContractAttribute)));
                }

                return attrs;
            }

            protected override Type[] GetProxiableInterfaces(Type[] interfaces)
            {
                Type[] proxiableInterfaces = base.GetProxiableInterfaces(interfaces);

                return proxiableInterfaces;
            }
        }

        #endregion
    }
}
#endif