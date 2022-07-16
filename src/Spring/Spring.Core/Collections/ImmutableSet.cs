/* Copyright � 2002-2011 by Aidant Systems, Inc., and by Jason Smith. */

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
	/// Implements an immutable (read-only)
	/// <see cref="Spring.Collections.ISet"/> wrapper.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Although this class is advertised as immutable, it really isn't.
	/// Anyone with access to the wrapped <see cref="Spring.Collections.ISet"/>
	/// can still change the data. So <see cref="System.Object.GetHashCode"/>
	/// is not implemented for this <see cref="Spring.Collections.ISet"/>, as
	/// is the case for all <see cref="Spring.Collections.ISet"/>
	/// implementations in this library. This design decision was based on the
	/// efficiency of not having to <b>clone</b> the wrapped
	/// <see cref="Spring.Collections.ISet"/> every time you wrap a mutable
	/// <see cref="Spring.Collections.ISet"/>.
	/// </p>
	/// </remarks>
    [Serializable]
    public sealed class ImmutableSet : Set
	{
		private const string ErrorMessage = "Object is immutable.";
		private ISet _mBasisSet;

		internal ISet BasisSet
		{
			get { return _mBasisSet; }
		}

		/// <summary>
		/// Constructs an immutable (read-only)
		/// <see cref="Spring.Collections.ISet"/> wrapper.
		/// </summary>
		/// <param name="basisSet">
		/// The <see cref="Spring.Collections.ISet"/> that is to be wrapped.
		/// </param>
		public ImmutableSet(ISet basisSet)
		{
			_mBasisSet = basisSet;
		}

		/// <summary>
		/// Adds the specified element to this set if it is not already present.
		/// </summary>
		/// <param name="element">The object to add to the set.</param>
		/// <returns>
		/// <see langword="true"/> is the object was added,
		/// <see langword="true"/> if the object was already present.
		/// </returns>
		/// <exception cref="System.NotSupportedException"/>
		public override sealed bool Add(object element)
		{
			throw CreateNotSupportedException();
		}

		/// <summary>
		/// Adds all the elements in the specified collection to the set if
		/// they are not already present.
		/// </summary>
		/// <param name="collection">A collection of objects to add to the set.</param>
		/// <returns>
		/// <see langword="true"/> is the set changed as a result of this
		/// operation.
		/// </returns>
		/// <exception cref="System.NotSupportedException"/>
		public override sealed bool AddAll(ICollection collection)
		{
			throw CreateNotSupportedException();
		}

		/// <summary>
		/// Removes all objects from this set.
		/// </summary>
		/// <exception cref="System.NotSupportedException"/>
		public override sealed void Clear()
		{
			throw CreateNotSupportedException();
		}

		/// <summary>
		/// Returns <see langword="true"/> if this set contains the specified
		/// element.
		/// </summary>
		/// <param name="element">The element to look for.</param>
		/// <returns>
		/// <see langword="true"/> if this set contains the specified element.
		/// </returns>
		public override sealed bool Contains(object element)
		{
			return _mBasisSet.Contains(element);
		}

		/// <summary>
		/// Returns <see langword="true"/> if the set contains all the
		/// elements in the specified collection.
		/// </summary>
		/// <param name="collection">A collection of objects.</param>
		/// <returns>
		/// <see langword="true"/> if the set contains all the elements in the
		/// specified collection.
		/// </returns>
		public override sealed bool ContainsAll(ICollection collection)
		{
			return _mBasisSet.ContainsAll(collection);
		}

		/// <summary>
		/// Returns <see langword="true"/> if this set contains no elements.
		/// </summary>
		public override sealed bool IsEmpty
		{
			get { return _mBasisSet.IsEmpty; }
		}

		/// <summary>
		/// Removes the specified element from the set.
		/// </summary>
		/// <param name="element">The element to be removed.</param>
		/// <returns>
		/// <see langword="true"/> if the set contained the specified element.
		/// </returns>
		/// <exception cref="System.NotSupportedException"/>
		public override sealed bool Remove(object element)
		{
			throw CreateNotSupportedException();
		}

		/// <summary>
		/// Remove all the specified elements from this set, if they exist in
		/// this set.
		/// </summary>
		/// <param name="collection">A collection of elements to remove.</param>
		/// <returns>
		/// <see langword="true"/> if the set was modified as a result of this
		/// operation.
		/// </returns>
		/// <exception cref="System.NotSupportedException"/>
		public override sealed bool RemoveAll(ICollection collection)
		{
			throw CreateNotSupportedException();
		}

		/// <summary>
		/// Retains only the elements in this set that are contained in the
		/// specified collection.
		/// </summary>
		/// <param name="collection">
		/// The collection that defines the set of elements to be retained.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if this set changed as a result of this
		/// operation.
		/// </returns>
		/// <exception cref="System.NotSupportedException"/>
		public override sealed bool RetainAll(ICollection collection)
		{
			throw CreateNotSupportedException();
		}

		private static NotSupportedException CreateNotSupportedException()
		{
			return new NotSupportedException(ImmutableSet.ErrorMessage);
		}

		/// <summary>
		/// Copies the elements in the <see cref="Spring.Collections.ISet"/> to
		/// an array.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The type of array needs to be compatible with the objects in the
		/// <see cref="Spring.Collections.ISet"/>, obviously.
		/// </p>
		/// </remarks>
		/// <param name="array">
		/// An array that will be the target of the copy operation.
		/// </param>
		/// <param name="index">
		/// The zero-based index where copying will start.
		/// </param>
		public override sealed void CopyTo(Array array, int index)
		{
			_mBasisSet.CopyTo(array, index);
		}

		/// <summary>
		/// The number of elements currently contained in this collection.
		/// </summary>
		public override sealed int Count
		{
			get { return _mBasisSet.Count; }
		}

		/// <summary>
		/// Returns <see langword="true"/> if the
		/// <see cref="Spring.Collections.ISet"/> is synchronized across
		/// threads.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Note that enumeration is inherently not thread-safe. Use the
		/// <see cref="SyncRoot"/> to lock the object during enumeration.
		/// </p>
		/// </remarks>
		public override sealed bool IsSynchronized
		{
			get { return _mBasisSet.IsSynchronized; }
		}

		/// <summary>
		/// An object that can be used to synchronize this collection to make
		/// it thread-safe.
		/// </summary>
		/// <value>
		/// An object that can be used to synchronize this collection to make
		/// it thread-safe.
		/// </value>
		public override sealed object SyncRoot
		{
			get { return _mBasisSet.SyncRoot; }
		}

		/// <summary>
		/// Gets an enumerator for the elements in the
		/// <see cref="Spring.Collections.ISet"/>.
		/// </summary>
		/// <returns>
		/// An <see cref="System.Collections.IEnumerator"/> over the elements
		/// in the <see cref="Spring.Collections.ISet"/>.
		/// </returns>
		public override sealed IEnumerator GetEnumerator()
		{
			return _mBasisSet.GetEnumerator();
		}

		/// <summary>
		/// Returns a clone of the <see cref="Spring.Collections.ISet"/>
		/// instance.
		/// </summary>
		/// <returns>A clone of this object.</returns>
		public override sealed object Clone()
		{
			return new ImmutableSet(_mBasisSet);
		}

		/// <summary>
		/// Performs a "union" of the two sets, where all the elements
		/// in both sets are present.
		/// </summary>
		/// <param name="setOne">A collection of elements.</param>
		/// <returns>
		/// A new <see cref="Spring.Collections.ISet"/> containing the union of
		/// this <see cref="Spring.Collections.ISet"/> with the specified
		/// collection. Neither of the input objects is modified by the union.
		/// </returns>
		/// <see cref="Spring.Collections.ISet.Union(ISet)"/>
		public override sealed ISet Union(ISet setOne)
		{
			ISet m = this;
			while (m is ImmutableSet)
			{
				m = ((ImmutableSet) m).BasisSet;
			}
			return new ImmutableSet(m.Union(setOne));
		}

		/// <summary>
		/// Performs an "intersection" of the two sets, where only the elements
		/// that are present in both sets remain.
		/// </summary>
		/// <param name="setOne">A set of elements.</param>
		/// <returns>
		/// The intersection of this set with <paramref name="setOne"/>.
		/// </returns>
		/// <see cref="Spring.Collections.ISet.Intersect(ISet)"/>
		public override sealed ISet Intersect(ISet setOne)
		{
			ISet m = this;
			while (m is ImmutableSet)
			{
				m = ((ImmutableSet) m).BasisSet;
			}
			return new ImmutableSet(m.Intersect(setOne));
		}

		/// <summary>
		/// Performs a "minus" of this set from the <paramref name="setOne"/>
		/// set.
		/// </summary>
		/// <param name="setOne">A set of elements.</param>
		/// <returns>
		/// A set containing the elements from this set with the elements in
		/// <paramref name="setOne"/> removed.
		/// </returns>
		/// <seealso cref="Spring.Collections.ISet.Minus(ISet)"/>
		public override sealed ISet Minus(ISet setOne)
		{
			ISet m = this;
			while (m is ImmutableSet)
			{
				m = ((ImmutableSet) m).BasisSet;
			}
			return new ImmutableSet(m.Minus(setOne));
		}

		/// <summary>
		/// Performs an "exclusive-or" of the two sets, keeping only those
		/// elements that are in one of the sets, but not in both.
		/// </summary>
		/// <param name="setOne">A set of elements.</param>
		/// <returns>
		/// A set containing the result of
		/// <c><paramref name="setOne"/> ^ this</c>.
		/// </returns>
		/// <seealso cref="Spring.Collections.ISet.ExclusiveOr(ISet)"/>
		public override sealed ISet ExclusiveOr(ISet setOne)
		{
			ISet m = this;
			while (m is ImmutableSet)
			{
				m = ((ImmutableSet) m).BasisSet;
			}
			return new ImmutableSet(m.ExclusiveOr(setOne));
		}
	}
}
