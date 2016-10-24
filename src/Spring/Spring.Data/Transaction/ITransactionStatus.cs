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

namespace Spring.Transaction
{
	/// <summary>
	/// Representation of the status of a transaction,
    /// consisting of a transaction object and some status flags.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Transactional code can use this to retrieve status information,
    /// and to programmatically request a rollback (instead of throwing
    /// an exception that causes an implicit rollback).
    /// </p>
    /// <p>
	/// Derives from the <see cref="Spring.Transaction.ISavepointManager"/> interface to provide access
	/// to savepoint management facilities. Note that savepoint management
	/// is just available if the actual transaction manager supports it.
    /// </p>
    /// </remarks>
	/// <author>Juergen Hoeller</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Mark Pollack (.NET)</author>
	public interface ITransactionStatus : ISavepointManager
	{
		/// <summary>
		/// Returns <b>true</b> if the transaction is new, else <b>false</b> if participating
		/// in an existing transaction.
		/// </summary>
		bool IsNewTransaction { get; }

		/// <summary>
        /// Return whether the transaction has been marked as rollback-only, 
        /// (either by the application or by the transaction infrastructure).
        /// </summary>
		bool RollbackOnly { get; }

        /// <summary>
        /// Set the transaction rollback-only. This instructs the transaction manager that the only possible outcome of
        /// the transaction may be a rollback, proceeding with the normal application
        /// workflow though (i.e. no exception).
        /// </summary>
        /// <remarks>
        /// <p>
        /// For transactions managed by a <see cref="Spring.Transaction.Support.TransactionTemplate"/> or
        /// <see cref="Spring.Transaction.Interceptor.TransactionInterceptor"/>.
        /// An alternative way to trigger a rollback is throwing an transaction exception.
        /// </p>
        /// </remarks>
	    void SetRollbackOnly();

	    /// <summary>
		/// Gets the current transaction object.
		/// </summary>
		/// <remarks>
		/// Returns the current transaction object for a given connection.
		/// <p>
		/// Used to associate with the <see cref="System.Data.IDbCommand.Transaction"/> property.
		/// </p>
		/// </remarks>
		object Transaction { get; }

        bool Completed { get; }
	}
}
