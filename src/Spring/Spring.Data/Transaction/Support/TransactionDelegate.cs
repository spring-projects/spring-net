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
    /// Callback delegate for performing actions within a transactional context.
    /// </summary>
    /// <remarks>
    /// <p>To be used with <see cref="TransactionTemplate"/>'s Execute
    /// methods.
    /// </p>
    /// <p>
    /// Typically used to gather various calls to transaction-unaware low-level
    /// services into a higher-level method implementation with transaction
    /// demarcation.
    /// </p>
    /// </remarks>
    /// <param name="status">The status of the transaction, can be used to
    /// trigger a rollback the current transaction by settings its
    /// RollbackOnly property to true.</param>
    /// <returns>A result object or <c>null</c>.</returns>
    public delegate object TransactionDelegate(ITransactionStatus status);
}
