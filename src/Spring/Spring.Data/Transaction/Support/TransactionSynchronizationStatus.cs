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
	/// Enumeration of status values when synchronizing transactions.
	/// </summary>
	/// <author>Griffin Caprio</author>
	public enum TransactionSynchronizationStatus
	{
		/// <summary>
		/// Completion status in case of proper commit.
		/// </summary>
		Committed,
		/// <summary>
		/// Completion status in case of proper rollback.
		/// </summary>
		Rolledback,
		/// <summary>
		/// Completion status in case of heuristic mixed completion or system error.
		/// </summary>
		Unknown,
	}
}
