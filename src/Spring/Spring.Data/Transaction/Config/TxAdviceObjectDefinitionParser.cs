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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using Spring.Collections;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Transaction.Interceptor;
using Spring.Util;

namespace Spring.Transaction.Config
{
    /// <summary>
    /// The <see cref="IObjectDefinitionParser"/> for the <code>&lt;tx:advice&gt;</code> tag.
    /// </summary>
    /// <author>Rob Harrop</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Adrian Colyer</author>
    /// <author>Mark Pollack (.NET)</author>
    public class TxAdviceObjectDefinitionParser : AbstractSingleObjectDefinitionParser
    {
        private static string TIMEOUT = "timeout";

        private static string READ_ONLY = "read-only";

        private static string NAME_MAP = "nameMap";

        private static string PROPAGATION = "propagation";

        private static string ISOLATION = "isolation";

        private static string ROLLBACK_FOR = "rollback-for";

        private static string NO_ROLLBACK_FOR = "no-rollback-for";

        protected override Type GetObjectType(XmlElement element)
        {
            return typeof (TransactionInterceptor);
        }

        protected override void DoParse(XmlElement element, ParserContext parserContext, ObjectDefinitionBuilder builder)
        {

            builder.AddPropertyReference(TxNamespaceUtils.TRANSACTION_MANAGER_PROPERTY,
                GetAttributeValue(element, TxNamespaceUtils.TRANSACTION_MANAGER_ATTRIBUTE));
            XmlNodeList txAttributes = element.SelectNodes("*[local-name()='attributes' and namespace-uri()='" + element.NamespaceURI + "']");
            if (txAttributes.Count > 1 )
            {
                parserContext.ReaderContext.ReportException(element, "tx advice", "Element <attributes> is allowed at most once inside element <advice>");
            }
            else if (txAttributes.Count == 1)
            {
                //using xml defined source
                XmlElement attributeSourceElement = txAttributes[0] as XmlElement;
                AbstractObjectDefinition attributeSourceDefinition =
                    ParseAttributeSource(attributeSourceElement, parserContext);
                builder.AddPropertyValue(TxNamespaceUtils.TRANSACTION_ATTRIBUTE_SOURCE, attributeSourceDefinition);
            }
            else
            {
                //Assume attibutes source
                ObjectDefinitionBuilder txAttributeSourceBuilder = 
                    parserContext.ParserHelper.CreateRootObjectDefinitionBuilder(typeof (AttributesTransactionAttributeSource));

                builder.AddPropertyValue(TxNamespaceUtils.TRANSACTION_ATTRIBUTE_SOURCE,
                                         txAttributeSourceBuilder.ObjectDefinition);

            }
        }

        private AbstractObjectDefinition ParseAttributeSource(XmlElement element, ParserContext parserContext)
        {
            XmlNodeList methods = element.SelectNodes("*[local-name()='method' and namespace-uri()='" + element.NamespaceURI + "']");
            ManagedDictionary transactionAttributeMap = new ManagedDictionary();
            foreach (XmlElement methodElement in methods)
            {
                string name = GetAttributeValue(methodElement, "name");
                TypedStringValue nameHolder = new TypedStringValue(name);
                
                RuleBasedTransactionAttribute attribute = new RuleBasedTransactionAttribute();
                string propagation = GetAttributeValue(methodElement, PROPAGATION);
                string isolation = GetAttributeValue(methodElement, ISOLATION);
                string timeout = GetAttributeValue(methodElement, TIMEOUT);
                string readOnly = GetAttributeValue(methodElement, READ_ONLY);
                if (StringUtils.HasText(propagation))
                {
                    attribute.PropagationBehavior = (TransactionPropagation) Enum.Parse(typeof (TransactionPropagation), propagation, true);
                }
                if (StringUtils.HasText(isolation))
                {
                    attribute.TransactionIsolationLevel =
                        (IsolationLevel) Enum.Parse(typeof (IsolationLevel), isolation, true);
                }
                if (StringUtils.HasText(timeout))
                {
                    try
                    {
                        attribute.TransactionTimeout = Int32.Parse(timeout);
                    }
                    catch (FormatException ex)
                    {
                        parserContext.ReaderContext.ReportException(methodElement,"tx advice","timeout must be an integer value: [" + timeout + "]", ex);
                    }
                }
                if (StringUtils.HasText(readOnly))
                {
                    attribute.ReadOnly = Boolean.Parse(GetAttributeValue(methodElement, READ_ONLY));
                }

                var rollbackRules = new List<RollbackRuleAttribute>();
                if (methodElement.HasAttribute(ROLLBACK_FOR))
                {
                    string rollbackForValue = GetAttributeValue(methodElement, ROLLBACK_FOR);
                    AddRollbackRuleAttributesTo(rollbackRules, rollbackForValue);
                }
                if (methodElement.HasAttribute(NO_ROLLBACK_FOR))
                {
                    string noRollbackForValue = GetAttributeValue(methodElement, NO_ROLLBACK_FOR);
                    AddNoRollbackRuleAttributesTo(rollbackRules, noRollbackForValue);
                }
                attribute.RollbackRules = rollbackRules;

                transactionAttributeMap[nameHolder] = attribute;
            }

            ObjectDefinitionBuilder builder = parserContext
                                                .ParserHelper
                                                .CreateRootObjectDefinitionBuilder(typeof (NameMatchTransactionAttributeSource));
            builder.AddPropertyValue(NAME_MAP, transactionAttributeMap);
            return builder.ObjectDefinition;

        }

        private void AddRollbackRuleAttributesTo(List<RollbackRuleAttribute> rollbackRules, string rollbackForValue)
        {
            string[] exceptionTypeNames = StringUtils.CommaDelimitedListToStringArray(rollbackForValue);
            foreach (string exceptionTypeName in exceptionTypeNames)
            {
                rollbackRules.Add(new RollbackRuleAttribute(exceptionTypeName.Trim()));
            }
        }

        private void AddNoRollbackRuleAttributesTo(List<RollbackRuleAttribute> rollbackRules, string noRollbackForValue)
        {
            string[] exceptionTypeNames = StringUtils.CommaDelimitedListToStringArray(noRollbackForValue);
            foreach (string exceptionTypeName in exceptionTypeNames)
            {
                rollbackRules.Add(new NoRollbackRuleAttribute(exceptionTypeName.Trim()));
            }
        }
    }
}