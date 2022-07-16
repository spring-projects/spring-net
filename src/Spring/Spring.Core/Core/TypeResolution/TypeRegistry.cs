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

using Spring.Util;

#endregion

namespace Spring.Core.TypeResolution
{
    /// <summary>
    /// Provides access to a central registry of aliased <see cref="System.Type"/>s.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Simplifies configuration by allowing aliases to be used instead of
    /// fully qualified type names.
    /// </p>
    /// <p>
    /// Comes 'pre-loaded' with a number of convenience alias' for the more
    /// common types; an example would be the '<c>int</c>' (or '<c>Integer</c>'
    /// for Visual Basic.NET developers) alias for the <see cref="System.Int32"/>
    /// type.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <seealso cref="Spring.Context.Support.TypeAliasesSectionHandler"/>
    public sealed class TypeRegistry
    {
        #region Constants

        /// <summary>
        /// Name of the .Net config section that contains Spring.Net type aliases.
        /// </summary>
        private const string TypeAliasesSectionName = "spring/typeAliases";

        /// <summary>
        /// The alias around the 'int' type.
        /// </summary>
        public const string Int32Alias = "int";

        /// <summary>
        /// The alias around the 'Integer' type (Visual Basic.NET style).
        /// </summary>
        public const string Int32AliasVB = "Integer";

        /// <summary>
        /// The alias around the 'int[]' array type.
        /// </summary>
        public const string Int32ArrayAlias = "int[]";

        /// <summary>
        /// The alias around the 'Integer()' array type (Visual Basic.NET style).
        /// </summary>
        public const string Int32ArrayAliasVB = "Integer()";

        /// <summary>
        /// The alias around the 'decimal' type.
        /// </summary>
        public const string DecimalAlias = "decimal";

        /// <summary>
        /// The alias around the 'Decimal' type (Visual Basic.NET style).
        /// </summary>
        public const string DecimalAliasVB = "Decimal";

        /// <summary>
        /// The alias around the 'decimal[]' array type.
        /// </summary>
        public const string DecimalArrayAlias = "decimal[]";

        /// <summary>
        /// The alias around the 'Decimal()' array type (Visual Basic.NET style).
        /// </summary>
        public const string DecimalArrayAliasVB = "Decimal()";

        /// <summary>
        /// The alias around the 'char' type.
        /// </summary>
        public const string CharAlias = "char";

        /// <summary>
        /// The alias around the 'Char' type (Visual Basic.NET style).
        /// </summary>
        public const string CharAliasVB = "Char";

        /// <summary>
        /// The alias around the 'char[]' array type.
        /// </summary>
        public const string CharArrayAlias = "char[]";

        /// <summary>
        /// The alias around the 'Char()' array type (Visual Basic.NET style).
        /// </summary>
        public const string CharArrayAliasVB = "Char()";

        /// <summary>
        /// The alias around the 'long' type.
        /// </summary>
        public const string Int64Alias = "long";

        /// <summary>
        /// The alias around the 'Long' type (Visual Basic.NET style).
        /// </summary>
        public const string Int64AliasVB = "Long";

        /// <summary>
        /// The alias around the 'long[]' array type.
        /// </summary>
        public const string Int64ArrayAlias = "long[]";

        /// <summary>
        /// The alias around the 'Long()' array type (Visual Basic.NET style).
        /// </summary>
        public const string Int64ArrayAliasVB = "Long()";

        /// <summary>
        /// The alias around the 'short' type.
        /// </summary>
        public const string Int16Alias = "short";

        /// <summary>
        /// The alias around the 'Short' type (Visual Basic.NET style).
        /// </summary>
        public const string Int16AliasVB = "Short";

        /// <summary>
        /// The alias around the 'short[]' array type.
        /// </summary>
        public const string Int16ArrayAlias = "short[]";

        /// <summary>
        /// The alias around the 'Short()' array type (Visual Basic.NET style).
        /// </summary>
        public const string Int16ArrayAliasVB = "Short()";

        /// <summary>
        /// The alias around the 'unsigned int' type.
        /// </summary>
        public const string UInt32Alias = "uint";

