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
    #region IDynamicField interface

    /// <summary>
    /// Defines methods that dynamic field class has to implement.
    /// </summary>
    public interface IDynamicField
    {
        /// <summary>
        /// Gets the value of the dynamic field for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get field value from.
        /// </param>
        /// <returns>
        /// A field value.
        /// </returns>
        object GetValue(object target);

        /// <summary>
        /// Gets the value of the dynamic field for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set field value on.
        /// </param>
        /// <param name="value">
        /// A new field value.
        /// </param>
        void SetValue(object target, object value);
    }

    #endregion

    #region Safe wrapper

    /// <summary>
    /// Safe wrapper for the dynamic field.
    /// </summary>
    /// <remarks>
    /// <see cref="SafeField"/> will attempt to use dynamic
    /// field if possible, but it will fall back to standard
    /// reflection if necessary.
    /// </remarks>
    public class SafeField : IDynamicField
    {
        private readonly FieldInfo fieldInfo;
#if NET_2_0
        #region Cache

        private static readonly IDictionary fieldCache = new Hashtable();

        /// <summary>
        /// Holds cached Getter/Setter delegates for a Field
        /// </summary>
        private class DynamicFieldCacheEntry
        {
            public readonly FieldGetterDelegate Getter;
            public readonly FieldSetterDelegate Setter;

            public DynamicFieldCacheEntry(FieldGetterDelegate getter, FieldSetterDelegate setter)
            {
                Getter = getter;
                Setter = setter;
            }
        }

        /// <summary>
        /// Obtains cached fieldInfo or creates a new entry, if none is found.
        /// </summary>
        private static DynamicFieldCacheEntry GetOrCreateDynamicField(FieldInfo field)
        {
            DynamicFieldCacheEntry fieldInfo = (DynamicFieldCacheEntry)fieldCache[field];
            if (fieldInfo == null)
            {
                fieldInfo = new DynamicFieldCacheEntry(DynamicReflectionManager.CreateFieldGetter(field), DynamicReflectionManager.CreateFieldSetter(field));
                lock (fieldCache)
                {
                    fieldCache[field] = fieldInfo;
                }
            }
            return fieldInfo;
        }

        #endregion

        private readonly FieldGetterDelegate getter;
        private readonly FieldSetterDelegate setter;

        /// <summary>
        /// Creates a new instance of the safe field wrapper.
        /// </summary>
        /// <param name="field">Field to wrap.</param>
        public SafeField(FieldInfo field)
        {
            AssertUtils.ArgumentNotNull(field, "You cannot create a dynamic field for a null value.");

            fieldInfo = field;
            DynamicFieldCacheEntry fi = GetOrCreateDynamicField(field);
            getter = fi.Getter;
            setter = fi.Setter;
        }

        /// <summary>
        /// Gets the value of the dynamic field for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get field value from.
        /// </param>
        /// <returns>
        /// A field value.
        /// </returns>
        public object GetValue(object target)
        {
            return getter(target);
        }

        /// <summary>
        /// Gets the value of the dynamic field for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set field value on.
        /// </param>
        /// <param name="value">
        /// A new field value.
        /// </param>
        public void SetValue(object target, object value)
        {
            setter(target, value);
        }

#else
        /// <summary>
        /// Returns a <see cref="IDynamicField"/> implementation
        /// by determining the fastest possible dynamic access strategy 
        /// </summary>
        /// <param name="field">the field to be wrapped</param>
        /// <returns>an <see cref="IDynamicField"/> instance for accessing the 
        /// field represented by the given <see cref="FieldInfo"/></returns>
        public static IDynamicField CreateFrom(FieldInfo field)
        {
            AssertUtils.ArgumentNotNull(field, "You cannot create a dynamic field for a null value.");

            IDynamicField dynamicField;

            if (field.IsPublic &&
               ReflectionUtils.IsTypeVisible(field.DeclaringType, DynamicReflectionManager.ASSEMBLY_NAME))
            {
                dynamicField = DynamicField.Create(field);
            }
            else
            {
                dynamicField = new SafeField(field);
            }

            return dynamicField;
        }

        private readonly IDynamicField dynamicField;
        private readonly bool isOptimizedGet = false;
        private bool isOptimizedSet = false;
        private readonly bool canSet;

        /// <summary>
        /// Creates a new instance of the safe field wrapper.
        /// </summary>
        /// <param name="field">Field to wrap.</param>
        public SafeField(FieldInfo field)
        {
            AssertUtils.ArgumentNotNull(field, "You cannot create a dynamic field for a null value.");

            this.fieldInfo = field;
            this.canSet = (!fieldInfo.IsLiteral
                && !fieldInfo.IsInitOnly
                && !(fieldInfo.DeclaringType.IsValueType && !fieldInfo.IsStatic));

            if (fieldInfo.IsPublic &&
                ReflectionUtils.IsTypeVisible(fieldInfo.DeclaringType, DynamicReflectionManager.ASSEMBLY_NAME))
            {
                dynamicField = DynamicField.Create(fieldInfo);
                isOptimizedGet = isOptimizedSet = true;
            }
        }

        /// <summary>
        /// Gets the value of the dynamic field for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get field value from.
        /// </param>
        /// <returns>
        /// A field value.
        /// </returns>
        public object GetValue(object target)
        {
            if (isOptimizedGet)
            {
                return dynamicField.GetValue(target);
            }
            else
            {
                return fieldInfo.GetValue(target);
            }
        }

        /// <summary>
        /// Gets the value of the dynamic field for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set field value on.
        /// </param>
        /// <param name="value">
        /// A new field value.
        /// </param>
        public void SetValue(object target, object value)
        {
            if (isOptimizedSet)
            {
                try
                {
                    dynamicField.SetValue(target, value);
                    return;
                }
                catch (InvalidCastException)
                {
                    isOptimizedSet = false;
                }
            }

            if (!canSet)
            {
                throw new InvalidOperationException("Cannot set value of a read-only field or a constant.");
            }

            try
            {
                fieldInfo.SetValue(target, value);
            }
            catch(ArgumentException)
            {
                if (value != null && !fieldInfo.GetType().IsAssignableFrom(value.GetType()))
                {
                    throw new InvalidCastException();
                }
                throw;
            }
        }
#endif
        internal FieldInfo FieldInfo
        {
            get { return fieldInfo; }
        }
    }

    #endregion

