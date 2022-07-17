#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using Apache.NMS;
using Common.Logging;
using Spring.Messaging.Nms.Support;
using Spring.Transaction.Support;
using Spring.Util;

namespace Spring.Messaging.Nms.Connections
{
    /// <summary> Helper class for obtaining transactional NMS resources
    /// for a given ConnectionFactory.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public abstract class ConnectionFactoryUtils
    {
        #region Logging

        private static readonly ILog LOG = LogManager.GetLogger(typeof(ConnectionFactoryUtils));

        #endregion

        /// <summary>
        /// Releases the given connection, stopping it (if necessary) and eventually closing it.
        /// </summary>
        /// <remarks>Checks <see cref="ISmartConnectionFactory.ShouldStop"/>, if available.
        /// This is essentially a more sophisticated version of 
        /// <see cref="NmsUtils.CloseConnection(IConnection, bool)"/>
        /// </remarks>
        /// <param name="connection">The connection to release. (if this is <code>null</code>, the call will be ignored)</param>
        /// <param name="cf">The ConnectionFactory that the Connection was obtained from. (may be <code>null</code>)</param>
        /// <param name="started">whether the Connection might have been started by the application.</param>
        public static void ReleaseConnection(IConnection connection, IConnectionFactory cf, bool started)
        {
            if (connection == null)
            {
                return;
            }

            if (started && cf is ISmartConnectionFactory && ((ISmartConnectionFactory)cf).ShouldStop(connection))
            {
                try
                {
                    connection.Stop();
                }
                catch (Exception ex)
                {
                    LOG.Debug("Could not stop NMS Connection before closing it", ex);

                }
            }
            try
            {
                connection.Close();
            } catch (Exception ex)
            {
                LOG.Debug("Could not close NMS Connection", ex);
            }           
        } 
        
        /// <summary>
        /// Releases the given connection, stopping it (if necessary) and eventually closing it.
        /// </summary>
        /// <remarks>Checks <see cref="ISmartConnectionFactory.ShouldStop"/>, if available.
        /// This is essentially a more sophisticated version of 
        /// <see cref="NmsUtils.CloseConnection(IConnection, bool)"/>
        /// </remarks>
        /// <param name="connection">The connection to release. (if this is <code>null</code>, the call will be ignored)</param>
        /// <param name="cf">The ConnectionFactory that the Connection was obtained from. (may be <code>null</code>)</param>
        /// <param name="started">whether the Connection might have been started by the application.</param>
        public static async Task ReleaseConnectionAsync(IConnection connection, IConnectionFactory cf, bool started)
        {
            if (connection == null)
            {
                return;
            }

            if (started && cf is ISmartConnectionFactory && ((ISmartConnectionFactory)cf).ShouldStop(connection))
            {
                try
                {
                    await connection.StopAsync().Awaiter();
                }
                catch (Exception ex)
                {
                    LOG.Debug("Could not stop NMS Connection before closing it", ex);

                }
            }
            try
            {
                await connection.CloseAsync().Awaiter();
            } catch (Exception ex)
            {
                LOG.Debug("Could not close NMS Connection", ex);
            }           
        }

        /// <summary>
        /// Return the innermost target Session of the given Session. If the given
        /// Session is a decorated session, it will be unwrapped until a non-decorated
        /// Session is found. Otherwise, the passed-in Session will be returned as-is.
        /// </summary>
        /// <param name="session">The session to unwrap</param>
        /// <returns>The innermost target Session, or the passed-in one if no decorator</returns>
        public static ISession GetTargetSession(ISession session)
        {
            ISession sessionToUse = session;
            while (sessionToUse is IDecoratorSession)
            {
                sessionToUse = ((IDecoratorSession) sessionToUse).TargetSession;
            }
            return sessionToUse;
        }

        /// <summary>
        /// Determines whether the given JMS Session is transactional, that is,
        /// bound to the current thread by Spring's transaction facilities.
        /// </summary>
        /// <param name="session">The session to check.</param>
        /// <param name="cf">The ConnectionFactory that the Session originated from</param>
        /// <returns>
        /// 	<c>true</c> if is session transactional, bound to current thread; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSessionTransactional(ISession session, IConnectionFactory cf)
        {
            if (session == null || cf == null)
            {
                return false;
            }
            NmsResourceHolder resourceHolder = (NmsResourceHolder) TransactionSynchronizationManager.GetResource(cf);
            return (resourceHolder != null && resourceHolder.ContainsSession(session));
        }

        /// <summary> Obtain a NMS Session that is synchronized with the current transaction, if any.</summary>
        /// <param name="cf">the ConnectionFactory to obtain a Session for
        /// </param>
        /// <param name="existingCon">the existing NMS Connection to obtain a Session for
        /// (may be <code>null</code>)
        /// </param>
        /// <param name="synchedLocalTransactionAllowed">whether to allow for a local NMS transaction
        /// that is synchronized with a Spring-managed transaction (where the main transaction
        /// might be a ADO.NET-based one for a specific DataSource, for example), with the NMS
        /// transaction committing right after the main transaction. If not allowed, the given
        /// ConnectionFactory needs to handle transaction enlistment underneath the covers.
        /// </param>
        /// <returns> the transactional Session, or <code>null</code> if none found
        /// </returns>
        /// <throws>  NMSException in case of NMS failure </throws>
        public static ISession GetTransactionalSession(IConnectionFactory cf, IConnection existingCon,
                                                       bool synchedLocalTransactionAllowed)
        {
            return
                DoGetTransactionalSession(cf,
                                          new AnonymousClassResourceFactory(existingCon, cf,
                                                                            synchedLocalTransactionAllowed), true, true).GetAsyncResult();
        }

        /// <summary>
        /// Obtain a NMS Session that is synchronized with the current transaction, if any.
        /// </summary>
        /// <param name="resourceKey">the TransactionSynchronizationManager key to bind to
        /// (usually the ConnectionFactory)</param>
        /// <param name="resourceFactory">the ResourceFactory to use for extracting or creating
        /// NMS resources</param>
        /// <param name="startConnection">whether the underlying Connection approach should be
	    /// started in order to allow for receiving messages. Note that a reused Connection
	    /// may already have been started before, even if this flag is <code>false</code>.</param>
        /// <returns>
        /// the transactional Session, or <code>null</code> if none found
        /// </returns>
        /// <throws>NMSException in case of NMS failure </throws>
        public static async Task<ISession> DoGetTransactionalSession(Object resourceKey, ResourceFactory resourceFactory, bool startConnection, bool sync = false)
        {
            AssertUtils.ArgumentNotNull(resourceKey, "Resource key must not be null");
            AssertUtils.ArgumentNotNull(resourceKey, "ResourceFactory must not be null");

            NmsResourceHolder resourceHolder =
                (NmsResourceHolder)TransactionSynchronizationManager.GetResource(resourceKey);
            if (resourceHolder != null)
            {
                ISession rhSession = resourceFactory.GetSession(resourceHolder);
                if (rhSession != null)
                {
                    if (startConnection)
                    {
                        IConnection conn = resourceFactory.GetConnection(resourceHolder);
                        if (conn != null)
                        {
                            if(sync) conn.Start();
                            else await conn.StartAsync().Awaiter();
                        }
                    }
                    return rhSession;
                }
            }
            if (!TransactionSynchronizationManager.SynchronizationActive)
            {
                return null;
            }
            NmsResourceHolder resourceHolderToUse = resourceHolder;
            if (resourceHolderToUse == null)
            {
                resourceHolderToUse = new NmsResourceHolder();
            }

            IConnection con = resourceFactory.GetConnection(resourceHolderToUse);
            ISession session = null;
            try
            {
                bool isExistingCon = (con != null);
                if (!isExistingCon)
                {
                    con = await resourceFactory.CreateConnectionAsync().Awaiter();
                    resourceHolderToUse.AddConnection(con);
                }
                session = await resourceFactory.CreateSessionAsync(con).Awaiter();
                resourceHolderToUse.AddSession(session, con);
                if (startConnection)
                {
                    if (sync) con.Start();
                    else await con.StartAsync().Awaiter();
                }
            }
            catch (NMSException)
            {
                if (session != null)
                {
                    try
                    {
                        if (sync) session.Close();
                        else await session.CloseAsync().Awaiter();
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }
                if (con != null)
                {
                    try
                    {
                        if (sync) con.Close();
                        else await con.CloseAsync().Awaiter();
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }
                throw;
            }
            if (resourceHolderToUse != resourceHolder)
            {
                TransactionSynchronizationManager.RegisterSynchronization(
                    new NmsResourceSynchronization(resourceHolderToUse, resourceKey,
                                                   resourceFactory.SynchedLocalTransactionAllowed));
                resourceHolderToUse.SynchronizedWithTransaction = true;
                TransactionSynchronizationManager.BindResource(resourceKey, resourceHolderToUse);
            }
            return session;
        }

        #region ResourceFactory helper classes

        private class AnonymousClassResourceFactory : ResourceFactory
        {
            private IConnection existingCon;
            private IConnectionFactory cf;
            private bool synchedLocalTransactionAllowed;

            public AnonymousClassResourceFactory(IConnection existingCon, IConnectionFactory cf,
                                                 bool synchedLocalTransactionAllowed)
            {
                InitBlock(existingCon, cf, synchedLocalTransactionAllowed);
            }

            private void InitBlock(IConnection existingCon, IConnectionFactory cf, bool synchedLocalTransactionAllowed)
            {
                this.existingCon = existingCon;
                this.cf = cf;
                this.synchedLocalTransactionAllowed = synchedLocalTransactionAllowed;
            }

            public virtual ISession GetSession(NmsResourceHolder holder)
            {
                return holder.GetSession(typeof(ISession), existingCon);
            }

            public virtual IConnection GetConnection(NmsResourceHolder holder)
            {
                return (existingCon != null ? existingCon : holder.GetConnection());
            }

            public virtual IConnection CreateConnection()
            {
                return cf.CreateConnection();
            }

            public virtual ISession CreateSession(IConnection con)
            {
                if (synchedLocalTransactionAllowed)
                {
                    return con.CreateSession(AcknowledgementMode.Transactional);
                }
                else
                {
                    return con.CreateSession(AcknowledgementMode.AutoAcknowledge);
                }
            }

            public Task<IConnection> CreateConnectionAsync()
            {
                return cf.CreateConnectionAsync();
            }

            public async Task<ISession> CreateSessionAsync(IConnection con)
            {
                if (synchedLocalTransactionAllowed)
                {
                    return await con.CreateSessionAsync(AcknowledgementMode.Transactional).Awaiter();
                }
                else
                {
                    return await con.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge).Awaiter();
                }
            }

            public bool SynchedLocalTransactionAllowed
            {
                get { return synchedLocalTransactionAllowed; }
            }
        }

        #endregion

        #region Helper classes/interfaces

        /// <summary> Callback interface for resource creation.
        /// Serving as argument for the <code>DoGetTransactionalSession</code> method.
        /// </summary>
        public interface ResourceFactory
        {
            /// <summary> Fetch an appropriate Session from the given MessageResourceHolder.</summary>
            /// <param name="holder">the MessageResourceHolder
            /// </param>
            /// <returns> an appropriate Session fetched from the holder,
            /// or <code>null</code> if none found
            /// </returns>
            ISession GetSession(NmsResourceHolder holder);

            /// <summary> Fetch an appropriate Connection from the given MessageResourceHolder.</summary>
            /// <param name="holder">the MessageResourceHolder
            /// </param>
            /// <returns> an appropriate Connection fetched from the holder,
            /// or <code>null</code> if none found
            /// </returns>
            IConnection GetConnection(NmsResourceHolder holder);

            /// <summary> Create a new NMS Connection for registration with a MessageResourceHolder.</summary>
            /// <returns> the new NMS Connection
            /// </returns>
            /// <throws>NMSException if thrown by NMS API methods </throws>
            IConnection CreateConnection();

            /// <summary> Create a new NMS ISession for registration with a MessageResourceHolder.</summary>
            /// <param name="con">the NMS Connection to create a ISession for
            /// </param>
            /// <returns> the new NMS Session
            /// </returns>
            /// <throws>NMSException if thrown by NMS API methods </throws>
            ISession CreateSession(IConnection con);
 
            /// <summary> Create a new NMS Connection for registration with a MessageResourceHolder.</summary>
            /// <returns> the new NMS Connection
            /// </returns>
            /// <throws>NMSException if thrown by NMS API methods </throws>
            Task<IConnection> CreateConnectionAsync();

            /// <summary> Create a new NMS ISession for registration with a MessageResourceHolder.</summary>
            /// <param name="con">the NMS Connection to create a ISession for
            /// </param>
            /// <returns> the new NMS Session
            /// </returns>
            /// <throws>NMSException if thrown by NMS API methods </throws>
            Task<ISession> CreateSessionAsync(IConnection con);


            /// <summary>
            /// Return whether to allow for a local NMS transaction that is synchronized with
            /// a Spring-managed transaction (where the main transaction might be a ADO.NET-based
            /// one for a specific IDbProvider, for example), with the NMS transaction
            /// committing right after the main transaction.
            /// Returns whether to allow for synchronizing a local NMS transaction
            /// </summary>
            /// 
            bool SynchedLocalTransactionAllowed { get; }
        }

        /// <summary> Callback for resource cleanup at the end of a non-native NMS transaction
        /// </summary>
        private class NmsResourceSynchronization : TransactionSynchronizationAdapter
        {
            private object resourceKey;

            private NmsResourceHolder resourceHolder;

            private bool transacted;

            private bool holderActive = true;

            public NmsResourceSynchronization(NmsResourceHolder resourceHolder, object resourceKey, bool transacted)
            {
                this.resourceKey = resourceKey;
                this.resourceHolder = resourceHolder;
                this.transacted = transacted;
            }

            public override void Suspend()
            {
                if (holderActive)
                {
                    TransactionSynchronizationManager.UnbindResource(resourceKey);
                }
            }

            public override void Resume()
            {
                if (holderActive)
                {
                    TransactionSynchronizationManager.BindResource(resourceKey, resourceHolder);
                }
            }

            public override void BeforeCompletion()
            {
                TransactionSynchronizationManager.UnbindResource(resourceKey);
                holderActive = false;
                if (!transacted)
                {
                    resourceHolder.CloseAll();
                }
            }

            public override void AfterCommit()
            {
                if (transacted)
                {
                    try
                    {
                        resourceHolder.CommitAll();
                    }
                    catch (NMSException ex)
                    {
                        throw new SynchedLocalTransactionFailedException("Local NMS transaction failed to commit", ex);
                    }
                }
            }

            public override void AfterCompletion(TransactionSynchronizationStatus status)
            {
                if (transacted)
                {
                    resourceHolder.CloseAll();
                }
            }
        }

        #endregion
    }
}