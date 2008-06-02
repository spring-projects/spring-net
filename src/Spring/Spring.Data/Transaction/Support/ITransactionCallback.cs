#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
    /// Callback interface for transactional code. 
    /// </summary>
    /// <remarks>
    /// <p>To be used with <see cref="Spring.Transaction.Support.TransactionTemplate"/>'s Execute
    /// methods.
    /// </p>
    /// <p>
    /// Typically used to gather various calls to transaction-unaware low-level
    /// services into a higher-level method implementation with transaction
    /// demarcation.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface ITransactionCallback 
    {
        /// <summary>
        /// Gets called by TransactionTemplate.Execute within a 
        /// transaction context.
        /// </summary>
        /// <param name="status">The associated transaction status.</param>
        /// <returns>A result object or <c>null</c>.</returns>
        object DoInTransaction(ITransactionStatus status);
    }
}
