#region License

/* 
 * Copyright © 2002-2011 the original author or authors. 
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
using NUnit.Framework;
using Spring.Objects;
using Spring.Objects.Factory;

using IBar=Spring.Objects.Factory.IBar;
using Spring.Context;
using Spring.Context.Support;

#endregion

namespace Spring.Core.TypeResolution
{
	/// <summary>
	/// Unit tests for the TypeRegistry class.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class TypeRegistryTests
	{
		[Test]
		public void TestAliasResolution()
		{
			TypeRegistry.RegisterType("Foo", typeof (Foo));
			TypeRegistry.RegisterType("Bar", "Spring.Objects.Factory.Bar, Spring.Core.Tests");

			Assert.AreEqual(TypeRegistry.ResolveType("Foo"), typeof (Foo));
			Assert.AreEqual(TypeRegistry.ResolveType("Bar"), typeof (Bar));

			IApplicationContext ctx =
                new XmlApplicationContext("assembly://Spring.Core.Tests/Spring.Core.TypeResolution/aliasedObjects.xml");

			Foo foo = ctx.GetObject("aliasedType") as Foo;
			Assert.IsNotNull(foo);
			Assert.IsNotNull(foo.Bar);
			Assert.AreEqual(foo.Bar, typeof (Bar));
			Assert.IsTrue(typeof (IBar).IsAssignableFrom(foo.Bar));
		}

		[Test]
		public void ResolveTypeWithNullAliasArg()
		{
            Assert.Throws<ArgumentNullException>(() => TypeRegistry.ResolveType(null));
		}

		[Test]
		public void ResolveTypeWithEmptyAliasArg()
		{
            Assert.Throws<ArgumentNullException>(() => TypeRegistry.ResolveType(string.Empty));
		}

		[Test]
		public void ResolveTypeWithWhitespacedAliasArg()
		{
            Assert.Throws<ArgumentNullException>(() => TypeRegistry.ResolveType("   "));
		}

		[Test]
		public void RegisterTypeWithNullAliasArg()
		{
            Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType(null, typeof (TestObject)));
		}

		[Test]
		public void RegisterTypeWithEmptyAliasArg()
		{
            Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType(string.Empty, typeof (TestObject)));
		}

		[Test]
		public void RegisterTypeWithWhitespacedAliasArg()
		{
            Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType("   ", typeof (TestObject)));
		}

		[Test]
		public void RegisterTypeWithNullTypeArg()
		{
            Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType("foo", (Type) null));
		}

        [Test]
		public void RegisterTypeWithNullTypeStringArg()
		{
            Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType("foo", (string) null));
		}

		[Test]
		public void RegisterTypeWithEmptyTypeStringArg()
		{
            Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType("foo", string.Empty));
		}

		[Test]
		public void RegisterTypeWithWhitespacedTypeStringArg()
		{
            Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType("foo", "   "));
		}

		[Test]
		public void ReturnsNullIfNoTypeAliasRegistered()
		{
			Type type = TypeRegistry.ResolveType("panko");
			Assert.IsNull(type, "Must return null if no Type is registered under the supplied alias.");
		}

		[Test]
		public void RegisteringAnAliasTwiceDoesNotThrowException()
		{
			const string Alias = "foo";

			TypeRegistry.RegisterType(Alias, typeof (TestObject));
			TypeRegistry.RegisterType(Alias, GetType());

			Type type = TypeRegistry.ResolveType(Alias);
			Assert.AreEqual(GetType(), type, "Overriding Type was not registered.");
		}

		[Test]
		public void ResolveIntegerByName()
		{
			Assert.AreEqual(typeof (int),
			                TypeRegistry.ResolveType("int"));
		}

		[Test]
		public void ResolveChar()
		{
			Assert.AreEqual(typeof (char),
			                TypeRegistry.ResolveType(TypeRegistry.CharAlias));
		}

		[Test]
		public void ResolveInteger()
		{
			Assert.AreEqual(typeof (int),
			                TypeRegistry.ResolveType(TypeRegistry.Int32Alias));
		}

		[Test]
		public void ResolveDecimal()
		{
			Assert.AreEqual(typeof (decimal),
			                TypeRegistry.ResolveType(TypeRegistry.DecimalAlias));
		}

		[Test]
		public void ResolveUnsignedIntegerByName()
		{
			Assert.AreEqual(typeof (uint),
			                TypeRegistry.ResolveType("uint"));
		}

		[Test]
		public void ResolveUnsignedInteger()
		{
			Assert.AreEqual(typeof (uint),
			                TypeRegistry.ResolveType(TypeRegistry.UInt32Alias));
		}

		[Test]
		public void ResolveFloatByName()
		{
			Assert.AreEqual(typeof (float),
			                TypeRegistry.ResolveType("float"));
		}

		[Test]
		public void ResolveFloat()
		{
			Assert.AreEqual(typeof (float),
			                TypeRegistry.ResolveType(TypeRegistry.FloatAlias));
		}

		[Test]
		public void ResolveDoubleByName()
		{
			Assert.AreEqual(typeof (double),
			                TypeRegistry.ResolveType("double"));
		}

		[Test]
		public void ResolveDouble()
		{
			Assert.AreEqual(typeof (double),
			                TypeRegistry.ResolveType(TypeRegistry.DoubleAlias));
		}

		[Test]
		public void ResolveLongByName()
		{
			Assert.AreEqual(typeof (long),
			                TypeRegistry.ResolveType("long"));
		}

		[Test]
		public void ResolveLong()
		{
			Assert.AreEqual(typeof (long),
			                TypeRegistry.ResolveType(TypeRegistry.Int64Alias));
		}

		[Test]
		public void ResolveUnsignedLongByName()
		{
			Assert.AreEqual(typeof (ulong),
			                TypeRegistry.ResolveType("ulong"));
		}

		[Test]
		public void ResolveUnsignedLong()
		{
			Assert.AreEqual(typeof (ulong),
			                TypeRegistry.ResolveType(TypeRegistry.UInt64Alias));
		}

		[Test]
		public void ResolveShortByName()
		{
			Assert.AreEqual(typeof (short),
			                TypeRegistry.ResolveType("short"));
		}

		[Test]
		public void ResolveShort()
		{
			Assert.AreEqual(typeof (short),
			                TypeRegistry.ResolveType(TypeRegistry.Int16Alias));
		}

		[Test]
		public void ResolveUnsignedShortByName()
		{
			Assert.AreEqual(typeof (ushort),
			                TypeRegistry.ResolveType("ushort"));
		}

		[Test]
		public void ResolveUnsignedShort()
		{
			Assert.AreEqual(typeof (ushort),
			                TypeRegistry.ResolveType(TypeRegistry.UInt16Alias));
		}

		[Test]
		public void ResolveDate()
		{
			Assert.AreEqual(typeof (DateTime),
			                TypeRegistry.ResolveType(TypeRegistry.DateAlias));
		}

		[Test]
		public void ResolveBool()
		{
			Assert.AreEqual(typeof (bool),
			                TypeRegistry.ResolveType(TypeRegistry.BoolAlias));
		}

		[Test]
		public void ResolveIntegerByVBName()
		{
			Assert.AreEqual(typeof (int),
			                TypeRegistry.ResolveType("Integer"));
		}

		[Test]
		public void ResolveVBInteger()
		{
			Assert.AreEqual(typeof (int),
			                TypeRegistry.ResolveType(TypeRegistry.Int32AliasVB));
		}

		[Test]
		public void ResolveVBDecimal()
		{
			Assert.AreEqual(typeof (decimal),
			                TypeRegistry.ResolveType(TypeRegistry.DecimalAliasVB));
		}

		[Test]
		public void ResolveSingleByName()
		{
			Assert.AreEqual(typeof (float),
			                TypeRegistry.ResolveType("Single"));
		}

		[Test]
		public void ResolveSingle()
		{
			Assert.AreEqual(typeof (float),
			                TypeRegistry.ResolveType(TypeRegistry.SingleAlias));
		}

		[Test]
		public void ResolveVBDouble()
		{
			Assert.AreEqual(typeof (double),
			                TypeRegistry.ResolveType(TypeRegistry.DoubleAliasVB));
		}

		[Test]
		public void ResolveVBLong()
		{
			Assert.AreEqual(typeof (long),
			                TypeRegistry.ResolveType(TypeRegistry.Int64AliasVB));
		}

		[Test]
		public void ResolveVBShort()
		{
			Assert.AreEqual(typeof (short),
			                TypeRegistry.ResolveType(TypeRegistry.Int16AliasVB));
		}

		[Test]
		public void ResolveVBDate()
		{
			Assert.AreEqual(typeof (DateTime),
			                TypeRegistry.ResolveType(TypeRegistry.DateAliasVB));
		}

		[Test]
		public void ResolveVBBool()
		{
			Assert.AreEqual(typeof (bool),
			                TypeRegistry.ResolveType(TypeRegistry.BoolAliasVB));
		}

        [Test]
        public void ResolveString()
        {
            Assert.AreEqual(typeof(string),
                            TypeRegistry.ResolveType(TypeRegistry.StringAlias));
        }

        [Test]
        public void ResolveVBString()
        {
            Assert.AreEqual(typeof(string),
                            TypeRegistry.ResolveType(TypeRegistry.StringAliasVB));
        }

		[Test]
		public void ResolveStringArray()
		{
			Assert.AreEqual(typeof (string[]),
			                TypeRegistry.ResolveType(TypeRegistry.StringArrayAlias));
		}

		[Test]
		public void ResolveVBStringArray()
		{
			Assert.AreEqual(typeof (string[]),
			                TypeRegistry.ResolveType(TypeRegistry.StringArrayAliasVB));
		}

        [Test]
        public void ResolveObject()
        {
            Assert.AreEqual(typeof(object),
                            TypeRegistry.ResolveType(TypeRegistry.ObjectAlias));
        }

        [Test]
        public void ResolveVBObject()
        {
            Assert.AreEqual(typeof(object),
                            TypeRegistry.ResolveType(TypeRegistry.ObjectAliasVB));
        }

        [Test]
        public void ResolveObjectArray()
        {
            Assert.AreEqual(typeof(object[]),
                            TypeRegistry.ResolveType(TypeRegistry.ObjectArrayAlias));
        }

        [Test]
        public void ResolveVBObjectArray()
        {
            Assert.AreEqual(typeof(object[]),
                            TypeRegistry.ResolveType(TypeRegistry.ObjectArrayAliasVB));
        }

		[Test]
		public void ResolveCharArray()
		{
			Assert.AreEqual(typeof (char[]),
			                TypeRegistry.ResolveType(TypeRegistry.CharArrayAlias));
		}

		[Test]
		public void ResolveVBCharArray()
		{
			Assert.AreEqual(typeof (char[]),
			                TypeRegistry.ResolveType(TypeRegistry.CharArrayAliasVB));
		}

		[Test]
		public void ResolveInt32Array()
		{
			Assert.AreEqual(typeof (int[]),
			                TypeRegistry.ResolveType(TypeRegistry.Int32ArrayAlias));
		}

		[Test]
		public void ResolveVBInt32Array()
		{
			Assert.AreEqual(typeof (int[]),
			                TypeRegistry.ResolveType(TypeRegistry.Int32ArrayAliasVB));
		}

		[Test]
		public void ResolveInt16Array()
		{
			Assert.AreEqual(typeof (short[]),
			                TypeRegistry.ResolveType(TypeRegistry.Int16ArrayAlias));
		}

		[Test]
		public void ResolveVBInt16Array()
		{
			Assert.AreEqual(typeof (short[]),
			                TypeRegistry.ResolveType(TypeRegistry.Int16ArrayAliasVB));
		}

		[Test]
		public void ResolveInt64Array()
		{
			Assert.AreEqual(typeof (long[]),
			                TypeRegistry.ResolveType(TypeRegistry.Int64ArrayAlias));
		}

		[Test]
		public void ResolveVBInt64Array()
		{
			Assert.AreEqual(typeof (long[]),
			                TypeRegistry.ResolveType(TypeRegistry.Int64ArrayAliasVB));
		}

		[Test]
		public void ResolveUInt16Array()
		{
			Assert.AreEqual(typeof (ushort[]),
			                TypeRegistry.ResolveType(TypeRegistry.UInt16ArrayAlias));
		}

		[Test]
		public void ResolveUInt32Array()
		{
			Assert.AreEqual(typeof (uint[]),
			                TypeRegistry.ResolveType(TypeRegistry.UInt32ArrayAlias));
		}

		[Test]
		public void ResolveUInt64Array()
		{
			Assert.AreEqual(typeof (ulong[]),
			                TypeRegistry.ResolveType(TypeRegistry.UInt64ArrayAlias));
		}

		[Test]
		public void ResolveBoolArray()
		{
			Assert.AreEqual(typeof (bool[]),
			                TypeRegistry.ResolveType(TypeRegistry.BoolArrayAlias));
		}

		[Test]
		public void ResolveVBBoolArray()
		{
			Assert.AreEqual(typeof (bool[]),
			                TypeRegistry.ResolveType(TypeRegistry.BoolArrayAliasVB));
		}

		[Test]
		public void ResolveDateArray()
		{
			Assert.AreEqual(typeof (DateTime[]),
			                TypeRegistry.ResolveType(TypeRegistry.DateTimeArrayAlias));
		}

		[Test]
		public void ResolveVBDateArray()
		{
			Assert.AreEqual(typeof (DateTime[]),
			                TypeRegistry.ResolveType(TypeRegistry.DateTimeArrayAliasVB));
		}

		[Test]
		public void ResolveFloatArray()
		{
			Assert.AreEqual(typeof (float[]),
			                TypeRegistry.ResolveType(TypeRegistry.FloatArrayAlias));
		}

		[Test]
		public void ResolveVBSingleArray()
		{
			Assert.AreEqual(typeof (float[]),
			                TypeRegistry.ResolveType(TypeRegistry.SingleArrayAliasVB));
		}

		[Test]
		public void ResolveDoubleArray()
		{
			Assert.AreEqual(typeof (double[]),
			                TypeRegistry.ResolveType(TypeRegistry.DoubleArrayAlias));
		}

		[Test]
		public void ResolveVBDoubleArray()
		{
			Assert.AreEqual(typeof (double[]),
			                TypeRegistry.ResolveType(TypeRegistry.DoubleArrayAliasVB));
		}

        [Test]
        public void ResolveNullableChar()
        {
            Assert.AreEqual(typeof(char?),
                            TypeRegistry.ResolveType(TypeRegistry.NullableCharAlias));
            Assert.AreEqual(typeof(Nullable<char>),
                            TypeRegistry.ResolveType(TypeRegistry.NullableCharAlias));
        }

        [Test]
        public void ResolveNullableInteger()
        {
            Assert.AreEqual(typeof(int?),
                            TypeRegistry.ResolveType(TypeRegistry.NullableInt32Alias));
            Assert.AreEqual(typeof(Nullable<int>),
                TypeRegistry.ResolveType(TypeRegistry.NullableInt32Alias));
        }

        [Test]
        public void ResolveNullableDecimal()
        {
            Assert.AreEqual(typeof(decimal?),
                            TypeRegistry.ResolveType(TypeRegistry.NullableDecimalAlias));
            Assert.AreEqual(typeof(Nullable<decimal>),
                            TypeRegistry.ResolveType(TypeRegistry.NullableDecimalAlias));
        }

        [Test]
        public void ResolveNullableUnsignedInteger()
        {
            Assert.AreEqual(typeof(uint?),
                            TypeRegistry.ResolveType(TypeRegistry.NullableUInt32Alias));
            Assert.AreEqual(typeof(Nullable<uint>),
                            TypeRegistry.ResolveType(TypeRegistry.NullableUInt32Alias));
        }

        [Test]
        public void ResolveNullableFloat()
        {
            Assert.AreEqual(typeof(float?),
                            TypeRegistry.ResolveType(TypeRegistry.NullableFloatAlias));
            Assert.AreEqual(typeof(Nullable<float>),
                           TypeRegistry.ResolveType(TypeRegistry.NullableFloatAlias));
        }

        [Test]
        public void ResolveNullableDouble()
        {
            Assert.AreEqual(typeof(double?),
                            TypeRegistry.ResolveType(TypeRegistry.NullableDoubleAlias));
            Assert.AreEqual(typeof(Nullable<double>),
                            TypeRegistry.ResolveType(TypeRegistry.NullableDoubleAlias));
        }

        [Test]
        public void ResolveNullableLong()
        {
            Assert.AreEqual(typeof(long?),
                            TypeRegistry.ResolveType(TypeRegistry.NullableInt64Alias));
            Assert.AreEqual(typeof(Nullable<long>),
                            TypeRegistry.ResolveType(TypeRegistry.NullableInt64Alias));
        }

        [Test]
        public void ResolveNullableUnsignedLong()
        {
            Assert.AreEqual(typeof(ulong?),
                            TypeRegistry.ResolveType(TypeRegistry.NullableUInt64Alias));
            Assert.AreEqual(typeof(Nullable<ulong>),
                            TypeRegistry.ResolveType(TypeRegistry.NullableUInt64Alias));
        }

        [Test]
        public void ResolveNullableShort()
        {
            Assert.AreEqual(typeof(short?),
                            TypeRegistry.ResolveType(TypeRegistry.NullableInt16Alias));
            Assert.AreEqual(typeof(Nullable<short>),
                            TypeRegistry.ResolveType(TypeRegistry.NullableInt16Alias));
        }

        [Test]
        public void ResolveNullableUnsignedShort()
        {
            Assert.AreEqual(typeof(ushort?),
                            TypeRegistry.ResolveType(TypeRegistry.NullableUInt16Alias));
            Assert.AreEqual(typeof(Nullable<ushort>),
                            TypeRegistry.ResolveType(TypeRegistry.NullableUInt16Alias));
        }

        [Test]
        public void ResolveNullableBool()
        {
            Assert.AreEqual(typeof(bool?),
                            TypeRegistry.ResolveType(TypeRegistry.NullableBoolAlias));
            Assert.AreEqual(typeof(Nullable<bool>),
                            TypeRegistry.ResolveType(TypeRegistry.NullableBoolAlias));
        }

        [Test]
        public void ResolveNullableCharArray()
        {
            Assert.AreEqual(typeof(char?[]),
                            TypeRegistry.ResolveType(TypeRegistry.NullableCharArrayAlias));
        }

        [Test]
        public void ResolveNullableInt32Array()
        {
            Assert.AreEqual(typeof(int?[]),
                            TypeRegistry.ResolveType(TypeRegistry.NullableInt32ArrayAlias));
        }

        [Test]
        public void ResolveNullableDecimalArray()
        {
            Assert.AreEqual(typeof(decimal?[]),
                            TypeRegistry.ResolveType(TypeRegistry.NullableDecimalArrayAlias));
        }

        [Test]
        public void ResolveNullableInt16Array()
        {
            Assert.AreEqual(typeof(short?[]),
                            TypeRegistry.ResolveType(TypeRegistry.NullableInt16ArrayAlias));
        }

        [Test]
        public void ResolveNullableInt64Array()
        {
            Assert.AreEqual(typeof(long?[]),
                            TypeRegistry.ResolveType(TypeRegistry.NullableInt64ArrayAlias));
        }

        [Test]
        public void ResolveNullableUInt16Array()
        {
            Assert.AreEqual(typeof(ushort?[]),
                            TypeRegistry.ResolveType(TypeRegistry.NullableUInt16ArrayAlias));
        }

        [Test]
        public void ResolveNullableUInt32Array()
        {
            Assert.AreEqual(typeof(uint?[]),
                            TypeRegistry.ResolveType(TypeRegistry.NullableUInt32ArrayAlias));
        }

        [Test]
        public void ResolveNullableUInt64Array()
        {
            Assert.AreEqual(typeof(ulong?[]),
                            TypeRegistry.ResolveType(TypeRegistry.NullableUInt64ArrayAlias));
        }

        [Test]
        public void ResolveNullableBoolArray()
        {
            Assert.AreEqual(typeof(bool?[]),
                            TypeRegistry.ResolveType(TypeRegistry.NullableBoolArrayAlias));
        }

        [Test]
        public void ResolveNullableFloatArray()
        {
            Assert.AreEqual(typeof(float?[]),
                            TypeRegistry.ResolveType(TypeRegistry.NullableFloatArrayAlias));
        }

        [Test]
        public void ResolveNullableDoubleArray()
        {
            Assert.AreEqual(typeof(double?[]),
                            TypeRegistry.ResolveType(TypeRegistry.NullableDoubleArrayAlias));
        }
	}

	internal class Foo
	{
		private Type bar;

		public Type Bar
		{
			get { return bar; }
			set { bar = value; }
		}
	}
}