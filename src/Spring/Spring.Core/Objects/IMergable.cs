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

namespace Spring.Objects
{
    /// <summary>
    /// Interface representing an object whose value set can be merged with that of a parent object.
    /// </summary>
    /// <author>Rob Harrop</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface IMergable
    {
        /// <summary>
        /// Gets a value indicating whether this instance is merge enabled for this instance
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is merge enabled; otherwise, <c>false</c>.
        /// </value>
        bool MergeEnabled {
            get;
        }

        /// <summary>
        /// Merges the current value set with that of the supplied object.
        /// </summary>
        /// <remarks>The supplied object is considered the parent, and values in the
        /// callee's value set must override those of the supplied object.
        /// </remarks>
        /// <param name="parent">The parent object to merge with</param>
        /// <returns>The result of the merge operation</returns>
        /// <exception cref="ArgumentNullException">If the supplied parent is <code>null</code></exception>
        /// <exception cref="InvalidOperationException">If merging is not enabled for this instance,
        /// (i.e. <code>MergeEnabled</code> equals <code>false</code>.</exception>
        object Merge(object parent);
    }
}
