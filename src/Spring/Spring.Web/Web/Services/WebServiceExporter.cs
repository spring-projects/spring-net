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

#region Imports

using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.Services;
using Spring.Core.TypeResolution;
using Spring.Objects.Factory;
using Spring.Util;
using Spring.Proxy;

#endregion

namespace Spring.Web.Services
{
    /// <summary>
    /// Exports an object as a web service.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The exporter will create a web service wrapper for the object that is
    /// to be exposed and additionally enable its configuration as a web
    /// service.
    /// </p>
    /// <p>
    /// The exported object can be either a standard .NET web service
    /// implementation, with methods marked using the standard
    /// <see cref="System.Web.Services.WebMethodAttribute"/>, or it can be a
    /// plain .NET object.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class WebServiceExporter : IInitializingObject, IObjectFactoryAware, IObjectNameAware, IDisposable
    {
        /// <summary>
        /// Holds EXPORTER_ID to WebServiceExporter instance mappings.
        /// </summary>
        private static readonly IDictionary s_activeExporters = new Hashtable();

        ///<summary>
        /// Returns the target object instance exported by the WebServiceExporter identified by <see cref="EXPORTER_ID"/>.
        ///</summary>
        ///<param name="exporterId"></param>
        ///<returns></returns>
        public static object GetTarget( string exporterId )
        {
            WebServiceExporter exporterInstance;
            lock (s_activeExporters.SyncRoot)
            {
                exporterInstance = (WebServiceExporter)s_activeExporters[exporterId];
            }
            if (exporterInstance == null)
            {
                throw new ArgumentNullException("exporterId", "WebService object is not associated with any active WebServiceExporter");
            }
            object target = exporterInstance.GetTargetInstance();
            if (target == null)
            {
                throw new ArgumentNullException("exporterId", string.Format( "Failed retrieving target object for WebServiceExporter ID {0}", exporterId ));
            }
            return target;
        }

        #region Fields

        private WsiProfiles _wsiProfile = WsiProfiles.BasicProfile1_1;
        private readonly string EXPORTER_ID = Guid.NewGuid().ToString();
        private Type _webServiceBaseType = typeof(WebService);
        private string _targetName;
        private string _description;
        private string _name;
        private string _namespace = WebServiceAttribute.DefaultNamespace;
        private string[] _interfaces;
        private IList _typeAttributes = new ArrayList();
        private IDictionary _memberAttributes = new Hashtable();

        /// <summary>
        /// The name of the object in the factory.
        /// </summary>
        private string objectName;

        /// <summary>
        /// The owning factory.
        /// </summary>
        private IObjectFactory objectFactory;

        /// <summary>
        /// The generated web service wrapper type.
        /// </summary>
        protected Type proxyType;

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the <see cref="WebServiceExporter"/> class.
        /// </summary>
        public WebServiceExporter()
        {
            lock (s_activeExporters.SyncRoot)
            {
                s_activeExporters[EXPORTER_ID] = this;
            }
        }

        /// <summary>
        /// Cleanup before GC
        /// </summary>
        ~WebServiceExporter()
        {
            Dispose( false );
        }

        #region IDisposable Members

        /// <summary>
        /// Disconnect the remote object from the registered remoting channels.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize( this );
            Dispose( true );
        }

        /// <summary>
        /// Stops exporting the object identified by <see cref="TargetName"/>.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose( bool disposing )
        {
            if (disposing)
            {
                lock (s_activeExporters.SyncRoot)
                {
                    s_activeExporters.Remove( this.EXPORTER_ID );
                }
                objectFactory = null;
            }
        }

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the Web Services Interoperability (WSI) specification
        /// to which the Web Service claims to conform.
        /// </summary>
        /// <remarks>
        /// Default is <see cref="System.Web.Services.WsiProfiles.BasicProfile1_1"/>
        /// </remarks>
        public WsiProfiles WsiProfile
        {
            get { return _wsiProfile; }
            set { _wsiProfile = value; }
        }

        /// <summary>
        /// Gets or sets the base type that web service should inherit.
        /// </summary>
        /// <remarks>
        /// Default is <see cref="System.Web.Services.WebService"/>
        /// </remarks>
        public Type WebServiceBaseType
        {
            get { return _webServiceBaseType; }
            set { _webServiceBaseType = value; }
        }

