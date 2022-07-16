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
using Spring.Util;

namespace Spring.Collections
{
	/// <summary>
	/// Implements an <see cref="Spring.Collections.ISet"/> based on a sorted
	/// tree.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This gives good performance for operations on very large data-sets,
	/// though not as good - asymptotically - as a
	/// <see cref="Spring.Collections.HashedSet"/>. However, iteration occurs
	/// in order.
	/// </p>
	/// <p>
	/// Elements that you put into this type of collection must implement
	/// <see cref="System.IComparable"/>, and they must actually be comparable.
	/// You can't mix <see cref="System.String"/> and
	/// <see cref="System.Int32"/> values, for example.
	/// </p>
	/// <p>
	/// This <see cref="Spring.Collections.ISet"/> implementation does
	/// <b>not</b> support elements that are <see langword="null"/>.
	/// </p>
	/// </remarks>
	/// <seealso cref="Spring.Collections.ISet"/>
    [Serializable]
    public class SortedSet : DictionarySet
	{
		/// <summary>
		/// Creates a new set instance based on a sorted tree.
		/// </summary>
		public SortedSet()
		{
			InternalDictionary = new SortedList();
		}

		/// <summary>
		/// Creates a new set instance based on a sorted tree using <param name="comparer"/> for ordering.
		/// </summary>
        public SortedSet(IComparer comparer)
        {
            AssertUtils.ArgumentNotNull(comparer, "comparer");
			InternalDictionary = new SortedList(comparer);
        }

		/// <summary>
		/// Creates a new set instance based on a sorted tree and initializes
		/// it based on a collection of elements.
		/// </summary>
		/// <param name="initialValues">
		/// A collection of elements that defines the initial set contents.
		/// </param>
		public SortedSet(ICollection initialValues) : this()
		{
			this.AddAll(initialValues);
		}
	}
}
