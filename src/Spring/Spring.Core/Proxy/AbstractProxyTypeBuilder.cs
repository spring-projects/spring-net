#region License

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

#endregion

#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Common.Logging;
using Spring.Core.TypeResolution;
using Spring.Util;

#endregion

namespace Spring.Proxy
{
	/// <summary>
	/// Base class for proxy builders that can be used 
    /// to create a proxy for any class.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This <see langword="abstract"/> class provides a set of template
	/// methods that derived classes can override to provide custom behaviour
	/// appropriate to the type of proxy that is being generated (one of
	/// inheritance or composition-based proxying).
	/// </p>
	/// </remarks>
	/// <author>Aleksandar Seovic</author>
    /// <author>Bruno Baia</author>
	public abstract class AbstractProxyTypeBuilder : IProxyTypeBuilder, IProxyTypeGenerator
	{
        #region Fields

        /// <summary>
        /// The shared <see cref="Common.Logging.ILog"/> instance for this class (and derived classes).
        /// </summary>
        protected static readonly ILog log = LogManager.GetLogger(typeof(AbstractProxyTypeBuilder));

        private const string DEFAULT_PROXY_TYPE_NAME = "Proxy";
        
        private string _name;
		private Type _targetType;
		private Type _baseType = typeof (object);
        private IList<Type> _interfaces;
        private bool _proxyTargetAttributes = true;
		private IList _typeAttributes = new ArrayList();
		private IDictionary _memberAttributes = new Hashtable();

        #endregion

        #region IProxyTypeBuilder Members

        /// <summary>
        /// Creates the proxy type.
        /// </summary>
        /// <returns>The generated proxy class.</returns>
        public abstract Type BuildProxyType();

        /// <summary>
        /// The name of the proxy <see cref="System.Type"/>.
        /// </summary>
        /// <value>The name of the proxy <see cref="System.Type"/>.</value>
        public string Name
        {
            get 
            {
                if (StringUtils.IsNullOrEmpty(_name))
                {
                    _name = DEFAULT_PROXY_TYPE_NAME;
                }
                return _name; 
            }
            set { _name = value; }
        }

        /// <summary>
        /// The <see cref="System.Type"/> of the target object.
        /// </summary>
        public Type TargetType
        {
            get { return _targetType; }
            set { _targetType = value; }
        }

        /// <summary>
        /// The <see cref="System.Type"/> of the class that the proxy must
        /// inherit from.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default value of this property is the
        /// <see cref="System.Object"/> <see cref="System.Type"/>.
        /// </p>
        /// </remarks>
        public Type BaseType
        {
            get { return _baseType; }
            set { _baseType = value; }
        }

        /// <summary>
        /// Gets or sets the list of interfaces proxy should implement.
        /// </summary>
        /// <remarks>
        /// The default value of this property is all the interfaces 
        /// implemented or inherited by the target type.
        /// </remarks>
        public IList<Type> Interfaces
        {
            get
            {
                if (_interfaces == null)
                {
                    _interfaces = GetProxiableInterfaces(TargetType.GetInterfaces());
                }
                return _interfaces;
            }
            set { _interfaces = value; }
        }

        /// <summary>
        /// Should we proxy target attributes?
        /// </summary>
        /// <see cref="IProxyTypeBuilder.ProxyTargetAttributes"/>
        public bool ProxyTargetAttributes
        {
            get { return _proxyTargetAttributes; }
            set { _proxyTargetAttributes = value; }
        }

        /// <summary>
        /// The list of custom <see cref="System.Attribute"/>s that the proxy
        /// class must be decorated with.
        /// </summary>
        /// <see cref="IProxyTypeBuilder.TypeAttributes"/>
        public IList TypeAttributes
        {
            get { return _typeAttributes; }
            set { _typeAttributes = value; }
        }

        /// <summary>
        /// The custom <see cref="System.Attribute"/>s that the proxy
        /// members must be decorated with.
        /// </summary>
        /// <see cref="IProxyTypeBuilder.MemberAttributes"/>
        public IDictionary MemberAttributes
        {
            get { return _memberAttributes; }
            set { _memberAttributes = value; }
        }

        #endregion

        #region IProxyTypeGenerator Members

        /// <summary>
        /// Generates the IL instructions that pushes 
        /// the proxy instance on stack.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        public virtual void PushProxy(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
        }

