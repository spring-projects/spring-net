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
using System.Globalization;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.Services;

using Spring.Context;
using Spring.Context.Support;
using Spring.Core;
using Spring.Core.TypeResolution;
using Spring.Objects;
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
    [WebServiceBinding(ConformsTo=WsiProfiles.BasicProfile1_1)]
    public class WebServiceExporter : IInitializingObject, IObjectFactoryAware, IFactoryObject, IObjectNameAware
    {
        #region Fields

#if NET_2_0
        private WsiProfiles _wsiProfile = WsiProfiles.BasicProfile1_1;
#endif
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
        protected string objectName;
        
        /// <summary>
        /// The owning factory.
        /// </summary>
        protected IObjectFactory objectFactory;
        
        /// <summary>
        /// The generated web service wrapper type.
        /// </summary>
        protected Type proxyType;

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Web.Services.WebServiceExporter"/> class.
        /// </summary>
        public WebServiceExporter()
        {}

        #endregion

        #region Properties

#if NET_2_0
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
#endif

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
            set { this.objectFactory = value; }
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
        public virtual object GetObject()
        {
            // no sense to call this method, because the web service type 
            // will be instantiated by the .NET infrastructure. (ObjectType is used instead)
            // Users should use GetObject("TargetName") instead.
            return new InvalidOperationException(
                "The web service instance is created and managed by the .NET infrastructure.");
        }

        /// <summary>
        /// Return the <see cref="System.Type"/> of object that this
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
        /// <see langword="null"/> if not known in advance.
        /// </summary>
        public virtual Type ObjectType
        {
            get { return (proxyType != null ? proxyType : objectFactory.GetType(TargetName)); }
        }

        /// <summary>
        /// Is the object managed by this factory a singleton or a prototype?
        /// </summary>
        public virtual bool IsSingleton
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
        }

        /// <summary>
        /// Generates the web service wrapper type.
        /// </summary>
        protected virtual void GenerateProxy()
        {
#if NET_2_0
            IProxyTypeBuilder builder = new WebServiceProxyTypeBuilder(TargetName, Description, Name, Namespace, WsiProfile);
#else
            IProxyTypeBuilder builder = new WebServiceProxyTypeBuilder(TargetName, Description, Name, Namespace);
#endif
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

        #endregion

        #region WebServiceProxyTypeBuilder inner class implementation

        private sealed class WebServiceProxyTypeBuilder : CompositionProxyTypeBuilder
        {
            #region Fields

            private static readonly MethodInfo GetObject = 
                typeof(IObjectFactory).GetMethod("GetObject", new Type[] {typeof(string)});

            private static readonly MethodInfo GetApplicationContext =
                typeof(WebApplicationContext).GetProperty("Current", BindingFlags.Public | BindingFlags.Static, null, typeof(IApplicationContext), Type.EmptyTypes, null).GetGetMethod();

            private string targetName;
            private CustomAttributeBuilder webServiceAttribute;
#if NET_2_0
            private CustomAttributeBuilder webServiceBindingAttribute;
#endif

            #endregion

            #region Constructor(s) / Destructor

            public WebServiceProxyTypeBuilder(
                string targetName, string description, string name, string ns)
            {
                this.targetName = targetName;

                // Creates a WebServiceAttribute from configuration info
                this.webServiceAttribute = CreateWebServiceAttribute(description, name, ns);
            }
#if NET_2_0
            public WebServiceProxyTypeBuilder(
                string targetName, string description, string name, string ns, WsiProfiles wsiProfile)
            {
                this.targetName = targetName;

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
#endif

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
            /// This implementation generates a constructor 
            /// that gets instance of the target object using 
            /// <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>.
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
                il.EmitCall(OpCodes.Call, GetApplicationContext, null);
                il.Emit(OpCodes.Ldstr, targetName);
                il.EmitCall(OpCodes.Callvirt, GetObject, null);
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
#if NET_2_0
                    else if (IsAttributeMatchingType(attrs[i], typeof(WebServiceBindingAttribute)))
                    {
                        containsWebServiceBindingAttribute = true;
                    }
#endif
                }

                // Add missing WebServiceAttribute
                if (!containsWebServiceAttribute)
                {
                    attrs.Add(webServiceAttribute);
                }

#if NET_2_0
                // Add missing WebServiceBindingAttribute
                if (!containsWebServiceBindingAttribute)
                {
                    attrs.Add(webServiceBindingAttribute);
                }
#endif

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