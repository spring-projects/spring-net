#region License

/*
 * Copyright 2002-2005 the original author or authors.
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
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the TypedStringValue class.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	/// <version>$Id: TypedStringValueTests.cs,v 1.4 2007/05/29 20:00:47 markpollack Exp $</version> 
	[TestFixture]
	public sealed class TypedStringValueTests
	{
		[Test]
		public void Instantiation()
		{
			string expectedNow = DateTime.Now.ToShortDateString();
			TypedStringValue tsv = new TypedStringValue(expectedNow, typeof (DateTime));
			Assert.AreEqual(expectedNow, tsv.Value);
			Assert.AreEqual(typeof (DateTime), tsv.TargetType);
            tsv = new TypedStringValue(expectedNow);
            Assert.AreEqual(expectedNow, tsv.Value);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void InstantiationWithNullType()
		{
			new TypedStringValue(string.Empty, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void SetTargetTypePropertyToNullType()
		{
			TypedStringValue tsv = new TypedStringValue(string.Empty, typeof (DateTime));
			tsv.TargetType = null;
		}

		[Test]
		public void IsSerializable()
		{
			Assert.IsTrue(SerializationTestUtils.IsSerializable(new TypedStringValue()),
				"Must be marked as [Serializable].");
		}

		[Test]
		public void Serialization()
		{
			TypedStringValue value = new TypedStringValue();
			Type expectedType = typeof(string);
			value.TargetType = expectedType;
			const string expectedValue = "rilo-kiley";
			value.Value = expectedValue;

			object foo = SerializationTestUtils.SerializeAndDeserialize(value);
			Assert.IsNotNull(foo, "Serialization roundtrip must never result in null.");
			TypedStringValue deser = foo as TypedStringValue;
			Assert.IsNotNull(deser,
				"Serialization roundtrip yielded the wrong Type of object.");
			Assert.AreEqual(expectedType, deser.TargetType,
				"Serialization roundtrip yielded the wrong TargetType.");
			Assert.AreEqual(expectedValue, deser.Value,
				"Serialization roundtrip yielded the wrong Value.");
		}
	}
}