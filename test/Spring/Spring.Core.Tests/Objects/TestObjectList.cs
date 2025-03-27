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

#region Imports

using System.Collections;

#endregion

namespace Spring.Objects
{
	public interface ITestObjectCollection
	{
		int Count { get; }

		bool IsSynchronized { get; }

		object SyncRoot { get; }

		void CopyTo(TestObject[] array, int arrayIndex);

		ITestObjectEnumerator GetEnumerator();
	}

	public interface ITestObjectList : ITestObjectCollection
	{
		bool IsFixedSize { get; }

		bool IsReadOnly { get; }

		TestObject this[int index] { get; set; }

		int Add(TestObject value);

		void Clear();

		bool Contains(TestObject value);

		int IndexOf(TestObject value);

		void Insert(int index, TestObject value);

		void Remove(TestObject value);

		void RemoveAt(int index);
	}

	public interface ITestObjectEnumerator
	{
		TestObject Current { get; }

		bool MoveNext();

		void Reset();
	}

	[Serializable]
	public class TestObjectList : ITestObjectList, IList, ICloneable
	{
		private const int _defaultCapacity = 16;

		private TestObject[] _array = null;
		private int _count = 0;

		[NonSerialized] private int _version = 0;

		public TestObjectList()
		{
			this._array = new TestObject[_defaultCapacity];
		}

		public TestObjectList(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity",
				                                      capacity, "Argument cannot be negative.");

			this._array = new TestObject[capacity];
		}

		public TestObjectList(TestObjectList collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			this._array = new TestObject[collection.Count];
			AddRange(collection);
		}

