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
    /// Base class for listener container implementations which are based on Peeking for messages on
    /// a MessageQueue.  Peeking is the only resource efficient approach that can be used in
    /// order to have MessageQueue receipt in conjunction with transactions, either local MSMQ transactions,
    /// local ADO.NET based transactions, or DTC transactions.  See SimpleMessageListenerContainer for
    /// an implementation based on a synchronous receives and you do not require transactional support.
    /// </summary>
    /// <remarks>
    /// The number of threads that will be created for processing messages after the Peek occurs
    /// is set via the property MaxConcurrentListeners.  Each processing thread will continue to listen
    /// for messages up until the the timeout value specified by ListenerTimeLimit or until
    /// there are no more messages on the queue (which ver comes first).
    /// <para>
    /// The default value of
    /// ListenerTimeLimit is TimeSpan.Zero, meaning that only one attempt to recieve a message from the
    /// queue will be performed by each listener thread.
    /// </para>
    /// <para>
    /// The current implementation uses the standard .NET thread pool.  Future implementations will
    /// use a custom (and pluggable) thread pool.
    /// </para>
    /// </remarks>
    public abstract class AbstractPeekingMessageListenerContainer : AbstractMessageListenerContainer
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (AbstractPeekingMessageListenerContainer));

        #endregion

        #region Fields

        private Thread dispatcherThread;

        private ManualResetEvent stopEvent = new ManualResetEvent(false);

        private MessageQueue messageQueue;

        private int maxConcurrentListeners = 1;
        private bool setMaxConcurrentListenersCalled = false;

        private int activeListenerCount;
        private int scheduledListenerCount;
        private object activeListenerMonitor = new object();

        private TimeSpan listenerTimeLimit = TimeSpan.Zero;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the listener time limit to continuously receive messages.
        /// The value is specified in milliseconds.  The default value is TimeSpan.Zero,
        /// indicating to only perform one Receive operation per Peek trigger.
        /// </summary>
        /// <value>The listener time limit in millis.</value>
        public TimeSpan ListenerTimeLimit
        {
            get { return listenerTimeLimit; }
            set { listenerTimeLimit = value; }
        }

        /// <summary>
        /// Gets or sets the max concurrent listeners to receive messages.
        /// </summary>
        /// <value>The max concurrent listeners.</value>
        public int MaxConcurrentListeners
        {
            get { return maxConcurrentListeners; }
            set
            {
                if (!setMaxConcurrentListenersCalled)
                {
                    setMaxConcurrentListenersCalled = true;
                    maxConcurrentListeners = value;
                }
                else
                {
                    LOG.Info("Ignoring resetting of MaxConcurrentListeners.  Using previous value of " +
                             maxConcurrentListeners);
                }
            }
        }

        /// <summary>
        /// Gets or sets the message queue used for Peeking.
        /// </summary>
        /// <value>The message queue.</value>
        public MessageQueue MessageQueue
        {
            get { return messageQueue; }
        }

        #endregion

        #region Protected Container Lifecycle Methods

        /// <summary>
        /// Retrieves a MessageQueue instance given the MessageQueueObjectName
        /// </summary>
        protected override void DoInitialize()
        {
            messageQueue = MessageQueueFactory.CreateMessageQueue(MessageQueueObjectName);
            //TODO would initialize resources for a seperate thread pool here.
        }

        /// <summary>
        /// Wait for all listener threads to exit and closes the DefaultMessageQueue.
        /// </summary>
        protected override void DoShutdown()
        {
            stopEvent.Set();
            WaitForListenerThreadsToExit();
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
        /// Starts peeking on the DefaultMessageQueue.
        /// </summary>
        protected override void DoStart()
        {
            base.DoStart();
            stopEvent = new ManualResetEvent(false);
            dispatcherThread = new Thread(new ThreadStart(StartPeeking));
            ConfigureInitialPeekThread(dispatcherThread);
            dispatcherThread.Start();
        }

        /// <summary>
        /// Stops peeking on the message queue.
        /// </summary>
        protected override void DoStop()
        {
            base.DoStop();
            stopEvent.Set();
            CloseQueueHandle(MessageQueue);
            if (dispatcherThread != null)
            {
                LOG.Debug("Waiting to join dispatcher thread.");
                dispatcherThread.Join();
                dispatcherThread = null;
                LOG.Debug("Dispatcher thread terminated.");
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Starts peeking on the DefaultMessageQueue.  This is the method that must be called
        /// again at the end of message procesing to continue the peeking process.
        /// </summary>
        protected virtual void StartPeeking()
        {
            if (Running)
            {
                try
                {
                    IAsyncResult asynchResult = MessageQueue.BeginPeek();
                    LOG.Debug("Waiting on Peek AsyncWaitHandle");
                    int firedWaitHandle = WaitHandle.WaitAny(new WaitHandle[] {asynchResult.AsyncWaitHandle, stopEvent});
                    if (firedWaitHandle == 0)
                    {
                        PeekCompleted(asynchResult);
                    }
                    else
                    {
                        //Stopping processing.
                        return;
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error(
                        "Exception executing DefaultMessageQueue.BeginPeek.  Reinvoking after recovery interval [" +
                        RecoveryTimeSpan + "]", ex);
                    Thread.Sleep(RecoveryTimeSpan);
                    StartPeeking();
                }
            }
        }

        /// <summary>
        /// The callback when the peek has completed.  Schedule up to the maximum number of
        /// concurrent listeners to receive messages off the queue.  Delegates to the abstract
        /// method DoReceiveAndExecute so that subclasses may customize the receiving process,
        /// for example to surround the receive operation with transactional semantics.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        protected virtual void PeekCompleted(IAsyncResult asyncResult)
        {
            bool listenerThreadWillCallStartPeek = false;
            try
            {
                LOG.Debug("Peek Completed called.");

                MessageQueue.EndPeek(asyncResult);

                int numberOfListenersToSchedule = 0;

                // lock also prevents listeners that are about to exit from invoking
                // StartPeeking while new listeners are being scheduled.
                lock (activeListenerMonitor)
                {
                    numberOfListenersToSchedule = maxConcurrentListeners -
                                                  (activeListenerCount + scheduledListenerCount);

                    LOG.Debug("Submitting " + numberOfListenersToSchedule + " listener work items");

                    #region Submit to thread pool up to max number of concurrent listeners

                    for (int i = 1; i <= numberOfListenersToSchedule; i++)
                    {
                        bool wasQueued = ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveAndExecute), MessageQueue);
                        if (wasQueued)
                        {
                            scheduledListenerCount++;
                            listenerThreadWillCallStartPeek = true;
                            LOG.Debug("Queued ReceiveAndExecute listener # " + i);
                        }
                        else
                        {
                            LOG.Error("Could not submit ReceiveAndExecute work item for listener # " + i);
                        }
                    }
                    Monitor.PulseAll(activeListenerMonitor);
                }

                #endregion
            }
            catch (MessageQueueException mex)
            {
                switch ((int) mex.MessageQueueErrorCode)
                {
                    case -1073741536: // = 0xc0000120 "STATUS_CANCELLED".
                        LOG.Info("Asynchronous Peek Thread sent STATUS_CANCELLED.");
                        break;
                    default:
                        LOG.Error("MessageQueueException Peeking Message", mex);
                        break;
                }
            }
            catch (Exception e)
            {
                LOG.Error("Exception Peeking Message", e);
            }
            finally
            {
                if (listenerThreadWillCallStartPeek == false && Running)
                {
                    LOG.Warn(
                        "Could not queue any listeners onto the thread pool.  Calling BeginPeek again after delay of " +
                        RecoveryTimeSpan);
                    Thread.Sleep(RecoveryTimeSpan);
                    StartPeeking();
                }
            }
        }

        /// <summary>
        /// Execute the listener for a message received from the given queue
        /// wrapping the entire operation in an external transaction if demanded.
        /// </summary>
        /// <param name="state">The DefaultMessageQueue upon which the call to receive should be
        /// called.</param>
        protected virtual void ReceiveAndExecute(object state)
        {
            bool messageReceived = true;
            bool listenerTimeOut = false;

            MessageQueue mq = state as MessageQueue;
            if (mq == null)
            {
                throw new ArgumentException("Expected asynchronous state object to be of the type DefaultMessageQueue");
            }

            try
            {
                LOG.Debug("Executing ReceiveAndExecute");

                #region Increment Active Listener Count

                lock (activeListenerMonitor)
                {
                    activeListenerCount++;
                    scheduledListenerCount--;
                    LOG.Debug("ActiveListenerCount = " + activeListenerCount);
                    LOG.Debug("ScheduledListenerCount = " + scheduledListenerCount);
                    Monitor.PulseAll(activeListenerMonitor);
                }

                #endregion

                DateTime expirationTime = DateTime.Now.Add(ListenerTimeLimit);
                while (!listenerTimeOut && messageReceived)
                {
                    //Subclasses to perform receive operation
                    messageReceived = DoReceiveAndExecute(mq);

                    if (ListenerTimeLimit == TimeSpan.Zero)
                    {
                        listenerTimeOut = true;
                        LOG.Trace("No listener timelimit specified, exiting recieve loop after one iteration.");
                    }
                    else if (DateTime.Now >= expirationTime)
                    {
                        listenerTimeOut = true;
                        LOG.Trace("Listener timeout, exiting receive loop.");
                    }
                    else
                    {
                        LOG.Trace("Continuing receive loop.");
                    }
                }
            }
            catch (Exception ex)
            {
                messageReceived = false;
                LOG.Error("Error receiving message from DefaultMessageQueue = [" + mq.Path + "]", ex);
            }
            finally
            {
                LOG.Debug("Exiting ReceiveAndExecute");

                #region Decrementing Listener Count and call StartPeeking if last listener or there are still messages to process

                lock (activeListenerMonitor)
                {
                    activeListenerCount--;
                    LOG.Debug("ActiveListenerCount = " + activeListenerCount);
                    LOG.Trace("ListenerTimeout = " + listenerTimeOut + ", MessageRecieved = " + messageReceived);
                    if (activeListenerCount == 0)
                    {
                        LOG.Debug("All processing threads ended - calling StartPeek again.");
                        //last active worker thread needs to restart the peeking process
                        StartPeeking();
                    }
                    else if (listenerTimeOut && messageReceived)
                    {
                        LOG.Debug(
                            "Processing thread ended due to timeout and last recieve operation was successfull, calling StartPeek again.");
                        StartPeeking();
                    }
                    Monitor.PulseAll(activeListenerMonitor);
                }

                #endregion
            }
        }

        /// <summary>
        /// Subclasses perform a receive opertion on the message queue and execute the
        /// message listener
        /// </summary>
        /// <param name="mq">The DefaultMessageQueue.</param>
        /// <returns>true if received a message, false otherwise</returns>
        protected abstract bool DoReceiveAndExecute(MessageQueue mq);


        /// <summary>
        /// Waits for listener threads to exit.
        /// </summary>
        protected virtual void WaitForListenerThreadsToExit()
        {
            try
            {
                lock (activeListenerMonitor)
                {
                    if (activeListenerCount > 0)
                    {
                        while (activeListenerCount > 0)
                        {
                            LOG.Debug("Waiting for termination of " + activeListenerCount + " listener threads.");
                            Monitor.Wait(activeListenerMonitor);
                        }
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                Thread.CurrentThread.Interrupt();
            }
        }


        /// <summary>
        /// Configures the initial peek thread, setting it to be a background thread.
        /// Can be overridden in subclasses.
        /// </summary>
        /// <param name="thread">The peek thread.</param>
        protected virtual void ConfigureInitialPeekThread(Thread thread)
        {
            thread.IsBackground = true;
        }


        /// <summary>
        /// Template method that gets called right when a new message has been received,
        /// before attempting to process it. Allows subclasses to react to the event
        /// of an actual incoming message, for example adapting their consumer count.
        /// </summary>
        /// <param name="message">The message.</param>
        protected virtual void MessageReceived(Message message)
        {
        }

        /// <summary>
        /// Template method that gets called right before a new message is received, i.e.
        /// messageQueue.Receive().
        /// </summary>
        /// <remarks>It allows subclasses to modify the state of the MessageQueue
        /// before receiving which maybe required when using remote queues</remarks>
        /// <param name="messageQueue"></param>
        protected virtual void BeforeMessageReceived(MessageQueue messageQueue)
        {
        }

        #endregion
    }
}
