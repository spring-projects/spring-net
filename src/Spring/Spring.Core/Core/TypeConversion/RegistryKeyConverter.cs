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

#region Imports

using System.ComponentModel;
using System.Globalization;
using Microsoft.Win32;

using Spring.Util;

#endregion

namespace Spring.Core.TypeConversion
{
    /// <summary>
    /// Converts string representation of the registry key
    /// into <see cref="RegistryKey"/> instance.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class RegistryKeyConverter : TypeConverter
    {
        /// <summary>
        /// Can we convert from a the sourcetype to a <see cref="RegistryKey"/>?
        /// </summary>
        /// <remarks>
        /// <p>
        /// Currently only supports conversion from a <see cref="System.String"/> instance.
        /// </p>
        /// </remarks>
        /// <param name="context">
        /// A <see cref="System.ComponentModel.ITypeDescriptorContext"/>
        /// that provides a format context.
        /// </param>
        /// <param name="sourceType">
        /// A <see cref="System.Type"/> that represents the
        /// <see cref="System.Type"/> you want to convert from.
        /// </param>
        /// <returns><see langword="true"/> if the conversion is possible.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        /// <summary>
        /// Convert from a <see cref="System.String"/> value to an
        /// <see cref="RegistryKey"/> instance.
        /// </summary>
        /// <param name="context">
        /// A <see cref="System.ComponentModel.ITypeDescriptorContext"/>
        /// that provides a format context.
        /// </param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> to use
        /// as the current culture.
        /// </param>
        /// <param name="value">
        /// The value that is to be converted.
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/> array if successful.
        /// </returns>        
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            AssertUtils.ArgumentNotNull(value, "value");

            if (value is string)
            {
                string keyName = (string) value;
                AssertUtils.ArgumentHasText(keyName, "value");

                string[] keys = StringUtils.Split(keyName, "\\", false, true);
                RegistryKey key = GetRootKey(keys[0]);
                for (int i = 1; i < keys.Length; i++)
                {
                    // open all sub-keys but the last one as read-only
                    key = key.OpenSubKey(keys[i], (i == keys.Length - 1));
                    if (key == null)
                    {
                        throw new ArgumentException("Registry key [" + GetPartialKeyName(keys, i) + "] does not exist.");
                    }
                }

                return key;
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Generates partial registry key name.
        /// </summary>
        /// <param name="keys">
        /// Key elements.
        /// </param>
        /// <param name="index">
        /// Index of the last element to use.
        /// </param>
        /// <returns>
        /// Friendly key name containing key element from 
        /// 0 to <paramref name="index"/>, inclusive.
        /// </returns>
        private static string GetPartialKeyName(string[] keys, int index)
        {
            string keyName = "";
            for (int i = 0; i <= index; i++)
            {
                keyName += (keys[i] + (i < index ? "\\" : ""));
            }
            return keyName;
        }

        /// <summary>
        /// Returns <see cref="RegistryKey"/> for the specified
        /// root hive name.
        /// </summary>
        /// <param name="name">
        /// Root hive name.
        /// </param>
        /// <returns>
        /// Registry key for the specified name.
        /// </returns>
        private static RegistryKey GetRootKey(string name)
        {
            switch (name)
            {
                case "HKEY_CURRENT_USER":
                    return Registry.CurrentUser;
                case "HKEY_LOCAL_MACHINE":
                    return Registry.LocalMachine;
                case "HKEY_CLASSES_ROOT":
                    return Registry.ClassesRoot;
                case "HKEY_USERS":
                    return Registry.Users;
                case "HKEY_PERFORMANCE_DATA":
                    return Registry.PerformanceData;
                case "HKEY_CURRENT_CONFIG":
                    return Registry.CurrentConfig;
                default:
                    throw new ArgumentException("Invalid root hive name [" + name + "].");
            }
        }
    }
}
