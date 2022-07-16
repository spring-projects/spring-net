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
using Spring.Data.Common;
using Spring.Transaction.Support;
using Spring.Util;

namespace Spring.Data.Support
{
    /// <summary>
    /// Summary description for DbProviderUtils.
    /// </summary>
    public abstract class ConnectionUtils
    {
        #region Logging

        private static readonly ILog LOG = LogManager.GetLogger(typeof(ConnectionUtils));

        #endregion

        public static readonly int CONNECTION_SYNCHRONIZATION_ORDER = 1000;
        /// <summary>
        /// Dispose of the given Connection, created via the given IDbProvider,
        /// if it is not managed externally (that is, not bound to the thread).
        /// </summary>
        /// <param name="conn">The connection to close if necessary.  If
        /// this is null the call will be ignored. </param>
        /// <param name="dbProvider">The IDbProvider the connection came from</param>
        public static void DisposeConnection(IDbConnection conn, IDbProvider dbProvider)
        {
            try
            {
                DoDisposeConnection(conn, dbProvider);
            }
            catch (Exception e)
            {
                LOG.Warn("Could not close connection", e);
            }

        }
        private static void DoDisposeConnection(IDbConnection conn, IDbProvider dbProvider)
        {
            if (conn == null)
            {
                return;
            }

            if (dbProvider != null)
            {
                ConnectionHolder conHolder = (ConnectionHolder)TransactionSynchronizationManager.GetResource(dbProvider);
                if (conHolder != null && ConnectionEquals(conHolder.Connection, conn))
                {
                    // It's the transactional connection bound to the thread so don't close it.
                    conHolder.Released();
                    return;
                }
            }
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Disposing of IDbConnection with connection string = [" + dbProvider.ConnectionString + "]");
            }
            conn.Dispose();
        }


        /// <summary>
        /// Get a ADO.NET Connection/Transaction Pair for the given IDbProvider.
        /// Changes any exception into the Spring hierarchy of generic data access
        /// exceptions, simplifying calling code and making any exception that is
        /// thrown more meaningful.
        /// </summary>
        /// <remarks>
        /// Is aware of a corresponding Connection/Transaction bound to the current thread, for example
        /// when using AdoPlatformTransactionManager. Will bind a IDbConnection to the thread
        /// if transaction synchronization is active
        /// </remarks>
        /// <param name="provider">The provider.</param>
        /// <returns>A Connection/Transaction pair.</returns>
        public static ConnectionTxPair GetConnectionTxPair(IDbProvider provider)
        {
            try
            {
                return DoGetConnection(provider);
            }
            catch (Exception e)
            {
                throw new CannotGetAdoConnectionException("Could not get ADO.NET connection.", e);
            }

        }


        /// <summary>
        /// Get a ADO.NET Connection/Transaction Pair for the given IDbProvider.
        /// Same as <see cref="GetConnection"/> but throwing original provider
        /// exception.
        /// </summary>
        /// <remarks>
        /// Is aware of a corresponding Connection/Transaction bound to the current thread, for example
        /// when using AdoPlatformTransactionManager. Will bind a IDbConnection to the thread
        /// if transaction synchronization is active
        /// </remarks>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public static ConnectionTxPair DoGetConnection(IDbProvider provider)
        {
            AssertUtils.ArgumentNotNull(provider, "provider");
            ConnectionHolder conHolder = (ConnectionHolder)TransactionSynchronizationManager.GetResource(provider);
            if (conHolder != null && (conHolder.HasConnection || conHolder.SynchronizedWithTransaction))
            {
                conHolder.Requested();
                if (!conHolder.HasConnection)
                {
                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug("Fetching resumed ADO.NET connection from DbProvider");
                    }
                    conHolder.Connection = provider.CreateConnection();
                }
                return new ConnectionTxPair(conHolder.Connection, conHolder.Transaction);
            }

            // Else we either got no holder or an empty thread-bound holder here.
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Fetching Connection from DbProvider");
            }
            IDbConnection conn = provider.CreateConnection();
            conn.Open();

            if (TransactionSynchronizationManager.SynchronizationActive)
            {
                LOG.Debug("Registering transaction synchronization for IDbConnection");
                //Use same connection for further ADO.NET actions with the transaction.
                //Thread-bound object will get removed by manager at transaction completion.

                ConnectionHolder holderToUse = conHolder;
                if (holderToUse == null)
                {
                    holderToUse = new ConnectionHolder(conn, null);
                }
                else
                {
                    holderToUse.Connection = conn;
                }
                holderToUse.Requested();
                TransactionSynchronizationManager.RegisterSynchronization(
                    new ConnectionSynchronization(holderToUse, provider));
                holderToUse.SynchronizedWithTransaction = true;
                if (holderToUse != conHolder)
                {
                    TransactionSynchronizationManager.BindResource(provider, holderToUse);
                }

            }
            return new ConnectionTxPair(conn, null);
        }

        /// <summary>
        /// Do the connection mgmt.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IDbConnection GetConnection(IDbProvider provider)
        {
            AssertUtils.ArgumentNotNull(provider, "provider");

            return GetConnectionTxPair(provider).Connection;

        }

        private static bool ConnectionEquals(IDbConnection heldCon, IDbConnection passedInCon)
        {
            return (heldCon == passedInCon || heldCon.Equals(passedInCon) ||
                getTargetConnection(heldCon).Equals(passedInCon));
        }

        private static IDbConnection getTargetConnection(IDbConnection con)
        {
            IDbConnection conToUse = con;
            /*
            while (conToUse is ConnectionProxy)
            {
                conToUse = (ConnectionProxy)conToUse.getTargetConnection();
            }
            */
            return conToUse;
        }

        /// <summary>
        /// Applies the current transaction timeout, if any, to the given ADO.NET IDbCommand object.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="dbProvider">The db provider.</param>
        public static void ApplyTransactionTimeout(IDbCommand command, IDbProvider dbProvider)
        {
            ApplyTransactionTimeout(command, dbProvider, 0);
        }

        /// <summary>
        /// Applies the specified timeout - overridden by the current transaction timeout, if any, to to the
        /// given ADO.NET IDb command object.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="dbProvider">The db provider the command was obtained from.</param>
        /// <param name="timeout">The timeout to apply (or 0 for no timeout outside of a transaction.</param>
        public static void ApplyTransactionTimeout(IDbCommand command, IDbProvider dbProvider, int timeout)
        {
            AssertUtils.ArgumentNotNull(command, "command", "No IDbCommand specified.");
            AssertUtils.ArgumentNotNull(dbProvider, "dbProvider", "No IDbProvider specified.");

            ConnectionHolder conHolder = (ConnectionHolder)TransactionSynchronizationManager.GetResource(dbProvider);
            if (conHolder != null && conHolder.HasTimeout)
            {
                // Remaining transaction timeout overrides specified value.
                command.CommandTimeout = conHolder.TimeToLiveInSeconds;
            }
            else if (timeout != -1)
            {
                // No current transaction timeout -> apply specified value.  0 = infinite timeout in some drivers.
                command.CommandTimeout = timeout;
            }

        }
    }
}
