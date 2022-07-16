#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Data;
using Common.Logging;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Data.Support
{
    /// <summary>
    /// Convenient base class for ADO.NET transaction aware objects.
    /// </summary>
    /// <remarks>
    /// Can contain a ConnectionHolder object.
    /// </remarks>
    /// <author>Mark Pollack (.NET)</author>
    public abstract class AdoTransactionObjectSupport : ISavepointManager, ISmartTransactionObject
    {
        #region Fields

        private ConnectionHolder connectionHolder;

        private IsolationLevel previousIsolationLevel;

        private bool savepointAllowed;

        #endregion

        #region Constants

        /// <summary>
        /// The shared log instance for this class (and derived classes).
        /// </summary>
        protected static readonly ILog log =
            LogManager.GetLogger(typeof (AdoTransactionObjectSupport));

        #endregion

        #region Properties

        public ConnectionHolder ConnectionHolder
        {
            get { return connectionHolder; }
            set { connectionHolder = value; }
        }

        public bool HasConnectionHolder
        {
            get { return (connectionHolder != null); }
        }

        public IsolationLevel PreviousIsolationLevel
        {
            get { return previousIsolationLevel; }
            set { previousIsolationLevel = value; }
        }

        public bool SavepointAllowed
        {
            get { return savepointAllowed; }
            set { savepointAllowed = value; }
        }

        /// <summary>
        /// Return whether the transaction is internally marked as rollback-only.
        /// </summary>
        /// <value></value>
        /// <returns>True of the transaction is marked as rollback-only.</returns>
        public abstract bool RollbackOnly { get; }

        #endregion

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
        public virtual void CreateSavepoint(string savepointName)
        {
            throw new NestedTransactionNotSupportedException(
                "Cannot create a nested transaction because savepoints have not been implemented in the metadata for the DbProvider.");
        }

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
        public virtual void RollbackToSavepoint(string savepoint)
        {
            throw new NestedTransactionNotSupportedException(
                           "Cannot rollback to a savepoint in a nested transaction because savepoints have not been implemented in the metadata for the DbProvider.");

        }

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
        public virtual void ReleaseSavepoint(string savepoint)
        {
            throw new NestedTransactionNotSupportedException(
                                       "Cannot release a savepoint in a nested transaction because savepoints have not been implemented in the metadata for the DbProvider.");

        }
    }
}
