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
using Spring.Objects.Factory;

namespace Spring.Messaging.Listener
{
    /// <summary>
    /// Provides basic lifecyle management methods for implementing a message listener container.
    /// </summary>
    /// <remarks>
    /// This base class does not assume any specific listener programming model
    /// or listener invoker mechanism. It just provides the general runtime
    /// lifecycle management needed for any kind of message-based listening mechanism.
    /// <para>
    /// For a concrete listener programming model, check out the
    /// <see cref="AbstractMessageListenerContainer"/> subclass. For a concrete listener
    /// invoker mechanism, check out the <see cref="NonTransactionalMessageListenerContainer"/>,
    /// <see cref="TransactionalMessageListenerContainer"/>, or
    /// <see cref="DistributedTxMessageListenerContainer"/> classes.
    /// </para>
    /// </remarks>
    /// <author>Mark Pollack</author>
    public abstract class AbstractListenerContainer : IInitializingObject, IObjectNameAware, IDisposable
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof (AbstractListenerContainer));

        #endregion

        private bool autoStartup = true;

        private string objectName;
        private bool active;
        private bool running;
        private object lifecycleMonitor = new object();

        #region Properties

        /// <summary>
        /// Sets a value indicating whether to automatically start the container after initialization.
        /// Default is "true"; set this to "false" to allow for manual startup though the
        /// <see cref="Start"/> method.
        /// </summary>
        /// <value><c>true</c> if autostartup; otherwise, <c>false</c>.</value>
        public bool AutoStartup
        {
            set { autoStartup = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this Container is active,
        /// that is, whether it has been set up but not shut down yet.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        public bool Active
        {
            get
            {
                lock (lifecycleMonitor)
                {
                    return active;
                }
            }
        }


        /// <summary>
        /// Gets a value indicating whether this Container is running,
        /// that is whether it has been started and not stopped yet.
        /// </summary>
        /// <value><c>true</c> if running; otherwise, <c>false</c>.</value>
        public bool Running
        {
            get
            {
                lock (lifecycleMonitor)
                {
                    return (running && RunningAllowed());
                }
            }
        }

        #region IObjectNameAware Members

        /// <summary>
        /// Return the object name that this listener container has been assigned
        /// in its containing object factory, if any.
        /// </summary>
        public string ObjectName
        {
            set { objectName = value; }
            get { return objectName; }
        }

        #endregion

        #endregion

        #region IInitializingObject Members

        /// <summary>
        /// Delegates to <see cref="ValidateConfiguration"/> and <see cref="Initialize"/>
        /// </summary>
        public void AfterPropertiesSet()
        {
            ValidateConfiguration();
            Initialize();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Calls <see cref="Shutdown"/> when the application context destroys the container instance.
        /// </summary>
        public void Dispose()
        {
            Shutdown();
        }

        #endregion

        /// <summary>
        /// Validates the configuration of this container
        /// The default implementation is empty. To be overridden in subclasses.
        /// </summary>
        protected virtual void ValidateConfiguration()
        {
        }

        /// <summary>
        /// Initializes this container.  Calls the abstract method <see cref="DoInitialize"/> to
        /// initialize the listening infrastructure (i.e. subclasses will typically
        /// resolve a MessageQueue instance from a MessageQueueObjectName) and then calls
        /// the abstract method DoStart if the property <see cref="AutoStartup"/> is set to true,
        /// </summary>
        public virtual void Initialize()
        {
            lock (lifecycleMonitor)
            {
                active = true;
                Monitor.PulseAll(lifecycleMonitor);
            }
            DoInitialize();
            if (autoStartup)
            {
                DoStart();
            }

        }

        /// <summary>
        /// Sets the container state to inactive and not running, calls template method
        /// <see cref="DoShutdown"/>
        /// </summary>
        public virtual void Shutdown()
        {
            LOG.Debug("Shutting down MessageListenerContainer");
            lock (lifecycleMonitor)
            {
                running = false;
                active = false;
                Monitor.PulseAll(lifecycleMonitor);
            }
            DoShutdown();
        }

        /// <summary>
        /// Starts this container.
        /// </summary>
        public virtual void Start()
        {
            DoStart();
        }

        /// <summary>
        /// Sets the state to running, can be overridden in subclasses.
        /// </summary>
        protected virtual void DoStart()
        {
            lock (lifecycleMonitor)
            {
                running = true;
                Monitor.PulseAll(lifecycleMonitor);
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public virtual void Stop()
        {
            DoStop();
        }

        /// <summary>
        /// Template method suitable for overriding that stops the container.
        /// </summary>
        protected virtual void DoStop()
        {
            lock (lifecycleMonitor)
            {
                running = false;
                Monitor.PulseAll(lifecycleMonitor);
            }
        }

        /// <summary>
        /// Check whether this container's listeners are generally allowed to run.
        /// </summary>
        /// <remarks>
        /// This implementation always returns <code>true</code>; the default 'running'
        /// state is purely determined by <see cref="Start"/> and <see cref="Stop"/>
        /// <para>
        /// Subclasses may override this method to check against temporary
        /// conditions that prevent listeners from actually running. In other words,
        /// they may apply further restrictions to the 'running' state, returning
        /// <code>false</code> if such a restriction prevents listeners from running.
        /// </para>
        /// </remarks>
        /// <returns><code>false</code> if such a restriction prevents listeners from running.</returns>
        protected virtual bool RunningAllowed()
        {
            return true;
        }

        #region Abstract Methods

        /// <summary>
        /// Subclasses need to implement this method for their specific
        /// listener management process.
        /// </summary>
        protected abstract void DoInitialize();

        /// <summary>
        /// Subclasses need to implement this method for their specific
        /// listener management process.
        /// </summary>
        protected abstract void DoShutdown();

        #endregion
    }
}