        /// <summary>
        /// Gets or sets the name of the target object that should be exposed as a web service.
        /// </summary>
        /// <value>
        /// The name of the target object that should be exposed as a web service.
        /// </value>
        public string TargetName
        {
            get { return _targetName; }
            set { _targetName = value; }
        }

        /// <summary>
        /// Gets or sets the description of the web service (optional).
        /// </summary>
        /// <value>
        /// The web service description.
        /// </value>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets or sets the name of the web service (optional).
        /// </summary>
        /// <remarks>
        /// <p>
        /// Defaults to the value of the exporter's object ID.
        /// </p>
        /// </remarks>
        /// <value>
        /// The web service name.
        /// </value>
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    _name = WebUtils.GetPageName(objectName);
                }
                return _name;
            }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the web service namespace.
        /// </summary>
        /// <value>
        /// The web service namespace.
        /// </value>
        public string Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        /// <summary>
        /// Gets or sets the list of interfaces whose methods should be exposed as web services.
        /// </summary>
        /// <remarks>
        /// If not set, all the interfaces implemented or inherited
        /// by the target type will be used.
        /// </remarks>
        /// <value>The interfaces.</value>
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
            protected get { return objectFactory; }
            set { objectFactory = value; }
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
            protected get { return objectName; }
            set { objectName = value; }
        }

        #endregion

        #region IInitializingObject Members

        /// <summary>
        /// Exports specified object as a web service.
        /// </summary>
        /// <exception cref="System.Exception">
        /// In the event of misconfiguration (such as failure to set an essential
        /// property) or if initialization fails.
        /// </exception>
        public virtual void AfterPropertiesSet()
        {
            ValidateConfiguration();
            GenerateProxy();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the Web Service wrapper type for the object that is to be exposed.
        /// </summary>
        /// <returns></returns>
        public virtual Type GetExportedType()
        {
            return (proxyType != null ? proxyType : objectFactory.GetType(TargetName));
        }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        protected virtual void ValidateConfiguration()
        {
            if (TargetName == null)
            {
                throw new ArgumentException("The TargetName property is required.");
            }
        }

        /// <summary>
        /// Generates the web service wrapper type.
        /// </summary>
        protected virtual void GenerateProxy()
        {
            IProxyTypeBuilder builder = new WebServiceProxyTypeBuilder(this, Description, Name, Namespace, WsiProfile);
            builder.Name = WebUtils.GetPageName(objectName);
            builder.BaseType = WebServiceBaseType;
            builder.TargetType = objectFactory.GetType(TargetName);
            if (Interfaces != null && Interfaces.Length > 0)
            {
                builder.Interfaces = TypeResolutionUtils.ResolveInterfaceArray(Interfaces);
            }
            builder.TypeAttributes = TypeAttributes;
            builder.MemberAttributes = MemberAttributes;

            proxyType = builder.BuildProxyType();
        }

        private object GetTargetInstance()
        {
            return objectFactory.GetObject(TargetName);
        }

        #endregion

        #region WebServiceProxyTypeBuilder inner class implementation

        private sealed class WebServiceProxyTypeBuilder : CompositionProxyTypeBuilder
        {
            #region Fields

            private static readonly MethodInfo WebServiceExporter_GetTargetInstance = typeof(WebServiceExporter).GetMethod( "GetTarget", new Type[] { typeof( string ) } );
            private WebServiceExporter exporter;
            private CustomAttributeBuilder webServiceAttribute;
            private CustomAttributeBuilder webServiceBindingAttribute;

            #endregion

            #region Constructor(s) / Destructor

            public WebServiceProxyTypeBuilder(WebServiceExporter exporter, string description, string name, string ns)
            {
                this.exporter = exporter;

                // Creates a WebServiceAttribute from configuration info
                this.webServiceAttribute = CreateWebServiceAttribute(description, name, ns);
            }

            public WebServiceProxyTypeBuilder(WebServiceExporter exporter, string description, string name, string ns, WsiProfiles wsiProfile)
            {
                this.exporter = exporter;

                // Creates a WebServiceAttribute from configuration info
                this.webServiceAttribute = CreateWebServiceAttribute(description, name, ns);

                // Creates a WebServiceAttribute from configuration info
                this.webServiceBindingAttribute = CreateWebServiceBindingAttribute(wsiProfile);
            }

            private static CustomAttributeBuilder CreateWebServiceBindingAttribute(WsiProfiles wsiProfile)
            {
                ReflectionUtils.CustomAttributeBuilderBuilder cabb =
                    new ReflectionUtils.CustomAttributeBuilderBuilder(typeof(WebServiceBindingAttribute));
                if (wsiProfile == WsiProfiles.BasicProfile1_1)
                {
                    cabb.AddPropertyValue("ConformsTo", wsiProfile);
                }
                return cabb.Build();
            }

            private static CustomAttributeBuilder CreateWebServiceAttribute(string description, string name, string ns)
            {
                ReflectionUtils.CustomAttributeBuilderBuilder cabb =
                    new ReflectionUtils.CustomAttributeBuilderBuilder(typeof(WebServiceAttribute));
                if (StringUtils.HasText(description))
                {
                    cabb.AddPropertyValue("Description", description);
                }
                if (StringUtils.HasText(name))
                {
                    cabb.AddPropertyValue("Name", name);
                }
                if (StringUtils.HasText(ns))
                {
                    cabb.AddPropertyValue("Namespace", ns);
                }
                return cabb.Build();
            }

            #endregion

            #region Protected Methods

            /// <summary>
            /// Implements constructors for the proxy class.
            /// </summary>
            /// <remarks>
            /// This implementation generates an empty noop default constructor
            /// </remarks>
            /// <param name="builder">
            /// The <see cref="System.Type"/> builder to use.
            /// </param>
            protected override void ImplementConstructors(TypeBuilder builder)
            {
                MethodAttributes attributes = MethodAttributes.Public |
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                    MethodAttributes.RTSpecialName;

                ConstructorBuilder cb = builder.DefineConstructor(
                    attributes, CallingConventions.Standard, Type.EmptyTypes);

                ILGenerator il = cb.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, this.exporter.EXPORTER_ID);
                il.Emit( OpCodes.Call, WebServiceExporter_GetTargetInstance );
                il.Emit(OpCodes.Stfld, targetInstance);

                il.Emit(OpCodes.Ret);
            }

            protected override IList GetTypeAttributes(Type type)
            {
                IList attrs = base.GetTypeAttributes(type);

                bool containsWebServiceAttribute = false;
                bool containsWebServiceBindingAttribute = false;

                for (int i = 0; i < attrs.Count; i++)
                {
                    if (IsAttributeMatchingType(attrs[i], typeof(WebServiceAttribute)))
                    {
                        // override existing WebServiceAttribute
                        containsWebServiceAttribute = true;
                        attrs[i] = webServiceAttribute;
                    }
                    else if (IsAttributeMatchingType(attrs[i], typeof(WebServiceBindingAttribute)))
                    {
                        containsWebServiceBindingAttribute = true;
                    }
                }

                // Add missing WebServiceAttribute
                if (!containsWebServiceAttribute)
                {
                    attrs.Add(webServiceAttribute);
                }

                // Add missing WebServiceBindingAttribute
                if (!containsWebServiceBindingAttribute)
                {
                    attrs.Add(webServiceBindingAttribute);
                }

                return attrs;
            }

            protected override IList GetMethodAttributes(MethodInfo method)
            {
                IList attrs = base.GetMethodAttributes(method);

                bool containsWebMethodAttribute = false;
                foreach (object attr in attrs)
                {
                    if (IsAttributeMatchingType(attr, typeof(WebMethodAttribute)))
                    {
                        containsWebMethodAttribute = true;
                        break;
                    }
                }

                // Creates default WebMethodAttribute if not set yet
                if (!containsWebMethodAttribute)
                {
                    attrs.Add(ReflectionUtils.CreateCustomAttribute(typeof(WebMethodAttribute)));
                }

                return attrs;
            }

            protected override TypeBuilder CreateTypeBuilder(string name, Type baseType)
            {
                return DynamicProxyManager.CreateTypeBuilder(name, baseType);
            }

            #endregion
        }

        #endregion
    }
}
