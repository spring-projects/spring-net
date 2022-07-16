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

namespace Spring.Objects
{
    /// <summary>
    /// Abstracts the state sharing strategy used
    /// by <see cref="Spring.Objects.Factory.Config.SharedStateAwareProcessor"/>
    /// </summary>
    /// <author>Erich Eichinger</author>
    public interface ISharedStateFactory
    {
        /// <summary>
        /// Indicate, whether the given instance can be served by this factory
        /// </summary>
        /// <param name="instance">the instance to serve state</param>
        /// <param name="name">the name of the instance</param>
        /// <returns>
        /// a boolean value indicating, whether state can
        /// be served for the given instance or not.
        /// </returns>
        bool CanProvideState(object instance, string name);

        /// <summary>
        /// Returns the shared state for the given instance.
        /// </summary>
        /// <param name="instance">the instance to obtain shared state for.</param>
        /// <param name="name">the name of this instance</param>
        /// <returns>a dictionary containing shared state for <paramref name="instance"/> or null.</returns>
        IDictionary GetSharedStateFor( object instance, string name );
    }
}
