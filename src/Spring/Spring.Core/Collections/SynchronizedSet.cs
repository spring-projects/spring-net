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
	/// Implements a thread-safe <see cref="Spring.Collections.ISet"/> wrapper.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The implementation is extremely conservative, serializing critical
	/// sections to prevent possible deadlocks, and locking on everything. The
	/// one exception is for enumeration, which is inherently not thread-safe.
	/// For this, you have to <c>lock</c> the <c>SyncRoot</c> object for the
	/// duration of the enumeration.
	/// </p>
	/// </remarks>
	/// <seealso cref="Spring.Collections.ISet"/>
    [Serializable]
    public sealed class SynchronizedSet : Set
	{
		private ISet _mBasisSet;
		private object _mSyncRoot;

		/// <summary>
		/// Constructs a thread-safe <see cref="Spring.Collections.ISet"/>
		/// wrapper.
		/// </summary>
		/// <param name="basisSet">
		/// The <see cref="Spring.Collections.ISet"/> object that this object
		/// will wrap.
		/// </param>
		/// <exception cref="System.NullReferenceException">
		/// If the supplied <paramref name="basisSet"/> ecposes a
		/// <see langword="null"/> <c>SyncRoot</c> value.
		/// </exception>
		public SynchronizedSet(ISet basisSet)
		{
			_mBasisSet = basisSet;
			_mSyncRoot = basisSet.SyncRoot;
			if (_mSyncRoot == null)
			{
				throw new NullReferenceException(
					"The Set you specified returned a null SyncRoot.");
			}
		}

		/// <summary>
		/// Adds the specified element to this set if it is not already present.
		/// </summary>
		/// <param name="element">The object to add to the set.</param>
		/// <returns>
		/// <see langword="true"/> is the object was added,
		/// <see langword="true"/> if the object was already present.
		/// </returns>
		public override sealed bool Add(object element)
		{
			lock (_mSyncRoot)
			{
				return _mBasisSet.Add(element);
			}
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
		public override sealed bool AddAll(ICollection collection)
		{
			if(collection == null)
			{
				return false;
			}
			Set temp;
			lock (collection.SyncRoot)
			{
				temp = new HybridSet(collection);
			}
			lock (_mSyncRoot)
			{
				return _mBasisSet.AddAll(temp);
			}
		}

		/// <summary>
		/// Removes all objects from this set.
		/// </summary>
		public override sealed void Clear()
		{
			lock (_mSyncRoot)
			{
				_mBasisSet.Clear();
			}
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
			lock (_mSyncRoot)
			{
				return _mBasisSet.Contains(element);
			}
		}

		/// <summary>
		/// Returns <see langword="true"/> if the set contains all the
		/// elements in the specified collection.
		/// </summary>
		/// <param name="collection">A collection of objects.</param>
		/// <returns>
		/// <see langword="true"/> if the set contains all the elements in the
		/// specified collection; also <see langword="false"/> if the
		/// supplied <paramref name="collection"/> is <see langword="null"/>.
		/// </returns>
		public override sealed bool ContainsAll(ICollection collection)
		{
			if(collection == null)
			{
				return false;
			}
			Set temp;
			lock (collection.SyncRoot)
			{
				temp = new HybridSet(collection);
			}
			lock (_mSyncRoot)
			{
				return _mBasisSet.ContainsAll(temp);
			}
		}

		/// <summary>
		/// Returns <see langword="true"/> if this set contains no elements.
		/// </summary>
		public override sealed bool IsEmpty
		{
			get
			{
				lock (_mSyncRoot)
				{
					return _mBasisSet.IsEmpty;
				}
			}
		}

		/// <summary>
		/// Removes the specified element from the set.
		/// </summary>
		/// <param name="element">The element to be removed.</param>
		/// <returns>
		/// <see langword="true"/> if the set contained the specified element.
		/// </returns>
		public override sealed bool Remove(object element)
		{
			lock (_mSyncRoot)
			{
				return _mBasisSet.Remove(element);
			}
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
		public override sealed bool RemoveAll(ICollection collection)
		{
			Set temp;
			lock (collection.SyncRoot)
			{
				temp = new HybridSet(collection);
			}
			lock (_mSyncRoot)
			{
				return _mBasisSet.RemoveAll(temp);
			}
		}

		/// <summary>
		/// Retains only the elements in this set that are contained in the
		/// specified collection.
		/// </summary>
		/// <param name="c">
		/// The collection that defines the set of elements to be retained.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if this set changed as a result of this
		/// operation.
		/// </returns>
		public override sealed bool RetainAll(ICollection c)
		{
			Set temp;
			lock (c.SyncRoot)
			{
				temp = new HybridSet(c);
			}
			lock (_mSyncRoot)
			{
				return _mBasisSet.RetainAll(temp);
			}
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
			lock (_mSyncRoot)
			{
				_mBasisSet.CopyTo(array, index);
			}
		}

		/// <summary>
		/// The number of elements currently contained in this collection.
		/// </summary>
		public override sealed int Count
		{
			get
			{
				lock (_mSyncRoot)
				{
					return _mBasisSet.Count;
				}
			}
		}

		/// <summary>
		/// Returns <see langword="true"/> if the
		/// <see cref="Spring.Collections.ISet"/> is synchronized across
		/// threads.
		/// </summary>
		/// <seealso cref="Spring.Collections.Set.IsSynchronized"/>
		public override sealed bool IsSynchronized
		{
			get { return true; }
		}

		/// <summary>
		/// An object that can be used to synchronize this collection to make
		/// it thread-safe.
		/// </summary>
		/// <value>
		/// An object that can be used to synchronize this collection to make
		/// it thread-safe.
		/// </value>
		/// <seealso cref="Spring.Collections.Set.SyncRoot"/>
		public override sealed object SyncRoot
		{
			get { return _mSyncRoot; }
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
		/// Returns a clone of the <see cref="Spring.Collections.ISet"/> instance.
		/// </summary>
		/// <returns>A clone of this object.</returns>
		public override object Clone()
		{
			return new SynchronizedSet((ISet) _mBasisSet.Clone());
		}
	}
}
