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
using System.Collections;

using NUnit.Framework;

#endregion

namespace Spring.Collections
{
	/// <summary>
	/// Set of tests for Spring.Util.LinkedList.
	/// </summary>
	/// <author>Simon White</author>
	[TestFixture]
	public sealed class LinkedListTests
	{
		[Test]
		public void AddMultipleObjects()
		{
			LinkedList ll = new LinkedList();
			ll.Add("item1");
			ll.Add("item2");
			ll.Add("item3");
			Assert.IsTrue(ll.Count == 3, "Expected 3 items not " + ll.Count);
			Assert.IsTrue(ll[0].Equals("item1"), "Expected first element to be \"item1\" not " + ll[0]);
			Assert.IsTrue(ll[1].Equals("item2"), "Expected second element to be \"item2\" not " + ll[1]);
		}

		[Test]
		public void Insert()
		{
			LinkedList ll = new LinkedList();
			ll.Add("item1");
			ll.Add("item2");
			ll.Insert(1, "item1andahalf");
			Assert.IsTrue(ll.Count == 3, "Expected 3 items not " + ll.Count);
			Assert.IsTrue(ll[0].Equals("item1"), "Expected first element to be \"item1\" not " + ll[0]);
			Assert.IsTrue(ll[1].Equals("item1andahalf"), "Expected second element to be \"item1andahalf\" not " + ll[1]);
			Assert.IsTrue(ll[2].Equals("item2"), "Expected third element to be \"item2\" not " + ll[0]);
		}

		[Test]
		public void RemoveAt()
		{
			LinkedList ll = new LinkedList();
			ll.Add("item1");
			ll.Add("item2");
			ll.Add("item3");
			ll.RemoveAt(1);
			Assert.IsTrue(ll.Count == 2, "Expected 2 items not " + ll.Count);
			Assert.IsTrue(ll[0].Equals("item1"), "Expected first element to be \"item1\" not " + ll[0]);
			Assert.IsTrue(ll[1].Equals("item3"), "Expected second element to be \"item3\" not " + ll[1]);
		}

		[Test]
		public void RemoveObject()
		{
			string item1 = "item1";
			string item2 = "item2";
			string item3 = "item3";
			LinkedList ll = new LinkedList();
			ll.Add(item1);
			ll.Add(item2);
			ll.Add(item3);
			ll.Remove(item2);
			Assert.IsTrue(ll.Count == 2, "Expected 2 items not " + ll.Count);
			Assert.IsTrue(ll[0].Equals("item1"), "Expected first element to be \"item1\" not " + ll[0]);
			Assert.IsTrue(ll[1].Equals("item3"), "Expected second element to be \"item3\" not " + ll[1]);
		}

		[Test]
		public void RemoveAtOnEmptyLinkedList()
		{
			LinkedList ll = new LinkedList();
			Assert.Throws<ArgumentOutOfRangeException>(() => ll.RemoveAt(0));
		}

		[Test]
		public void Enumerator()
		{
			LinkedList ll = new LinkedList();
			ll.Add("item1");
			ll.Add("item2");
			ll.Add("item3");
			IEnumerator ienum = ll.GetEnumerator();
			Assert.IsTrue(ienum.MoveNext());
			Assert.IsTrue(ienum.Current.Equals("item1"), "Expected first element to be \"item1\" not " + ienum.Current);
			Assert.IsTrue(ienum.MoveNext());
			Assert.IsTrue(ienum.Current.Equals("item2"), "Expected second element to be \"item2\" not " + ienum.Current);
		}

		[Test]
		public void EnumeratorModification()
		{
			LinkedList ll = new LinkedList();
			ll.Add("item1");
			ll.Add("item2");
			ll.Add("item3");
			IEnumerator ienum = ll.GetEnumerator();
			Assert.IsTrue(ienum.MoveNext());
			ll.RemoveAt(0);
            Assert.Throws<InvalidOperationException>(() => ienum.MoveNext());
		}

