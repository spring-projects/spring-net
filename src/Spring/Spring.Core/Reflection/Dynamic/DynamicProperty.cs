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

using System.Reflection;

using Common.Logging;

using Spring.Util;

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

        #region Cache

        private static readonly IDictionary<PropertyInfo, DynamicPropertyCacheEntry> propertyCache = new Dictionary<PropertyInfo, DynamicPropertyCacheEntry>();

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
            DynamicPropertyCacheEntry propertyInfo;
            if (!propertyCache.TryGetValue(property, out propertyInfo))
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

        /// <summary>
        /// Internal PropertyInfo accessor.
        /// </summary>
        internal PropertyInfo PropertyInfo
        {
            get { return propertyInfo; }
        }
    }

    #endregion

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
}
