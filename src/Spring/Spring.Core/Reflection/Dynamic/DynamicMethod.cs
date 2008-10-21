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
        object Invoke(object target, params object[] arguments);
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
        private readonly MethodInfo methodInfo;

        /// <summary>
        /// Gets the class, that declares this method
        /// </summary>
        public Type DeclaringType
        {
            get { return methodInfo.DeclaringType; }
        }

#if NET_2_0
        #region Generated Function Cache

        private static readonly IDictionary methodCache = new Hashtable();

        /// <summary>
        /// Obtains cached property info or creates a new entry, if none is found.
        /// </summary>
        private static FunctionDelegate GetOrCreateDynamicMethod(MethodInfo methodInfo)
        {
            FunctionDelegate method = (FunctionDelegate)methodCache[methodInfo];
            if (method == null)
            {
                method = DynamicReflectionManager.CreateMethod(methodInfo);
                lock (methodCache)
                {
                    methodCache[methodInfo] = method;
                }
            }
            return method;
        }

        #endregion

        private readonly FunctionDelegate method;

        /// <summary>
        /// Creates a new instance of the safe method wrapper.
        /// </summary>
        /// <param name="methodInfo">Method to wrap.</param>
        public SafeMethod(MethodInfo methodInfo)
        {
            AssertUtils.ArgumentNotNull(methodInfo, "You cannot create a dynamic method for a null value.");

            this.methodInfo = methodInfo;
            this.method = GetOrCreateDynamicMethod(methodInfo);
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
            return this.method(target, arguments);
        }
#else
        private IDynamicMethod dynamicMethod;
        private bool isOptimized = false;

        /// <summary>
        /// Creates a new instance of the safe method wrapper.
        /// </summary>
        /// <param name="method">Method to wrap.</param>
        public SafeMethod(MethodInfo method)
        {
            this.methodInfo = method;
            if (method.IsPublic && 
                ReflectionUtils.IsTypeVisible(method.DeclaringType, DynamicReflectionManager.ASSEMBLY_NAME))
            {
                this.dynamicMethod = DynamicMethod.Create(method);
                this.isOptimized = true;
            }
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
                    return methodInfo.Invoke(target, arguments);
                }
            }
            else
            {
                return methodInfo.Invoke(target, arguments);
            }
        }

        private bool ExceptionFromDynamicReflection(InvalidCastException e)
        {
            return e.TargetSite.DeclaringType.FullName.IndexOf(DynamicReflectionManager.ASSEMBLY_NAME) >= 0;
        }
#endif
    }

    #endregion
    
#if NET_2_0
    /// <summary>
    /// Factory class for dynamic methods.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class DynamicMethod : BaseDynamicMember
    {
        /// <summary>
        /// Creates dynamic method instance for the specified <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="method">Method info to create dynamic method for.</param>
        /// <returns>Dynamic method for the specified <see cref="MethodInfo"/>.</returns>
        public static IDynamicMethod Create(MethodInfo method)
        {
            AssertUtils.ArgumentNotNull(method, "You cannot create a dynamic method for a null value.");

            IDynamicMethod dynamicMethod = new SafeMethod(method);
            return dynamicMethod;
        }
    }
#else
    /// <summary>
    /// Factory class for dynamic methods.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
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
            DynamicReflectionManager.EmitInvokeMethod(il, method, true);
        }

        #endregion
    }

#endif
}