		public TestObjectList(TestObject[] array)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			this._array = new TestObject[array.Length];
			AddRange(array);
		}

		protected virtual TestObject[] InnerArray
		{
			get { return this._array; }
		}

		public virtual int Capacity
		{
			get { return this._array.Length; }
			set
			{
				if (value == this._array.Length) return;

				if (value < this._count)
					throw new ArgumentOutOfRangeException("Capacity",
					                                      value, "Value cannot be less than Count.");

				if (value == 0)
				{
					this._array = new TestObject[_defaultCapacity];
					return;
				}

				TestObject[] newArray = new TestObject[value];
				Array.Copy(this._array, newArray, this._count);
				this._array = newArray;
			}
		}

		public virtual int Count
		{
			get { return this._count; }
		}

		public virtual bool IsFixedSize
		{
			get { return false; }
		}

		public virtual bool IsReadOnly
		{
			get { return false; }
		}

		public virtual bool IsSynchronized
		{
			get { return false; }
		}

		public virtual bool IsUnique
		{
			get { return false; }
		}

		public virtual TestObject this[int index]
		{
			get
			{
				ValidateIndex(index);
				return this._array[index];
			}
			set
			{
				ValidateIndex(index);
				++this._version;
				this._array[index] = value;
			}
		}

		object IList.this[int index]
		{
			get { return this[index]; }
			set { this[index] = (TestObject) value; }
		}

		public virtual object SyncRoot
		{
			get { return this; }
		}

		public virtual int Add(TestObject value)
		{
			if (this._count == this._array.Length)
				EnsureCapacity(this._count + 1);

			++this._version;
			this._array[this._count] = value;
			return this._count++;
		}

		int IList.Add(object value)
		{
			return Add((TestObject) value);
		}

		public virtual void AddRange(TestObjectList collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			if (collection.Count == 0) return;
			if (this._count + collection.Count > this._array.Length)
				EnsureCapacity(this._count + collection.Count);

			++this._version;
			Array.Copy(collection.InnerArray, 0,
			           this._array, this._count, collection.Count);
			this._count += collection.Count;
		}

		public virtual void AddRange(TestObject[] array)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (array.Length == 0) return;
			if (this._count + array.Length > this._array.Length)
				EnsureCapacity(this._count + array.Length);

			++this._version;
			Array.Copy(array, 0, this._array, this._count, array.Length);
			this._count += array.Length;
		}

		public virtual int BinarySearch(TestObject value)
		{
			return Array.BinarySearch(this._array, 0, this._count, value);
		}

		public virtual void Clear()
		{
			if (this._count == 0) return;

			++this._version;
			Array.Clear(this._array, 0, this._count);
			this._count = 0;
		}

		public virtual object Clone()
		{
			TestObjectList collection = new TestObjectList(this._count);

			Array.Copy(this._array, 0, collection._array, 0, this._count);
			collection._count = this._count;
			collection._version = this._version;

			return collection;
		}

		public bool Contains(TestObject value)
		{
			return (IndexOf(value) >= 0);
		}

		bool IList.Contains(object value)
		{
			return Contains((TestObject) value);
		}

		public virtual void CopyTo(TestObject[] array)
		{
			CheckTargetArray(array, 0);
			Array.Copy(this._array, array, this._count);
		}

		public virtual void CopyTo(TestObject[] array, int arrayIndex)
		{
			CheckTargetArray(array, arrayIndex);
			Array.Copy(this._array, 0, array, arrayIndex, this._count);
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			CheckTargetArray(array, arrayIndex);
			CopyTo((TestObject[]) array, arrayIndex);
		}

		public virtual ITestObjectEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator) GetEnumerator();
		}

		public virtual int IndexOf(TestObject value)
		{
			return Array.IndexOf(this._array, value, 0, this._count);
		}

		int IList.IndexOf(object value)
		{
			return IndexOf((TestObject) value);
		}

		public virtual void Insert(int index, TestObject value)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("index",
				                                      index, "Argument cannot be negative.");

			if (index > this._count)
				throw new ArgumentOutOfRangeException("index",
				                                      index, "Argument cannot exceed Count.");

			if (this._count == this._array.Length)
				EnsureCapacity(this._count + 1);

			++this._version;
			if (index < this._count)
				Array.Copy(this._array, index,
				           this._array, index + 1, this._count - index);

			this._array[index] = value;
			++this._count;
		}

		void IList.Insert(int index, object value)
		{
			Insert(index, (TestObject) value);
		}

		public virtual void Remove(TestObject value)
		{
			int index = IndexOf(value);
			if (index >= 0) RemoveAt(index);
		}

		void IList.Remove(object value)
		{
			Remove((TestObject) value);
		}

		public virtual void RemoveAt(int index)
		{
			ValidateIndex(index);

			++this._version;
			if (index < --this._count)
				Array.Copy(this._array, index + 1,
				           this._array, index, this._count - index);

			this._array[this._count] = null;
		}

		public virtual void RemoveRange(int index, int count)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("index",
				                                      index, "Argument cannot be negative.");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count",
				                                      count, "Argument cannot be negative.");

			if (index + count > this._count)
				throw new ArgumentException(
					"Arguments denote invalid range of elements.");

			if (count == 0) return;

			++this._version;
			this._count -= count;

			if (index < this._count)
				Array.Copy(this._array, index + count,
				           this._array, index, this._count - index);

			Array.Clear(this._array, this._count, count);
		}

		public virtual void Reverse()
		{
			if (this._count <= 1) return;
			++this._version;
			Array.Reverse(this._array, 0, this._count);
		}

		public virtual void Reverse(int index, int count)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("index",
				                                      index, "Argument cannot be negative.");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count",
				                                      count, "Argument cannot be negative.");

			if (index + count > this._count)
				throw new ArgumentException(
					"Arguments denote invalid range of elements.");

			if (count <= 1 || this._count <= 1) return;
			++this._version;
			Array.Reverse(this._array, index, count);
		}

		public virtual void Sort()
		{
			if (this._count <= 1) return;
			++this._version;
			Array.Sort(this._array, 0, this._count);
		}

		public virtual void Sort(IComparer comparer)
		{
			if (this._count <= 1) return;
			++this._version;
			Array.Sort(this._array, 0, this._count, comparer);
		}

		public virtual void Sort(int index, int count, IComparer comparer)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("index",
				                                      index, "Argument cannot be negative.");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count",
				                                      count, "Argument cannot be negative.");

			if (index + count > this._count)
				throw new ArgumentException(
					"Arguments denote invalid range of elements.");

			if (count <= 1 || this._count <= 1) return;
			++this._version;
			Array.Sort(this._array, index, count, comparer);
		}

		public virtual TestObject[] ToArray()
		{
			TestObject[] array = new TestObject[this._count];
			Array.Copy(this._array, array, this._count);
			return array;
		}

		public virtual void TrimToSize()
		{
			Capacity = this._count;
		}

		private void CheckEnumIndex(int index)
		{
			if (index < 0 || index >= this._count)
				throw new InvalidOperationException(
					"Enumerator is not on a collection element.");
		}

		private void CheckEnumVersion(int version)
		{
			if (version != this._version)
				throw new InvalidOperationException(
					"Enumerator invalidated by modification to collection.");
		}

		private void CheckTargetArray(Array array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException("array");
			if (array.Rank > 1)
				throw new ArgumentException(
					"Argument cannot be multidimensional.", "array");

			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException("arrayIndex",
				                                      arrayIndex, "Argument cannot be negative.");
			if (arrayIndex >= array.Length)
				throw new ArgumentException(
					"Argument must be less than array length.", "arrayIndex");

			if (this._count > array.Length - arrayIndex)
				throw new ArgumentException(
					"Argument section must be large enough for collection.", "array");
		}

		private void EnsureCapacity(int minimum)
		{
			int newCapacity = (this._array.Length == 0 ?
				_defaultCapacity : this._array.Length*2);

			if (newCapacity < minimum) newCapacity = minimum;
			Capacity = newCapacity;
		}

		private void ValidateIndex(int index)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("index",
				                                      index, "Argument cannot be negative.");

			if (index >= this._count)
				throw new ArgumentOutOfRangeException("index",
				                                      index, "Argument must be less than Count.");
		}

		[Serializable]
		private sealed class Enumerator : ITestObjectEnumerator, IEnumerator
		{
			private readonly TestObjectList _collection;
			private readonly int _version;
			private int _index;

			internal Enumerator(TestObjectList collection)
			{
				this._collection = collection;
				this._version = collection._version;
				this._index = -1;
			}

			public TestObject Current
			{
				get
				{
					this._collection.CheckEnumIndex(this._index);
					this._collection.CheckEnumVersion(this._version);
					return this._collection[this._index];
				}
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			public bool MoveNext()
			{
				this._collection.CheckEnumVersion(this._version);
				return (++this._index < this._collection.Count);
			}

			public void Reset()
			{
				this._collection.CheckEnumVersion(this._version);
				this._index = -1;
			}
		}
	}
}
