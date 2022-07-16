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
	/// Implements an <see cref="Spring.Collections.ISet"/> based on a
	/// hash table.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This will give the best lookup, add, and remove performance for very
	/// large data-sets, but iteration will occur in no particular order.
	/// </p>
	/// </remarks>
	/// <seealso cref="Spring.Collections.ISet"/>
	[Serializable]
    public class HashedSet : DictionarySet
	{
		/// <summary>
		/// Creates a new instance of the <see cref="HashedSet"/> class.
		/// </summary>
		public HashedSet()
		{
			InternalDictionary = new Hashtable();
		}

		/// <summary>
		/// Creates a new instance of the <see cref="HashedSet"/> class, and
		/// initializes it based on a collection of elements.
		/// </summary>
		/// <param name="initialValues">
		/// A collection of elements that defines the initial set contents.
		/// </param>
		public HashedSet(ICollection initialValues) : this()
		{
			this.AddAll(initialValues);
		}
	}
}
