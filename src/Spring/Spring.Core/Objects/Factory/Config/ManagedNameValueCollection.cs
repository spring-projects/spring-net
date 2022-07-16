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

using System.Collections.Specialized;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Tag class which represent a Spring-managed <see cref="NameValueCollection"/> instance that
    /// supports merging of parent/child definitions.
    /// </summary>
    public class ManagedNameValueCollection: NameValueCollection, IMergable
    {
        private bool mergeEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameValueCollection"/> class that is empty, has the default initial capacity and uses the default case-insensitive hash code provider and the default case-insensitive comparer.
        /// </summary>
        public ManagedNameValueCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Specialized.NameValueCollection"/> class that is empty, has the specified initial capacity and uses the default case-insensitive hash code provider and the default case-insensitive comparer.
        /// </summary>
        /// <param name="capacity">The initial number of entries that the <see cref="T:System.Collections.Specialized.NameValueCollection"/> can contain.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="capacity"/> is less than zero.</exception>
        public ManagedNameValueCollection(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance is merge enabled for this instance
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is merge enabled; otherwise, <c>false</c>.
        /// </value>
        public bool MergeEnabled
        {
            get { return this.mergeEnabled; }
            set { this.mergeEnabled = value; }
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
        public object Merge(object parent)
        {
            if (!this.mergeEnabled)
            {
                throw new InvalidOperationException(
                    "Not allowed to merge when the 'MergeEnabled' property is set to 'false'");
            }
            if (parent == null)
            {
                return this;
            }
            NameValueCollection pDict = parent as NameValueCollection;
            if (pDict == null)
            {
                throw new InvalidOperationException("Cannot merge with object of type [" + parent.GetType() + "]");
            }
            NameValueCollection merged = new ManagedNameValueCollection();
            foreach (string s in pDict.AllKeys)
            {
                merged[s] = pDict.Get(s);
            }
            foreach (string s in this.AllKeys)
            {
                merged[s] = this.Get(s);
            }
            return merged;
        }
    }
}
