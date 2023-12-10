#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Spring.Objects;

namespace Spring.Util
{
	/// <summary>
	/// Utilities for testing serializability of objects.
	/// </summary>
	/// <remarks>
	/// Exposes static methods for use in other test cases.
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Simon White (.NET)</author>
	[TestFixture]
	public sealed class SerializationTestUtils
	{
		[Test]
		public void WithNonSerializableObject()
		{
			TestObject o = new TestObject();
			Assert.IsFalse(o is ISerializable);
			Assert.IsFalse(IsSerializable(o));
            Assert.Throws<SerializationException>(() => TrySerialization(o));
		}

		[Test]
		public void WithSerializableObject()
		{
			IPerson p = new SerializablePerson();
			p.Age = 12;
			Assert.IsTrue(p is ISerializable);
			TrySerialization(p);
			Assert.IsTrue(IsSerializable(p));
			IPerson p2 = (IPerson) SerializeAndDeserialize(p);
			Assert.IsTrue(p != p2);
			Assert.AreEqual(12, p2.Age);
		}

		/// <summary>
		/// Attempts to serialize the specified object to an in-memory stream.
		/// </summary>
		/// <param name="o">the object to serialize</param>
		public static void TrySerialization(object o)
		{
			using (Stream stream = new MemoryStream())
			{
				BinaryFormatter bformatter = new BinaryFormatter();
				bformatter.Serialize(stream, o);
			}
		}

		/// <summary>
		/// Tests whether the specified object is serializable.
		/// </summary>
		/// <param name="o">the object to test.</param>
		/// <returns>true if the object is serializable, otherwise false.</returns>
		public static bool IsSerializable(object o)
		{
#pragma warning disable SYSLIB0050 // Type or member is obsolete
			return o == null ? true : o.GetType().IsSerializable;
#pragma warning restore SYSLIB0050
		}

		/// <summary>
		/// Serializes the specified object to an in-memory stream, and returns
		/// the result of deserializing the object stream.
		/// </summary>
		/// <param name="o">the object to use.</param>
		/// <returns>the deserialized object.</returns>
		public static object SerializeAndDeserialize(object o)
		{
			using (Stream stream = new MemoryStream())
			{
				BinaryFormatter bformatter = new BinaryFormatter();
				bformatter.Serialize(stream, o);
				stream.Flush();

				stream.Seek(0, SeekOrigin.Begin);
				object o2 = bformatter.Deserialize(stream);
				return o2;
			}
		}

	}
}
