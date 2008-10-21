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

using Common.Logging;
using Spring.Util;

#endregion

namespace Spring.Reflection.Dynamic
{
    #region IDynamicProperty interface

    /// <summary>
    /// Defines methods that dynamic property class has to implement.
    /// </summary>
    public interface IDynamicProperty
    {
        /// <summary>
        /// Gets the value of the dynamic property for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get property value from.
        /// </param>
        /// <returns>
        /// A property value.
        /// </returns>
        object GetValue(object target);

        /// <summary>
        /// Gets the value of the dynamic property for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set property value on.
        /// </param>
        /// <param name="value">
        /// A new property value.
        /// </param>
        void SetValue(object target, object value);

        /// <summary>
        /// Gets the value of the dynamic property for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get property value from.
        /// </param>
        /// <param name="index">Optional index values for indexed properties. This value should be null reference for non-indexed properties.</param>
        /// <returns>
        /// A property value.
        /// </returns>
        object GetValue(object target, params object[] index);

        /// <summary>
        /// Gets the value of the dynamic property for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set property value on.
        /// </param>
        /// <param name="value">
        /// A new property value.
        /// </param>
        /// <param name="index">Optional index values for indexed properties. This value should be null reference for non-indexed properties.</param>
        void SetValue(object target, object value, params object[] index);
    }

    #endregion

    #region Safe wrapper

    /// <summary>
    /// Safe wrapper for the dynamic property.
    /// </summary>
    /// <remarks>
    /// <see cref="SafeProperty"/> will attempt to use dynamic
    /// property if possible, but it will fall back to standard
    /// reflection if necessary.
    /// </remarks>
    public class SafeProperty : IDynamicProperty
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SafeProperty));

        private readonly PropertyInfo propertyInfo;

#if NET_2_0

        #region Cache

        private static readonly IDictionary propertyCache = new Hashtable();

        /// <summary>
        /// Holds cached Getter/Setter delegates for a Property
        /// </summary>
        private class DynamicPropertyCacheEntry
        {
            public readonly PropertyGetterDelegate Getter;
            public readonly PropertySetterDelegate Setter;

            public DynamicPropertyCacheEntry(PropertyGetterDelegate getter, PropertySetterDelegate setter)
            {
                Getter = getter;
                Setter = setter;
            }
        }

        /// <summary>
        /// Obtains cached property info or creates a new entry, if none is found.
        /// </summary>
        private static DynamicPropertyCacheEntry GetOrCreateDynamicProperty(PropertyInfo property)
        {
            DynamicPropertyCacheEntry propertyInfo = (DynamicPropertyCacheEntry)propertyCache[property];
            if (propertyInfo == null)
            {
                propertyInfo = new DynamicPropertyCacheEntry(DynamicReflectionManager.CreatePropertyGetter(property), DynamicReflectionManager.CreatePropertySetter(property));
                lock (propertyCache)
                {
                    propertyCache[property] = propertyInfo;
                }
            }
            return propertyInfo;
        }

        #endregion

        private readonly PropertyGetterDelegate getter;
        private readonly PropertySetterDelegate setter;

        /// <summary>
        /// Creates a new instance of the safe property wrapper.
        /// </summary>
        /// <param name="propertyInfo">Property to wrap.</param>
        public SafeProperty(PropertyInfo propertyInfo)
        {
            AssertUtils.ArgumentNotNull(propertyInfo, "You cannot create a dynamic property for a null value.");

            this.propertyInfo = propertyInfo;
            DynamicPropertyCacheEntry pi = GetOrCreateDynamicProperty(propertyInfo);
            getter = pi.Getter;
            setter = pi.Setter;
        }

        /// <summary>
        /// Gets the value of the dynamic property for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get property value from.
        /// </param>
        /// <returns>
        /// A property value.
        /// </returns>
        public object GetValue(object target)
        {
            return getter(target);
        }

        /// <summary>
        /// Gets the value of the dynamic property for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get property value from.
        /// </param>
        /// <param name="index">Optional index values for indexed properties. This value should be null reference for non-indexed properties.</param>
        /// <returns>
        /// A property value.
        /// </returns>
        public object GetValue(object target, params object[] index)
        {
            return getter(target, index);
        }

        /// <summary>
        /// Gets the value of the dynamic property for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set property value on.
        /// </param>
        /// <param name="value">
        /// A new property value.
        /// </param>
        public void SetValue(object target, object value)
        {
            setter(target, value);
        }

        /// <summary>
        /// Gets the value of the dynamic property for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set property value on.
        /// </param>
        /// <param name="value">
        /// A new property value.
        /// </param>
        /// <param name="index">Optional index values for indexed properties. This value should be null reference for non-indexed properties.</param>
        public void SetValue(object target, object value, params object[] index)
        {
            setter(target, value, index);
        }

