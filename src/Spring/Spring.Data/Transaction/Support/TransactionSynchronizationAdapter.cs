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
	/// Adapter for the <see cref="Spring.Transaction.Support.ITransactionSynchronization"/>
	/// interface.
	/// </summary>
	/// <remarks>
	/// Contains empty implementations of all interface methods, for easy overriding of
	/// single methods.
	/// </remarks>
	/// <author>Juergen Hoeller</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Mark Pollack (.NET)</author>
	public abstract class TransactionSynchronizationAdapter : ITransactionSynchronization, IComparable
	{


		#region ITransactionSynchronization Members
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
		public virtual void Suspend() {}

		/// <summary>
		/// Resume this synchronization.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Supposed to unbind resources from
        /// <see cref="Spring.Transaction.Support.TransactionSynchronizationManager"/>
        /// if managing any.
        /// </p>
        /// </remarks>
		public virtual void Resume() {}

		/// <summary>
		/// Invoked before transaction commit (before
		/// <see cref="Spring.Transaction.Support.ITransactionSynchronization.BeforeCompletion"/>)
		/// </summary>
		/// <param name="readOnly">
		/// If the transaction is defined as a read-only transaction.
		/// </param>
		/// <remarks>
		/// <p>
		/// Can flush transactional sessions to the database.
		/// </p>
		/// <p>
		/// Note that exceptions will get propagated to the commit caller and
		/// cause a rollback of the transaction.
		/// </p>
		/// </remarks>
		public virtual void BeforeCommit( bool readOnly ) {}


        /// <summary>
        /// Invoked after transaction commit.
        /// </summary>
        /// <remarks>Can e.g. commit further operations that are supposed to follow on
        /// a successful commit of the main transaction.
        /// Throws exception in case of errors; will be propagated to the caller.
        /// Note: To not throw TransactionExeption sbuclasses here!
        /// </remarks>
	    public virtual void AfterCommit()
	    {
	    }

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
		public virtual void BeforeCompletion() {}

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
		public virtual void AfterCompletion( TransactionSynchronizationStatus status ) {}
		#endregion

	    ///<summary>
	    ///Compares the current instance with another object of the same type.
	    ///</summary>
	    ///
	    ///<returns>
	    ///A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than obj. Zero This instance is equal to obj. Greater than zero This instance is greater than obj.
	    ///</returns>
	    ///
	    ///<param name="obj">An object to compare with this instance. </param>
	    ///<exception cref="T:System.ArgumentException">obj is not the same type as this instance. </exception><filterpriority>2</filterpriority>
	    public virtual int CompareTo(object obj)
	    {
            return Int32.MinValue;
	    }
	}
}
