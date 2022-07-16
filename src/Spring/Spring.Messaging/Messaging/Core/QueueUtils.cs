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

using Spring.Context;
using Spring.Messaging.Support.Converters;
using Spring.Objects.Factory.Support;
using Spring.Transaction.Support;

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

namespace Spring.Messaging.Core
{
    /// <summary>
    /// Utility methods to support Spring's MSMQ functionality
    /// </summary>
    public class QueueUtils
    {

        /// <summary>
        /// Registers the default message converter with the application context.
        /// </summary>
        /// <param name="applicationContext">The application context.</param>
        /// <returns>The name of the message converter to use for lookups with
        /// <see cref="DefaultMessageQueueFactory"/>.
        /// </returns>
        public static string RegisterDefaultMessageConverter(IApplicationContext applicationContext)
        {
            //Create a default message converter to use.
            RootObjectDefinition rod = new RootObjectDefinition(typeof(XmlMessageConverter));
            rod.PropertyValues.Add("TargetTypes", new Type[] { typeof(String) });
            rod.IsSingleton = false;
            IConfigurableApplicationContext ctx = (IConfigurableApplicationContext)applicationContext;
            DefaultListableObjectFactory of = (DefaultListableObjectFactory)ctx.ObjectFactory;
            string messageConverterObjectName = "__XmlMessageConverter__";
            if (!applicationContext.ContainsObjectDefinition(messageConverterObjectName))
            {
                of.RegisterObjectDefinition(messageConverterObjectName, rod);
            }
            return messageConverterObjectName;

        }

        /// <summary>
        /// Gets the message queue transaction from thread local storage
        /// </summary>
        /// <param name="resourceFactory">The resource factory.</param>
        /// <returns>null if not found in thread local storage</returns>
        public static MessageQueueTransaction GetMessageQueueTransaction(IResourceFactory resourceFactory)
        {
            MessageQueueResourceHolder resourceHolder =
                (MessageQueueResourceHolder)
                TransactionSynchronizationManager.GetResource(
                    MessageQueueTransactionManager.CURRENT_TRANSACTION_SLOTNAME);
            if (resourceHolder != null)
            {
                return resourceHolder.MessageQueueTransaction;
            }
            else
            {
                return null;
            }
        }

    }

    internal class MessageQueueResourceSynchronization : ITransactionSynchronization
    {
        private object resourceKey;

        private MessageQueueResourceHolder resourceHolder;

        private bool holderActive = true;

        public MessageQueueResourceSynchronization(MessageQueueResourceHolder resourceHolder, object resourceKey)
        {
            this.resourceHolder = resourceHolder;
            this.resourceKey = resourceKey;
        }

        #region ITransactionSynchronization Members

        public void Suspend()
        {
            if (holderActive)
            {
                TransactionSynchronizationManager.UnbindResource(resourceKey);
            }
        }

        public void Resume()
        {
            if (holderActive)
            {
                TransactionSynchronizationManager.BindResource(resourceKey, resourceHolder);
            }
        }

        public void BeforeCompletion()
        {
            TransactionSynchronizationManager.UnbindResource(resourceKey);
            holderActive = false;
            //this.resourceHolder.closeAll();
            //TODO SPRNET-1244
        }

        public void BeforeCommit(bool readOnly)
        {

        }

        public void AfterCommit()
        {
        }

        public void AfterCompletion(TransactionSynchronizationStatus status)
        {
            //TODO SPRNET-1244
        }

        #endregion
    }

    /// <summary> Callback interface for resource creation.
    /// Serving as argument for the <code>GetMessageQueueTransaction</code> method.
    /// </summary>
    public interface IResourceFactory
    {
        /// <summary>
        /// Return whether to allow for a local transaction that is synchronized with
        /// a Spring-managed transaction (where the main transaction might be a ADO.NET-based
        /// one for a specific IDbProvider, for example), with the MSMQ transaction
        /// committing right after the main transaction.
        /// Returns whether to allow for synchronizing a local MSMQ transaction
        /// </summary>
        bool SynchedLocalTransactionAllowed { get; }
    }
}
