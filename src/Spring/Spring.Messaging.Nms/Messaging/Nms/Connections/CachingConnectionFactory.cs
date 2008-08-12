#region License

/*
 * Copyright 2002-2008 the original author or authors.
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

using System.Collections;
using Apache.NMS;
using Common.Logging;
using Spring.Collections;
using Spring.Util;
using IQueue=Apache.NMS.IQueue;

namespace Spring.Messaging.Nms.Connections
{
    /// <summary>
    /// <see cref="SingleConnectionFactory"/> subclass that adds
    /// Session and MessageProducer caching.  This ConnectionFactory
    /// also switches the ReconnectOnException property to true
    /// by default, allowing for automatic recovery of the underlying
    /// Connection.
    /// </summary>
    /// <remarks>
    /// By default, only one single Session will be cached, with further requested
    /// Sessions being created and disposed on demand. Consider raising the
    /// SessionCacheSize property in case of a high-concurrency environment.
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class CachingConnectionFactory : SingleConnectionFactory
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof(CachingConnectionFactory));

        #endregion

        private int sessionCacheSize = 1;

        private bool cacheProducers = true;

        private IDictionary cachedSessions = new Hashtable();


        /// <summary>
        /// Initializes a new instance of the <see cref="CachingConnectionFactory"/> class.
        /// and sets the ReconnectOnException to true
        /// </summary>
        public CachingConnectionFactory()
        {
            ReconnectOnException = true;
        }


        /// <summary>
        /// Gets or sets the size of the session cache.
        /// </summary>
        /// <remarks>
        /// This cache size is the maximum limit for the number of cached Sessions
        /// per session acknowledgement type (auto, client, dups_ok, transacted).
        /// As a consequence, the actual number of cached Sessions may be up to
        /// four times as high as the specified value - in the unlikely case
        /// of mixing and matching different acknowledgement types.
        /// <para>
        /// Default is 1: caching a single Session, (re-)creating further ones on
        /// demand. Specify a number like 10 if you'd like to raise the number of cached
        /// Sessions; that said, 1 may be sufficient for low-concurrency scenarios.
        /// </para>
        /// </remarks>
        /// <value>The size of the session cache.</value>
        public int SessionCacheSize
        {
            get { return sessionCacheSize; }
            set
            {
                AssertUtils.IsTrue(value >= 1, "Session cache size must be 1 or higher");
                sessionCacheSize = value;
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to cache MessageProducers per 
        /// Session instance. (more specifically: one MessageProducer per Destination 
        /// and Session).
        /// </summary>
        /// <remarks>
        /// <para>Default is "true". Switch this to "false" in order to recreate,
        /// MessageProducers on demand.
        /// </para>
        /// </remarks>
        /// <value><c>true</c> if should cache message producers; otherwise, <c>false</c>.</value>
        public bool CacheProducers
        {
            get { return cacheProducers; }
            set { cacheProducers = value; }
        }

        /// <summary>
        /// Resets the Session cache as well as resetting the connection.
        /// </summary>
        public override void ResetConnection()
        {
            lock (cachedSessions)
            {
                cachedSessions.Clear();                
            }
            base.ResetConnection();
        }

        /// <summary>
        /// Obtaining a cached Session.
        /// </summary>
        /// <param name="con">The connection to operate on.</param>
        /// <param name="mode">The session ack mode.</param>
        /// <returns>The Session to use
        /// </returns>
        public override ISession GetSession(IConnection con, AcknowledgementMode mode)
        {
            LinkedList sessionList;
            lock (cachedSessions)
            {
                sessionList = (LinkedList) cachedSessions[mode];
                if (sessionList == null)
                {
                    sessionList = new LinkedList();
                    cachedSessions.Add(mode, sessionList);
                }
            }

            ISession session = null;
            lock (sessionList)
            {
                if (sessionList.Count > 0)
                {
                    session = (ISession) sessionList[0];
                    sessionList.RemoveAt(0);
                }
            }
            if (session != null)
            {
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug("Found cached Session for mode " + mode + ": " + session);
                }
            } else
            {
                ISession targetSession = con.CreateSession(mode);
                session = GetCachedSessionWrapper(targetSession, sessionList);
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug("Created cached Session for mode " + mode + ": " + session);
                }
            }
            return session;
        }

        /// <summary>
        /// Wraps the given Session so that it delegates every method call to the target session but
        /// adapts close calls. This is useful for allowing application code to
	    /// handle a special framework Session just like an ordinary Session.
        /// </summary>
        /// <param name="targetSession">The original Session to wrap.</param>
        /// <param name="sessionList">The List of cached Sessions that the given Session belongs to.</param>
        /// <returns>The wrapped Session</returns>
        protected virtual ISession GetCachedSessionWrapper(ISession targetSession, LinkedList sessionList)
        {
            return new CachedSession(targetSession, sessionList, SessionCacheSize, CacheProducers);
        }
    }


}