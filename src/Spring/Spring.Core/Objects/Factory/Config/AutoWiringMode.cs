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

namespace Spring.Objects.Factory.Config
{

	/// <summary>
	/// The various autowiring modes.
    /// </summary>
    /// <author>Rick Evans</author>
    [Serializable]
    public enum AutoWiringMode
    {
        /// <summary>
        /// Do not autowire.
        /// </summary>
        No = 0,

        /// <summary>
        /// Autowire by name.
        /// </summary>
        ByName = 1,

        /// <summary>
        /// Autowire by <see cref="System.Type"/>.
        /// </summary>
        ByType = 2,

        /// <summary>
        /// Autowiring by constructor.
        /// </summary>
        Constructor = 3,

        /// <summary>
        /// The autowiring strategy is to be determined by introspection
        /// of the object's <see cref="System.Type"/>.
        /// </summary>
        AutoDetect = 4
    }
}
