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

namespace Spring.Collections
{
	/// <summary>
	/// This class provides skeletal implementations of some
	/// <see cref="IQueue"/> operations.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The implementations in this class are appropriate when the base
	/// implementation does not allow <see lang="null"/> elements. The methods
	/// <see cref="Spring.Collections.AbstractQueue.Add(object)"/>,
	/// <see cref="Spring.Collections.AbstractQueue.Remove()"/>, and
	/// <see cref="Spring.Collections.AbstractQueue.Element()"/> are based on
	/// the <see cref="Spring.Collections.AbstractQueue.Offer(object)"/>,
	/// <see cref="Spring.Collections.AbstractQueue.Poll()"/>, and
	/// <see cref="Spring.Collections.AbstractQueue.Peek()"/> methods
	/// respectively but throw exceptions instead of indicating failure via
	/// <see lang="false"/> or <see lang="null"/> returns.
	/// <p/>
	/// An <see cref="IQueue"/> implementation that extends this class must
	/// minimally define a method
	/// <see cref="Spring.Collections.AbstractQueue.Offer(object)"/> which does
	/// not permit the insertion of <see lang="null"/> elements, along with methods
	/// <see cref="Spring.Collections.AbstractQueue.Poll()"/>, and
	/// <see cref="Spring.Collections.AbstractQueue.Peek()"/>. Typically,
	/// additional methods will be overridden as well. If these requirements
	/// cannot be met, consider instead subclassing
	/// <see cref="System.Collections.CollectionBase"/>}.
	/// </p>
	/// </remarks>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public abstract class AbstractQueue : IQueue
	{
		/// <summary>
		/// Creates a new instance of the <see cref="Spring.Collections.AbstractQueue"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an abstract class, and as such has no publicly
		/// visible constructors.
		/// </p>
		/// </remarks>
		protected AbstractQueue()
		{}

		/// <summary>
		/// Inserts the specified element into this queue if it is possible
		/// to do so immediately without violating capacity restrictions.
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
		public virtual bool Add(object objectToAdd)
		{
			if(Offer(objectToAdd))
			{
				return true;
			}
			else
			{
				throw new InvalidOperationException("Queue full.");
			}
		}

		/// <summary>
		/// Retrieves and removes the head of this queue.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This method differs from
		/// <see cref="Spring.Collections.AbstractQueue.Poll()"/> only in that
		/// it throws an exception if this queue is empty.
		/// </p>
		/// </remarks>
		/// <returns>
		/// The head of this queue
		/// </returns>
		/// <exception cref="NoElementsException">
		/// If this queue is empty.
		/// </exception>
		public virtual object Remove()
		{
			object element = Poll();
			if(element != null)
			{
				return element;
			}
			else
			{
				throw new NoElementsException("Queue is empty.");
			}
		}


		/// <summary>
		/// Retrieves, but does not remove, the head of this queue.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This method differs from <see cref="Spring.Collections.AbstractQueue.Peek()"/>
		/// only in that it throws an exception if this queue is empty.
		/// </p>
		/// <p>
		/// ALso note that this implementation returns the result of
		/// <see cref="Spring.Collections.AbstractQueue.Peek()"/> unless the queue
		/// is empty.
		/// </p>
		/// </remarks>
		/// <returns>The head of this queue.</returns>
		/// <exception cref="NoElementsException">
		/// If this queue is empty.
		/// </exception>
		public virtual object Element()
		{
			object element = Peek();
			if(element != null)
			{
				return element;
			}
			else
			{
				throw new NoElementsException("Queue is empty.");
			}
		}

		/// <summary>
		/// Removes all of the elements from this queue.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The queue will be empty after this call returns.
		/// </p>
		/// <p>
		/// This implementation repeatedly invokes
		/// <see cref="Spring.Collections.AbstractQueue.Poll()"/> until it
		/// returns <see lang="null"/>.
		/// </p>
		/// </remarks>
		public virtual void Clear()
		{
			while(Poll() != null)
			{
				;
			}
		}

		/// <summary>
		/// Adds all of the elements in the supplied <paramref name="collection"/>
		/// to this queue.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Attempts to
		/// <see cref="Spring.Collections.AbstractQueue.AddAll(ICollection)"/>
		/// of a queue to itself result in <see cref="ArgumentException"/>.
		/// Further, the behavior of this operation is undefined if the specified
		/// collection is modified while the operation is in progress.
		/// </p>
		/// <p>
		/// This implementation iterates over the specified collection,
		/// and adds each element returned by the iterator to this queue, in turn.
		/// An exception encountered while trying to add an element (including,
		/// in particular, a <see lang="null"/> element) may result in only some
		/// of the elements having been successfully added when the associated
		/// exception is thrown.
		/// </p>
		/// </remarks>
		/// <param name="collection">
		/// The collection containing the elements to be added to this queue.
		/// </param>
		/// <returns>
		/// <see lang="true"/> if this queue changed as a result of the call.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="collection"/> or any one of its elements are <see lang="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If the collection is the current <see cref="Spring.Collections.IQueue"/> or
		/// the collection size is greater than the queue capacity.
		/// </exception>
		public virtual bool AddAll(ICollection collection)
		{
			if(collection == null)
			{
				throw new ArgumentNullException("Collection cannot be null.");
			}
			if(collection == this)
			{
				throw new ArgumentException();
			}
			if(collection.Count > Capacity)
			{
				throw new ArgumentException("Collcation size greater than queue capacity.");
			}
			bool modified = false;
			foreach(object element in collection)
			{
				if(element == null)
				{
					throw new ArgumentNullException("Cannot add null elements to this queue.");
				}
				else if(Add(element))
				{
					modified = true;
				}
			}
			return modified;
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
		/// <exception cref="System.InvalidOperationException">
		/// If the element cannot be added at this time due to capacity restrictions.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="objectToAdd"/> is
		/// <see lang="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If some property of the supplied <paramref name="objectToAdd"/> prevents
		/// it from being added to this queue.
		/// </exception>
		public abstract bool Offer(object objectToAdd);

		/// <summary>
		/// Retrieves, but does not remove, the head of this queue,
		/// or returns <see lang="null"/> if this queue is empty.
		/// </summary>
		/// <returns>
		/// The head of this queue, or <see lang="null"/> if this queue is empty.
		/// </returns>
		public abstract object Peek();

		/// <summary>
		/// Retrieves and removes the head of this queue,
		/// or returns <see lang="null"/> if this queue is empty.
		/// </summary>
		/// <returns>
		/// The head of this queue, or <see lang="null"/> if this queue is empty.
		/// </returns>
		public abstract object Poll();

		/// <summary>
		/// Returns <see lang="true"/> if there are no elements in the <see cref="IQueue"/>, <see lang="false"/> otherwise.
		/// </summary>
		public abstract bool IsEmpty { get; }

		/// <summary>
		/// Returns the current capacity of this queue.
		/// </summary>
		public abstract int Capacity { get; }

		///<summary>
		///Copies the elements of the <see cref="T:System.Collections.ICollection"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
		///</summary>
		///<param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing. </param>
		///<param name="index">The zero-based index in array at which copying begins. </param>
		///<exception cref="T:System.ArgumentNullException">array is null. </exception>
		///<exception cref="T:System.ArgumentOutOfRangeException">index is less than zero. </exception>
		///<exception cref="T:System.ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"></see> is greater than the available space from index to the end of the destination array. </exception>
		///<exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.ICollection"></see> cannot be cast automatically to the type of the destination array. </exception><filterpriority>2</filterpriority>
		public abstract void CopyTo(Array array, int index);

		///<summary>
		///Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"></see>.
		///</summary>
		///<returns>
		///The number of elements contained in the <see cref="T:System.Collections.ICollection"></see>.
		///</returns>
		public abstract int Count { get; }

		///<summary>
		///Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
		///</summary>
		///<returns>
		///An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
		///</returns>
		public abstract object SyncRoot { get; }

		///<summary>
		///Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe).
		///</summary>
		///<returns>
		///true if access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe); otherwise, false.
		///</returns>
		public abstract bool IsSynchronized { get; }

		///<summary>
		///Returns an enumerator that iterates through a collection.
		///</summary>
		///<returns>
		///An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
		///</returns>
		public abstract IEnumerator GetEnumerator();
	}
}
