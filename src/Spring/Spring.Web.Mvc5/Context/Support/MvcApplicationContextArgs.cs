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

using Spring.Core.IO;

namespace Spring.Context.Support
{
    /// <summary>
    /// Encapsulates arguments to the <see cref="Spring.Context.Support.MvcApplicationContext"/> class.
    /// </summary>
    public class MvcApplicationContextArgs : AbstractXmlApplicationContextArgs
    {

        private const bool DEFAULT_CASESENSITIVE = false;
        private const bool DEFAULT_REFRESH = false;

        /// <summary>
        /// Initializes a new instance of the MvcApplicationContextArgs class.
        /// </summary>
        public MvcApplicationContextArgs()
        {
            CaseSensitive = DEFAULT_CASESENSITIVE;
        }

        /// <summary>
        /// Initializes a new instance of the MvcApplicationContextArgs class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parentContext">The parent context.</param>
        /// <param name="configurationLocations">The configuration locations.</param>
        /// <param name="configurationResources">The configuration resources.</param>
        public MvcApplicationContextArgs(string name, IApplicationContext parentContext, string[] configurationLocations, IResource[] configurationResources)
            : this(name, parentContext, configurationLocations, configurationResources, DEFAULT_CASESENSITIVE)
        { }


        /// <summary>
        /// Initializes a new instance of the MvcApplicationContextArgs class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parentContext">The parent context.</param>
        /// <param name="configurationLocations">The configuration locations.</param>
        /// <param name="configurationResources">The configuration resources.</param>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        public MvcApplicationContextArgs(string name, IApplicationContext parentContext, string[] configurationLocations, IResource[] configurationResources, bool caseSensitive)
        {
            Name = name;
            ParentContext = parentContext;
            ConfigurationLocations = configurationLocations;
            ConfigurationResources = configurationResources;
            CaseSensitive = caseSensitive;
            Refresh = DEFAULT_REFRESH;
        }


    }
}
