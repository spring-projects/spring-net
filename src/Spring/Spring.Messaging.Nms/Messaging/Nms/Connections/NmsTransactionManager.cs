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

using System.Data;
using Apache.NMS;
using Common.Logging;
using Spring.Messaging.Nms.Core;
using Spring.Objects.Factory;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Messaging.Nms.Connections
{
    /// <summary>
    /// A <see cref="AbstractPlatformTransactionManager"/> implementation
    /// for a single NMS <code>ConnectionFactory</code>. Binds a
    /// Connection/Session pair from the specified ConnecctionFactory to the thread,
    /// potentially allowing for one thread-bound Session per ConnectionFactory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Application code is required to retrieve the transactional Session via
    /// <see cref="ConnectionFactoryUtils.GetTransactionalSession"/>.  Spring's
    /// <see cref="NmsTemplate"/> will autodetect a thread-bound Session and
    /// automatically participate in it.
    /// </para>
    /// <para>
    /// The use of <see cref="CachingConnectionFactory"/>as a target for this
    /// transaction manager is strongly recommended. CachingConnectionFactory
    /// uses a single NMS Connection for all NMS access in order to avoid the overhead
    /// of repeated Connection creation, as well as maintaining a cache of Sessions.
    /// Each transaction will then share the same NMS Connection, while still using
    /// its own individual NMS Session.
    /// </para>
    /// <para>The use of a <i>raw</i> target ConnectionFactory would not only be inefficient
    /// because of the lack of resource reuse. It might also lead to strange effects
    /// when your NMS provider doesn't accept <code>MessageProducer.close()</code> calls
    /// and/or <code>MessageConsumer.close()</code> calls before <code>Session.commit()</code>,
    /// with the latter supposed to commit all the messages that have been sent through the
    /// producer handle and received through the consumer handle. As a safe general solution,
    /// always pass in a <see cref="CachingConnectionFactory"/> into this transaction manager's
    /// ConnectionFactory property.
    /// </para>
    /// <para>
    /// Transaction synchronization is turned off by default, as this manager might be used
    /// alongside an IDbProvider based Spring transaction manager such as the
    /// AdoPlatformTransactionManager, which has stronger needs for synchronization.
    /// </para>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class NmsTransactionManager : AbstractPlatformTransactionManager,
        IResourceTransactionManager, IInitializingObject
    {

        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof(NmsTransactionManager));

        #endregion

        private IConnectionFactory connectionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NmsTransactionManager"/> class.
        /// </summary>
        /// <remarks>
        /// The ConnectionFactory has to be set before using the instance.
        /// This constructor can be used to prepare a NmsTemplate via a ApplicationContext,
        /// typically setting the ConnectionFactory via ConnectionFactory property.
        /// <para>
        /// Turns off transaction synchronization by default, as this manager might
        /// be used alongside a dbprovider-based Spring transaction manager like
	    /// AdoPlatformTransactionManager, which has stronger needs for synchronization.
	    /// Only one manager is allowed to drive synchronization at any point of time.
        /// </para>
        /// </remarks>
        public NmsTransactionManager()
        {
            TransactionSynchronization = TransactionSynchronizationState.Never;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NmsTransactionManager"/> class
        /// given a ConnectionFactory.
        /// </summary>
        /// <param name="connectionFactory">The connection factory to obtain connections from.</param>
        public NmsTransactionManager(IConnectionFactory connectionFactory) : this()
        {
            ConnectionFactory = connectionFactory;
            AfterPropertiesSet();
        }


        /// <summary>
        /// Gets or sets the connection factory that this instance should manage transaction.
        /// for.
        /// </summary>
        /// <value>The connection factory.</value>
        public IConnectionFactory ConnectionFactory
        {
            get { return connectionFactory; }
            set
            {
                connectionFactory = value;
            }
        }

        #region IInitializingObject Members

        /// <summary>
        /// Make sure the ConnectionFactory has been set.
        /// </summary>
        public void AfterPropertiesSet()
        {
            if (ConnectionFactory == null)
            {
                throw new ArgumentException("Property 'ConnectionFactory' is required.");
            }
        }

        #endregion

        #region IResourceTransactionManager Members

        /// <summary>
        /// Gets the resource factory that this transaction manager operates on,
        /// In tihs case the ConnectionFactory
        /// </summary>
        /// <value>The ConnectionFactory.</value>
        public object ResourceFactory
        {
            get { return ConnectionFactory; }
        }

        #endregion


        /// <summary>
        /// Get the MessageTransactionObject.
        /// </summary>
        /// <returns>he MessageTransactionObject.</returns>
        protected override object DoGetTransaction()
        {
            MessageTransactionObject txObject = new MessageTransactionObject();

            txObject.ResourceHolder =
                (NmsResourceHolder) TransactionSynchronizationManager.GetResource(ConnectionFactory);
            return txObject;
        }


        /// <summary>
        /// Begin a new transaction with the given transaction definition.
        /// </summary>
        /// <param name="transaction">Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.</param>
        /// <param name="definition"><see cref="Spring.Transaction.ITransactionDefinition"/> instance, describing
        /// propagation behavior, isolation level, timeout etc.</param>
        /// <remarks>
        /// Does not have to care about applying the propagation behavior,
        /// as this has already been handled by this abstract manager.
        /// </remarks>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of creation or system errors.
        /// </exception>
        protected override void DoBegin(object transaction, ITransactionDefinition definition)
        {
            //This is the default value defined in DefaultTransactionDefinition
            if (definition.TransactionIsolationLevel != IsolationLevel.ReadCommitted)
            {
                throw new InvalidIsolationLevelException("NMS does not support an isoliation level concept");
            }
            MessageTransactionObject txObject = (MessageTransactionObject) transaction;
            IConnection con = null;
            ISession session = null;
            try
            {
                con = CreateConnection();
                session = CreateSession(con);
                if (LOG.IsDebugEnabled)
                {
                    log.Debug("Created NMS transaction on Session [" + session + "] from Connection [" + con + "]");
                }
                txObject.ResourceHolder = new NmsResourceHolder(ConnectionFactory, con, session);
                txObject.ResourceHolder.SynchronizedWithTransaction = true;
                int timeout = DetermineTimeout(definition);
                if (timeout != DefaultTransactionDefinition.TIMEOUT_DEFAULT)
                {
                    txObject.ResourceHolder.TimeoutInSeconds = timeout;
                }
                TransactionSynchronizationManager.BindResource(ConnectionFactory, txObject.ResourceHolder);



            } catch (NMSException ex)
            {
                if (session != null)
                {
                    try
                    {
                        session.Close();
                    } catch (Exception)
                    {}
                }
                if (con != null)
                {
                    try
                    {
                        con.Close();
                    } catch (Exception){}
                }
                throw new CannotCreateTransactionException("Could not create NMS Transaction", ex);
            }

        }

        /// <summary>
        /// Suspend the resources of the current transaction.
        /// </summary>
        /// <param name="transaction">Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.</param>
        /// <returns>
        /// An object that holds suspended resources (will be kept unexamined for passing it into
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoResume"/>.)
        /// </returns>
        /// <remarks>
        /// Transaction synchronization will already have been suspended.
        /// </remarks>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// in case of system errors.
        /// </exception>
        protected override object DoSuspend(object transaction)
        {
            MessageTransactionObject txObject = (MessageTransactionObject) transaction;
            txObject.ResourceHolder = null;
            return TransactionSynchronizationManager.UnbindResource(ConnectionFactory);
        }

        /// <summary>
        /// Resume the resources of the current transaction.
        /// </summary>
        /// <param name="transaction">Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.</param>
        /// <param name="suspendedResources">The object that holds suspended resources as returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoSuspend"/>.</param>
        /// <remarks>Transaction synchronization will be resumed afterwards.
        /// </remarks>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected override void DoResume(object transaction, object suspendedResources)
        {
            NmsResourceHolder conHolder = (NmsResourceHolder) suspendedResources;
            TransactionSynchronizationManager.BindResource(ConnectionFactory, conHolder);
        }

        /// <summary>
        /// Perform an actual commit on the given transaction.
        /// </summary>
        /// <param name="status">The status representation of the transaction.</param>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected override void DoCommit(DefaultTransactionStatus status)
        {
            MessageTransactionObject txObject = (MessageTransactionObject)status.Transaction;
            ISession session = txObject.ResourceHolder.GetSession();
            try
            {
                if (status.Debug)
                {
                    LOG.Debug("Committing NMS transaction on Session [" + session + "]");
                }
                session.Commit();
                //Note that NMS does not have, TransactionRolledBackException
                //See https://issues.apache.org/activemq/browse/AMQNET-93
            }
            catch (NMSException ex)
            {
                throw new TransactionSystemException("Could not commit NMS transaction.", ex);
            }
        }

        /// <summary>
        /// Perform an actual rollback on the given transaction.
        /// </summary>
        /// <param name="status">The status representation of the transaction.</param>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected override void DoRollback(DefaultTransactionStatus status)
        {
            MessageTransactionObject txObject = (MessageTransactionObject)status.Transaction;
            ISession session = txObject.ResourceHolder.GetSession();
            try
            {
                if (status.Debug)
                {
                    LOG.Debug("Rolling back NMS transaction on Session [" + session + "]");
                }
                session.Rollback();
            }
            catch (NMSException ex)
            {
                throw new TransactionSystemException("Could not roll back NMS transaction.", ex);
            }
        }


        /// <summary>
        /// Set the given transaction rollback-only. Only called on rollback
        /// if the current transaction takes part in an existing one.
        /// </summary>
        /// <param name="status">The status representation of the transaction.</param>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected override void DoSetRollbackOnly(DefaultTransactionStatus status)
        {
            MessageTransactionObject txObject = (MessageTransactionObject)status.Transaction;
            txObject.ResourceHolder.RollbackOnly = true;
        }

        /// <summary>
        /// Cleanup resources after transaction completion.
        /// </summary>
        /// <param name="transaction">Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.</param>
        /// <remarks>
        /// <para>
        /// Called after <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoCommit"/>
        /// and
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoRollback"/>
        /// execution on any outcome.
        /// </para>
        /// </remarks>
        protected override void DoCleanupAfterCompletion(object transaction)
        {
            MessageTransactionObject txObject = (MessageTransactionObject)transaction;
            TransactionSynchronizationManager.UnbindResource(ConnectionFactory);
            txObject.ResourceHolder.CloseAll();
            txObject.ResourceHolder.Clear();
        }

        /// <summary>
        /// Check if the given transaction object indicates an existing transaction
        /// (that is, a transaction which has already started).
        /// </summary>
        /// <param name="transaction">Transaction object returned by
        /// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager.DoGetTransaction"/>.</param>
        /// <returns>
        /// True if there is an existing transaction.
        /// </returns>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In the case of system errors.
        /// </exception>
        protected override bool IsExistingTransaction(object transaction)
        {
            MessageTransactionObject txObject = transaction as MessageTransactionObject;
            if (txObject != null)
            {
                return txObject.ResourceHolder != null;
            }
            return false;
        }

        /// <summary>
        /// Creates the connection via thie manager's ConnectionFactory.
        /// </summary>
        /// <returns>The new Connection</returns>
        /// <exception cref="NMSException">If thrown by underlying messaging APIs</exception>
        protected virtual IConnection CreateConnection()
        {
            return ConnectionFactory.CreateConnection();
        }

        /// <summary>
        /// Creates the session for the given Connection
        /// </summary>
        /// <param name="connection">The connection to create a Session for.</param>
        /// <returns>the new Session</returns>
        /// <exception cref="NMSException">If thrown by underlying messaging APIs</exception>
        protected virtual ISession CreateSession(IConnection connection)
        {
            return connection.CreateSession(AcknowledgementMode.Transactional);
        }


        /// <summary>
        /// NMS Transaction object, representing a MessageResourceHolder.
        /// Used as transaction object by MessageTransactionManager
        /// </summary>
        internal class MessageTransactionObject : ISmartTransactionObject
        {
            private NmsResourceHolder resourceHolder;


            public NmsResourceHolder ResourceHolder
            {
                get { return resourceHolder; }
                set { resourceHolder = value; }
            }

            #region ISmartTransactionObject Members

            public bool RollbackOnly
            {
                get { return resourceHolder.RollbackOnly; }
            }

            #endregion
        }
    }
}
