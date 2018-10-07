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

namespace Spring.Transaction
{
	/// <summary>
	/// Interface for classes that define transaction properties. Base interface for
	/// <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Note that isolation level, timeout and read-only settings will only
    /// get applied when starting a new transaction. As only
    /// <see cref="Spring.Transaction.TransactionPropagation.Required"/> and
    /// <see cref="Spring.Transaction.TransactionPropagation.RequiresNew"/> can actually cause that, it doesn't make sense
    /// to specify any of those settings else. Furthermore, not all transaction
    /// managers will support those features and thus throw respective exceptions
    /// when given non-default values.
    /// </p>
    /// </remarks>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Mark Pollack (.NET)</author>
	public interface ITransactionDefinition
	{
		/// <summary>
		/// Return the propagation behavior of type
		/// <see cref="Spring.Transaction.TransactionPropagation"/>.
		/// </summary>
		TransactionPropagation PropagationBehavior { get; }

		/// <summary>
        /// Return the isolation level of type <see cref="System.Data.IsolationLevel"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Only makes sense in combination with
        /// <see cref="Spring.Transaction.TransactionPropagation.Required"/> and
        /// <see cref="Spring.Transaction.TransactionPropagation.RequiresNew"/>.
        /// </p>
        /// <p>
        /// Note that a transaction manager that does not support custom isolation levels
        /// will throw an exception when given any other level than
        /// <see cref="System.Data.IsolationLevel.Unspecified"/>.
        /// </p>
        /// </remarks>
		IsolationLevel TransactionIsolationLevel { get; }

		/// <summary>
        /// Return the transaction timeout.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Must return a number of seconds, or -1.
        /// Only makes sense in combination with
        /// <see cref="Spring.Transaction.TransactionPropagation.Required"/> and
        /// <see cref="Spring.Transaction.TransactionPropagation.RequiresNew"/>.
        /// Note that a transaction manager that does not support timeouts will
        /// throw an exception when given any other timeout than -1.
        /// </p>
        /// </remarks>
		int TransactionTimeout { get; }

		/// <summary>
        /// Get whether to optimize as read-only transaction.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This just serves as hint for the actual transaction subsystem,
        /// it will <i>not necessarily</i> cause failure of write accesses.
        /// </p>
        /// <p>
        /// Only makes sense in combination with
        /// <see cref="Spring.Transaction.TransactionPropagation.Required"/> and
        /// <see cref="Spring.Transaction.TransactionPropagation.RequiresNew"/>.
        /// </p>
        /// <p>
        /// A transaction manager that cannot interpret the read-only hint
        /// will <i>not</i> throw an exception when given <c>ReadOnly=true</c>.
        /// </p>
        /// </remarks>
		bool ReadOnly { get; }

        /// <summary>
        /// Return the name of this transaction.  Can be null.
        /// </summary>
        /// <remarks>
        /// This will be used as a transaction name to be shown in a 
        /// transaction monitor, if applicable.  In the case of Spring
        /// declarative transactions, the exposed name will be the fully
        /// qualified type name + "." method name + assembly (by default).
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets the async flow option.
        /// </summary>
        /// <value>The async flow option.</value>
        System.Transactions.TransactionScopeAsyncFlowOption AsyncFlowOption { get;}
	}
}