#else
        private readonly IDynamicProperty dynamicProperty;
        private readonly bool isOptimizedGet = false;
        private bool isOptimizedSet = false;
        private readonly bool canSet;

        /// <summary>
        /// Creates a new instance of the safe property wrapper.
        /// </summary>
        /// <param name="property">Property to wrap.</param>
        public SafeProperty(PropertyInfo property)
        {
            this.propertyInfo = property;

            if (property.CanRead
                && property.GetGetMethod() != null
                && ReflectionUtils.IsTypeVisible(property.DeclaringType, DynamicReflectionManager.ASSEMBLY_NAME))
            {
                dynamicProperty = DynamicProperty.Create(property);
                isOptimizedGet = true;
            }

            canSet = property.CanWrite && !(propertyInfo.DeclaringType.IsValueType && !propertyInfo.GetSetMethod(true).IsStatic);
            if (property.GetSetMethod() != null
                && ReflectionUtils.IsTypeVisible(property.DeclaringType, DynamicReflectionManager.ASSEMBLY_NAME))
            {
                if (dynamicProperty == null)
                {
                    dynamicProperty = DynamicProperty.Create(property);
                }
                isOptimizedSet = true;
            }
        }

        /// <summary>
        /// Gets the value of the dynamic property for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get property value from.
        /// </param>
        /// <returns>
        /// A property value.
        /// </returns>
        public object GetValue(object target)
        {
            if (isOptimizedGet)
            {
                return dynamicProperty.GetValue(target);
            }
            else
            {
                return propertyInfo.GetValue(target, null);
            }
        }

        /// <summary>
        /// Gets the value of the dynamic property for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set property value on.
        /// </param>
        /// <param name="value">
        /// A new property value.
        /// </param>
        public void SetValue(object target, object value)
        {
            try
            {
                if (isOptimizedSet)
                {
                    try
                    {
                        dynamicProperty.SetValue(target, value);
                    }
                    catch (InvalidCastException ex)
                    {
                        Log.Debug("Failed optimized set", ex);
                        isOptimizedSet = false;
                        propertyInfo.SetValue(target, value, null);
                    }
                }
                else
                {
                    if (!canSet)
                    {
                        throw (propertyInfo.DeclaringType.IsValueType)
                            ? new InvalidOperationException("Cannot set property value on a value type due to boxing.")
                            : new InvalidOperationException("Cannot set value of a read-only property.");
                    }

                    propertyInfo.SetValue(target, value, null);
                }
            }
            catch (Exception ex)
            {
                Log.Error(
                    string.Format("Failed setting value '{0}' on property '{2}.{1}' on instance of type '{3}'",
                                  (value != null ? value.GetType().FullName : "<null>"), 
                                  propertyInfo.Name, 
                                  propertyInfo.DeclaringType.FullName,
                                  (target != null ? target.GetType().FullName : "<null>")), ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get indexer value from.
        /// </param>
        /// <param name="index">
        /// Indexer arguments.
        /// </param>
        /// <returns>
        /// A indexer value.
        /// </returns>
        public object GetValue(object target, object[] index)
        {
            if (isOptimizedGet)
            {
                try
                {
                    return dynamicProperty.GetValue(target, index);
                }
                catch (InvalidCastException)
                {
                    isOptimizedSet = false;
                }
            }
            return propertyInfo.GetValue(target, index);
        }

        /// <summary>
        /// Sets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set indexer value on.
        /// </param>
        /// <param name="index">
        /// Indexer arguments.
        /// </param>
        /// <param name="value">
        /// A new indexer value.
        /// </param>
        public void SetValue(object target, object value, object[] index)
        {
            if (isOptimizedSet)
            {
                try
                {
                    dynamicProperty.SetValue(target, value, index);
                    return;
                }
                catch (InvalidCastException ex)
                {
                    Log.Debug("Failed optimized set", ex);
                    isOptimizedSet = false;
                }
            }
            propertyInfo.SetValue(target, value, index);
        }
#endif
        /// <summary>
        /// Internal PropertyInfo accessor.
        /// </summary>
        internal PropertyInfo PropertyInfo
        {
            get { return propertyInfo; }
        }
    }

    #endregion

#if NET_2_0
    /// <summary>
    /// Factory class for dynamic properties.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class DynamicProperty : BaseDynamicMember
    {
        /// <summary>
        /// Creates safe dynamic property instance for the specified <see cref="PropertyInfo"/>.
        /// </summary>
        /// <remarks>
        /// <p>This factory method will create a dynamic property with a "safe" wrapper.</p>
        /// <p>Safe wrapper will attempt to use generated dynamic property if possible,
        /// but it will fall back to standard reflection if necessary.</p>
        /// </remarks>
        /// <param name="property">Property info to create dynamic property for.</param>
        /// <returns>Safe dynamic property for the specified <see cref="PropertyInfo"/>.</returns>
        /// <seealso cref="SafeProperty"/>
        public static IDynamicProperty CreateSafe(PropertyInfo property)
        {
            return new SafeProperty(property);
        }

        /// <summary>
        /// Creates dynamic property instance for the specified <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="property">Property info to create dynamic property for.</param>
        /// <returns>Dynamic property for the specified <see cref="PropertyInfo"/>.</returns>
        public static IDynamicProperty Create(PropertyInfo property)
        {
            AssertUtils.ArgumentNotNull(property, "You cannot create a dynamic property for a null value.");

            IDynamicProperty dynamicProperty = new SafeProperty(property);
            return dynamicProperty;
        }
    }
#else
    /// <summary>
    /// Factory class for dynamic properties.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class DynamicProperty : BaseDynamicMember
    {
        private readonly static CreatePropertyCallback s_createPropertyCallback = new CreatePropertyCallback(CreateInternal);

        #region Create Method

        /// <summary>
        /// Creates safe dynamic property instance for the specified <see cref="PropertyInfo"/>.
        /// </summary>
        /// <remarks>
        /// <p>This factory method will create a dynamic property with a "safe" wrapper.</p>
        /// <p>Safe wrapper will attempt to use generated dynamic property if possible,
        /// but it will fall back to standard reflection if necessary.</p>
        /// </remarks>
        /// <param name="property">Property info to create dynamic property for.</param>
        /// <returns>Safe dynamic property for the specified <see cref="PropertyInfo"/>.</returns>
        /// <seealso cref="SafeProperty"/>
        public static IDynamicProperty CreateSafe(PropertyInfo property)
        {
            return new SafeProperty(property);
        }

        /// <summary>
        /// Creates dynamic property instance for the specified <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="property">Property info to create dynamic property for.</param>
        /// <returns>Dynamic property for the specified <see cref="PropertyInfo"/>.</returns>
        public static IDynamicProperty Create(PropertyInfo property)
        {
            AssertUtils.ArgumentNotNull(property, "You cannot create a dynamic property for a null value.");

            IDynamicProperty dynamicProperty = DynamicReflectionManager.GetDynamicProperty(property, s_createPropertyCallback);
            return dynamicProperty;
        }

        private static IDynamicProperty CreateInternal(PropertyInfo property)
        {
            IDynamicProperty dynamicProperty = null;

            TypeBuilder tb = DynamicReflectionManager.CreateTypeBuilder("Property_" + property.Name);
            tb.AddInterfaceImplementation(typeof(IDynamicProperty));

            GenerateGetValue(tb, property);
            GenerateGetIndexedValue(tb, property);
            GenerateSetValue(tb, property);
            GenerateSetIndexedValue(tb, property);

            Type dynamicPropertyType = tb.CreateType();
            ConstructorInfo ctor = dynamicPropertyType.GetConstructor(Type.EmptyTypes);
            dynamicProperty = (IDynamicProperty)ctor.Invoke(ObjectUtils.EmptyObjects);

            return dynamicProperty;
        }

        private static void GenerateGetValue(TypeBuilder tb, PropertyInfo property)
        {
            MethodBuilder getValueMethod =
                tb.DefineMethod("GetValue", METHOD_ATTRIBUTES, typeof(object), new Type[] { typeof(object) });
            getValueMethod.DefineParameter(1, ParameterAttributes.None, "target");

            ILGenerator il = getValueMethod.GetILGenerator();
            if (property.CanRead)
            {
                MethodInfo getMethod = property.GetGetMethod(true);
                bool isValueType = property.DeclaringType.IsValueType;
                bool isStatic = getMethod.IsStatic;

                if (!isStatic)
                {
                    SetupTargetInstance(il, property.DeclaringType);
                }

                InvokeMethod(il, isStatic, isValueType, getMethod);
                ProcessReturnValue(il, property.PropertyType);
                il.Emit(OpCodes.Ret);
            }
            else
            {
                ThrowInvalidOperationException(il, "Cannot get value of a non-readable property");
            }
        }

        private static void GenerateGetIndexedValue(TypeBuilder tb, PropertyInfo indexer)
        {
            MethodBuilder getValueMethod =
                tb.DefineMethod("GetValue", METHOD_ATTRIBUTES, typeof(object), new Type[] { typeof(object), typeof(object[]) });
            getValueMethod.DefineParameter(1, ParameterAttributes.None, "target");
            getValueMethod.DefineParameter(2, ParameterAttributes.None, "index");
            
            ILGenerator il = getValueMethod.GetILGenerator();
            if (indexer.CanRead)
            {
                MethodInfo getMethod = indexer.GetGetMethod();
                bool isValueType = indexer.DeclaringType.IsValueType;
                bool isStatic = getMethod.IsStatic;
                
                if (!isStatic)
                {
                    SetupTargetInstance(il, indexer.DeclaringType);
                }

                Type[] argTypes = ReflectionUtils.GetParameterTypes(getMethod);
                for (int i = 0; i < argTypes.Length; i++)
                {
                    SetupIndexerArgument(il, 2, argTypes[i], i, true);
                }

                InvokeMethod(il, isStatic, isValueType, getMethod);
                ProcessReturnValue(il, indexer.PropertyType);
                il.Emit(OpCodes.Ret);
            }
            else
            {
                ThrowInvalidOperationException(il, "Cannot get value of a non-readable indexer");
            }
        }

        private static void GenerateSetValue(TypeBuilder tb, PropertyInfo property)
        {
            MethodBuilder setValueMethod =
                tb.DefineMethod("SetValue", METHOD_ATTRIBUTES, typeof(void),
                                new Type[] { typeof(object), typeof(object) });
            setValueMethod.DefineParameter(1, ParameterAttributes.None, "target");
            setValueMethod.DefineParameter(2, ParameterAttributes.None, "value");

            ILGenerator il = setValueMethod.GetILGenerator();
            if (property.CanWrite)
            {
                bool isValueType = property.DeclaringType.IsValueType;
                // we can't set nonstatic properties on value types (due to boxing)
                bool canSet = !(isValueType && !property.GetSetMethod(true).IsStatic);
                if (!canSet)
                {
                    ThrowInvalidOperationException(il, "Cannot set property value on a value type due to boxing.");
                }
                else
                {
                    MethodInfo setMethod = property.GetSetMethod(true);
                    bool isStatic = setMethod.IsStatic;

                    if (!isStatic)
                    {
                        SetupTargetInstance(il, property.DeclaringType);
                    }

                    SetupArgument(il, property.PropertyType, 2);
                    InvokeMethod(il, isStatic, property.DeclaringType.IsValueType, setMethod);
                    il.Emit(OpCodes.Ret);
                }
            }
            else
            {
                ThrowInvalidOperationException(il, "Cannot set value of a read-only property");
            }
        }

        private static void GenerateSetIndexedValue(TypeBuilder tb, PropertyInfo indexer)
        {
            MethodBuilder setValueMethod =
                tb.DefineMethod("SetValue", METHOD_ATTRIBUTES, typeof(void),
                new Type[] { typeof(object), typeof(object), typeof(object[]) });
            setValueMethod.DefineParameter(1, ParameterAttributes.None, "target");
            setValueMethod.DefineParameter(2, ParameterAttributes.None, "value");
            setValueMethod.DefineParameter(3, ParameterAttributes.None, "index");

            ILGenerator il = setValueMethod.GetILGenerator();
            if (indexer.CanWrite)
            {
                bool isValueType = indexer.DeclaringType.IsValueType;
                if (isValueType)
                {
                    ThrowInvalidOperationException(il, "Cannot set indexer value on a value type due to boxing.");
                }
                else
                {
                    MethodInfo setMethod = indexer.GetSetMethod();
                    bool isStatic = setMethod.IsStatic;

                    if (!isStatic)
                    {
                        SetupTargetInstance(il, indexer.DeclaringType);
                    }

                    Type[] argTypes = ReflectionUtils.GetParameterTypes(setMethod);
                    for (int i = 0; i < argTypes.Length - 1; i++)
                    {
                        SetupIndexerArgument(il, 3, argTypes[i], i, true);
                    }

                    SetupArgument(il, indexer.PropertyType, 2);
                    InvokeMethod(il, isStatic, isValueType, setMethod);
                    il.Emit(OpCodes.Ret);
                }
            }
            else
            {
                ThrowInvalidOperationException(il, "Cannot set value of a read-only indexer");
            }
        }

        private static OpCode[] LdArgOpCodes = { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };

        private static void SetupIndexerArgument(ILGenerator il, int indexArgumentPosition, Type argumentType, int argumentPosition, bool isObjectArray)
        {
            il.Emit(LdArgOpCodes[indexArgumentPosition]);
            if (isObjectArray)
            {
                il.Emit(OpCodes.Ldc_I4, argumentPosition);
                il.Emit(OpCodes.Ldelem_Ref);
            }
            if (argumentType.IsValueType)
            {
                il.Emit(OpCodes.Unbox, argumentType);
                il.Emit(OpCodes.Ldobj, argumentType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, argumentType);
            }
        }

        #endregion
    }

#endif
}