#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

#region Imports



#endregion

namespace Spring.Core
{
	/// <summary>
	/// Interface that can be implemented by objects that should be orderable, e.g. in an
	/// <see cref="System.Collections.ICollection"/>.
	/// </summary>
	/// <remarks>
	/// <p>
    /// The actual order can be interpreted as prioritization, the first object (with the
    /// lowest order value) having the highest priority.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
    /// <author>Aleksandar Seovic (.Net)</author>
    public interface IOrdered
	{

		/// <summary>
		/// Return the order value of this object, where a higher value means greater in
		/// terms of sorting.
		/// </summary>
		/// <remarks>
		/// <p>
        /// Normally starting with 0 or 1, with <see cref="System.Int32.MaxValue"/> indicating
        /// greatest. Same order values will result in arbitrary positions for the affected
        /// objects.
		/// </p>
		/// <p>
        /// Higher value can be interpreted as lower priority, consequently the first object
        /// has highest priority.
		/// </p>
		/// </remarks>
		/// <returns>The order value.</returns>
        int Order
        {
            get;
        }
	}
}