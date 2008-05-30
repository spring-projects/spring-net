#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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
using Spring.Util;

#endregion

namespace Spring.Reflection.Dynamic
{
    #region IDynamicMethod interface

    /// <summary>
    /// Defines methods that dynamic method class has to implement.
    /// </summary>
    public interface IDynamicMethod
    {
        /// <summary>
        /// Invokes dynamic method on the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to invoke method on.
        /// </param>
        /// <param name="arguments">
        /// Method arguments.
        /// </param>
        /// <returns>
        /// A method return value.
        /// </returns>
        object Invoke(object target, object[] arguments);
    }

    #endregion

    #region Safe wrapper

    /// <summary>
    /// Safe wrapper for the dynamic method.
    /// </summary>
    /// <remarks>
    /// <see cref="SafeMethod"/> will attempt to use dynamic
    /// method if possible, but it will fall back to standard
    /// reflection if necessary.
    /// </remarks>
    public class SafeMethod : IDynamicMethod
    {
        private MethodInfo method;
        private IDynamicMethod dynamicMethod;
        private bool isOptimized = false;

        /// <summary>
        /// Creates a new instance of the safe method wrapper.
        /// </summary>
        /// <param name="method">Method to wrap.</param>
        public SafeMethod(MethodInfo method)
        {
            this.method = method;
            if (method.IsPublic && 
                ReflectionUtils.IsTypeVisible(method.DeclaringType, DynamicReflectionManager.ASSEMBLY_NAME))
            {
                this.dynamicMethod = DynamicMethod.Create(method);
                this.isOptimized = true;
            }
        }

        /// <summary>
        /// Gets the class, that declares this method
        /// </summary>
        public Type DeclaringType
        {
            get { return method.DeclaringType; }
        }

        /// <summary>
        /// Invokes dynamic method.
        /// </summary>
        /// <param name="target">
        /// Target object to invoke method on.
        /// </param>
        /// <param name="arguments">
        /// Method arguments.
        /// </param>
        /// <returns>
        /// A method return value.
        /// </returns>
        public object Invoke(object target, object[] arguments)
        {
            if (isOptimized)
            {
                // try dynamic method first but fall back to standard reflection 
                // if arguments are causing InvalidCastExceptions
                try
                {
                    return dynamicMethod.Invoke(target, arguments);
                }
                catch (InvalidCastException e)
                {
                    // Only attempt if DynamicReflection code itself threw the exception.
                    if (!ExceptionFromDynamicReflection(e))
                    {
                        throw;
                    }
                    isOptimized = false;
                    return method.Invoke(target, arguments);
                }
            }
            else
            {
                return method.Invoke(target, arguments);
            }
        }

        private bool ExceptionFromDynamicReflection(InvalidCastException e)
        {
            return e.TargetSite.DeclaringType.FullName.IndexOf(DynamicReflectionManager.ASSEMBLY_NAME) >= 0;
        }
    }

    #endregion
    
    /// <summary>
    /// Factory class for dynamic methods.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <version>$Id: DynamicMethod.cs,v 1.2 2008/03/06 20:20:37 oakinger Exp $</version>
    public class DynamicMethod : BaseDynamicMember
    {
        private static readonly CreateMethodCallback s_createMethodCallback = new CreateMethodCallback(CreateInternal);

        #region Create Method

        /// <summary>
        /// Creates dynamic method instance for the specified <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="method">Method info to create dynamic method for.</param>
        /// <returns>Dynamic method for the specified <see cref="MethodInfo"/>.</returns>
        public static IDynamicMethod Create(MethodInfo method)
        {
            AssertUtils.ArgumentNotNull(method, "You cannot create a dynamic method for a null value.");

            IDynamicMethod dynamicMethod = DynamicReflectionManager.GetDynamicMethod(method, s_createMethodCallback);
            return dynamicMethod;
        }

