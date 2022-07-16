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
using System.Collections.Specialized;

namespace Spring.Collections
{
	/// <summary>
	/// Implements a <see cref="Spring.Collections.ISet"/> based on a list.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Performance is much better for very small lists than either
	/// <see cref="Spring.Collections.HashedSet"/> or <see cref="Spring.Collections.SortedSet"/>.
	/// However, performance degrades rapidly as the data-set gets bigger. Use a
	/// <see cref="Spring.Collections.HybridSet"/> instead if you are not sure your data-set
	/// will always remain very small. Iteration produces elements in the order they were added.
	/// However, element order is not guaranteed to be maintained by the various
	/// <see cref="Spring.Collections.ISet"/> mathematical operators.
	/// </p>
	/// </remarks>
    [Serializable]
    public class ListSet : DictionarySet
	{
		/// <summary>
		/// Creates a new set instance based on a list.
		/// </summary>
		public ListSet()
		{
			InternalDictionary = new ListDictionary();
		}

		/// <summary>
		/// Creates a new set instance based on a list and initializes it based on a
		/// collection of elements.
		/// </summary>
		/// <param name="initialValues">
		/// A collection of elements that defines the initial set contents.
		/// </param>
		public ListSet(ICollection initialValues) : this()
		{
			this.AddAll(initialValues);
		}
	}
}
