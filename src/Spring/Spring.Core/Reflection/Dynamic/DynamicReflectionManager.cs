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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Spring.Util;

#if NET_2_0
using NetDynamicMethod = System.Reflection.Emit.DynamicMethod;
#endif

#endregion

namespace Spring.Reflection.Dynamic
{
    /// <summary>
    /// Represents a Get method
    /// </summary>
    /// <param name="target">the target instance when calling an instance method</param>
    /// <returns>the value return by the Get method</returns>
    public delegate object GetterDelegate(object target);

    /// <summary>
    /// Represents a Set method
    /// </summary>
    /// <param name="target">the target instance when calling an instance method</param>
    /// <param name="value">the value to be set</param>
    public delegate void SetterDelegate(object target, object value);

    /// <summary>
    /// Represents a method
    /// </summary>
    /// <param name="target">the target instance when calling an instance method</param>
    /// <param name="args">arguments to be passed to the method</param>
    /// <returns>the value return by the method. <value>null</value> when calling a void method</returns>
    public delegate object FunctionDelegate(object target, params object[] args);

    /// <summary>
    /// Represents a callback method used to create an <see cref="IDynamicProperty"/> from a <see cref="PropertyInfo"/> instance.
    /// </summary>
    internal delegate IDynamicProperty CreatePropertyCallback(PropertyInfo property);
    /// <summary>
    /// Represents a callback method used to create an <see cref="IDynamicField"/> from a <see cref="FieldInfo"/> instance.
    /// </summary>
    internal delegate IDynamicField CreateFieldCallback(FieldInfo property);
    /// <summary>
    /// Represents a callback method used to create an <see cref="IDynamicMethod"/> from a <see cref="MethodInfo"/> instance.
    /// </summary>
    internal delegate IDynamicMethod CreateMethodCallback(MethodInfo method);
    /// <summary>
    /// Represents a callback method used to create an <see cref="IDynamicConstructor"/> from a <see cref="ConstructorInfo"/> instance.
    /// </summary>
    internal delegate IDynamicConstructor CreateConstructorCallback(ConstructorInfo constructor);
    /// <summary>
    /// Represents a callback method used to create an <see cref="IDynamicIndexer"/> from a <see cref="PropertyInfo"/> instance.
    /// </summary>
    internal delegate IDynamicIndexer CreateIndexerCallback(PropertyInfo indexer);

    /// <summary>
    /// Allows easy access to existing and creation of new dynamic relection members.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <version>$Id: DynamicReflectionManager.cs,v 1.5 2008/05/17 11:05:26 oakinger Exp $</version>
    public sealed class DynamicReflectionManager
    {
        #region Fields

        /// <summary>
        /// The name of the assembly that defines reflection types created.
        /// </summary>
        public const string ASSEMBLY_NAME = "Spring.DynamicReflection";

        /// <summary>
        /// The attributes of the reflection type to generate.
        /// </summary>
        private const TypeAttributes TYPE_ATTRIBUTES = TypeAttributes.BeforeFieldInit | TypeAttributes.Public;

        /// <summary>
        /// Cache for dynamic property types.
        /// </summary>
        private readonly static IDictionary propertyCache = new Hashtable();

        /// <summary>
        /// Cache for dynamic field types.
        /// </summary>
        private readonly static IDictionary fieldCache = new Hashtable();

        /// <summary>
        /// Cache for dynamic indexer types.
        /// </summary>
        private readonly static IDictionary indexerCache = new Hashtable();

        /// <summary>
        /// Cache for dynamic method types.
        /// </summary>
        private readonly static IDictionary methodCache = new Hashtable();

        /// <summary>
        /// Cache for dynamic constructor types.
        /// </summary>
        private readonly static IDictionary constructorCache = new Hashtable();

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an appropriate type builder.
        /// </summary>
        /// <param name="name">
        /// The base name to use for the reflection type name.
        /// </param>
        /// <returns>The type builder to use.</returns>
        internal static TypeBuilder CreateTypeBuilder(string name)
        {
            // Generates type name
            string typeName = String.Format("{0}.{1}_{2}",
                ASSEMBLY_NAME, name, Guid.NewGuid().ToString("N"));

            ModuleBuilder module = DynamicCodeManager.GetModuleBuilder(ASSEMBLY_NAME);
            return module.DefineType(typeName, TYPE_ATTRIBUTES);
        }

        /// <summary>
        /// Returns dynamic property if one exists.
        /// </summary>
        /// <param name="property">Property to look up.</param>
        /// <param name="createCallback">callback function that will be called to create the dynamic property</param>
        /// <returns>An <see cref="IDynamicProperty"/> for the given property info.</returns>
        internal static IDynamicProperty GetDynamicProperty(PropertyInfo property, CreatePropertyCallback createCallback)
        {
            lock (propertyCache.SyncRoot)
            {
                IDynamicProperty dynamicProperty = (IDynamicProperty)propertyCache[property];
                if (dynamicProperty == null)
                {
                    dynamicProperty = createCallback(property);
                    propertyCache[property] = dynamicProperty;
                }
                return dynamicProperty;
            }
        }