        private static IDynamicMethod CreateInternal(MethodInfo method)
        {
            IDynamicMethod dynamicMethod = null;

            TypeBuilder tb = DynamicReflectionManager.CreateTypeBuilder("Method_" + method.Name);
            tb.AddInterfaceImplementation(typeof(IDynamicMethod));

            GenerateInvoke(tb, method);

            Type dynamicMethodType = tb.CreateType();
            ConstructorInfo ctor = dynamicMethodType.GetConstructor(Type.EmptyTypes);
            dynamicMethod = (IDynamicMethod) ctor.Invoke(ObjectUtils.EmptyObjects);

            return dynamicMethod;
        }

        private static void GenerateInvoke(TypeBuilder tb, MethodInfo method)
        {
            MethodBuilder invokeMethod = 
                tb.DefineMethod("Invoke", METHOD_ATTRIBUTES, typeof(object), new Type[] {typeof(object), typeof(object[])});
            invokeMethod.DefineParameter(1, ParameterAttributes.None, "target");
            invokeMethod.DefineParameter(2, ParameterAttributes.None, "args");

            ILGenerator il = invokeMethod.GetILGenerator();
            bool isValueType = method.DeclaringType.IsValueType;
            bool isStatic = method.IsStatic;

            IDictionary outArgs = new Hashtable();
            ParameterInfo[] args = method.GetParameters();
            for (int i = 0; i < args.Length; i++)
            {
                if (IsOutputOrRefArgument(args[i]))
                {
                    SetupOutputArgument(il, args[i], outArgs);
                }
            }

            if (!isStatic)
            {
                SetupTargetInstance(il, method.DeclaringType);
            }

            for (int i = 0; i < args.Length; i++)
            {
                SetupMethodArgument(il, args[i], outArgs);
            }
            InvokeMethod(il, isStatic, isValueType, method);
            for (int i = 0; i < args.Length; i++)
            {
                if (IsOutputOrRefArgument(args[i]))
                {
                    ProcessOutputArgument(il, args[i], outArgs);
                }
            }
            ProcessReturnValue(il, method.ReturnType);
            il.Emit(OpCodes.Ret);
        }

        private static bool IsOutputOrRefArgument(ParameterInfo argInfo)
        {
            return argInfo.IsOut || argInfo.ParameterType.Name.EndsWith("&");
        }

        private static void SetupOutputArgument(ILGenerator il, ParameterInfo argInfo, IDictionary outArgs)
        {
            Type argType = argInfo.ParameterType.GetElementType();

            LocalBuilder lb = il.DeclareLocal(argType);
            if (!argInfo.IsOut)
            {
                PushArgumentValue(il, argType, argInfo.Position);
                il.Emit(OpCodes.Stloc, lb);
            }
            outArgs[argInfo.Position] = lb;
        }

        private static void ProcessOutputArgument(ILGenerator il, ParameterInfo argInfo, IDictionary outArgs)
        {
            Type argType = argInfo.ParameterType.GetElementType();

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldc_I4, argInfo.Position);
            il.Emit(OpCodes.Ldloc, (LocalBuilder)outArgs[argInfo.Position]);
            if (argType.IsValueType)
            {
                il.Emit(OpCodes.Box, argType);
            }
            il.Emit(OpCodes.Stelem_Ref);
        }

        private static void SetupMethodArgument(ILGenerator il, ParameterInfo argInfo, IDictionary outArgs)
        {
            if (IsOutputOrRefArgument(argInfo))
            {
                il.Emit(OpCodes.Ldloca_S, (LocalBuilder)outArgs[argInfo.Position]);
            }
            else
            {
                PushArgumentValue(il, argInfo.ParameterType, argInfo.Position);
            }
        }

        private static void PushArgumentValue(ILGenerator il, Type argumentType, int argumentPosition)
        {
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldc_I4, argumentPosition);
            il.Emit(OpCodes.Ldelem_Ref);
            if (argumentType.IsValueType)
            {
#if NET_2_0
                il.Emit(OpCodes.Unbox_Any, argumentType);
#else
				il.Emit(OpCodes.Unbox, argumentType);
				il.Emit(OpCodes.Ldobj, argumentType);
#endif
            }
            else
            {
                il.Emit(OpCodes.Castclass, argumentType);
            }
        }

        #endregion
    }
}
