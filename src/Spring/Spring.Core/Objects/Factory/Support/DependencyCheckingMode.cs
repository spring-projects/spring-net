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

namespace Spring.Objects.Factory.Support {

	/// <summary>
	/// The various modes of dependency checking.
    /// </summary>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public enum DependencyCheckingMode
    {
        /// <summary>
        /// DO not do any dependency checking.
        /// </summary>
        None = 0,

        /// <summary>
        /// Check object references.
        /// </summary>
        Objects = 1,

        /// <summary>
        /// Just check primitive (string, int, etc) values.
        /// </summary>
        Simple = 2,

        /// <summary>
        /// Check everything.
        /// </summary>
        All = 3
	}
}
