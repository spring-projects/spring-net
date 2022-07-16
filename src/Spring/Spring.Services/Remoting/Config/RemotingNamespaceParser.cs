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

using System.Xml;

using Spring.Core.TypeResolution;
using Spring.Objects;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Remoting.Config
{
    /// <summary>
    /// Implementation of the custom configuration parser for remoting definitions.
    /// </summary>
    /// <author>Bruno Baia</author>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/remoting",
            SchemaLocationAssemblyHint = typeof(RemotingNamespaceParser),
            SchemaLocation = "/Spring.Remoting.Config/spring-remoting-1.1.xsd")
    ]
    public sealed class RemotingNamespaceParser  : ObjectsNamespaceParser
    {
        private const string RemotingTypePrefix = "remoting: ";

        static RemotingNamespaceParser()
        {
            TypeRegistry.RegisterType(
                RemotingTypePrefix + RemotingConfigurerConstants.RemotingConfigurerElement,
                typeof(RemotingConfigurer));
            TypeRegistry.RegisterType(
                RemotingTypePrefix + SaoFactoryObjectConstants.SaoFactoryObjectElement,
                typeof(SaoFactoryObject));
            TypeRegistry.RegisterType(
                RemotingTypePrefix + CaoFactoryObjectConstants.CaoFactoryObjectElement,
                typeof(CaoFactoryObject));
            TypeRegistry.RegisterType(
                RemotingTypePrefix + RemoteObjectFactoryConstants.RemoteObjectFactoryElement,
                typeof(RemoteObjectFactory));
            TypeRegistry.RegisterType(
                RemotingTypePrefix + SaoExporterConstants.SaoExporterElement,
                typeof(SaoExporter));
            TypeRegistry.RegisterType(
                RemotingTypePrefix + CaoExporterConstants.CaoExporterElement,
                typeof(CaoExporter));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemotingNamespaceParser"/> class.
        /// </summary>
        public RemotingNamespaceParser()
        {}


        /// <summary>
        /// Parse the specified element and register any resulting
        /// IObjectDefinitions with the IObjectDefinitionRegistry that is
        /// embedded in the supplied ParserContext.
        /// </summary>
        /// <param name="element">The element to be parsed into one or more IObjectDefinitions</param>
        /// <param name="parserContext">The object encapsulating the current state of the parsing
        /// process.</param>
        /// <returns>
        /// The primary IObjectDefinition (can be null as explained above)
        /// </returns>
        /// <remarks>
        /// Implementations should return the primary IObjectDefinition
        /// that results from the parse phase if they wish to used nested
        /// inside (for example) a <code>&lt;property&gt;</code> tag.
        /// <para>Implementations may return null if they will not
        /// be used in a nested scenario.
        /// </para>
        /// </remarks>
        public override IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
        {
            string name = element.GetAttribute(ObjectDefinitionConstants.IdAttribute);
            IConfigurableObjectDefinition remotingDefinition = ParseRemotingDefinition(element, name, parserContext);
            if (!StringUtils.HasText(name))
            {
                name = ObjectDefinitionReaderUtils.GenerateObjectName(remotingDefinition, parserContext.Registry);
            }
            parserContext.Registry.RegisterObjectDefinition(name, remotingDefinition);

            return null;
        }



        /// <summary>
        /// Parses remoting definitions.
        /// </summary>
        /// <param name="element">Validator XML element.</param>
        /// <param name="name">The name of the object definition.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <returns>A remoting object definition.</returns>
        private IConfigurableObjectDefinition ParseRemotingDefinition(
            XmlElement element, string name, ParserContext parserContext)
        {
            switch (element.LocalName)
            {
                case RemotingConfigurerConstants.RemotingConfigurerElement:
                    return ParseRemotingConfigurer(element, name, parserContext);
                case SaoFactoryObjectConstants.SaoFactoryObjectElement:
                    return ParseSaoFactoryObject(element, name, parserContext);
                case CaoFactoryObjectConstants.CaoFactoryObjectElement:
                    return ParseCaoFactoryObject(element, name, parserContext);
                case RemoteObjectFactoryConstants.RemoteObjectFactoryElement:
                    return ParseRemoteObjectFactory(element, name, parserContext);
                case SaoExporterConstants.SaoExporterElement:
                    return ParseSaoExporter(element, name, parserContext);
                case CaoExporterConstants.CaoExporterElement:
                    return ParseCaoExporter(element, name, parserContext);
            }

            return null;
        }

        /// <summary>
        /// Parses the RemotingConfigurer definition.
        /// </summary>
        /// <param name="element">The element to parse.</param>
        /// <param name="name">The name of the object definition.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <returns>RemotingConfigurer object definition.</returns>
        private IConfigurableObjectDefinition ParseRemotingConfigurer(
            XmlElement element, string name, ParserContext parserContext)
        {
            string typeName = GetTypeName(element);
            string filename = element.GetAttribute(RemotingConfigurerConstants.FilenameAttribute);
            string useConfigFile = element.GetAttribute(RemotingConfigurerConstants.UseConfigFileAttribute);
            string ensureSecurity = element.GetAttribute(RemotingConfigurerConstants.EnsureSecurityAttribute);

            MutablePropertyValues properties = new MutablePropertyValues();
            if (StringUtils.HasText(filename))
            {
                properties.Add("Filename", filename);
            }
            if (StringUtils.HasText(useConfigFile))
            {
                properties.Add("UseConfigFile", useConfigFile);
            }
            if (StringUtils.HasText(ensureSecurity))
            {
                properties.Add("EnsureSecurity", ensureSecurity);
            }

            IConfigurableObjectDefinition cod = parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                typeName, null, parserContext.ReaderContext.Reader.Domain);
            cod.PropertyValues = properties;
            return cod;
        }

        /// <summary>
        /// Parses the SaoFactoryObject definition.
        /// </summary>
        /// <param name="element">The element to parse.</param>
        /// <param name="name">The name of the object definition.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <returns>SaoFactoryObject object definition.</returns>
        private IConfigurableObjectDefinition ParseSaoFactoryObject(
            XmlElement element, string name, ParserContext parserContext)
        {
            string typeName = GetTypeName(element);
            string serviceInterface = element.GetAttribute(SaoFactoryObjectConstants.ServiceInterfaceAttribute);
            string serviceUrl = element.GetAttribute(SaoFactoryObjectConstants.ServiceUrlAttribute);

            MutablePropertyValues properties = new MutablePropertyValues();
            if (StringUtils.HasText(serviceInterface))
            {
                properties.Add("ServiceInterface", serviceInterface);
            }
            if (StringUtils.HasText(serviceUrl))
            {
                properties.Add("ServiceUrl", serviceUrl);
            }
            IConfigurableObjectDefinition cod = parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                typeName, null, parserContext.ReaderContext.Reader.Domain);
            cod.PropertyValues = properties;
            return cod;
        }

        /// <summary>
        /// Parses the CaoFactoryObject definition.
        /// </summary>
        /// <param name="element">The element to parse.</param>
        /// <param name="name">The name of the object definition.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <returns>CaoFactoryObject object definition.</returns>
        private IConfigurableObjectDefinition ParseCaoFactoryObject(
            XmlElement element, string name, ParserContext parserContext)
        {
            string typeName = GetTypeName(element);
            string remoteTargetName = element.GetAttribute(CaoFactoryObjectConstants.RemoteTargetNameAttribute);
            string serviceUrl = element.GetAttribute(CaoFactoryObjectConstants.ServiceUrlAttribute);

            MutablePropertyValues properties = new MutablePropertyValues();
            if (StringUtils.HasText(remoteTargetName))
            {
                properties.Add("RemoteTargetName", remoteTargetName);
            }
            if (StringUtils.HasText(serviceUrl))
            {
                properties.Add("ServiceUrl", serviceUrl);
            }

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.LocalName)
                {
                    case CaoFactoryObjectConstants.ConstructorArgumentsElement:
                        properties.Add("ConstructorArguments", base.ParseListElement(child, name, parserContext));
                        break;
                }
            }

            IConfigurableObjectDefinition cod = parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                typeName, null, parserContext.ReaderContext.Reader.Domain);
            cod.PropertyValues = properties;

            return cod;
        }

        /// <summary>
        /// Parses the RemoteObjectFactory definition.
        /// </summary>
        /// <param name="element">The element to parse.</param>
        /// <param name="name">The name of the object definition.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <returns>RemoteObjectFactory object definition.</returns>
        private IConfigurableObjectDefinition ParseRemoteObjectFactory(
            XmlElement element, string name, ParserContext parserContext)
        {
            string typeName = GetTypeName(element);
            string targetName = element.GetAttribute(RemoteObjectFactoryConstants.TargetNameAttribute);
            string infinite = element.GetAttribute(RemoteObjectFactoryConstants.InfiniteAttribute);

            MutablePropertyValues properties = new MutablePropertyValues();
            if (StringUtils.HasText(targetName))
            {
                properties.Add("Target", targetName);
            }
            if (StringUtils.HasText(infinite))
            {
                properties.Add("Infinite", infinite);
            }

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.LocalName)
                {
                    case LifeTimeConstants.LifeTimeElement:
                        ParseLifeTime(properties, child, parserContext);
                        break;
                    case InterfacesConstants.InterfacesElement:
                        properties.Add("Interfaces", base.ParseListElement(child, name, parserContext));
                        break;
                }
            }

            IConfigurableObjectDefinition cod = parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                typeName, null, parserContext.ReaderContext.Reader.Domain);
            cod.PropertyValues = properties;

            return cod;
        }

        /// <summary>
        /// Parses the SaoExporter definition.
        /// </summary>
        /// <param name="element">The element to parse.</param>
        /// <param name="name">The name of the object definition.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <returns>SaoExporter object definition.</returns>
        private IConfigurableObjectDefinition ParseSaoExporter(
            XmlElement element, string name, ParserContext parserContext)
        {
            string typeName = GetTypeName(element);
            string targetName = element.GetAttribute(SaoExporterConstants.TargetNameAttribute);
            string applicationName = element.GetAttribute(SaoExporterConstants.ApplicationNameAttribute);
            string serviceName = element.GetAttribute(SaoExporterConstants.ServiceNameAttribute);
            string infinite = element.GetAttribute(SaoExporterConstants.InfiniteAttribute);

            MutablePropertyValues properties = new MutablePropertyValues();
            if (StringUtils.HasText(targetName))
            {
                properties.Add("TargetName", targetName);
            }
            if (StringUtils.HasText(applicationName))
            {
                properties.Add("ApplicationName", applicationName);
            }
            if (StringUtils.HasText(serviceName))
            {
                properties.Add("ServiceName", serviceName);
            }
            if (StringUtils.HasText(infinite))
            {
                properties.Add("Infinite", infinite);
            }

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.LocalName)
                {
                    case LifeTimeConstants.LifeTimeElement:
                        ParseLifeTime(properties, child, parserContext);
                        break;
                    case InterfacesConstants.InterfacesElement:
                        properties.Add("Interfaces", base.ParseListElement(child, name, parserContext));
                        break;
                }
            }

            IConfigurableObjectDefinition cod = parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                typeName, null, parserContext.ReaderContext.Reader.Domain);
            cod.PropertyValues = properties;
            return cod;
        }

        /// <summary>
        /// Parses the CaoExporter definition.
        /// </summary>
        /// <param name="element">The element to parse.</param>
        /// <param name="name">The name of the object definition.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <returns>CaoExporter object definition.</returns>
        private IConfigurableObjectDefinition ParseCaoExporter(
            XmlElement element, string name, ParserContext parserContext)
        {
            string typeName = GetTypeName(element);
            string targetName = element.GetAttribute(CaoExporterConstants.TargetNameAttribute);
            string infinite = element.GetAttribute(CaoExporterConstants.InfiniteAttribute);

            MutablePropertyValues properties = new MutablePropertyValues();
            if (StringUtils.HasText(targetName))
            {
                properties.Add("TargetName", targetName);
            }
            if (StringUtils.HasText(infinite))
            {
                properties.Add("Infinite", infinite);
            }

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.LocalName)
                {
                    case LifeTimeConstants.LifeTimeElement:
                        ParseLifeTime(properties, child, parserContext);
                        break;
                    case InterfacesConstants.InterfacesElement:
                        properties.Add("Interfaces", base.ParseListElement(child, name, parserContext));
                        break;
                }
            }

            IConfigurableObjectDefinition cod = parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                typeName, null, parserContext.ReaderContext.Reader.Domain);
            cod.PropertyValues = properties;

            return cod;
        }

        /// <summary>
        /// Parses the LifeTime definition.
        /// </summary>
        private void ParseLifeTime(MutablePropertyValues properties, XmlElement child, ParserContext parserContext)
        {
            string initialLeaseTime = child.GetAttribute(LifeTimeConstants.InitialLeaseTimeAttribute);
            string renewOnCallTime = child.GetAttribute(LifeTimeConstants.RenewOnCallTimeAttribute);
            string sponsorshipTimeout = child.GetAttribute(LifeTimeConstants.SponsorshipTimeoutAttribute);

            if (StringUtils.HasText(initialLeaseTime))
            {
                properties.Add("InitialLeaseTime", initialLeaseTime);
            }
            if (StringUtils.HasText(renewOnCallTime))
            {
                properties.Add("RenewOnCallTime", renewOnCallTime);
            }
            if (StringUtils.HasText(sponsorshipTimeout))
            {
                properties.Add("SponsorshipTimeout", sponsorshipTimeout);
            }
        }

        /// <summary>
        /// Gets the name of the object type for the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The name of the object type.</returns>
        private string GetTypeName(XmlElement element)
        {
            string typeName = element.GetAttribute(ObjectDefinitionConstants.TypeAttribute);
            if (StringUtils.IsNullOrEmpty(typeName))
            {
                return RemotingTypePrefix + element.LocalName;
            }
            return typeName;
        }

        #region Element & Attribute Name Constants

        private class RemotingConfigurerConstants
        {
            public const string RemotingConfigurerElement = "configurer";
            public const string FilenameAttribute = "filename";
            public const string UseConfigFileAttribute = "useConfigFile";
            public const string EnsureSecurityAttribute = "ensureSecurity";
        }

        private class SaoFactoryObjectConstants
        {
            public const string SaoFactoryObjectElement = "saoFactory";
            public const string ServiceInterfaceAttribute = "serviceInterface";
            public const string ServiceUrlAttribute = "serviceUrl";
        }

        private class CaoFactoryObjectConstants
        {
            public const string CaoFactoryObjectElement = "caoFactory";
            public const string ConstructorArgumentsElement = "constructor-args";
            public const string RemoteTargetNameAttribute = "remoteTargetName";
            public const string ServiceUrlAttribute = "serviceUrl";
        }

        private class LifeTimeConstants
        {
            public const string LifeTimeElement = "lifeTime";
            public const string InitialLeaseTimeAttribute = "initialLeaseTime";
            public const string RenewOnCallTimeAttribute = "renewOnCallTime";
            public const string SponsorshipTimeoutAttribute = "sponsorshipTimeout";
        }

        private class InterfacesConstants
        {
            public const string InterfacesElement = "interfaces";
        }

        private class RemoteObjectFactoryConstants
        {
            public const string RemoteObjectFactoryElement = "remoteObjectFactory";
            public const string TargetNameAttribute = "targetName";
            public const string InfiniteAttribute = "infinite";
        }

        private class SaoExporterConstants
        {
            public const string SaoExporterElement = "saoExporter";
            public const string TargetNameAttribute = "targetName";
            public const string ApplicationNameAttribute = "applicationName";
            public const string ServiceNameAttribute = "serviceName";
            public const string InfiniteAttribute = "infinite";
        }

        private class CaoExporterConstants
        {
            public const string CaoExporterElement = "caoExporter";
            public const string TargetNameAttribute = "targetName";
            public const string InfiniteAttribute = "infinite";
        }

        #endregion
    }
}
