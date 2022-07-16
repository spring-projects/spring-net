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
using Spring.Collections;
using Spring.Core;
using Spring.Util;

namespace Spring.Objects.Support
{
    /// <summary>
    /// Convenience base class for <see cref="ISharedStateFactory"/> implementations.
    /// </summary>
    public abstract class AbstractSharedStateFactory : ISharedStateFactory, IOrdered
    {
        private bool _caseSensitiveState;
        private int _order = Int32.MaxValue;
        private readonly IDictionary _sharedStateCache = new Hashtable();

        /// <summary>
        /// Create shared state dictionaries case-sensitive or case-insensitive?
        /// </summary>
        public bool CaseSensitiveState
        {
            get { return _caseSensitiveState; }
            set { _caseSensitiveState = value; }
        }

        /// <summary>
        /// Gets a dictionary acc. to the type of <paramref name="instance"/>.
        /// If no dictionary is found, create it according to <see cref="CaseSensitiveState"/>
        /// </summary>
        /// <param name="instance">the instance to obtain shared state for</param>
        /// <param name="name">the name of the instance.</param>
        /// <returns>
        /// A dictionary containing the <paramref name="instance"/>'s state,
        /// or null if no state can be served by this provider.
        /// </returns>
        public IDictionary GetSharedStateFor( object instance, string name )
        {
            AssertUtils.ArgumentNotNull(instance, "instance");

            if (!CanProvideState(instance, name))
            {
                return null;
            }

            object key = GetKey(instance, name);
            if (key == null)
            {
                return null;
            }

            IDictionary sharedState = (IDictionary) _sharedStateCache[key];
            if (sharedState == null)
            {
                lock(_sharedStateCache)
                {
                    sharedState = (IDictionary) _sharedStateCache[key];
                    if (sharedState == null)
                    {
                        sharedState = CreateSharedStateDictionary(key);
                        _sharedStateCache[key] = sharedState;
                    }
                }
            }
            return sharedState;
        }

        /// <summary>
        /// A number indicating the priority of this <see cref="AbstractSharedStateFactory"/> (<see cref="IOrdered"/> for more).
        /// </summary>
        public virtual int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        /// <summary>
        /// Creates a dictionary to hold the shared state identified by <paramref name="key"/>.
        /// </summary>
        /// <param name="key">a key to create the dictionary for.</param>
        /// <returns>a dictionary according to <paramref name="key"/> and <see cref="CaseSensitiveState"/>.</returns>
        protected virtual IDictionary CreateSharedStateDictionary(object key)
        {
            return _caseSensitiveState ? new Hashtable() : new CaseInsensitiveHashtable();
        }

        /// <summary>
        /// Indicate, whether the given instance will be served by this provider
        /// </summary>
        /// <param name="instance">the instance to serve state</param>
        /// <param name="name">the name of the instance</param>
        /// <returns>
        /// a boolean value indicating, whether state shall
        /// be resolved for the given instance or not.
        /// </returns>
        public virtual bool CanProvideState(object instance, string name)
        {
            return true;
        }

        /// <summary>
        /// Create the key used for obtaining the state dictionary for <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">the instance to create the key for</param>
        /// <param name="name">the name of the instance.</param>
        /// <returns>
        /// the key identifying the state dictionary to be used for <paramref name="instance"/>
        /// or null, if this state manager doesn't serve the given instance.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Implementations may choose to return null from this method to indicate,
        /// that they won't serve state for the given instance.
        /// </para>
        /// <para>
        /// <b>Note:</b>Keys returned by this method are always treated case-sensitive!
        /// </para>
        /// </remarks>
        protected abstract object GetKey(object instance, string name);
    }
}
