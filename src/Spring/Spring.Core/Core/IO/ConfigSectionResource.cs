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

#region Imports

using System.Text;
using System.Xml;

using Spring.Util;

#endregion

namespace Spring.Core.IO
{
    /// <summary>
    /// Used when retrieving information from the standard .NET configuration
    /// files (App.config / Web.config).
    /// </summary>
    /// <remarks>
    /// <p>
    /// If created with the name of a configuration section, then all methods
    /// aside from the description return <see langword="null"/>,
    /// <see langword="false"/>, or throw an exception. If created with an
    /// <see cref="System.Xml.XmlElement"/>, then the
    /// <see cref="Spring.Core.IO.ConfigSectionResource.InputStream"/> property
    /// will return a corresponding <see cref="System.IO.Stream"/> to parse.
    /// </p>
    /// </remarks>
    /// <author>Mark Pollack</author>
    /// <author>Rick Evans</author>
    public class ConfigSectionResource : AbstractResource
    {
        private XmlElement configElement;
        private string sectionName;

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates new instance of the
        /// <see cref="Spring.Core.IO.ConfigSectionResource"/> class.
        /// </summary>
        /// <param name="configSection">
        /// The actual XML configuration section.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="configSection"/> is <see langword="null"/>.
        /// </exception>
        public ConfigSectionResource(XmlElement configSection)
        {
            AssertUtils.ArgumentNotNull(configSection, "configSection");
            sectionName = configSection.Name;
            configElement = configSection;
        }

        /// <summary>
        /// Creates new instance of the
        /// <see cref="Spring.Core.IO.ConfigSectionResource"/> class.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the configuration section.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="resourceName"/> is
        /// <see langword="null"/> or contains only whitespace character(s).
        /// </exception>
        public ConfigSectionResource(string resourceName) : base(resourceName)
        {
            AssertUtils.ArgumentHasText(resourceName, "resourceName");
            sectionName = GetResourceNameWithoutProtocol(resourceName);
            configElement = (XmlElement) ConfigurationUtils.GetSection(sectionName);
        }

        #endregion

        #region IResource Members

        /// <summary>
        /// Returns the <see cref="System.Uri"/> handle for this resource.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation always returns <see langword="null"/>.
        /// </p>
        /// </remarks>
        /// <value>
        /// <see langword="null"/>.
        /// </value>
        /// <seealso cref="Spring.Core.IO.IResource.Uri"/>
        public override Uri Uri
        {
            get { return null; }
        }

        /// <summary>
        /// Returns a <see cref="System.IO.FileInfo"/> handle for this resource.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation always returns <see langword="null"/>.
        /// </p>
        /// </remarks>
        /// <value>
        /// <see langword="null"/>.
        /// </value>
        /// <seealso cref="Spring.Core.IO.IResource.File"/>
        public override FileInfo File
        {
            get { return null; }
        }

        /// <summary>
        /// Returns a description for this resource (the name of the
        /// configuration section in this case).
        /// </summary>
        /// <value>
        /// A description for this resource.
        /// </value>
        /// <seealso cref="Spring.Core.IO.IResource.Description"/>
        public override string Description
        {
            get
            {
                return string.Format("config [{0}#{1}]", ConfigurationUtils.GetFileName(configElement), sectionName);
            }
        }

        /// <summary>
        /// Does this resource actually exist in physical form?
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation always returns <see langword="false"/>.
        /// </p>
        /// </remarks>
        /// <value>
        /// <see langword="false"/>
        /// </value>
        /// <seealso cref="Spring.Core.IO.IResource.Exists"/>
        /// <seealso cref="Spring.Core.IO.IResource.File"/>
        public override bool Exists
        {
            get { return false; }
        }

        #endregion

        #region IInputStreamSource Members

        /// <summary>
        /// Return an <see cref="System.IO.Stream"/> for this resource.
        /// </summary>
        /// <value>
        /// An <see cref="System.IO.Stream"/>.
        /// </value>
        /// <exception cref="System.IO.IOException">
        /// If the stream could not be opened.
        /// </exception>
        /// <seealso cref="Spring.Core.IO.IInputStreamSource"/>
        public override Stream InputStream
        {
            get
            {
                if (configElement == null)
                {
                    throw new FileNotFoundException(string.Format("Configuration Section '{0}' does not exist", this.sectionName), this.sectionName);
                }
                return new MemoryStream(Encoding.UTF8.GetBytes(configElement.OuterXml));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Exposes the actual <see cref="System.Xml.XmlElement"/> for the
        /// configuration section.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Introduced to accomodate line info tracking during parsing.
        /// </p>
        /// </remarks>
        internal XmlElement ConfigElement
        {
            get { return configElement; }
        }

        #endregion
    }
}
