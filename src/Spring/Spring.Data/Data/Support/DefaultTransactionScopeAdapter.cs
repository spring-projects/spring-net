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
    /// Uses <see cref="TransactionScope"/> and System.Transactions.Transaction.Current to provide
    /// necessary state and operations to TxScopeTransactionManager.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    public class DefaultTransactionScopeAdapter : ITransactionScopeAdapter
    {
        private TransactionScope txScope;

        /// <inheritdoc />
        public void Complete()
        {
            txScope.Complete();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            txScope.Dispose();
        }

        /// <inheritdoc />
        public bool IsExistingTransaction => System.Transactions.Transaction.Current != null;

        /// <inheritdoc />
        public bool RollbackOnly
        {
            get
            {
                var transaction = System.Transactions.Transaction.Current;
                if (transaction != null &&
                    transaction.TransactionInformation != null &&
                    transaction.TransactionInformation.Status == TransactionStatus.Aborted)
                {
                    return true;
                }

                return false;
            }
        }

        /// <inheritdoc />
        public void CreateTransactionScope(
            TransactionScopeOption txScopeOption, 
            TransactionOptions txOptions,
            TransactionScopeAsyncFlowOption asyncFlowOption)
        {
            txScope = new TransactionScope(txScopeOption, txOptions, asyncFlowOption);
        }
    }
}