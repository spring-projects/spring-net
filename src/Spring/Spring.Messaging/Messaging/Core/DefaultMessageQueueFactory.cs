#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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


using System.Collections;
using System.Messaging;
using Spring.Context;
using Spring.Messaging.Support.Converters;
using Spring.Threading;
using Spring.Util;

namespace Spring.Messaging.Core
{
    /// <summary>
    /// A <see cref="IMessageQueueFactory"/> implementation that caches MessageQueue and IMessageConverter
    /// instances.
    /// </summary>
    /// <author>Mark Pollack</author>
    public class DefaultMessageQueueFactory : IMessageQueueFactory, IApplicationContextAware
    {
        private static readonly string QUEUE_DICTIONARY_SLOTNAME =
            UniqueKey.GetTypeScopedString(typeof (DefaultMessageQueueFactory), "Queue");

        private static readonly string CONVERTER_DICTIONARY_SLOTNAME =
            UniqueKey.GetTypeScopedString(typeof (DefaultMessageQueueFactory), "Converter");

        private IApplicationContext applicationContext;

        #region IMessageQueueFactory Members

        public MessageQueue CreateMessageQueue(string messageQueueObjectName)
        {
            AssertUtils.ArgumentHasText(messageQueueObjectName, "DefaultMessageQueueObjectName");
            IDictionary queues = LogicalThreadContext.GetData(QUEUE_DICTIONARY_SLOTNAME) as IDictionary;
            if (queues == null)
            {
                queues = new Hashtable();
                LogicalThreadContext.SetData(QUEUE_DICTIONARY_SLOTNAME, queues);
            }
            if (!queues.Contains(messageQueueObjectName))
            {
                MessageQueue mq = applicationContext.GetObject(messageQueueObjectName) as MessageQueue;
                queues.Add(messageQueueObjectName, mq);
            }
            return queues[messageQueueObjectName] as MessageQueue;
        }

        public IMessageConverter CreateMessageConverter(string messgaeConverterObjectName)
        {
            AssertUtils.ArgumentHasText(messgaeConverterObjectName, "MessgaeFormatterObjectName");
            IDictionary converters = LogicalThreadContext.GetData(CONVERTER_DICTIONARY_SLOTNAME) as IDictionary;
            if (converters == null)
            {
                converters = new Hashtable();
                LogicalThreadContext.SetData(CONVERTER_DICTIONARY_SLOTNAME, converters);
            }
            if (!converters.Contains(messgaeConverterObjectName))
            {
                IMessageConverter mc = applicationContext.GetObject(messgaeConverterObjectName) as IMessageConverter;
                converters.Add(messgaeConverterObjectName, mc);
            }
            return converters[messgaeConverterObjectName] as IMessageConverter;
        }

        #endregion

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
    }
}