#region Licence

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

using System.Globalization;

using Spring.Util;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Provides methods for type-safe accessing <see cref="IVariableSource"/>s.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class VariableAccessor
    {
        private readonly IVariableSource variableSource;

        /// <summary>
        /// Initialize a new instance of an <see cref="VariableAccessor"/>
        /// </summary>
        /// <param name="variableSource">The underlying <see cref="IVariableSource"/> to read values from.</param>
        public VariableAccessor(IVariableSource variableSource)
        {
            this.variableSource = variableSource;
        }

        /// <summary>
        /// Returns a <see cref="float"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="float"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public float GetFloat(string name, float defaultValue)
        {
            return GetFloat(name, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="float"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="float"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public float GetFloat(string name, float defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                return float.Parse(text);
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="double"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="double"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public double GetDouble(string name, double defaultValue)
        {
            return GetDouble(name, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="double"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="double"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public double GetDouble(string name, double defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                return double.Parse(text);
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="decimal"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="decimal"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public decimal GetDecimal(string name, decimal defaultValue)
        {
            return GetDecimal(name, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="decimal"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="decimal"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public decimal GetDecimal(string name, decimal defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                return decimal.Parse(text);
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="long"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="long"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public long GetInt64(string name, long defaultValue)
        {
            return GetInt64(name, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="long"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="long"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public long GetInt64(string name, long defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                return Int64.Parse(text);
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="ulong"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="ulong"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public ulong GetUInt64(string name, ulong defaultValue)
        {
            return GetUInt64(name, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="ulong"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="ulong"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public ulong GetUInt64(string name, ulong defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                return UInt64.Parse(text);
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="int"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="int"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public int GetInt32(string name, int defaultValue)
        {
            return GetInt32(name, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="int"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="int"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public int GetInt32(string name, int defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                return Int32.Parse(text);
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="uint"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="uint"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public uint GetUInt32(string name, uint defaultValue)
        {
            return GetUInt32(name, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="uint"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="uint"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public uint GetUInt32(string name, uint defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                return UInt32.Parse(text);
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="short"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="short"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public short GetInt16(string name, short defaultValue)
        {
            return GetInt16(name, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="short"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="short"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public short GetInt16(string name, short defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                return Int16.Parse(text);
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="short"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="short"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public ushort GetUInt16(string name, ushort defaultValue)
        {
            return GetUInt16(name, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="short"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="short"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public ushort GetUInt16(string name, ushort defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                return UInt16.Parse(text);
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="byte"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="byte"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public byte GetByte(string name, byte defaultValue)
        {
            return GetByte(name, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="byte"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="byte"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public byte GetByte(string name, byte defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                return byte.Parse(text);
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="Guid"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="Guid"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public Guid GetGuid(string name, Guid defaultValue)
        {
            return GetGuid(name, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="Guid"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="Guid"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public Guid GetGuid(string name, Guid defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                return new Guid(text);
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="DateTime"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="format">The expected format of the variable's value</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="DateTime"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public DateTime GetDateTime(string name, string format, DateTime defaultValue)
        {
            return GetDateTime(name, format, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="DateTime"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="format">The expected format of the variable's value</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="DateTime"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public DateTime GetDateTime(string name, string format, DateTime defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                if (format == null)
                {
                    return DateTime.Parse(text);
                }
                else
                {
                    return DateTime.ParseExact(text, format, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="char"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="char"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public char GetChar(string name, char defaultValue)
        {
            return GetChar(name, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="char"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="char"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public char GetChar(string name, char defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                if (text.Length != 1)
                {
                    throw new ArgumentException("string '{0}' can't be converted to char", text);
                }
                return text[0];
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="bool"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// A <see cref="bool"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public bool GetBoolean(string name, bool defaultValue)
        {
            return GetBoolean(name, defaultValue, true);
        }

        /// <summary>
        /// Returns a <see cref="bool"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// A <see cref="bool"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public bool GetBoolean(string name, bool defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                string text = GetString(name, defaultValue.ToString());
                return bool.Parse(text);
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns an <see cref="Enum"/> of <paramref name="defaultValue"/>'s type that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <returns>
        /// An <see cref="Enum"/> of <paramref name="defaultValue"/>'s type that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public Enum GetEnum(string name, Enum defaultValue)
        {
            return GetEnum(name, defaultValue, true);
        }

        /// <summary>
        /// Returns an <see cref="Enum"/> of <paramref name="defaultValue"/>'s type that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.</param>
        /// <param name="throwOnInvalidValue">
        /// If <c>false</c>, suppresses exceptions if the result
        /// of <see cref="IVariableSource.ResolveVariable"/> cannot be parsed
        /// and returns <paramref name="defaultValue"/> instead.</param>
        /// <returns>
        /// An <see cref="Enum"/> of <paramref name="defaultValue"/>'s type that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> cannot be parsed.
        /// </returns>
        public Enum GetEnum(string name, Enum defaultValue, bool throwOnInvalidValue)
        {
            try
            {
                return (Enum)Enum.Parse(defaultValue.GetType(), GetString(name, defaultValue.ToString()), true);
            }
            catch
            {
                if (throwOnInvalidValue)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that contains the value of the specified variable.
        /// </summary>
        /// <param name="name">The name of the variable to be read.</param>
        /// <param name="defaultValue">The value to be returned if <see cref="IVariableSource.ResolveVariable"/> returns <see lang="null"/> or <see cref="String.Empty"/>.</param>
        /// <returns>
        /// A <see cref="string"/> that contains the value of the specified variable
        /// or <paramref name="defaultValue"/>, if <see cref="IVariableSource.ResolveVariable"/> returns <c>null</c>.
        /// </returns>
        public string GetString(string name, string defaultValue)
        {
            string value = null;
            if (variableSource != null && variableSource.CanResolveVariable(name))
            {
                value = variableSource.ResolveVariable(name);
            }

            if (!StringUtils.HasLength(value))
            {
                return defaultValue;
            }

            return value;
        }
    }
}
