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
	/// <version>$Id: NumberUtilsTests.cs,v 1.2 2006/04/09 07:24:51 markpollack Exp $</version>
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
		[ExpectedException(typeof(ArgumentException))]
		public void NegateNull()
		{
			NumberUtils.Negate(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void NegateString()
		{
			NumberUtils.Negate(null);
		}

		[Test]
		public void Negate()
		{
			Assert.AreEqual(-10, NumberUtils.Negate(10));
		}
	}
}