#region License

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

#endregion

using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace Spring.Collections
{
	/// <summary> 
	/// An unbounded priority <see cref="Spring.Collections.IQueue"/> based on a priority
	/// heap.  This queue orders elements according to an order specified
	/// at construction time, which is specified either according to their
	/// <i>natural order</i> (see <see cref="System.IComparable"/>, or according to a
	/// <see cref="System.Collections.IComparer"/>, depending on which constructor is
	/// used. A priority queue does not permit <see lang="null"/> elements.
	/// A priority queue relying on natural ordering also does not
	/// permit insertion of non-comparable objects (doing so will result
	/// <see cref="System.InvalidCastException"/>.
	/// 
	/// <p/>
	/// The <i>head</i> of this queue is the <i>lowest</i> element
	/// with respect to the specified ordering.  If multiple elements are
	/// tied for lowest value, the head is one of those elements -- ties are
	/// broken arbitrarily. 
	/// 
	/// <p/>
	/// A priority queue is unbounded, but has an internal
	/// <i>capacity</i> governing the size of an array used to store the
	/// elements on the queue.  It is always at least as large as the queue
	/// size.  As elements are added to a priority queue, its capacity
	/// grows automatically.  The details of the growth policy are not
	/// specified.
	/// 
	/// <p/>
	/// This class and its enumerator implement all of the
	/// <i>optional</i> methods of the <see cref="System.Collections.ICollection"/> and
	/// <see cref="System.Collections.IEnumerator"/> interfaces.
	/// The enumerator provided in method <see cref="System.Collections.IEnumerable.GetEnumerator"/> 
	/// is <b>not</b> guaranteed to traverse the elements of the PriorityQueue in any
	/// particular order.
	/// 
	/// <p/> 
	/// Note that this implementation is <b>NOT</b> synchronized.
	/// Multiple threads should not access a <see cref="Spring.Collections.PriorityQueue"/>
	/// instance concurrently if any of the threads modifies the list
	/// structurally. Instead, use the thread-safe PriorityBlockingQueue.
	/// </summary>
	/// <author>Josh Bloch</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class PriorityQueue : AbstractQueue, ISerializable
	{
		private class PriorityQueueEnumerator : IEnumerator
		{
			private PriorityQueue _enclosingInstance;

			public PriorityQueueEnumerator(PriorityQueue enclosingInstance)
			{
				_enclosingInstance = enclosingInstance;
			}

			public Object Current
			{
				get
				{
					Object result = null;
					if (_cursorIndex <= _enclosingInstance._priorityQueueSize)
					{
						result = _enclosingInstance._queue[_cursorIndex];
					}
					return result;
				}

			}

			/// <summary> 
			/// Index (into queue array) of element to be returned by subsequent call to next.
			/// </summary>
			private int _cursorIndex = 0;

			public bool MoveNext()
			{
				if ( _cursorIndex < _enclosingInstance._priorityQueueSize )
				{
					_cursorIndex++;
					return true;
				}
				return false;
			}

			public void Reset()
			{
				_cursorIndex = 0;
			}
		}


		#region Private Fields

		private const int DEFAULT_INITIAL_CAPACITY = 11;

		/// <summary> 
		/// Priority queue represented as a balanced binary heap: the two children
		/// of queue[n] are queue[2*n] and queue[2*n + 1].  The priority queue is
		/// ordered by comparator, or by the elements' natural ordering, if
		/// comparator is null:  For each node n in the heap and each descendant d
		/// of n, n &lt;= d.
		/// 
		/// The element with the lowest value is in queue[1], assuming the queue is
		/// nonempty.  (A one-based array is used in preference to the traditional
		/// zero-based array to simplify parent and child calculations.)
		/// 
		/// queue.length must be >= 2, even if size == 0.
		/// </summary>
		[NonSerialized] private object[] _queue;

		/// <summary> The number of elements in the priority queue.</summary>
		private int _priorityQueueSize = 0;
		
		/// <summary> 
		/// The comparator, or null if priority queue uses elements'
		/// natural ordering.
		/// </summary>
		private IComparer _comparator;

		/// <summary> 
		/// The number of times this priority queue has been
		/// <i>structurally modified</i>.
		/// </summary>
		[NonSerialized] private int _queueModificationCount = 0;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a <see cref="Spring.Collections.PriorityQueue"/> with the default initial capacity
		/// (11) that orders its elements according to their natural
		/// ordering (using <see cref="System.IComparable"/>).
		/// </summary>
		public PriorityQueue() : this(DEFAULT_INITIAL_CAPACITY, null)
		{
		}

		/// <summary> 
		/// Creates a <see cref="Spring.Collections.PriorityQueue"/> with the specified initial capacity
		/// that orders its elements according to their natural ordering
		/// (using <see cref="System.IComparable"/>).
		/// </summary>
		/// <param name="initialCapacity">the initial capacity for this priority queue.
		/// </param>
		/// <exception cref="System.ArgumentException">if <paramref name="initialCapacity"/> is less than 1.</exception>
		public PriorityQueue(int initialCapacity) : this(initialCapacity, null)
		{
		}

		/// <summary> 
		/// Creates a <see cref="Spring.Collections.PriorityQueue"/> with the specified initial capacity
		/// that orders its elements according to the specified comparator.
		/// </summary>
		/// <param name="initialCapacity">the initial capacity for this priority queue.</param>
		/// <param name="comparator">the comparator used to order this priority queue.
		/// If <see lang="null"/> then the order depends on the elements' natural ordering.
		/// </param>
		/// <exception cref="System.ArgumentException">if <paramref name="initialCapacity"/> is less than 1.</exception>
		public PriorityQueue(int initialCapacity, IComparer comparator)
		{
			if (initialCapacity < 1)
				throw new ArgumentException("initialCapacity");
			_queue = new object[initialCapacity + 1];
			_comparator = comparator;
		}

		/// <summary> 
		/// Creates a <see cref="Spring.Collections.PriorityQueue"/> containing the elements in the
		/// specified collection.  The priority queue has an initial
		/// capacity of 110% of the size of the specified collection or 1
		/// if the collection is empty.  If the specified collection is an
		/// instance of a <see cref="Spring.Collections.PriorityQueue"/>, the priority queue will be sorted
		/// according to the same comparator, or according to its elements'
		/// natural order if the collection is sorted according to its
		/// elements' natural order.  Otherwise, the priority queue is
		/// ordered according to its elements' natural order.
		/// </summary>
		/// <param name="collection">the collection whose elements are to be placed into this priority queue.</param>
		/// <exception cref="System.InvalidCastException">if elements of <paramref name="collection"/> cannot be 
		/// compared to one another according to the priority queue's ordering</exception>
		/// <exception cref="System.ArgumentNullException">if <paramref name="collection"/> or any element with it is
		/// <see lang="null"/>
		/// </exception>
		public PriorityQueue(ICollection collection)
		{
			if ( null == collection )
				throw new ArgumentNullException("collection");
			initializeArray(collection);
			if (collection is PriorityQueue)
			{
				PriorityQueue s = (PriorityQueue) collection;
				_comparator = s.Comparator();
				fillFromSorted(s);
			}
			else
			{
				_comparator = null;
				fillFromUnsorted(collection);
			}
		}

		#endregion

		#region Private Helper Methods

		/// <summary> 
		/// Common code to initialize underlying queue array across
		/// constructors below.
		/// </summary>
		private void initializeArray(ICollection c)
		{
			int size = c.Count;
			int initialCapacity = getQueueSizeBasedOnPercentage(size, 110);
			if (initialCapacity < 1)
				initialCapacity = 1;

			_queue = new Object[initialCapacity + 1];
		}

		/// <summary>
		/// Performs an unsigned bitwise right shift with the specified number
		/// </summary>
		/// <param name="number">Number to operate on</param>
		/// <param name="bits">Amount of bits to shift</param>
		/// <returns>The resulting number from the shift operation</returns>
		private int urShift(int number, int bits)
		{
			if (number >= 0)
				return number >> bits;
			else
				return (number >> bits) + (2 << ~bits);
		}

		/// <summary> 
		/// Establishes the heap invariant assuming the heap
		/// satisfies the invariant except possibly for the leaf-node indexed by k
		/// (which may have a nextExecutionTime less than its parent's).
		/// </summary>
		/// <remarks>
		/// This method functions by "promoting" queue[k] up the hierarchy
		/// (by swapping it with its parent) repeatedly until queue[k]
		/// is greater than or equal to its parent.
		/// </remarks>
		private void fixUp(int k)
		{
			if (_comparator == null)
			{
				while (k > 1)
				{
					int j = k >> 1;
					if (((IComparable) _queue[j]).CompareTo(_queue[k]) <= 0)
						break;
					Object tmp = _queue[j];
					_queue[j] = _queue[k];
					_queue[k] = tmp;
					k = j;
				}
			}
			else
			{
				while (k > 1)
				{
					int j = urShift(k, 1);
					if (_comparator.Compare(_queue[j], _queue[k]) <= 0)
						break;
					Object tmp = _queue[j];
					_queue[j] = _queue[k];
					_queue[k] = tmp;
					k = j;
				}
			}
		}

		/// <summary> 
		/// Establishes the heap invariant (described above) in the subtree
		/// rooted at k, which is assumed to satisfy the heap invariant except
		/// possibly for node k itself (which may be greater than its children).
		/// </summary>
		/// <remarks>
		/// This method functions by "demoting" queue[k] down the hierarchy
		/// (by swapping it with its smaller child) repeatedly until queue[k]
		/// is less than or equal to its children.
		/// </remarks>
		private void fixDown(int k)
		{
			int j;
			if (_comparator == null)
			{
				while ((j = k << 1) <= _priorityQueueSize && (j > 0))
				{
					if (j < _priorityQueueSize && ((IComparable) _queue[j]).CompareTo(_queue[j + 1]) > 0)
						j++; // j indexes smallest kid

					if (((IComparable) _queue[k]).CompareTo(_queue[j]) <= 0)
						break;
					Object tmp = _queue[j];
					_queue[j] = _queue[k];
					_queue[k] = tmp;
					k = j;
				}
			}
			else
			{
				while ((j = k << 1) <= _priorityQueueSize && (j > 0))
				{
					if (j < _priorityQueueSize && _comparator.Compare(_queue[j], _queue[j + 1]) > 0)
						j++; // j indexes smallest kid
					if (_comparator.Compare(_queue[k], _queue[j]) <= 0)
						break;
					Object tmp = _queue[j];
					_queue[j] = _queue[k];
					_queue[k] = tmp;
					k = j;
				}
			}
		}

		/// <summary> 
		/// Establishes the heap invariant in the entire tree,
		/// assuming nothing about the order of the elements prior to the call.
		/// </summary>
		private void heapify()
		{
			for (int i = _priorityQueueSize/2; i >= 1; i--)
				fixDown(i);
		}

		/// <summary>
		/// Returns the <paramref name="percentage"/> of <paramref name="size"/> or <see cref="System.Int32.MaxValue"/> - 1,
		/// whichever is smaller. 
		/// </summary>
		/// <param name="size">base size</param>
		/// <param name="percentage">percentage to return</param>
		/// <returns><paramref name="percentage"/> of <paramref name="size"/></returns>
		private int getQueueSizeBasedOnPercentage(int size, long percentage)
		{
			return (int) Math.Min((size*percentage)/100, Int32.MaxValue - 1);
		}

		/// <summary> 
		/// Initially fill elements of the queue array under the
		/// knowledge that it is sorted or is another <see cref="Spring.Collections.PriorityQueue"/>, in which
		/// case we can just place the elements in the order presented.
		/// </summary>
		private void fillFromSorted(ICollection collection)
		{
			fillArray(collection, true);
		}

		private void fillArray(ICollection collection, bool sorted)
		{
			foreach (object currentObject in collection)
			{
				if (null == currentObject )
					throw new ArgumentNullException("collection", "Cannot add null elements to queue.");
				_queue[++_priorityQueueSize] = currentObject;
			}
			if (! sorted)
			{
				heapify();
			}
		}

		/// <summary> 
		/// Initially fill elements of the queue array that is not to our knowledge
		/// sorted, so we must rearrange the elements to guarantee the heap
		/// invariant.
		/// </summary>
		private void fillFromUnsorted(ICollection collection)
		{
			fillArray(collection, false);
		}

		/// <summary> 
		/// Removes and returns element located at <paramref name="index"/> from queue.  (Recall that the queue
		/// is one-based, so 1 &lt;= i &lt;= size.)
		/// </summary>
		/// <remarks>
		/// Normally this method leaves the elements at positions from 1 up to i-1,
		/// inclusive, untouched.  Under these circumstances, it returns <see lang="null"/>.
		/// Occasionally, in order to maintain the heap invariant, it must move
		/// the last element of the list to some index in the range [2, i-1],
		/// and move the element previously at position (i/2) to position i.
		/// Under these circumstances, this method returns the element that was
		/// previously at the end of the list and is now at some position between
		/// 2 and i-1 inclusive.
		/// </remarks>
		private Object removeAt(int index)
		{
			Debug.Assert(index > 0 && index <= _priorityQueueSize);
			_queueModificationCount++;

			Object moved = _queue[_priorityQueueSize];
			_queue[index] = moved;
			_queue[_priorityQueueSize--] = null;
			if (index <= _priorityQueueSize)
			{
				fixDown(index);
				if (_queue[index] == moved)
				{
					fixUp(index);
					if (_queue[index] != moved)
						return moved;
				}
			}
			return null;
		}

		/// <summary> Resize array, if necessary, to be able to hold given index</summary>
		private void grow(int index)
		{
			int newLength = _queue.Length;
			if (index < newLength)
				return;
			if (index == Int32.MaxValue)
				throw new InvalidOperationException("Cannot grow queue to accomdate index " + index + ".  Doing so would result in a memory overflow.");
			while (newLength <= index)
			{
				if (newLength >= Int32.MaxValue/2)
				{
					newLength = Int32.MaxValue;
				}
				else
				{
					newLength <<= 2;
				}
			}
			Object[] newQueue = new Object[newLength];
			Array.Copy(_queue, 0, newQueue, 0, _queue.Length);
			_queue = newQueue;
		}

		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the Capacity of this queue.  Will equal <see cref="System.Collections.ICollection.Count"/>
		/// </summary>
		public override int Capacity
		{
			get
			{
				return _queue.Length;
			}
		}

		/// <summary>
		/// Returns the queue count.
		/// </summary>
		public override int Count
		{
			get { return _priorityQueueSize; }
		}

		/// <summary> 
		/// Inserts the specified element into this queue if it is possible to do
		/// so immediately without violating capacity restrictions.
		/// </summary>
		/// <remarks>
		/// <p>
		/// When using a capacity-restricted queue, this method is generally
		/// preferable to <see cref="Spring.Collections.IQueue.Add(object)"/>,
		/// which can fail to insert an element only by throwing an exception.
		/// </p>
		/// </remarks>
		/// <param name="objectToAdd">
		/// The element to add.
		/// </param>
		/// <returns>
		/// <see lang="true"/> if the element was added to this queue.
		/// </returns>
		/// <exception cref="System.InvalidCastException">
		/// if the specified element cannot be compared
		/// with elements currently in the priority queue according
		/// to the priority queue's ordering.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// If the element cannot be added at this time due to capacity restrictions.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="objectToAdd"/> is
		/// <see lang="null"/> and this queue does not permit <see lang="null"/>
		/// elements.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If some property of the supplied <paramref name="objectToAdd"/> prevents
		/// it from being added to this queue.
		/// </exception>
		public override bool Offer(Object objectToAdd)
		{
			if (objectToAdd == null)
				throw new ArgumentNullException("objectToAdd");
			_queueModificationCount++;
			++_priorityQueueSize;

			if (_priorityQueueSize >= _queue.Length)
			{
				grow(_priorityQueueSize);
			}
			_queue[_priorityQueueSize] = objectToAdd;
			fixUp(_priorityQueueSize);
			return true;
		}

		/// <summary> 
		/// Retrieves, but does not remove, the head of this queue,
		/// or returns <see lang="null"/> if this queue is empty.
		/// </summary>
		/// <returns> 
		/// The head of this queue, or <see lang="null"/> if this queue is empty.
		/// </returns>
		public override Object Peek()
		{
			if (_priorityQueueSize == 0)
				return null;
			return _queue[1];
		}

		// Collection Methods - the first two override to update docs
		/// <summary> 
		/// Inserts the specified element into this queue if it is possible to do so
		/// immediately without violating capacity restrictions, returning
		/// <see lang="true"/> upon success and throwing an
		/// <see cref="System.InvalidOperationException"/> if no space is
		/// currently available.
		/// </summary>
		/// <param name="objectToAdd">
		/// The element to add.
		/// </param>
		/// <returns> 
		/// <see lang="true"/> if successful.
		/// </returns>
		/// <exception cref="System.InvalidOperationException">
		/// If the element cannot be added at this time due to capacity restrictions.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// If the specified element is <see lang="null"/> and this queue does not
		/// permit <see lang="null"/> elements.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If some property of the supplied <paramref name="objectToAdd"/> prevents
		/// it from being added to this queue.
		/// </exception>
		/// <exception cref="System.InvalidCastException">
		/// if the specified element cannot be compared
		/// with elements currently in the priority queue according
		/// to the priority queue's ordering.
		/// </exception>
		public override bool Add(Object objectToAdd)
		{
			return Offer(objectToAdd);
		}

		/// <summary> 
		/// Removes a single instance of the specified element from this
		/// queue, if it is present.
		/// </summary>
		public virtual bool Remove(Object objectToRemove)
		{
			if (objectToRemove == null)
			{
				return false;
			}

			if (_comparator == null)
			{
				for (int i = 1; i <= _priorityQueueSize; i++)
				{
					if (((IComparable) _queue[i]).CompareTo(objectToRemove) == 0)
					{
						removeAt(i);
						return true;
					}
				}
			}
			else
			{
				for (int i = 1; i <= _priorityQueueSize; i++)
				{
					if (_comparator.Compare(_queue[i], objectToRemove) == 0)
					{
						removeAt(i);
						return true;
					}
				}
			}
			return false;
		}

		/// <summary> 
		/// Returns an <see cref="System.Collections.IEnumerator"/> over the elements in this queue. 
		/// The enumerator does not return the elements in any particular order.
		/// </summary>
		/// <returns> an enumerator over the elements in this queue.</returns>
		public override IEnumerator GetEnumerator()
		{
			return new PriorityQueueEnumerator(this);
		}


		/// <summary> 
		/// Removes all elements from the priority queue.
		/// The queue will be empty after this call returns.
		/// </summary>
		public override void Clear()
		{
			_queueModificationCount++;

			for (int i = 1; i <= _priorityQueueSize; i++)
				_queue[i] = null;

			_priorityQueueSize = 0;
		}

		/// <summary> 
		/// Retrieves and removes the head of this queue,
		/// or returns <see lang="null"/> if this queue is empty.
		/// </summary>
		/// <returns> 
		/// The head of this queue, or <see lang="null"/> if this queue is empty.
		/// </returns>
		public override Object Poll()
		{
			if (_priorityQueueSize == 0)
				return null;
			_queueModificationCount++;

			Object result = _queue[1];
			_queue[1] = _queue[_priorityQueueSize];
			_queue[_priorityQueueSize--] = null;
			if (_priorityQueueSize > 1)
				fixDown(1);
			return result;
		}
		/// <summary>
		/// Queries the queue to see if it contains the specified <paramref name="element"/>
		/// </summary>
		/// <param name="element">element to look for.</param>
		/// <returns><see lang="true"/> if the queue contains the <paramref name="element"/>, 
		/// <see lang="false"/> otherwise.</returns>
		public bool Contains( object element )
		{
			if (element == null)
			{
				for (int num1 = 0; num1 < Capacity; num1++)
				{
					if (_queue[num1] == null)
					{
						return true;
					}
				}
				return false;
			}
			for (int num2 = 0; num2 < Capacity; num2++)
			{
				if (element.Equals(_queue[num2]))
				{
					return true;
				}
			}
			return false;
		}


		/// <summary> Returns the comparator used to order this collection, or <see lang="null"/>
		/// if this collection is sorted according to its elements natural ordering
		/// (using <see cref="System.IComparable"/>).
		/// 
		/// </summary>
		/// <returns> the comparator used to order this collection, or <see lang="null"/>
		/// if this collection is sorted according to its elements natural ordering.
		/// </returns>
		public virtual IComparer Comparator()
		{
			return _comparator;
		}
		#endregion

		#region ISerializable Implementation
		/// <summary> 
		/// Save the state of the instance to a stream (that
		/// is, serialize it).
		/// </summary>
		/// <serialData> The length of the array backing the instance is
		/// emitted (int), followed by all of its elements (each an
		/// <see cref="System.Object"/>) in the proper order.
		/// </serialData>
		/// <param name="serializationInfo">the stream</param>
		/// <param name="context">the context</param>
		public virtual void GetObjectData(SerializationInfo serializationInfo, StreamingContext context)
		{
			Type thisType = this.GetType();
			MemberInfo[] mi = FormatterServices.GetSerializableMembers(thisType, context);
			for (int i = 0; i < mi.Length; i++)
			{
				serializationInfo.AddValue(mi[i].Name, ((FieldInfo) mi[i]).GetValue(this));
			}

			// Write out array length
			serializationInfo.AddValue("Spring.Collections.PriorityQueuedata1", _queue.Length);

			// Write out all elements in the proper order.
			for (int i = 1; i <= _priorityQueueSize; i++)
			{
				serializationInfo.AddValue("Spring.Collections.PriorityQueueData" + i, _queue[i]);
			}
		}

		/// <summary> 
		/// Reconstitute the <see cref="Spring.Collections.PriorityQueue"/> instance from a stream (that is,
		/// deserialize it).
		/// </summary>
		/// <param name="serializationInfo">the stream</param>
		/// <param name="context">the context</param>
		protected PriorityQueue(SerializationInfo serializationInfo, StreamingContext context)
		{
			Type thisType = this.GetType();
			MemberInfo[] mi = FormatterServices.GetSerializableMembers(thisType, context);
			for (int i = 0; i < mi.Length; i++)
			{
				FieldInfo fi = (FieldInfo) mi[i];
				fi.SetValue(this, serializationInfo.GetValue(fi.Name, fi.FieldType));
			}
			int arrayLength = serializationInfo.GetInt32("Spring.Collections.PriorityQueuedata1");
			_queue = new Object[arrayLength];

			for (int i = 1; i <= _priorityQueueSize; i++)
			{
				_queue[i] = serializationInfo.GetValue("Spring.Collections.PriorityQueueData" + i, typeof (Object));
			}
		}
		#endregion

		#region ICollection Implementation

		///<summary>
		///Copies the elements of the <see cref="T:System.Collections.ICollection"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
		///</summary>
		///<param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing. </param>
		///<param name="index">The zero-based index in array at which copying begins. </param>
		///<exception cref="T:System.ArgumentNullException">array is null. </exception>
		///<exception cref="T:System.ArgumentOutOfRangeException">index is less than zero. </exception>
		///<exception cref="T:System.ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"></see> is greater than the available space from index to the end of the destination array. </exception>
		///<exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.ICollection"></see> cannot be cast automatically to the type of the destination array. </exception><filterpriority>2</filterpriority>
		public override void CopyTo(Array array,Int32 index)
		{
			if ( _priorityQueueSize > array.Length ) throw new ArgumentException("Destination array too small.", "array");
			if ( index > array.Length - 1 ) throw new ArgumentException("Starting index outside bounds of target array.", "index");
			if ( index + _priorityQueueSize > array.Length ) throw new IndexOutOfRangeException("Destination array not long enough to begin copying at index " + index + ".");

			for ( int queueElementCount = 1; queueElementCount <= _priorityQueueSize; queueElementCount++)
			{
				array.SetValue(_queue[queueElementCount], index );
				index++;
			}
			Array.Sort(array);
		}

		///<summary>
		///Copies the elements of the <see cref="T:System.Collections.ICollection"></see> to an <see cref="T:System.Array"></see>, starting at index 0.
		///</summary>
		///<param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing. </param>
		///<exception cref="T:System.ArgumentNullException">array is null. </exception>
		///<exception cref="T:System.ArgumentOutOfRangeException">index is less than zero. </exception>
		///<exception cref="T:System.ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"></see> is greater than the available space from index to the end of the destination array. </exception>
		///<exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.ICollection"></see> cannot be cast automatically to the type of the destination array. </exception><filterpriority>2</filterpriority>
		public void CopyTo(Array array)
		{
			CopyTo(array, 0);
		}

		///<summary>
		///Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
		///</summary>
		///<returns>
		///An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
		///</returns>
		public override Object SyncRoot
		{
			get { return null; }

		}

		///<summary>
		///Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe).
		///</summary>
		///<returns>
		///true if access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe); otherwise, false.
		///</returns>
		public override Boolean IsSynchronized
		{
			get { return false; }

		}

		/// <summary>
		/// Returns <see lang="true"/> if there are no elements in the <see cref="IQueue"/>, <see lang="false"/> otherwise.
		/// </summary>
		public override bool IsEmpty
		{
			get { return _priorityQueueSize == 0; }
		}

		#endregion
	}
}
