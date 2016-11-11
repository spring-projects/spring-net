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

namespace Spring.Transaction.Support
{
	/// <summary>
	/// Interface for transaction synchronization callbacks.
	/// </summary>
	/// <remarks>
	/// Supported by <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager"/>.
	/// </remarks>
	/// <author>Juergen Hoeller</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Mark Pollack (.NET)</author>
	public interface ITransactionSynchronization
	{
        /// <summary>
        /// Suspend this synchronization. 
        /// </summary>
        /// <remarks>
        /// <p>
        /// Supposed to unbind resources from
        /// <see cref="Spring.Transaction.Support.TransactionSynchronizationManager"/>
        /// if managing any.
        /// </p>
        /// </remarks>
		void Suspend();

        /// <summary>
        /// Resume this synchronization.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Supposed to rebind resources from
        /// <see cref="Spring.Transaction.Support.TransactionSynchronizationManager"/>
        /// if managing any.
        /// </p>
        /// </remarks>
		void Resume();

        /// <summary>
        /// Invoked before transaction commit (before
        /// <see cref="Spring.Transaction.Support.ITransactionSynchronization.BeforeCompletion"/>)
        /// Can e.g. flush transactional O/R Mapping sessions to the database
        /// </summary>
        /// <remarks>
        /// <para>
        /// This callback does not mean that the transaction will actually be
        /// commited.  A rollback decision can still occur after this method
        /// has been called.  This callback is rather meant to perform work 
        /// that's only relevant if a commit still has a chance
        /// to happen, such as flushing SQL statements to the database.
        /// </para>
        /// <para>
        /// Note that exceptions will get propagated to the commit caller and cause a
        /// rollback of the transaction.</para>
        /// <para>
        /// (note: do not throw TransactionException subclasses here!)
        /// </para>
        /// </remarks>
        /// <param name="readOnly">
        /// If the transaction is defined as a read-only transaction.
        /// </param>
		void BeforeCommit( bool readOnly );

        /// <summary>
        /// Invoked after transaction commit.
        /// </summary>
        /// <remarks>Can e.g. commit further operations that are supposed to follow on
        /// a successful commit of the main transaction.
        /// Throws exception in case of errors; will be propagated to the caller.
        /// Note: To not throw TransactionExeption sbuclasses here!
        /// </remarks>
        /// 
        void AfterCommit();
	    
        /// <summary>
        /// Invoked before transaction commit/rollback (after
        /// <see cref="Spring.Transaction.Support.ITransactionSynchronization.BeforeCommit"/>,
        /// even if
        /// <see cref="Spring.Transaction.Support.ITransactionSynchronization.BeforeCommit"/>
        /// threw an exception).
        /// </summary>
        /// <remarks>
        /// <p>
        /// Can e.g. perform resource cleanup.
        /// </p>
        /// <p>
        /// Note that exceptions will get propagated to the commit caller
        /// and cause a rollback of the transaction.
        /// </p>
        /// </remarks>
		void BeforeCompletion();

        /// <summary>
        /// Invoked after transaction commit/rollback.
        /// </summary>
        /// <param name="status">
        /// Status according to <see cref="Spring.Transaction.Support.TransactionSynchronizationStatus"/>
        /// </param>
        /// <remarks>
        /// Can e.g. perform resource cleanup, in this case after transaction completion.
        /// <p>
        /// Note that exceptions will get propagated to the commit or rollback
        /// caller, although they will not influence the outcome of the transaction.
        /// </p>
        /// </remarks>
		void AfterCompletion( TransactionSynchronizationStatus status );
	}
}
