/*
 * Copyright 2004 the original author or authors.
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

namespace Spring.Util;

/// <summary>
/// Unit tests for the NumberUtils class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class NumberUtilsTests
{
    [Test]
    public void IsInteger()
    {
        Assert.That(NumberUtils.IsInteger(10), Is.True);
        Assert.That(NumberUtils.IsInteger(10L), Is.True);
        Assert.That(NumberUtils.IsInteger((short) 10), Is.True);
        Assert.That(NumberUtils.IsInteger('e'), Is.False);
        Assert.That(NumberUtils.IsInteger(null), Is.False);
        Assert.That(NumberUtils.IsInteger(9.5D), Is.False);
        Assert.That(NumberUtils.IsInteger(9.5F), Is.False);
        Assert.That(NumberUtils.IsInteger(this), Is.False);
        Assert.That(NumberUtils.IsInteger(null), Is.False);
        Assert.That(NumberUtils.IsInteger(string.Empty), Is.False);
    }

    [Test]
    public void IsDecimal()
    {
        Assert.That(NumberUtils.IsDecimal(10), Is.False);
        Assert.That(NumberUtils.IsDecimal(10L), Is.False);
        Assert.That(NumberUtils.IsDecimal((short) 10), Is.False);
        Assert.That(NumberUtils.IsDecimal('e'), Is.False);
        Assert.That(NumberUtils.IsDecimal(null), Is.False);
        Assert.That(NumberUtils.IsDecimal(9.5D), Is.True);
        Assert.That(NumberUtils.IsDecimal(9.5F), Is.True);
        Assert.That(NumberUtils.IsDecimal(this), Is.False);
        Assert.That(NumberUtils.IsDecimal(null), Is.False);
        Assert.That(NumberUtils.IsDecimal(string.Empty), Is.False);
    }

    [Test]
    public void IsNumber()
    {
        Assert.That(NumberUtils.IsNumber(10), Is.True);
        Assert.That(NumberUtils.IsNumber(10L), Is.True);
        Assert.That(NumberUtils.IsNumber((short) 10), Is.True);
        Assert.That(NumberUtils.IsNumber('e'), Is.False);
        Assert.That(NumberUtils.IsNumber(null), Is.False);
        Assert.That(NumberUtils.IsNumber(9.5D), Is.True);
        Assert.That(NumberUtils.IsNumber(9.5F), Is.True);
        Assert.That(NumberUtils.IsNumber(this), Is.False);
        Assert.That(NumberUtils.IsNumber(null), Is.False);
        Assert.That(NumberUtils.IsNumber(string.Empty), Is.False);
    }

    [Test]
    public void IsZero()
    {
        Assert.That(NumberUtils.IsZero((short) 2), Is.False);
        Assert.That(NumberUtils.IsZero((short) 0), Is.True);

        Assert.That(NumberUtils.IsZero(2), Is.False);
        Assert.That(NumberUtils.IsZero(0), Is.True);

        Assert.That(NumberUtils.IsZero((long) 2), Is.False);
        Assert.That(NumberUtils.IsZero((long) 0), Is.True);

        Assert.That(NumberUtils.IsZero((ushort) 2), Is.False);
        Assert.That(NumberUtils.IsZero((ushort) 0), Is.True);

        Assert.That(NumberUtils.IsZero((uint) 2), Is.False);
        Assert.That(NumberUtils.IsZero((uint) 0), Is.True);

        Assert.That(NumberUtils.IsZero((ulong) 2), Is.False);
        Assert.That(NumberUtils.IsZero((ulong) 0), Is.True);

        Assert.That(NumberUtils.IsZero((decimal) 2), Is.False);
        Assert.That(NumberUtils.IsZero((decimal) 0), Is.True);

        Assert.That(NumberUtils.IsZero((byte?) 0), Is.True);
        Assert.That(NumberUtils.IsZero((byte) 2), Is.False);

        Assert.That(NumberUtils.IsZero((sbyte?) 0), Is.True);
        Assert.That(NumberUtils.IsZero((sbyte) 2), Is.False);
    }

    [Test]
    public void NegateNull()
    {
        Assert.Throws<ArgumentException>(() => NumberUtils.Negate(null));
    }

    [Test]
    public void NegateString()
    {
        Assert.Throws<ArgumentException>(() => NumberUtils.Negate(string.Empty));
    }

    [Test]
    public void Negate()
    {
        Assert.That(NumberUtils.Negate(10), Is.EqualTo(-10));
    }

    [Test]
    public void CoercesTypes()
    {
        object x = (int) 1;
        object y = (double) 2;
        NumberUtils.CoerceTypes(ref x, ref y);
        Assert.That(x.GetType(), Is.EqualTo(typeof(double)));
    }

    [Test]
    public void Add()
    {
        Assert.That(NumberUtils.Add(2, 3), Is.EqualTo(5));
        try
        {
            NumberUtils.Add(2, "3");
            Assert.Fail();
        }
        catch (ArgumentException)
        {
        }
    }

    [Test]
    public void BitwiseNot()
    {
        Assert.That(NumberUtils.BitwiseNot((byte) 2), Is.EqualTo(~(byte) 2));
        Assert.That(NumberUtils.BitwiseNot((sbyte) 2), Is.EqualTo(~(sbyte) 2));
        Assert.That(NumberUtils.BitwiseNot((short) 2), Is.EqualTo(~(short) 2));
        Assert.That(NumberUtils.BitwiseNot((ushort) 2), Is.EqualTo(~(ushort) 2));
        Assert.That(NumberUtils.BitwiseNot((int) 2), Is.EqualTo(~(int) 2));
        Assert.That(NumberUtils.BitwiseNot((uint) 2), Is.EqualTo(~(uint) 2));
        Assert.That(NumberUtils.BitwiseNot((long) 2), Is.EqualTo(~(long) 2));
        Assert.That(NumberUtils.BitwiseNot((ulong) 2), Is.EqualTo(~(ulong) 2));
        Assert.That(NumberUtils.BitwiseNot(true), Is.EqualTo(false));
        try
        {
            NumberUtils.BitwiseNot((double) 2.0);
            Assert.Fail();
        }
        catch (ArgumentException)
        {
        }
    }

    [Test]
    public void BitwiseAnd()
    {
        Assert.That(NumberUtils.BitwiseAnd((byte) 2, (byte) 3), Is.EqualTo((byte) 2 & (byte) 3));
        Assert.That(NumberUtils.BitwiseAnd((sbyte) 2, (sbyte) 3), Is.EqualTo((sbyte) 2 & (sbyte) 3));
        Assert.That(NumberUtils.BitwiseAnd((short) 2, (short) 3), Is.EqualTo((short) 2 & (short) 3));
        Assert.That(NumberUtils.BitwiseAnd((ushort) 2, (ushort) 3), Is.EqualTo((ushort) 2 & (ushort) 3));
        Assert.That(NumberUtils.BitwiseAnd((int) 2, (int) 3), Is.EqualTo((int) 2 & (int) 3));
        Assert.That(NumberUtils.BitwiseAnd((uint) 2, (uint) 3), Is.EqualTo((uint) 2 & (uint) 3));
        Assert.That(NumberUtils.BitwiseAnd((long) 2, (long) 3), Is.EqualTo((long) 2 & (long) 3));
        Assert.That(NumberUtils.BitwiseAnd((ulong) 2, (ulong) 3), Is.EqualTo((ulong) 2 & (ulong) 3));
        Assert.That(NumberUtils.BitwiseAnd((ulong) 2, (byte) 3), Is.EqualTo((ulong) 2 & (byte) 3));
        Assert.That(NumberUtils.BitwiseAnd(true, true), Is.EqualTo(true));
        Assert.That(NumberUtils.BitwiseAnd(false, true), Is.EqualTo(false));
        try
        {
            NumberUtils.BitwiseAnd((double) 2.0, 3);
            Assert.Fail();
        }
        catch (ArgumentException)
        {
        }
    }

    [Test]
    public void BitwiseOr()
    {
        Assert.That(NumberUtils.BitwiseOr((byte) 2, (byte) 3), Is.EqualTo((byte) 2 | (byte) 3));
        Assert.That(NumberUtils.BitwiseOr((sbyte) 2, (sbyte) 3), Is.EqualTo((sbyte) 2 | (sbyte) 3));
        Assert.That(NumberUtils.BitwiseOr((short) 2, (short) 3), Is.EqualTo((short) 2 | (short) 3));
        Assert.That(NumberUtils.BitwiseOr((ushort) 2, (ushort) 3), Is.EqualTo((ushort) 2 | (ushort) 3));
        Assert.That(NumberUtils.BitwiseOr((int) 2, (int) 3), Is.EqualTo((int) 2 | (int) 3));
        Assert.That(NumberUtils.BitwiseOr((uint) 2, (uint) 3), Is.EqualTo((uint) 2 | (uint) 3));
        Assert.That(NumberUtils.BitwiseOr((long) 2, (long) 3), Is.EqualTo((long) 2 | (long) 3));
        Assert.That(NumberUtils.BitwiseOr((ulong) 2, (ulong) 3), Is.EqualTo((ulong) 2 | (ulong) 3));
        Assert.That(NumberUtils.BitwiseOr((ulong) 2, (byte) 3), Is.EqualTo((ulong) 2 | (byte) 3));
        Assert.That(NumberUtils.BitwiseOr(false, false), Is.EqualTo(false));
        Assert.That(NumberUtils.BitwiseOr(false, true), Is.EqualTo(true));
        try
        {
            NumberUtils.BitwiseAnd((double) 2.0, 3);
            Assert.Fail();
        }
        catch (ArgumentException)
        {
        }
    }

    [Test]
    public void BitwiseXor()
    {
        Assert.That(NumberUtils.BitwiseXor((byte) 2, (byte) 3), Is.EqualTo((byte) 2 ^ (byte) 3));
        Assert.That(NumberUtils.BitwiseXor((sbyte) 2, (sbyte) 3), Is.EqualTo((sbyte) 2 ^ (sbyte) 3));
        Assert.That(NumberUtils.BitwiseXor((short) 2, (short) 3), Is.EqualTo((short) 2 ^ (short) 3));
        Assert.That(NumberUtils.BitwiseXor((ushort) 2, (ushort) 3), Is.EqualTo((ushort) 2 ^ (ushort) 3));
        Assert.That(NumberUtils.BitwiseXor((int) 2, (int) 3), Is.EqualTo((int) 2 ^ (int) 3));
        Assert.That(NumberUtils.BitwiseXor((uint) 2, (uint) 3), Is.EqualTo((uint) 2 ^ (uint) 3));
        Assert.That(NumberUtils.BitwiseXor((long) 2, (long) 3), Is.EqualTo((long) 2 ^ (long) 3));
        Assert.That(NumberUtils.BitwiseXor((ulong) 2, (ulong) 3), Is.EqualTo((ulong) 2 ^ (ulong) 3));
        Assert.That(NumberUtils.BitwiseXor((ulong) 2, (byte) 3), Is.EqualTo((ulong) 2 ^ (byte) 3));
        Assert.That(NumberUtils.BitwiseXor(false, false), Is.EqualTo(false));
        Assert.That(NumberUtils.BitwiseXor(true, true), Is.EqualTo(false));
        Assert.That(NumberUtils.BitwiseXor(false, true), Is.EqualTo(true));
        Assert.That(NumberUtils.BitwiseXor(true, false), Is.EqualTo(true));
        try
        {
            NumberUtils.BitwiseAnd((double) 2.0, 3);
            Assert.Fail();
        }
        catch (ArgumentException)
        {
        }
    }
}
