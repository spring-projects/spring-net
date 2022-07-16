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

using Spring.Proxy;

namespace Spring.Aop.Framework.DynamicProxy
{
    /// <summary>
    /// Base class for AOP method builders that contains common functionalities.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <author>Bruno Baia</author>
    public abstract class AbstractAopProxyMethodBuilder : AbstractProxyMethodBuilder
    {
        /// <summary>
        /// The <see cref="IAopProxyTypeGenerator"/> implementation to use.
        /// </summary>
        protected IAopProxyTypeGenerator aopProxyGenerator;

        /// <summary>
        /// The dictionary to cache the list of target
        /// <see cref="System.Reflection.MethodInfo"/>s.
        /// </summary>
        protected IDictionary<string, MethodInfo> targetMethods;

        /// <summary>
        /// The dictionary to cache the list of target
        /// <see cref="System.Reflection.MethodInfo"/>s defined on the proxy.
        /// </summary>
        protected IDictionary<string, MethodInfo> onProxyTargetMethods;

        // variables

        /// <summary>
        /// The local variable to store the list of method interceptors.
        /// </summary>
        protected LocalBuilder interceptors;

        /// <summary>
        /// The local variable to store the target type being proxied.
        /// </summary>
        protected LocalBuilder targetType;

        /// <summary>
        /// The local variable to store method arguments.
        /// </summary>
        protected LocalBuilder arguments;

        /// <summary>
        /// The local variable to store the return value.
        /// </summary>
        protected LocalBuilder returnValue;

        /// <summary>
        /// The local variable to store the closed generic method
        /// when the target method is generic.
        /// </summary>
        protected LocalBuilder genericTargetMethod;

        /// <summary>
        /// The local variable to store the closed generic method
        /// when the target method defined on the proxy is generic.
        /// </summary>
        protected LocalBuilder genericOnProxyTargetMethod;

        /// <summary>
        /// The field to cache the target <see cref="System.Reflection.MethodInfo"/>.
        /// </summary>
        protected FieldBuilder targetMethodCacheField;

        /// <summary>
        /// The field to cache the target <see cref="System.Reflection.MethodInfo"/>
        /// defined on the proxy.
        /// </summary>
        protected FieldBuilder onProxyTargetMethodCacheField;

        // convinience fields

        /// <summary>
        /// Indicates if the method returns a value.
        /// </summary>
        protected bool methodReturnsValue;

        // private fields

        private static Dictionary<Type, OpCode> ldindOpCodes;

        static AbstractAopProxyMethodBuilder()
        {
			ldindOpCodes = new Dictionary<Type, OpCode>();
            ldindOpCodes[typeof(sbyte)] = OpCodes.Ldind_I1;
            ldindOpCodes[typeof(short)] = OpCodes.Ldind_I2;
            ldindOpCodes[typeof(int)] = OpCodes.Ldind_I4;
            ldindOpCodes[typeof(long)] = OpCodes.Ldind_I8;
            ldindOpCodes[typeof(byte)] = OpCodes.Ldind_U1;
            ldindOpCodes[typeof(ushort)] = OpCodes.Ldind_U2;
            ldindOpCodes[typeof(uint)] = OpCodes.Ldind_U4;
            ldindOpCodes[typeof(ulong)] = OpCodes.Ldind_I8;
            ldindOpCodes[typeof(float)] = OpCodes.Ldind_R4;
            ldindOpCodes[typeof(double)] = OpCodes.Ldind_R8;
            ldindOpCodes[typeof(char)] = OpCodes.Ldind_U2;
            ldindOpCodes[typeof(bool)] = OpCodes.Ldind_I1;
        }

