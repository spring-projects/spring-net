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
using Apache.NMS;
using Spring.Context;
using Spring.Messaging.Nms.Connection;
using Spring.Messaging.Nms.Support;
using Spring.Messaging.Nms.Support.IDestinations;
using Spring.Objects.Factory;

namespace Spring.Messaging.Nms.Listener
{
    /// <summary>
    /// Common base class for all containers which need to implement listening
    /// based on a Connection (either shared or freshly obtained for each attempt).
    /// Inherits basic Connection and Session configuration handling from the
    /// <see cref="NmsAccessor"/> base class.
    /// </summary>
    /// <para>
    /// This class provides basic lifecycle management, in particular management
    /// of a shared Connection. Subclasses are supposed to plug into this
    /// lifecycle, implementing the <see cref="SharedConnectionEnabled"/> as well as
    ///
    /// </para>
    /// <remarks>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    public abstract class AbstractNmsListeningContainer : NmsDestinationAccessor, ILifecycle, IObjectNameAware, IDisposable
    {
        #region Fields

        private String clientId;

        protected bool autoStartup = true;
        
        private string objectName;
        
        private IConnection sharedConnection;
        
        protected object sharedConnectionMonitor = new object();
        
        private volatile bool active = false;
        
        private bool running = false;
        
        protected object lifecycleMonitor = new object();

        #endregion

        public string ClientId
        {
            set { clientId = value; }
            get { return clientId;  }
        }

        /// <summary> Set whether to automatically start the listener after initialization.
        /// <p>Default is "true"; set this to "false" to allow for manual startup.</p>
        /// </summary>
        public virtual bool AutoStartup
        {
            set { this.autoStartup = value; }
        }

        public bool IsRunning
        {
            get
            {
                lock (lifecycleMonitor)
                {
                    return running;
                }
            }
        }

        /// <summary> Return whether a shared NMS IConnection should be maintained
        /// by this listener container base class.
        /// </summary>
        /// <seealso cref="AbstractMessageListenerContainer.SharedConnection">
        /// </seealso>
        protected abstract bool SharedConnectionEnabled { get; }

        public void Dispose()
        {
            Shutdown();
        }

        virtual public bool Active
        {
            get
            {
                lock (this.lifecycleMonitor)
                {
                    return this.active;
                }
            }

        }

        public string ObjectName
        {
            set { objectName = value; }
        }


        public void Start()
        {
            DoStart();
        }

        public virtual void Shutdown()
        {
            logger.Debug("Shutting down message listener container");
            bool wasRunning = false;
            lock (this.lifecycleMonitor)
            {
                wasRunning = this.running;
                this.running = false;
                this.active = false;
                System.Threading.Monitor.PulseAll(this.lifecycleMonitor);
            }

            if (wasRunning && SharedConnectionEnabled)
            {
                try
                {
                    StopSharedConnection();
                } catch (Exception ex)
                {
                    logger.Debug("Could not stop NMS Connection on shutdown", ex);
                }
            }

            // Shut down the invokers
            try
            {
                DestroyListener();
            }
            finally
            {
                lock (this.sharedConnectionMonitor)
                {
                    NmsUtils.CloseConnection(this.sharedConnection, wasRunning);
                }
            }
        }

        protected void DoStart()
        {
            lock (this.lifecycleMonitor)
            {
                running = true;
                System.Threading.Monitor.PulseAll(this.lifecycleMonitor);

                //TODO - PausedTasks
            }

            if (SharedConnectionEnabled)
            {
                StartSharedConnection();
            }
        }

        protected virtual void StartSharedConnection()
        {
            lock (sharedConnectionMonitor)
            {
                if (sharedConnection != null)
                {
                    try
                    {
                        sharedConnection.Start();
                    }
                    catch (Exception ex)
                    {
                        logger.Debug("Ignoring IConnection start exception - assuming already started", ex);
                    }
                }
            }
        }

        public IConnection SharedConnection
        {
            get
            {
                if (!SharedConnectionEnabled)
                {
                    throw new System.SystemException("This message listener container does not maintain a shared IConnection");
                }
                lock (this.sharedConnectionMonitor)
                {
                    if (this.sharedConnection == null)
                    {
                        //TODO SharedConnectionNotInitializedException
                        throw new ApplicationException("This message listener container's shared IConnection has not been initialized yet");
                    }
                    return this.sharedConnection;
                }
            }
        }

        public virtual void Initialize()
        {
            try
            {
                lock (this.lifecycleMonitor)
                {
                    this.active = true;
                    System.Threading.Monitor.PulseAll(this.lifecycleMonitor);
                }

                if (SharedConnectionEnabled)
                {
                    EstablishSharedConnection();
                }

                if (this.autoStartup)
                {
                    DoStart();
                }

                RegisterListener();
            }
            catch (Exception)
            {
                lock (this.sharedConnectionMonitor)
                {
                    ConnectionFactoryUtils.ReleaseConnection(sharedConnection, ConnectionFactory, autoStartup);
                }
                throw;
            }
        }

        protected virtual void EstablishSharedConnection()
        {
            RefreshSharedConnection();
        }


        protected void RefreshSharedConnection()
        {
            bool running = IsRunning;
            lock (this.sharedConnectionMonitor)
            {
                NmsUtils.CloseConnection(this.sharedConnection, running);

                IConnection con = CreateConnection();
                try
                {
                    PrepareSharedConnection(con);
                }
                catch (Exception)
                {
                    NmsUtils.CloseConnection(con);
                    throw;
                }
                this.sharedConnection = con;
            }
        }

        protected virtual void PrepareSharedConnection(IConnection connection)
        {
            if (ClientId != null)
            {
                connection.ClientId = ClientId;
            }
        }

        /// <summary> Register the specified listener on the underlying NMS IConnection.
        /// <p>Subclasses need to implement this method for their specific
        /// listener management process.</p>
        /// </summary>
        /// <throws>  NMSException if registration failed </throws>
        /// <seealso cref="IMessageListener">
        /// </seealso>
        /// <seealso cref="SharedConnection">
        /// </seealso>
        protected abstract void RegisterListener();

        public void Stop()
        {
            DoStop();
        }

        protected virtual void DoStop()
        {
            lock (this.lifecycleMonitor)
            {
                this.running = false;
                System.Threading.Monitor.PulseAll(this.lifecycleMonitor);
            }

            if (SharedConnectionEnabled)
            {
                StopSharedConnection();
            }
        }

        protected virtual void StopSharedConnection()
        {
            lock (this.sharedConnectionMonitor)
            {
                if (this.sharedConnection != null)
                {
                    try
                    {
                        this.sharedConnection.Stop();
                    }
                    catch (System.InvalidOperationException ex)
                    {
                        logger.Debug("Ignoring IConnection stop exception - assuming already stopped", ex);
                    }
                }
            }
        }



        /// <summary> Destroy the registered listener.
        /// The NMS IConnection will automatically be closed <i>afterwards</i>
        /// <p>Subclasses need to implement this method for their specific
        /// listener management process.</p>
        /// </summary>
        /// <throws>  NMSException if destruction failed </throws>
        protected abstract void DestroyListener();
    }
}