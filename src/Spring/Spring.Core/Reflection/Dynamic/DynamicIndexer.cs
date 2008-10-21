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
using Spring.Reflection.Dynamic;
using Spring.Util;

#endregion

namespace Spring.Reflection.Dynamic
{
    #region IDynamicIndexer interface

    /// <summary>
    /// Defines methods that dynamic indexer class has to implement.
    /// </summary>
    public interface IDynamicIndexer
    {
        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get the indexer value from.
        /// </param>
        /// <param name="index">
        /// Indexer argument.
        /// </param>
        /// <returns>
        /// A indexer value.
        /// </returns>
        object GetValue( object target, int index );

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get the indexer value from.
        /// </param>
        /// <param name="index">
        /// Indexer argument.
        /// </param>
        /// <returns>
        /// A indexer value.
        /// </returns>
        object GetValue( object target, object index );

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get the indexer value from.
        /// </param>
        /// <param name="index">
        /// Indexer arguments.
        /// </param>
        /// <returns>
        /// A indexer value.
        /// </returns>
        object GetValue( object target, object[] index );

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set the indexer value on.
        /// </param>
        /// <param name="index">
        /// Indexer argument.
        /// </param>
        /// <param name="value">
        /// A new indexer value.
        /// </param>
        void SetValue( object target, int index, object value );

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set the indexer value on.
        /// </param>
        /// <param name="index">
        /// Indexer argument.
        /// </param>
        /// <param name="value">
        /// A new indexer value.
        /// </param>
        void SetValue( object target, object index, object value );

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set the indexer value on.
        /// </param>
        /// <param name="index">
        /// Indexer arguments.
        /// </param>
        /// <param name="value">
        /// A new indexer value.
        /// </param>
        void SetValue( object target, object[] index, object value );
    }

    #endregion

    #region Safe wrapper

#if NET_2_0
    /// <summary>
    /// Safe wrapper for the dynamic indexer.
    /// </summary>
    /// <remarks>
    /// <see cref="SafeIndexer"/> will attempt to use dynamic
    /// indexer if possible, but it will fall back to standard
    /// reflection if necessary.
    /// </remarks>    
    [Obsolete("Use SafeProperty instead", false)]
    public class SafeIndexer : IDynamicIndexer
    {
        private PropertyInfo indexerProperty;

        /// <summary>
        /// Internal PropertyInfo accessor.
        /// </summary>
        internal PropertyInfo IndexerProperty
        {
            get { return indexerProperty; }
        }

        private SafeProperty property;