        /// <summary>
        /// The alias around the 'unsigned long' type.
        /// </summary>
        public const string UInt64Alias = "ulong";

        /// <summary>
        /// The alias around the 'ulong[]' array type.
        /// </summary>
        public const string UInt64ArrayAlias = "ulong[]";

        /// <summary>
        /// The alias around the 'uint[]' array type.
        /// </summary>
        public const string UInt32ArrayAlias = "uint[]";

        /// <summary>
        /// The alias around the 'unsigned short' type.
        /// </summary>
        public const string UInt16Alias = "ushort";

        /// <summary>
        /// The alias around the 'ushort[]' array type.
        /// </summary>
        public const string UInt16ArrayAlias = "ushort[]";

        /// <summary>
        /// The alias around the 'double' type.
        /// </summary>
        public const string DoubleAlias = "double";

        /// <summary>
        /// The alias around the 'Double' type (Visual Basic.NET style).
        /// </summary>
        public const string DoubleAliasVB = "Double";

        /// <summary>
        /// The alias around the 'double[]' array type.
        /// </summary>
        public const string DoubleArrayAlias = "double[]";

        /// <summary>
        /// The alias around the 'Double()' array type (Visual Basic.NET style).
        /// </summary>
        public const string DoubleArrayAliasVB = "Double()";

        /// <summary>
        /// The alias around the 'float' type.
        /// </summary>
        public const string FloatAlias = "float";

        /// <summary>
        /// The alias around the 'Single' type (Visual Basic.NET style).
        /// </summary>
        public const string SingleAlias = "Single";

        /// <summary>
        /// The alias around the 'float[]' array type.
        /// </summary>
        public const string FloatArrayAlias = "float[]";

        /// <summary>
        /// The alias around the 'Single()' array type (Visual Basic.NET style).
        /// </summary>
        public const string SingleArrayAliasVB = "Single()";

        /// <summary>
        /// The alias around the 'DateTime' type.
        /// </summary>
        public const string DateTimeAlias = "DateTime";

        /// <summary>
        /// The alias around the 'DateTime' type (C# style).
        /// </summary>
        public const string DateAlias = "date";

        /// <summary>
        /// The alias around the 'DateTime' type (Visual Basic.NET style).
        /// </summary>
        public const string DateAliasVB = "Date";

        /// <summary>
        /// The alias around the 'DateTime[]' array type.
        /// </summary>
        public const string DateTimeArrayAlias = "DateTime[]";

        /// <summary>
        /// The alias around the 'DateTime[]' array type.
        /// </summary>
        public const string DateTimeArrayAliasCSharp = "date[]";

        /// <summary>
        /// The alias around the 'DateTime()' array type (Visual Basic.NET style).
        /// </summary>
        public const string DateTimeArrayAliasVB = "DateTime()";

        /// <summary>
        /// The alias around the 'bool' type.
        /// </summary>
        public const string BoolAlias = "bool";

        /// <summary>
        /// The alias around the 'Boolean' type (Visual Basic.NET style).
        /// </summary>
        public const string BoolAliasVB = "Boolean";

        /// <summary>
        /// The alias around the 'bool[]' array type.
        /// </summary>
        public const string BoolArrayAlias = "bool[]";

        /// <summary>
        /// The alias around the 'Boolean()' array type (Visual Basic.NET style).
        /// </summary>
        public const string BoolArrayAliasVB = "Boolean()";

        /// <summary>
        /// The alias around the 'string' type.
        /// </summary>
        public const string StringAlias = "string";

        /// <summary>
        /// The alias around the 'string' type (Visual Basic.NET style).
        /// </summary>
        public const string StringAliasVB = "String";

        /// <summary>
        /// The alias around the 'string[]' array type.
        /// </summary>
        public const string StringArrayAlias = "string[]";

        /// <summary>
        /// The alias around the 'string[]' array type (Visual Basic.NET style).
        /// </summary>
        public const string StringArrayAliasVB = "String()";

        /// <summary>
        /// The alias around the 'object' type.
        /// </summary>
        public const string ObjectAlias = "object";