        /// <summary>
        /// Generates the IL instructions that pushes 
        /// the target instance on which calls should be delegated to.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        public abstract void PushTarget(ILGenerator il);

        #endregion

        #region Protected Methods

        #region Type generation

        /// <summary>
        /// Creates an appropriate type builder.
        /// </summary>
        /// <param name="name">The name to use for the proxy type name.</param>
        /// <param name="baseType">The type to extends if provided.</param>
        /// <returns>The type builder to use.</returns>
        protected virtual TypeBuilder CreateTypeBuilder(string name, Type baseType)
        {
            // Generates unique type name
            string typeName = String.Format("{0}_{1}", 
                name, Guid.NewGuid().ToString("N"));

            return DynamicProxyManager.CreateTypeBuilder(typeName, baseType);
        }

        #endregion

        #region Attributes generation

        /// <summary>
		/// Applies attributes to the proxy class.
		/// </summary>
        /// <param name="typeBuilder">The type builder to use.</param>
        /// <param name="targetType">The proxied class.</param>
        /// <see cref="IProxyTypeBuilder.ProxyTargetAttributes"/>
        /// <see cref="IProxyTypeBuilder.TypeAttributes"/>
		protected virtual void ApplyTypeAttributes(TypeBuilder typeBuilder, Type targetType)
		{
            foreach (object attr in GetTypeAttributes(targetType))
			{
				if (attr is CustomAttributeBuilder)
				{
                    typeBuilder.SetCustomAttribute((CustomAttributeBuilder)attr);
				}
                else if (attr is CustomAttributeData)
                {
                    typeBuilder.SetCustomAttribute(
                        ReflectionUtils.CreateCustomAttribute((CustomAttributeData)attr));
                }
				else if (attr is Attribute)
				{
                    typeBuilder.SetCustomAttribute(ReflectionUtils.CreateCustomAttribute((Attribute)attr));
				}
			}
		}

		/// <summary>
		/// Applies attributes to the proxied method.
		/// </summary>
        /// <param name="methodBuilder">The method builder to use.</param>
        /// <param name="targetMethod">The proxied method.</param>
        /// <see cref="IProxyTypeBuilder.ProxyTargetAttributes"/>
        /// <see cref="IProxyTypeBuilder.MemberAttributes"/>
        protected virtual void ApplyMethodAttributes(MethodBuilder methodBuilder, MethodInfo targetMethod)
		{
            foreach (object attr in GetMethodAttributes(targetMethod))
            {
                if (attr is CustomAttributeBuilder)
                {
                    methodBuilder.SetCustomAttribute((CustomAttributeBuilder)attr);
                }
                else if (attr is CustomAttributeData)
                {
                    methodBuilder.SetCustomAttribute(
                        ReflectionUtils.CreateCustomAttribute((CustomAttributeData)attr));
                }
                else if (attr is Attribute)
                {
                    methodBuilder.SetCustomAttribute(
                        ReflectionUtils.CreateCustomAttribute((Attribute)attr));
                }
            }

            ApplyMethodReturnTypeAttributes(methodBuilder, targetMethod);
            ApplyMethodParameterAttributes(methodBuilder, targetMethod);
        }

        /// <summary>
        /// Applies attributes to the proxied method's return type.
        /// </summary>
        /// <param name="methodBuilder">The method builder to use.</param>
        /// <param name="targetMethod">The proxied method.</param>
        /// <see cref="IProxyTypeBuilder.ProxyTargetAttributes"/>
        protected virtual void ApplyMethodReturnTypeAttributes(MethodBuilder methodBuilder, MethodInfo targetMethod)
        {
            ParameterBuilder parameterBuilder = methodBuilder.DefineParameter(0, ParameterAttributes.Retval, null);
            foreach (object attr in GetMethodReturnTypeAttributes(targetMethod))
            {
                if (attr is CustomAttributeBuilder)
                {
                    parameterBuilder.SetCustomAttribute((CustomAttributeBuilder)attr);
                }
                else if (attr is CustomAttributeData)
                {
                    parameterBuilder.SetCustomAttribute(
                        ReflectionUtils.CreateCustomAttribute((CustomAttributeData)attr));
                }
                else if (attr is Attribute)
                {
                    parameterBuilder.SetCustomAttribute(
                        ReflectionUtils.CreateCustomAttribute((Attribute)attr));
                }
            }
        }

