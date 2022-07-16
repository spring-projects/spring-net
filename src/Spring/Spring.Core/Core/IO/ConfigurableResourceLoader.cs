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

using Spring.Util;
using Spring.Reflection.Dynamic;

#endregion

namespace Spring.Core.IO
{
    /// <summary>
    /// Configurable implementation of the
    /// <see cref="Spring.Core.IO.IResourceLoader"/> interface.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This <see cref="Spring.Core.IO.IResourceLoader"/> implementation
    /// supports the configuration of resource access protocols and the
    /// corresponding .NET types that know how to handle those protocols.
    /// </p>
    /// <p>
    /// Basic protocol-to-resource type mappings are also defined by this class,
    /// while others can be added either internally, by application contexts
    /// extending this class, or externally, by the end user configuring the
    /// context.
    /// </p>
    /// <p>
    /// Only one resource type can be defined for each protocol, but multiple
    /// protocols can map to the same resource type (for example, the
    /// <c>"http"</c> and <c>"ftp"</c> protocols both map to the
    /// <see cref="Spring.Core.IO.UrlResource"/> type. The protocols that are
    /// mapped by default can be found in the following list.
    /// </p>
    /// <p>
    /// <list type="bullet">
    /// <item>
    /// <description>assembly</description>
    /// </item>
    /// <item>
    /// <description>config</description>
    /// </item>
    /// <item>
    /// <description>file</description>
    /// </item>
    /// <item>
    /// <description>http</description>
    /// </item>
    /// <item>
    /// <description>https</description>
    /// </item>
    /// </list>
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <seealso cref="Spring.Core.IO.IResource"/>
    /// <seealso cref="Spring.Core.IO.ResourceConverter"/>
    /// <seealso cref="Spring.Context.IApplicationContext"/>
    public class ConfigurableResourceLoader : IResourceLoader
    {
        /// <summary>
        /// The separator between the protocol name and the resource name.
        /// </summary>
        public const string ProtocolSeparator = "://";

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Core.IO.ConfigurableResourceLoader"/> class.
        /// </summary>
        public ConfigurableResourceLoader()
        { }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Core.IO.ConfigurableResourceLoader"/> class using the specified default protocol for unqualified resources.
        /// </summary>
        public ConfigurableResourceLoader(string defaultProtocol)
        {
            AssertUtils.ArgumentNotNull(defaultProtocol, "defaultProtocol");
            this.defaultProtocol = defaultProtocol;
        }

        /// <summary>
        /// The default protocol to use for unqualified resources.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The initial value is <c>"file"</c>.
        /// </p>
        /// </remarks>
        public string DefaultResourceProtocol
        {
            get { return defaultProtocol; }
            set { defaultProtocol = value; }
        }

        /// <summary>
        /// Returns a <see cref="Spring.Core.IO.IResource"/> that has been
        /// mapped to the protocol of the supplied <paramref name="resourceName"/>.
        /// </summary>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns>
        /// A new <see cref="Spring.Core.IO.IResource"/> instance for the
        /// supplied <paramref name="resourceName"/>.
        /// </returns>
        /// <exception cref="System.UriFormatException">
        /// If a <see cref="Spring.Core.IO.IResource"/> <see cref="System.Type"/>
        /// mapping does not exist for the supplied <paramref name="resourceName"/>.
        /// </exception>
        /// <exception cref="System.Exception">
        /// In the case of any errors arising from the instantiation of the
        /// returned <see cref="Spring.Core.IO.IResource"/> instance.
        /// </exception>
        /// <seealso cref="ResourceHandlerRegistry.RegisterResourceHandler(string, Type)"/>
        public IResource GetResource(string resourceName)
        {
            string protocol = GetProtocol(resourceName);
            if (protocol == null)
            {
                protocol = DefaultResourceProtocol;
                resourceName = protocol + ProtocolSeparator + resourceName;
            }

            IDynamicConstructor handler = ResourceHandlerRegistry.GetResourceHandler(protocol);
            if (handler == null)
            {
                throw new UriFormatException("Resource handler for the '" + protocol + "' protocol is not defined.");
            }

            return (IResource) handler.Invoke(new object[] { resourceName });
        }

        /// <summary>
        /// Checks that the supplied <paramref name="resourceName"/> starts
        /// with one of the protocol names currently mapped by this
        /// <see cref="ConfigurableResourceLoader"/> instance.
        /// </summary>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns>
        /// <see langword="true"/> if the supplied
        /// <paramref name="resourceName"/> starts with one of the known
        /// protocols; <see langword="false"/> if not, or if the supplied
        /// <paramref name="resourceName"/> is itself <see langword="null"/>.
        /// </returns>
        public static bool HasProtocol(string resourceName)
        {
            string protocol = GetProtocol(resourceName);
            return protocol != null && ResourceHandlerRegistry.IsHandlerRegistered(protocol);
        }

        /// <summary>
        /// Extracts the protocol name from the supplied
        /// <paramref name="resourceName"/>.
        /// </summary>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns>
        /// The extracted protocol name or <see langword="null"/> if the
        /// supplied <paramref name="resourceName"/> is unqualified (or
        /// is itself <see langword="null"/>).
        /// </returns>
        internal static string GetProtocol(string resourceName)
        {
            if (resourceName == null)
            {
                return null;
            }
            int pos = resourceName.IndexOf(ProtocolSeparator);
            return pos == -1 ? null : resourceName.Substring(0, pos);
        }

        #region Fields

        private string defaultProtocol = "file";

        #endregion
    }
}
