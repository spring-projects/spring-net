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

using Spring.Core;

namespace Spring.Transaction.Config
{
    /// <summary>
    /// This is a utility class to help in parsing the transaction namespace
    /// </summary>
    /// <author>Mark Pollack</author>
    public sealed class TxNamespaceUtils
    {
        /// <summary>
        /// The transaction manager attribute
        /// </summary>
        public const string TRANSACTION_MANAGER_ATTRIBUTE = "transaction-manager";

        /// <summary>
        /// The source of transaction metadata
        /// </summary>
        public const string TRANSACTION_ATTRIBUTE_SOURCE = "transactionAttributeSource";

        /// <summary>
        /// The property asociated with the transaction manager xml element
        /// </summary>
        public static readonly string TRANSACTION_MANAGER_PROPERTY =
            Conventions.AttributeNameToPropertyName(TRANSACTION_MANAGER_ATTRIBUTE);


    }
}