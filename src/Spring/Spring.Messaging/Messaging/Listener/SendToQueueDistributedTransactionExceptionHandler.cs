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

using Common.Logging;

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif

namespace Spring.Messaging.Listener
{
    /// <summary>
    /// detects poison messages by tracking the Message Id  property in memory with a count of how many
    /// times an exception has occurred. If that count is greater than the handler's MaxRetry count it
    /// will be sent to another queue. The queue to send the message to is specified via the property M
    /// essageQueueObjectName.
    /// </summary>
    /// <remarks>Exception handler when using DistributedTxMessageListenerContainer</remarks>
    public class SendToQueueDistributedTransactionExceptionHandler : AbstractSendToQueueExceptionHandler,
                                                                     IDistributedTransactionExceptionHandler
    {
        #region Logging Definition

        private static readonly ILog LOG =
            LogManager.GetLogger(typeof (SendToQueueDistributedTransactionExceptionHandler));

        #endregion

        #region IDistributedTransactionExceptionHandler Members

        /// <summary>
        /// Determines whether the incoming message is a poison message.  This method is
        /// called before the <see cref="IMessageListener"/> is invoked.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        /// <returns>
        /// 	<c>true</c> if it is a poison message; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The <see cref="DistributedTxMessageListenerContainer"/> will call
        /// <see cref="HandlePoisonMessage"/> if this method returns true and will
        /// then commit the distibuted transaction (removing the message from the queue).
        /// </remarks>
        public bool IsPoisonMessage(Message message)
        {
            string messageId = message.Id;
            lock (messageMapMonitor)
            {
                MessageStats messageStats = null;
                if (messageMap.Contains(messageId))
                {
                    messageStats = (MessageStats) messageMap[messageId];
                    if (messageStats.Count > MaxRetry)
                    {
                        LOG.Warn("Message with id = [" + message.Id + "] detected as poison message.");
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Handles the poison message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void HandlePoisonMessage(Message message)
        {
            SendMessageToQueue(message);
        }

        /// <summary>
        /// Called when an exception is thrown in listener processing.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        public void OnException(Exception exception, Message message)
        {
            string messageId = message.Id;
            lock (messageMapMonitor)
            {
                MessageStats messageStats = null;
                if (messageMap.Contains(messageId))
                {
                    messageStats = (MessageStats) messageMap[messageId];
                }
                else
                {
                    messageStats = new MessageStats();
                    messageMap[messageId] = messageStats;
                }
                messageStats.Count++;
                LOG.Warn("Message Error Count = [" + messageStats.Count + "] for message id = [" + messageId +
                         "]");
            }
        }

        #endregion

        /// <summary>
        /// Sends the message to queue.
        /// </summary>
        /// <param name="message">The message.</param>
        protected virtual void SendMessageToQueue(Message message)
        {
            MessageQueue mq = MessageQueueFactory.CreateMessageQueue(MessageQueueObjectName);
            try
            {
                #region Logging

                if (LOG.IsInfoEnabled)
                {
                    LOG.Info("Sending message with id = [" + message.Id + "] to queue [" + mq.Path + "].");
                }

                #endregion

                mq.Send(message, MessageQueueTransactionType.Automatic);
            }
            catch (Exception e)
            {
                #region Logging

                if (LOG.IsErrorEnabled)
                {
                    LOG.Error("Could not send message with id = [" + message.Id + "] to queue [" + mq.Path + "].", e);
                    LOG.Error("Message will not be processed.  Message Body = " + message.Body);
                }

                #endregion
            }
        }
    }
}
