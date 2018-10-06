#region License

/*
 * Copyright 2007 the original author or authors.
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

using System.Transactions;

namespace Spring.Data.Support
{
    /// <summary>
    /// Provides the necessary transactional state and operations for TxScopeTransactionManager to
    /// work with TransactionScope and Transaction.Current.
    /// </summary>
    /// <remarks>Introduced for purposes of unit testing.</remarks>
    /// <author>Mark Pollack (.NET)</author>
    public interface ITransactionScopeAdapter
    {
        /// <summary>
        /// Creates the transaction scope.
        /// </summary>
        /// <param name="txScopeOption">The tx scope option.</param>
        /// <param name="txOptions">The tx options.</param>
        /// <param name="asyncFlowOption">The async flow option.</param>
        void CreateTransactionScope(
            TransactionScopeOption txScopeOption,
            TransactionOptions txOptions,
            TransactionScopeAsyncFlowOption asyncFlowOption);

        /// <summary>
        /// Call Complete() on the TransactionScope object created by this instance.
        /// </summary>
        void Complete();

        /// <summary>
        /// Call Dispose() on the TransactionScope object created by this instance.
        /// </summary>
        void Dispose();

        /// <summary>
        /// Gets a value indicating whether there is a new transaction or an existing transaction.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is existing transaction; otherwise, <c>false</c>.
        /// </value>
        bool IsExistingTransaction { get; }

        /// <summary>
        /// Gets a value indicating whether rollback only has been called (i.e. Rollback() on the
        /// Transaction object) and therefore voting that the transaction will be aborted.
        /// </summary>
        /// <value><c>true</c> if rollback only; otherwise, <c>false</c>.</value>
        bool RollbackOnly { get; }
    }
}