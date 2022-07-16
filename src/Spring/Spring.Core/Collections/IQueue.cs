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
	/// A collection designed for holding elements prior to processing.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Besides basic <see cref="ICollection"/> operations,
	/// queues provide additional insertion, extraction, and inspection
	/// operations.
	/// </p>
	/// <p>
	/// Each of these methods exists in two forms: one throws
	/// an exception if the operation fails, the other returns a special
	/// value (either <see lang="null"/> or <see lang="false"/>, depending on the
	/// operation). The latter form of the insert operation is designed
	/// specifically for use with capacity-restricted <see cref="IQueue"/>
	/// implementations; in most implementations, insert operations cannot
	/// fail.
	/// </p>
	/// <p>
	/// Queues typically, but do not necessarily, order elements in a
	/// FIFO (first-in-first-out) manner. Among the exceptions are
	/// priority queues, which order elements according to a supplied
	/// comparator, or the elements' natural ordering, and LIFO queues (or
	/// stacks) which order the elements LIFO (last-in-first-out).
	/// Whatever the ordering used, the head of the queue is that
	/// element which would be removed by a call to
	/// <see cref="Spring.Collections.IQueue.Remove"/> or
	/// <see cref="Spring.Collections.IQueue.Poll"/>. In a FIFO queue, all new
	/// elements are inserted at the tail of the queue. Other kinds of queues may
	/// use different placement rules. Every <see cref="IQueue"/> implementation
	/// must specify its ordering properties.
	/// </p>
	/// <p>
	/// The <see cref="Spring.Collections.IQueue.Offer(object)"/> method inserts an
	/// element if possible, otherwise returning <see lang="false"/>. This differs from the
	/// <see cref="Spring.Collections.IQueue.Add(object)"/> method, which can fail to
	/// add an element only by throwing an exception. The
	/// <see cref="Spring.Collections.IQueue.Offer(object)"/> method is designed for
	/// use when failure is a normal, rather than exceptional occurrence, for example,
	/// in fixed-capacity (or "bounded" queues.
	/// </p>
	/// <p>
	/// The <see cref="Spring.Collections.IQueue.Remove()"/>
	/// <see cref="Spring.Collections.IQueue.Poll()"/> methods remove and
	/// return the head of the queue. Exactly which element is removed from the
	/// queue is a function of the queue's ordering policy, which differs from
	/// implementation to implementation. The
	/// <see cref="Spring.Collections.IQueue.Remove()"/> and
	/// <see cref="Spring.Collections.IQueue.Poll()"/> methods differ only in their
	/// behavior when the queue is empty: the
	/// <see cref="Spring.Collections.IQueue.Remove()"/> method throws an exception,
	/// while the <see cref="Spring.Collections.IQueue.Poll()"/> method returns
	/// <see lang="null"/>.
	/// </p>
	/// <p>
	/// The <see cref="Spring.Collections.IQueue.Element()"/> and
	/// <see cref="Spring.Collections.IQueue.Peek()"/> methods return, but do
	/// not remove, the head of the queue.
	/// </p>
	/// <p>
	/// The <see cref="IQueue"/> interface does not define the blocking queue
	/// methods, which are common in concurrent programming.
	/// </p>
	/// <p>
	/// <see cref="IQueue"/> implementations generally do not allow insertion
	/// of <see lang="null"/> elements, although some implementations, such as
	/// a linked list, do not prohibit the insertion of <see lang="null"/>.
	/// Even in the implementations that permit it, <see lang="null"/> should
	/// not be inserted into a <see cref="IQueue"/>, as <see lang="null"/> is also
	/// used as a special return value by the
	/// <see cref="Spring.Collections.IQueue.Poll()"/> method to
	/// indicate that the queue contains no elements.
	/// </p>
	/// <p>
	/// <see cref="IQueue"/> implementations generally do not define
	/// element-based versions of methods <see cref="System.Object.Equals(object)"/>
	/// and <see cref="System.Object.GetHashCode()"/>, but instead inherit the
	/// identity based versions from the class object, because element-based equality
	/// is not always well-defined for queues with the same elements but different
	/// ordering properties.
	/// </p>
	/// <p>
	/// Based on the back port of JCP JSR-166.
	/// </p>
	/// </remarks>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	public interface IQueue : ICollection
	{
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
		/// <exception cref="InvalidCastException">
		/// If the class of the supplied <paramref name="objectToAdd"/> prevents it
		/// from being added to this queue.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// If the specified element is <see lang="null"/> and this queue does not
		/// permit <see lang="null"/> elements.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If some property of the supplied <paramref name="objectToAdd"/> prevents
		/// it from being added to this queue.
		/// </exception>
		bool Add(object objectToAdd);

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
		bool Offer(object objectToAdd);

		/// <summary>
		/// Retrieves and removes the head of this queue.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This method differs from <see cref="Spring.Collections.IQueue.Poll()"/>
		/// only in that it throws an exception if this queue is empty.
		/// </p>
		/// </remarks>
		/// <returns>
		/// The head of this queue
		/// </returns>
		/// <exception cref="Spring.Collections.NoElementsException">if this queue is empty</exception>
		object Remove();

		/// <summary>
		/// Retrieves and removes the head of this queue,
		/// or returns <see lang="null"/> if this queue is empty.
		/// </summary>
		/// <returns>
		/// The head of this queue, or <see lang="null"/> if this queue is empty.
		/// </returns>
		object Poll();

		/// <summary>
		/// Retrieves, but does not remove, the head of this queue.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This method differs from <see cref="Spring.Collections.IQueue.Peek()"/>
		/// only in that it throws an exception if this queue is empty.
		/// </p>
		/// </remarks>
		/// <returns>
		/// The head of this queue.
		/// </returns>
		/// <exception cref="Spring.Collections.NoElementsException">If this queue is empty.</exception>
		object Element();

		/// <summary>
		/// Retrieves, but does not remove, the head of this queue,
		/// or returns <see lang="null"/> if this queue is empty.
		/// </summary>
		/// <returns>
		/// The head of this queue, or <see lang="null"/> if this queue is empty.
		/// </returns>
		object Peek();

		/// <summary>
		/// Returns <see lang="true"/> if there are no elements in the <see cref="IQueue"/>, <see lang="false"/> otherwise.
		/// </summary>
		bool IsEmpty { get; }
	}
}
