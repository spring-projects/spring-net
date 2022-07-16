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

using Spring.Util;
using Spring.Objects.Factory.Xml;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Core.TypeResolution;
using Spring.Objects;

namespace Spring.ServiceModel.Config
{
    /// <summary>
    /// The <see cref="IObjectDefinitionParser"/> for the <code>&lt;wcf:channelFactory&gt;</code> tag.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class ChannelFactoryObjectDefinitionParser : ObjectsNamespaceParser, IObjectDefinitionParser
    {
        private static readonly string ChannelTypeAttribute = "channelType";
        private static readonly string EndpointConfigurationNameAttribute = "endpointConfigurationName";

        #region IObjectDefinitionParser Members

        /// <summary>
        /// Parse the specified XmlElement and register the resulting
        /// ObjectDefinitions with the <see cref="ParserContext.Registry"/> IObjectDefinitionRegistry
        /// embedded in the supplied <see cref="ParserContext"/>
        /// </summary>
        /// <param name="element">The element to be parsed.</param>
        /// <param name="parserContext">The object encapsulating the current state of the parsing process.
        /// Provides access to a IObjectDefinitionRegistry</param>
        /// <returns>The primary object definition.</returns>
        /// <remarks>
        /// <p>
        /// This method is never invoked if the parser is namespace aware
        /// and was called to process the root node.
        /// </p>
        /// </remarks>
        IObjectDefinition IObjectDefinitionParser.ParseElement(XmlElement element, ParserContext parserContext)
        {
            AssertUtils.ArgumentNotNull(parserContext, "parserContext");

            string id = element.GetAttribute(ObjectDefinitionConstants.IdAttribute);
            string unresolvedChannelType = element.GetAttribute(ChannelTypeAttribute);
            string endpointConfigurationName = element.GetAttribute(EndpointConfigurationNameAttribute);

            IObjectDefinition channelFactoryDefinition;
            try
            {
                Type channelType = TypeResolutionUtils.ResolveType(unresolvedChannelType);
                Type channelFactoryType = typeof(ChannelFactoryObject<>).MakeGenericType(new Type[1] { channelType });
                channelFactoryDefinition = new RootObjectDefinition(channelFactoryType);
            }
            catch
            {
                // Try to resolve type later (Can be a type alias)
                channelFactoryDefinition = new RootObjectDefinition(
                    String.Format("Spring.ServiceModel.ChannelFactoryObject<{0}>, Spring.Services", unresolvedChannelType),
                    new ConstructorArgumentValues(),
                    new MutablePropertyValues());
            }

            if (!StringUtils.HasText(id))
            {
                id = parserContext.ReaderContext.GenerateObjectName(channelFactoryDefinition);
            }

            channelFactoryDefinition.ConstructorArgumentValues.AddNamedArgumentValue("endpointConfigurationName", endpointConfigurationName);

            foreach (PropertyValue pv in base.ParsePropertyElements(id, element, parserContext))
            {
                channelFactoryDefinition.PropertyValues.Add(pv);
            }

            parserContext.Registry.RegisterObjectDefinition(id, channelFactoryDefinition);

            return null;
        }

        #endregion
    }
}
