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
using System.Runtime.CompilerServices;
using System.Security;
using Spring.Util;

using NetDynamicMethod = System.Reflection.Emit.DynamicMethod;

namespace Spring.Reflection.Dynamic
{
    /// <summary>
    /// Represents a Get method
    /// </summary>
    /// <param name="target">the target instance when calling an instance method</param>
    /// <returns>the value return by the Get method</returns>
    public delegate object FieldGetterDelegate(object target);

    /// <summary>
    /// Represents a Set method
    /// </summary>
    /// <param name="target">the target instance when calling an instance method</param>
    /// <param name="value">the value to be set</param>
    public delegate void FieldSetterDelegate(object target, object value);

    /// <summary>
    /// Represents an Indexer Get method
    /// </summary>
    /// <param name="target">the target instance when calling an instance method</param>
    /// <param name="index"></param>
    /// <returns>the value return by the Get method</returns>
    public delegate object PropertyGetterDelegate(object target, params object[] index);

    /// <summary>
    /// Represents a Set method
    /// </summary>
    /// <param name="target">the target instance when calling an instance method</param>
    /// <param name="value">the value to be set</param>
    /// <param name="index"></param>
    public delegate void PropertySetterDelegate(object target, object value, params object[] index);

    /// <summary>
    /// Represents a method
    /// </summary>
    /// <param name="target">the target instance when calling an instance method</param>
    /// <param name="args">arguments to be passed to the method</param>
    /// <returns>the value return by the method. <value>null</value> when calling a void method</returns>
    public delegate object FunctionDelegate(object target, params object[] args);

    /// <summary>
    /// Represents a constructor
    /// </summary>
    /// <param name="args">arguments to be passed to the method</param>
    /// <returns>the new object instance</returns>
    public delegate object ConstructorDelegate(params object[] args);

