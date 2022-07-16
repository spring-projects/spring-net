#region License

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

#endregion

using System.Collections;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Web.Services.Discovery;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Proxy;
using Spring.Util;
using Spring.Core.IO;

namespace Spring.Web.Services
{
    /// <summary>
    /// Factory Object that dynamically implements service interface for web service.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This factory object should be used to obtain reference to a web service
    /// that can be safely cast to a service interface, which allows client code to code
    /// against interface, and not directly against the web service.
    /// </p>
    /// <p>
    /// The WSDL contract needs to conform to WS-I Basic Profiles.
    /// </p>
    /// </remarks>
    /// <author>Bruno Baia</author>
    /// <author>Aleksandar Seovic</author>
    public class WebServiceProxyFactory : IConfigurableFactoryObject, IInitializingObject
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(WebServiceProxyFactory));

        #endregion

        #region Fields

        private IObjectDefinition _productTemplate;
        private Type _webServiceProxyBaseType = typeof(SoapHttpClientProtocol);
        private IResource _serviceUri;
        private Type _proxyType;
        private Type _serviceInterface;
        private NetworkCredential _credential;
        private string _proxyUrl;
        private NetworkCredential _proxyCredential;
        private string _bindingName;
        private IList _typeAttributes = new ArrayList();
        private IDictionary _memberAttributes = new Hashtable();

        /// <summary>
        /// The web service proxy default constructor.
        /// </summary>
        protected ConstructorInfo proxyConstructor;

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="WebServiceProxyFactory"/> class.
        /// </summary>
        public WebServiceProxyFactory()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the base type that web service proxy should inherit.
        /// </summary>
        /// <remarks>
        /// Default is <see cref="System.Web.Services.Protocols.SoapHttpClientProtocol"/>
        /// </remarks>
        public Type WebServiceProxyBaseType
        {
            get { return _webServiceProxyBaseType; }
            set { _webServiceProxyBaseType = value; }
        }

        /// <summary>
        /// Gets or sets the URI for an <see cref="Spring.Core.IO.IResource"/>
        /// that contains the web service description (WSDL).
        /// </summary>
        public IResource ServiceUri
        {
            get { return _serviceUri; }
            set { _serviceUri = value; }
        }

        /// <summary>
        /// Gets or sets type of the proxy class to wrap.
        /// </summary>
        public Type ProxyType
        {
            get { return _proxyType; }
            set { _proxyType = value; }
        }

        /// <summary>
        /// Gets or sets service interface that proxy should implement.
        /// </summary>
        public Type ServiceInterface
        {
            get { return _serviceInterface; }
            set { _serviceInterface = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Net.NetworkCredential"/> instance
        /// to use when connecting to a server that requires authentication.
        /// </summary>
        public NetworkCredential Credential
        {
            get { return _credential; }
            set { _credential = value; }
        }

        /// <summary>
        /// Gets or sets the url of the proxy server to use for retrieving the WSDL.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This only applies when using an <see cref="Spring.Core.IO.UrlResource"/> as uri.
        /// </p>
        /// <p>
        /// The default is to use the system proxy setting.
        /// </p>
        /// </remarks>
        public string ProxyUrl
        {
            get { return _proxyUrl; }
            set { _proxyUrl = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Net.NetworkCredential"/> instance
        /// to use when connecting to a proxy server that requires authentication.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This only applies when using an <see cref="Spring.Core.IO.UrlResource"/> as uri.
        /// </p>
        /// </remarks>
        public NetworkCredential ProxyCredential
        {
            get { return _proxyCredential; }
            set { _proxyCredential = value; }
        }

        /// <summary>
        /// Gets or sets the web service binding name to use for the proxy.
        /// </summary>
        public string BindingName
        {
            get { return _bindingName; }
            set { _bindingName = value; }
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
        /// that should be applied to web service members.
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

        #endregion

        #region IConfigurableFactoryObject Members

        /// <summary>
        /// Returns type of the web service proxy.
        /// </summary>
        public virtual Type ObjectType
        {
            get { return (proxyConstructor != null ? proxyConstructor.DeclaringType : ServiceInterface); }
        }

        /// <summary>
        /// Creates new instance of the web service proxy.
        /// </summary>
        /// <returns>New instance of the web service proxy.</returns>
        public virtual object GetObject()
        {
            if (proxyConstructor == null)
            {
                GenerateProxy();
            }

            return ObjectUtils.InstantiateType(proxyConstructor, ObjectUtils.EmptyObjects);
        }

        /// <summary>
        /// Always returns false.
        /// </summary>
        public virtual bool IsSingleton
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the template object definition
        /// that should be used to configure proxy instance.
        /// </summary>
        public virtual IObjectDefinition ProductTemplate
        {
            get { return _productTemplate; }
            set { _productTemplate = value; }
        }

        #endregion

        #region IInitializingObject Members

        /// <summary>
        /// Initializes factory object.
        /// </summary>
        public virtual void AfterPropertiesSet()
        {
            ValidateConfiguration();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        protected virtual void ValidateConfiguration()
        {
            if (ServiceUri == null && ProxyType == null)
            {
                throw new ArgumentException("ServiceUri or ProxyType property is required.");
            }
            if (ServiceInterface == null)
            {
                throw new ArgumentException("The ServiceInterface property is required.");
            }
            if (!ServiceInterface.IsInterface)
            {
                throw new ArgumentException("ServiceInterface must be an interface");
            }
            if (WebServiceProxyBaseType.IsSealed)
            {
                throw new ArgumentException("Web service client proxy cannot be created for a sealed class [" + WebServiceProxyBaseType.FullName + "]");
            }
        }

        /// <summary>
        /// Generates the web service proxy type.
        /// </summary>
        protected virtual void GenerateProxy()
        {
            IProxyTypeBuilder builder;
            if (ProxyType != null)
            {
                // Wrap .NET generated proxy class or another
                builder = new WebServiceProxyProxyTypeBuilder();
                builder.TargetType = ProxyType;
            }
            else
            {
                // Dynamically generates proxy class from WSDL
                builder = new SoapHttpClientProxyTypeBuilder(
                    ServiceUri, GetWsDocuments(ServiceUri), BindingName);
                builder.BaseType = WebServiceProxyBaseType;
            }

            builder.Interfaces = ReflectionUtils.ToInterfaceArray(ServiceInterface);
            builder.TypeAttributes = TypeAttributes;
            builder.MemberAttributes = MemberAttributes;

            Type wrapper = builder.BuildProxyType();

            proxyConstructor = wrapper.GetConstructor(Type.EmptyTypes);

            #region Instrumentation

            if (LOG.IsDebugEnabled)
            {
                if (ServiceUri != null)
                {
                    LOG.Debug(
                        String.Format("Generated client proxy type [{0}] for web service [{1}]", wrapper.FullName,
                                      ServiceUri.Description));
                }
                else if (ProxyType != null)
                {
                    LOG.Debug(
                        String.Format("Generated client proxy type [{0}] for web service based on provided proxy type [{1}]", wrapper.FullName,
                                      ProxyType.FullName));
                }
            }

            #endregion
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets XML Web Services documents from a Spring resource.
        /// </summary>
        private DiscoveryClientDocumentCollection GetWsDocuments(IResource resource)
        {
            try
            {
                if (ServiceUri is UrlResource ||
                    ServiceUri is FileSystemResource)
                {
                    DiscoveryClientProtocol dcProtocol = new DiscoveryClientProtocol();
                    dcProtocol.AllowAutoRedirect = true;
                    dcProtocol.Credentials = CreateCredentials();
                    dcProtocol.Proxy = ConfigureProxy();
                    dcProtocol.DiscoverAny(resource.Uri.AbsoluteUri);
                    dcProtocol.ResolveAll();

                    return dcProtocol.Documents;
                }
                else
                {
                    DiscoveryClientDocumentCollection dcdc = new DiscoveryClientDocumentCollection();
                    dcdc[resource.Description] = ServiceDescription.Read(resource.InputStream);
                    return dcdc;
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format("Couldn't retrieve the description of the web service located at '{0}'.", resource.Description), ex);
            }
        }

        private IWebProxy ConfigureProxy()
        {
            IWebProxy webProxy = null;
            if (ProxyUrl != null)
            {
                webProxy = new WebProxy(ProxyUrl);
            }
            if (ProxyCredential != null)
            {
                if (webProxy == null)
                {
                    webProxy = WebRequest.DefaultWebProxy;
                }
                webProxy.Credentials = ProxyCredential;
            }

            return webProxy;
        }

        private ICredentials CreateCredentials()
        {
            if (Credential != null)
            {
                CredentialCache credentialCache = new CredentialCache();

                Uri wsUri = new Uri(ServiceUri.Uri.AbsoluteUri.Substring(0, ServiceUri.Uri.AbsoluteUri.Length - ServiceUri.Uri.AbsolutePath.Length));
                IEnumerator enumerator = AuthenticationManager.RegisteredModules;
                while (enumerator.MoveNext())
                {
                    credentialCache.Add(wsUri, ((IAuthenticationModule)enumerator.Current).AuthenticationType, Credential);
                }

                return credentialCache;
            }
            else
            {
                return CredentialCache.DefaultCredentials;
            }
        }

        #endregion

        #region SoapHttpClientProxyTypeBuilder inner class implementation

        /// <summary>
        /// Proxy type builder that can be used to create a proxy for
        /// <see cref="System.Web.Services.Protocols.SoapHttpClientProtocol"/> derived classes.
        /// </summary>
        private sealed class SoapHttpClientProxyTypeBuilder : AbstractProxyTypeBuilder
        {
            #region Logging

            private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(SoapHttpClientProxyTypeBuilder));

            #endregion

            #region Fields

            // Binding/Type related
            private IResource serviceUri;
            private ServiceDescriptionCollection wsDescriptions;
            private XmlSchemaImporter schemaImporter;
            private Binding wsBinding;
            private string wsUrl;

            // Operation/Method related
            private Operation operation;
            private OperationBinding operationBinding;
            private SoapOperationBinding soapOperationBinding;
            private XmlMembersMapping inputMembersMapping;
            private XmlMembersMapping outputMembersMapping;

            #endregion

            #region Constructor(s) / Destructor

            /// <summary>
            /// Creates a new instance of the
            /// <see cref="SoapHttpClientProxyTypeBuilder"/> class.
            /// </summary>
            /// <param name="serviceUri">The URI that contains the Web Service meta info (WSDL).</param>
            /// <param name="wsDocuments">The XML Web Service documents to use to create the proxy.</param>
            /// <param name="bindingName">The name of the Web Service binding to use.</param>
            public SoapHttpClientProxyTypeBuilder(IResource serviceUri,
                DiscoveryClientDocumentCollection wsDocuments, string bindingName)
            {
                this.serviceUri = serviceUri;

                Name = "SoapHttpClientProxy";
                ProxyTargetAttributes = false;

                Initialize(wsDocuments, bindingName);
            }

            #endregion

            #region IProxyTypeBuilder Members

            /// <summary>
            /// Creates the proxy type.
            /// </summary>
            /// <returns>The generated proxy class.</returns>
            public override Type BuildProxyType()
            {
                if (Interfaces == null || Interfaces.Count == 0)
                {
                    throw new ArgumentException(
                        "Web service client proxy must implement at least one interface.");
                }

                TypeBuilder typeBuilder = CreateTypeBuilder(Name, BaseType);

                // apply custom attributes to the proxy type.
                ApplyTypeAttributes(typeBuilder, TargetType);

                // create constructors
                ImplementConstructors(typeBuilder);

                // implement service interfaces
                foreach (Type intf in Interfaces)
                {
                    ImplementInterface(typeBuilder,
                        new SoapHttpClientProxyMethodBuilder(typeBuilder, this),
                        intf, TargetType);
                }

                return typeBuilder.CreateType();
            }

            #endregion

            #region IProxyTypeGenerator Members

            /// <summary>
            /// Generates the IL instructions that pushes
            /// the target instance on which calls should be delegated to.
            /// </summary>
            /// <param name="il">The IL generator to use.</param>
            public override void PushTarget(ILGenerator il)
            {
                PushProxy(il);
            }

            #endregion

            #region Protected Methods

            /// <summary>
            /// Implements constructors for the proxy class.
            /// </summary>
            /// <param name="builder">
            /// The <see cref="System.Reflection.Emit.TypeBuilder"/> to use.
            /// </param>
            protected override void ImplementConstructors(TypeBuilder builder)
            {
                MethodAttributes attributes = MethodAttributes.Public |
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                    MethodAttributes.RTSpecialName;
                ConstructorBuilder cb = builder.DefineConstructor(attributes,
                    CallingConventions.Standard, Type.EmptyTypes);

                ILGenerator il = cb.GetILGenerator();

                PushProxy(il);
                il.Emit(OpCodes.Call, this.BaseType.GetConstructor(Type.EmptyTypes));

                // Set Url Property
                PushProxy(il);
                il.Emit(OpCodes.Ldstr, this.wsUrl);
                il.Emit(OpCodes.Call, this.BaseType.GetMethod("set_Url", BindingFlags.Public | BindingFlags.Instance));

                il.Emit(OpCodes.Ret);
            }

            protected override void ApplyMethodAttributes(
                MethodBuilder methodBuilder, MethodInfo targetMethod)
            {
                MoveToMethod(targetMethod);

                base.ApplyMethodAttributes(methodBuilder, targetMethod);
            }

            protected override IList GetTypeAttributes(Type type)
            {
                IList attrs = base.GetTypeAttributes(type);

                // Add the WebServiceBindingAttribute
                attrs.Add(CreateWebServiceBindingAttribute(this.wsBinding));

                return attrs;
            }

            protected override IList GetMethodAttributes(MethodInfo method)
            {
                IList attrs = base.GetMethodAttributes(method);

                // Add the SoapMethodAttribute
                attrs.Add(CreateSoapMethodAttribute(
                    operation, operationBinding, soapOperationBinding, inputMembersMapping, outputMembersMapping));

                return attrs;
            }

            protected override IList GetMethodReturnTypeAttributes(MethodInfo method)
            {
                IList attrs = base.GetMethodReturnTypeAttributes(method);

                // Add the XmlElementAttribute if needed
                if (operation.Messages.Output != null)
                {
                    if (method.ReturnType != typeof(void))
                    {
                        if (outputMembersMapping.Count > 0)
                        {
                            XmlMemberMapping outMemberMapping = outputMembersMapping[0];
                            bool useMemberName = (outMemberMapping.MemberName != operation.Name + "Result");
                            bool useNamespace = outMemberMapping.Namespace != outputMembersMapping.Namespace;
                            bool useTypeNamespace = outMemberMapping.TypeNamespace != XmlSchema.Namespace &&
                                                    outMemberMapping.TypeNamespace != outputMembersMapping.Namespace;
                            if (useMemberName || useNamespace || useTypeNamespace)
                            {
                                if (outMemberMapping.TypeName.StartsWith ("ArrayOf", StringComparison.Ordinal))
                                {
                                    if (useMemberName || useNamespace)
                                    {
                                        ReflectionUtils.CustomAttributeBuilderBuilder cabb =
                                            new ReflectionUtils.CustomAttributeBuilderBuilder(typeof(XmlArrayAttribute));
                                        if (useMemberName)
                                        {
                                            cabb.AddPropertyValue("ElementName", outMemberMapping.MemberName);
                                        }
                                        if (useNamespace)
                                        {
                                            cabb.AddPropertyValue("Namespace", outMemberMapping.Namespace);
                                        }
                                        attrs.Add(cabb.Build());
                                    }
                                    if (useTypeNamespace)
                                    {
                                        ReflectionUtils.CustomAttributeBuilderBuilder cabb =
                                            new ReflectionUtils.CustomAttributeBuilderBuilder(typeof(XmlArrayItemAttribute));
                                        cabb.AddPropertyValue("Namespace", outMemberMapping.TypeNamespace);
                                        attrs.Add(cabb.Build());
                                    }
                                }
                                else
                                {
                                    if (useMemberName || useNamespace)
                                    {
                                        ReflectionUtils.CustomAttributeBuilderBuilder cabb =
                                            new ReflectionUtils.CustomAttributeBuilderBuilder(typeof(XmlElementAttribute));
                                        if (useMemberName)
                                        {
                                            cabb.AddPropertyValue("ElementName", outMemberMapping.MemberName);
                                        }
                                        if (useNamespace)
                                        {
                                            cabb.AddPropertyValue("Namespace", outMemberMapping.Namespace);
                                        }
                                        attrs.Add(cabb.Build());
                                    }
                                }
                            }
                        }
                    }
                }

                return attrs;
            }

/*
            protected override IList GetMethodParameterAttributes(MethodInfo method, ParameterInfo paramInfo)
            {
                IList attrs = base.GetMethodParameterAttributes(method, paramInfo);

                // Add the XmlElementAttribute if needed
                XmlMemberMapping inMemberMapping = inputMembersMapping[paramInfo.Position];
                if (inMemberMapping.Namespace != inputMembersMapping.Namespace)
                {
                    CustomAttributeBuilderBuilder cabb =
                                new CustomAttributeBuilderBuilder(typeof(XmlElementAttribute));
                    cabb.AddPropertyValue("Namespace", inMemberMapping.Namespace);

                    attrs.Add(cabb.Build());
                }

                return attrs;
            }
*/

            #endregion

            #region Private Methods

            private void Initialize(DiscoveryClientDocumentCollection wsDocuments, string bindingName)
            {
                // Service descriptions
                this.wsDescriptions = new ServiceDescriptionCollection();
                XmlSchemas schemas = new XmlSchemas();
                foreach (DictionaryEntry entry in wsDocuments)
                {
                    if (entry.Value is ServiceDescription)
                    {
                        this.wsDescriptions.Add((ServiceDescription)entry.Value);
                    }
                    if (entry.Value is XmlSchema)
                    {
                        schemas.Add((XmlSchema)entry.Value);
                    }
                }

                // XmlSchemaImporter
                foreach (ServiceDescription serviceDescription in this.wsDescriptions)
                {
                    foreach (XmlSchema schema in serviceDescription.Types.Schemas)
                    {
                        if (schemas[schema.TargetNamespace] == null)
                        {
                            schemas.Add(schema);
                        }
                    }
                }
                this.schemaImporter = new XmlSchemaImporter(schemas);

                this.wsBinding = GetWsBinding(this.wsDescriptions, bindingName);
                this.wsUrl = GetWsUrl(this.wsDescriptions, this.wsBinding);
            }

            /// <summary>
            /// Search and returns the binding for the specified name.
            /// </summary>
            private Binding GetWsBinding(ServiceDescriptionCollection serviceDescriptions, string bindingName)
            {
                if (bindingName != null)
                {
                    // Search for a specific binding
                    foreach (ServiceDescription description in serviceDescriptions)
                    {
                        foreach (Binding binding in description.Bindings)
                        {
                            if (binding.Name == bindingName)
                            {
                                return binding;
                            }
                        }
                    }
                    throw new ApplicationException(String.Format("Binding '{0}' does not exist in the WSDL document located at '{1}'", bindingName, this.serviceUri.Description));
                }
                else
                {
                    // Use the first one
                    foreach (ServiceDescription description in serviceDescriptions)
                    {
                        foreach (Binding binding in description.Bindings)
                        {
                            #region Instrumentation

                            if (LOG.IsInfoEnabled)
                            {
                                LOG.Info(String.Format("The binding '{0}', found in the WSDL document located at '{1}', will be use.", binding.Name, this.serviceUri.Description));
                            }

                            #endregion

                            return binding;
                        }
                    }
                    throw new ApplicationException(String.Format("No bindings exists in the WSDL document located at '{0}'", this.serviceUri.Description));
                }
            }

            /// <summary>
            /// Search and returns the url for the specified binding.
            /// </summary>
            private string GetWsUrl(ServiceDescriptionCollection serviceDescriptions, Binding binding)
            {
                foreach (ServiceDescription description in serviceDescriptions)
                {
                    foreach (Service service in description.Services)
                    {
                        foreach (Port port in service.Ports)
                        {
                            if (port.Binding.Name == binding.Name)
                            {
                                SoapAddressBinding soapAddress = (SoapAddressBinding)port.Extensions.Find(typeof(SoapAddressBinding));
                                return soapAddress.Location;
                            }
                        }
                    }
                }
                throw new ApplicationException(String.Format("No SoapAddressBinding has been found for binding '{0}' in the WSDL document located at '{1}'.", binding.Name, serviceUri));
            }

            /// <summary>
            /// Search and returns the operation that matches the specified method.
            /// </summary>
            private Operation GetOperation(ServiceDescriptionCollection descriptions, Binding binding, MethodInfo method)
            {
                PortType portType = descriptions.GetPortType(binding.Type);
                foreach (Operation operation in portType.Operations)
                {
                    if (operation.Name == method.Name)
                    {
                        return operation;
                    }
                }
                throw new ApplicationException(String.Format("No Operation has been found for the method '{0}' in the WSDL document located at '{1}'.", method.Name, serviceUri.Description));
            }

            /// <summary>
            /// Search and returns the OperationBinding that matches the specified Operation.
            /// </summary>
            private OperationBinding GetOperationBinding(Operation operation, Binding binding)
            {
                foreach (OperationBinding operationBinding in binding.Operations)
                {
                    if (operation.IsBoundBy(operationBinding))
                    {
                        return operationBinding;
                    }
                }
                throw new ApplicationException(String.Format("No OperationBinding matches the Operation '{0}' in the WSDL document located at '{2}'.", operation.Name, this.serviceUri.Description));
            }

            /// <summary>
            /// Search and returns the type mapping between method parameters/return value
            /// and the element parts of a literal-use SOAP message.
            /// </summary>
            private XmlMembersMapping GetMembersMapping(string messageName, MessagePartCollection messageParts, SoapBodyBinding soapBodyBinding, SoapBindingStyle soapBindingStyle)
            {
                if (soapBindingStyle == SoapBindingStyle.Rpc)
                {
                    SoapSchemaMember[] soapSchemaMembers = new SoapSchemaMember[messageParts.Count];
                    for (int i = 0; i < messageParts.Count; i++)
                    {
                        SoapSchemaMember ssm = new SoapSchemaMember();
                        ssm.MemberName = messageParts[i].Name;
                        ssm.MemberType = messageParts[i].Type;
                        soapSchemaMembers[i] = ssm;
                    }
                    return this.schemaImporter.ImportMembersMapping(messageName, soapBodyBinding.Namespace, soapSchemaMembers);
                }
                else
                {
                    return this.schemaImporter.ImportMembersMapping(messageParts[0].Element);
                }
            }

            private void MoveToMethod(MethodInfo targetMethod)
            {
                operation = GetOperation(this.wsDescriptions, this.wsBinding, targetMethod);
                operationBinding = GetOperationBinding(operation, this.wsBinding);
                soapOperationBinding = (SoapOperationBinding)operationBinding.Extensions.Find(typeof(SoapOperationBinding));

                string inputMessageName = (!StringUtils.IsNullOrEmpty(operationBinding.Input.Name) && (soapOperationBinding.Style != SoapBindingStyle.Rpc)) ? operationBinding.Input.Name : operation.Name;
                SoapBodyBinding inputSoapBodyBinding = (SoapBodyBinding)operationBinding.Input.Extensions.Find(typeof(SoapBodyBinding));

                if (inputSoapBodyBinding.Use != SoapBindingUse.Literal)
                {
                    throw new NotSupportedException("WebServiceProxyFactory only supports document-literal and rpc-literal SOAP messages to conform to WS-I Basic Profiles.");
                }

                Message inputMessage = this.wsDescriptions.GetMessage(operation.Messages.Input.Message);
                inputMembersMapping = GetMembersMapping(inputMessageName, inputMessage.Parts, inputSoapBodyBinding, soapOperationBinding.Style);

                outputMembersMapping = null;
                if (operation.Messages.Output != null)
                {
                    string outputMessageName = (!StringUtils.IsNullOrEmpty(operationBinding.Output.Name) && (soapOperationBinding.Style != SoapBindingStyle.Rpc)) ? operationBinding.Output.Name : (operation.Name + "Response");
                    SoapBodyBinding outputSoapBodyBinding = (SoapBodyBinding)operationBinding.Output.Extensions.Find(typeof(SoapBodyBinding));
                    Message outputMessage = this.wsDescriptions.GetMessage(operation.Messages.Output.Message);
                    outputMembersMapping = GetMembersMapping(outputMessageName, outputMessage.Parts, outputSoapBodyBinding, soapOperationBinding.Style);
                }
            }

            /// <summary>
            /// Creates a <see cref="WebServiceBindingAttribute"/> that should be applied to proxy type.
            /// </summary>
            private CustomAttributeBuilder CreateWebServiceBindingAttribute(Binding wsBinding)
            {
                ReflectionUtils.CustomAttributeBuilderBuilder cabb =
                    new ReflectionUtils.CustomAttributeBuilderBuilder(typeof(WebServiceBindingAttribute));

                cabb.AddContructorArgument(this.wsBinding.Name);
                cabb.AddPropertyValue("Namespace", this.wsBinding.ServiceDescription.TargetNamespace);

                return cabb.Build();
            }

            /// <summary>
            /// Creates a <see cref="SoapDocumentMethodAttribute"/> or a <see cref="SoapRpcMethodAttribute"/>
            /// that should be applied to proxy method.
            /// </summary>
            private static CustomAttributeBuilder CreateSoapMethodAttribute(Operation operation,
                OperationBinding operationBinding, SoapOperationBinding soapOperationBinding,
                XmlMembersMapping inputMembersMapping, XmlMembersMapping outputMembersMapping)
            {
                ReflectionUtils.CustomAttributeBuilderBuilder cabb;

                string inputMembersMappingElementName = inputMembersMapping.ElementName;
                string inputMembersMappingNamespace = inputMembersMapping.Namespace;

                if (soapOperationBinding.Style == SoapBindingStyle.Rpc)
                {
                    cabb = new ReflectionUtils.CustomAttributeBuilderBuilder(typeof(SoapRpcMethodAttribute));
                }
                else
                {
                    cabb = new ReflectionUtils.CustomAttributeBuilderBuilder(typeof(SoapDocumentMethodAttribute));
                    cabb.AddPropertyValue("ParameterStyle", SoapParameterStyle.Wrapped);
                }

                cabb.AddContructorArgument(soapOperationBinding.SoapAction);
                cabb.AddPropertyValue("Use", SoapBindingUse.Literal);

                if (inputMembersMappingElementName.Length > 0 &&
                    inputMembersMappingElementName != operation.Name)
                {
                    cabb.AddPropertyValue("RequestElementName", inputMembersMappingElementName);
                }

                if (inputMembersMappingNamespace != null)
                {
                    cabb.AddPropertyValue("RequestNamespace", inputMembersMappingNamespace);
                }

                if (outputMembersMapping == null)
                {
                    cabb.AddPropertyValue("OneWay", true);
                }
                else
                {
                    string outputMembersMappingElementName = outputMembersMapping.ElementName;
                    string outputMembersMappingNamespace = outputMembersMapping.Namespace;

                    if (outputMembersMappingElementName.Length > 0 &&
                        outputMembersMappingElementName != (operation.Name + "Response"))
                    {
                        cabb.AddPropertyValue("ResponseElementName", outputMembersMappingElementName);
                    }

                    if (outputMembersMappingNamespace != null)
                    {
                        cabb.AddPropertyValue("ResponseNamespace", outputMembersMappingNamespace);
                    }
                }

                return cabb.Build();
            }

            #endregion

            #region SoapHttpClientProxyMethodBuilder inner class implementation

            /// <summary>
            /// Proxy method builder that can be used to create a proxy method
            /// for web services operation invocation.
            /// </summary>
            private sealed class SoapHttpClientProxyMethodBuilder : AbstractProxyMethodBuilder
            {
                #region Fields

                private static readonly MethodInfo Invoke =
                    typeof(SoapHttpClientProtocol).GetMethod("Invoke", BindingFlags.NonPublic | BindingFlags.Instance);

                #endregion

                #region Constructor(s) / Destructor

                /// <summary>
                /// Creates a new instance of the method builder.
                /// </summary>
                /// <param name="typeBuilder">The type builder to use.</param>
                /// <param name="proxyGenerator">
                /// The <see cref="IProxyTypeGenerator"/> implementation to use.
                /// </param>
                public SoapHttpClientProxyMethodBuilder(
                    TypeBuilder typeBuilder, IProxyTypeGenerator proxyGenerator)
                    : base(typeBuilder, proxyGenerator, false)
                {
                }

                #endregion

                #region Protected Methods

                /// <summary>
                /// Generates the proxy method.
                /// </summary>
                /// <param name="il">The IL generator to use.</param>
                /// <param name="method">The method to proxy.</param>
                /// <param name="interfaceMethod">
                /// The interface definition of the method, if applicable.
                /// </param>
                protected override void GenerateMethod(
                    ILGenerator il, MethodInfo method, MethodInfo interfaceMethod)
                {
                    // In Parameters
                    ArrayList inParams = new ArrayList();
                    // Ref or Out Parameters
                    ArrayList refOutParams = new ArrayList();

                    foreach (ParameterInfo paramInfo in interfaceMethod.GetParameters())
                    {
                        if (paramInfo.IsRetval || paramInfo.IsOut)
                        {
                            refOutParams.Add(paramInfo);
                        }
                        else
                        {
                            inParams.Add(paramInfo);
                        }
                    }

                    proxyGenerator.PushTarget(il);

                    // Parameter #1
                    il.Emit(OpCodes.Ldstr, interfaceMethod.Name);

                    // Parameter #2
                    LocalBuilder parameters = il.DeclareLocal(typeof(Object[]));
                    il.Emit(OpCodes.Ldc_I4, inParams.Count);
                    il.Emit(OpCodes.Newarr, typeof(Object));
                    il.Emit(OpCodes.Stloc, parameters);

                    int paramIndex = 0;
                    foreach (ParameterInfo paramInfo in inParams)
                    {
                        il.Emit(OpCodes.Ldloc, parameters);
                        il.Emit(OpCodes.Ldc_I4, paramIndex);
                        il.Emit(OpCodes.Ldarg, paramInfo.Position + 1);
                        if (paramInfo.ParameterType.IsValueType)
                        {
                            il.Emit(OpCodes.Box, paramInfo.ParameterType);
                        }
                        il.Emit(OpCodes.Stelem_Ref);

                        paramIndex++;
                    }

                    il.Emit(OpCodes.Ldloc, parameters);

                    // Call Invoke method and save result
                    LocalBuilder results = il.DeclareLocal(typeof(Object[]));
                    il.EmitCall(OpCodes.Callvirt, Invoke, null);
                    il.Emit(OpCodes.Stloc, results);

                    int resultIndex = (interfaceMethod.ReturnType == typeof(void) ? 0 : 1);
                    foreach (ParameterInfo paramInfo in refOutParams)
                    {
                        il.Emit(OpCodes.Ldarg, paramInfo.Position + 1);
                        il.Emit(OpCodes.Ldloc, results);

                        // Cast / Unbox the return value
                        il.Emit(OpCodes.Ldc_I4, resultIndex);
                        il.Emit(OpCodes.Ldelem_Ref);

                        Type elementType = paramInfo.ParameterType.GetElementType();
                        if (elementType.IsValueType)
                        {
                            il.Emit(OpCodes.Unbox, elementType);
                            il.Emit(OpCodes.Ldobj, elementType);
                            il.Emit(OpCodes.Stobj, elementType);
                        }
                        else
                        {
                            il.Emit(OpCodes.Castclass, elementType);
                            il.Emit(OpCodes.Stind_Ref);
                        }

                        resultIndex++;
                    }

                    if (interfaceMethod.ReturnType != typeof(void))
                    {
                        il.Emit(OpCodes.Ldloc, results);

                        // Cast / Unbox the return value
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ldelem_Ref);
                        if (interfaceMethod.ReturnType.IsValueType)
                        {
                            il.Emit(OpCodes.Unbox, interfaceMethod.ReturnType);
                            il.Emit(OpCodes.Ldobj, interfaceMethod.ReturnType);
                        }
                        else
                        {
                            il.Emit(OpCodes.Castclass, interfaceMethod.ReturnType);
                        }
                    }
                }

                #endregion
            }

            #endregion
        }

        #endregion

        #region WebServiceProxyProxyTypeBuilder inner class implementation

        /// <summary>
        /// Proxy type builder that can be used to create a proxy for
        /// .Net-generated proxy class that can be safely cast to a service interface.
        /// </summary>
        private sealed class WebServiceProxyProxyTypeBuilder : InheritanceProxyTypeBuilder
        {
            #region Constructor(s) / Destructor

            /// <summary>
            /// Creates a new instance of the
            /// <see cref="WebServiceProxyProxyTypeBuilder"/> class.
            /// </summary>
            public WebServiceProxyProxyTypeBuilder()
            {
                this.Name = "WebServiceProxyProxy";
                this.DeclaredMembersOnly = true;
            }

            #endregion

            #region Protected Methods

            /// <summary>
            /// Gets the mapping of the interface to proxy
            /// into the actual methods on the target type
            /// that does not need to implement that interface.
            /// </summary>
            /// <remarks>
            /// <p>
            /// As the proxy type does not implement the interface,
            /// we try to find matching methods.
            /// </p>
            /// </remarks>
            /// <param name="targetType">
            /// The <see cref="System.Type"/> of the target object.
            /// </param>
            /// <param name="intf">The interface to implement.</param>
            /// <returns>
            /// An interface mapping for the interface to proxy.
            /// </returns>
            protected override InterfaceMapping GetInterfaceMapping(
                Type targetType, Type intf)
            {
                InterfaceMapping mapping;

                mapping.TargetType = targetType;
                mapping.InterfaceType = intf;
                mapping.InterfaceMethods = intf.GetMethods();
                mapping.TargetMethods = ReflectionUtils.GetMatchingMethods(
                    targetType, mapping.InterfaceMethods, true);

                return mapping;
            }

            #endregion
        }

        #endregion
    }
}
