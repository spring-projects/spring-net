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

using System.Configuration;
using System.Xml;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    /// Default section handler that can handle any configuration section.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Simply returns the configuration section as an <see cref="System.Xml.XmlElement"/>.
    /// </p>
    /// </remarks>
	/// <author>Aleksandar Seovic</author>
    public class DefaultSectionHandler : IConfigurationSectionHandler
    {
        #region Methods
        /// <summary>
        /// Returns the configuration section as an <see cref="System.Xml.XmlElement"/>
        /// </summary>
        /// <param name="parent">
        /// The configuration settings in a corresponding parent
        /// configuration section.
        /// </param>
        /// <param name="configContext">
        /// The configuration context when called from the ASP.NET
        /// configuration system. Otherwise, this parameter is reserved and
        /// is a null reference.
        /// </param>
        /// <param name="section">
        /// The <see cref="System.Xml.XmlNode"/> for the section.
        /// </param>
        /// <returns>Config section as XmlElement.</returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            return section as XmlElement;
        }
        #endregion
    }
}