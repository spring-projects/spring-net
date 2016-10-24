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
	/// This is the central interface in Spring.NET's transaction support.
	/// </summary>
	/// <remarks>
	/// <p>
    /// Applications can use this directly, but it is not primarily meant as an API.
    /// Typically, applications will work with either
    /// <see cref="Spring.Transaction.Support.TransactionTemplate"/> or the AOP transaction
    /// interceptor.
    /// </p>
    /// <p>
    /// For implementers, <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager"/>
    /// is a good starting point.
    /// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	public interface IPlatformTransactionManager
	{
		/// <summary>
		/// Return a currently active transaction or create a new one.
		/// </summary>
		/// <remarks>
		/// <p>
        /// Note that parameters like isolation level or timeout will only be applied
        /// to new transactions, and thus be ignored when participating in active ones.
        /// Furthermore, they aren't supported by every transaction manager:
        /// a proper implementation should throw an exception when custom values
        /// that it doesn't support are specified.
		/// </p>
		/// </remarks>
		/// <param name="definition">
		/// <see cref="Spring.Transaction.ITransactionDefinition"/> instance (can be null for
		/// defaults), describing propagation behavior, isolation level, timeout etc.
		/// </param>
		/// <exception cref="Spring.Transaction.TransactionException">
		/// In case of lookup, creation, or system errors.
		/// </exception>
		/// <returns>
		/// A <see cref="Spring.Transaction.ITransactionStatus"/> representing the new or current transaction.
		/// </returns>
		ITransactionStatus GetTransaction( ITransactionDefinition definition );

		/// <summary>
        /// Commit the given transaction, with regard to its status.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the transaction has been marked rollback-only programmatically,
        /// perform a rollback.
        /// </p>
        /// <p>
        /// If the transaction wasn't a new one, omit the commit to take part
        /// in the surrounding transaction properly.
        /// </p>
        /// </remarks>
		/// <param name="transactionStatus">
		/// The <see cref="Spring.Transaction.ITransactionStatus"/> instance returned by the
		/// <see cref="Spring.Transaction.IPlatformTransactionManager.GetTransaction"/>() method.
		/// </param>
		/// <exception cref="Spring.Transaction.TransactionException">
		/// In case of commit or system errors
		/// </exception>
		void Commit( ITransactionStatus transactionStatus );

		/// <summary>
		/// Roll back the given transaction, with regard to its status.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the transaction wasn't a new one, just set it rollback-only
        /// to take part in the surrounding transaction properly.
        /// </p>
        /// </remarks>
		/// <param name="transactionStatus">
		/// The <see cref="Spring.Transaction.ITransactionStatus"/> instance returned by the
		/// <see cref="Spring.Transaction.IPlatformTransactionManager.GetTransaction"/>() method.
		/// </param>
		/// <exception cref="Spring.Transaction.TransactionException">
		/// In case of system errors.
		/// </exception>
		void Rollback( ITransactionStatus transactionStatus );
	}
}
