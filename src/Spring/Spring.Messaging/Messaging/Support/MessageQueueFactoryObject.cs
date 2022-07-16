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
using Spring.Objects.Factory.Config;

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

namespace Spring.Messaging.Support
{
    /// <summary>
    /// Factory for creating MessageQueues.  This factory will create prototype instances, i.e. every call to GetObject
    /// will return a new MessageQueue object.
    /// </summary>
    /// <remarks>All MessageQueue constructor arguments are exposed as properties of the factory object.  As this
    /// is a <see cref="IConfigurableFactoryObject"/> use the PropertyTemplate property to specify additional
    /// configuration of the MessageQueue.
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class MessageQueueFactoryObject : IConfigurableFactoryObject, IObjectNameAware
    {
        // fields used in constructor
        private string path = string.Empty;
        private bool modeDenySharedReceive;
        private bool enableCache;
        private QueueAccessMode accessMode = QueueAccessMode.SendAndReceive;

        // To avoid stale handles when MSMQ restarts and when stopping async recieve/peek operations
        private bool enableConnectionCache = false;

        private IObjectDefinition productTemplate;

        private bool messageReadPropertyFilterSetAll = true;

        private bool messageReadPropertyFilterSetDefaults = false;

        private MessageQueueCreatorDelegate messageCreatorDelegate;

        private bool remoteQueue = false;

        private bool remoteQueueIsTransactional = false;
        private string objectName;


        /// <summary>
        /// Gets or sets an instance of the MessageQueueCreator delegate that will be used to create the
        /// MessageQueue object, instead of using the various public properties on this class such
        /// as Path, AccessMode, etc.  Not intended for end-users but rather as a means to help
        /// register MessageQueueFactoryObject at runtime using convenience method on the IMessageQueueFactory.
        /// </summary>
        /// <remarks>
        /// Can also be specifed using an instance of MessageCreatorDelegate.  If both are specifed, the
        /// Interface implementation has priority.
        /// </remarks>
        /// <value>The function that is responsbile for creating a message queue.</value>
        public MessageQueueCreatorDelegate MessageCreatorDelegate
        {
            get { return messageCreatorDelegate; }
            set { messageCreatorDelegate = value; }
        }

        /// <summary>
        /// Gets or sets the path used to create a MessageQueue instance.
        /// </summary>
        /// <value>The location of the queue.</value>
        public string Path
        {
            get { return path; }
            set { path = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to create the MessageQueue instance with
        /// exclusive read access to the first application that accesses the queue
        /// </summary>
        /// <value>
        /// 	<c>true</c> to grant exclusive read access to the first application that accesses the queue; otherwise,  <c>false</c>.
        /// </value>
        public bool DenySharedReceive
        {
            get { return modeDenySharedReceive; }
            set { modeDenySharedReceive = value; }
        }


        /// <summary>
        /// Gets or sets the queue access mode.
        /// </summary>
        /// <value>The queue access mode.</value>
        /// <see cref="AccessMode"/>
        public QueueAccessMode AccessMode
        {
            get { return accessMode; }
            set { accessMode = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether [enable cache].
        /// </summary>
        /// <value><c>true</c> to create and use a connection cache; otherwise <c>false</c>.</value>
        public bool EnableCache
        {
            get { return enableCache; }
            set { enableCache = value; }
        }

        /// <summary>
        /// Sets a value indicating whether to enable connection cache.  The default is false, which
        /// is different than the default value when creating a System.Messaging.MessageQueue object.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if enable connection cache; otherwise, <c>false</c>.
        /// </value>
        public bool EnableConnectionCache
        {
            set { enableConnectionCache = value; }
        }


        /// <summary>
        /// Sets a value indicating whether to retrieve all message properties when receiving a message.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if should etrieve all message properties when receiving a message; otherwise, <c>false</c>.
        /// </value>
        public bool MessageReadPropertyFilterSetAll
        {
            set { messageReadPropertyFilterSetAll = value; }
        }


        /// <summary>
        /// Sets a value indicating whether to set the filter values of common Message Queuing properties
        /// to true and the integer-valued properties to their default values..
        /// </summary>
        /// <value>
        /// 	<c>true</c> if should set the filter values of common Message Queuing properties; otherwise, <c>false</c>.
        /// </value>
        public bool MessageReadPropertyFilterSetDefaults
        {
            set { messageReadPropertyFilterSetDefaults = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the queue is a remote queue.
        /// </summary>
        /// <remarks>
        /// The operations that one can perform on the MessageQueue depend on if it is local or remote, for
        /// example checking if it is transactional.  This is very difficult to determine programmatically.
        /// The property was made virtual so it can be overridden to take into account custom heuristics you
        /// may want to use to determine this programmatically.
        /// </remarks>
        /// <value><c>true</c> if remote queue; otherwise, <c>false</c>.</value>
        public virtual bool RemoteQueue
        {
            get { return remoteQueue; }
            set { remoteQueue = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the remote queue is transactional.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the remote queue is transactional; otherwise, <c>false</c>.
        /// </value>
        public virtual bool RemoteQueueIsTransactional
        {
            get { return remoteQueueIsTransactional; }
            set { remoteQueueIsTransactional = value; }
        }

        #region IFactoryObject Members

        /// <summary>
        /// Retrun a configured MessageQueue object.
        /// </summary>
        /// <returns>A newly configured MessageQueue object</returns>
        public object GetObject()
        {
            if (MessageCreatorDelegate != null)
            {
                return MessageCreatorDelegate();
            }
            else
            {
                MessageQueue mq = new MessageQueue(Path, DenySharedReceive, EnableCache, AccessMode);
                if (messageReadPropertyFilterSetDefaults)
                {
                    mq.MessageReadPropertyFilter.SetDefaults();
                }
                if (messageReadPropertyFilterSetAll)
                {
                    mq.MessageReadPropertyFilter.SetAll();
                }
                return mq;
            }
        }

        /// <summary>
        /// Return the <see cref="System.Type"/> of object that this
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
        /// <see langword="null"/> if not known in advance.
        /// </summary>
        /// <value>The type System.Messaging.MessageQueue</value>
        public Type ObjectType
        {
            get { return typeof (MessageQueue); }
        }

        /// <summary>
        /// Is the object managed by this factory a singleton or a prototype?
        /// </summary>
        /// <value>return false, a new object will be created for each request of the object</value>
        public bool IsSingleton
        {
            get { return false; }
        }

        #region IConfigurableFactoryObject Members

        /// <summary>
        /// Gets the template object definition that should be used
        /// to configure the instance of the object managed by this factory.
        /// </summary>
        /// <value>The object definition to configure the factory's product</value>
        public IObjectDefinition ProductTemplate
        {
            get { return productTemplate; }
            set { productTemplate = value; }
        }

        #endregion

        #endregion

        /// <summary>
        /// Set the name of the object in the object factory that created this object.
        /// </summary>
        /// <value>The name of the object in the factory.</value>
        /// <remarks>
        /// 	<p>
        /// Invoked after population of normal object properties but before an init
        /// callback like <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// method or a custom init-method.
        /// </p>
        /// </remarks>
        public string ObjectName
        {
            set { objectName = value; }
            get { return ObjectName; }
        }
    }
}