        /// <summary>
        /// Returns dynamic field if one exists.
        /// </summary>
        /// <param name="field">Field to look up.</param>
        /// <param name="createCallback">callback function that will be called to create the dynamic field</param>
        /// <returns>An <see cref="IDynamicField"/> for the given field info.</returns>
        internal static IDynamicField GetDynamicField(FieldInfo field, CreateFieldCallback createCallback)
        {
            lock (fieldCache.SyncRoot)
            {
                IDynamicField dynamicField = (IDynamicField)fieldCache[field];
                if (dynamicField == null)
                {
                    dynamicField = createCallback(field);
                    fieldCache[field] = dynamicField;
                }
                return dynamicField;
            }
        }

        /// <summary>
        /// Returns dynamic indexer if one exists.
        /// </summary>
        /// <param name="indexer">Indexer to look up.</param>
        /// <param name="createCallback">callback function that will be called to create the dynamic indexer</param>
        /// <returns>An <see cref="IDynamicIndexer"/> for the given indexer.</returns>
        internal static IDynamicIndexer GetDynamicIndexer(PropertyInfo indexer, CreateIndexerCallback createCallback)
        {
            lock (indexerCache.SyncRoot)
            {
                IDynamicIndexer dynamicIndexer = (IDynamicIndexer)indexerCache[indexer];
                if (dynamicIndexer == null)
                {
                    dynamicIndexer = createCallback(indexer);
                    indexerCache[indexer] = dynamicIndexer;
                }
                return dynamicIndexer;
            }
        }

        /// <summary>
        /// Returns dynamic method if one exists.
        /// </summary>
        /// <param name="method">Method to look up.</param>
        /// <param name="createCallback">callback function that will be called to create the dynamic method</param>
        /// <returns>An <see cref="IDynamicMethod"/> for the given method.</returns>
        internal static IDynamicMethod GetDynamicMethod(MethodInfo method, CreateMethodCallback createCallback)
        {
            lock (methodCache.SyncRoot)
            {
                IDynamicMethod dynamicMethod = (IDynamicMethod)methodCache[method];
                if (dynamicMethod == null)
                {
                    dynamicMethod = createCallback(method);
                    methodCache[method] = dynamicMethod;
                }
                return dynamicMethod;
            }
        }

        /// <summary>
        /// Returns dynamic constructor if one exists.
        /// </summary>
        /// <param name="constructor">Constructor to look up.</param>
        /// <param name="createCallback">callback function that will be called to create the dynamic constructor</param>
        /// <returns>An <see cref="IDynamicConstructor"/> for the given constructor.</returns>
        internal static IDynamicConstructor GetDynamicConstructor(ConstructorInfo constructor, CreateConstructorCallback createCallback)
        {
            lock (constructorCache.SyncRoot)
            {
                IDynamicConstructor dynamicConstructor = (IDynamicConstructor)constructorCache[constructor];
                if (dynamicConstructor == null)
                {
                    dynamicConstructor = createCallback(constructor);
                    constructorCache[constructor] = dynamicConstructor;
                }
                return dynamicConstructor;
            }
        }

        /// <summary>
        /// Saves dynamically generated assembly to disk.
        /// Can only be called in DEBUG mode, per ConditionalAttribute rules.
        /// </summary>
        [Conditional("DEBUG_DYNAMIC")]
        public static void SaveAssembly()
        {
            DynamicCodeManager.SaveAssembly(ASSEMBLY_NAME);
        }