        /// <summary>
        /// Applies attributes to proxied method's parameters.
        /// </summary>
        /// <param name="methodBuilder">The method builder to use.</param>
        /// <param name="targetMethod">The proxied method.</param>
        /// <see cref="IProxyTypeBuilder.ProxyTargetAttributes"/>
        protected virtual void ApplyMethodParameterAttributes(MethodBuilder methodBuilder, MethodInfo targetMethod)
        {
            foreach (ParameterInfo paramInfo in targetMethod.GetParameters())
            {
                ParameterBuilder parameterBuilder = methodBuilder.DefineParameter(
                    (paramInfo.Position + 1), paramInfo.Attributes, paramInfo.Name);
                foreach (object attr in GetMethodParameterAttributes(targetMethod, paramInfo))
                {
                    if (attr is CustomAttributeBuilder)
                    {
                        parameterBuilder.SetCustomAttribute((CustomAttributeBuilder)attr);
                    }
                    else if (attr is CustomAttributeData)
                    {
                        parameterBuilder.SetCustomAttribute(
                            ReflectionUtils.CreateCustomAttribute((CustomAttributeData)attr));
                    }
                    else if (attr is Attribute)
                    {
                        parameterBuilder.SetCustomAttribute(
                            ReflectionUtils.CreateCustomAttribute((Attribute)attr));
                    }
                }
            }
        }

        /// <summary>
        /// Calculates and returns the list of attributes that apply to the
        /// specified type.
        /// </summary>
        /// <param name="type">The type to find attributes for.</param>
        /// <returns>
        /// A list of custom attributes that should be applied to type.
        /// </returns>
        /// <see cref="IProxyTypeBuilder.ProxyTargetAttributes"/>
        /// <see cref="IProxyTypeBuilder.TypeAttributes"/>
        protected virtual IList GetTypeAttributes(Type type)
        {
            ArrayList attributes = new ArrayList();

            if (this.ProxyTargetAttributes && !type.Equals(typeof(object)))
            {
                // add attributes that apply to the target type
                attributes.AddRange(ReflectionUtils.GetCustomAttributes(type));
            }

            // add attributes defined by configuration
            attributes.AddRange(TypeAttributes);

            return attributes;
        }

        /// <summary>
        /// Calculates and returns the list of attributes that apply to the
        /// specified method.
        /// </summary>
        /// <param name="method">The method to find attributes for.</param>
        /// <returns>
        /// A list of custom attributes that should be applied to method.
        /// </returns>
        /// <see cref="IProxyTypeBuilder.ProxyTargetAttributes"/>
        /// <see cref="IProxyTypeBuilder.MemberAttributes"/>
        protected virtual IList GetMethodAttributes(MethodInfo method)
        {
            ArrayList attributes = new ArrayList();

            if (this.ProxyTargetAttributes && 
                !method.DeclaringType.IsInterface)
            {
                // add attributes that apply to the target method
                attributes.AddRange(ReflectionUtils.GetCustomAttributes(method));
            }

            // add attributes defined by configuration
            foreach (DictionaryEntry entry in MemberAttributes)
            {
                if (TypeResolutionUtils.MethodMatch((string)entry.Key, method))
                {
                    if (entry.Value is Attribute)
                    {
                        attributes.Add(entry.Value);
                    }
                    else if (entry.Value is IList)
                    {
                        attributes.AddRange(entry.Value as IList);
                    }
                }
            }

            return attributes;
        }

        /// <summary>
        /// Calculates and returns the list of attributes that apply to the
        /// specified method's return type.
        /// </summary>
        /// <param name="method">The method to find attributes for.</param>
        /// <returns>
        /// A list of custom attributes that should be applied to method's return type.
        /// </returns>
        /// <see cref="IProxyTypeBuilder.ProxyTargetAttributes"/>
        protected virtual IList GetMethodReturnTypeAttributes(MethodInfo method)
        {
            ArrayList attributes = new ArrayList();

            if (this.ProxyTargetAttributes &&
                !method.DeclaringType.IsInterface)
            {
                // add attributes that apply to the target method' return type
                object[] attrs = method.ReturnTypeCustomAttributes.GetCustomAttributes(false);
                try
                {
                    System.Collections.Generic.IList<CustomAttributeData> attrsData = 
                        CustomAttributeData.GetCustomAttributes(method.ReturnParameter);
                    
                    if (attrs.Length != attrsData.Count)
                    {
                        // http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=94803
                        attributes.AddRange(attrs);
                    }
                    else
                    {
                        foreach (CustomAttributeData cad in attrsData)
                        {
                            attributes.Add(cad);
                        }
                    }
                }
                catch (ArgumentException)
                {
                    // http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=296032
                    attributes.AddRange(attrs);
                }
            }

            // TODO: add attributes defined by configuration

            return attributes;
        }