        /// <summary>
        /// The alias around the 'object' type (Visual Basic.NET style).
        /// </summary>
        public const string ObjectAliasVB = "Object";

        /// <summary>
        /// The alias around the 'object[]' array type.
        /// </summary>
        public const string ObjectArrayAlias = "object[]";

        /// <summary>
        /// The alias around the 'object[]' array type (Visual Basic.NET style).
        /// </summary>
        public const string ObjectArrayAliasVB = "Object()";

        /// <summary>
        /// The alias around the 'int?' type.
        /// </summary>
        public const string NullableInt32Alias = "int?";

        /// <summary>
        /// The alias around the 'int?[]' array type.
        /// </summary>
        public const string NullableInt32ArrayAlias = "int?[]";

        /// <summary>
        /// The alias around the 'decimal?' type.
        /// </summary>
        public const string NullableDecimalAlias = "decimal?";

        /// <summary>
        /// The alias around the 'decimal?[]' array type.
        /// </summary>
        public const string NullableDecimalArrayAlias = "decimal?[]";

        /// <summary>
        /// The alias around the 'char?' type.
        /// </summary>
        public const string NullableCharAlias = "char?";

        /// <summary>
        /// The alias around the 'char?[]' array type.
        /// </summary>
        public const string NullableCharArrayAlias = "char?[]";

        /// <summary>
        /// The alias around the 'long?' type.
        /// </summary>
        public const string NullableInt64Alias = "long?";

        /// <summary>
        /// The alias around the 'long?[]' array type.
        /// </summary>
        public const string NullableInt64ArrayAlias = "long?[]";

        /// <summary>
        /// The alias around the 'short?' type.
        /// </summary>
        public const string NullableInt16Alias = "short?";

        /// <summary>
        /// The alias around the 'short?[]' array type.
        /// </summary>
        public const string NullableInt16ArrayAlias = "short?[]";

        /// <summary>
        /// The alias around the 'unsigned int?' type.
        /// </summary>
        public const string NullableUInt32Alias = "uint?";

        /// <summary>
        /// The alias around the 'unsigned long?' type.
        /// </summary>
        public const string NullableUInt64Alias = "ulong?";

        /// <summary>
        /// The alias around the 'ulong?[]' array type.
        /// </summary>
        public const string NullableUInt64ArrayAlias = "ulong?[]";

        /// <summary>
        /// The alias around the 'uint?[]' array type.
        /// </summary>
        public const string NullableUInt32ArrayAlias = "uint?[]";

        /// <summary>
        /// The alias around the 'unsigned short?' type.
        /// </summary>
        public const string NullableUInt16Alias = "ushort?";

        /// <summary>
        /// The alias around the 'ushort?[]' array type.
        /// </summary>
        public const string NullableUInt16ArrayAlias = "ushort?[]";

        /// <summary>
        /// The alias around the 'double?' type.
        /// </summary>
        public const string NullableDoubleAlias = "double?";

        /// <summary>
        /// The alias around the 'double?[]' array type.
        /// </summary>
        public const string NullableDoubleArrayAlias = "double?[]";

        /// <summary>
        /// The alias around the 'float?' type.
        /// </summary>
        public const string NullableFloatAlias = "float?";

        /// <summary>
        /// The alias around the 'float?[]' array type.
        /// </summary>
        public const string NullableFloatArrayAlias = "float?[]";

        /// <summary>
        /// The alias around the 'bool?' type.
        /// </summary>
        public const string NullableBoolAlias = "bool?";

        /// <summary>
        /// The alias around the 'bool?[]' array type.
        /// </summary>
        public const string NullableBoolArrayAlias = "bool?[]";

        #endregion

        #region Fields

