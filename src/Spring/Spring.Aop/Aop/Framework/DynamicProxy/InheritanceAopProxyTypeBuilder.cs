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

using System.Runtime.Serialization;
using System.Reflection;
using System.Reflection.Emit;

using Spring.Proxy;
using Spring.Util;

namespace Spring.Aop.Framework.DynamicProxy
{
    /// <summary>
    /// Builds an AOP proxy type using inheritance.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class InheritanceAopProxyTypeBuilder : AbstractAopProxyTypeBuilder
    {
        private const string PROXY_TYPE_NAME = "InheritanceAopProxy";

        private IAdvised advised;
        private bool proxyDeclaredMembersOnly = true;

        /// <summary>
        /// AdvisedProxy instance calls should be delegated to.
        /// </summary>
        protected FieldBuilder advisedProxyField;

        /// <summary>
        /// Gets or sets a value indicating whether inherited members should be proxied.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if inherited members should be proxied;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool ProxyDeclaredMembersOnly
        {
            get { return proxyDeclaredMembersOnly; }
            set { proxyDeclaredMembersOnly = value; }
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="CompositionAopProxyTypeBuilder"/> class.
        /// </summary>
        /// <param name="advised">The proxy configuration.</param>
        public InheritanceAopProxyTypeBuilder(IAdvised advised)
        {
            if (!ReflectionUtils.IsTypeVisible(advised.TargetSource.TargetType, DynamicProxyManager.ASSEMBLY_NAME))
            {
                throw new AopConfigException(String.Format(
                    "Cannot create inheritance-based IAopProxy for a non visible class [{0}]",
                    advised.TargetSource.TargetType.FullName));
            }
            if (advised.TargetSource.TargetType.IsSealed)
            {
                throw new AopConfigException(String.Format(
                    "Cannot create inheritance-based IAopProxy for a sealed class [{0}]",
                    advised.TargetSource.TargetType.FullName));
            }
            this.advised = advised;

            Name = PROXY_TYPE_NAME;
            TargetType = advised.TargetSource.TargetType;
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
            var proxyMethods = new Dictionary<string, MethodInfo>();

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
                object target = interfaceMap[intf];
                if (target == null)
                {
                    // implement interface (proxy only final methods)
                    ImplementInterface(typeBuilder,
                        new BaseAopProxyMethodBuilder(typeBuilder, this, targetMethods, proxyMethods),
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
                new BaseAopProxyMethodBuilder(typeBuilder, this, targetMethods, proxyMethods),
                TargetType, ProxyDeclaredMembersOnly);

            // implement IAdvised interface
            ImplementInterface(typeBuilder,
                new IAdvisedProxyMethodBuilder(typeBuilder, this),
                typeof(IAdvised), TargetType);

            // implement IAopProxy interface
            ImplementIAopProxy(typeBuilder);

            Type proxyType;
            proxyType = typeBuilder.CreateTypeInfo();

            // set target method references
            foreach (var entry in targetMethods)
            {
                FieldInfo field = proxyType.GetField(entry.Key, BindingFlags.NonPublic | BindingFlags.Static);
                field.SetValue(proxyType, entry.Value);
            }

            // set proxy method references
            foreach (var entry in proxyMethods)
            {
                FieldInfo field = proxyType.GetField(entry.Key, BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo proxyMethod = proxyType.GetMethod("proxy_" + entry.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                field.SetValue(proxyType, proxyMethod);
            }

            return proxyType;
        }

        /// <summary>
        /// Generates the IL instructions that pushes
        /// the target instance on which calls should be delegated to.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        public override void PushTarget(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
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
                                         typeof(void),
                                         new Type[] { typeof(SerializationInfo), typeof(StreamingContext) });

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
        /// Defines the types of the parameters for the specified constructor.
        /// </summary>
        /// <param name="constructor">The constructor to use.</param>
        /// <returns>The types for constructor's parameters.</returns>
        protected override Type[] DefineConstructorParameters(ConstructorInfo constructor)
        {
            Type[] currentParams = ReflectionUtils.GetParameterTypes(constructor.GetParameters());
            Type[] newParams = new Type[currentParams.Length + 1];

            newParams[currentParams.Length] = typeof(IAdvised);
            currentParams.CopyTo(newParams, 0);

            return newParams;
        }

        /// <summary>
        /// Generates the proxy constructor.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation creates instance of the AdvisedProxy object.
        /// </p>
        /// </remarks>
        /// <param name="builder">The constructor builder to use.</param>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="constructor">The constructor to delegate the creation to.</param>
        protected override void GenerateConstructor(
            ConstructorBuilder builder, ILGenerator il, ConstructorInfo constructor)
        {
            int paramCount = constructor.GetParameters().Length;
            il.Emit(OpCodes.Ldarg_0);
            for (int i = 0; i < paramCount; i++)
            {
                il.Emit(OpCodes.Ldarg_S, i + 1);
            }
            il.Emit(OpCodes.Call, constructor);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_S, paramCount + 1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Newobj, References.AdvisedProxyConstructor);
            il.Emit(OpCodes.Stfld, advisedProxyField);
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
        /// <see langword="true"/> if the type is a inheritance-based proxy;
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool IsInheritanceProxy(Type type)
        {
            return type.FullName.StartsWith(PROXY_TYPE_NAME);
        }
    }
}
