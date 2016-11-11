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

namespace Spring.Transaction.Support
{
	/// <summary>
	/// Enumeration containing the state of transaction synchronization.
    /// </summary>
    /// <author>Griffin Caprio (.NET)</author>
	public enum TransactionSynchronizationState
	{
		/// <summary>
		/// Always activate transaction synchronization, even for "empty" transactions
		/// that result from <see cref="Spring.Transaction.TransactionPropagation"/>.
		/// </summary>
		/// <seealso cref="Spring.Transaction.TransactionPropagation.Supports"/> with no existing backend transaction.
		Always,
		/// <summary>
		/// Activate transaction synchronization only for actual transactions,
		/// i.e. not for empty ones that result from <see cref="Spring.Transaction.TransactionPropagation"/>.
		/// </summary>
        /// <seealso cref="Spring.Transaction.TransactionPropagation.Supports"/> with no
        /// existing backend transaction.
		OnActualTransaction,
		/// <summary>
		/// Never active transaction synchronization.
		/// </summary>
		Never
	}
}
