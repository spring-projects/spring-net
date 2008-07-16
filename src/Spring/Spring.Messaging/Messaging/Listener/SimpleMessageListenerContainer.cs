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
using System.Threading;
using Common.Logging;

namespace Spring.Messaging.Listener
{
    public class SimpleMessageListenerContainer : AbstractMessageListenerContainer
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (SimpleMessageListenerContainer));

        private Thread dispatcherThread;

        protected ManualResetEvent stopEvent = new ManualResetEvent(false);

        private IExceptionHandler exceptionHandler;

        private MessageQueue messageQueue;

        public MessageQueue MessageQueue
        {
            get { return messageQueue; }
            set { messageQueue = value; }
        }

        public IExceptionHandler ExceptionHandler
        {
            get { return exceptionHandler; }
            set { exceptionHandler = value; }
        }

        #endregion

        protected override void DoInitialize()
        {
            messageQueue = ApplicationContext.GetObject(MessageQueueObjectName, typeof (MessageQueue)) as MessageQueue;
        }

        /// <summary>
        /// Unsubscribe for messaging events and closethe queue
        /// </summary>
        protected override void DoShutdown()
        {
            CloseQueueHandle(MessageQueue);
            if (dispatcherThread != null)
            {
                LOG.Debug("Waiting to join dispatcher thread.");
                dispatcherThread.Join();
                dispatcherThread = null;
                LOG.Debug("Dispatcher thread terminated.");
            }
        }

        /// <summary>
        /// Re-initializes this container's consumers,  if not initialized already.
        /// </summary>
        protected override void DoStart()
        {
            base.DoStart();
            stopEvent = new ManualResetEvent(false);
            dispatcherThread = new Thread(new ThreadStart(StartListening));
            dispatcherThread.Start();
        }

        /// <summary>
        /// Stops the container from listening to message events.
        /// </summary>
        public override void DoStop()
        {
            base.DoStop();
            CloseQueueHandle(MessageQueue);
            stopEvent.Set();
            if (dispatcherThread != null)
            {
                LOG.Debug("Waiting to join dispatcher thread.");
                dispatcherThread.Join();
                dispatcherThread = null;
                LOG.Debug("Dispatcher thread terminated.");
            }
        }

        /// <summary>
        /// Starts listening off the queue.
        /// </summary>
        protected virtual void StartListening()
        {
            if (Running)
            {
                try
                {
                    IAsyncResult asynchResult = MessageQueue.BeginReceive();
                    LOG.Debug("WaitAny");
                    int firedWaitHandle = WaitHandle.WaitAny(new WaitHandle[] {asynchResult.AsyncWaitHandle, stopEvent});
                    if (firedWaitHandle == 0)
                    {
                        ReceiveCompleted(asynchResult);
                    }
                    else
                    {
                        //Do the endreceive?
                        return;
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error(
                        "Exception executing DefaultMessageQueue.BeginReceive.  Reinvoking after recovery interval [" +
                        RecoveryTimeSpan + "]", ex);
                    Thread.Sleep(RecoveryTimeSpan);
                    StartListening();
                }
            }
        }

        protected virtual void ReceiveCompleted(IAsyncResult asyncResult)
        {
            Message message;

            #region Receive Message

            try
            {
                LOG.Debug("ReceiveCompleted called.");
                // Get reference to the queue.

                // End the asynchronous receive operation.
                message = MessageQueue.EndReceive(asyncResult);
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                {
                    if (LOG.IsTraceEnabled)
                    {
                        LOG.Trace("IOTimeout: Message to receive was already processed by another thread.");
                    }
                }
                else
                {
                    // A real issue in receiving the message
                    LOG.Error("Error receiving message from DefaultMessageQueue = [" + MessageQueue.QueueName + "]");
                    Thread.Sleep(RecoveryTimeSpan);
                    //InvokeReceiveExceptionHandler(ex);?
                }
                MessageQueue.Close();
                StartListening();
                return;
            }

            #endregion

            if (message == null)
            {
                LOG.Error("Message recieved is null");
                StartListening();
                return;
            }

            try
            {
                bool queued = ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteListener), message);

                #region Could not enqueue

                if (!queued)
                {
                    LOG.Warn("Could not queue work item into thread pool. Retrying.");
                    Thread.Sleep(RecoveryTimeSpan);
                }

                #endregion
            }
            catch (Exception e)
            {
                LOG.Error("Error enqueue message in thread pool DefaultMessageQueue = [" + MessageQueue.QueueName + "]", e);
                Thread.Sleep(RecoveryTimeSpan);
            }
            finally
            {
                StartListening();
            }
        }

        protected virtual void ExecuteListener(object state)
        {
            Message message = state as Message;
            try
            {
                DoExecuteListener(message);
            }
            catch (Exception e)
            {
                HandleListenerException(e, message);
            }
        }

        private void HandleListenerException(Exception e, Message message)
        {
            IExceptionHandler exceptionHandler = ExceptionHandler;
            if (exceptionHandler != null)
            {
                exceptionHandler.OnException(e, message);
            }
        }
    }
}