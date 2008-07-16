#region License

/*
 * Copyright 2002-2008 the original author or authors.
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
using System.Messaging;
using Spring.Objects.Factory.Config;

namespace Spring.Messaging.Support
{
    /// <summary>
    /// Factory for creating MessageQueues
    /// </summary>
    /// <author>Mark Pollack</author>
    public class MessageQueueFactoryObject : IConfigurableFactoryObject
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

        //myQueue.MessageReadPropertyFilter.SetAll();

        /// <summary>
        /// Gets or sets the path used to creat DefaultMessageQueue instance.
        /// </summary>
        /// <value>The location of the queue referenced by the DefaultMessageQueue.</value>
        public string Path
        {
            get { return path; }
            set { path = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to create the DefaultMessageQueue instance with 
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
        /// is different than the default value when creating a DefaultMessageQueue object.
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

        #region IFactoryObject Members

        public object GetObject()
        {
            MessageQueue.EnableConnectionCache = enableConnectionCache;
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

        /// <summary>
        /// Return the <see cref="System.Type"/> of object that this
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
        /// <see langword="null"/> if not known in advance.
        /// </summary>
        /// <value>The type DefaultMessageQueue</value>
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
    }
}