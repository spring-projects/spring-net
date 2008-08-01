#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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
using Common.Logging;
using Spring.Objects.Factory;
using Apache.NMS;

namespace Spring.Messaging.Nms.Core
{
    /// <summary>
    /// Convenient super class for application classes that need NMS access.
    /// </summary>
    /// <remarks>
    ///  Requires a ConnectionFactory or a MessageTemplate instance to be set.
    ///  It will create its own MessageTemplate if a ConnectionFactory is passed in.
    ///  A custom MessageTemplate instance can be created for a given ConnectionFactory
    ///  through overriding the <code>createNmsTemplate</code> method.
    ///
    /// </remarks>
    public class MessageGatewaySupport : IInitializingObject
    {

        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof(MessageGatewaySupport));

        #endregion
        
        private MessageTemplate jmsTemplate;


        /// <summary>
        /// Gets or sets the NMS template for the gateway.
        /// </summary>
        /// <value>The NMS template.</value>
        public MessageTemplate MessageTemplate
        {
            get { return jmsTemplate; }
            set { jmsTemplate = value; }
        }

        /// <summary>
        /// Gets or sets he NMS connection factory to be used by the gateway.
	    /// Will automatically create a MessageTemplate for the given ConnectionFactory.
        /// </summary>
        /// <value>The connection factory.</value>
        public IConnectionFactory ConnectionFactory
        {
            get
            {
                return (jmsTemplate != null ? this.jmsTemplate.ConnectionFactory : null);
            }
            set
            {
                this.jmsTemplate = CreateNmsTemplate(value);
            }
        }

        /// <summary>
        /// Creates a MessageTemplate for the given ConnectionFactory.
        /// </summary>
	    /// <remarks>Only invoked if populating the gateway with a ConnectionFactory reference.
	    /// Can be overridden in subclasses to provide a different MessageTemplate instance
	    /// </remarks>
	    ///
	    /// <param name="connectionFactory">The connection factory.</param>
        /// <returns></returns>
        protected virtual MessageTemplate CreateNmsTemplate(IConnectionFactory connectionFactory)
        {
            return new MessageTemplate(connectionFactory);
        }

        /// <summary>
        /// Ensures that the JmsTemplate is specified and calls <see cref="InitGateway"/>.
        /// </summary>
        public void AfterPropertiesSet()
        {
            if (jmsTemplate == null)
            {
                throw new ArgumentException("connectionFactory or jmsTemplate is required");
            }
            try
            {
                InitGateway();
            }
            catch (Exception e)
            {
                throw new ObjectInitializationException("Initialization of the NMS gateway failed: " + e.Message, e);
            }
        }

        /// <summary>
        /// Subclasses can override this for custom initialization behavior.
	    /// Gets called after population of this instance's properties.
        /// </summary>
        protected virtual void InitGateway()
        {
            
        }
    }
}