        /// <summary>
        /// Calculates and returns the list of attributes that apply to the
        /// specified method's parameters.
        /// </summary>
        /// <param name="method">The method to find attributes for.</param>
        /// <param name="paramInfo">The method's parameter to find attributes for.</param>
        /// <returns>
        /// A list of custom attributes that should be applied to the specified method's parameter.
        /// </returns>
        /// <see cref="IProxyTypeBuilder.ProxyTargetAttributes"/>
        protected virtual IList GetMethodParameterAttributes(MethodInfo method, ParameterInfo paramInfo)
        {
            ArrayList attributes = new ArrayList();

            if (this.ProxyTargetAttributes &&
                !method.DeclaringType.IsInterface)
            {
                // add attributes that apply to the target method's parameter
                object[] attrs = paramInfo.GetCustomAttributes(false);
                try
                {
                    IList<CustomAttributeData> attrsData = CustomAttributeData.GetCustomAttributes(paramInfo);
                    
                    if (attrs.Length != attrsData.Count)
                    {
                        // http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=94803
                        attributes.AddRange(attrs);
                    }
                    else
                    {
                        foreach (CustomAttributeData cad in attrsData)
                        {
                            attributes.Add(cad);
                        }
                    }
                }
                catch (ArgumentException)
                {
                    // http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=296032
                    attributes.AddRange(attrs);
                }
            }

            // TODO: add attributes defined by configuration

            return attributes;
        }

        /// <summary>
        /// Check that the specified object is matching the passed attribute type.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The specified object can be of different type :
        /// </p>
        /// <list type="bullet">
        /// <item>
        /// <see cref="System.Attribute"/>
        /// </item>
        /// <item>
        /// System.Reflection.CustomAttributeData (Only with .NET 2.0)
        /// </item>
        /// <item>
        /// <see cref="System.Reflection.Emit.CustomAttributeBuilder"/>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="attr">The object instance to check.</param>
        /// <param name="attrType">The attribute type to test against.</param>
        /// <returns>
        /// <see langword="true"/> if the object instance matches the attribute type;
        /// otherwise <see langword="false"/>.
        /// </returns>
        protected virtual bool IsAttributeMatchingType(object attr, Type attrType)
        {
            if (attr is Attribute)
            {
                return (attrType == attr.GetType());
            }
            else if (attr is CustomAttributeData)
            {
                return (attrType == ((CustomAttributeData)attr).Constructor.DeclaringType);
            }
            else if (attr is CustomAttributeBuilder)
            {
                return (attrType == ((ConstructorInfo)CustomAttributeConstructorField.GetValue(attr)).DeclaringType);
            }
            return false;
        }

        private static readonly FieldInfo CustomAttributeConstructorField = 
            typeof(CustomAttributeBuilder).GetField("m_con", BindingFlags.Instance | BindingFlags.NonPublic);

        #endregion

        #region Constructors generation

        /// <summary>
        /// Defines the types of the parameters for the specified constructor.
        /// </summary>
        /// <param name="constructor">The constructor to use.</param>
        /// <returns>The types for constructor's parameters.</returns>
        protected virtual Type[] DefineConstructorParameters(ConstructorInfo constructor)
        {
            return ReflectionUtils.GetParameterTypes(constructor.GetParameters());
        }

