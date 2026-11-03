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

using NUnit.Framework;
using Spring.Objects;
using Spring.Objects.Factory;
using IBar = Spring.Objects.Factory.IBar;
using Spring.Context;
using Spring.Context.Support;

namespace Spring.Core.TypeResolution;

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
        TypeRegistry.RegisterType("Foo", typeof(Foo));
        TypeRegistry.RegisterType("Bar", "Spring.Objects.Factory.Bar, Spring.Core.Tests");

        Assert.That(TypeRegistry.ResolveType("Foo"), Is.EqualTo(typeof(Foo)));
        Assert.That(TypeRegistry.ResolveType("Bar"), Is.EqualTo(typeof(Bar)));

        IApplicationContext ctx =
            new XmlApplicationContext("assembly://Spring.Core.Tests/Spring.Core.TypeResolution/aliasedObjects.xml");

        Foo foo = ctx.GetObject("aliasedType") as Foo;
        Assert.That(foo, Is.Not.Null);
        Assert.That(foo.Bar, Is.Not.Null);
        Assert.That(foo.Bar, Is.EqualTo(typeof(Bar)));
        Assert.That(typeof(IBar).IsAssignableFrom(foo.Bar), Is.True);
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
        Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType(null, typeof(TestObject)));
    }

    [Test]
    public void RegisterTypeWithEmptyAliasArg()
    {
        Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType(string.Empty, typeof(TestObject)));
    }

    [Test]
    public void RegisterTypeWithWhitespacedAliasArg()
    {
        Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType("   ", typeof(TestObject)));
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
        Assert.That(type, Is.Null, "Must return null if no Type is registered under the supplied alias.");
    }

    [Test]
    public void RegisteringAnAliasTwiceDoesNotThrowException()
    {
        const string Alias = "foo";

        TypeRegistry.RegisterType(Alias, typeof(TestObject));
        TypeRegistry.RegisterType(Alias, GetType());

        Type type = TypeRegistry.ResolveType(Alias);
        Assert.That(type, Is.EqualTo(GetType()), "Overriding Type was not registered.");
    }

    [Test]
    public void ResolveIntegerByName()
    {
        Assert.That(TypeRegistry.ResolveType("int"),
            Is.EqualTo(typeof(int)));
    }

    [Test]
    public void ResolveChar()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.CharAlias),
            Is.EqualTo(typeof(char)));
    }

    [Test]
    public void ResolveInteger()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.Int32Alias),
            Is.EqualTo(typeof(int)));
    }

    [Test]
    public void ResolveDecimal()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.DecimalAlias),
            Is.EqualTo(typeof(decimal)));
    }

    [Test]
    public void ResolveUnsignedIntegerByName()
    {
        Assert.That(TypeRegistry.ResolveType("uint"),
            Is.EqualTo(typeof(uint)));
    }

    [Test]
    public void ResolveUnsignedInteger()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.UInt32Alias),
            Is.EqualTo(typeof(uint)));
    }

    [Test]
    public void ResolveFloatByName()
    {
        Assert.That(TypeRegistry.ResolveType("float"),
            Is.EqualTo(typeof(float)));
    }

    [Test]
    public void ResolveFloat()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.FloatAlias),
            Is.EqualTo(typeof(float)));
    }

    [Test]
    public void ResolveDoubleByName()
    {
        Assert.That(TypeRegistry.ResolveType("double"),
            Is.EqualTo(typeof(double)));
    }

    [Test]
    public void ResolveDouble()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.DoubleAlias),
            Is.EqualTo(typeof(double)));
    }

    [Test]
    public void ResolveLongByName()
    {
        Assert.That(TypeRegistry.ResolveType("long"),
            Is.EqualTo(typeof(long)));
    }

    [Test]
    public void ResolveLong()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.Int64Alias),
            Is.EqualTo(typeof(long)));
    }

    [Test]
    public void ResolveUnsignedLongByName()
    {
        Assert.That(TypeRegistry.ResolveType("ulong"),
            Is.EqualTo(typeof(ulong)));
    }

    [Test]
    public void ResolveUnsignedLong()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.UInt64Alias),
            Is.EqualTo(typeof(ulong)));
    }

    [Test]
    public void ResolveShortByName()
    {
        Assert.That(TypeRegistry.ResolveType("short"),
            Is.EqualTo(typeof(short)));
    }

    [Test]
    public void ResolveShort()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.Int16Alias),
            Is.EqualTo(typeof(short)));
    }

    [Test]
    public void ResolveUnsignedShortByName()
    {
        Assert.That(TypeRegistry.ResolveType("ushort"),
            Is.EqualTo(typeof(ushort)));
    }

    [Test]
    public void ResolveUnsignedShort()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.UInt16Alias),
            Is.EqualTo(typeof(ushort)));
    }

    [Test]
    public void ResolveDate()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.DateAlias),
            Is.EqualTo(typeof(DateTime)));
    }

    [Test]
    public void ResolveBool()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.BoolAlias),
            Is.EqualTo(typeof(bool)));
    }

    [Test]
    public void ResolveIntegerByVBName()
    {
        Assert.That(TypeRegistry.ResolveType("Integer"),
            Is.EqualTo(typeof(int)));
    }

    [Test]
    public void ResolveVBInteger()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.Int32AliasVB),
            Is.EqualTo(typeof(int)));
    }

    [Test]
    public void ResolveVBDecimal()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.DecimalAliasVB),
            Is.EqualTo(typeof(decimal)));
    }

    [Test]
    public void ResolveSingleByName()
    {
        Assert.That(TypeRegistry.ResolveType("Single"),
            Is.EqualTo(typeof(float)));
    }

    [Test]
    public void ResolveSingle()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.SingleAlias),
            Is.EqualTo(typeof(float)));
    }

    [Test]
    public void ResolveVBDouble()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.DoubleAliasVB),
            Is.EqualTo(typeof(double)));
    }

    [Test]
    public void ResolveVBLong()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.Int64AliasVB),
            Is.EqualTo(typeof(long)));
    }

    [Test]
    public void ResolveVBShort()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.Int16AliasVB),
            Is.EqualTo(typeof(short)));
    }

    [Test]
    public void ResolveVBDate()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.DateAliasVB),
            Is.EqualTo(typeof(DateTime)));
    }

    [Test]
    public void ResolveVBBool()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.BoolAliasVB),
            Is.EqualTo(typeof(bool)));
    }

    [Test]
    public void ResolveString()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.StringAlias),
            Is.EqualTo(typeof(string)));
    }

    [Test]
    public void ResolveVBString()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.StringAliasVB),
            Is.EqualTo(typeof(string)));
    }

    [Test]
    public void ResolveStringArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.StringArrayAlias),
            Is.EqualTo(typeof(string[])));
    }

    [Test]
    public void ResolveVBStringArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.StringArrayAliasVB),
            Is.EqualTo(typeof(string[])));
    }

    [Test]
    public void ResolveObject()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.ObjectAlias),
            Is.EqualTo(typeof(object)));
    }

    [Test]
    public void ResolveVBObject()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.ObjectAliasVB),
            Is.EqualTo(typeof(object)));
    }

    [Test]
    public void ResolveObjectArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.ObjectArrayAlias),
            Is.EqualTo(typeof(object[])));
    }

    [Test]
    public void ResolveVBObjectArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.ObjectArrayAliasVB),
            Is.EqualTo(typeof(object[])));
    }

    [Test]
    public void ResolveCharArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.CharArrayAlias),
            Is.EqualTo(typeof(char[])));
    }

    [Test]
    public void ResolveVBCharArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.CharArrayAliasVB),
            Is.EqualTo(typeof(char[])));
    }

    [Test]
    public void ResolveInt32Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.Int32ArrayAlias),
            Is.EqualTo(typeof(int[])));
    }

    [Test]
    public void ResolveVBInt32Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.Int32ArrayAliasVB),
            Is.EqualTo(typeof(int[])));
    }

    [Test]
    public void ResolveInt16Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.Int16ArrayAlias),
            Is.EqualTo(typeof(short[])));
    }

    [Test]
    public void ResolveVBInt16Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.Int16ArrayAliasVB),
            Is.EqualTo(typeof(short[])));
    }

    [Test]
    public void ResolveInt64Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.Int64ArrayAlias),
            Is.EqualTo(typeof(long[])));
    }

    [Test]
    public void ResolveVBInt64Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.Int64ArrayAliasVB),
            Is.EqualTo(typeof(long[])));
    }

    [Test]
    public void ResolveUInt16Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.UInt16ArrayAlias),
            Is.EqualTo(typeof(ushort[])));
    }

    [Test]
    public void ResolveUInt32Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.UInt32ArrayAlias),
            Is.EqualTo(typeof(uint[])));
    }

    [Test]
    public void ResolveUInt64Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.UInt64ArrayAlias),
            Is.EqualTo(typeof(ulong[])));
    }

    [Test]
    public void ResolveBoolArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.BoolArrayAlias),
            Is.EqualTo(typeof(bool[])));
    }

    [Test]
    public void ResolveVBBoolArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.BoolArrayAliasVB),
            Is.EqualTo(typeof(bool[])));
    }

    [Test]
    public void ResolveDateArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.DateTimeArrayAlias),
            Is.EqualTo(typeof(DateTime[])));
    }

    [Test]
    public void ResolveVBDateArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.DateTimeArrayAliasVB),
            Is.EqualTo(typeof(DateTime[])));
    }

    [Test]
    public void ResolveFloatArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.FloatArrayAlias),
            Is.EqualTo(typeof(float[])));
    }

    [Test]
    public void ResolveVBSingleArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.SingleArrayAliasVB),
            Is.EqualTo(typeof(float[])));
    }

    [Test]
    public void ResolveDoubleArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.DoubleArrayAlias),
            Is.EqualTo(typeof(double[])));
    }

    [Test]
    public void ResolveVBDoubleArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.DoubleArrayAliasVB),
            Is.EqualTo(typeof(double[])));
    }

    [Test]
    public void ResolveNullableChar()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableCharAlias),
            Is.EqualTo(typeof(char?)));
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableCharAlias),
            Is.EqualTo(typeof(Nullable<char>)));
    }

    [Test]
    public void ResolveNullableInteger()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableInt32Alias),
            Is.EqualTo(typeof(int?)));
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableInt32Alias),
            Is.EqualTo(typeof(Nullable<int>)));
    }

    [Test]
    public void ResolveNullableDecimal()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableDecimalAlias),
            Is.EqualTo(typeof(decimal?)));
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableDecimalAlias),
            Is.EqualTo(typeof(Nullable<decimal>)));
    }

    [Test]
    public void ResolveNullableUnsignedInteger()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableUInt32Alias),
            Is.EqualTo(typeof(uint?)));
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableUInt32Alias),
            Is.EqualTo(typeof(Nullable<uint>)));
    }

    [Test]
    public void ResolveNullableFloat()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableFloatAlias),
            Is.EqualTo(typeof(float?)));
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableFloatAlias),
            Is.EqualTo(typeof(Nullable<float>)));
    }

    [Test]
    public void ResolveNullableDouble()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableDoubleAlias),
            Is.EqualTo(typeof(double?)));
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableDoubleAlias),
            Is.EqualTo(typeof(Nullable<double>)));
    }

    [Test]
    public void ResolveNullableLong()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableInt64Alias),
            Is.EqualTo(typeof(long?)));
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableInt64Alias),
            Is.EqualTo(typeof(Nullable<long>)));
    }

    [Test]
    public void ResolveNullableUnsignedLong()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableUInt64Alias),
            Is.EqualTo(typeof(ulong?)));
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableUInt64Alias),
            Is.EqualTo(typeof(Nullable<ulong>)));
    }

    [Test]
    public void ResolveNullableShort()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableInt16Alias),
            Is.EqualTo(typeof(short?)));
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableInt16Alias),
            Is.EqualTo(typeof(Nullable<short>)));
    }

    [Test]
    public void ResolveNullableUnsignedShort()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableUInt16Alias),
            Is.EqualTo(typeof(ushort?)));
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableUInt16Alias),
            Is.EqualTo(typeof(Nullable<ushort>)));
    }

    [Test]
    public void ResolveNullableBool()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableBoolAlias),
            Is.EqualTo(typeof(bool?)));
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableBoolAlias),
            Is.EqualTo(typeof(Nullable<bool>)));
    }

    [Test]
    public void ResolveNullableCharArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableCharArrayAlias),
            Is.EqualTo(typeof(char?[])));
    }

    [Test]
    public void ResolveNullableInt32Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableInt32ArrayAlias),
            Is.EqualTo(typeof(int?[])));
    }

    [Test]
    public void ResolveNullableDecimalArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableDecimalArrayAlias),
            Is.EqualTo(typeof(decimal?[])));
    }

    [Test]
    public void ResolveNullableInt16Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableInt16ArrayAlias),
            Is.EqualTo(typeof(short?[])));
    }

    [Test]
    public void ResolveNullableInt64Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableInt64ArrayAlias),
            Is.EqualTo(typeof(long?[])));
    }

    [Test]
    public void ResolveNullableUInt16Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableUInt16ArrayAlias),
            Is.EqualTo(typeof(ushort?[])));
    }

    [Test]
    public void ResolveNullableUInt32Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableUInt32ArrayAlias),
            Is.EqualTo(typeof(uint?[])));
    }

    [Test]
    public void ResolveNullableUInt64Array()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableUInt64ArrayAlias),
            Is.EqualTo(typeof(ulong?[])));
    }

    [Test]
    public void ResolveNullableBoolArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableBoolArrayAlias),
            Is.EqualTo(typeof(bool?[])));
    }

    [Test]
    public void ResolveNullableFloatArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableFloatArrayAlias),
            Is.EqualTo(typeof(float?[])));
    }

    [Test]
    public void ResolveNullableDoubleArray()
    {
        Assert.That(TypeRegistry.ResolveType(TypeRegistry.NullableDoubleArrayAlias),
            Is.EqualTo(typeof(double?[])));
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