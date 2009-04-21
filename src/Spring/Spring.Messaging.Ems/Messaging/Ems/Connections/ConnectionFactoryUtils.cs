#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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
using Common.Logging;
using Spring.Messaging.Ems.Support;
using Spring.Transaction.Support;
using Spring.Util;
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Connections
{
    /// <summary> Helper class for obtaining transactional EMS resources
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
        /// Releases the given connection.
        /// </summary>
        /// <param name="connection">The connection to release. (if this is <code>null</code>, the call will be ignored)</param>
        /// <param name="cf">The ConnectionFactory that the Connection was obtained from. (may be <code>null</code>)</param>
        /// <param name="started">whether the Connection might have been started by the application.</param>
        public static void ReleaseConnection(Connection connection, ConnectionFactory cf, bool started)
        {
            if (connection == null)
            {
                return;
            }

            try
            {
                connection.Close();
            } catch (Exception ex)
            {
                LOG.Debug("Could not close EMS Connection", ex);
            }           
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
        public static bool IsSessionTransactional(Session session, ConnectionFactory cf)
        {
            if (session == null || cf == null)
            {
                return false;
            }
            EmsResourceHolder resourceHolder = (EmsResourceHolder) TransactionSynchronizationManager.GetResource(cf);
            return (resourceHolder != null && resourceHolder.ContainsSession(session));
        }

        /// <summary> Obtain a EMS Session that is synchronized with the current transaction, if any.</summary>
        /// <param name="cf">the ConnectionFactory to obtain a Session for
        /// </param>
        /// <param name="existingCon">the existing EMS Connection to obtain a Session for
        /// (may be <code>null</code>)
        /// </param>
        /// <param name="synchedLocalTransactionAllowed">whether to allow for a local EMS transaction
        /// that is synchronized with a Spring-managed transaction (where the main transaction
        /// might be a ADO.NET-based one for a specific DataSource, for example), with the EMS
        /// transaction committing right after the main transaction. If not allowed, the given
        /// ConnectionFactory needs to handle transaction enlistment underneath the covers.
        /// </param>
        /// <returns> the transactional Session, or <code>null</code> if none found
        /// </returns>
        /// <throws>  EMSException in case of EMS failure </throws>
        public static Session GetTransactionalSession(ConnectionFactory cf, Connection existingCon,
                                                       bool synchedLocalTransactionAllowed)
        {
            return
                DoGetTransactionalSession(cf,
                                          new AnonymousClassResourceFactory(existingCon, cf,
                                                                            synchedLocalTransactionAllowed), true);
        }

        /// <summary>
        /// Obtain a EMS Session that is synchronized with the current transaction, if any.
        /// </summary>
        /// <param name="resourceKey">the TransactionSynchronizationManager key to bind to
        /// (usually the ConnectionFactory)</param>
        /// <param name="resourceFactory">the ResourceFactory to use for extracting or creating
        /// EMS resources</param>
        /// <param name="startConnection">whether the underlying Connection approach should be
	    /// started in order to allow for receiving messages. Note that a reused Connection
	    /// may already have been started before, even if this flag is <code>false</code>.</param>
        /// <returns>
        /// the transactional Session, or <code>null</code> if none found
        /// </returns>
        /// <throws>EMSException in case of EMS failure </throws>
        public static Session DoGetTransactionalSession(Object resourceKey, ResourceFactory resourceFactory, bool startConnection)
        {
            AssertUtils.ArgumentNotNull(resourceKey, "Resource key must not be null");
            AssertUtils.ArgumentNotNull(resourceKey, "ResourceFactory must not be null");

            EmsResourceHolder resourceHolder =
                (EmsResourceHolder)TransactionSynchronizationManager.GetResource(resourceKey);
            if (resourceHolder != null)
            {
                Session rhSession = resourceFactory.GetSession(resourceHolder);
                if (rhSession != null)
                {
                    if (startConnection)
                    {
                        Connection conn = resourceFactory.GetConnection(resourceHolder);
                        if (conn != null)
                        {
                            conn.Start();
                        }
                    }
                    return rhSession;
                }
            }
            if (!TransactionSynchronizationManager.SynchronizationActive)
            {
                return null;
            }
            EmsResourceHolder resourceHolderToUse = resourceHolder;
            if (resourceHolderToUse == null)
            {
                resourceHolderToUse = new EmsResourceHolder();
            }

            Connection con = resourceFactory.GetConnection(resourceHolderToUse);
            Session session = null;
            try
            {
                bool isExistingCon = (con != null);
                if (!isExistingCon)
                {
                    con = resourceFactory.CreateConnection();
                    resourceHolderToUse.AddConnection(con);
                }
                session = resourceFactory.CreateSession(con);
                resourceHolderToUse.AddSession(session, con);
                if (startConnection)
                {
                    con.Start();
                }
            }
            catch (EMSException)
            {
                if (session != null)
                {
                    try
                    {
                        session.Close();
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
                        con.Close();
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
                    new EmsResourceSynchronization(resourceHolderToUse,
                                                   resourceKey, resourceFactory.SynchedLocalTransactionAllowed));
                resourceHolderToUse.SynchronizedWithTransaction = true;
                TransactionSynchronizationManager.BindResource(resourceKey, resourceHolderToUse);
            }
            return session;
        }

        #region ResourceFactory helper classes

        private class AnonymousClassResourceFactory : ResourceFactory
        {
            private Connection existingCon;
            private ConnectionFactory cf;
            private bool synchedLocalTransactionAllowed;

            public AnonymousClassResourceFactory(Connection existingCon, ConnectionFactory cf,
                                                 bool synchedLocalTransactionAllowed)
            {
                InitBlock(existingCon, cf, synchedLocalTransactionAllowed);
            }

            private void InitBlock(Connection existingCon, ConnectionFactory cf, bool synchedLocalTransactionAllowed)
            {
                this.existingCon = existingCon;
                this.cf = cf;
                this.synchedLocalTransactionAllowed = synchedLocalTransactionAllowed;
            }

            public virtual Session GetSession(EmsResourceHolder holder)
            {
                return holder.GetSession(typeof(Session), existingCon);
            }

            public virtual Connection GetConnection(EmsResourceHolder holder)
            {
                return (existingCon != null ? existingCon : holder.GetConnection());
            }

            public virtual Connection CreateConnection()
            {
                return cf.CreateConnection();
            }

            public virtual Session CreateSession(Connection con)
            {
                return con.CreateSession(synchedLocalTransactionAllowed, Session.SESSION_TRANSACTED);                
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
            /// <summary> Fetch an appropriate Session from the given EmsResourceHolder.</summary>
            /// <param name="holder">the EmsResourceHolder
            /// </param>
            /// <returns> an appropriate Session fetched from the holder,
            /// or <code>null</code> if none found
            /// </returns>
            Session GetSession(EmsResourceHolder holder);

            /// <summary> Fetch an appropriate Connection from the given EmsResourceHolder.</summary>
            /// <param name="holder">the EmsResourceHolder
            /// </param>
            /// <returns> an appropriate Connection fetched from the holder,
            /// or <code>null</code> if none found
            /// </returns>
            Connection GetConnection(EmsResourceHolder holder);

            /// <summary> Create a new EMS Connection for registration with a EmsResourceHolder.</summary>
            /// <returns> the new EMS Connection
            /// </returns>
            /// <throws>EMSException if thrown by EMS API methods </throws>
            Connection CreateConnection();

            /// <summary> Create a new EMS Session for registration with a EmsResourceHolder.</summary>
            /// <param name="con">the EMS Connection to create a Session for
            /// </param>
            /// <returns> the new EMS Session
            /// </returns>
            /// <throws>EMSException if thrown by EMS API methods </throws>
            Session CreateSession(Connection con);


            /// <summary>
            /// Return whether to allow for a local EMS transaction that is synchronized with
            /// a Spring-managed transaction (where the main transaction might be a ADO.NET-based
            /// one for a specific IDbProvider, for example), with the EMS transaction
            /// committing right after the main transaction.
            /// Returns whether to allow for synchronizing a local EMS transaction
            /// </summary>
            /// 
            bool SynchedLocalTransactionAllowed { get; }
        }

        /// <summary> Callback for resource cleanup at the end of a non-native EMS transaction
        /// </summary>
        private class EmsResourceSynchronization : TransactionSynchronizationAdapter
        {
            private object resourceKey;

            private EmsResourceHolder resourceHolder;

            private bool transacted;

            private bool holderActive = true;

            public EmsResourceSynchronization(EmsResourceHolder resourceHolder, object resourceKey, bool transacted)
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
                    catch (EMSException ex)
                    {
                        throw new SynchedLocalTransactionFailedException("Local EMS transaction failed to commit", ex);
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