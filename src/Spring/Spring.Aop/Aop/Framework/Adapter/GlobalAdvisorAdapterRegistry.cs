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

namespace Spring.Aop.Framework.Adapter
{
    /// <summary>
    /// Provides Singleton-style access to the default
    /// <see cref="IAdvisorAdapterRegistry"/> instance.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    [Serializable]
    public sealed class GlobalAdvisorAdapterRegistry : DefaultAdvisorAdapterRegistry
    {
        private static readonly GlobalAdvisorAdapterRegistry instance
            = new GlobalAdvisorAdapterRegistry();

        /// <summary>
        /// The default <see cref="IAdvisorAdapterRegistry"/> instance.
        /// </summary>
        public static GlobalAdvisorAdapterRegistry Instance
        {
            get { return instance; }
        }

        #region Constructor (s) / Destructor

        // CLOVER:OFF

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Aop.Framework.Adapter.GlobalAdvisorAdapterRegistry"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This contructor is marked as <see langword="private"/> to enforce the
        /// Singleton pattern
        /// </p>
        /// </remarks>
        private GlobalAdvisorAdapterRegistry()
        {
        }

        // CLOVER:ON

        #endregion
    }
}
