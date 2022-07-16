#region License

/*
 * Copyright � 2002-2010 the original author or authors.
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

using Common.Logging;
using Spring.Messaging.Ems.Common;
using Spring.Objects.Factory;

namespace Spring.Messaging.Ems.Core
{
    /// <summary>
    /// Convenient super class for application classes that need EMS access.
    /// </summary>
    /// <remarks>
    ///  Requires a ConnectionFactory or a EmsTemplate instance to be set.
    ///  It will create its own EmsTemplate if a ConnectionFactory is passed in.
    ///  A custom EmsTemplate instance can be created for a given ConnectionFactory
    ///  through overriding the <code>createEmsTemplate</code> method.
    ///
    /// </remarks>
    public class EmsGatewaySupport : IInitializingObject
    {

        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof(EmsGatewaySupport));

        #endregion
        
        private EmsTemplate emsTemplate;


        /// <summary>
        /// Gets or sets the EMS template for the gateway.
        /// </summary>
        /// <value>The EMS template.</value>
        public EmsTemplate EmsTemplate
        {
            get { return emsTemplate; }
            set { emsTemplate = value; }
        }

        /// <summary>
        /// Gets or sets he EMS connection factory to be used by the gateway.
	    /// Will automatically create a EmsTemplate for the given ConnectionFactory.
        /// </summary>
        /// <value>The connection factory.</value>
        public IConnectionFactory ConnectionFactory
        {
            get
            {
                return (emsTemplate != null ? this.emsTemplate.ConnectionFactory : null);
            }
            set
            {
                this.emsTemplate = CreateEmsTemplate(value);
            }
        }

        /// <summary>
        /// Creates a EmsTemplate for the given ConnectionFactory.
        /// </summary>
	    /// <remarks>Only invoked if populating the gateway with a ConnectionFactory reference.
	    /// Can be overridden in subclasses to provide a different EmsTemplate instance
	    /// </remarks>
	    ///
	    /// <param name="connectionFactory">The connection factory.</param>
        /// <returns></returns>
        protected virtual EmsTemplate CreateEmsTemplate(IConnectionFactory connectionFactory)
        {
            return new EmsTemplate(connectionFactory);
        }

        /// <summary>
        /// Ensures that the JmsTemplate is specified and calls <see cref="InitGateway"/>.
        /// </summary>
        public virtual void AfterPropertiesSet()
        {
            if (emsTemplate == null)
            {
                throw new ArgumentException("connectionFactory or jmsTemplate is required");
            }
            try
            {
                InitGateway();
            }
            catch (Exception e)
            {
                throw new ObjectInitializationException("Initialization of the EMS gateway failed: " + e.Message, e);
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
