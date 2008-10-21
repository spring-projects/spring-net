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
    #region IDynamicConstructor interface

    /// <summary>
    /// Defines constructors that dynamic constructor class has to implement.
    /// </summary>
    public interface IDynamicConstructor
    {
        /// <summary>
        /// Invokes dynamic constructor.
        /// </summary>
        /// <param name="arguments">
        /// Constructor arguments.
        /// </param>
        /// <returns>
        /// A constructor value.
        /// </returns>
        object Invoke(object[] arguments);
    }

    #endregion

    #region Safe wrapper

    /// <summary>
    /// Safe wrapper for the dynamic constructor.
    /// </summary>
    /// <remarks>
    /// <see cref="SafeConstructor"/> will attempt to use dynamic
    /// constructor if possible, but it will fall back to standard
    /// reflection if necessary.
    /// </remarks>
    public class SafeConstructor : IDynamicConstructor
    {
        private ConstructorInfo constructorInfo;

#if NET_2_0
        #region Generated Function Cache

        private static readonly IDictionary constructorCache = new Hashtable();

        /// <summary>
        /// Obtains cached constructor info or creates a new entry, if none is found.
        /// </summary>
        private static ConstructorDelegate GetOrCreateDynamicConstructor(ConstructorInfo constructorInfo)
        {
            ConstructorDelegate method = (ConstructorDelegate)constructorCache[constructorInfo];
            if (method == null)
            {
                method = DynamicReflectionManager.CreateConstructor(constructorInfo);
                lock (constructorCache)
                {
                    constructorCache[constructorInfo] = method;
                }
            }
            return method;
        }

        #endregion

        private ConstructorDelegate constructor;

        /// <summary>
        /// Creates a new instance of the safe constructor wrapper.
        /// </summary>
        /// <param name="constructorInfo">Constructor to wrap.</param>
        public SafeConstructor(ConstructorInfo constructorInfo)
        {
            this.constructorInfo = constructorInfo;
            this.constructor = GetOrCreateDynamicConstructor(constructorInfo);
        }


        /// <summary>
        /// Invokes dynamic constructor.
        /// </summary>
        /// <param name="arguments">
        /// Constructor arguments.
        /// </param>
        /// <returns>
        /// A constructor value.
        /// </returns>
        public object Invoke(object[] arguments)
        {
            return constructor(arguments);
        }
#else
        private IDynamicConstructor dynamicConstructor;
        private bool isOptimized = false;
        
        /// <summary>
        /// Creates a new instance of the safe constructor wrapper.
        /// </summary>
        /// <param name="constructor">Constructor to wrap.</param>
        public SafeConstructor(ConstructorInfo constructor)
        {
            this.constructorInfo = constructor;
            if (constructor.IsPublic &&
                ReflectionUtils.IsTypeVisible(constructor.DeclaringType, DynamicReflectionManager.ASSEMBLY_NAME))
            {
                this.dynamicConstructor = DynamicConstructor.Create(constructor);
                this.isOptimized = true;
            }
        }

        /// <summary>
        /// Invokes dynamic constructor.
        /// </summary>
        /// <param name="arguments">
        /// Constructor arguments.
        /// </param>
        /// <returns>
        /// A constructor value.
        /// </returns>
        public object Invoke(object[] arguments)
        {
            if (isOptimized)
            {
                // try dynamic constructor first but fall back to standard reflection 
                // if arguments are causing InvalidCastExceptions
                try
                {
                    return dynamicConstructor.Invoke(arguments);
                }
                catch (InvalidCastException)
                {
                    isOptimized = false;
                    return constructorInfo.Invoke(arguments);
                }
            }
            else
            {
                return constructorInfo.Invoke(arguments);
            }
        }
#endif
    }

    #endregion

#if NET_2_0
    /// <summary>
    /// Factory class for dynamic constructors.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class DynamicConstructor : BaseDynamicMember
    {
        /// <summary>
        /// Creates dynamic constructor instance for the specified <see cref="ConstructorInfo"/>.
        /// </summary>
        /// <param name="constructorInfo">Constructor info to create dynamic constructor for.</param>
        /// <returns>Dynamic constructor for the specified <see cref="ConstructorInfo"/>.</returns>
        public static IDynamicConstructor Create(ConstructorInfo constructorInfo)
        {
            AssertUtils.ArgumentNotNull(constructorInfo, "You cannot create a dynamic constructor for a null value.");

            return new SafeConstructor(constructorInfo);
        }
    }
    
#else
    /// <summary>
    /// Factory class for dynamic constructors.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class DynamicConstructor : BaseDynamicMember
    {
        private static readonly CreateConstructorCallback s_createCallback = new CreateConstructorCallback(CreateInternal);

        #region Create Constructor

        /// <summary>
        /// Creates dynamic constructor instance for the specified <see cref="ConstructorInfo"/>.
        /// </summary>
        /// <param name="constructor">Constructor info to create dynamic constructor for.</param>
        /// <returns>Dynamic constructor for the specified <see cref="ConstructorInfo"/>.</returns>
        public static IDynamicConstructor Create(ConstructorInfo constructor)
        {
            AssertUtils.ArgumentNotNull(constructor, "You cannot create a dynamic constructor for a null value.");

            IDynamicConstructor dynamicConstructor = DynamicReflectionManager.GetDynamicConstructor(constructor, s_createCallback);
            return dynamicConstructor;
        }

        private static IDynamicConstructor CreateInternal(ConstructorInfo constructor)
        {
            IDynamicConstructor dynamicConstructor = null;

            TypeBuilder tb = DynamicReflectionManager.CreateTypeBuilder("Ctor_" + constructor.DeclaringType.Name);
            tb.AddInterfaceImplementation(typeof(IDynamicConstructor));

            GenerateInvoke(tb, constructor);

            Type dynamicConstructorType = tb.CreateType();
            ConstructorInfo ctor = dynamicConstructorType.GetConstructor(Type.EmptyTypes);
            dynamicConstructor = (IDynamicConstructor) ctor.Invoke(ObjectUtils.EmptyObjects);
            
            return dynamicConstructor;
        }

        private static void GenerateInvoke(TypeBuilder tb, ConstructorInfo constructor)
        {
            MethodBuilder invokeMethod = 
                tb.DefineMethod("Invoke", METHOD_ATTRIBUTES, typeof(object), new Type[] {typeof(object[])});
            invokeMethod.DefineParameter(1, ParameterAttributes.None, "args");

            ILGenerator il = invokeMethod.GetILGenerator();

            DynamicReflectionManager.EmitInvokeConstructor(il, constructor, true);
        }
        #endregion
    }
#endif
}
