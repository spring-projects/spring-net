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
using Apache.NMS;
using Spring.Core.TypeResolution;
using Spring.Messaging.Nms.Listener;
using Spring.Messaging.Nms.Listener.Adapter;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Messaging.Nms.Config
{
    /// <summary>
    /// Parser for the NMS <code>&lt;listener-container&gt;</code> element.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class MessageListenerContainerObjectDefinitionParser : IObjectDefinitionParser
    {
        #region Fields

        private readonly string LISTENER_ELEMENT = "listener";

        private readonly string ID_ATTRIBUTE = "id";

        private readonly string DESTINATION_ATTRIBUTE = "destination";

        private readonly string SUBSCRIPTION_ATTRIBUTE = "subscription";

        private readonly string SELECTOR_ATTRIBUTE = "selector";

        private readonly string REF_ATTRIBUTE = "ref";

        private readonly string METHOD_ATTRIBUTE = "method";

        private readonly string DESTINATION_RESOLVER_ATTRIBUTE = "destination-resolver";

        private readonly string MESSAGE_CONVERTER_ATTRIBUTE = "message-converter";

        private readonly string RESPONSE_DESTINATION_ATTRIBUTE = "response-destination";

        private readonly string DESTINATION_TYPE_ATTRIBUTE = "destination-type";

        private readonly string DESTINATION_TYPE_QUEUE = "queue";

        private readonly string DESTINATION_TYPE_TOPIC = "topic";

        private readonly string DESTINATION_TYPE_DURABLE_TOPIC = "durableTopic";

        private readonly string CLIENT_ID_ATTRIBUTE = "client-id";

        private readonly string ACKNOWLEDGE_ATTRIBUTE = "acknowledge";

        private readonly string ACKNOWLEDGE_AUTO = "auto";

        private readonly string ACKNOWLEDGE_CLIENT = "client";

        private readonly string ACKNOWLEDGE_DUPS_OK = "dups-ok";

        private readonly string ACKNOWLEDGE_TRANSACTED = "transacted";

        private readonly string CONCURRENCY_ATTRIBUTE = "concurrency";

        private readonly string RECOVERY_INTERVAL_ATTRIBUTE = "recovery-interval";

        private readonly string MAX_RECOVERY_TIME_ATTRIBUTE = "max-recovery-time";

        private readonly string CONNECTION_FACTORY_ATTRIBUTE = "connection-factory";

        private readonly string CONTAINER_CUSTOM_TYPE = "container-custom-type";

        private readonly string PUBSUB_DOMAIN_ATTRIBUTE = "pubsub-domain";

        private readonly string AUTO_STARTUP = "auto-startup";

        private readonly string ERROR_HANDLER_ATTRIBUTE = "error-handler";

        private readonly string EXCEPTION_LISTENER_ATTRIBUTE = "exception-listener";

        #endregion

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

            XmlNodeList childNodes = element.ChildNodes;
            foreach (XmlNode childNode in childNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element)
                {
                    string localName = childNode.LocalName;
                    if (LISTENER_ELEMENT.Equals(localName))
                    {
                        ParseListener((XmlElement) childNode, element, parserContext);
                    }
                }
            }
            return null;
        }

        #endregion

        private void ParseListener(XmlElement listenerElement, XmlElement containerElement, ParserContext parserContext)
        {
            ObjectDefinitionBuilder listenerDefBuilder =
                parserContext.ParserHelper.CreateRootObjectDefinitionBuilder(typeof (MessageListenerAdapter));

            string reference = listenerElement.GetAttribute(REF_ATTRIBUTE);
            if (!StringUtils.HasText(reference))
            {
                parserContext.ReaderContext.ReportException(listenerElement, LISTENER_ELEMENT,
                                                            "Listener '" + REF_ATTRIBUTE +
                                                            "' attribute contains empty value.");
            }
            listenerDefBuilder.AddPropertyValue("HandlerObject", new RuntimeObjectReference(reference));

            string handlerMethod = null;
            if (listenerElement.HasAttribute(METHOD_ATTRIBUTE))
            {
                handlerMethod = listenerElement.GetAttribute(METHOD_ATTRIBUTE);
                {
                    if (!StringUtils.HasText(handlerMethod))
                    {
                        parserContext.ReaderContext.ReportException(listenerElement, LISTENER_ELEMENT,
                                                                    "Listener '" + METHOD_ATTRIBUTE +
                                                                    "' attribute contains empty value.");
                    }
                }
            }
            listenerDefBuilder.AddPropertyValue("DefaultHandlerMethod", handlerMethod);

            if (containerElement.HasAttribute(MESSAGE_CONVERTER_ATTRIBUTE))
            {
                string messageConverter = containerElement.GetAttribute(MESSAGE_CONVERTER_ATTRIBUTE);
                listenerDefBuilder.AddPropertyValue("MessageConverter", new RuntimeObjectReference(messageConverter));
            }

            ObjectDefinitionBuilder containerDefBuilder = ParseContainer(listenerElement, containerElement, parserContext);

            if (listenerElement.HasAttribute(RESPONSE_DESTINATION_ATTRIBUTE))
            {
                string responseDestination = listenerElement.GetAttribute(RESPONSE_DESTINATION_ATTRIBUTE);
                bool pubSubDomain = IndicatesPubSub(containerDefBuilder.RawObjectDefinition);
                listenerDefBuilder.AddPropertyValue(pubSubDomain ? "DefaultResponseTopicName" : "DefaultResponseQueueName",
                                             responseDestination);
                if (containerDefBuilder.RawObjectDefinition.PropertyValues.Contains("DestinationResolver"))
                {
                    listenerDefBuilder.AddPropertyValue("DestinationResolver",
                                                 containerDefBuilder.RawObjectDefinition.PropertyValues.GetPropertyValue
                                                     (
                                                     "DestinationResolver").Value);
                }
            }

            containerDefBuilder.AddPropertyValue("MessageListener", listenerDefBuilder.ObjectDefinition);

            string containerObjectName = listenerElement.GetAttribute(ID_ATTRIBUTE);
            // If no object id is given auto generate one using the ReaderContext's ObjectNameGenerator
            if (!StringUtils.HasText(containerObjectName))
            {
                containerObjectName =
                    parserContext.ReaderContext.GenerateObjectName(containerDefBuilder.RawObjectDefinition);
            }

            string pubsubDomain = listenerElement.GetAttribute(PUBSUB_DOMAIN_ATTRIBUTE);
            containerDefBuilder.AddPropertyValue("PubSubDomain", pubsubDomain);


            parserContext.Registry.RegisterObjectDefinition(containerObjectName, containerDefBuilder.ObjectDefinition);
        }

        private ObjectDefinitionBuilder ParseContainer(XmlElement listenerElement, XmlElement containerElement,
                                                       ParserContext parserContext)
        {
            //Only support SimpleMessageListenerContainer or container-custom-type

            Type containerType = typeof (SimpleMessageListenerContainer);
            if (containerElement.HasAttribute(CONTAINER_CUSTOM_TYPE))
            {
                string customType = containerElement.GetAttribute(CONTAINER_CUSTOM_TYPE);
                if (!StringUtils.HasLength(customType))
                {
                    parserContext.ReaderContext.ReportException(containerElement, CONTAINER_CUSTOM_TYPE,
                                            "Listener container '" + CONTAINER_CUSTOM_TYPE +
                                            "' attribute contains empty value.");
                }
                try
                {
                    containerType = TypeResolutionUtils.ResolveType(customType);
                } catch (Exception ex)
                {
                    parserContext.ReaderContext.ReportException(containerElement, CONTAINER_CUSTOM_TYPE,
                            "Invalid container-custom-type value [" + customType + "]", ex);
                }
            }

            ObjectDefinitionBuilder containerDef =
                    parserContext.ParserHelper.CreateRootObjectDefinitionBuilder(containerType);

            ParseListenerConfiguration(listenerElement, parserContext, containerDef);
            ParseContainerConfiguration(containerElement, parserContext, containerDef);


            if (containerElement.HasAttribute(AUTO_STARTUP))
            {
                string autoStartup = containerElement.GetAttribute(AUTO_STARTUP);
                if (StringUtils.HasText(autoStartup))
                {
                    containerDef.AddPropertyValue("AutoStartup", autoStartup);
                }
            }

            string connectionFactoryObjectName = "connectionFactory";
            if (containerElement.HasAttribute(CONNECTION_FACTORY_ATTRIBUTE))
            {
                connectionFactoryObjectName = containerElement.GetAttribute(CONNECTION_FACTORY_ATTRIBUTE);
                if (!StringUtils.HasText(connectionFactoryObjectName))
                {
                    parserContext.ReaderContext.ReportException(listenerElement, LISTENER_ELEMENT,
                                                                "Listener container '" + CONNECTION_FACTORY_ATTRIBUTE +
                                                                "' attribute contains empty value.");
                }

            }

            containerDef.AddPropertyValue("ConnectionFactory", new RuntimeObjectReference(connectionFactoryObjectName));


            string errorHandlerObjectName = containerElement.GetAttribute(ERROR_HANDLER_ATTRIBUTE);
            if (StringUtils.HasText(errorHandlerObjectName))
            {
                containerDef.AddPropertyValue("ErrorHandler",
                                              new RuntimeObjectReference(errorHandlerObjectName));
            }

            string exceptionListenerObjectName = containerElement.GetAttribute(EXCEPTION_LISTENER_ATTRIBUTE);
            if (StringUtils.HasText(exceptionListenerObjectName))
            {
                containerDef.AddPropertyValue("ExceptionListener",
                                              new RuntimeObjectReference(exceptionListenerObjectName));
            }

            string destinationResolverObjectName = containerElement.GetAttribute(DESTINATION_RESOLVER_ATTRIBUTE);
            if (StringUtils.HasText(destinationResolverObjectName))
            {
                containerDef.AddPropertyValue("DestinationResolver",
                                              new RuntimeObjectReference(destinationResolverObjectName));
            }

            string acknowledge = containerElement.GetAttribute(ACKNOWLEDGE_ATTRIBUTE);
            if (StringUtils.HasText(acknowledge))
            {
                AcknowledgementMode acknowledgementMode = ParseAcknowledgementMode(containerElement, parserContext);
                containerDef.AddPropertyValue("SessionAcknowledgeMode", acknowledgementMode);
            }

            string concurrency = ParseConcurrency(containerElement, parserContext);
            if (concurrency != null)
            {
                containerDef.AddPropertyValue("ConcurrentConsumers", concurrency);
            }
            containerDef.AddPropertyValue("RecoveryInterval", ParseRecoveryInterval(containerElement, parserContext));

            containerDef.AddPropertyValue("MaxRecoveryTime", ParseMaxRecoveryTime(containerElement, parserContext));

            return containerDef;
        }

        private bool IndicatesPubSub(AbstractObjectDefinition configDef)
        {
            return (bool) configDef.PropertyValues.GetPropertyValue("PubSubDomain").Value;
        }

        private void ParseListenerConfiguration(XmlElement ele, ParserContext parserContext,
                                                ObjectDefinitionBuilder containerDef)
        {
            string destination = ele.GetAttribute(DESTINATION_ATTRIBUTE);
            if (!StringUtils.HasText(destination))
            {
                parserContext.ReaderContext.ReportException(ele, LISTENER_ELEMENT,
                                                            "Listener '" + DESTINATION_ATTRIBUTE +
                                                            "' attribute contains empty value.");
            }
            containerDef.AddPropertyValue("DestinationName", destination);

            if (ele.HasAttribute(SUBSCRIPTION_ATTRIBUTE))
            {
                string subscription = ele.GetAttribute(SUBSCRIPTION_ATTRIBUTE);
                if (!StringUtils.HasText(subscription))
                {
                    parserContext.ReaderContext.ReportException(ele, SUBSCRIPTION_ATTRIBUTE,
                                                                "Listener '" + SUBSCRIPTION_ATTRIBUTE +
                                                                "' attribute contains empty value.");
                }
                containerDef.AddPropertyValue("DurableSubscriptionName", subscription);
            }

            if (ele.HasAttribute(SELECTOR_ATTRIBUTE))
            {
                string selector = ele.GetAttribute(SELECTOR_ATTRIBUTE);
                if (!StringUtils.HasText(selector))
                {
                    parserContext.ReaderContext.ReportException(ele, selector,
                                                                "Listener '" + SELECTOR_ATTRIBUTE +
                                                                "' attribute contains empty value.");
                }
                containerDef.AddPropertyValue("MessageSelector", selector);
            }
        }

        private void ParseContainerConfiguration(XmlElement ele, ParserContext parserContext,
                                                 ObjectDefinitionBuilder containerDef)
        {
            string destinationType = ele.GetAttribute(DESTINATION_TYPE_ATTRIBUTE);
            bool pubSubDomain = false;
            bool subscriptionDurable = false;
            if (DESTINATION_TYPE_DURABLE_TOPIC.Equals(destinationType))
            {
                pubSubDomain = true;
                subscriptionDurable = true;
            }
            else if (DESTINATION_TYPE_TOPIC.Equals(destinationType))
            {
                pubSubDomain = true;
            }
            else if ("".Equals(destinationType) || DESTINATION_TYPE_QUEUE.Equals(destinationType))
            {
                // the default: queue
            }
            else
            {
                parserContext.ReaderContext.ReportException(ele, destinationType,
                                                            "Invalid listener container '" + DESTINATION_TYPE_ATTRIBUTE +
                                                            "': only 'queue', 'topic' and 'durableTopic' supported");
            }

            containerDef.AddPropertyValue("PubSubDomain", pubSubDomain);
            containerDef.AddPropertyValue("SubscriptionDurable", subscriptionDurable);

            if (ele.HasAttribute(CLIENT_ID_ATTRIBUTE))
            {
                string clientId = ele.GetAttribute(CLIENT_ID_ATTRIBUTE);
                if (!StringUtils.HasText(clientId))
                {
                    parserContext.ReaderContext.ReportException(ele, clientId,
                                                                "Listener '" + CLIENT_ID_ATTRIBUTE +
                                                                "' attribute contains empty value.");
                }
                containerDef.AddPropertyValue("ClientId", clientId);
            }
        }

        private AcknowledgementMode ParseAcknowledgementMode(XmlElement element, ParserContext parserContext)
        {
            string acknowledge = element.GetAttribute(ACKNOWLEDGE_ATTRIBUTE);
            if (acknowledge.Equals(ACKNOWLEDGE_TRANSACTED))
            {
                return AcknowledgementMode.Transactional;
            }
            else if (acknowledge.Equals(ACKNOWLEDGE_DUPS_OK))
            {
                return AcknowledgementMode.DupsOkAcknowledge;
            }
            else if (acknowledge.Equals(ACKNOWLEDGE_CLIENT))
            {
                return AcknowledgementMode.ClientAcknowledge;
            }
            else if (!acknowledge.Equals(ACKNOWLEDGE_AUTO))
            {
                parserContext.ReaderContext.ReportException(element, ACKNOWLEDGE_ATTRIBUTE,
                                                            "Invalid listener container 'acknowledge' setting ['" +
                                                            acknowledge +
                                                            "]: only \"auto\", \"client\", \"dups-ok\" and \"transacted\" supported.");
            }
            return AcknowledgementMode.AutoAcknowledge;
        }

        private string ParseConcurrency(XmlElement ele, ParserContext parserContext)
        {
            string concurrency = ele.GetAttribute(CONCURRENCY_ATTRIBUTE);
            if (!StringUtils.HasText(concurrency))
            {
                return null;
            } else
            {
                return concurrency;
            }
        }

        private string ParseRecoveryInterval(XmlElement ele, ParserContext parserContext)
        {
            string recoveryInterval = ele.GetAttribute(RECOVERY_INTERVAL_ATTRIBUTE);
            if (StringUtils.HasText(recoveryInterval))
            {
                return recoveryInterval;

            } else
            {
                return SimpleMessageListenerContainer.DEFAULT_RECOVERY_INTERVAL;
            }
        }

        private string ParseMaxRecoveryTime(XmlElement ele, ParserContext parserContext)
        {
            string recoverTime = ele.GetAttribute(MAX_RECOVERY_TIME_ATTRIBUTE);
            if (StringUtils.HasText(recoverTime))
            {
                return recoverTime;
            }
            else
            {
                return SimpleMessageListenerContainer.DEFAULT_MAX_RECOVERY_TIME;
            }
        }
    }
}
