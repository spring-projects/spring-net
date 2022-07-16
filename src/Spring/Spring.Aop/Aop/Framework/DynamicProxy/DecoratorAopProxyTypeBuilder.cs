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

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

using Spring.Util;
using Spring.Proxy;

namespace Spring.Aop.Framework.DynamicProxy
{
    /// <summary>
    /// Builds an AOP proxy type using the decorator pattern.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class DecoratorAopProxyTypeBuilder : AbstractAopProxyTypeBuilder
    {
        private const string PROXY_TYPE_NAME = "DecoratorAopProxy";

        private IAdvised advised;

        /// <summary>
        /// AdvisedProxy instance calls should be delegated to.
        /// </summary>
        protected FieldBuilder advisedProxyField;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="DecoratorAopProxyTypeBuilder"/> class.
        /// </summary>
        /// <param name="advised">The proxy configuration.</param>
        public DecoratorAopProxyTypeBuilder(IAdvised advised)
        {
            if (!ReflectionUtils.IsTypeVisible(advised.TargetSource.TargetType, DynamicProxyManager.ASSEMBLY_NAME))
            {
                throw new AopConfigException(String.Format(
                    "Cannot create decorator-based IAopProxy for a non visible class [{0}]",
                    advised.TargetSource.TargetType.FullName));
            }
            if (advised.TargetSource.TargetType.IsSealed)
            {
                throw new AopConfigException(String.Format(
                    "Cannot create decorator-based IAopProxy for a sealed class [{0}]",
                    advised.TargetSource.TargetType.FullName));
            }
            this.advised = advised;

            Name = PROXY_TYPE_NAME;
            TargetType = advised.TargetSource.TargetType.IsInterface ? typeof(object) : advised.TargetSource.TargetType;
            BaseType = TargetType;
            Interfaces = GetProxiableInterfaces(advised.Interfaces);
            ProxyTargetAttributes = advised.ProxyTargetAttributes;
        }

        /// <summary>
        /// Creates the proxy type.
        /// </summary>
        /// <returns>The generated proxy class.</returns>
        public override Type BuildProxyType()
        {
            var targetMethods = new Dictionary<string, MethodInfo>();

            TypeBuilder typeBuilder = CreateTypeBuilder(Name, BaseType);

            // apply custom attributes to the proxy type.
            ApplyTypeAttributes(typeBuilder, TargetType);

            // declare fields
            DeclareAdvisedProxyInstanceField(typeBuilder);

            // implement ISerializable if possible
            if (advised.IsSerializable)
            {
                typeBuilder.SetCustomAttribute(
                    ReflectionUtils.CreateCustomAttribute(typeof(SerializableAttribute)));
                ImplementSerializationConstructor(typeBuilder);
                ImplementGetObjectDataMethod(typeBuilder);
            }

            // create constructors
            ImplementConstructors(typeBuilder);

            // implement interfaces
            var interfaceMap = advised.InterfaceMap;
            foreach (Type intf in Interfaces)
            {
                interfaceMap.TryGetValue(intf, out var target);
                if (target is null)
                {
                    // implement interface (proxy only final methods)
                    ImplementInterface(typeBuilder,
                        new TargetAopProxyMethodBuilder(typeBuilder, this, true, targetMethods),
                        intf, TargetType, false);
                }
                else if (target is IIntroductionAdvisor)
                {
                    // implement introduction
                    ImplementInterface(typeBuilder,
                        new IntroductionProxyMethodBuilder(typeBuilder, this, targetMethods, advised.IndexOf((IIntroductionAdvisor)target)),
                        intf, TargetType);
                }
            }

            // inherit from target type
            InheritType(typeBuilder,
                new TargetAopProxyMethodBuilder(typeBuilder, this, false, targetMethods),
                TargetType);

            // implement IAdvised interface
            ImplementInterface(typeBuilder,
                new IAdvisedProxyMethodBuilder(typeBuilder, this),
                typeof(IAdvised), TargetType);

            // implement IAopProxy interface
            ImplementIAopProxy(typeBuilder);

            Type proxyType = typeBuilder.CreateTypeInfo();

            // set target method references
            foreach (var entry in targetMethods)
            {
                FieldInfo field = proxyType.GetField(entry.Key, BindingFlags.NonPublic | BindingFlags.Static);
                field.SetValue(proxyType, entry.Value);
            }

            return proxyType;
        }

        /// <summary>
        /// Generates the IL instructions that pushes
        /// the current <see cref="Spring.Aop.Framework.DynamicProxy.AdvisedProxy"/>
        /// instance on stack.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        public override void PushAdvisedProxy(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, advisedProxyField);
        }