        /// <summary>
        /// Creates a new instance of the method builder.
        /// </summary>
        /// <param name="typeBuilder">The type builder to use.</param>
        /// <param name="aopProxyGenerator">
        /// The <see cref="IAopProxyTypeGenerator"/> implementation to use.
        /// </param>
        /// <param name="explicitImplementation">
        /// <see langword="true"/> if the interface is to be
        /// implemented explicitly; otherwise <see langword="false"/>.
        /// </param>
        /// <param name="targetMethods">
        /// The dictionary to cache the list of target
        /// <see cref="System.Reflection.MethodInfo"/>s.
        /// </param>
        protected AbstractAopProxyMethodBuilder(
            TypeBuilder typeBuilder, IAopProxyTypeGenerator aopProxyGenerator,
            bool explicitImplementation, IDictionary<string, MethodInfo> targetMethods)
            : this(typeBuilder, aopProxyGenerator, explicitImplementation, targetMethods, new Dictionary<string, MethodInfo>())
        {
        }

        /// <summary>
        /// Creates a new instance of the method builder.
        /// </summary>
        /// <param name="typeBuilder">The type builder to use.</param>
        /// <param name="aopProxyGenerator">
        /// The <see cref="IAopProxyTypeGenerator"/> implementation to use.
        /// </param>
        /// <param name="explicitImplementation">
        /// <see langword="true"/> if the interface is to be
        /// implemented explicitly; otherwise <see langword="false"/>.
        /// </param>
        /// <param name="targetMethods">
        /// The dictionary to cache the list of target
        /// <see cref="System.Reflection.MethodInfo"/>s.
        /// </param>
        /// <param name="onProxyTargetMethods">
        /// The dictionary to cache the list of target
        /// <see cref="System.Reflection.MethodInfo"/>s defined on the proxy.
        /// </param>
        protected AbstractAopProxyMethodBuilder(
            TypeBuilder typeBuilder, IAopProxyTypeGenerator aopProxyGenerator,
            bool explicitImplementation, IDictionary<string, MethodInfo> targetMethods, IDictionary<string, MethodInfo> onProxyTargetMethods)
            : base(typeBuilder, aopProxyGenerator, explicitImplementation)
        {
            this.aopProxyGenerator = aopProxyGenerator;
            this.targetMethods = targetMethods;
            this.onProxyTargetMethods = onProxyTargetMethods;
        }

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
            methodReturnsValue = (method.ReturnType != typeof(void));

            DeclareLocals(il, method);

            GenerateTargetMethodCacheField(il, method);
            GenerateOnProxyTargetMethodCacheField(il, method);

            BeginMethod(il, method);
            GenerateMethodLogic(il, method, interfaceMethod);
            EndMethod(il, method);
        }

        /// <summary>
        /// Generates unique method id for the cache field.
        /// </summary>
        /// <param name="method">The target method.</param>
        /// <returns>An unique method name.</returns>
        protected virtual string GenerateMethodCacheFieldId(MethodInfo method)
        {
            return "_m" + Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Create static field that will cache target method.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The target method.</param>
        protected virtual void GenerateTargetMethodCacheField(
            ILGenerator il, MethodInfo method)
        {
            string methodId = GenerateMethodCacheFieldId(method);
            targetMethods.Add(methodId, method);

            targetMethodCacheField = typeBuilder.DefineField(methodId, typeof(MethodInfo),
                FieldAttributes.Private | FieldAttributes.Static);

            MakeGenericMethod(il, method, targetMethodCacheField, genericTargetMethod);
        }

        /// <summary>
        /// Create static field that will cache target method when defined on the proxy.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The target method.</param>
        protected virtual void GenerateOnProxyTargetMethodCacheField(
            ILGenerator il, MethodInfo method)
        {
        }

        /// <summary>
        /// Create a closed generic method for the current call
        /// if target method is a generic definition.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The target method.</param>
        /// <param name="methodCacheField">
        /// The field that contains the method generic definition
        /// </param>
        /// <param name="localMethod">
        /// The local variable to store the closed generic method.
        /// </param>
        protected void MakeGenericMethod(ILGenerator il, MethodInfo method,
            FieldBuilder methodCacheField, LocalBuilder localMethod)
        {
            // if target method is a generic definition,
            // create a closed generic method for the current call.
            if (method.IsGenericMethodDefinition)
            {
                Type[] genericArgs = method.GetGenericArguments();

                LocalBuilder typeArgs = il.DeclareLocal(typeof(Type[]));

                il.Emit(OpCodes.Ldsfld, methodCacheField);

                // specify array size and create an array
                il.Emit(OpCodes.Ldc_I4, genericArgs.Length);
                il.Emit(OpCodes.Newarr, typeof(Type));
                il.Emit(OpCodes.Stloc, typeArgs);

                // populate array with type arguments
                for (int i = 0; i < genericArgs.Length; i++)
                {
                    il.Emit(OpCodes.Ldloc, typeArgs);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldtoken, genericArgs[i]);
                    il.EmitCall(OpCodes.Call, References.GetTypeFromHandle, null);
                    il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Ldloc, typeArgs);
                il.Emit(OpCodes.Callvirt, References.MakeGenericMethod);
                il.Emit(OpCodes.Stloc, localMethod);
            }
        }