        /// <summary>
        /// Implements constructors for the proxy class.
        /// </summary>
        /// <param name="typeBuilder">
        /// The <see cref="System.Type"/> builder to use.
        /// </param>
        protected virtual void ImplementConstructors(TypeBuilder typeBuilder)
        {
            ConstructorInfo[] constructors = TargetType.GetConstructors(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (ConstructorInfo constructor in constructors)
            {
                if (constructor.IsPublic || constructor.IsFamily)
                {
                    ConstructorBuilder cb = typeBuilder.DefineConstructor(
                        constructor.Attributes,
                        constructor.CallingConvention, 
                        DefineConstructorParameters(constructor));

                    ILGenerator il = cb.GetILGenerator();
                    GenerateConstructor(cb, il, constructor);
                    il.Emit(OpCodes.Ret);
                }
            }
        }

        /// <summary>
        /// Generates the proxy constructor.
        /// </summary>
        /// <param name="builder">The constructor builder to use.</param>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="constructor">The constructor to use.</param>
        protected virtual void GenerateConstructor(
            ConstructorBuilder builder, ILGenerator il, ConstructorInfo constructor)
        {
        }

        #endregion

        #region Members generation

        /// <summary>
        /// Implements an interface.
		/// </summary>
        /// <remarks>
        /// Generates proxy methods that belongs to the interface 
        /// using the specified <paramref name="proxyMethodBuilder"/>.
        /// </remarks>
        /// <param name="typeBuilder">The type builder to use.</param>
        /// <param name="proxyMethodBuilder">
        /// The <see cref="IProxyMethodBuilder"/> implementation to use
        /// </param>
        /// <param name="intf">The interface to implement.</param>
        /// <param name="targetType">
        /// The <see cref="System.Type"/> of the target object.
        /// </param>
        protected virtual void ImplementInterface(TypeBuilder typeBuilder, 
            IProxyMethodBuilder proxyMethodBuilder, Type intf, Type targetType)
        {
            ImplementInterface(typeBuilder, proxyMethodBuilder, intf, targetType, true);
        }

        /// <summary>
        /// Implements an interface.
		/// </summary>
        /// <remarks>
        /// Generates proxy methods that belongs to the interface 
        /// using the specified <paramref name="proxyMethodBuilder"/>.
        /// </remarks>
        /// <param name="typeBuilder">The type builder to use.</param>
        /// <param name="proxyMethodBuilder">
        /// The <see cref="IProxyMethodBuilder"/> implementation to use
        /// </param>
        /// <param name="intf">The interface to implement.</param>
        /// <param name="targetType">
        /// The <see cref="System.Type"/> of the target object.
        /// </param>
        /// <param name="proxyVirtualMethods">
        /// <see langword="false"/> if target virtual methods should not be proxied;
        /// otherwise <see langword="true"/>.
        /// </param>
		protected virtual void ImplementInterface(TypeBuilder typeBuilder,
            IProxyMethodBuilder proxyMethodBuilder, Type intf, 
            Type targetType, bool proxyVirtualMethods)
		{
            Dictionary<string, MethodBuilder> methodMap = new Dictionary<string, MethodBuilder>();

            InterfaceMapping mapping = GetInterfaceMapping(targetType, intf);

            typeBuilder.AddInterfaceImplementation(intf);

            for (int i = 0; i < mapping.InterfaceMethods.Length; i++)
			{
                if (!proxyVirtualMethods && 
                    !mapping.TargetMethods[i].DeclaringType.IsInterface &&
                    mapping.TargetMethods[i].IsVirtual &&
                    !mapping.TargetMethods[i].IsFinal)
                    continue;

                MethodBuilder methodBuilder = proxyMethodBuilder.BuildProxyMethod(
                    mapping.TargetMethods[i], mapping.InterfaceMethods[i]);

                ApplyMethodAttributes(methodBuilder, mapping.TargetMethods[i]);

                methodMap[mapping.InterfaceMethods[i].Name] = methodBuilder;
			}
			foreach (PropertyInfo property in intf.GetProperties())
			{
                ImplementProperty(typeBuilder, intf, property, methodMap);
			}
			foreach (EventInfo evt in intf.GetEvents())
			{
                ImplementEvent(typeBuilder, intf, evt, methodMap);
			}
		}

        /// <summary>
        /// Gets the mapping of the interface to proxy 
        /// into the actual methods on the target type 
        /// that does not need to implement that interface.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the target type does not implement the interface, 
        /// we return the interfaces methods as the target methods for many reasons :
        /// <ul>
        /// <li>
        /// The target object can change for an object that implements the interface.
        /// (See 'Spring.Aop.Framework.DynamicProxy.IAdvisedProxyMethodBuilder' 
        /// implementation in the Spring AOP framework for an example)
        /// </li>
        /// <li>
        /// Allow Transparent proxies to be proxied.
        /// (See Spring Remoting framework for an example)
        /// </li>
        /// <li>
        /// Allow null target to be proxied.
        /// (See Spring AOP framework which avoid calls to the target object 
        /// by intercepting all methods. Think "dynamic mock")
        /// (See 'Spring.Web.Services.WebServiceProxyFactory' implementation for another example)
        /// </li>
        /// </ul>
        /// </p>
        /// </remarks>
        /// <param name="targetType">
        /// The <see cref="System.Type"/> of the target object.
        /// </param>
        /// <param name="intf">The interface to implement.</param>
        /// <returns>
        /// An interface mapping for the interface to proxy.
        /// </returns>
        protected virtual InterfaceMapping GetInterfaceMapping(
            Type targetType, Type intf)
        {
            InterfaceMapping mapping;

            if (intf.IsAssignableFrom(targetType))
            {
                // target type implements the interface
                mapping = targetType.GetInterfaceMap(intf);
            }
            else
            {
                // target type does not implement the interface
                mapping.TargetType = targetType;
                mapping.InterfaceType = intf;
                mapping.InterfaceMethods = intf.GetMethods();
                mapping.TargetMethods = mapping.InterfaceMethods;
            }

            return mapping;
        }

        /// <summary>
		/// Inherit from a type.
		/// </summary>
        /// <remarks>
        /// Generates proxy methods for base virtual methods 
        /// using the specified <paramref name="proxyMethodBuilder"/>.
        /// </remarks>
        /// <param name="typeBuilder">
		/// The <see cref="System.Type"/> builder to use for code generation.
		/// </param>
        /// <param name="proxyMethodBuilder">
        /// The <see cref="IProxyMethodBuilder"/> implementation to use to override base virtual methods.
        /// </param>
        /// <param name="type">The <see cref="System.Type"/> to inherit from.</param>
        protected virtual void InheritType(TypeBuilder typeBuilder, 
            IProxyMethodBuilder proxyMethodBuilder, Type type)
        {
            InheritType(typeBuilder, proxyMethodBuilder, type, false);
        }

        /// <summary>
		/// Inherit from a type.
		/// </summary>
        /// <remarks>
        /// Generates proxy methods for base virtual methods 
        /// using the specified <paramref name="proxyMethodBuilder"/>.
        /// </remarks>
        /// <param name="typeBuilder">
		/// The <see cref="System.Type"/> builder to use for code generation.
		/// </param>
        /// <param name="proxyMethodBuilder">
        /// The <see cref="IProxyMethodBuilder"/> implementation to use to override base virtual methods.
        /// </param>
        /// <param name="type">The <see cref="System.Type"/> to inherit from.</param>
        /// <param name="declaredMembersOnly">
        /// <see langword="true"/> if only members declared at the level 
        /// of the supplied <paramref name="type"/>'s hierarchy should be proxied; 
        /// otherwise <see langword="false"/>.
        /// </param>
        protected virtual void InheritType(TypeBuilder typeBuilder,
            IProxyMethodBuilder proxyMethodBuilder, Type type, bool declaredMembersOnly)
        {
            IDictionary<string, MethodBuilder> methodMap = new Dictionary<string, MethodBuilder>();

            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            if (declaredMembersOnly)
            {
                bindingFlags |= BindingFlags.DeclaredOnly;
            }

            // override virtual methods
            MethodInfo[] methods = type.GetMethods(bindingFlags);
            foreach (MethodInfo method in methods)
            {
                MethodAttributes memberAccess = method.Attributes & MethodAttributes.MemberAccessMask;

                if (method.IsVirtual && !method.IsFinal && !method.Name.Equals("Finalize")
                    && (memberAccess == MethodAttributes.Public || memberAccess == MethodAttributes.Family || memberAccess == MethodAttributes.FamORAssem))
                {
                    MethodBuilder methodBuilder = proxyMethodBuilder.BuildProxyMethod(method, null);
                    ApplyMethodAttributes(methodBuilder, method);
                    methodMap[method.Name] = methodBuilder;
                }
            }
            // override virtual properties
            foreach (PropertyInfo property in type.GetProperties(bindingFlags))
            {
                ImplementProperty(typeBuilder, type, property, methodMap);
            }
            // override virtual events
            foreach (EventInfo evt in type.GetEvents(bindingFlags))
            {
                ImplementEvent(typeBuilder, type, evt, methodMap);
            }
        }

		/// <summary>
		/// Implements the specified <paramref name="property"/>.
		/// </summary>
		/// <param name="typeBuilder">The type builder to use.</param>
		/// <param name="type">The type the property is defined on.</param>
		/// <param name="property">The property to proxy.</param>
		/// <param name="methodMap">The implemented methods map.</param>
		protected virtual void ImplementProperty(
			TypeBuilder typeBuilder, Type type, PropertyInfo property, IDictionary<string, MethodBuilder> methodMap)
		{
            MethodBuilder getMethod;
            methodMap.TryGetValue("get_" + property.Name, out getMethod);
		    MethodBuilder setMethod;
            methodMap.TryGetValue("set_" + property.Name, out setMethod);

            if (getMethod != null || setMethod != null)
            {
                string propertyName = (type.IsInterface && 
                                      ((getMethod != null && getMethod.IsPrivate) || (setMethod != null && setMethod.IsPrivate)))
                                        ? type.FullName + "." + property.Name
                                        : property.Name;
                PropertyBuilder pb = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None,
                                                property.PropertyType, null);

                // set get/set methods
                if (property.CanRead && getMethod != null)
                {
                    pb.SetGetMethod(getMethod);
                }
                if (property.CanWrite && setMethod != null)
                {
                    pb.SetSetMethod(setMethod);
                }
            }
		}

