

using System;
using System.Data;
using Apache.NMS;
using Common.Logging;
using Spring.Objects.Factory;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Messaging.Nms.Connection
{
    /// <summary>
    /// A <see cref="AbstractPlatformTransactionManager"/> implementation
    /// for a single NMS <code>Apache.NMS.IConnectionFactory</code>. Binds a 
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
    /// This transaction strategy will typically be used in combination with 
    /// <see cref="SingleConnectionFactory"/>, which uses a single NMS Connection
    /// for all NMS access in order to avoid the overhead of repeated Connection
    /// creation.  Each transaction will then share the same NMS Connection, while still using
    /// its own individual Session.
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
                //TODO if create TransactionAwareConnectionFactoryProxy need to check for it here.
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


        protected override object DoGetTransaction()
        {
            NmsTransactionObject txObject = new NmsTransactionObject();

            txObject.ResourceHolder =
                (NmsResourceHolder) TransactionSynchronizationManager.GetResource(ConnectionFactory);
            return txObject;
        }

        protected override void DoBegin(object transaction, ITransactionDefinition definition)
        {
            //This is the default value defined in DefaultTransactionDefinition
            if (definition.TransactionIsolationLevel != IsolationLevel.ReadCommitted)
            {
                throw new InvalidIsolationLevelException("NMS does not support an isoliation level concept");
            }
            NmsTransactionObject txObject = (NmsTransactionObject) transaction;
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

        protected override object DoSuspend(object transaction)
        {
            NmsTransactionObject txObject = (NmsTransactionObject) transaction;
            txObject.ResourceHolder = null;
            return TransactionSynchronizationManager.UnbindResource(ConnectionFactory);
        }

        protected override void DoResume(object transaction, object suspendedResources)
        {
            NmsResourceHolder conHolder = (NmsResourceHolder) suspendedResources;
            TransactionSynchronizationManager.BindResource(ConnectionFactory, conHolder);
        }

        protected override void DoCommit(DefaultTransactionStatus status)
        {
            NmsTransactionObject txObject = (NmsTransactionObject)status.Transaction;
            ISession session = txObject.ResourceHolder.GetSession();
            try
            {
                if (status.Debug)
                {
                    LOG.Debug("Committing NMS transaction on Session [" + session + "]");
                }
                session.Commit();
                /** https://issues.apache.org/activemq/browse/AMQNET-93
                   TODO - need to mirror JMS exception classes in NMS API.
                   } catch (TransactionRolledBackException ex)
                   {
                 */

            }
            catch (NMSException ex)
            {
                throw new TransactionSystemException("Could not commit NMS transaction.", ex);
            }
        }

        protected override void DoRollback(DefaultTransactionStatus status)
        {
            NmsTransactionObject txObject = (NmsTransactionObject)status.Transaction;
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


        protected override void DoSetRollbackOnly(DefaultTransactionStatus status)
        {
            NmsTransactionObject txObject = (NmsTransactionObject)status.Transaction;
            txObject.ResourceHolder.RollbackOnly = true;
        }

        protected override void DoCleanupAfterCompletion(object transaction)
        {
            NmsTransactionObject txObject = (NmsTransactionObject)transaction;
            TransactionSynchronizationManager.UnbindResource(ConnectionFactory);
            txObject.ResourceHolder.CloseAll();
            txObject.ResourceHolder.Clear();
        }

        protected override bool IsExistingTransaction(object transaction)
        {
            NmsTransactionObject txObject = transaction as NmsTransactionObject;
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
        /// NMS Transaction object, representing a NmsResourceHolder.
        /// Used as transaction object by NmsTransactionManager
        /// </summary>
        internal class NmsTransactionObject : ISmartTransactionObject
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