        /// <summary>
        /// Generates the IL instructions that pushes
        /// the target type on stack.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        protected virtual void PushTargetType(ILGenerator il)
        {
            aopProxyGenerator.PushAdvisedProxy(il);
            il.Emit(OpCodes.Ldfld, References.TargetTypeField);
        }

        /// <summary>
        /// Generates the IL instructions that pushes
        /// the current <see cref="Spring.Aop.Framework.DynamicProxy.AdvisedProxy"/>
        /// instance on stack.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        protected virtual void PushAdvisedProxy(ILGenerator il)
        {
            aopProxyGenerator.PushAdvisedProxy(il);
        }

        /// <summary>
        /// Pushes the target <see cref="System.Reflection.MethodInfo"/> to stack.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        protected virtual void PushTargetMethodInfo(ILGenerator il, MethodInfo method)
        {
            if (method.IsGenericMethodDefinition)
            {
                il.Emit(OpCodes.Ldloc, genericTargetMethod);
                return;
            }
            il.Emit(OpCodes.Ldsfld, targetMethodCacheField);
        }

        /// <summary>
        /// Pushes the target <see cref="System.Reflection.MethodInfo"/> defined on the proxy to stack.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        protected virtual void PushOnProxyTargetMethodInfo(ILGenerator il, MethodInfo method)
        {
            if (onProxyTargetMethodCacheField != null)
            {
                if (method.IsGenericMethodDefinition)
                {
                    il.Emit(OpCodes.Ldloc, genericOnProxyTargetMethod);
                    return;
                }
                il.Emit(OpCodes.Ldsfld, onProxyTargetMethodCacheField);
            }
            else
            {
                il.Emit(OpCodes.Ldnull);
            }
        }

        /// <summary>
        /// Creates local variable declarations.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        protected virtual void DeclareLocals(ILGenerator il, MethodInfo method)
        {
            interceptors = il.DeclareLocal(typeof(System.Collections.IList));
            targetType = il.DeclareLocal(typeof(Type));
            arguments = il.DeclareLocal(typeof(Object[]));

            if (method.IsGenericMethodDefinition)
            {
                genericTargetMethod = il.DeclareLocal(typeof(MethodInfo));
                genericOnProxyTargetMethod = il.DeclareLocal(typeof(MethodInfo));
            }
            if (methodReturnsValue)
            {
                returnValue = il.DeclareLocal(method.ReturnType);
            }

#if DEBUG && !NETSTANDARD
            interceptors.SetLocalSymInfo("interceptors");
            targetType.SetLocalSymInfo("targetType");
            arguments.SetLocalSymInfo("arguments");

            if (method.IsGenericMethodDefinition)
            {
                genericTargetMethod.SetLocalSymInfo("genericTargetMethod");
                genericOnProxyTargetMethod.SetLocalSymInfo("genericOnProxyTargetMethod");
            }

            if (methodReturnsValue)
            {
                returnValue.SetLocalSymInfo("returnValue");
            }
#endif
        }

