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
    /// Keeps track of the Message's Id property in memory with a count of how many times an 
    /// exception has occurred. If that count is greater than the handler's MaxRetry count it 
    /// will be sent to another queue using the provided MessageQueueTransaction. The queue to 
    /// send the message to is specified via the property MessageQueueObjectName.
    /// </summary>
    public class SendToQueueExceptionHandler : AbstractSendToQueueExceptionHandler, IMessageTransactionExceptionHandler
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (SendToQueueExceptionHandler));

        #endregion

        #region Fields

        private string[] messageAlreadyProcessedExceptionNames;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the exception anmes that indicate the message has already
        /// been processed.  If the exception thrown matches one of these names then
        /// the returned TransactionAction is Commit to remove it from the queue.
        /// </summary>
        /// <remarks>The name test is thrownException.GetType().Name.IndexOf(exceptionName) >= 0</remarks>
        /// <value>The message already processed exception types.</value>
        public string[] MessageAlreadyProcessedExceptionNames
        {
            set { messageAlreadyProcessedExceptionNames = value; }
            get { return messageAlreadyProcessedExceptionNames; }
        }

        #endregion

        #region IMessageTransactionExceptionHandler Members

        /// <summary>
        /// Called when an exception is thrown during listener processing under the
        /// scope of a <see cref="MessageQueueTransaction"/>.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageQueueTransaction">The message queue transaction.</param>
        /// <returns>
        /// An action indicating if the caller should commit or rollback the
        /// <see cref="MessageQueueTransaction"/>
        /// </returns>
        public TransactionAction OnException(Exception exception, Message message,
                                             MessageQueueTransaction messageQueueTransaction)
        {
            if (IsMessageAlreadyProcessedException(exception))
            {
                return TransactionAction.Commit;
            }

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
                LOG.Warn("Message Error Count = [" + messageStats.Count + "] for message id = [" + messageId + "]");

                if (messageStats.Count > MaxRetry)
                {
                    LOG.Info("Maximum number of redelivery attempts exceeded for message id = [" + messageId + "]");
                    messageMap.Remove(messageId);
                    return SendMessageToQueue(message, messageQueueTransaction);
                }
                else
                {
                    LOG.Warn("Rolling back delivery of message id [" + messageId + "]");
                    return TransactionAction.Rollback;
                }
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Determines whether this exception was already processed.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// 	<c>true</c> if the exception was already processed; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsMessageAlreadyProcessedException(Exception exception)
        {
            if (MessageAlreadyProcessedExceptionNames != null)
            {
                foreach (string exceptionName in MessageAlreadyProcessedExceptionNames)
                {
                    if (exception.GetType().Name.IndexOf(exceptionName) >= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Sends the message to queue.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageQueueTransaction">The message queue transaction.</param>
        /// <returns>TransactionAction.Commit</returns>
        protected virtual TransactionAction SendMessageToQueue(Message message,
                                                               MessageQueueTransaction messageQueueTransaction)
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

                ProcessExceptionalMessage(message);
                mq.Send(message, messageQueueTransaction);
            }
            catch (Exception e)
            {
                #region Logging

                if (LOG.IsErrorEnabled)
                {
                    LOG.Error("Could not send message with id = [" + message.Id + "] to queue [" + mq.Path + "].",e);
                    LOG.Error("Message will not be processed.  Message Body = " + message.Body);
                }

                #endregion
            }
            return TransactionAction.Commit;
        }

        /// <summary>
        /// Template method called before the message that caused the exception is
        /// send to another queue.  The default behavior is to set the CorrelationId
        /// to the current message's Id value for tracking purposes.  Subclasses
        /// can use other means, perhaps using the AppSpecific field or modifying the
        /// body of the message to a known shared format that keeps track of
        /// the full 'lifecycle' of the message as it goes from queue-to-queue.
        /// </summary>
        /// <param name="message">The message.</param>
        protected virtual void ProcessExceptionalMessage(Message message)
        {
            if (message.CorrelationId == null)
            {
                message.CorrelationId = message.Id;
            }
        }

        #endregion
    }


    internal class MessageStats
    {
        private int count;

        public int Count
        {
            get { return count; }
            set { count = value; }
        }
    }
}
