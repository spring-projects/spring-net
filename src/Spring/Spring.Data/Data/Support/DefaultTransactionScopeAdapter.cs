#if NET_2_0

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

        /// <summary>
        /// Call Complete() on the TransactionScope object created by this instance.
        /// </summary>
        public void Complete()
        {
            txScope.Complete();
        }

        /// <summary>
        /// Call Disponse() on the TransactionScope object created by this instance.
        /// </summary>
        public void Dispose()
        {
            txScope.Dispose();
        }


        /// <summary>
        /// Gets a value indicating whether there is a new transaction or an existing transaction.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is existing transaction; otherwise, <c>false</c>.
        /// </value>
        public bool IsExistingTransaction
        {
            get {
                if (System.Transactions.Transaction.Current != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether rollback only has been called (i.e. Rollback() on the
        /// Transaction object) and therefore voting that the transaction will be aborted.
        /// </summary>
        /// <value><c>true</c> if rollback only; otherwise, <c>false</c>.</value>
        public bool RollbackOnly
        {
            get
            {
                if (System.Transactions.Transaction.Current != null &&
                    System.Transactions.Transaction.Current.TransactionInformation != null &&
                    System.Transactions.Transaction.Current.TransactionInformation.Status == TransactionStatus.Aborted)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            } 
        }

        /// <summary>
        /// Creates the transaction scope.
        /// </summary>
        /// <param name="txScopeOption">The tx scope option.</param>
        /// <param name="txOptions">The tx options.</param>
        /// <param name="interopOption">The interop option.</param>
        public void CreateTransactionScope(TransactionScopeOption txScopeOption, TransactionOptions txOptions,
                                           EnterpriseServicesInteropOption interopOption)
        {
            txScope = new TransactionScope(txScopeOption, txOptions, interopOption);
        }
    }
}
#endif