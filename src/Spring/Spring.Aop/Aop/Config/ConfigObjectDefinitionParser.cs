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
using Spring.Aop.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Aop.Config
{
    /// <summary>
    /// The <see cref="IObjectDefinitionParser"/> for the <code>&lt;aop:config&gt;</code> tag.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    public class ConfigObjectDefinitionParser : IObjectDefinitionParser
    {
        /// <summary>
        /// The '<code>proxy-target-type</code>' attribute
        /// </summary>
        private static readonly string PROXY_TARGET_TYPE = "proxy-target-type";

        private static readonly string ID = "id";

        private static readonly string ORDER_PROPERTY = "order";

        private static readonly string ADVICE_REF = "advice-ref";

        private static readonly string ADVICE_OBJECT_NAME = "adviceObjectName";

        private static readonly string POINTCUT_REF = "pointcut-ref";


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
        /// 	<p>
        /// This method is never invoked if the parser is namespace aware
        /// and was called to process the root node.
        /// </p>
        /// </remarks>
        public IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
        {
            ConfigureAutoProxyCreator(parserContext, element);
            XmlNodeList advisorNodes = element.GetElementsByTagName("advisor", element.NamespaceURI);

            //XmlNodeList advisorNodes = element.SelectNodes("*[local-name()='advisor' and namespace-uri()='" + element.NamespaceURI + "']");
            foreach (XmlElement advisorElement in advisorNodes)
            {
                ParseAdvisor(advisorElement, parserContext);
            }

            return null;
        }

        /// <summary>
        /// Parses the supplied advisor element and registers the resulting <see cref="IAdvisor"/> 
        /// </summary>
        /// <param name="advisorElement">The advisor element.</param>
        /// <param name="parserContext">The parser context.</param>
        private void ParseAdvisor(XmlElement advisorElement, ParserContext parserContext)
        {
            AbstractObjectDefinition advisorDef = CreateAdvisorObjectDefinition(advisorElement, parserContext);
            string id = advisorElement.GetAttribute(ID);

            string pointcutObjectName = ParsePointcutProperty(advisorElement, parserContext);
            advisorDef.PropertyValues.Add(POINTCUT_REF, new RuntimeObjectReference(pointcutObjectName));
            string advisorObjectName = id;
            if (StringUtils.HasText(advisorObjectName))
            {
                parserContext.Registry.RegisterObjectDefinition(advisorObjectName, advisorDef);
            }
            else
            {
                parserContext.ReaderContext.RegisterWithGeneratedName(advisorDef);
            }
        }


        private string ParsePointcutProperty(XmlElement element, ParserContext parserContext)
        {
            if (element.HasAttribute(POINTCUT_REF))
            {
                string pointcutRef = element.GetAttribute(POINTCUT_REF);
                if (!StringUtils.HasText(pointcutRef))
                {
                    parserContext.ReaderContext.ReportException(element, "advisor", "'pointcut-ref' attribute contains empty value.");
                }
                return pointcutRef;
            }
            else
            {
                parserContext.ReaderContext.ReportException(element, "advisor", "'must define 'pointcut-ref' on <advisor> tag.");
                return null;
            }
        }


        private AbstractObjectDefinition CreateAdvisorObjectDefinition(XmlElement advisorElement, ParserContext parserContext)
        {
            ObjectDefinitionBuilder advisorDefinitionBuilder =
                parserContext.ParserHelper.CreateRootObjectDefinitionBuilder(typeof(DefaultObjectFactoryPointcutAdvisor));
            advisorDefinitionBuilder.RawObjectDefinition.Role = ObjectRole.ROLE_INFRASTRUCTURE;

            if (advisorElement.HasAttribute(ORDER_PROPERTY))
            {
                advisorDefinitionBuilder.AddPropertyValue(ORDER_PROPERTY, advisorElement.GetAttribute(ORDER_PROPERTY));
            }

            advisorDefinitionBuilder.AddPropertyValue(ADVICE_OBJECT_NAME, advisorElement.GetAttribute(ADVICE_REF));


            return advisorDefinitionBuilder.ObjectDefinition;
        }


        private static void ConfigureAutoProxyCreator(ParserContext parserContext, XmlElement element)
        {
            AopNamespaceUtils.RegisterAutoProxyCreatorIfNecessary(parserContext, element);

            bool proxyTargetClass =
                parserContext.ParserHelper.IsTrueStringValue(element.GetAttribute(PROXY_TARGET_TYPE));
            if (proxyTargetClass)
            {
                AopNamespaceUtils.ForceAutoProxyCreatorToUseDecoratorProxy(parserContext.Registry);
            }
        }

        #endregion
    }
}