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
	/// Implements an <see cref="Spring.Collections.ISet"/> that automatically
	/// changes from a list based implementation to a hashtable based
	/// implementation when the size reaches a certain threshold.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This is good if you are unsure about whether you data-set will be tiny
	/// or huge.
	/// </p>
	/// <note>
	/// Because this uses a dual implementation, iteration order is <b>not</b>
	/// guaranteed!
	/// </note>
	/// </remarks>
	/// <seealso cref="Spring.Collections.ISet"/>
	[Serializable]
    public class HybridSet : DictionarySet
	{
		/// <summary>
		/// Creates a new set instance based on either a list or a hash table,
		/// depending on which will be more efficient based on the data-set
		/// size.
		/// </summary>
		public HybridSet()
		{
			InternalDictionary = new HybridDictionary();
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="HybridSet"/> class with a given capacity
        /// </summary>
        /// <param name="size">The size.</param>
	    public HybridSet(int size)
	    {
            InternalDictionary = new HybridDictionary(size);
	    }

	    /// <summary>
		/// Creates a new set instance based on either a list or a hash table,
		/// depending on which will be more efficient based on the data-set
		/// size, and initializes it based on a collection of elements.
		/// </summary>
		/// <param name="initialValues">
		/// A collection of elements that defines the initial set contents.
		/// </param>
		public HybridSet(ICollection initialValues) : this()
		{
			this.AddAll(initialValues);
		}
	}
}