	    /// <summary>
	    /// Implements the specified event.
	    /// </summary>
	    /// <param name="typeBuilder">The type builder to use.</param>
	    /// <param name="type">The type the event is defined on.</param>
	    /// <param name="evt">The event to proxy.</param>
	    /// <param name="methodMap">The implemented methods map.</param>
	    protected virtual void ImplementEvent(TypeBuilder typeBuilder, Type type, EventInfo evt, IDictionary<string, MethodBuilder> methodMap)
		{
            MethodBuilder addOnMethod;
            methodMap.TryGetValue("add_" + evt.Name, out addOnMethod);
            MethodBuilder removeOnMethod;
	        methodMap.TryGetValue("remove_" + evt.Name, out removeOnMethod);

            if (addOnMethod != null && removeOnMethod != null)
            {
                string eventName = (addOnMethod.IsPrivate) 
                    ? addOnMethod.DeclaringType.FullName + "." + evt.Name 
                    : evt.Name;

                EventBuilder eb = typeBuilder.DefineEvent(
                    eventName, EventAttributes.None, evt.EventHandlerType);
                
                // set add/remove methods
                eb.SetAddOnMethod(addOnMethod);
                eb.SetRemoveOnMethod(removeOnMethod);
            }		
		}

        #endregion

	    /// <summary>
	    /// Returns an array of <see cref="System.Type"/>s that represent 
	    /// the proxiable interfaces.
	    /// </summary>
	    /// <remarks>
	    /// An interface is proxiable if it's not marked with the 
	    /// <see cref="ProxyIgnoreAttribute"/>.
	    /// </remarks>
	    /// <param name="interfaces">
	    /// The array of interfaces from which 
	    /// we want to get the proxiable interfaces.
	    /// </param>
	    /// <returns>
	    /// An array containing the interface <see cref="System.Type"/>s.
	    /// </returns>
	    protected virtual IList<Type> GetProxiableInterfaces(IList<Type> interfaces)
        {
            List<Type>  proxiableInterfaces = new List<Type>();

            foreach(Type intf in interfaces)
            {
                if (!Attribute.IsDefined(intf, typeof(ProxyIgnoreAttribute), false) &&
                    !IsSpecialInterface(intf) &&
                    ReflectionUtils.IsTypeVisible(intf, DynamicProxyManager.ASSEMBLY_NAME))
                {
                    if (!proxiableInterfaces.Contains(intf))
                    {
                        proxiableInterfaces.Add(intf);
                    }

                    Type[] baseInterfaces = intf.GetInterfaces();
                    foreach (Type baseInterface in baseInterfaces)
                    {
                        if (!proxiableInterfaces.Contains(baseInterface))
                        {
                            proxiableInterfaces.Add(baseInterface);
                        }
                    }
                }
            }

            return proxiableInterfaces;
        }

        /// <summary>
        /// Checks if specified interface is of a special type 
        /// that should never be proxied (i.e. ISerializable).
        /// </summary>
        /// <param name="intf">Interface type to check.</param>
        /// <returns>
        /// <c>true</c> if it is, <c>false</c> otherwise.
        /// </returns>
        private bool IsSpecialInterface(Type intf)
	    {
	        return intf == typeof(ISerializable);
	    }

	    #endregion
    }
}