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
            Assert.IsTrue(NumberUtils.IsNumber((short)10));
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
            Assert.IsFalse(NumberUtils.IsZero((Int16)2));
            Assert.IsTrue(NumberUtils.IsZero((Int16)0));

            Assert.IsFalse(NumberUtils.IsZero((Int32)2));
            Assert.IsTrue(NumberUtils.IsZero((Int32)0));

            Assert.IsFalse(NumberUtils.IsZero((Int64)2));
            Assert.IsTrue(NumberUtils.IsZero((Int64)0));

            Assert.IsFalse(NumberUtils.IsZero((UInt16)2));
            Assert.IsTrue(NumberUtils.IsZero((UInt16)0));

            Assert.IsFalse(NumberUtils.IsZero((UInt32)2));
            Assert.IsTrue(NumberUtils.IsZero((UInt32)0));

            Assert.IsFalse(NumberUtils.IsZero((UInt64)2));
            Assert.IsTrue(NumberUtils.IsZero((UInt64)0));

            Assert.IsFalse(NumberUtils.IsZero((decimal)2));
            Assert.IsTrue(NumberUtils.IsZero((decimal)0));

            Assert.IsTrue(NumberUtils.IsZero((Byte?)0));
            Assert.IsFalse(NumberUtils.IsZero((Byte)2));

            Assert.IsTrue(NumberUtils.IsZero((SByte?)0));
            Assert.IsFalse(NumberUtils.IsZero((SByte)2));
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
            object x = (int)1;
            object y = (double)2;
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
            catch(ArgumentException)
            {}
	    }

        [Test]
        public void BitwiseNot()
        {
            Assert.AreEqual( ~((Byte)2), NumberUtils.BitwiseNot((Byte)2) );
            Assert.AreEqual(~((SByte)2), NumberUtils.BitwiseNot((SByte)2));
            Assert.AreEqual(~((Int16)2), NumberUtils.BitwiseNot((Int16)2));
            Assert.AreEqual(~((UInt16)2), NumberUtils.BitwiseNot((UInt16)2));
            Assert.AreEqual(~((Int32)2), NumberUtils.BitwiseNot((Int32)2));
            Assert.AreEqual(~((UInt32)2), NumberUtils.BitwiseNot((UInt32)2));
            Assert.AreEqual(~((Int64)2), NumberUtils.BitwiseNot((Int64)2));
            Assert.AreEqual(~((UInt64)2), NumberUtils.BitwiseNot((UInt64)2));
            Assert.AreEqual( false, NumberUtils.BitwiseNot(true) );
            try
            {
                NumberUtils.BitwiseNot((double)2.0);
                Assert.Fail();
            }
            catch(ArgumentException)
            {}
        }

        [Test]
        public void BitwiseAnd()
        {
            Assert.AreEqual( ((Byte)2)&((Byte)3), NumberUtils.BitwiseAnd((Byte)2, (Byte)3));
            Assert.AreEqual(((SByte)2) & ((SByte)3), NumberUtils.BitwiseAnd((SByte)2, (SByte)3));
            Assert.AreEqual(((Int16)2) & ((Int16)3), NumberUtils.BitwiseAnd((Int16)2, (Int16)3));
            Assert.AreEqual(((UInt16)2) & ((UInt16)3), NumberUtils.BitwiseAnd((UInt16)2, (UInt16)3));
            Assert.AreEqual(((Int32)2) & ((Int32)3), NumberUtils.BitwiseAnd((Int32)2, (Int32)3));
            Assert.AreEqual(((UInt32)2) & ((UInt32)3), NumberUtils.BitwiseAnd((UInt32)2, (UInt32)3));
            Assert.AreEqual(((Int64)2) & ((Int64)3), NumberUtils.BitwiseAnd((Int64)2, (Int64)3));
            Assert.AreEqual(((UInt64)2) & ((UInt64)3), NumberUtils.BitwiseAnd((UInt64)2, (UInt64)3));
            Assert.AreEqual(((UInt64)2) & ((Byte)3), NumberUtils.BitwiseAnd((UInt64)2, (Byte)3));
            Assert.AreEqual(true, NumberUtils.BitwiseAnd(true, true));
            Assert.AreEqual( false, NumberUtils.BitwiseAnd(false, true) );
            try
            {
                NumberUtils.BitwiseAnd((double)2.0, 3);
                Assert.Fail();
            }
            catch(ArgumentException)
            {}
        }

        [Test]
        public void BitwiseOr()
        {
            Assert.AreEqual( ((Byte)2) | ((Byte)3), NumberUtils.BitwiseOr((Byte)2, (Byte)3));
            Assert.AreEqual(((SByte)2) | ((SByte)3), NumberUtils.BitwiseOr((SByte)2, (SByte)3));
            Assert.AreEqual(((Int16)2) | ((Int16)3), NumberUtils.BitwiseOr((Int16)2, (Int16)3));
            Assert.AreEqual(((UInt16)2) | ((UInt16)3), NumberUtils.BitwiseOr((UInt16)2, (UInt16)3));
            Assert.AreEqual(((Int32)2) | ((Int32)3), NumberUtils.BitwiseOr((Int32)2, (Int32)3));
            Assert.AreEqual(((UInt32)2) | ((UInt32)3), NumberUtils.BitwiseOr((UInt32)2, (UInt32)3));
            Assert.AreEqual(((Int64)2) | ((Int64)3), NumberUtils.BitwiseOr((Int64)2, (Int64)3));
            Assert.AreEqual(((UInt64)2) | ((UInt64)3), NumberUtils.BitwiseOr((UInt64)2, (UInt64)3));
            Assert.AreEqual(((UInt64)2) | ((Byte)3), NumberUtils.BitwiseOr((UInt64)2, (Byte)3));
            Assert.AreEqual(false, NumberUtils.BitwiseOr(false, false));
            Assert.AreEqual(true, NumberUtils.BitwiseOr(false, true));
            try
            {
                NumberUtils.BitwiseAnd((double)2.0, 3);
                Assert.Fail();
            }
            catch(ArgumentException)
            {}
        }

        [Test]
        public void BitwiseXor()
        {
            Assert.AreEqual( ((Byte)2) ^ ((Byte)3), NumberUtils.BitwiseXor((Byte)2, (Byte)3));
            Assert.AreEqual(((SByte)2) ^ ((SByte)3), NumberUtils.BitwiseXor((SByte)2, (SByte)3));
            Assert.AreEqual(((Int16)2) ^ ((Int16)3), NumberUtils.BitwiseXor((Int16)2, (Int16)3));
            Assert.AreEqual(((UInt16)2) ^ ((UInt16)3), NumberUtils.BitwiseXor((UInt16)2, (UInt16)3));
            Assert.AreEqual(((Int32)2) ^ ((Int32)3), NumberUtils.BitwiseXor((Int32)2, (Int32)3));
            Assert.AreEqual(((UInt32)2) ^ ((UInt32)3), NumberUtils.BitwiseXor((UInt32)2, (UInt32)3));
            Assert.AreEqual(((Int64)2) ^ ((Int64)3), NumberUtils.BitwiseXor((Int64)2, (Int64)3));
            Assert.AreEqual(((UInt64)2) ^ ((UInt64)3), NumberUtils.BitwiseXor((UInt64)2, (UInt64)3));
            Assert.AreEqual(((UInt64)2) ^ ((Byte)3), NumberUtils.BitwiseXor((UInt64)2, (Byte)3));
            Assert.AreEqual(false, NumberUtils.BitwiseXor(false, false));
            Assert.AreEqual(false, NumberUtils.BitwiseXor(true, true));
            Assert.AreEqual(true, NumberUtils.BitwiseXor(false, true));
            Assert.AreEqual(true, NumberUtils.BitwiseXor(true, false));
            try
            {
                NumberUtils.BitwiseAnd((double)2.0, 3);
                Assert.Fail();
            }
            catch(ArgumentException)
            {}
        }
	}
}