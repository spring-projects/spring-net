#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Defines additional members web object definitions need to implement.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public interface IWebObjectDefinition
    {
        /// <summary>
        /// Gets or sets scope for the web object (application, session or request)
        /// </summary>
        ObjectScope Scope { get; set; }

        /// <summary>
        /// Returns true if web object is .aspx page.
        /// </summary>
        bool IsPage { get; }

		/// <summary>
		/// Gets the rooted url of the .aspx page, if object definition represents page.
		/// </summary>
		string PageName { get; }
    }
}