        /// <summary>
        /// Initializes local variables
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        protected virtual void BeginMethod(ILGenerator il, MethodInfo method)
        {
            Label jmpProxyNotExposed = il.DefineLabel();

            // set current proxy to this object
            PushAdvisedProxy(il);
            il.Emit(OpCodes.Ldfld, References.AdvisedField);
            il.EmitCall(OpCodes.Callvirt, References.ExposeProxyProperty, null);
            il.Emit(OpCodes.Brfalse_S, jmpProxyNotExposed);
            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(OpCodes.Call, References.PushProxyMethod, null);

            il.MarkLabel(jmpProxyNotExposed);

            // initialize targetType
            PushTargetType(il);
            il.Emit(OpCodes.Stloc, targetType);

            // initialize interceptors
            PushAdvisedProxy(il);
            il.Emit(OpCodes.Ldloc, targetType);
            PushTargetMethodInfo(il, method);
            il.EmitCall(OpCodes.Call, References.GetInterceptorsMethod, null);
            il.Emit(OpCodes.Stloc, interceptors);
        }

        /// <summary>
        /// Generates method logic.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        /// <param name="interfaceMethod">
        /// The interface definition of the method, if applicable.
        /// </param>
        protected virtual void GenerateMethodLogic(
            ILGenerator il, MethodInfo method, MethodInfo interfaceMethod)
        {
            Label jmpDirectCall = il.DefineLabel();
            Label jmpEndIf = il.DefineLabel();

            // check if there are any interceptors
            il.Emit(OpCodes.Ldloc, interceptors);
            il.EmitCall(OpCodes.Callvirt, References.CountProperty, null);
            il.Emit(OpCodes.Ldc_I4_0);

            // if not jump to direct call
            il.Emit(OpCodes.Ble, jmpDirectCall);

            // otherwise call Invoke and jump to method end
            CallInvoke(il, method);
            il.Emit(OpCodes.Br, jmpEndIf);

            // call method directly
            il.MarkLabel(jmpDirectCall);
            CallDirectProxiedMethod(il, method, interfaceMethod);
            if (methodReturnsValue)
            {
                // store return value, unboxing is not necessary because we called method directly
                il.Emit(OpCodes.Stloc, returnValue);
            }

            il.MarkLabel(jmpEndIf);

            if (methodReturnsValue)
            {
                if (!method.ReturnType.IsValueType)
                {
                    ProcessReturnValue(il, returnValue);
                }
            }
        }

