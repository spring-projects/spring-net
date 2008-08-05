using System;
using System.Collections;
using System.Messaging;
using Spring.Context;
using Spring.Messaging.Core;
using Spring.Objects.Factory;

namespace Spring.Messaging.Listener
{
    public class AbstractSendToQueueExceptionHandler : IInitializingObject, IApplicationContextAware
    {
        private int maxRetry = 5;

        private IMessageQueueFactory messageQueueFactory;
        private string messageQueueObjectName;
        private IApplicationContext applicationContext;

        protected object messageMapMonitor = new object();
        protected IDictionary messageMap = new Hashtable();

        /// <summary>
        /// Gets or sets the maximum retry count to reattempt processing of a message that has thrown
        /// an exception
        /// </summary>
        /// <value>The max retry count.</value>
        public int MaxRetry
        {
            get { return maxRetry; }
            set { maxRetry = value; }
        }

        /// <summary>
        /// Gets or sets the message queue factory.
        /// </summary>
        /// <value>The message queue factory.</value>
        public IMessageQueueFactory MessageQueueFactory
        {
            get { return messageQueueFactory; }
            set { messageQueueFactory = value; }
        }

        /// <summary>
        /// Gets or sets the name of the message queue object to send the message that cannot be
        /// processed successfully after MaxRetry delivery attempts.
        /// </summary>
        /// <value>The name of the message queue object.</value>
        public string MessageQueueObjectName
        {
            get { return messageQueueObjectName; }
            set { messageQueueObjectName = value; }
        }

        #region IApplicationContextAware Members

        /// <summary>
        /// Gets or sets the <see cref="Spring.Context.IApplicationContext"/> that this
        /// object runs in.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Normally this call will be used to initialize the object.
        /// </p>
        /// <p>
        /// Invoked after population of normal object properties but before an
        /// init callback such as
        /// <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// or a custom init-method. Invoked after the setting of any
        /// <see cref="Spring.Context.IResourceLoaderAware"/>'s
        /// <see cref="Spring.Context.IResourceLoaderAware.ResourceLoader"/>
        /// property.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Context.ApplicationContextException">
        /// In the case of application context initialization errors.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If thrown by any application context methods.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectInitializationException"/>
        public IApplicationContext ApplicationContext
        {
            get { return applicationContext; }
            set { applicationContext = value; }
        }

        #endregion

        #region IInitializingObject Members

        /// <summary>
        /// Ensure that the MessageQueueObject name is set and creates a 
        /// <see cref="DefaultMessageQueueFactory"/> if no <see cref="IMessageQueueFactory"/>
        /// is specified.
        /// </summary>
        /// <remarks>Will attempt to create an instance of the DefaultMessageQueue to detect early
        /// any configuraiton errors.</remarks>
        /// <exception cref="System.Exception">
        /// In the event of misconfiguration (such as the failure to set a
        /// required property) or if initialization fails.
        /// </exception>
        public virtual void AfterPropertiesSet()
        {
            if (MessageQueueObjectName == null)
            {
                throw new ArgumentException("The DefaultMessageQueueObjectName property has not been set.");
            }
            if (messageQueueFactory == null)
            {
                DefaultMessageQueueFactory mqf = new DefaultMessageQueueFactory();
                mqf.ApplicationContext = applicationContext;
                messageQueueFactory = mqf;
            }
            //Create an instance so we can 'fail-fast' if there isn't an DefaultMessageQueue unde 
            MessageQueue mq = MessageQueueFactory.CreateMessageQueue(messageQueueObjectName);
        }

        #endregion
    }
}