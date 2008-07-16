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
using Common.Logging;

namespace Spring.Messaging.Listener
{
    public class SendToQueueDistributedTransactionExceptionHandler : AbstractSendToQueueExceptionHandler,
                                                                     IDistributedTransactionExceptionHandler
    {
        #region Logging Definition

        private static readonly ILog LOG =
            LogManager.GetLogger(typeof (SendToQueueDistributedTransactionExceptionHandler));

        #endregion

        #region IDistributedTransactionExceptionHandler Members

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

        public void HandlePoisonMessage(Message message)
        {
            SendMessageToQueue(message);
        }

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

        protected virtual void SendMessageToQueue(Message message)
        {
            MessageQueue mq = MessageQueueFactory.CreateMessageQueue(MessageQueueObjectName);
            try
            {
                #region Logging

                if (LOG.IsInfoEnabled)
                {
                    LOG.Info("Sending message with id = [" + message.Id + "] to queue [" + mq.QueueName + "].");
                }

                #endregion

                mq.Send(message, MessageQueueTransactionType.Automatic);
            }
            catch (Exception e)
            {
                #region Logging

                if (LOG.IsErrorEnabled)
                {
                    LOG.Error("Could not send message with id = [" + message.Id + "] to queue [" + mq.QueueName + "].", e);
                    LOG.Error("Message will not be processed.  Message Body = " + message.Body);
                }

                #endregion
            }
        }
    }
}