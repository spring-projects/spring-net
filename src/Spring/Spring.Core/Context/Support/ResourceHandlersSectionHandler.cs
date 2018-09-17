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
using System.Globalization;
using System.Xml;

using Spring.Core.IO;
using Spring.Util;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    /// Handler for Spring.NET <c>resourceHandlers</c> config section.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Spring allows registration of custom resource handlers that can be used to load
    /// object definitions from.
    /// </p>
    /// <p>
    /// For example, if you wanted to store your object definitions in a database instead
    /// of in the config file, you could write a custom <see cref="IResource"/> implementation
    /// and register it with Spring using 'db' as a protocol name.
    /// </p>
    /// <p>
    /// Afterwards, you would simply specify resource URI within the <c>context</c> config element
    /// using your custom resource handler.
    /// </p>
    /// </remarks>
    /// <example>
    /// <p>
    /// The following example shows how to configure both this section handler,
    /// how to define custom resource within Spring config section, and how to load
    /// object definitions using custom resource handler:
    /// </p>
    /// <code escaped="true">
    /// <configuration>
    ///     <configSections>
    ///		    <sectionGroup name="spring">
    ///			    <section name="resourceHandlers" type="Spring.Context.Support.ResourceHandlersSectionHandler, Spring.Core"/>
    ///		    </sectionGroup>
    ///     </configSections>
    ///     <spring>
    ///		    <resourceHandlers>
    ///			    <handler protocol="db" type="MyCompany.MyApp.Resources.MyDbResource, MyAssembly"/>
    ///		    </resourceHandlers>
    ///		    <context>
    ///		        <resource uri="db://user:pass@dbName/MyDefinitionsTable"/>
    ///		    </context>
    ///     </spring>
    /// </configuration>
    /// </code>
    /// </example>
    /// <author>Aleksandar Seovic</author>
    /// <seealso cref="IResource"/>
    public class ResourceHandlersSectionHandler : IConfigurationSectionHandler
    {
        private const string HandlerElement = "handler";
        private const string ProtocolAttribute = "protocol";
        private const string TypeAttribute = "type";

        /// <summary>
        /// Registers resource handlers that are specified in
        /// the <c>resources</c> config section with the <see cref="ResourceHandlerRegistry"/>.
        /// </summary>
        /// <param name="parent">
        /// The configuration settings in a corresponding parent
        /// configuration section. Ignored.
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
        /// This method always returns <c>null</c>, because resource handlers are registered
        /// as a sideffect of its execution and there is no need to return anything.
        /// </returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            if (section != null)
            {
                XmlNodeList resourceHandlers = ((XmlElement) section).GetElementsByTagName(HandlerElement);

                foreach (XmlElement handler in resourceHandlers)
                {
                    string protocolName = GetRequiredAttributeValue(handler, ProtocolAttribute, section);
                    string typeName = GetRequiredAttributeValue(handler, TypeAttribute, section);

                    ResourceHandlerRegistry.RegisterResourceHandler(protocolName, typeName);
                }
            }
            return null;
        }

        private static string GetRequiredAttributeValue(XmlElement resourceElement, string requiredAttributeName, XmlNode section)
        {
            XmlAttribute attribute = resourceElement.GetAttributeNode(requiredAttributeName);
            if (attribute == null)
            {
                string errorMessage =
                        string.Format(CultureInfo.InvariantCulture, "The '{0}' attribute is required for the resource handler definition.",
                                      requiredAttributeName);
                throw ConfigurationUtils.CreateConfigurationException(errorMessage, section);
            }
            return attribute.Value;
        }
    }
}