#if NET_2_0
    /// <summary>
    /// Factory class for dynamic fields.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class DynamicField : BaseDynamicMember
    {
        /// <summary>
        /// Creates dynamic field instance for the specified <see cref="FieldInfo"/>.
        /// </summary>
        /// <param name="field">Field info to create dynamic field for.</param>
        /// <returns>Dynamic field for the specified <see cref="FieldInfo"/>.</returns>
        public static IDynamicField Create(FieldInfo field)
        {
            AssertUtils.ArgumentNotNull(field, "You cannot create a dynamic field for a null value.");

            IDynamicField dynamicField = new SafeField(field);
            return dynamicField;
        }
    }
#else
    /// <summary>
    /// Factory class for dynamic fields.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class DynamicField : BaseDynamicMember
    {
        private static readonly CreateFieldCallback s_createCallback = new CreateFieldCallback(CreateInternal);

        #region Create Method

        /// <summary>
        /// Creates dynamic field instance for the specified <see cref="FieldInfo"/>.
        /// </summary>
        /// <param name="field">Field info to create dynamic field for.</param>
        /// <returns>Dynamic field for the specified <see cref="FieldInfo"/>.</returns>
        public static IDynamicField Create(FieldInfo field)
        {
            AssertUtils.ArgumentNotNull(field, "You cannot create a dynamic field for a null value.");

            IDynamicField dynamicField = DynamicReflectionManager.GetDynamicField(field, s_createCallback);
            return dynamicField;
        }

        private static IDynamicField CreateInternal(FieldInfo field)
        {
            IDynamicField dynamicField = null;

            TypeBuilder tb = DynamicReflectionManager.CreateTypeBuilder("Field_" + field.Name);
            tb.AddInterfaceImplementation(typeof(IDynamicField));

            GenerateGetValue(tb, field);
            GenerateSetValue(tb, field);

            Type dynamicFieldType = tb.CreateType();
            ConstructorInfo ctor = dynamicFieldType.GetConstructor(Type.EmptyTypes);
            dynamicField = (IDynamicField)ctor.Invoke(ObjectUtils.EmptyObjects);

            return dynamicField;
        }

        private static void GenerateGetValue(TypeBuilder tb, FieldInfo field)
        {
            MethodBuilder getValueMethod =
                tb.DefineMethod("GetValue", METHOD_ATTRIBUTES, typeof(object), new Type[] { typeof(object) });
            getValueMethod.DefineParameter(1, ParameterAttributes.None, "target");

            ILGenerator il = getValueMethod.GetILGenerator();
            if (field.IsLiteral)
            {
                EmitConstant(il, field.GetValue(field.FieldType));
            }
            else if (field.IsStatic)
            {
                il.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                SetupTargetInstance(il, field.DeclaringType);
                il.Emit(OpCodes.Ldfld, field);
            }
            ProcessReturnValue(il, field.FieldType);
            il.Emit(OpCodes.Ret);
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

        private static void GenerateSetValue(TypeBuilder tb, FieldInfo field)
        {
            MethodBuilder setValueMethod =
                tb.DefineMethod("SetValue", METHOD_ATTRIBUTES, typeof(void),
                                new Type[] { typeof(object), typeof(object) });
            setValueMethod.DefineParameter(1, ParameterAttributes.None, "target");
            setValueMethod.DefineParameter(2, ParameterAttributes.None, "value");

            ILGenerator il = setValueMethod.GetILGenerator();
            if (!(field.IsInitOnly || field.IsLiteral))
            {
                bool isValueType = field.DeclaringType.IsValueType;
                if (isValueType && !field.IsStatic)
                {
                    ThrowInvalidOperationException(il, "Cannot set field value on a value type due to boxing.");
                }
                else
                {
                    bool isStatic = field.IsStatic;

                    if (!isStatic)
                    {
                        SetupTargetInstance(il, field.DeclaringType);
                    }

                    SetupArgument(il, field.FieldType, 2);
                    if (!isStatic)
                    {
                        il.Emit(OpCodes.Stfld, field);
                    }
                    else
                    {
                        il.Emit(OpCodes.Stsfld, field);
                    }
                    il.Emit(OpCodes.Ret);
                }
            }
            else
            {
                ThrowInvalidOperationException(il, "Cannot set value of a read-only field or a constant.");
            }
        }

        #endregion
    }
#endif // NET_2_0
}