        /// <summary>
        /// Calls method using Invoke
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        protected virtual void CallInvoke(ILGenerator il, MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            SetupMethodArguments(il, method, parameters);

            PushAdvisedProxy(il);

            // setup parameters for call
            il.Emit(OpCodes.Ldarg_0);                       // proxy
            PushTarget(il);                                 // target
            il.Emit(OpCodes.Ldloc, targetType);             // target type
            PushTargetMethodInfo(il, method);               // method
            PushOnProxyTargetMethodInfo(il, method);        // method defined on proxy
            il.Emit(OpCodes.Ldloc, arguments);              // args
            il.Emit(OpCodes.Ldloc, interceptors);           // interceptors

            // call Invoke
            il.EmitCall(OpCodes.Call, References.InvokeMethod, null);

            // process return value
            if (methodReturnsValue)
            {
                EmitUnboxIfNeeded(il, method.ReturnType);
                il.Emit(OpCodes.Stloc, returnValue);
            }
            else
            {
                il.Emit(OpCodes.Pop);
            }

            // process byRef arguments
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_S, i + 1);
                    il.Emit(OpCodes.Ldloc, arguments);
                    il.Emit(OpCodes.Ldc_I4_S, i);
                    il.Emit(OpCodes.Ldelem_Ref);
                    Type type = parameters[i].ParameterType.GetElementType();
                    EmitUnboxIfNeeded(il, type);
                    EmitStoreValueIndirect(il, type);
                }
            }
        }

        /// <summary>
        /// Setup proxied method arguments.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        /// <param name="parameters">The method's parameters.</param>
        protected void SetupMethodArguments(
            ILGenerator il, MethodInfo method, ParameterInfo[] parameters)
        {
            if (parameters.Length > 0)
            {
                // specify array size and create an array
                il.Emit(OpCodes.Ldc_I4, parameters.Length);
                il.Emit(OpCodes.Newarr, typeof(Object));
                il.Emit(OpCodes.Stloc, arguments);

                // populate array with params
                for (int i = 0; i < parameters.Length; i++)
                {
                    Type type = parameters[i].ParameterType;

                    il.Emit(OpCodes.Ldloc, arguments);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg_S, i + 1);

                    // setup byRef arguments
                    if (type.IsByRef)
                    {
                        type = type.GetElementType();
                        EmitLoadValueIndirect(il, type);
                    }

                    if (type.IsValueType || type.IsGenericParameter)
                    {
                        il.Emit(OpCodes.Box, type);
                    }

                    il.Emit(OpCodes.Stelem_Ref);
                }
            }
            else
            {
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stloc, arguments);
            }
        }

        /// <summary>
        /// Calls proxied method directly.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        /// <param name="interfaceMethod">
        /// The interface definition of the method, if applicable.
        /// </param>
        protected abstract void CallDirectProxiedMethod(
            ILGenerator il, MethodInfo method, MethodInfo interfaceMethod);

        /// <summary>
        /// Ends method by returning return value if appropriate.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        protected virtual void EndMethod(ILGenerator il, MethodInfo method)
        {
            Label jmpProxyNotExposed = il.DefineLabel();

            // reset current proxy to old value
            PushAdvisedProxy(il);
            il.Emit(OpCodes.Ldfld, References.AdvisedField);
            il.EmitCall(OpCodes.Callvirt, References.ExposeProxyProperty, null);
            il.Emit(OpCodes.Brfalse_S, jmpProxyNotExposed);
            il.EmitCall(OpCodes.Call, References.PopProxyMethod, null);

            il.MarkLabel(jmpProxyNotExposed);

            if (methodReturnsValue)
            {
                il.Emit(OpCodes.Ldloc, returnValue);
            }
        }

        /// <summary>
        /// Emits MSIL instructions to load a value of the specified <paramref name="type"/>
        /// onto the evaluation stack indirectly.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="type">The type of the value.</param>
        protected static void EmitLoadValueIndirect(ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                if (type == typeof(int)) il.Emit(OpCodes.Ldind_I4);
                else if (type == typeof(uint)) il.Emit(OpCodes.Ldind_U4);
                else if (type == typeof(char)) il.Emit(OpCodes.Ldind_I2);
                else if (type == typeof(bool)) il.Emit(OpCodes.Ldind_I1);
                else if (type == typeof(float)) il.Emit(OpCodes.Ldind_R4);
                else if (type == typeof(double)) il.Emit(OpCodes.Ldind_R8);
                else if (type == typeof(short)) il.Emit(OpCodes.Ldind_I2);
                else if (type == typeof(ushort)) il.Emit(OpCodes.Ldind_U2);
                else if (type == typeof(long) || type == typeof(ulong)) il.Emit(OpCodes.Ldind_I8);
                else il.Emit(OpCodes.Ldobj, type);
            }
            else
            {
                il.Emit(OpCodes.Ldind_Ref);
            }
        }

        /// <summary>
        /// Emit MSIL instructions to store a value of the specified <paramref name="type"/>
        /// at a supplied address.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="type">The type of the value.</param>
        protected static void EmitStoreValueIndirect(ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                if (type.IsEnum) EmitStoreValueIndirect(il, Enum.GetUnderlyingType(type));
                else if (type == typeof(int)) il.Emit(OpCodes.Stind_I4);
                else if (type == typeof(short)) il.Emit(OpCodes.Stind_I2);
                else if (type == typeof(long) || type == typeof(ulong)) il.Emit(OpCodes.Stind_I8);
                else if (type == typeof(char)) il.Emit(OpCodes.Stind_I2);
                else if (type == typeof(bool)) il.Emit(OpCodes.Stind_I1);
                else if (type == typeof(float)) il.Emit(OpCodes.Stind_R4);
                else if (type == typeof(double)) il.Emit(OpCodes.Stind_R8);
                else il.Emit(OpCodes.Stobj, type);
            }
            else
            {
                il.Emit(OpCodes.Stind_Ref);
            }
        }

        /// <summary>
        /// Emits MSIL instructions to convert the boxed representation
        /// of the supplied <paramref name="type"/> to its unboxed form.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="type">The type specified in the instruction.</param>
        protected static void EmitUnboxIfNeeded(ILGenerator il, Type type)
        {
            if (type.IsValueType || type.IsGenericParameter)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
        }
    }

    internal struct References
    {
        // fields
        public static readonly FieldInfo AdvisedField =
            typeof(AdvisedProxy).GetField("m_advised", BindingFlags.Instance | BindingFlags.Public);

        public static readonly FieldInfo TargetTypeField =
            typeof(AdvisedProxy).GetField("m_targetType", BindingFlags.Instance | BindingFlags.Public);

        public static readonly FieldInfo IntroductionsField =
            typeof(AdvisedProxy).GetField("m_introductions", BindingFlags.Instance | BindingFlags.Public);

        public static readonly FieldInfo TargetSourceField =
            typeof(AdvisedProxy).GetField("m_targetSource", BindingFlags.Instance | BindingFlags.Public);

        // constructors
        public static readonly ConstructorInfo BaseCompositionAopProxyConstructor =
            typeof(BaseCompositionAopProxy).GetConstructor(new Type[] { typeof(IAdvised) });

        public static readonly ConstructorInfo BaseCompositionAopProxySerializationConstructor =
            typeof(BaseCompositionAopProxy).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                                                            new Type[] {typeof(SerializationInfo), typeof(StreamingContext)},
                                                            null);

        public static readonly ConstructorInfo AdvisedProxyConstructor =
            typeof(AdvisedProxy).GetConstructor(new Type[] { typeof(IAdvised), typeof(IAopProxy) });

        public static readonly ConstructorInfo AdvisedProxySerializationConstructor =
            typeof(AdvisedProxy).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                                                 new Type[] {typeof (SerializationInfo), typeof (StreamingContext)},
                                                 null);
        public static readonly ConstructorInfo ObjectConstructor =
            typeof(Object).GetConstructor(Type.EmptyTypes);

        // methods
        public static readonly MethodInfo PushProxyMethod =
            typeof(AopContext).GetMethod("PushProxy", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Object) }, null);

        public static readonly MethodInfo PopProxyMethod =
            typeof(AopContext).GetMethod("PopProxy", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);

        public static readonly MethodInfo InvokeMethod =
            typeof(AdvisedProxy).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Object), typeof(Object), typeof(Type), typeof(MethodInfo), typeof(MethodInfo), typeof(Object[]), typeof(System.Collections.IList) }, null);

        public static readonly MethodInfo GetInterceptorsMethod =
            typeof(AdvisedProxy).GetMethod("GetInterceptors", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Type), typeof(MethodInfo) }, null);

        public static readonly MethodInfo GetTargetMethod =
            typeof(ITargetSource).GetMethod("GetTarget", Type.EmptyTypes);

        public static readonly MethodInfo GetReleaseTargetMethod =
            typeof(ITargetSource).GetMethod("ReleaseTarget", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Object) }, null);

        public static readonly MethodInfo GetTypeMethod =
            typeof(Object).GetMethod("GetType", Type.EmptyTypes);

        public static readonly MethodInfo GetTypeFromHandle =
            typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });

        public static readonly MethodInfo MakeGenericMethod =
            typeof(MethodInfo).GetMethod("MakeGenericMethod", new Type[] { typeof(Type[]) });

        public static readonly MethodInfo DisposeMethod =
            typeof(IDisposable).GetMethod("Dispose", Type.EmptyTypes);

        public static readonly MethodInfo AddSerializationValue =
            typeof(SerializationInfo).GetMethod("AddValue", new Type[] { typeof(string), typeof(object) });

        public static readonly MethodInfo GetSerializationValue =
            typeof(SerializationInfo).GetMethod("GetValue", new Type[] { typeof(string), typeof(Type) });

        // properties
        public static readonly MethodInfo ExposeProxyProperty =
            typeof(IAdvised).GetProperty("ExposeProxy", typeof(Boolean)).GetGetMethod();

        public static readonly MethodInfo CountProperty =
            typeof(System.Collections.ICollection).GetProperty("Count", typeof(Int32)).GetGetMethod();
    }
}
