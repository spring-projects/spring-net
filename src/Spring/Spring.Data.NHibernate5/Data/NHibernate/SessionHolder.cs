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

using System.Collections;
using System.Data;
using Common.Logging;
using NHibernate;

using Spring.Transaction.Support;
using Spring.Util;

namespace Spring.Data.NHibernate
{
	/// <summary>
    /// Session holder, wrapping a NHibernate ISession and a NHibernate Transaction.
    /// HibernateTransactionManager binds instances of this class
    /// to the thread, for a given ISessionFactory.
	/// </summary>
	/// <remarks>
	/// Note: This is an SPI class, not intended to be used by applications.
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public class SessionHolder : ResourceHolderSupport
	{
	    private static readonly object DEFAULT_KEY = new object();

        private readonly object sessionDictionaryLock = new object();
        private readonly Dictionary<object, ISession> sessionDictionary = new Dictionary<object, ISession>(1);
               
        private IDbConnection connection;

        private ITransaction transaction;

        private FlushMode flushMode;

	    //needed to see if we actually assigned the enum value...
        private bool assignedPreviousFlushMode = false;

	    private static readonly ILog log = LogManager.GetLogger(typeof (SessionHolder));

	    /// <summary>
	    /// May be used by derived classes to create an empty SessionHolder.
	    /// </summary>
	    /// <remarks>
	    /// When using this ctor in your derived class, you MUST override <see cref="EnsureInitialized"/>!
	    /// </remarks>
	    protected SessionHolder()
	    {	        
	    }
	    
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionHolder"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public SessionHolder(ISession session)
        {
            AddSession(session);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionHolder"/> class.
        /// </summary>
        /// <param name="key">The key to store the session under.</param>
        /// <param name="session">The hibernate session.</param>
        public SessionHolder(object key, ISession session)
        {
            AddSession(key, session);
        }
	    
	    /// <summary>
	    /// May be overridden in a derived class to e.g. lazily create a session
	    /// </summary>
	    protected virtual void EnsureInitialized()
	    {	        
	        // noop here - but may be overridden to lazily create a session
	    }

	    /// <summary>
        /// Gets the session using the default key
        /// </summary>
        /// <value>The hibernate session.</value>
        public ISession Session
        {
            get
            {
                lock (sessionDictionaryLock)
                {
                    EnsureInitialized();
                    ISession session;
                    sessionDictionary.TryGetValue(DEFAULT_KEY, out session);
                    return session;
                }
            }
        }

        /// <summary>
        /// Gets the first session based on iteration over
        /// the IDictionary storage.
        /// </summary>
        /// <value>Any hibernate session.</value>
        public ISession AnySession
        {
            get
            {
                lock(sessionDictionaryLock)
                {
                    EnsureInitialized();                    
                    if (sessionDictionary.Count > 0)
                    {
                        IEnumerator enumerator = sessionDictionary.Values.GetEnumerator();
                        enumerator.MoveNext();
                        return (ISession)enumerator.Current;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether dictionary of
        /// hibernate sessions is empty.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this session holder is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get
            {
                lock(sessionDictionaryLock)
                {
                    EnsureInitialized();                    
                    return (sessionDictionary.Count > 0 ? false : true);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this SessionHolder
        /// does not hold non default session.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if does not hold non default session; otherwise, <c>false</c>.
        /// </value>
        public bool DoesNotHoldNonDefaultSession
        {
            get
            {
                lock (sessionDictionaryLock)
                {
                    EnsureInitialized();                    
                    return ( 
                        sessionDictionary.Count == 0 ||
                        (sessionDictionary.Count == 1 && sessionDictionary.ContainsKey(DEFAULT_KEY)) 
                        );

                }
            }
        }

        /// <summary>
        /// Gets or sets the hibernate transaction.
        /// </summary>
        /// <value>The transaction.</value>
	    public ITransaction Transaction
	    {
	        get { return transaction; }
	        set { transaction = value; }
	    }

        /// <summary>
        /// Gets or sets the ADO.NET Connection used to create the session.
        /// </summary>
        /// <value>The ADO.NET connection.</value>
	    public IDbConnection Connection
	    {
	        get { return connection; }
	        set { connection = value; }
	    }

	    /// <summary>
        /// Gets or sets the previous flush mode.
        /// </summary>
        /// <value>The previous flush mode.</value>
	    public FlushMode PreviousFlushMode
	    {
	        get { return flushMode; }
	        set
	        {
	            flushMode = value;
                assignedPreviousFlushMode = true;
	        }
	    }

        /// <summary>
        /// Gets a value indicating whether the PreviousFlushMode property
        /// was set.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if assigned PreviousFlushMode property; otherwise, <c>false</c>.
        /// </value>
        public bool AssignedPreviousFlushMode
        {
            get
            {
                return assignedPreviousFlushMode;
            }
        }

        /// <summary>
        /// Gets the validated session.
        /// </summary>
        /// <value>The validated session.</value>
        public ISession ValidatedSession
        {
            get
            {
                return GetValidatedSession(DEFAULT_KEY);
            }
        }

	    /// <summary>
        /// Gets the session given key identifier
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A hibernate session</returns>
        public ISession GetSession(object key)
        {
            lock (sessionDictionaryLock)
            {
                EnsureInitialized();
                ISession session;
                sessionDictionary.TryGetValue(key, out session);
                return session;
            }
        }

        
        /// <summary>
        /// Gets the session given the key and removes the session from
        /// the dictionary storage.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A hibernate session</returns>
        public ISession GetValidatedSession(object key)
        {
            lock (sessionDictionaryLock)
            {
                EnsureInitialized();                
                ISession session;
                // Check for dangling Session that's around but already closed.
                // Effectively an assertion: that should never happen in practice.
                // We'll seamlessly remove the Session here, to not let it cause
                // any side effects.
                if (sessionDictionary.TryGetValue(key, out session) && !session.IsOpen) 
                {
                    sessionDictionary.Remove(key);
                    session = null;
                }
                return session;
            }
        }
        /// <summary>
        /// Adds the session to the dictionary storage using the default key.
        /// </summary>
        /// <param name="session">The hibernate session.</param>
        public void AddSession(ISession session)
        {
            AddSession(DEFAULT_KEY, session);
        }

        /// <summary>
        /// Adds the session to the dictionary storage using the supplied key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="session">The hibernate session.</param>
        public void AddSession(object key, ISession session)
        {
            AssertUtils.ArgumentNotNull(key, "key", "Key must not be null");
            AssertUtils.ArgumentNotNull(session, "session", "Session must not be null");

            lock (sessionDictionaryLock)
            {
                if (sessionDictionary.ContainsKey(key))
                {
                    log.Debug("Overwriting Session in SessionHolder with key = "+ key);
                }

                sessionDictionary[key] =  session;
            }
        }

        /// <summary>
        /// Removes the session from the dictionary storage for the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The session that was previously contained in the
        /// dictionary storage.</returns>
        public ISession RemoveSession(object key)
        {
            lock(sessionDictionaryLock)
            {
                ISession oldSession;
                sessionDictionary.TryGetValue(key, out oldSession);
                sessionDictionary.Remove(key);
                return oldSession;
            }
        }

        /// <summary>
        /// Determines whether the holder the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>
        /// 	<c>true</c> if the holder contains the specified session; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsSession(ISession session)
        {
            lock (sessionDictionaryLock)
            {
                EnsureInitialized();                
                return sessionDictionary.ContainsValue(session);
            }
        }

        /// <summary>
        /// Clear the transaction state of this resource holder.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            Transaction = null;
            assignedPreviousFlushMode = false;
            Connection = null;
        }
	}
}
