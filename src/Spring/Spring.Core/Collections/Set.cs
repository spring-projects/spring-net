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
	/// A collection that contains no duplicate elements.
	/// </summary>
	/// <seealso cref="Spring.Collections.ISet"/>
    [Serializable]
    public abstract class Set : ISet
	{
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
		public virtual ISet Union(ISet setOne)
		{
			ISet resultSet = (ISet) this.Clone();
			if (setOne != null)
			{
				resultSet.AddAll(setOne);
			}
			return resultSet;
		}

		/// <summary>
		/// Performs a "union" of two sets, where all the elements in both are
		/// present.
		/// </summary>
		/// <remarks>
		/// <p>
		/// That is, the element is included if it is in either
		/// <paramref name="setOne"/> or <paramref name="anotherSet"/>. The return
		/// value is a <b>clone</b> of one of the sets (<paramref name="setOne"/>
		/// if it is not <see langword="null"/>) with elements of the other set
		/// added in. Neither of the input sets is modified by the operation.
		/// </p>
		/// </remarks>
		/// <param name="setOne">A set of elements.</param>
		/// <param name="anotherSet">A set of elements.</param>
		/// <returns>
		/// A set containing the union of the input sets;
		/// <see langword="null"/> if both sets are <see langword="null"/>.
		/// </returns>
		public static ISet Union(ISet setOne, ISet anotherSet)
		{
			if (setOne == null && anotherSet == null)
			{
				return null;
			}
			else if (setOne == null)
			{
				return (ISet) anotherSet.Clone();
			}
			else if (anotherSet == null)
			{
				return (ISet) setOne.Clone();
			}
			else
			{
				return setOne.Union(anotherSet);
			}
		}

		/// <summary>
		/// Performs a "union" of two sets, where all the elements in both are
		/// present.
		/// </summary>
		/// <param name="setOne">A set of elements.</param>
		/// <param name="anotherSet">A set of elements.</param>
		/// <returns>
		/// A set containing the union of the input sets;
		/// <see langword="null"/> if both sets are <see langword="null"/>.
		/// </returns>
		/// <seealso cref="Union(ISet, ISet)"/>
		public static Set operator |(Set setOne, Set anotherSet)
		{
			return (Set) Union(setOne, anotherSet);
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
		public virtual ISet Intersect(ISet setOne)
		{
			ISet resultSet = (ISet) this.Clone();
			if (setOne != null)
			{
				resultSet.RetainAll(setOne);
			}
			else
			{
				resultSet.Clear();
			}
			return resultSet;
		}

		/// <summary>
		/// Performs an "intersection" of the two sets, where only the elements
		/// that are present in both sets remain.
		/// </summary>
		/// <remarks>
		/// <p>
		/// That is, the element is included only if it exists in both
		/// <paramref name="setOne"/> and <paramref name="anotherSet"/>. Neither input
		/// object is modified by the operation. The result object is a
		/// <b>clone</b> of one of the input objects (<paramref name="setOne"/>
		/// if it is not <see langword="null"/>) containing the elements from
		/// the intersect operation.
		/// </p>
		/// </remarks>
		/// <param name="setOne">A set of elements.</param>
		/// <param name="anotherSet">A set of elements.</param>
		/// <returns>
		/// The intersection of the two input sets; <see langword="null"/> if
		/// both sets are <see langword="null"/>.
		/// </returns>
		public static ISet Intersect(ISet setOne, ISet anotherSet)
		{
			if (setOne == null && anotherSet == null)
			{
				return null;
			}
			else if (setOne == null)
			{
				return anotherSet.Intersect(setOne);
			}
			else
			{
				return setOne.Intersect(anotherSet);
			}
		}

		/// <summary>
		/// Performs an "intersection" of the two sets, where only the elements
		/// that are present in both sets remain.
		/// </summary>
		/// <param name="setOne">A set of elements.</param>
		/// <param name="anotherSet">A set of elements.</param>
		/// <returns>
		/// The intersection of the two input sets; <see langword="null"/> if
		/// both sets are <see langword="null"/>.
		/// </returns>
		/// <seealso cref="Intersect(ISet, ISet)"/>
		public static Set operator &(Set setOne, Set anotherSet)
		{
			return (Set) Intersect(setOne, anotherSet);
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
		public virtual ISet Minus(ISet setOne)
		{
			ISet resultSet = (ISet) this.Clone();
			if (setOne != null)
			{
				resultSet.RemoveAll(setOne);
			}
			return resultSet;
		}

		/// <summary>
		/// Performs a "minus" of set <paramref name="anotherSet"/> from set
		/// <paramref name="setOne"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This returns a set of all the elements in set
		/// <paramref name="setOne"/>, removing the elements that are also in
		/// set <paramref name="anotherSet"/>. The original sets are not modified
		/// during this operation. The result set is a <b>clone</b> of set
		/// <paramref name="setOne"/> containing the elements from the operation.
		/// </p>
		/// </remarks>
		/// <param name="setOne">A set of elements.</param>
		/// <param name="anotherSet">A set of elements.</param>
		/// <returns>
		/// A set containing
		/// <c><paramref name="setOne"/> - <paramref name="anotherSet"/></c> elements.
		/// <see langword="null"/> if <paramref name="setOne"/> is
		/// <see langword="null"/>.
		/// </returns>
		public static ISet Minus(ISet setOne, ISet anotherSet)
		{
			if (setOne == null)
			{
				return null;
			}
			else
			{
				return setOne.Minus(anotherSet);
			}
		}

		/// <summary>
		/// Performs a "minus" of set <paramref name="anotherSet"/> from set
		/// <paramref name="setOne"/>.
		/// </summary>
		/// <param name="setOne">A set of elements.</param>
		/// <param name="anotherSet">A set of elements.</param>
		/// <returns>
		/// A set containing
		/// <c><paramref name="setOne"/> - <paramref name="anotherSet"/></c> elements.
		/// <see langword="null"/> if <paramref name="setOne"/> is
		/// <see langword="null"/>.
		/// </returns>
		/// <seealso cref="Minus(ISet, ISet)"/>
		public static Set operator -(Set setOne, Set anotherSet)
		{
			return (Set) Minus(setOne, anotherSet);
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
		public virtual ISet ExclusiveOr(ISet setOne)
		{
			ISet resultSet = (ISet) this.Clone();
			foreach (object element in setOne)
			{
				if (resultSet.Contains(element))
				{
					resultSet.Remove(element);
				}
				else
				{
					resultSet.Add(element);
				}
			}
			return resultSet;
		}

		/// <summary>
		/// Performs an "exclusive-or" of the two sets, keeping only those
		/// elements that are in one of the sets, but not in both.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The original sets are not modified during this operation. The
		/// result set is a <b>clone</b> of one of the sets (
		/// <paramref name="setOne"/> if it is not <see langword="null"/>)
		/// containing the elements from the exclusive-or operation.
		/// </p>
		/// </remarks>
		/// <param name="setOne">A set of elements.</param>
		/// <param name="anotherSet">A set of elements.</param>
		/// <returns>
		/// A set containing the result of
		/// <c><paramref name="setOne"/> ^ <paramref name="anotherSet"/></c>.
		/// <see langword="null"/> if both sets are <see langword="null"/>.
		/// </returns>
		public static ISet ExclusiveOr(ISet setOne, ISet anotherSet)
		{
			if (setOne == null && anotherSet == null)
			{
				return null;
			}
			else if (setOne == null)
			{
				return (Set) anotherSet.Clone();
			}
			else if (anotherSet == null)
			{
				return (Set) setOne.Clone();
			}
			else
			{
				return setOne.ExclusiveOr(anotherSet);
			}
		}

		/// <summary>
		/// Performs an "exclusive-or" of the two sets, keeping only those
		/// elements that are in one of the sets, but not in both.
		/// </summary>
		/// <param name="setOne">A set of elements.</param>
		/// <param name="anotherSet">A set of elements.</param>
		/// <returns>
		/// A set containing the result of
		/// <c><paramref name="setOne"/> ^ <paramref name="anotherSet"/></c>.
		/// <see langword="null"/> if both sets are <see langword="null"/>.
		/// </returns>
		/// <seealso cref="ExclusiveOr(ISet, ISet)"/>
		public static Set operator ^(Set setOne, Set anotherSet)
		{
			return (Set) ExclusiveOr(setOne, anotherSet);
		}

		/// <summary>
		/// Adds the specified element to this set if it is not already present.
		/// </summary>
		/// <param name="element">The object to add to the set.</param>
		/// <returns>
		/// <see langword="true"/> is the object was added,
		/// <see langword="true"/> if the object was already present.
		/// </returns>
		public abstract bool Add(object element);

		/// <summary>
		/// Adds all the elements in the specified collection to the set if
		/// they are not already present.
		/// </summary>
		/// <param name="collection">A collection of objects to add to the set.</param>
		/// <returns>
		/// <see langword="true"/> is the set changed as a result of this
		/// operation.
		/// </returns>
		public abstract bool AddAll(ICollection collection);

		/// <summary>
		/// Removes all objects from this set.
		/// </summary>
		public abstract void Clear();

		/// <summary>
		/// Returns <see langword="true"/> if this set contains the specified
		/// element.
		/// </summary>
		/// <param name="element">The element to look for.</param>
		/// <returns>
		/// <see langword="true"/> if this set contains the specified element.
		/// </returns>
		public abstract bool Contains(object element);

		/// <summary>
		/// Returns <see langword="true"/> if the set contains all the
		/// elements in the specified collection.
		/// </summary>
		/// <param name="collection">A collection of objects.</param>
		/// <returns>
		/// <see langword="true"/> if the set contains all the elements in the
		/// specified collection.
		/// </returns>
		public abstract bool ContainsAll(ICollection collection);

		/// <summary>
		/// Returns <see langword="true"/> if this set contains no elements.
		/// </summary>
		public abstract bool IsEmpty { get; }

		/// <summary>
		/// Removes the specified element from the set.
		/// </summary>
		/// <param name="element">The element to be removed.</param>
		/// <returns>
		/// <see langword="true"/> if the set contained the specified element.
		/// </returns>
		public abstract bool Remove(object element);

		/// <summary>
		/// Remove all the specified elements from this set, if they exist in
		/// this set.
		/// </summary>
		/// <param name="collection">A collection of elements to remove.</param>
		/// <returns>
		/// <see langword="true"/> if the set was modified as a result of this
		/// operation.
		/// </returns>
		public abstract bool RemoveAll(ICollection collection);

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
		public abstract bool RetainAll(ICollection collection);

		/// <summary>
		/// Returns a clone of the <see cref="Spring.Collections.ISet"/>
		/// instance.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This will work for derived <see cref="Spring.Collections.ISet"/>
		/// classes if the derived class implements a constructor that takes no
		/// arguments.
		/// </p>
		/// </remarks>
		/// <returns>A clone of this object.</returns>
		public virtual object Clone()
		{
			Set newSet = (Set) Activator.CreateInstance(this.GetType());
			newSet.AddAll(this);
			return newSet;
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
		public abstract void CopyTo(Array array, int index);

		/// <summary>
		/// The number of elements currently contained in this collection.
		/// </summary>
		public abstract int Count { get; }

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
		public abstract bool IsSynchronized { get; }

		/// <summary>
		/// An object that can be used to synchronize this collection to make
		/// it thread-safe.
		/// </summary>
		/// <remarks>
		/// <p>
		/// When implementing this, if your object uses a base object, like an
		/// <see cref="System.Collections.IDictionary"/>, or anything that has
		/// a <c>SyncRoot</c>, return that object instead of "<c>this</c>".
		/// </p>
		/// </remarks>
		/// <value>
		/// An object that can be used to synchronize this collection to make
		/// it thread-safe.
		/// </value>
		public abstract object SyncRoot { get; }

		/// <summary>
		/// Gets an enumerator for the elements in the
		/// <see cref="Spring.Collections.ISet"/>.
		/// </summary>
		/// <returns>
		/// An <see cref="System.Collections.IEnumerator"/> over the elements
		/// in the <see cref="Spring.Collections.ISet"/>.
		/// </returns>
		public abstract IEnumerator GetEnumerator();

		/// <summary>
		/// This method will test the <see cref="Spring.Collections.ISet"/>
		/// against another <see cref="Spring.Collections.ISet"/> for
		/// "equality".
		/// </summary>
		/// <remarks>
		/// <p>
		/// In this case, "equality" means that the two sets contain the same
		/// elements. The "==" and "!=" operators are not overridden by design.
		/// If you wish to check for "equivalent"
		/// <see cref="Spring.Collections.ISet"/> instances, use
		/// <c>Equals()</c>. If you wish to check to see if two references are
		/// actually the same object, use "==" and "!=".
		/// </p>
		/// </remarks>
		/// <param name="obj">
		/// A <see cref="Spring.Collections.ISet"/> object to compare to.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the two sets contain the same elements.
		/// </returns>
		public override bool Equals(object obj)
		{
			Set theOtherSet = obj as Set;
			if (theOtherSet == null || theOtherSet.Count != Count)
			{
				return false;
			}
			else
			{
				foreach (object element in theOtherSet)
				{
					if (!this.Contains(element))
					{
						return false;
					}
				}
				return true;
			}
		}

		/// <summary>
		/// Gets the hashcode for the object.
		/// </summary>
		public override int GetHashCode()
		{
            int hashCode = 0, count = 0;
            foreach (object element in this)
            {
                hashCode += element.GetHashCode();
                count++;
            }
            return count + hashCode;
		}
	}
}
