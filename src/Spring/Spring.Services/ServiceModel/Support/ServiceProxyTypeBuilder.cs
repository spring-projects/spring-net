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
using System.Reflection;
using System.Reflection.Emit;

using Spring.Objects.Factory;
using Spring.Proxy;
using Spring.Util;

namespace Spring.ServiceModel.Support
{
    /// <summary>
    /// Builds a WCF service type.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class ServiceProxyTypeBuilder : CompositionProxyTypeBuilder
    {
        #region Fields

        private static readonly MethodInfo GetObject =
            //typeof(IObjectFactory).GetMethod("GetObject", new Type[] { typeof(string) });
            ReflectionUtils.GetMethod(typeof (IObjectFactory), "GetObject", new Type[] {typeof (string)});

        private IObjectFactory objectFactory;
        private static Hashtable s_serviceTypeCache = new Hashtable();

        private string targetName;
        private bool useServiceProxyTypeCache;

        /// <summary>
        /// Target instance calls should be delegated to.
        /// </summary>
        protected FieldBuilder objectFactoryField;

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.ServiceModel.Support.ServiceProxyTypeBuilder"/> class.
        /// </summary>
        /// <param name="targetName">The name of the service within Spring's IoC container.</param>
        /// <param name="objectFactory">The <see cref="IObjectFactory"/> to use.</param>
        /// <param name="useServiceProxyTypeCache">Whether to cache the generated service proxy type.</param>
        public ServiceProxyTypeBuilder(string targetName, IObjectFactory objectFactory, bool useServiceProxyTypeCache)
            : this(targetName, targetName, objectFactory, useServiceProxyTypeCache)
        {
        }


        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.ServiceModel.Support.ServiceProxyTypeBuilder"/> class.
        /// </summary>
        /// <param name="targetName">The name of the service within Spring's IoC container.</param>
        /// <param name="serviceTypeName">The name of the generated WCF service <see cref="System.Type"/>.</param>
        /// <param name="objectFactory">The <see cref="IObjectFactory"/> to use.</param>
        /// <param name="useServiceProxyTypeCache">Whether to cache the generated service proxy type.</param>
        public ServiceProxyTypeBuilder(string targetName, string serviceTypeName, IObjectFactory objectFactory, bool useServiceProxyTypeCache)
        {
            this.targetName = targetName;
            this.objectFactory = objectFactory;
            this.useServiceProxyTypeCache = useServiceProxyTypeCache;

            this.Name = serviceTypeName;
            this.TargetType = objectFactory.GetType(targetName);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Creates a proxy that delegates calls to an instance of the target object.
        /// This overriden implementation caches the generated proxy type
        /// and sets the '__objectFactory' field.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// If the <see cref="IProxyTypeBuilder.TargetType"/>
        /// does not implement any interfaces.
        /// </exception>
        public override Type BuildProxyType()
        {
            Type proxyType = null;
            if (useServiceProxyTypeCache)
            {
                lock (s_serviceTypeCache)
                {
                    proxyType = (Type)s_serviceTypeCache[this.Name];
                    if (proxyType == null)
                    {
                        proxyType = base.BuildProxyType();
                        s_serviceTypeCache[this.Name] = proxyType;
                    }
                }
            }
            else
            {
                proxyType = base.BuildProxyType();
            }

            FieldInfo field = proxyType.GetField("__objectFactory", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(proxyType, this.objectFactory);

            return proxyType;
        }

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
            il.Emit(OpCodes.Ldsfld, objectFactoryField);
            il.Emit(OpCodes.Ldstr, targetName);
            il.EmitCall(OpCodes.Callvirt, GetObject, null);
            il.Emit(OpCodes.Stfld, targetInstance);

            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Creates an appropriate type builder. Add a field to hold a reference to the application context.
        /// </summary>
        /// <param name="name">The name to use for the proxy type name.</param>
        /// <param name="baseType">The type to extends if provided.</param>
        /// <returns>The type builder to use.</returns>
        protected override TypeBuilder CreateTypeBuilder(string name, Type baseType)
        {
            TypeBuilder typeBuilder = DynamicProxyManager.CreateTypeBuilder(name, baseType);

            objectFactoryField = typeBuilder.DefineField("__objectFactory", typeof(IObjectFactory),
                FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);

            return typeBuilder;
        }

        #endregion
    }
}
