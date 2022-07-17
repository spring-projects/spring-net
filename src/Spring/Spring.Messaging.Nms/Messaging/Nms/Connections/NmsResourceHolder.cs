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

using Common.Logging;
using Spring.Transaction.Support;
using Spring.Util;
using Apache.NMS;
using Spring.Messaging.Nms.Support;

namespace Spring.Messaging.Nms.Connections
{
    /// <summary>Connection holder, wrapping a NMS Connection and a NMS Session.
    /// MessageTransactionManager binds instances of this class to the thread,
    /// for a given NMS ConnectionFactory.
    ///
    /// <p>Note: This is an SPI class, not intended to be used by applications.</p>
    ///
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class NmsResourceHolder : ResourceHolderSupport
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(NmsResourceHolder));

        private readonly IConnectionFactory connectionFactory;
        private readonly bool frozen = false;
        private readonly List<IConnection> connections = new List<IConnection>();
        private readonly List<ISession> sessions = new List<ISession>();
        private readonly Dictionary<IConnection, List<ISession>> sessionsPerIConnection = new Dictionary<IConnection, List<ISession>>();

        /// <summary> Create a new MessageResourceHolder that is open for resources to be added.</summary>
        public NmsResourceHolder()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="NmsResourceHolder"/> class
        /// at is open for resources to be added.
        /// </summary>
        /// <param name="connectionFactory">The connection factory that this
        /// resource holder is associated with (may be <code>null</code>)
        /// </param>
        public NmsResourceHolder(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NmsResourceHolder"/> class for the
        /// given Session.
        /// </summary>
        /// <param name="session">The session.</param>
        public NmsResourceHolder(ISession session)
        {
            AddSession(session);
            frozen = true;
        }

        /// <summary> Create a new MessageResourceHolder for the given NMS resources.</summary>
        /// <param name="connection">the NMS Connection
        /// </param>
        /// <param name="session">the NMS Session
        /// </param>
        public NmsResourceHolder(IConnection connection, ISession session)
        {
            AddConnection(connection);
            AddSession(session, connection);
            frozen = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NmsResourceHolder"/> class.
        /// </summary>
        /// <param name="connectionFactory">The connection factory.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="session">The session.</param>
        public NmsResourceHolder(IConnectionFactory connectionFactory, IConnection connection, ISession session)
        {
            this.connectionFactory = connectionFactory;
            AddConnection(connection);
            AddSession(session, connection);
            frozen = true;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="NmsResourceHolder"/> is frozen, namely that 
        /// additional resources can be registered with the holder.  If using any of the constructors with
        /// a Session, the holder will be set to the frozen state.
        /// </summary>
        /// <value><c>true</c> if frozen; otherwise, <c>false</c>.</value>
        public virtual bool Frozen => frozen;

        /// <summary>
        /// Adds the connection to the list of resources managed by this holder.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void AddConnection(IConnection connection)
        {
            AssertUtils.IsTrue(!frozen, "Cannot add IConnection because MessageResourceHolder is frozen");
            AssertUtils.ArgumentNotNull(connection, "IConnection must not be null");
            if (!connections.Contains(connection))
            {
                connections.Add(connection);
            }
        }

        /// <summary>
        /// Adds the session to the list of resources managed by this holder.
        /// </summary>
        /// <param name="session">The session.</param>
        public void AddSession(ISession session)
        {
            AddSession(session, null);
        }

        /// <summary>
        /// Adds the session and connection to the list of resources managed by this holder.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="connection">The connection.</param>
        public void AddSession(ISession session, IConnection connection)
        {
            AssertUtils.IsTrue(!frozen, "Cannot add ISession because MessageResourceHolder is frozen");
            AssertUtils.ArgumentNotNull(session, "ISession must not be null");
            if (!sessions.Contains(session))
            {
                sessions.Add(session);
                if (connection != null)
                {
                    if (!sessionsPerIConnection.TryGetValue(connection, out var sessionsList))
                    {
                        sessionsList = new List<ISession>();
                        sessionsPerIConnection[connection] = sessionsList;
                    }
                    sessionsList.Add(session);
                }
            }
        }

        /// <summary>
        /// Gets the connection managed by this resource holder
        /// </summary>
        /// <returns>A Connection, or null if no managed connection.</returns>
        public virtual IConnection GetConnection()
        {
            return (connections.Count != 0 ? connections[0] : null);
        }

        /// <summary>
        /// Gets the connection of a given type managed by this resource holder.  This is used
        /// when storing Queue or Topic Connections (from the older 1.0.2 API) as compared to the
        /// 'unified domain' API , just Connection, in the newer 1.2 API.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        /// <returns>The connection, or null if not found.</returns>
        public virtual IConnection GetConnection(Type connectionType)
        {
            return (IConnection)CollectionUtils.FindValueOfType(connections, connectionType);
        }

        /// <summary>
        /// Gets the first session manged by this holder or null if not available.
        /// </summary>
        /// <returns>The session or null if not available.</returns>
       public virtual ISession GetSession()
        {
           return sessions.Count != 0 ? sessions[0] : null;
        }

        /// <summary>
        /// Gets the session managed by this holder by type.
        /// </summary>
        /// <param name="sessionType">Type of the session.</param>
        /// <returns>The session or null if not available.</returns>
        public virtual ISession GetSession(Type sessionType)
        {
            return GetSession(sessionType, null);
        }

        /// <summary>
        /// Gets the session of a given type associated with the given connection
        /// </summary>
        /// <param name="sessionType">Type of the session.</param>
        /// <param name="connection">The connection.</param>
        /// <returns>The sessin or null if not available.</returns>
        public virtual ISession GetSession(Type sessionType, IConnection connection)
        {
            var sessions = this.sessions;
            if (connection != null)
            {
                sessionsPerIConnection.TryGetValue(connection, out sessions);
            }

            return (ISession) CollectionUtils.FindValueOfType(sessions, sessionType);
        }

        /// <summary>
        /// Commits all sessions.
        /// </summary>
        public virtual void CommitAll()
        {
            foreach (ISession session in sessions)
            {
				session.Commit();
            }
        }

        /// <summary>
        /// Closes all sessions then stops and closes all connections, in that order.
        /// </summary>
        public virtual void CloseAll()
        {
            foreach (ISession session in sessions)
            {
                try
                {
                    session.Close();
                }
                catch (Exception ex)
                {
                    logger.Debug("Could not close NMS ISession after transaction", ex);
                }
            }
            foreach (IConnection connection in connections)
            {
                ConnectionFactoryUtils.ReleaseConnection(connection, connectionFactory, true);
            }
            connections.Clear();
            sessions.Clear();
            sessionsPerIConnection.Clear();            
        } 
        
        /// <summary>
        /// Commits all sessions.
        /// </summary>
        public virtual async Task CommitAllAsync()
        {
            foreach (ISession session in sessions)
            {
				await session.CommitAsync().Awaiter();
            }
        }

        /// <summary>
        /// Closes all sessions then stops and closes all connections, in that order.
        /// </summary>
        public virtual async Task CloseAllAsync()
        {
            foreach (ISession session in sessions)
            {
                try
                {
                    await session.CloseAsync().Awaiter();
                }
                catch (Exception ex)
                {
                    logger.Debug("Could not close NMS ISession after transaction", ex);
                }
            }
            foreach (IConnection connection in connections)
            {
                await ConnectionFactoryUtils.ReleaseConnectionAsync(connection, connectionFactory, true).Awaiter();
            }
            connections.Clear();
            sessions.Clear();
            sessionsPerIConnection.Clear();            
        }

        /// <summary>
        /// Determines whether the holder contains the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>
        /// 	<c>true</c> if the holder contains the specified session; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsSession(ISession session)
        {
            return sessions.Contains(session);
        }
    }
}
