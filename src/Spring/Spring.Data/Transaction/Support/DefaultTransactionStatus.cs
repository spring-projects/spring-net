#region License

/*
 * Copyright 2002-2004 the original author or authors.
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


namespace Spring.Transaction.Support
{
	/// <summary>
	/// Default implementation of the <see cref="Spring.Transaction.ITransactionStatus"/> interface,
	/// used by <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager"/>.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Holds all status information that
	/// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager"/>
	/// needs internally, including a generic transaction object determined by
	/// the concrete transaction manager implementation.
	/// </p>
	/// <p>
	/// Supports delegating savepoint-related methods to a transaction object
	/// that implements the <see cref="Spring.Transaction.ISavepointManager"/> interface.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Mark Pollack (.NET)</author>
	public class DefaultTransactionStatus : ITransactionStatus
	{
		private object _transaction;
        private bool _newTransaction;
        private bool _newSynchronization;
        private bool _readOnly;
        private readonly bool _debug;
		private object _suspendedResources;
		
	    private string _savepoint;
		private bool _rollbackOnly;
	    private bool _completed = false;

	    /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Transaction.Support.DefaultTransactionStatus"/> class.
        /// </summary>
        /// <param name="transaction">The underlying transaction object that can hold state for the internal
        /// transaction implementation.</param>
        /// <param name="newTransaction">True if the transaction is new, else false if participating in an existing transaction.</param>
        /// <param name="newSynchronization">True if a new transaction synchronization has been opened for the given
        /// <paramref name="transaction"/>.</param>
        /// <param name="readOnly">True if the transaction is read only.</param>
        /// <param name="debug">if set to <c>true</c>, enable debug log in tx managers.</param>
        /// <param name="suspendedResources">The suspended resources for the given <paramref name="transaction"/>.</param>
		public DefaultTransactionStatus( object transaction, 
											bool newTransaction, 
											bool newSynchronization,
											bool readOnly,
                                            bool debug,
											object suspendedResources)
		{
			_transaction = transaction;
			_newTransaction = newTransaction;
			_newSynchronization = newSynchronization;
			_readOnly = readOnly;
            _debug = debug;
			_suspendedResources = suspendedResources;
		}

		#region Properties

        /// <summary>
        /// Gets a value indicating whether the progress of this transaction is debugged. 
        /// This is used by AbstractPlatformTransactionManager as an optimization, to prevent repeated
        /// calls to log.IsDebug. Not really intended for client code.</summary>
        /// <value><c>true</c> if debug; otherwise, <c>false</c>.</value>
	    public bool Debug
	    {
	        get { return _debug; }
	    }

	    /// <summary>
		/// Returns the underlying transaction object.
		/// </summary>
		public object Transaction
		{
			get { return _transaction; }
		}

        /// <summary>
        /// Gets or sets a value indicating whether the Transaction is completed, that is commited or rolled back.
        /// </summary>
        /// <value><c>true</c> if completed; otherwise, <c>false</c>.</value>
	    public bool Completed
	    {
	        get { return _completed; }
	        set { _completed = value; }
	    }

	    /// <summary>
		/// Returns true if the underlying transaction is read only.
		/// </summary>
		public bool ReadOnly
		{
			get { return _readOnly; }
		}

		/// <summary>
		/// Flag indicating if a new transaction synchronization has been opened
		/// for this transaction.
		/// </summary>
		public bool NewSynchronization
		{
			get { return _newSynchronization; }
		}

		/// <summary>
		/// Returns suspended resources for this transaction.
		/// </summary>
		public object SuspendedResources
		{
			get { return _suspendedResources; }
		}

		/// <summary>
		/// Gets and sets the savepoint for the current transaction, if any. 
		/// </summary>
		public string Savepoint
		{
			get { return _savepoint; }
			set { _savepoint = value; }
		}

		/// <summary>
		/// Returns a flag indicating if the transaction has a savepoint.
		/// </summary>
		public bool HasSavepoint
		{
			get { return ( _savepoint != null ); }
		}

        /// <summary>
        /// Determines whether there is an actual transaction active.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if there is an actual transaction active; otherwise, <c>false</c>.
        /// </returns>
	    public bool HasTransaction()
	    {
	        return Transaction != null;
	    }

		/// <summary>
		/// Return the underlying transaction as a
		/// <see cref="Spring.Transaction.ISavepointManager"/>, if possible.
		/// </summary>
		/// <exception cref="Spring.Transaction.NestedTransactionNotSupportedException">
		/// If the underlying transaction does not support savepoints.
		/// </exception>
		protected ISavepointManager SavepointManager
		{
			get 
			{ 
				if ( ! IsTransactionSavepointManager ) 
				{
					throw new NestedTransactionNotSupportedException(
						"Transaction object [" + Transaction + "] does not support savepoints.");
				}	
				return ( ISavepointManager) Transaction;
			} 
		}
		/// <summary>
		/// Return true if the underlying transaction implements the
		/// <see cref="Spring.Transaction.ISavepointManager"/> interface.
		/// </summary>
		public bool IsTransactionSavepointManager
		{
			get { return ( Transaction is ISavepointManager ); }
		}
		#endregion

		#region ITransactionStatus Members
		/// <summary>
		/// Returns <b>true</b> if the transaction is new, else <b>false</b> if participating
		/// in an existing transaction.
		/// </summary>
		public bool IsNewTransaction
		{
			get
			{
				return ( HasTransaction() && _newTransaction );
			}
		}

		/// <summary>
		/// Determine the rollbackOnly flag via checking both this
		/// <see cref="Spring.Transaction.ITransactionStatus"/>
		/// and the transaction object, provided that the latter implements the
		/// <see cref="Spring.Transaction.Support.ISmartTransactionObject"/> interface.
		/// </summary>
		/// <remarks>The property can only be set to true.</remarks>
		public bool RollbackOnly
		{
			get { 
				return ( LocalRollbackOnly || GlobalRollbackOnly); 
			}
		}

        /// <summary>
        /// Set the transaction rollback-only. This instructs the transaction manager that the only possible outcome of
        /// the transaction may be a rollback, proceeding with the normal application
        /// workflow though (i.e. no exception).
        /// </summary>
        /// <remarks>
        /// 	<p>
        /// For transactions managed by a <see cref="Spring.Transaction.Support.TransactionTemplate"/> or
        /// <see cref="Spring.Transaction.Interceptor.TransactionInterceptor"/>.
        /// An alternative way to trigger a rollback is throwing an transaction exception.
        /// </p>
        /// </remarks>
	    public void SetRollbackOnly()
	    {
	        _rollbackOnly = true;	        
	    }

	    #endregion


        /// <summary>
        /// Determine the rollback-only flag via checking this TransactionStatus.  Will only
        /// return true if the application set the property RollbackOnly to true on this
        /// TransactionStatus object.
        /// </summary>
        /// <value><c>true</c> if [local rollback only]; otherwise, <c>false</c>.</value>
	    public bool LocalRollbackOnly
	    {
            get
            {
                return _rollbackOnly;
            }
        }

	    public bool GlobalRollbackOnly
	    {
	        get
	        {
	            return ((_transaction is ISmartTransactionObject) &&
	                    ((ISmartTransactionObject) _transaction).RollbackOnly);
	        }
	    }
		#region ISavepointManager Members
		/// <summary>
		/// This implementation delegates to the underlying transaction object
		/// (if it implements the <see cref="Spring.Transaction.ISavepointManager"/> interface)
		/// to create a savepoint.
		/// </summary>
		/// <exception cref="Spring.Transaction.NestedTransactionNotSupportedException">
		/// If the underlying transaction does not support savepoints.
		/// </exception>
		public void CreateSavepoint( string savepoint )
		{
			SavepointManager.CreateSavepoint( savepoint );
		}

		/// <summary>
		/// This implementation delegates to the underlying transaction object
		/// (if it implements the <see cref="Spring.Transaction.ISavepointManager"/> interface)
		/// to rollback to the supplied <paramref name="savepoint"/>.
		/// </summary>
		/// <param name="savepoint">The savepoint to rollback to.</param>
		public void RollbackToSavepoint( string savepoint )
		{
			SavepointManager.RollbackToSavepoint( savepoint );
		}

		/// <summary>
		/// This implementation delegates to the underlying transaction object
		/// (if it implements the <see cref="Spring.Transaction.ISavepointManager"/> interface)
		/// to release the supplied <paramref name="savepoint"/>.
		/// </summary>
		/// <param name="savepoint">The savepoint to release.</param>
		public void ReleaseSavepoint( string savepoint)
		{
			SavepointManager.ReleaseSavepoint( savepoint );
		}

		#endregion
		/// <summary>
		/// Create a savepoint and hold it for the transaction.
		/// </summary>
		/// <exception cref="Spring.Transaction.NestedTransactionNotSupportedException">
		/// If the underlying transaction does not support savepoints.
		/// </exception>
		public void CreateAndHoldSavepoint( string savepoint ) 
		{
			SavepointManager.CreateSavepoint( savepoint );
			Savepoint = savepoint;
		}

		/// <summary>
		/// Roll back to the savepoint that is held for the transaction.
		/// </summary>
		/// <exception cref="Spring.Transaction.TransactionUsageException">
		/// If no save point has been created.
		/// </exception>
		public void RollbackToHeldSavepoint() 
		{
			if ( ! HasSavepoint ) 
			{
				throw new TransactionUsageException( "No savepoint associated with current transaction" );
			}
			SavepointManager.RollbackToSavepoint( Savepoint );
		}

		/// <summary>
		/// Release the savepoint that is held for the transaction.
		/// </summary>
		/// <exception cref="Spring.Transaction.TransactionUsageException">
		/// If no save point has been created.
		/// </exception>
		public void ReleaseHeldSavepoint() 
		{
			if ( ! HasSavepoint ) 
			{
				throw new TransactionUsageException( "No savepoint associated with current transaction" );
			}
			SavepointManager.ReleaseSavepoint( Savepoint );
		}
	}
}
