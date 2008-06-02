using Spring.Data.Common;
using Spring.Transaction.Support;

namespace Spring.Data.Support
{
	/// <summary>
	/// Callback for resource cleanup at end of transaction.
	/// </summary>
	public class ConnectionSynchronization : TransactionSynchronizationAdapter
	{
	    
        private ConnectionHolder connectionHolder;

        private IDbProvider dbProvider;

        private bool holderActive = true;

        private int order;
        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="connHolder"></param>
        /// <param name="provider"></param>
		public ConnectionSynchronization(ConnectionHolder connHolder, IDbProvider provider)
	    {
            connectionHolder = connHolder;
            dbProvider = provider;
            order = ConnectionUtils.CONNECTION_SYNCHRONIZATION_ORDER;
		}


        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// The value of the order property.
        /// </returns>
        public override int CompareTo(object obj)
        {
        	return order;
        }
        /// <summary>
        /// Suspend this synchronization.
        /// </summary>
        /// <remarks>
        /// 	<p>
        /// Supposed to unbind resources from
        /// <see cref="Spring.Transaction.Support.TransactionSynchronizationManager"/>
        /// if managing any.
        /// </p>
        /// </remarks>
        public override void Suspend()
        {
            if (holderActive)
            {
                TransactionSynchronizationManager.UnbindResource(dbProvider);

                //TODO do we need to reset the connection inside the conHolder to null?
            }
        }

        /// <summary>
        /// Resume this synchronization.
        /// </summary>
        /// <remarks>
        /// 	<p>
        /// Supposed to unbind resources from
        /// <see cref="Spring.Transaction.Support.TransactionSynchronizationManager"/>
        /// if managing any.
        /// </p>
        /// </remarks>
        public override void Resume()
        {
            if (holderActive)
            {
                TransactionSynchronizationManager.BindResource(dbProvider, connectionHolder);
            }
        }

        /// <summary>
        /// Invoked before transaction commit/rollback (after
        /// <see cref="Spring.Transaction.Support.ITransactionSynchronization.BeforeCommit"/>,
        /// even if
        /// <see cref="Spring.Transaction.Support.ITransactionSynchronization.BeforeCommit"/>
        /// threw an exception).
        /// </summary>
        /// <remarks>
        /// 	<p>
        /// Can e.g. perform resource cleanup.
        /// </p>
        /// 	<p>
        /// Note that exceptions will get propagated to the commit caller
        /// and cause a rollback of the transaction.
        /// </p>
        /// </remarks>
        public override void BeforeCompletion()
        {
            if (connectionHolder.IsOpen )
            {
                TransactionSynchronizationManager.UnbindResource(dbProvider);
                holderActive= false;
                ConnectionUtils.DisposeConnection(connectionHolder.Connection, dbProvider);
            }
        }

        /// <summary>
        /// Invoked after transaction commit/rollback.
        /// </summary>
        /// <param name="status">Status according to <see cref="Spring.Transaction.Support.TransactionSynchronizationStatus"/></param>
        /// <remarks>
        /// Can e.g. perform resource cleanup, in this case after transaction completion.
        /// <p>
        /// Note that exceptions will get propagated to the commit or rollback
        /// caller, although they will not influence the outcome of the transaction.
        /// </p>
        /// </remarks>
        public override void AfterCompletion(TransactionSynchronizationStatus status)
        {
            if (TransactionSynchronizationManager.HasResource(dbProvider))
            {
                TransactionSynchronizationManager.UnbindResource(dbProvider);
                holderActive = false;
                ConnectionUtils.DisposeConnection(connectionHolder.Connection, dbProvider);   
            }
        }

	}
}
