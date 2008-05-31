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
using Spring.Transaction.Support;
using Spring.Util;
using NMS;

namespace Spring.Messaging.Nms.IConnections
{
    /// <summary> Helper class for obtaining transactional NMS resources
    /// for a given IConnectionFactory.
    ///
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public abstract class ConnectionFactoryUtils
    {
        

        /// <summary> Obtain a NMS ISession that is synchronized with the current transaction, if any.</summary>
        /// <param name="cf">the IConnectionFactory to obtain a ISession for
        /// </param>
        /// <param name="existingCon">the existing NMS IConnection to obtain a ISession for
        /// (may be <code>null</code>)
        /// </param>
        /// <param name="acknowledgementMode">whether to allow for a local NMS transaction
        /// that is synchronized with a Spring-managed transaction (where the main transaction
        /// might be a JDBC-based one for a specific DataSource, for example), with the NMS
        /// transaction committing right after the main transaction. If not allowed, the given
        /// IConnectionFactory needs to handle transaction enlistment underneath the covers.
        /// </param>
        /// <returns> the transactional ISession, or <code>null</code> if none found
        /// </returns>
        /// <throws>  NMSException in case of NMS failure </throws>
        public static ISession GetTransactionalSession(IConnectionFactory cf, IConnection existingCon,  bool synchedLocalTransactionAllowed)
        {

            return DoGetTransactionalSession(cf, new AnonymousClassResourceFactory(existingCon, cf, synchedLocalTransactionAllowed));
        }

        /// <summary> Obtain a NMS ISession that is synchronized with the current transaction, if any.</summary>
        /// <param name="resourceKey">the TransactionSynchronizationManager key to bind to
        /// (usually the IConnectionFactory)
        /// </param>
        /// <param name="resourceFactory">the ResourceFactory to use for extracting or creating
        /// NMS resources
        /// </param>
        /// <returns> the transactional ISession, or <code>null</code> if none found
        /// </returns>
        /// <throws>NMSException in case of NMS failure </throws>
        public static ISession DoGetTransactionalSession(System.Object resourceKey, ConnectionFactoryUtils.ResourceFactory resourceFactory)
        {

            AssertUtils.ArgumentNotNull(resourceKey, "Resource key must not be null");
            AssertUtils.ArgumentNotNull(resourceKey, "ResourceFactory must not be null");

            NmsResourceHolder resourceHolder = (NmsResourceHolder)TransactionSynchronizationManager.GetResource(resourceKey);
            if (resourceHolder != null)
            {
                ISession rssession = resourceFactory.GetSession(resourceHolder);
                if (rssession != null || resourceHolder.Frozen)
                {
                    return rssession;
                }
            }
            if (!TransactionSynchronizationManager.SynchronizationActive)
            {
                return null;
            }
            NmsResourceHolder conHolderToUse = resourceHolder;
            if (conHolderToUse == null)
            {
                conHolderToUse = new NmsResourceHolder();
            }
            NMS.IConnection con = resourceFactory.GetConnection(conHolderToUse);
            ISession session = null;
            try
            {
                bool isExistingCon = (con != null);
                if (!isExistingCon)
                {
                    con = resourceFactory.CreateConnection();
                    conHolderToUse.AddConnection(con);
                }
                session = resourceFactory.CreateSession(con);
                conHolderToUse.AddSession(session, con);
                if (!isExistingCon)
                {
                    con.Start();
                }
            }
            catch (NMSException)
            {
                if (session != null)
                {
                    try
                    {
                        session.Close();
                    }
                    catch (System.Exception)
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
                    catch (System.Exception)
                    {
                        // ignore
                    }
                }
                throw;
            }
            if (conHolderToUse != resourceHolder)
            {
                TransactionSynchronizationManager.RegisterSynchronization(new NmsResourceSynchronization(resourceKey, conHolderToUse, resourceFactory.SynchedLocalTransactionAllowed));
                conHolderToUse.SynchronizedWithTransaction = true;
                TransactionSynchronizationManager.BindResource(resourceKey, conHolderToUse);
            }
            return session;
        }
		
		


        #region ResourceFactory helper classes

        private class AnonymousClassResourceFactory : ConnectionFactoryUtils.ResourceFactory
        {
            private IConnection existingCon;
            private IConnectionFactory cf;
            private bool synchedLocalTransactionAllowed;

            public AnonymousClassResourceFactory(NMS.IConnection existingCon, IConnectionFactory cf, bool synchedLocalTransactionAllowed)
            {
                InitBlock(existingCon, cf, synchedLocalTransactionAllowed);
            }

            private void InitBlock(NMS.IConnection existingCon, IConnectionFactory cf, bool synchedLocalTransactionAllowed)
            {
                this.existingCon = existingCon;
                this.cf = cf;
                this.synchedLocalTransactionAllowed = synchedLocalTransactionAllowed;
            }
			
            public virtual ISession GetSession(NmsResourceHolder holder)
            {
               return holder.GetSession(typeof(ISession), existingCon);
            }
            
            public virtual NMS.IConnection GetConnection(NmsResourceHolder holder)
            {
               return (existingCon != null ? existingCon : holder.GetConnection());
            }
			
            public virtual NMS.IConnection CreateConnection()
            {
                return cf.CreateConnection();
            }
            
            public virtual ISession CreateSession(NMS.IConnection con)
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

            /// <summary> Fetch an appropriate ISession from the given NmsResourceHolder.</summary>
            /// <param name="holder">the NmsResourceHolder
            /// </param>
            /// <returns> an appropriate ISession fetched from the holder,
            /// or <code>null</code> if none found
            /// </returns>
            ISession GetSession(NmsResourceHolder holder);

            /// <summary> Fetch an appropriate IConnection from the given NmsResourceHolder.</summary>
            /// <param name="holder">the NmsResourceHolder
            /// </param>
            /// <returns> an appropriate IConnection fetched from the holder,
            /// or <code>null</code> if none found
            /// </returns>
            IConnection GetConnection(NmsResourceHolder holder);

            /// <summary> Create a new NMS IConnection for registration with a NmsResourceHolder.</summary>
            /// <returns> the new NMS IConnection
            /// </returns>
            /// <throws>NMSException if thrown by NMS API methods </throws>
            IConnection CreateConnection();

            /// <summary> Create a new NMS ISession for registration with a NmsResourceHolder.</summary>
            /// <param name="con">the NMS IConnection to create a ISession for
            /// </param>
            /// <returns> the new NMS ISession
            /// </returns>
            /// <throws>NMSException if thrown by NMS API methods </throws>
            ISession CreateSession(NMS.IConnection con);


            /// <summary>
            /// Return whether to allow for a local NMS transaction that is synchronized with
            /// a Spring-managed transaction (where the main transaction might be a ADO.NET-based
            /// one for a specific IDbProvider, for example), with the NMS transaction
            /// committing right after the main transaction.
            /// Returns whether to allow for synchronizing a local NMS transaction
            /// </summary>
            /// 
            bool SynchedLocalTransactionAllowed
            { 
                get;
            }
        }

        /// <summary> Callback for resource cleanup at the end of a non-native NMS transaction
        /// </summary>
        private class NmsResourceSynchronization : TransactionSynchronizationAdapter
        {

            private object resourceKey;

            private NmsResourceHolder resourceHolder;

            private bool transacted;

            private bool holderActive = true;

            public NmsResourceSynchronization(object resourceKey, NmsResourceHolder resourceHolder, bool transacted)
            {
                this.resourceKey = resourceKey;
                this.resourceHolder = resourceHolder;
                this.transacted = transacted;
            }

            public override void Suspend()
            {
                if (this.holderActive)
                {
                    TransactionSynchronizationManager.UnbindResource(resourceKey);
                }
            }

            public override void Resume()
            {
                if (this.holderActive)
                {
                    TransactionSynchronizationManager.BindResource(resourceKey, resourceHolder);
                }
            }

            public override void BeforeCompletion()
            {
                TransactionSynchronizationManager.UnbindResource(this.resourceKey);
                this.holderActive = false;
                if (!transacted)
                {
                    this.resourceHolder.CloseAll();
                }
            }

            //TODO bring in new Spring.Data library to Integration project which has this method in interface.
            public override void AfterCommit()
            {
                if (this.transacted)
                {
                    try
                    {
                        this.resourceHolder.CommitAll();
                    }
                    catch (NMSException ex)
                    {
                        throw new SynchedLocalTransactionFailedException("Local NMS transaction failed to commit", ex);
                    }
                }
            }

            public override void AfterCompletion(TransactionSynchronizationStatus status)
            {
                if (this.transacted)
                {
                    this.resourceHolder.CloseAll();
                }
            }
        }
        #endregion
    }
}
