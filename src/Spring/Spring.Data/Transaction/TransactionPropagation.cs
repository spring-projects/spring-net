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

namespace Spring.Transaction
{
    /// <summary>
    /// Enumeration describing <a href="http://www.springframework.net/">Spring.NET's</a>
    /// transaction propagation settings.
    /// </summary>
    /// <author>Griffin Caprio (.NET)</author>
    public enum TransactionPropagation
    {
        /// <summary>
        /// Support a current transaction, create a new one if none exists.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Analogous to System.EnterpriseServices.TransactionOption value of the same name.
        /// This is typically the default setting of a transaction definition.
        /// </p>
        /// </remarks>
        Required,

        /// <summary>
        /// Support a current transaction, execute non-transactionally if none exists.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Analogous to System.EnterpriseServices.TransactionOption.Supported.
        /// </p>
        /// </remarks>
        Supports,

        /// <summary>
        /// Support a current transaction, throw an exception if none exists.
        /// </summary>
        /// <remarks>
        /// <p>
        /// No corresponding System.EnterpriseServices.TransactionOption value.
        /// </p>
        /// </remarks>
        Mandatory,

        /// <summary>
        /// Create a new transaction, suspending the current transaction if one exists.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Analogous to System.EnterpriseServices.TransactionOption value of the same name.
        /// </p>
        /// </remarks>
        RequiresNew,

        /// <summary>
        /// Execute non-transactionally, suspending the current transaction if one exists.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Analogous to System.EnterpriseServices.TransactionOption value of the same name.
        /// </p>
        /// </remarks>
        NotSupported,

        /// <summary>
        /// Execute non-transactionally, throw an exception if a transaction exists.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Analogous to System.EnterpriseServices.TransactionOption.Disabled.
        /// </p>
        /// </remarks>
        Never,

		/// <summary>
		/// Execute within a nested transaction if a current transaction exists, else
		/// behave like <see cref="Spring.Transaction.TransactionPropagation.Required"/>. 
		/// </summary>
		/// <remarks>
		/// <p>
		/// There is no analogous feature in TransactionOption.
		/// </p>
		/// </remarks>
		Nested
    }
}
