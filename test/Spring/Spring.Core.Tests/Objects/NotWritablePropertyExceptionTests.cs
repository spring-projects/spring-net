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

using NUnit.Framework;
using Spring.Core;

namespace Spring.Objects
{
	/// <summary>
	/// Unit tests for the NotWritablePropertyException class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class NotWritablePropertyExceptionTests
	{
		[Test]
		public void InstantiationSupplyingPropertyTypeAndRootException()
		{
			NotWritablePropertyException ex = new NotWritablePropertyException("Doctor", typeof (TestObject), null);
			Assert.AreEqual("Doctor", ex.OffendingPropertyName);
			Assert.AreEqual(typeof(TestObject), ex.ObjectType);
		}

		[Test]
		public void InstantiationSupplyingNullPropertyTypeAndRootException()
		{
			NotWritablePropertyException ex = new NotWritablePropertyException(null, typeof (TestObject), null);
			Assert.AreEqual(null, ex.OffendingPropertyName);
			Assert.AreEqual(typeof(TestObject), ex.ObjectType);
		}

		[Test]
		public void InstantiationSupplyingNullPropertyNullTypeAndRootException()
		{
			NotWritablePropertyException ex = new NotWritablePropertyException(null, null, null);
			Assert.AreEqual(null, ex.OffendingPropertyName);
			Assert.AreEqual(null, ex.ObjectType);
		}
	}
}