        /// <summary>
        /// Declares field that holds the <see cref="Spring.Aop.Framework.DynamicProxy.AdvisedProxy"/>
        /// instance used by the proxy.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="System.Type"/> builder to use for code generation.
        /// </param>
        protected virtual void DeclareAdvisedProxyInstanceField(TypeBuilder builder)
        {
            advisedProxyField = builder.DefineField("__advisedProxy", typeof(AdvisedProxy), FieldAttributes.Private);
        }

        /// <summary>
        /// Implements serialization method.
        /// </summary>
        /// <param name="typeBuilder"></param>
        private void ImplementGetObjectDataMethod(TypeBuilder typeBuilder)
        {
            typeBuilder.AddInterfaceImplementation(typeof(ISerializable));

            MethodBuilder mb =
                typeBuilder.DefineMethod("GetObjectData",
                                         MethodAttributes.Public | MethodAttributes.HideBySig |
                                         MethodAttributes.NewSlot | MethodAttributes.Virtual,
                                         typeof (void),
                                         new Type[] {typeof (SerializationInfo), typeof (StreamingContext)});

            ILGenerator il = mb.GetILGenerator();
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldstr, "advisedProxy");
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, advisedProxyField);
            il.EmitCall(OpCodes.Callvirt, References.AddSerializationValue, null);
            il.Emit(OpCodes.Ret);

            //typeBuilder.DefineMethodOverride(mb, typeof(ISerializable).GetMethod("GetObjectData"));
        }


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
            il.Emit(OpCodes.Ldstr, "advisedProxy");
            il.Emit(OpCodes.Ldtoken, typeof(AdvisedProxy));
            il.EmitCall(OpCodes.Call, References.GetTypeFromHandle, null);
            il.EmitCall(OpCodes.Callvirt, References.GetSerializationValue, null);
            il.Emit(OpCodes.Castclass, typeof(AdvisedProxy));
            il.Emit(OpCodes.Stfld, advisedProxyField);
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Implements constructors for the proxy class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation creates a new instance
        /// of the <see cref="Spring.Aop.Framework.DynamicProxy.AdvisedProxy"/> class.
        /// </p>
        /// </remarks>
        /// <param name="typeBuilder">
        /// The <see cref="System.Type"/> builder to use.
        /// </param>
        protected override void ImplementConstructors(TypeBuilder typeBuilder)
        {
            ConstructorBuilder cb =
                typeBuilder.DefineConstructor(References.ObjectConstructor.Attributes,
                                              References.ObjectConstructor.CallingConvention,
                                              new Type[] { typeof(IAdvised) });

            ILGenerator il = cb.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Newobj, References.AdvisedProxyConstructor);
            il.Emit(OpCodes.Stfld, advisedProxyField);
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Implements <see cref="Spring.Aop.Framework.IAopProxy"/> interface.
        /// </summary>
        /// <param name="typeBuilder">The type builder to use.</param>
        protected virtual void ImplementIAopProxy(TypeBuilder typeBuilder)
        {
            Type intf = typeof(IAopProxy);
            MethodInfo getProxyMethod = intf.GetMethod("GetProxy", Type.EmptyTypes);

            typeBuilder.AddInterfaceImplementation(intf);

            MethodBuilder mb = typeBuilder.DefineMethod(typeof(IAdvised).FullName + "." + getProxyMethod.Name,
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                getProxyMethod.CallingConvention, getProxyMethod.ReturnType, Type.EmptyTypes);

            ILGenerator il = mb.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(mb, getProxyMethod);
        }

        /// <summary>
        /// Determines if the specified <paramref name="type"/>
        /// is one of those generated by this builder.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <see langword="true"/> if the type is a decorator-based proxy;
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool IsDecoratorProxy(Type type)
        {
            return type.FullName.StartsWith(PROXY_TYPE_NAME);
        }
    }
}
