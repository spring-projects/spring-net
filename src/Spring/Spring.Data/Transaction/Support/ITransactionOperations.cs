#region License

/*
 * Copyright 2002-2007 the original author or authors.
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
    /// Interface specifying basic transaction exectuion operations.
    /// </summary>
    /// <remarks>
    /// Implemented by <see cref="TransactionTemplate"/>.  Not often used directly,
    /// but a useful option to enhance testability, as it can easily be mocked or stubbed.
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollac (.NET)</author>
    public interface ITransactionOperations
    {
        /// <summary>
        /// Executes the the action specified by the given delegate callback within a transaction.
        /// </summary>
        /// <remarks>Allows for returning a result object created within the transaction, that is,
        /// a domain object or a collection of domain objects.  An exception thrown by the callback
        /// is treated as a fatal exception that enforces a rollback.  Such an exception gets
        /// propagated to the caller of the template.
        /// </remarks>
        /// <param name="transactionMethod">The delegate that specifies the transactional action.</param>
        /// <returns>A result object returned by the callback, or <code>null</code> if one</returns>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In case of initialization or system errors.
        /// </exception>
        object Execute(TransactionDelegate transactionMethod);

        /// <summary>
        /// Executes the action specified by the given callback object within a transaction.
        /// </summary>
        /// <remarks>Allows for returning a result object created within the transaction, that is,
        /// a domain object or a collection of domain objects.  An exception thrown by the callback
        /// is treated as a fatal exception that enforces a rollback.  Such an exception gets
        /// propagated to the caller of the template.
        /// </remarks>
        /// <param name="action">The callback object that specifies the transactional action.</param>
        /// <returns>A result object returned by the callback, or <code>null</code> if one</returns>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In case of initialization or system errors.
        /// </exception>
        object Execute(ITransactionCallback action);
    }
}