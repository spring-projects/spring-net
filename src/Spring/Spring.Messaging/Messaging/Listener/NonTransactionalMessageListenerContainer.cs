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
    /// An implementation of a Peeking based MessageListener container that does not surround the
    /// receive operation with a transaction.
    /// </summary>
    /// <remarks>
    /// Exceptions that occur during message processing are handled by an instance
    /// of <see cref="IExceptionHandler"/>.
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class NonTransactionalMessageListenerContainer : AbstractPeekingMessageListenerContainer
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (NonTransactionalMessageListenerContainer));

        #endregion

        private IExceptionHandler exceptionHandler;


        /// <summary>
        /// Gets or sets the exception handler.
        /// </summary>
        /// <value>The exception handler.</value>
        public IExceptionHandler ExceptionHandler
        {
            get { return exceptionHandler; }
            set { exceptionHandler = value; }
        }


        /// <summary>
        /// Handles the listener exception.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="message">The message delivered that resultd in an processing exception.</param>
        protected virtual void HandleListenerException(Exception e, Message message)
        {
            IExceptionHandler exceptionHandler = ExceptionHandler;
            if (exceptionHandler != null)
            {
                exceptionHandler.OnException(e, message);
            }
        }

        /// <summary>
        /// Perform a receive opertion on the message queue and execute the
        /// message listener
        /// </summary>
        /// <param name="mq">The MessageQueue.</param>
        /// <returns>
        /// true if received a message, false otherwise
        /// </returns>
        protected override bool DoReceiveAndExecute(MessageQueue mq)
        {
            Message message = null;
            try
            {
                #region Logging

                if (LOG.IsTraceEnabled)
                {
                    LOG.Trace("Receiving message with zero timeout for queue = [" + mq.Path + "]");
                }

                #endregion
                BeforeMessageReceived(mq);
                message = mq.Receive(TimeSpan.Zero);
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                {
                    //expected to occur occasionally

                    #region Logging

                    if (LOG.IsTraceEnabled)
                    {
                        LOG.Trace(
                            "MessageQueueErrorCode.IOTimeout: No message available to receive.  May have been processed by another thread.");
                    }

                    #endregion

                    return false; // no more peeking unless this is the last listener thread
                }
                else
                {
                    // A real issue in receiving the message

                    #region Logging

                    if (LOG.IsErrorEnabled)
                    {
                        LOG.Error("Error receiving message from MessageQueue [" + mq.Path +
                                  "], closing queue and clearing connection cache.");
                    }

                    #endregion

                    lock (messageQueueMonitor)
                    {
                        mq.Close();
                        MessageQueue.ClearConnectionCache();
                    }
                    throw; // will log exception.
                }
            }

            if (message == null)
            {
                #region Logging

                if (LOG.IsTraceEnabled)
                {
                    LOG.Trace("Message recieved is null from Queue = [" + mq.Path + "]");
                }

                #endregion

                return false; // no more peeking unless this is the last listener thread
            }

            try
            {
                #region Logging

                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug("Received message [" + message.Id + "] on queue [" + mq.Path + "]");
                }

                #endregion

                MessageReceived(message);
                DoExecuteListener(message);
            }
            catch (Exception ex)
            {
                HandleListenerException(ex, message);
            }
            finally
            {
                message.Dispose();
            }
            return true;
        }
    }
}
