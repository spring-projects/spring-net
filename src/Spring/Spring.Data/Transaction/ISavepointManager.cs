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

namespace Spring.Transaction
{
	/// <summary>
	/// Interface that specifies means to programmatically manage
	/// transaction savepoints in a generic fashion.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Note that savepoints can only work within an active transaction.
	/// Just use this programmatic savepoint handling for advanced needs;
	/// else, a subtransaction with a <see cref="Spring.Transaction.TransactionPropagation"/> value
	/// of <see cref="Spring.Transaction.TransactionPropagation.Nested"/> is preferable.
	///	</p>	
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	public interface ISavepointManager
	{
		/// <summary>
		/// Create a new savepoint.
		/// </summary>
		/// <param name="savepointName">
		/// The name of the savepoint to create.
		/// </param>
		/// <remarks>
		/// You can roll back to a specific savepoint
		/// via <see cref="Spring.Transaction.ISavepointManager.RollbackToSavepoint"/>,
		/// and explicitly release a savepoint that you don't need anymore via
		/// <see cref="Spring.Transaction.ISavepointManager.ReleaseSavepoint"/>.
		/// <p>
		/// Note that most transaction managers will automatically release
		/// savepoints at transaction completion.
		/// </p>
		/// </remarks>
		/// <exception cref="Spring.Transaction.TransactionException">
		/// If the savepoint could not be created,
		/// either because the backend does not support it or because the
		/// transaction is not in an appropriate state.
		/// </exception>
		/// <returns>
		/// A savepoint object, to be passed into
		/// <see cref="Spring.Transaction.ISavepointManager.RollbackToSavepoint"/>
		/// or <see cref="Spring.Transaction.ISavepointManager.ReleaseSavepoint"/>.
		/// </returns>
		void CreateSavepoint( string savepointName );

		/// <summary>
		/// Roll back to the given savepoint.
		/// </summary>
		/// <remarks>
		/// The savepoint will be automatically released afterwards.
		/// </remarks>
		/// <param name="savepoint">The savepoint to roll back to.</param>
		/// <exception cref="Spring.Transaction.TransactionException">
		/// If the rollback failed.
		/// </exception>
		void RollbackToSavepoint( string savepoint );

		/// <summary>
		/// Explicitly release the given savepoint.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Note that most transaction managers will automatically release
		/// savepoints at transaction completion.
		/// </p>
		/// <p>
		/// Implementations should fail as silently as possible if
		/// proper resource cleanup will still happen at transaction completion.
		/// </p>
		/// </remarks>
		/// <param name="savepoint">The savepoint to release.</param>
		/// <exception cref="Spring.Transaction.TransactionException">
		/// If the release failed.
		/// </exception>
		void ReleaseSavepoint( string savepoint );
	}
}
