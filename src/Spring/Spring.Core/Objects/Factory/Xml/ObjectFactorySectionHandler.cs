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

using System.Configuration;
using System.Xml;

using Spring.Core.IO;

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Creates an <see cref="Spring.Objects.Factory.IObjectFactory"/> instance
    /// populated with the object definitions supplied in the configuration
    /// section.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Applications will typically want to use an
    /// <see cref="Spring.Context.IApplicationContext"/>, and instantiate it
    /// via the use of the <see cref="Spring.Context.Support.ContextHandler"/>
    /// class (which is similar in functionality to this class). This class is
    /// provided for those times when only an
    /// <see cref="Spring.Objects.Factory.IObjectFactory"/> is required.
    /// </p>
    /// <para>Creates an instance of the class XmlObjectFactory</para>
    /// </remarks>
    /// <example>
    /// <p>
    ///
    /// </p>
    /// </example>
    /// <author>Mark Pollack (.NET)</author>
    public class ObjectFactorySectionHandler : IConfigurationSectionHandler
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ObjectFactorySectionHandler"/>
        /// class.
        /// </summary>
        public ObjectFactorySectionHandler()
        {}

        /// <summary>
        /// Creates a <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// instance populated with the object definitions supplied in the
        /// configuration section.
        /// </summary>
        /// <param name="parent">
        /// The configuration settings in a corresponding parent configuration
        /// section.
        /// </param>
        /// <param name="configContext">
        /// The configuration context when called from the ASP.NET
        /// configuration system. Otherwise, this parameter is reserved and
        /// is <see langword="null"/>.
        /// </param>
        /// <param name="section">
        /// The <see cref="System.Xml.XmlNode"/> for the section.
        /// </param>
        /// <returns>
        /// A <see cref="Spring.Objects.Factory.IObjectFactory"/> instance
        /// populated with the object definitions supplied in the configuration
        /// section.
        /// </returns>
        public object Create(
            object parent, object configContext, XmlNode section)
        {
            return new XmlObjectFactory(new ConfigSectionResource(section as XmlElement), true);
        }
    }
}