        private static readonly object syncRoot = new object();
        private static IDictionary<string, Type> types = new Dictionary<string, Type>();

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Registers standard and user-configured type aliases.
        /// </summary>
        static TypeRegistry()
        {
            lock (syncRoot)
            {
                types["Int32"] = typeof(Int32);
                types[Int32Alias] = typeof(Int32);
                types[Int32AliasVB] = typeof(Int32);
                types[Int32ArrayAlias] = typeof(Int32[]);
                types[Int32ArrayAliasVB] = typeof(Int32[]);

                types["UInt32"] = typeof(UInt32);
                types[UInt32Alias] = typeof(UInt32);
                types[UInt32ArrayAlias] = typeof(UInt32[]);

                types["Int16"] = typeof(Int16);
                types[Int16Alias] = typeof(Int16);
                types[Int16AliasVB] = typeof(Int16);
                types[Int16ArrayAlias] = typeof(Int16[]);
                types[Int16ArrayAliasVB] = typeof(Int16[]);

                types["UInt16"] = typeof(UInt16);
                types[UInt16Alias] = typeof(UInt16);
                types[UInt16ArrayAlias] = typeof(UInt16[]);

                types["Int64"] = typeof(Int64);
                types[Int64Alias] = typeof(Int64);
                types[Int64AliasVB] = typeof(Int64);
                types[Int64ArrayAlias] = typeof(Int64[]);
                types[Int64ArrayAliasVB] = typeof(Int64[]);

                types["UInt64"] = typeof(UInt64);
                types[UInt64Alias] = typeof(UInt64);
                types[UInt64ArrayAlias] = typeof(UInt64[]);

                types[DoubleAlias] = typeof(double);
                types[DoubleAliasVB] = typeof(double);
                types[DoubleArrayAlias] = typeof(double[]);
                types[DoubleArrayAliasVB] = typeof(double[]);

                types[FloatAlias] = typeof(float);
                types[SingleAlias] = typeof(float);
                types[FloatArrayAlias] = typeof(float[]);
                types[SingleArrayAliasVB] = typeof(float[]);

                types[DateTimeAlias] = typeof(DateTime);
                types[DateAlias] = typeof(DateTime);
                types[DateAliasVB] = typeof(DateTime);
                types[DateTimeArrayAlias] = typeof(DateTime[]);
                types[DateTimeArrayAliasCSharp] = typeof(DateTime[]);
                types[DateTimeArrayAliasVB] = typeof(DateTime[]);

                types[BoolAlias] = typeof(bool);
                types[BoolAliasVB] = typeof(bool);
                types[BoolArrayAlias] = typeof(bool[]);
                types[BoolArrayAliasVB] = typeof(bool[]);

                types[DecimalAlias] = typeof(decimal);
                types[DecimalAliasVB] = typeof(decimal);
                types[DecimalArrayAlias] = typeof(decimal[]);
                types[DecimalArrayAliasVB] = typeof(decimal[]);

                types[CharAlias] = typeof(char);
                types[CharAliasVB] = typeof(char);
                types[CharArrayAlias] = typeof(char[]);
                types[CharArrayAliasVB] = typeof(char[]);

                types[StringAlias] = typeof(string);
                types[StringAliasVB] = typeof(string);
                types[StringArrayAlias] = typeof(string[]);
                types[StringArrayAliasVB] = typeof(string[]);

                types[ObjectAlias] = typeof(object);
                types[ObjectAliasVB] = typeof(object);
                types[ObjectArrayAlias] = typeof(object[]);
                types[ObjectArrayAliasVB] = typeof(object[]);

                types[NullableInt32Alias] = typeof(int?);
                types[NullableInt32ArrayAlias] = typeof(int?[]);

                types[NullableDecimalAlias] = typeof(decimal?);
                types[NullableDecimalArrayAlias] = typeof(decimal?[]);

                types[NullableCharAlias] = typeof(char?);
                types[NullableCharArrayAlias] = typeof(char?[]);

                types[NullableInt64Alias] = typeof(long?);
                types[NullableInt64ArrayAlias] = typeof(long?[]);

                types[NullableInt16Alias] = typeof(short?);
                types[NullableInt16ArrayAlias] = typeof(short?[]);

                types[NullableUInt32Alias] = typeof(uint?);
                types[NullableUInt32ArrayAlias] = typeof(uint?[]);

                types[NullableUInt64Alias] = typeof(ulong?);
                types[NullableUInt64ArrayAlias] = typeof(ulong?[]);

                types[NullableUInt16Alias] = typeof(ushort?);
                types[NullableUInt16ArrayAlias] = typeof(ushort?[]);

                types[NullableDoubleAlias] = typeof(double?);
                types[NullableDoubleArrayAlias] = typeof(double?[]);

                types[NullableFloatAlias] = typeof(float?);
                types[NullableFloatArrayAlias] = typeof(float?[]);

                types[NullableBoolAlias] = typeof(bool?);
                types[NullableBoolArrayAlias] = typeof(bool?[]);

                // register user-configured type aliases
                ConfigurationUtils.GetSection(TypeAliasesSectionName);
            }
        }

