#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using Spring.Objects.Factory.Xml;

namespace Spring.ServiceModel.Config
{
    /// <summary>
    /// Namespace parser for the WCF namespace.
    /// </summary>
    /// <author>Bruno Baia</author>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/wcf",
            SchemaLocationAssemblyHint = typeof(WcfNamespaceParser),
            SchemaLocation = "/Spring.ServiceModel.Config/spring-wcf-1.3.xsd")
    ]
    public sealed class WcfNamespaceParser : NamespaceParserSupport
    {
        private const string ChannelFactoryElement = "channelFactory";
        private const string ServiceHostElement = "serviceHost";
        private const string ServiceExporterElement = "serviceExporter";

        /// <summary>
        /// Register the <see cref="IObjectDefinitionParser"/> for the WCF tags.
        /// </summary>
        public override void Init()
        {
            RegisterObjectDefinitionParser(ChannelFactoryElement, new ChannelFactoryObjectDefinitionParser());
        }
    }
}
