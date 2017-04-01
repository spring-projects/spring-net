#region License

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

#endregion

#region Imports

using System;

using NUnit.Framework;

#endregion

namespace Spring.Util
{
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
            Assert.IsTrue(NumberUtils.IsInteger(10));
            Assert.IsTrue(NumberUtils.IsInteger(10L));
            Assert.IsTrue(NumberUtils.IsInteger((short) 10));
            Assert.IsFalse(NumberUtils.IsInteger('e'));
            Assert.IsFalse(NumberUtils.IsInteger(null));
            Assert.IsFalse(NumberUtils.IsInteger(9.5D));
            Assert.IsFalse(NumberUtils.IsInteger(9.5F));
            Assert.IsFalse(NumberUtils.IsInteger(this));
            Assert.IsFalse(NumberUtils.IsInteger(null));
            Assert.IsFalse(NumberUtils.IsInteger(string.Empty));
        }

        [Test]
        public void IsDecimal()
        {
            Assert.IsFalse(NumberUtils.IsDecimal(10));
            Assert.IsFalse(NumberUtils.IsDecimal(10L));
            Assert.IsFalse(NumberUtils.IsDecimal((short) 10));
            Assert.IsFalse(NumberUtils.IsDecimal('e'));
            Assert.IsFalse(NumberUtils.IsDecimal(null));
            Assert.IsTrue(NumberUtils.IsDecimal(9.5D));
            Assert.IsTrue(NumberUtils.IsDecimal(9.5F));
            Assert.IsFalse(NumberUtils.IsDecimal(this));
            Assert.IsFalse(NumberUtils.IsDecimal(null));
            Assert.IsFalse(NumberUtils.IsDecimal(string.Empty));
        }

        [Test]
        public void IsNumber()
        {
            Assert.IsTrue(NumberUtils.IsNumber(10));
            Assert.IsTrue(NumberUtils.IsNumber(10L));
            Assert.IsTrue(NumberUtils.IsNumber((short) 10));
            Assert.IsFalse(NumberUtils.IsNumber('e'));
            Assert.IsFalse(NumberUtils.IsNumber(null));
            Assert.IsTrue(NumberUtils.IsNumber(9.5D));
            Assert.IsTrue(NumberUtils.IsNumber(9.5F));
            Assert.IsFalse(NumberUtils.IsNumber(this));
            Assert.IsFalse(NumberUtils.IsNumber(null));
            Assert.IsFalse(NumberUtils.IsNumber(string.Empty));
        }

        [Test]
        public void IsZero()
        {
            Assert.IsFalse(NumberUtils.IsZero((short) 2));
            Assert.IsTrue(NumberUtils.IsZero((short) 0));

            Assert.IsFalse(NumberUtils.IsZero(2));
            Assert.IsTrue(NumberUtils.IsZero(0));

            Assert.IsFalse(NumberUtils.IsZero((long) 2));
            Assert.IsTrue(NumberUtils.IsZero((long) 0));

            Assert.IsFalse(NumberUtils.IsZero((ushort) 2));
            Assert.IsTrue(NumberUtils.IsZero((ushort) 0));

            Assert.IsFalse(NumberUtils.IsZero((uint) 2));
            Assert.IsTrue(NumberUtils.IsZero((uint) 0));

            Assert.IsFalse(NumberUtils.IsZero((ulong) 2));
            Assert.IsTrue(NumberUtils.IsZero((ulong) 0));

            Assert.IsFalse(NumberUtils.IsZero((decimal) 2));
            Assert.IsTrue(NumberUtils.IsZero((decimal) 0));

            Assert.IsTrue(NumberUtils.IsZero((byte?) 0));
            Assert.IsFalse(NumberUtils.IsZero((byte) 2));

            Assert.IsTrue(NumberUtils.IsZero((sbyte?) 0));
            Assert.IsFalse(NumberUtils.IsZero((sbyte) 2));
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
            Assert.AreEqual(-10, NumberUtils.Negate(10));
        }

        [Test]
        public void CoercesTypes()
        {
            object x = (int) 1;
            object y = (double) 2;
            NumberUtils.CoerceTypes(ref x, ref y);
            Assert.AreEqual(typeof(double), x.GetType());
        }

        [Test]
        public void Add()
        {
            Assert.AreEqual(5, NumberUtils.Add(2, 3));
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
            Assert.AreEqual(~(byte) 2, NumberUtils.BitwiseNot((byte) 2));
            Assert.AreEqual(~(sbyte) 2, NumberUtils.BitwiseNot((sbyte) 2));
            Assert.AreEqual(~(short) 2, NumberUtils.BitwiseNot((short) 2));
            Assert.AreEqual(~(ushort) 2, NumberUtils.BitwiseNot((ushort) 2));
            Assert.AreEqual(~(int) 2, NumberUtils.BitwiseNot((int) 2));
            Assert.AreEqual(~(uint) 2, NumberUtils.BitwiseNot((uint) 2));
            Assert.AreEqual(~(long) 2, NumberUtils.BitwiseNot((long) 2));
            Assert.AreEqual(~(ulong) 2, NumberUtils.BitwiseNot((ulong) 2));
            Assert.AreEqual(false, NumberUtils.BitwiseNot(true));
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
            Assert.AreEqual((byte) 2 & (byte) 3, NumberUtils.BitwiseAnd((byte) 2, (byte) 3));
            Assert.AreEqual((sbyte) 2 & (sbyte) 3, NumberUtils.BitwiseAnd((sbyte) 2, (sbyte) 3));
            Assert.AreEqual((short) 2 & (short) 3, NumberUtils.BitwiseAnd((short) 2, (short) 3));
            Assert.AreEqual((ushort) 2 & (ushort) 3, NumberUtils.BitwiseAnd((ushort) 2, (ushort) 3));
            Assert.AreEqual((int) 2 & (int) 3, NumberUtils.BitwiseAnd((int) 2, (int) 3));
            Assert.AreEqual((uint) 2 & (uint) 3, NumberUtils.BitwiseAnd((uint) 2, (uint) 3));
            Assert.AreEqual((long) 2 & (long) 3, NumberUtils.BitwiseAnd((long) 2, (long) 3));
            Assert.AreEqual((ulong) 2 & (ulong) 3, NumberUtils.BitwiseAnd((ulong) 2, (ulong) 3));
            Assert.AreEqual((ulong) 2 & (byte) 3, NumberUtils.BitwiseAnd((ulong) 2, (byte) 3));
            Assert.AreEqual(true, NumberUtils.BitwiseAnd(true, true));
            Assert.AreEqual(false, NumberUtils.BitwiseAnd(false, true));
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
            Assert.AreEqual((byte) 2 | (byte) 3, NumberUtils.BitwiseOr((byte) 2, (byte) 3));
            Assert.AreEqual((sbyte) 2 | (sbyte) 3, NumberUtils.BitwiseOr((sbyte) 2, (sbyte) 3));
            Assert.AreEqual((short) 2 | (short) 3, NumberUtils.BitwiseOr((short) 2, (short) 3));
            Assert.AreEqual((ushort) 2 | (ushort) 3, NumberUtils.BitwiseOr((ushort) 2, (ushort) 3));
            Assert.AreEqual((int) 2 | (int) 3, NumberUtils.BitwiseOr((int) 2, (int) 3));
            Assert.AreEqual((uint) 2 | (uint) 3, NumberUtils.BitwiseOr((uint) 2, (uint) 3));
            Assert.AreEqual((long) 2 | (long) 3, NumberUtils.BitwiseOr((long) 2, (long) 3));
            Assert.AreEqual((ulong) 2 | (ulong) 3, NumberUtils.BitwiseOr((ulong) 2, (ulong) 3));
            Assert.AreEqual((ulong) 2 | (byte) 3, NumberUtils.BitwiseOr((ulong) 2, (byte) 3));
            Assert.AreEqual(false, NumberUtils.BitwiseOr(false, false));
            Assert.AreEqual(true, NumberUtils.BitwiseOr(false, true));
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
            Assert.AreEqual((byte) 2 ^ (byte) 3, NumberUtils.BitwiseXor((byte) 2, (byte) 3));
            Assert.AreEqual((sbyte) 2 ^ (sbyte) 3, NumberUtils.BitwiseXor((sbyte) 2, (sbyte) 3));
            Assert.AreEqual((short) 2 ^ (short) 3, NumberUtils.BitwiseXor((short) 2, (short) 3));
            Assert.AreEqual((ushort) 2 ^ (ushort) 3, NumberUtils.BitwiseXor((ushort) 2, (ushort) 3));
            Assert.AreEqual((int) 2 ^ (int) 3, NumberUtils.BitwiseXor((int) 2, (int) 3));
            Assert.AreEqual((uint) 2 ^ (uint) 3, NumberUtils.BitwiseXor((uint) 2, (uint) 3));
            Assert.AreEqual((long) 2 ^ (long) 3, NumberUtils.BitwiseXor((long) 2, (long) 3));
            Assert.AreEqual((ulong) 2 ^ (ulong) 3, NumberUtils.BitwiseXor((ulong) 2, (ulong) 3));
            Assert.AreEqual((ulong) 2 ^ (byte) 3, NumberUtils.BitwiseXor((ulong) 2, (byte) 3));
            Assert.AreEqual(false, NumberUtils.BitwiseXor(false, false));
            Assert.AreEqual(false, NumberUtils.BitwiseXor(true, true));
            Assert.AreEqual(true, NumberUtils.BitwiseXor(false, true));
            Assert.AreEqual(true, NumberUtils.BitwiseXor(true, false));
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
}