        #endregion

        /// <summary>
        /// Registers an alias for the specified <see cref="System.Type"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This overload does eager resolution of the <see cref="System.Type"/>
        /// referred to by the <paramref name="typeName"/> parameter. It will throw a
        /// <see cref="System.TypeLoadException"/> if the <see cref="System.Type"/> referred
        /// to by the <paramref name="typeName"/> parameter cannot be resolved.
        /// </p>
        /// </remarks>
        /// <param name="alias">
        /// A string that will be used as an alias for the specified
        /// <see cref="System.Type"/>.
        /// </param>
        /// <param name="typeName">
        /// The (possibly partially assembly qualified) name of the
        /// <see cref="System.Type"/> to register the alias for.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If either of the supplied parameters is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        /// <exception cref="System.TypeLoadException">
        /// If the <see cref="System.Type"/> referred to by the supplied
        /// <paramref name="typeName"/> cannot be loaded.
        /// </exception>
        public static void RegisterType(string alias, string typeName)
        {
            AssertUtils.ArgumentHasText(alias, "alias");
            AssertUtils.ArgumentHasText(typeName, "typeName");

            Type type = TypeResolutionUtils.ResolveType(typeName);

            if (type.IsGenericTypeDefinition)
                alias += ("`" + type.GetGenericArguments().Length);

            RegisterType(alias, type);
        }

        /// <summary>
        /// Registers short type name as an alias for
        /// the supplied <see cref="System.Type"/>.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> to register.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public static void RegisterType(Type type)
        {
            AssertUtils.ArgumentNotNull(type, "type");

            lock (syncRoot)
            {
                types[type.Name] = type;
            }
        }

        /// <summary>
        /// Registers an alias for the supplied <see cref="System.Type"/>.
        /// </summary>
        /// <param name="alias">
        /// The alias for the supplied <see cref="System.Type"/>.
        /// </param>
        /// <param name="type">
        /// The <see cref="System.Type"/> to register the supplied <paramref name="alias"/> under.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="type"/> is <see langword="null"/>; or if
        /// the supplied <paramref name="alias"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        public static void RegisterType(string alias, Type type)
        {
            AssertUtils.ArgumentHasText(alias, "alias");
            AssertUtils.ArgumentNotNull(type, "type");

            lock (syncRoot)
            {
                types[alias] = type;
            }
        }

        /// <summary>
        /// Resolves the supplied <paramref name="alias"/> to a <see cref="System.Type"/>.
        /// </summary>
        /// <param name="alias">
        /// The alias to resolve.
        /// </param>
        /// <returns>
        /// The <see cref="System.Type"/> the supplied <paramref name="alias"/> was
        /// associated with, or <see lang="null"/> if no <see cref="System.Type"/>
        /// was previously registered for the supplied <paramref name="alias"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="alias"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        public static Type ResolveType(string alias)
        {
            AssertUtils.ArgumentHasText(alias, "alias");
            Type type;
            types.TryGetValue(alias, out type);
            return type;
        }

        /// <summary>
        /// Returns a flag specifying whether <b>TypeRegistry</b> contains
        /// specified alias or not.
        /// </summary>
        /// <param name="alias">
        /// Alias to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified type alias is registered,
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool ContainsAlias(string alias)
        {
            return types.ContainsKey(alias);
        }
    }
}
