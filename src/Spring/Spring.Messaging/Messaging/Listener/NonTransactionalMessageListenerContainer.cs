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

#if NETSTANDARD
using Experimental.System.Messaging;
#else
using System.Messaging;
#endif
using Microsoft.Extensions.Logging;

namespace Spring.Messaging.Listener;

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
    private static readonly ILogger LOG = LogManager.GetLogger(typeof(NonTransactionalMessageListenerContainer));

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
            if (LOG.IsEnabled(LogLevel.Trace))
            {
                LOG.LogTrace("Receiving message with zero timeout for queue = [" + mq.Path + "]");
            }

            BeforeMessageReceived(mq);
            message = mq.Receive(TimeSpan.Zero);
        }
        catch (MessageQueueException ex)
        {
            if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
            {
                //expected to occur occasionally

                if (LOG.IsEnabled(LogLevel.Trace))
                {
                    LOG.LogTrace("MessageQueueErrorCode.IOTimeout: No message available to receive.  May have been processed by another thread.");
                }

                return false; // no more peeking unless this is the last listener thread
            }
            else
            {
                // A real issue in receiving the message

                if (LOG.IsEnabled(LogLevel.Error))
                {
                    LOG.LogError("Error receiving message from MessageQueue [" + mq.Path +
                                 "], closing queue and clearing connection cache.");
                }

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
            if (LOG.IsEnabled(LogLevel.Trace))
            {
                LOG.LogTrace("Message recieved is null from Queue = [" + mq.Path + "]");
            }

            return false; // no more peeking unless this is the last listener thread
        }

        try
        {
            if (LOG.IsEnabled(LogLevel.Debug))
            {
                LOG.LogDebug("Received message [" + message.Id + "] on queue [" + mq.Path + "]");
            }

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