    /// <summary>
    /// Allows easy access to existing and creation of new dynamic relection members.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public sealed class DynamicReflectionManager
    {
        /// <summary>
        /// Create a new Get method delegate for the specified field using <see cref="System.Reflection.Emit.DynamicMethod"/>
        /// </summary>
        /// <param name="fieldInfo">the field to create the delegate for</param>
        /// <returns>a delegate that can be used to read the field</returns>
        public static FieldGetterDelegate CreateFieldGetter(FieldInfo fieldInfo)
        {
            AssertUtils.ArgumentNotNull(fieldInfo, "You cannot create a delegate for a null value.");

            bool skipVisibility = true; //!IsPublic(fieldInfo);
            Type[] argumentTypes = new Type[] { typeof(object) };
            System.Reflection.Emit.DynamicMethod dmGetter = CreateDynamicMethod("get_" + fieldInfo.Name, typeof(object), argumentTypes, fieldInfo, skipVisibility);
            ILGenerator il = dmGetter.GetILGenerator();
            EmitFieldGetter(il, fieldInfo, false);
            return (FieldGetterDelegate)dmGetter.CreateDelegate(typeof(FieldGetterDelegate));
        }

        /// <summary>
        /// Create a new Set method delegate for the specified field using <see cref="System.Reflection.Emit.DynamicMethod"/>
        /// </summary>
        /// <param name="fieldInfo">the field to create the delegate for</param>
        /// <returns>a delegate that can be used to read the field.</returns>
        /// <remarks>
        /// If the field's <see cref="FieldInfo.IsLiteral"/> returns true, the returned method
        /// will throw an <see cref="InvalidOperationException"/> when called.
        /// </remarks>
        public static FieldSetterDelegate CreateFieldSetter(FieldInfo fieldInfo)
        {
            AssertUtils.ArgumentNotNull(fieldInfo, "You cannot create a delegate for a null value.");

            bool skipVisibility = true; // !IsPublic(fieldInfo);
            System.Reflection.Emit.DynamicMethod dmSetter = CreateDynamicMethod("set_" + fieldInfo.Name, null, new Type[] { typeof(object), typeof(object) }, fieldInfo, skipVisibility);
            ILGenerator il = dmSetter.GetILGenerator();
            EmitFieldSetter(il, fieldInfo, false);
            return (FieldSetterDelegate)dmSetter.CreateDelegate(typeof(FieldSetterDelegate));
        }

        /// <summary>
        /// Create a new Get method delegate for the specified property using <see cref="System.Reflection.Emit.DynamicMethod"/>
        /// </summary>
        /// <param name="propertyInfo">the property to create the delegate for</param>
        /// <returns>a delegate that can be used to read the property.</returns>
        /// <remarks>
        /// If the property's <see cref="PropertyInfo.CanRead"/> returns false, the returned method
        /// will throw an <see cref="InvalidOperationException"/> when called.
        /// </remarks>
        public static PropertyGetterDelegate CreatePropertyGetter(PropertyInfo propertyInfo)
        {
            AssertUtils.ArgumentNotNull(propertyInfo, "You cannot create a delegate for a null value.");

            MethodInfo getMethod = propertyInfo.GetGetMethod();
            bool skipVisibility = true; // (null == getMethod || !IsPublic(getMethod)); // getter is public
            NetDynamicMethod dm = CreateDynamicMethod("get_" + propertyInfo.Name, typeof(object), new Type[] { typeof(object), typeof(object[]) }, propertyInfo, skipVisibility);
            ILGenerator il = dm.GetILGenerator();
            EmitPropertyGetter(il, propertyInfo, false);
            return (PropertyGetterDelegate)dm.CreateDelegate(typeof(PropertyGetterDelegate));
        }

        /// <summary>
        /// Create a new Set method delegate for the specified property using <see cref="System.Reflection.Emit.DynamicMethod"/>
        /// </summary>
        /// <param name="propertyInfo">the property to create the delegate for</param>
        /// <returns>a delegate that can be used to write the property.</returns>
        /// <remarks>
        /// If the property's <see cref="PropertyInfo.CanWrite"/> returns false, the returned method
        /// will throw an <see cref="InvalidOperationException"/> when called.
        /// </remarks>
        public static PropertySetterDelegate CreatePropertySetter(PropertyInfo propertyInfo)
        {
            AssertUtils.ArgumentNotNull(propertyInfo, "You cannot create a delegate for a null value.");

            MethodInfo setMethod = propertyInfo.GetSetMethod();
            bool skipVisibility = true; // (null == setMethod || !IsPublic(setMethod)); // setter is public
            Type[] argumentTypes = new Type[] { typeof(object), typeof(object), typeof(object[]) };
            NetDynamicMethod dm = CreateDynamicMethod("set_" + propertyInfo.Name, null, argumentTypes, propertyInfo, skipVisibility);
            ILGenerator il = dm.GetILGenerator();
            EmitPropertySetter(il, propertyInfo, false);
            return (PropertySetterDelegate)dm.CreateDelegate(typeof(PropertySetterDelegate));
        }

        /// <summary>
        /// Create a new method delegate for the specified method using <see cref="System.Reflection.Emit.DynamicMethod"/>
        /// </summary>
        /// <param name="methodInfo">the method to create the delegate for</param>
        /// <returns>a delegate that can be used to invoke the method.</returns>
        public static FunctionDelegate CreateMethod(MethodInfo methodInfo)
        {
            AssertUtils.ArgumentNotNull(methodInfo, "You cannot create a delegate for a null value.");

            bool skipVisibility = true; // !IsPublic(methodInfo);
            NetDynamicMethod dm = CreateDynamicMethod(methodInfo.Name, typeof(object), new Type[] { typeof(object), typeof(object[]) }, methodInfo, skipVisibility);
            ILGenerator il = dm.GetILGenerator();
            EmitInvokeMethod(il, methodInfo, false);
            return (FunctionDelegate)dm.CreateDelegate(typeof(FunctionDelegate));
        }

        ///<summary>
        /// Creates a new delegate for the specified constructor.
        ///</summary>
        ///<param name="constructorInfo">the constructor to create the delegate for</param>
        ///<returns>delegate that can be used to invoke the constructor.</returns>
        public static ConstructorDelegate CreateConstructor(ConstructorInfo constructorInfo)
        {
            AssertUtils.ArgumentNotNull(constructorInfo, "You cannot create a dynamic constructor for a null value.");

            bool skipVisibility = true; //!IsPublic(constructorInfo);
            System.Reflection.Emit.DynamicMethod dmGetter;
            Type[] argumentTypes = new Type[] { typeof(object[]) };
            dmGetter = CreateDynamicMethod(constructorInfo.Name, typeof(object), argumentTypes, constructorInfo, skipVisibility);
            ILGenerator il = dmGetter.GetILGenerator();
            EmitInvokeConstructor(il, constructorInfo, false);
            ConstructorDelegate ctor = (ConstructorDelegate)dmGetter.CreateDelegate(typeof(ConstructorDelegate));
            return ctor;
        }

        /// <summary>
        /// Creates a <see cref="System.Reflection.Emit.DynamicMethod"/> instance with the highest possible code access security.
        /// </summary>
        /// <remarks>
        /// If allowed by security policy, associates the method with the <paramref name="member"/>s declaring type.
        /// Otherwise associates the dynamic method with <see cref="DynamicReflectionManager"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static NetDynamicMethod CreateDynamicMethod(string methodName, Type returnType, Type[] argumentTypes, MemberInfo member, bool skipVisibility)
        {
            NetDynamicMethod dmGetter = null;
            methodName = "_dynamic_" + member.DeclaringType.FullName + "." + methodName;
            try
            {
#if !NETSTANDARD
                new PermissionSet(System.Security.Permissions.PermissionState.Unrestricted).Demand();
#endif
                dmGetter = CreateDynamicMethodInternal(methodName, returnType, argumentTypes, member, skipVisibility);
            }
            catch(SecurityException)
            {
                dmGetter = CreateDynamicMethodInternal(methodName, returnType, argumentTypes, MethodBase.GetCurrentMethod(), false);
            }
            return dmGetter;
        }

        private static NetDynamicMethod CreateDynamicMethodInternal(string methodName, Type returnType, Type[] argumentTypes, MemberInfo member, bool skipVisibility)
        {
            NetDynamicMethod dm;
            dm = new NetDynamicMethod(methodName, returnType, argumentTypes, member.Module, skipVisibility);
            return dm;
        }

        /* TODO (EE): I am not sure, if "skipVisibility" in "CreateDynamicMethodInternal" may be true all the time or if visibility needs to be calculated like below
         *
                private static bool IsPublic(MemberInfo member)
                {
                    if (member == null) return true;

                    switch(member.MemberType)
                    {
                        case MemberTypes.Event:
                            {
                                bool isPublic = ((EventInfo) member).GetAddMethod() != null;
                                return isPublic && IsPublic(member.DeclaringType);
                            }
                        case MemberTypes.Field:
                            {
                                bool isPublic = ((FieldInfo)member).IsPublic;
                                return isPublic && IsPublic(member.DeclaringType);
                            }
                        case MemberTypes.Property:
                            {
                                throw new NotSupportedException();
                            }
                        case MemberTypes.Constructor:
                        case MemberTypes.Method:
                            {
                                MethodBase methodBase = ((MethodBase)member);
                                bool isPublic = methodBase.IsPublic;
                                if (!isPublic)
                                {
                                    return false;
                                }
                                if (!IsPublic(methodBase.DeclaringType))
                                {
                                    return false;
                                }
                                if (member.MemberType == MemberTypes.Method
                                    && !IsPublic(((MethodInfo)methodBase).ReturnType))
                                {
                                    return false;
                                }
                                foreach(ParameterInfo arg in methodBase.GetParameters())
                                {
                                    if (!IsPublic(arg.ParameterType)) return false;
                                }

                                return true;
                            }
                        case MemberTypes.NestedType:
                        case MemberTypes.TypeInfo:
                            {
                                Type type = (Type)member;
                                bool isPublic = type.IsPublic;
                                if (type.IsNested && !IsPublic(type.DeclaringType))
                                {
                                    return false;
                                }
                                if (type.IsGenericType)
                                {
                                    foreach(Type genericTypeArg in type.GetGenericArguments())
                                    {
                                        if (!IsPublic(genericTypeArg))
                                        {
                                            return false;
                                        }
                                    }
                                }
                                return true;
                            }
                        default:
                            throw new NotSupportedException();
                    }
                }
        */

        #region Shared Code Generation

        private static void EmitFieldGetter(ILGenerator il, FieldInfo fieldInfo, bool isInstanceMethod)
        {
            if (fieldInfo.IsLiteral)
            {
                object value = fieldInfo.GetValue(null);
                EmitConstant(il, value);
            }
            else if (fieldInfo.IsStatic)
            {
//                object v = fieldInfo.GetValue(null); // ensure type is initialized...
                il.Emit(OpCodes.Ldsfld, fieldInfo);
            }
            else
            {
                EmitTarget(il, fieldInfo.DeclaringType, isInstanceMethod);
                il.Emit(OpCodes.Ldfld, fieldInfo);
            }

            if (fieldInfo.FieldType.IsValueType)
            {
                il.Emit(OpCodes.Box, fieldInfo.FieldType);
            }
            il.Emit(OpCodes.Ret);
        }

        internal static void EmitFieldSetter(ILGenerator il, FieldInfo fieldInfo, bool isInstanceMethod)
        {
            if (!fieldInfo.IsLiteral
                && !fieldInfo.IsInitOnly
                && !(fieldInfo.DeclaringType.IsValueType && !fieldInfo.IsStatic))
            {
                if (!fieldInfo.IsStatic)
                {
                    EmitTarget(il, fieldInfo.DeclaringType, isInstanceMethod);
                }

                il.Emit(OpCodes.Ldarg_1);
                if (fieldInfo.FieldType.IsValueType)
                {
                    EmitUnbox(il, fieldInfo.FieldType);
                }
                else
                {
                    il.Emit(OpCodes.Castclass, fieldInfo.FieldType);
                }

                if (fieldInfo.IsStatic)
                {
                    il.Emit(OpCodes.Stsfld, fieldInfo);
                }
                else
                {
                    il.Emit(OpCodes.Stfld, fieldInfo);
                }
                il.Emit(OpCodes.Ret);
            }
            else
            {
                EmitThrowInvalidOperationException(il, string.Format("Cannot write to read-only field '{0}.{1}'", fieldInfo.DeclaringType.FullName, fieldInfo.Name));
            }
        }

        internal static void EmitPropertyGetter(ILGenerator il, PropertyInfo propertyInfo, bool isInstanceMethod)
        {
            if (propertyInfo.CanRead)
            {
                MethodInfo getMethod = propertyInfo.GetGetMethod(true);
                EmitInvokeMethod(il, getMethod, isInstanceMethod);
            }
            else
            {
                EmitThrowInvalidOperationException(il, string.Format("Cannot read from write-only property '{0}.{1}'", propertyInfo.DeclaringType.FullName, propertyInfo.Name));
            }
        }

        internal static void EmitPropertySetter(ILGenerator il, PropertyInfo propertyInfo, bool isInstanceMethod)
        {
            MethodInfo method = propertyInfo.GetSetMethod(true);

            if (propertyInfo.CanWrite
                && !(propertyInfo.DeclaringType.IsValueType && !method.IsStatic))
            {
                // Note: last arg is property value!
                // property set method signature:
                // void set_MyOtherProperty( [indexArg1, indexArg2, ...], string value)

                const int paramsArrayPosition = 2;
                IDictionary outArgs = new Hashtable();
                ParameterInfo[] args = propertyInfo.GetIndexParameters(); // get indexParameters here!
                for (int i = 0; i < args.Length; i++)
                {
                    SetupOutputArgument(il, paramsArrayPosition, args[i], outArgs);
                }

                // load target
                if (!method.IsStatic)
                {
                    EmitTarget(il, method.DeclaringType, isInstanceMethod);
                }

                // load indexer arguments
                for (int i = 0; i < args.Length; i++)
                {
                    SetupMethodArgument(il, paramsArrayPosition, args[i], outArgs);
                }

                // load value
                il.Emit(OpCodes.Ldarg_1);
                if (propertyInfo.PropertyType.IsValueType)
                {
                    EmitUnbox(il, propertyInfo.PropertyType);
                }
                else
                {
                    il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                }

                // call setter
                EmitCall(il, method);

                for (int i = 0; i < args.Length; i++)
                {
                    ProcessOutputArgument(il, paramsArrayPosition, args[i], outArgs);
                }
                il.Emit(OpCodes.Ret);
            }
            else
            {
                EmitThrowInvalidOperationException(il, string.Format("Cannot write to read-only property '{0}.{1}'", propertyInfo.DeclaringType.FullName, propertyInfo.Name));
            }
        }

        /// <summary>
        /// Delegates a Method(object target, params object[] args) call to the actual underlying method.
        /// </summary>
        internal static void EmitInvokeMethod(ILGenerator il, MethodInfo method, bool isInstanceMethod)
        {
            int paramsArrayPosition = (isInstanceMethod) ? 2 : 1;
            ParameterInfo[] args = method.GetParameters();
            IDictionary outArgs = new Hashtable();
            for (int i = 0; i < args.Length; i++)
            {
                SetupOutputArgument(il, paramsArrayPosition, args[i], outArgs);
            }

            if (!method.IsStatic)
            {
                EmitTarget(il, method.DeclaringType, isInstanceMethod);
            }

            for (int i = 0; i < args.Length; i++)
            {
                SetupMethodArgument(il, paramsArrayPosition, args[i], outArgs);
            }

            EmitCall(il, method);

            for (int i = 0; i < args.Length; i++)
            {
                ProcessOutputArgument(il, paramsArrayPosition, args[i], outArgs);
            }

            EmitMethodReturn(il, method.ReturnType);
        }

        internal static void EmitInvokeConstructor(ILGenerator il, ConstructorInfo constructor, bool isInstanceMethod)
        {
            int paramsArrayPosition = (isInstanceMethod) ? 1 : 0;
            ParameterInfo[] args = constructor.GetParameters();

            IDictionary outArgs = new Hashtable();
            for (int i = 0; i < args.Length; i++)
            {
                SetupOutputArgument(il, paramsArrayPosition, args[i], outArgs);
            }

            for (int i = 0; i < args.Length; i++)
            {
                SetupMethodArgument(il, paramsArrayPosition, args[i], null);
            }

            il.Emit(OpCodes.Newobj, constructor);

            for (int i = 0; i < args.Length; i++)
            {
                ProcessOutputArgument(il, paramsArrayPosition, args[i], outArgs);
            }

            EmitMethodReturn(il, constructor.DeclaringType);
        }

        #endregion

        private static OpCode[] LdArgOpCodes = { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2 };

        private static void SetupOutputArgument(ILGenerator il, int paramsArrayPosition, ParameterInfo argInfo, IDictionary outArgs)
        {
            if (!IsOutputOrRefArgument(argInfo))
                return;

            Type argType = argInfo.ParameterType.GetElementType();

            LocalBuilder lb = il.DeclareLocal(argType);
            if (!argInfo.IsOut)
            {
                PushParamsArgumentValue(il, paramsArrayPosition, argType, argInfo.Position);
                il.Emit(OpCodes.Stloc, lb);
            }
            outArgs[argInfo.Position] = lb;
        }

        private static bool IsOutputOrRefArgument(ParameterInfo argInfo)
        {
            return argInfo.IsOut || argInfo.ParameterType.Name.EndsWith("&");
        }

        private static void ProcessOutputArgument(ILGenerator il, int paramsArrayPosition, ParameterInfo argInfo, IDictionary outArgs)
        {
            if (!IsOutputOrRefArgument(argInfo))
                return;

            Type argType = argInfo.ParameterType.GetElementType();

            il.Emit(LdArgOpCodes[paramsArrayPosition]);
            il.Emit(OpCodes.Ldc_I4, argInfo.Position);
            il.Emit(OpCodes.Ldloc, (LocalBuilder)outArgs[argInfo.Position]);
            if (argType.IsValueType)
            {
                il.Emit(OpCodes.Box, argType);
            }
            il.Emit(OpCodes.Stelem_Ref);
        }

        private static void SetupMethodArgument(ILGenerator il, int paramsArrayPosition, ParameterInfo argInfo, IDictionary outArgs)
        {
            if (IsOutputOrRefArgument(argInfo))
            {
                il.Emit(OpCodes.Ldloca_S, (LocalBuilder)outArgs[argInfo.Position]);
            }
            else
            {
                PushParamsArgumentValue(il, paramsArrayPosition, argInfo.ParameterType, argInfo.Position);
            }
        }

        private static void PushParamsArgumentValue(ILGenerator il, int paramsArrayPosition, Type argumentType, int argumentPosition)
        {
            il.Emit(LdArgOpCodes[paramsArrayPosition]);
            il.Emit(OpCodes.Ldc_I4, argumentPosition);
            il.Emit(OpCodes.Ldelem_Ref);
            if (argumentType.IsValueType)
            {
                // call ConvertArgumentIfNecessary() to convert e.g. int32 to double if necessary
                il.Emit(OpCodes.Ldtoken, argumentType);
                EmitCall(il, FnGetTypeFromHandle);
                il.Emit(OpCodes.Ldc_I4, argumentPosition);
                EmitCall(il, FnConvertArgumentIfNecessary);
                EmitUnbox(il, argumentType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, argumentType);
            }
        }

        private static void EmitUnbox(ILGenerator il, Type argumentType)
        {
            il.Emit(OpCodes.Unbox_Any, argumentType);
        }

        /// <summary>
        /// Generates code to process return value if necessary.
        /// </summary>
        /// <param name="il">IL generator to use.</param>
        /// <param name="returnValueType">Type of the return value.</param>
        private static void EmitMethodReturn(ILGenerator il, Type returnValueType)
        {
            if (returnValueType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if (returnValueType.IsValueType)
            {
                il.Emit(OpCodes.Box, returnValueType);
            }
            il.Emit(OpCodes.Ret);
        }

        private delegate Type GetTypeFromHandleDelegate(RuntimeTypeHandle handle);
        private static readonly MethodInfo FnGetTypeFromHandle = new GetTypeFromHandleDelegate(Type.GetTypeFromHandle).Method;
        private delegate object ChangeTypeDelegate(object value, Type targetType, int argIndex);
        private static readonly MethodInfo FnConvertArgumentIfNecessary = new ChangeTypeDelegate(ConvertValueTypeArgumentIfNecessary).Method;

        /// <summary>
        /// Converts <paramref name="value"/> to an instance of <paramref name="targetType"/> if necessary to
        /// e.g. avoid e.g. double/int cast exceptions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method mimics the behavior of the compiler that
        /// automatically performs casts like int to double in "Math.Sqrt(4)".<br/>
        /// See about implicit, widening type conversions on <a href="http://social.msdn.microsoft.com/Search/en-US/?query=type conversion tables">MSDN - Type Conversion Tables</a>
        /// </para>
        /// <para>
        /// Note: <paramref name="targetType"/> is expected to be a value type!
        /// </para>
        /// </remarks>
        public static object ConvertValueTypeArgumentIfNecessary(object value, Type targetType, int argIndex)
        {
            if (value == null)
            {
                if (ReflectionUtils.IsNullableType(targetType))
                {
                    return null;
                }
                throw new InvalidCastException(string.Format("Cannot convert NULL at position {0} to argument type {1}", argIndex, targetType.FullName));
            }

            Type valueType = value.GetType();

            if (ReflectionUtils.IsNullableType(targetType))
            {
                targetType = Nullable.GetUnderlyingType(targetType);
            }

            // no conversion necessary?
            if (valueType == targetType)
            {
                return value;
            }

            if (!valueType.IsValueType)
            {
                // we're facing a reftype/valuetype mix that never can convert
                throw new InvalidCastException(string.Format("Cannot convert value '{0}' of type {1} at position {2} to argument type {3}", value, valueType.FullName, argIndex, targetType.FullName));
            }

            // we're dealing only with ValueType's now - try to convert them
            try
            {
                // TODO: allow widening conversions only
                return Convert.ChangeType(value, targetType);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(string.Format("Cannot convert value '{0}' of type {1} at position {2} to argument type {3}", value, valueType.FullName, argIndex, targetType.FullName), ex);
            }
        }

        private static void EmitTarget(ILGenerator il, Type targetType, bool isInstanceMethod)
        {
            il.Emit((isInstanceMethod) ? OpCodes.Ldarg_1 : OpCodes.Ldarg_0);
            if (targetType.IsValueType)
            {
                LocalBuilder local = il.DeclareLocal(targetType);
                EmitUnbox(il, targetType);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, 0);
            }
            else
            {
                il.Emit(OpCodes.Castclass, targetType);
            }
        }

        private static void EmitCall(ILGenerator il, MethodInfo method)
        {
            il.EmitCall((method.IsVirtual) ? OpCodes.Callvirt : OpCodes.Call, method, null);
        }

        private static void EmitConstant(ILGenerator il, object value)
        {
            if (value is String)
            {
                il.Emit(OpCodes.Ldstr, (string)value);
                return;
            }

            if (value is bool)
            {
                if ((bool)value)
                {
                    il.Emit(OpCodes.Ldc_I4_1);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                }
                return;
            }

            if (value is Char)
            {
                il.Emit(OpCodes.Ldc_I4, (Char)value);
                il.Emit(OpCodes.Conv_I2);
                return;
            }

            if (value is byte)
            {
                il.Emit(OpCodes.Ldc_I4_S, (byte)value);
                il.Emit(OpCodes.Conv_I1);
            }
            else if (value is Int16)
            {
                il.Emit(OpCodes.Ldc_I4, (Int16)value);
                il.Emit(OpCodes.Conv_I2);
            }
            else if (value is Int32)
            {
                il.Emit(OpCodes.Ldc_I4, (Int32)value);
            }
            else if (value is Int64)
            {
                il.Emit(OpCodes.Ldc_I8, (Int64)value);
            }
            else if (value is UInt16)
            {
                il.Emit(OpCodes.Ldc_I4, (UInt16)value);
                il.Emit(OpCodes.Conv_U2);
            }
            else if (value is UInt32)
            {
                il.Emit(OpCodes.Ldc_I4, (UInt32)value);
                il.Emit(OpCodes.Conv_U4);
            }
            else if (value is UInt64)
            {
                il.Emit(OpCodes.Ldc_I8, (UInt64)value);
                il.Emit(OpCodes.Conv_U8);
            }
            else if (value is Single)
            {
                il.Emit(OpCodes.Ldc_R4, (Single)value);
            }
            else if (value is Double)
            {
                il.Emit(OpCodes.Ldc_R8, (Double)value);
            }
        }

        private static readonly ConstructorInfo NewInvalidOperationException =
            typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) });

        /// <summary>
        /// Generates code that throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="il">IL generator to use.</param>
        /// <param name="message">Error message to use.</param>
        private static void EmitThrowInvalidOperationException(ILGenerator il, string message)
        {
            il.Emit(OpCodes.Ldstr, message);
            il.Emit(OpCodes.Newobj, NewInvalidOperationException);
            il.Emit(OpCodes.Throw);
        }
    }
}
