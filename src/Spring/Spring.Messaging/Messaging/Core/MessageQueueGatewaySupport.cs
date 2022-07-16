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

using Spring.Objects.Factory;

namespace Spring.Messaging.Core
{
    /// <summary>
    /// Convenient super class for application classes that need MSMQ access.
    /// </summary>
    /// <remarks>
    /// Override the InitGateway method to perform custom startup tasks.
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class MessageQueueGatewaySupport : IInitializingObject
    {
        private MessageQueueTemplate messageQueueTemplate;


        /// <summary>
        /// Gets or sets the message queue template. <see cref="messageQueueTemplate"/>.
        /// </summary>
        /// <value>The message queue template.</value>
        public MessageQueueTemplate MessageQueueTemplate
        {
            get { return messageQueueTemplate; }
            set { messageQueueTemplate = value; }
        }

        /// <summary>
        /// Gets the message queue factory, a convenience method.
        /// </summary>
        /// <value>The message queue factory.</value>
        protected IMessageQueueFactory MessageQueueFactory
        {
            get { return MessageQueueTemplate.MessageQueueFactory;  }
        }


		/// <summary>
		/// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// after it has injected all of an object's dependencies.
		/// </summary>
		public void AfterPropertiesSet()
        {
            if (this.MessageQueueTemplate == null)
            {
                throw new ArgumentException("MessageQueueTemplate is required");
            }
            try
            {
                InitGateway();
            }
            catch (Exception e)
            {
                throw new ObjectInitializationException("Initialization of the MessageQueue gateway failed: " + e.Message, e);
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
