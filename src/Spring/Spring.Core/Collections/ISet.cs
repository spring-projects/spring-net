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
	/// <remarks>
	/// <p>
	/// This interface models the mathematical
	/// <see cref="Spring.Collections.ISet"/> abstraction. The order of
	/// elements in a set is dependant on (a)the data-structure implementation, and
	/// (b)the implementation of the various
	/// <see cref="Spring.Collections.ISet"/> methods, and thus is not
	/// guaranteed.
	/// </p>
	/// <p>
	/// <see cref="Spring.Collections.ISet"/> overrides the
	/// <see cref="System.Object.Equals(object)"/> method to test for "equivalency":
	/// whether the two sets contain the same elements. The "==" and "!="
	/// operators are not overridden by design, since it is often desirable to
	/// compare object references for equality.
	/// </p>
	/// <p>
	/// Also, the <see cref="System.Object.GetHashCode"/> method is not
	/// implemented on any of the set implementations, since none of them are
	/// truly immutable. This is by design, and it is the way almost all
	/// collections in the .NET framework function. So as a general rule, don't
	/// store collection objects inside <see cref="Spring.Collections.ISet"/>
	/// instances. You would typically want to use a keyed
	/// <see cref="System.Collections.IDictionary"/> instead.
	/// </p>
	/// <p>
	/// None of the <see cref="Spring.Collections.ISet"/> implementations in
	/// this library are guaranteed to be thread-safe in any way unless wrapped
	/// in a <see cref="Spring.Collections.SynchronizedSet"/>.
	/// </p>
	/// <p>
	/// The following table summarizes the binary operators that are supported
	/// by the <see cref="Spring.Collections.ISet"/> class.
	/// </p>
	/// <list type="table">
	///		<listheader>
	///			<term>Operation</term>
	///			<term>Description</term>
	///			<term>Method</term>
	///		</listheader>
	///		<item>
	///			<term>Union (OR)</term>
	///			<term>
	///			Element included in result if it exists in either <c>A</c> OR
	///			<c>B</c>.
	///			</term>
	///			<term><c>Union()</c></term>
	///		</item>
	///		<item>
	///			<term>Intersection (AND)</term>
	///			<term>
	///			Element included in result if it exists in both <c>A</c> AND
	///			<c>B</c>.
	///			</term>
	///			<term><c>InterSect()</c></term>
	///		</item>
	///		<item>
	///			<term>Exclusive Or (XOR)</term>
	///			<term>
	///			Element included in result if it exists in one, but not both,
	///			of <c>A</c> and <c>B</c>.
	///			</term>
	///			<term><c>ExclusiveOr()</c></term>
	///		</item>
	///		<item>
	///			<term>Minus (n/a)</term>
	///			<term>
	///			Take all the elements in <c>A</c>. Now, if any of them exist in
	///			 <c>B</c>, remove them. Note that unlike the other operators,
	///			 <c>A - B</c> is not the same as <c>B - A</c>.
	///			 </term>
	///			<term><c>Minus()</c></term>
	///		</item>
	/// </list>
	/// </remarks>
	public interface ISet : ICollection, ICloneable
	{
		/// <summary>
		/// Performs a "union" of the two sets, where all the elements
		/// in both sets are present.
		/// </summary>
		/// <remarks>
		/// <p>
		/// That is, the element is included if it is in either
		/// <paramref name="setOne"/> or this set. Neither this set nor the input
		/// set are modified during the operation. The return value is a
		/// <b>clone</b> of this set with the extra elements added in.
		/// </p>
		/// </remarks>
		/// <param name="setOne">A collection of elements.</param>
		/// <returns>
		/// A new <see cref="Spring.Collections.ISet"/> containing the union of
		/// this <see cref="Spring.Collections.ISet"/> with the specified
		/// collection. Neither of the input objects is modified by the union.
		/// </returns>
		ISet Union(ISet setOne);

		/// <summary>
		/// Performs an "intersection" of the two sets, where only the elements
		/// that are present in both sets remain.
		/// </summary>
		/// <remarks>
		/// <p>
		/// That is, the element is included if it exists in both sets. The
		/// <c>Intersect()</c> operation does not modify the input sets. It
		/// returns a <b>clone</b> of this set with the appropriate elements
		/// removed.
		/// </p>
		/// </remarks>
		/// <param name="setOne">A set of elements.</param>
		/// <returns>
		/// The intersection of this set with <paramref name="setOne"/>.
		/// </returns>
		ISet Intersect(ISet setOne);

		/// <summary>
		/// Performs a "minus" of this set from the <paramref name="setOne"/>
		/// set.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This returns a set of all the elements in set
		/// <paramref name="setOne"/>, removing the elements that are also in
		/// this set. The original sets are not modified during this operation.
		/// The result set is a <b>clone</b> of this
		/// <see cref="Spring.Collections.ISet"/> containing the elements from
		/// the operation.
		/// </p>
		/// </remarks>
		/// <param name="setOne">A set of elements.</param>
		/// <returns>
		/// A set containing the elements from this set with the elements in
		/// <paramref name="setOne"/> removed.
		/// </returns>
		ISet Minus(ISet setOne);

		/// <summary>
		/// Performs an "exclusive-or" of the two sets, keeping only those
		/// elements that are in one of the sets, but not in both.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The original sets are not modified during this operation. The
		/// result set is a <b>clone</b> of this set containing the elements
		/// from the exclusive-or operation.
		/// </p>
		/// </remarks>
		/// <param name="setOne">A set of elements.</param>
		/// <returns>
		/// A set containing the result of
		/// <c><paramref name="setOne"/> ^ this</c>.
		/// </returns>
		ISet ExclusiveOr(ISet setOne);

		/// <summary>
		/// Returns <see langword="true"/> if this set contains the specified
		/// element.
		/// </summary>
		/// <param name="element">The element to look for.</param>
		/// <returns>
		/// <see langword="true"/> if this set contains the specified element.
		/// </returns>
		bool Contains(object element);

		/// <summary>
		/// Returns <see langword="true"/> if the set contains all the
		/// elements in the specified collection.
		/// </summary>
		/// <param name="collection">A collection of objects.</param>
		/// <returns>
		/// <see langword="true"/> if the set contains all the elements in the
		/// specified collection.
		/// </returns>
		bool ContainsAll(ICollection collection);

		/// <summary>
		/// Returns <see langword="true"/> if this set contains no elements.
		/// </summary>
		bool IsEmpty { get; }

		/// <summary>
		/// Adds the specified element to this set if it is not already present.
		/// </summary>
		/// <param name="element">The object to add to the set.</param>
		/// <returns>
		/// <see langword="true"/> is the object was added,
		/// <see langword="true"/> if the object was already present.
		/// </returns>
		bool Add(object element);

		/// <summary>
		/// Adds all the elements in the specified collection to the set if
		/// they are not already present.
		/// </summary>
		/// <param name="collection">A collection of objects to add to the set.</param>
		/// <returns>
		/// <see langword="true"/> is the set changed as a result of this
		/// operation.
		/// </returns>
		bool AddAll(ICollection collection);

		/// <summary>
		/// Removes the specified element from the set.
		/// </summary>
		/// <param name="element">The element to be removed.</param>
		/// <returns>
		/// <see langword="true"/> if the set contained the specified element.
		/// </returns>
		bool Remove(object element);

		/// <summary>
		/// Remove all the specified elements from this set, if they exist in
		/// this set.
		/// </summary>
		/// <param name="collection">A collection of elements to remove.</param>
		/// <returns>
		/// <see langword="true"/> if the set was modified as a result of this
		/// operation.
		/// </returns>
		bool RemoveAll(ICollection collection);

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
		bool RetainAll(ICollection collection);

		/// <summary>
		/// Removes all objects from this set.
		/// </summary>
		void Clear();
	}
}
