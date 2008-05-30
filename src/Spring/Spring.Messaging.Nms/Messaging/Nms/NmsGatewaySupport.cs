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
using NMS;

namespace Spring.Messaging.Nms
{
    /// <summary>
    /// Convenient super class for application classes that need NMS access.
    /// </summary>
    /// <remarks>
    ///  Requires a IConnectionFactory or a NmsTemplate instance to be set.
    ///  It will create its own NmsTemplate if a IConnectionFactory is passed in.
    ///  A custom NmsTemplate instance can be created for a given IConnectionFactory
    ///  through overriding the <code>createNmsTemplate</code> method.
    ///
    /// </remarks>
    public class NmsGatewaySupport : IInitializingObject
    {

        #region Logging

        protected readonly ILog logger = LogManager.GetLogger(typeof(NmsGatewaySupport));

        #endregion
        
        private NmsTemplate jmsTemplate;


        /// <summary>
        /// Gets or sets the NMS template for the gateway.
        /// </summary>
        /// <value>The NMS template.</value>
        public NmsTemplate NmsTemplate
        {
            get { return jmsTemplate; }
            set { jmsTemplate = value; }
        }

        /// <summary>
        /// Gets or sets he NMS connection factory to be used by the gateway.
	    /// Will automatically create a NmsTemplate for the given IConnectionFactory.
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
        /// Creates a NmsTemplate for the given IConnectionFactory.
        /// </summary>
	    /// <remarks>Only invoked if populating the gateway with a IConnectionFactory reference.
	    /// Can be overridden in subclasses to provide a different NmsTemplate instance
	    /// </remarks>
	    ///
	    /// <param name="connectionFactory">The connection factory.</param>
        /// <returns></returns>
        protected virtual NmsTemplate CreateNmsTemplate(IConnectionFactory connectionFactory)
        {
            return new NmsTemplate(connectionFactory);
        }

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
