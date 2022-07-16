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
using Spring.Aop.Config;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Transaction.Interceptor;

namespace Spring.Transaction.Config
{
    /// <summary>
    /// IObjectDefinitionParser implementation that allows users to easily configure all the
    /// infrastructure objects required to enable attribute-driven transction demarcation.
    /// </summary>
    /// <author>Rob Harrop</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class AttributeDrivenObjectDefinitionParser : AbstractObjectDefinitionParser
    {
        /// <summary>
        /// The '<code>proxy-target-type</code>' attribute
        /// </summary>
        private static readonly string PROXY_TARGET_TYPE = "proxy-target-type";

        /// <summary>
        /// The '<code>order</code>' property/attribute
        /// </summary>
        private static readonly string ORDER = "order";

        /// <summary>
        /// Central template method to actually parse the supplied XmlElement
        /// into one or more IObjectDefinitions.
        /// </summary>
        /// <param name="element">The element that is to be parsed into one or more <see cref="IObjectDefinition"/>s</param>
        /// <param name="parserContext">The the object encapsulating the current state of the parsing process;
        /// provides access to a <see cref="IObjectDefinitionRegistry"/></param>
        /// <returns>
        /// The primary IObjectDefinition resulting from the parsing of the supplied XmlElement
        /// </returns>
        protected override AbstractObjectDefinition ParseInternal(XmlElement element, ParserContext parserContext)
        {
            ConfigureAutoProxyCreator(parserContext, element);

            //Create the TransactionAttributeSource
            RootObjectDefinition sourceDef = new RootObjectDefinition(typeof(AttributesTransactionAttributeSource));
            sourceDef.Role = ObjectRole.ROLE_INFRASTRUCTURE;
            string sourceName = parserContext.ReaderContext.RegisterWithGeneratedName(sourceDef);

            //Create the TransactionInterceptor definition.
            RootObjectDefinition interceptorDefinition = new RootObjectDefinition(typeof(TransactionInterceptor));
            interceptorDefinition.Role = ObjectRole.ROLE_INFRASTRUCTURE;
            RegisterTransactionManager(element, interceptorDefinition);
            interceptorDefinition.PropertyValues.Add(TxNamespaceUtils.TRANSACTION_ATTRIBUTE_SOURCE, new RuntimeObjectReference(sourceName));
            String interceptorName = parserContext.ReaderContext.RegisterWithGeneratedName(interceptorDefinition);

            // Create the TransactionAttributeSourceAdvisor definition.
            RootObjectDefinition advisorDef = new RootObjectDefinition(typeof(ObjectFactoryTransactionAttributeSourceAdvisor));
            advisorDef.Role = ObjectRole.ROLE_INFRASTRUCTURE;
            advisorDef.PropertyValues.Add("transactionAttributeSource", new RuntimeObjectReference(sourceName));
            advisorDef.PropertyValues.Add("adviceObjectName", interceptorName);

            if (element.HasAttribute(ORDER))
            {
                advisorDef.PropertyValues.Add(ORDER, GetAttributeValue(element, ORDER));
            }

            return advisorDef;
        }

        private void RegisterTransactionManager(XmlElement element, RootObjectDefinition interceptorDefinition)
        {
            string transactionManagerName = GetAttributeValue(element, TxNamespaceUtils.TRANSACTION_MANAGER_ATTRIBUTE);
            interceptorDefinition.PropertyValues.Add(TxNamespaceUtils.TRANSACTION_MANAGER_PROPERTY,
                                          new RuntimeObjectReference(transactionManagerName));


        }

        /// <summary>
        /// Configures the auto proxy creator.
        /// </summary>
        /// <param name="parserContext">The parser context.</param>
        /// <param name="element">The element.</param>
        private static void ConfigureAutoProxyCreator(ParserContext parserContext, XmlElement element)
        {
            AopNamespaceUtils.RegisterAutoProxyCreatorIfNecessary(parserContext, element);

            bool proxyTargetClass = parserContext.ParserHelper.IsTrueStringValue(GetAttributeValue(element, PROXY_TARGET_TYPE));
            if (proxyTargetClass)
            {
                AopNamespaceUtils.ForceAutoProxyCreatorToUseDecoratorProxy(parserContext.Registry);
            }
        }

        /// <summary>
        /// Gets a value indicating whether an ID should be generated instead of read
        /// from the passed in XmlElement.
        /// </summary>
        /// <value><c>true</c> if should generate id; otherwise, <c>false</c>.</value>
        /// <remarks>Note that this flag is about always generating an ID; the parser
        /// won't even check for an "id" attribute in this case.
        /// </remarks>
        protected override bool ShouldGenerateId
        {
            get { return true; }
        }
    }
}