		[Test]
		public void ConstructorWithIList()
		{
			ArrayList al = new ArrayList();
			al.Add("al1");
			al.Add("al2");
			al.Add("al3");
			LinkedList ll = new LinkedList(al);
			Assert.IsTrue(ll.Count == 3, "Expected 3 items not " + ll.Count);
			Assert.IsTrue(ll[0].Equals("al1"), "Expected first element to be \"al1\" not " + ll[0]);
			Assert.IsTrue(ll[1].Equals("al2"), "Expected second element to be \"al2\" not " + ll[1]);
		}

		[Test]
		public void IndexOfObject()
		{
			string item1 = "item1";
			string item2 = "item2";
			string item3 = "item3";
			LinkedList ll = new LinkedList();
			ll.Add(item1);
			ll.Add(item2);
			ll.Add(item3);
			int index = ll.IndexOf(item2);
			Assert.IsTrue(index == 1, "Expected index of 1 not " + index);
		}

		[Test]
		public void CopyToWithZeroIndex()
		{
			LinkedList ll = new LinkedList();
			ll.Add("item1");
			ll.Add("item2");
			ll.Add("item3");
			string[] strings = new string[3];
			ll.CopyTo(strings, 0);
			Assert.IsTrue(strings[0].Equals("item1"), "Expected first element to be \"item1\" not " + strings[0]);
			Assert.IsTrue(strings[1].Equals("item2"), "Expected second element to be \"item2\" not " + strings[1]);
			Assert.IsTrue(strings[2].Equals("item3"), "Expected third element to be \"item3\" not " + strings[2]);
		}

		[Test]
		public void CopyToWithNonZeroIndex()
		{
			LinkedList ll = new LinkedList();
			ll.Add("item1");
			ll.Add("item2");
			ll.Add("item3");
			string[] strings = new string[5];
			strings[0] = "string1";
			strings[1] = "string2";
			ll.CopyTo(strings, 2);
			Assert.IsTrue(strings[0].Equals("string1"), "Expected first element to be \"string1\" not " + strings[0]);
			Assert.IsTrue(strings[1].Equals("string2"), "Expected second element to be \"string2\" not " + strings[1]);
			Assert.IsTrue(strings[2].Equals("item1"), "Expected third element to be \"item1\" not " + strings[2]);
			Assert.IsTrue(strings[3].Equals("item2"), "Expected fourth element to be \"item2\" not " + strings[3]);
			Assert.IsTrue(strings[4].Equals("item3"), "Expected fifth element to be \"item3\" not " + strings[4]);
		}

		[Test]
		public void CopyToWithNullArray()
		{
			LinkedList ll = new LinkedList();
			ll.Add("item1");
            Assert.Throws<ArgumentNullException>(() => ll.CopyTo(null, 0));
		}

		[Test]
		public void CopyToWithNegativeIndex()
		{
			LinkedList ll = new LinkedList();
			ll.Add("item1");
			string[] strings = new string[1];
            Assert.Throws<ArgumentOutOfRangeException>(() => ll.CopyTo(strings, -1));
		}

		[Test]
		public void CopyToWithIndexGreaterThanArrayLength()
		{
			LinkedList ll = new LinkedList();
			ll.Add("item1");
			string[] strings = new string[1];
            Assert.Throws<ArgumentOutOfRangeException>(() => ll.CopyTo(strings, 2));
		}

		[Test]
		public void CopyToWithInsufficientSizeArray()
		{
			LinkedList ll = new LinkedList();
			ll.Add("item1");
			ll.Add("item2");
			string[] strings = new string[2];
            Assert.Throws<ArgumentException>(() => ll.CopyTo(strings, 1));
		}

		[Test]
		public void Contains()
		{
			LinkedList ll = new LinkedList();
			Assert.IsFalse(ll.Contains("Foo"));
            
			ll = new LinkedList();
			Assert.IsFalse(ll.Contains(null));
			ll.Add("Foo");
			ll.Add(null);
			ll.Add("Bar");
			Assert.IsTrue(ll.Contains(null));
			Assert.IsTrue(ll.Contains("Bar"));
			Assert.IsTrue(ll.Contains("Foo"));

			ll = new LinkedList();
			ll.Add("Foo");
			ll.Add("Bar");
			Assert.IsFalse(ll.Contains(null));
		}
	}
}