        #endregion

#if NET_2_0
        private static readonly ConstructorInfo NewInvalidOperationException =
            typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) });

        /// <summary>
        /// Create a new Get method delegate for the specified field using <see cref="System.Reflection.Emit.DynamicMethod"/>
        /// </summary>
        /// <param name="fieldInfo">the field to create the delegate for</param>
        /// <returns>a delegate that can be used to read the field</returns>
        public static GetterDelegate CreateFieldGetter(FieldInfo fieldInfo)
        {
            AssertUtils.ArgumentNotNull(fieldInfo, "You cannot create a delegate for a null value.");

            System.Reflection.Emit.DynamicMethod dmGetter = new System.Reflection.Emit.DynamicMethod("getter", typeof(object), new Type[] { typeof(object) }, fieldInfo.DeclaringType.Module, true);
            ILGenerator il = dmGetter.GetILGenerator();
            if (fieldInfo.IsLiteral)
            {
                object value = fieldInfo.GetValue(null);
                EmitConstant(il, value);
            }
            else if (fieldInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldsfld, fieldInfo);
            }
            else
            {
                EmitTarget(il, fieldInfo.DeclaringType);
                il.Emit(OpCodes.Ldfld, fieldInfo);
            }

            if (fieldInfo.FieldType.IsValueType)
            {
                il.Emit(OpCodes.Box, fieldInfo.FieldType);
            }
            il.Emit(OpCodes.Ret);
            return (GetterDelegate)dmGetter.CreateDelegate(typeof(GetterDelegate));
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
        public static SetterDelegate CreateFieldSetter(FieldInfo fieldInfo)
        {
            AssertUtils.ArgumentNotNull(fieldInfo, "You cannot create a delegate for a null value.");

            System.Reflection.Emit.DynamicMethod dmSetter = new System.Reflection.Emit.DynamicMethod("setter", null, new Type[] { typeof(object), typeof(object) }, fieldInfo.DeclaringType.Module, true);
            ILGenerator il = dmSetter.GetILGenerator();

            if (!fieldInfo.IsLiteral
                && !fieldInfo.IsInitOnly
                && !(fieldInfo.DeclaringType.IsValueType && !fieldInfo.IsStatic))
            {
                if (!fieldInfo.IsStatic)
                {
                    EmitTarget(il, fieldInfo.DeclaringType);
                }
                il.Emit(OpCodes.Ldarg_1);
                if (fieldInfo.FieldType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
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
                EmitThrowInvalidOperationException(il, string.Format("Cannot write to constant field '{0}.{1}'", fieldInfo.DeclaringType.FullName, fieldInfo.Name));
            }
            return (SetterDelegate)dmSetter.CreateDelegate(typeof(SetterDelegate));
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
        public static GetterDelegate CreatePropertyGetter(PropertyInfo propertyInfo)
        {
            AssertUtils.ArgumentNotNull(propertyInfo, "You cannot create a delegate for a null value.");

            NetDynamicMethod dm = new NetDynamicMethod(string.Empty, typeof(object), new Type[] { typeof(object) }, propertyInfo.DeclaringType.Module, true);
            ILGenerator il = dm.GetILGenerator();

            if (propertyInfo.CanRead)
            {
                MethodInfo method = propertyInfo.GetGetMethod(true);
                if (!method.IsStatic)
                {
                    EmitTarget(il, method.DeclaringType);
                }
                EmitCall(il, method);
                if (propertyInfo.PropertyType.IsValueType)
                {
                    il.Emit(OpCodes.Box, propertyInfo.PropertyType);
                }
                il.Emit(OpCodes.Ret);
            }
            else
            {
                EmitThrowInvalidOperationException(il, string.Format("Cannot read from write-only property '{0}.{1}'", propertyInfo.DeclaringType.FullName, propertyInfo.Name));
            }
            return (GetterDelegate)dm.CreateDelegate(typeof(GetterDelegate));
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
        public static SetterDelegate CreatePropertySetter(PropertyInfo propertyInfo)
        {
            AssertUtils.ArgumentNotNull(propertyInfo, "You cannot create a delegate for a null value.");

            NetDynamicMethod dm = new NetDynamicMethod(string.Empty, null, new Type[] { typeof(object), typeof(object) }, propertyInfo.DeclaringType.Module, true);
            ILGenerator il = dm.GetILGenerator();

            if (propertyInfo.CanWrite
                && !(propertyInfo.DeclaringType.IsValueType && !propertyInfo.GetSetMethod(true).IsStatic))
            {
                MethodInfo method = propertyInfo.GetSetMethod(true);
                if (!method.IsStatic)
                {
                    EmitTarget(il, method.DeclaringType);
                }

                il.Emit(OpCodes.Ldarg_1);
                if (propertyInfo.PropertyType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                }
                EmitCall(il, method);
                il.Emit(OpCodes.Ret);
            }
            else
            {
                EmitThrowInvalidOperationException(il, string.Format("Cannot write to read-only property '{0}.{1}'", propertyInfo.DeclaringType.FullName, propertyInfo.Name));
            }

            return (SetterDelegate)dm.CreateDelegate(typeof(SetterDelegate));
        }

        /// <summary>
        /// Create a new method delegate for the specified method using <see cref="System.Reflection.Emit.DynamicMethod"/>
        /// </summary>
        /// <param name="method">the method to create the delegate for</param>
        /// <returns>a delegate that can be used to invoke the method.</returns>
        private static FunctionDelegate CreateFunctionDelegate(MethodInfo method)
        {
            NetDynamicMethod dm = new NetDynamicMethod(string.Empty, typeof(object), new Type[] { typeof(object), typeof(object[]) }, method.DeclaringType.Module, true);
            ILGenerator il = dm.GetILGenerator();
            ParameterInfo[] parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo p = parameters[i];
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_Ref);
                if (p.ParameterType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, p.ParameterType);
                }
            }

            if (!method.IsStatic)
            {
                EmitTarget(il, method.DeclaringType);
            }
            EmitCall(il, method);

            if (method.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            il.Emit(OpCodes.Ret);
            return (FunctionDelegate)dm.CreateDelegate(typeof(FunctionDelegate));
        }

        private static void EmitTarget(ILGenerator il, Type targetType)
        {
            il.Emit(OpCodes.Ldarg_0);
            if (targetType.IsValueType)
            {
                LocalBuilder local = il.DeclareLocal(targetType);
                il.Emit(OpCodes.Unbox_Any, targetType);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, 0);
            }
        }

        private static void EmitCall(ILGenerator il, MethodInfo method)
        {
            il.Emit((method.IsVirtual) ? OpCodes.Callvirt : OpCodes.Call, method);
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
                il.Emit(OpCodes.Ldc_I4, (byte)value);
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
#endif
    }
}