        /// <summary>
        /// Creates a new instance of the safe indexer wrapper.
        /// </summary>
        /// <param name="indexerInfo">Indexer to wrap.</param>
        public SafeIndexer( PropertyInfo indexerInfo )
        {
            AssertUtils.ArgumentNotNull( indexerInfo, "You cannot create a dynamic indexer for a null value." );

            this.indexerProperty = indexerInfo;
            this.property = new SafeProperty( indexerInfo );
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
        public object GetValue( object target, int index )
        {
            return property.GetValue( target, index );
        }

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get the indexer value from.
        /// </param>
        /// <param name="index">
        /// Indexer argument.
        /// </param>
        /// <returns>
        /// A indexer value.
        /// </returns>
        public object GetValue( object target, object index )
        {
            return property.GetValue( target, index );
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
        public object GetValue( object target, object[] index )
        {
            return property.GetValue( target, index );
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
        public void SetValue( object target, int index, object value )
        {
            property.SetValue( target, value, index );
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
        public void SetValue( object target, object index, object value )
        {
            property.SetValue( target, value, index );
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
        public void SetValue( object target, object[] index, object value )
        {
            property.SetValue( target, value, index );
        }
    }

#else

    /// <summary>
    /// Safe wrapper for the dynamic indexer.
    /// </summary>
    /// <remarks>
    /// <see cref="SafeIndexer"/> will attempt to use dynamic
    /// indexer if possible, but it will fall back to standard
    /// reflection if necessary.
    /// </remarks>    
    public class SafeIndexer : IDynamicIndexer
    {
        private PropertyInfo indexerProperty;

        /// <summary>
        /// Internal PropertyInfo accessor.
        /// </summary>
        internal PropertyInfo IndexerProperty
        {
            get { return indexerProperty; }
        }

        private IDynamicIndexer dynamicIndexer;
        private bool isOptimizedGet = false;
        private bool isOptimizedSet = false;

        /// <summary>
        /// Creates a new instance of the safe indexer wrapper.
        /// </summary>
        /// <param name="indexer">Indexer to wrap.</param>
        public SafeIndexer(PropertyInfo indexer)
        {
            this.indexerProperty = indexer;

            if (indexer.CanRead 
                && indexer.GetGetMethod() != null 
                && ReflectionUtils.IsTypeVisible(indexer.DeclaringType, DynamicReflectionManager.ASSEMBLY_NAME))
            {
                dynamicIndexer = DynamicIndexer.Create(indexer);
                isOptimizedGet = true;
            }
            if (indexer.CanWrite 
                && indexer.GetSetMethod() != null
                && ReflectionUtils.IsTypeVisible(indexer.DeclaringType, DynamicReflectionManager.ASSEMBLY_NAME))
            {
                if (dynamicIndexer == null)
                {
                    dynamicIndexer = DynamicIndexer.Create(indexer);
                }
                isOptimizedSet = true;
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
        public object GetValue(object target, int index)
        {
            if (isOptimizedGet)
            {
                return dynamicIndexer.GetValue(target, index);
            }
            else
            {
                return indexerProperty.GetValue(target, new object[] { index });
            }
        }

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get the indexer value from.
        /// </param>
        /// <param name="index">
        /// Indexer argument.
        /// </param>
        /// <returns>
        /// A indexer value.
        /// </returns>
        public object GetValue(object target, object index)
        {
            if (isOptimizedGet)
            {
                return dynamicIndexer.GetValue(target, index);
            }
            else
            {
                return indexerProperty.GetValue(target, new object[] { index });
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
                return dynamicIndexer.GetValue(target, index);
            }
            else
            {
                return indexerProperty.GetValue(target, index);
            }
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
        public void SetValue(object target, int index, object value)
        {
            if (isOptimizedSet)
            {
                try
                {
                    dynamicIndexer.SetValue(target, index, value);
                }
                catch (InvalidCastException)
                {
                    isOptimizedSet = false;
                    indexerProperty.SetValue(target, value, new object[] { index });
                }
            }
            else
            {
                indexerProperty.SetValue(target, value, new object[] { index });
            }
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
        public void SetValue(object target, object index, object value)
        {
            if (isOptimizedSet)
            {
                try
                {
                    dynamicIndexer.SetValue(target, index, value);
                }
                catch (InvalidCastException)
                {
                    isOptimizedSet = false;
                    indexerProperty.SetValue(target, value, new object[] { index });
                }
            }
            else
            {
                indexerProperty.SetValue(target, value, new object[] { index });
            }
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
        public void SetValue(object target, object[] index, object value)
        {
            if (isOptimizedSet)
            {
                try
                {
                    dynamicIndexer.SetValue(target, index, value);
                }
                catch (InvalidCastException)
                {
                    isOptimizedSet = false;
                    indexerProperty.SetValue(target, value, index);
                }
            }
            else
            {
                indexerProperty.SetValue(target, value, index);
            }
        }
    }
#endif

    #endregion

#if NET_2_0
    /// <summary>
    /// Factory class for dynamic indexers.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Obsolete( "Use DynamicProperty instead", false )]
    public sealed class DynamicIndexer : BaseDynamicMember
    {
        /// <summary>
        /// Prevent instantiation
        /// </summary>
        private DynamicIndexer() { }

        /// <summary>
        /// Creates dynamic indexer instance for the specified <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="indexer">Indexer info to create dynamic indexer for.</param>
        /// <returns>Dynamic indexer for the specified <see cref="PropertyInfo"/>.</returns>
        public static IDynamicIndexer Create( PropertyInfo indexer )
        {
            AssertUtils.ArgumentNotNull( indexer, "You cannot create a dynamic indexer for a null value." );

            IDynamicIndexer dynamicIndexer = new SafeIndexer( indexer );
            return dynamicIndexer;
        }
    }
#else
    /// <summary>
    /// Factory class for dynamic indexers.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class DynamicIndexer : BaseDynamicMember
    {
        private static readonly CreateIndexerCallback s_createCallback = new CreateIndexerCallback(CreateInternal);

    #region Create Method

        /// <summary>
        /// Creates dynamic indexer instance for the specified <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="indexer">Indexer info to create dynamic indexer for.</param>
        /// <returns>Dynamic indexer for the specified <see cref="PropertyInfo"/>.</returns>
        public static IDynamicIndexer Create(PropertyInfo indexer)
        {
            AssertUtils.ArgumentNotNull(indexer, "You cannot create a dynamic indexer for a null value.");

            IDynamicIndexer dynamicIndexer = DynamicReflectionManager.GetDynamicIndexer(indexer, s_createCallback);
            return dynamicIndexer;
        }

        private static IDynamicIndexer CreateInternal(PropertyInfo indexer)
        {
            IDynamicIndexer dynamicIndexer = null;

            TypeBuilder tb = DynamicReflectionManager.CreateTypeBuilder("Indexer_" + indexer.DeclaringType.Name);
            tb.AddInterfaceImplementation(typeof(IDynamicIndexer));

            GenerateGetValue(tb, indexer, typeof(int));
            GenerateGetValue(tb, indexer, typeof(object));
            GenerateGetValue(tb, indexer, typeof(object[]));
            GenerateSetValue(tb, indexer, typeof(int));
            GenerateSetValue(tb, indexer, typeof(object));
            GenerateSetValue(tb, indexer, typeof(object[]));

            Type dynamicIndexerType = tb.CreateType();
            ConstructorInfo ctor = dynamicIndexerType.GetConstructor(Type.EmptyTypes);
            dynamicIndexer = (IDynamicIndexer) ctor.Invoke(ObjectUtils.EmptyObjects);
            
            return dynamicIndexer;
        }

        private static void GenerateGetValue(TypeBuilder tb, PropertyInfo indexer, Type indexType)
        {
            MethodBuilder getValueMethod =
                tb.DefineMethod("GetValue", METHOD_ATTRIBUTES, typeof(object), new Type[] { typeof(object), indexType });
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

                if (indexType != typeof(object) && indexType != typeof(object[]))
                {
                    il.Emit(OpCodes.Ldarg_2);
                }
                else if (indexType == typeof(object))
                {
                    Type argumentType = ReflectionUtils.GetParameterTypes(getMethod)[0];
                    SetupIndexerArgument(il, argumentType, 0, false);
                }
                else
                {
                    Type[] argTypes = ReflectionUtils.GetParameterTypes(getMethod);
                    for (int i = 0; i < argTypes.Length; i++)
                    {
                        SetupIndexerArgument(il, argTypes[i], i, true);
                    }
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

        private static void GenerateSetValue(TypeBuilder tb, PropertyInfo indexer, Type indexType)
        {
            MethodBuilder setValueMethod =
                tb.DefineMethod("SetValue", METHOD_ATTRIBUTES, typeof(void),
                                new Type[] { typeof(object), indexType, typeof(object) });
            setValueMethod.DefineParameter(1, ParameterAttributes.None, "target");
            setValueMethod.DefineParameter(2, ParameterAttributes.None, "value");

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

                    if (indexType != typeof(object) && indexType != typeof(object[]))
                    {
                        il.Emit(OpCodes.Ldarg_2);
                    }
                    else if (indexType == typeof(object))
                    {
                        Type argumentType = ReflectionUtils.GetParameterTypes(setMethod)[0];
                        SetupIndexerArgument(il, argumentType, 0, false);
                    }
                    else
                    {
                        Type[] argTypes = ReflectionUtils.GetParameterTypes(setMethod);
                        for (int i = 0; i < argTypes.Length - 1; i++)
                        {
                            SetupIndexerArgument(il, argTypes[i], i, true);
                        }
                    }
                    SetupArgument(il, indexer.PropertyType, 3);
                    InvokeMethod(il, isStatic, isValueType, setMethod);
                    il.Emit(OpCodes.Ret);
                }
            }
            else
            {
                ThrowInvalidOperationException(il, "Cannot set value of a read-only indexer");
            }
        }

        private static void SetupIndexerArgument(ILGenerator il, Type argumentType, int argumentPosition, bool isObjectArray)
        {
            il.Emit(OpCodes.Ldarg_2);
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

} // namespace