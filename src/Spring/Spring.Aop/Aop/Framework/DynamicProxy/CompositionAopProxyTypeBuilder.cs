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
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

using Spring.Util;

#endregion

namespace Spring.Aop.Framework.DynamicProxy
{
	/// <summary>
    /// Builds an AOP proxy type using composition.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
    /// <author>Bruno Baia</author>
	public class CompositionAopProxyTypeBuilder : AbstractAopProxyTypeBuilder
    {
        #region Fields

        private const string PROXY_TYPE_NAME = "CompositionAopProxy";

        private readonly IAdvised advised;

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the 
        /// <see cref="CompositionAopProxyTypeBuilder"/> class.
        /// </summary>
        /// <param name="advised">The proxy configuration.</param>
        public CompositionAopProxyTypeBuilder(IAdvised advised)
        {
            this.advised = advised;
            
            Name = PROXY_TYPE_NAME;
            BaseType = typeof(BaseCompositionAopProxy);
            TargetType = advised.TargetSource.TargetType.IsInterface ? typeof(object) : advised.TargetSource.TargetType;
            Interfaces = GetProxiableInterfaces(advised.Interfaces);
            ProxyTargetAttributes = advised.ProxyTargetAttributes;
        }

        #endregion

        #region IProxyTypeBuilder Members

        /// <summary>
        /// Creates the proxy type.
        /// </summary>
        /// <returns>The generated proxy type.</returns>
        public override Type BuildProxyType()
		{
            IDictionary targetMethods = new Hashtable();

            TypeBuilder typeBuilder = CreateTypeBuilder(Name, BaseType);

            // apply custom attributes to the proxy type.
            ApplyTypeAttributes(typeBuilder, TargetType);

            if (advised.IsSerializable)
            {
                typeBuilder.SetCustomAttribute(
                    ReflectionUtils.CreateCustomAttribute(typeof(SerializableAttribute)));
                ImplementSerializationConstructor(typeBuilder);
            }

			// create constructors
			ImplementConstructors(typeBuilder);

			// implement interfaces
            IDictionary interfaceMap = advised.InterfaceMap;
			foreach (Type intf in Interfaces)
			{
				object target = interfaceMap[intf];
				if (target == null)
				{
                    // implement interface
					ImplementInterface(typeBuilder, 
                        new TargetAopProxyMethodBuilder(typeBuilder, this, false, targetMethods), 
                        intf, TargetType);
				}
				else if (target is IIntroductionAdvisor)
				{
                    // implement introduction
					ImplementInterface(typeBuilder,
                        new IntroductionProxyMethodBuilder(typeBuilder, this, targetMethods, advised.IndexOf((IIntroductionAdvisor) target)),
                        intf, TargetType);
				}
			}
			
			Type proxyType;
            proxyType = typeBuilder.CreateTypeInfo();

            // set target method references
            foreach (DictionaryEntry entry in targetMethods)
			{
				FieldInfo field = proxyType.GetField((string) entry.Key, BindingFlags.NonPublic | BindingFlags.Static);
				field.SetValue(proxyType, entry.Value);
			}

			return proxyType;
		}

		#endregion

        #region IAopProxyTypeGenerator Members

        /// <summary>
        /// Generates the IL instructions that pushes  
        /// the current <see cref="Spring.Aop.Framework.DynamicProxy.AdvisedProxy"/> 
        /// instance on stack.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        public override void PushAdvisedProxy(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Implements serialization constructor.
        /// </summary>
        /// <param name="typeBuilder">Type builder to use.</param>
        private void ImplementSerializationConstructor(TypeBuilder typeBuilder)
        {
            ConstructorBuilder cb =
                typeBuilder.DefineConstructor(MethodAttributes.Family,
                                              CallingConventions.Standard,
                                              new Type[] { typeof(SerializationInfo), typeof(StreamingContext) });

            ILGenerator il = cb.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, References.BaseCompositionAopProxySerializationConstructor);
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Implements constructors for the proxy class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation calls the base constructor.
        /// </p>
        /// </remarks>
        /// <param name="typeBuilder">
        /// The <see cref="System.Type"/> builder to use.
        /// </param>
        protected override void ImplementConstructors(TypeBuilder typeBuilder)
        {
            ConstructorBuilder cb =
                typeBuilder.DefineConstructor(References.BaseCompositionAopProxyConstructor.Attributes,
                                              References.BaseCompositionAopProxyConstructor.CallingConvention,
                                              new Type[] { typeof(IAdvised) });

            ILGenerator il = cb.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, References.BaseCompositionAopProxyConstructor);
            il.Emit(OpCodes.Ret);
        }

		#endregion

        #region Public Methods

        /// <summary>
        /// Determines if the specified <paramref name="type"/> 
        /// is one of those generated by this builder.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <see langword="true"/> if the type is a composition-based proxy;
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool IsCompositionProxy(Type type)
        {
            return type.FullName.StartsWith(PROXY_TYPE_NAME);
        }

        #endregion
	}
}