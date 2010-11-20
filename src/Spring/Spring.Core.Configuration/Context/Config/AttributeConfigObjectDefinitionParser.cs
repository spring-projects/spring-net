using System;
using System.Collections.Generic;
using System.Text;
using Spring.Objects.Factory.Xml;
using Spring.Objects.Factory.Support;
using Spring.Util;
using Spring.Context.Attributes;
using Spring.Objects.Factory.Config;

namespace Spring.Context.Config
{
    public class AttributeConfigObjectDefinitionParser : IObjectDefinitionParser
    {
        /// <summary>
        ///  The object name of the internally managed configuration attribure processor
        /// </summary>
        public static readonly string CONFIGURATION_ATTRIBUTE_PROCESSOR_OBJECT_NAME = "Spring.Context.Attributes.InternalConfigurationAttributeProcessor";


        private static readonly Type ConfigurationClassPostProcessorType = typeof(ConfigurationClassPostProcessor);

        public AttributeConfigObjectDefinitionParser()
        {

        }

        public Objects.Factory.Config.IObjectDefinition ParseElement(System.Xml.XmlElement element, ParserContext parserContext)
        {
            IObjectDefinitionRegistry registry = parserContext.ReaderContext.Registry;
            
            AssertUtils.ArgumentNotNull(registry, "registry");

            if (!registry.ContainsObjectDefinition(CONFIGURATION_ATTRIBUTE_PROCESSOR_OBJECT_NAME))
            {
                RootObjectDefinition objectDefinition = new RootObjectDefinition(ConfigurationClassPostProcessorType);
                objectDefinition.Role = ObjectRole.ROLE_INFRASTRUCTURE;
                registry.RegisterObjectDefinition(CONFIGURATION_ATTRIBUTE_PROCESSOR_OBJECT_NAME, objectDefinition);
            }
            
            return null;
        }
    }
}
