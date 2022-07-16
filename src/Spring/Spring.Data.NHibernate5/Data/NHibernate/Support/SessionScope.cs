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

using System.Reflection;

using Common.Logging;
using NHibernate;
using Spring.Threading;
using Spring.Transaction.Support;
using Spring.Util;

namespace Spring.Data.NHibernate.Support
{
    /// <summary>
    /// Implementation of SessionScope that associates a single session within the using scope.
    /// </summary>
    /// <remarks>
    /// <para>It is recommended to be used in the following type of scenario:
    /// <code>
    /// using (new SessionScope())
    /// {
    ///    ... do multiple operation, possibly in multiple transactions. 
    /// }
    /// </code>
    /// At the end of "using", the session is automatically closed. All transactions within the scope use the same session,
    /// if you are using Spring's HibernateTemplate or using Spring's implementation of NHibernate 1.2's 
    /// ICurrentSessionContext interface.  
    /// </para>
    /// <para>
    /// It is assumed that the session factory object name is called "SessionFactory". In case that you named the object 
    /// in different way you can specify your can specify it in the application settings using the key
    /// Spring.Data.NHibernate.Support.SessionScope.SessionFactoryObjectName.  Values for EntityInterceptorObjectName
    /// and SingleSessionMode can be specified similarly.
    /// </para>
    /// <para>
    /// <b>Note:</b>
    /// The session is managed on a per thread basis on the thread that opens the scope instance. This means that you must
    /// never pass a reference to a <see cref="SessionScope"/> instance over to another thread!
    /// </para>
    /// </remarks>
    /// <author>Robert M. (.NET)</author>
    /// <author>Harald Radi (.NET)</author>
    public class SessionScope : IDisposable
    {
        /// <summary>
        /// The logging instance.
        /// </summary>        
        protected readonly ILog log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        private readonly SessionScopeSettings settings;

        // Keys into LogicalThreadContext for runtime values.
        private readonly string PARTICIPATE_KEY;
        private readonly string ISOPEN_KEY;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionScope"/> class in single session mode, 
        /// associating a session with the thread.  The session is opened lazily on demand.
        /// </summary>
        public SessionScope() 
            : this(true)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SessionScope"/> class.
        /// </summary>
        /// <param name="open">
        /// If set to <c>true</c> associate a session with the thread.  If false, another
        /// collaborating class will associate the session with the thread, potentially by calling
        /// the Open method on this class.
        /// </param>
        public SessionScope(bool open)
            : this("appSettings", open)
        {
            // noop
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionScope"/> class.
        /// </summary>
        /// <param name="sectionName">
        /// The name of the configuration section to read configuration settings from. 
        /// See <see cref="ConfigSectionSessionScopeSettings"/> for more info.
        /// </param>
        /// <param name="open">
        /// If set to <c>true</c> associate a session with the thread.  If false, another
        /// collaborating class will associate the session with the thread, potentially by calling
        /// the Open method on this class.
        /// </param>
        public SessionScope(string sectionName, bool open)
            : this( sectionName, typeof(SessionScope), open)
        {
            // noop
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SessionScope"/> class.
        /// </summary>
        /// <param name="sectionName">
        /// The name of the configuration section to read configuration settings from. 
        /// See <see cref="ConfigSectionSessionScopeSettings"/> for more info.
        /// </param>
        /// <param name="namespaceType">The type, who's full name is used for prefixing appSetting keys</param>
        /// <param name="open">
        /// If set to <c>true</c> associate a session with the thread.  If false, another
        /// collaborating class will associate the session with the thread, potentially by calling
        /// the Open method on this class.
        /// </param>
        public SessionScope(string sectionName, Type namespaceType, bool open)
          : this(new ConfigSectionSessionScopeSettings(namespaceType, sectionName), open)
        {
          // noop
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionScope"/> class.
        /// </summary>
        /// <param name="sessionFactory">
        /// The <see cref="ISessionFactory"/> instance to be used for obtaining <see cref="ISession"/> instances.
        /// </param>
        /// <param name="open">
        /// If set to <c>true</c> associate a session with the thread.  If false, another
        /// collaborating class will associate the session with the thread, potentially by calling
        /// the Open method on this class.
        /// </param>
        public SessionScope(ISessionFactory sessionFactory, bool open)
            : this(new SessionScopeSettings(sessionFactory), open)
        {
            // noop
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionScope"/> class.
        /// </summary>
        /// <param name="sessionFactory">
        /// The <see cref="ISessionFactory"/> instance to be used for obtaining <see cref="ISession"/> instances.
        /// </param>
        /// <param name="entityInterceptor">
        /// Specify the <see cref="IInterceptor"/> to be set on each session provided by this <see cref="SessionScope"/> instance.
        /// </param>
        /// <param name="singleSession">
        /// Set whether to use a single session for each request. See <see cref="SingleSession"/> property for details.
        /// </param>
        /// <param name="defaultFlushMode">
        /// Specify the flushmode to be applied on each session provided by this <see cref="SessionScope"/> instance.
        /// </param>
        /// <param name="open">
        /// If set to <c>true</c> associate a session with the thread.  If false, another
        /// collaborating class will associate the session with the thread, potentially by calling
        /// the Open method on this class.
        /// </param>
        public SessionScope(ISessionFactory sessionFactory, IInterceptor entityInterceptor, bool singleSession, FlushMode defaultFlushMode, bool open)
            :this(new SessionScopeSettings(sessionFactory, entityInterceptor, singleSession, defaultFlushMode), open)
        {
            // noop
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionScope"/> class.
        /// </summary>
        /// <param name="settings">An <see cref="SessionScopeSettings"/> instance holding the scope configuration</param>
        /// <param name="open">
        /// If set to <c>true</c> associate a session with the thread.  If false, another
        /// collaborating class will associate the session with the thread, potentially by calling
        /// the Open method on this class.
        /// </param>
        public SessionScope(SessionScopeSettings settings, bool open)
        {
            log = LogManager.GetLogger(this.GetType());
            this.settings = settings;

            PARTICIPATE_KEY = UniqueKey.GetInstanceScopedString(this, "Participate");
            ISOPEN_KEY = UniqueKey.GetInstanceScopedString(this, "IsOpen");

            if (open)
            {
                Open();
            }
        }

        /// <summary>
        /// Set whether to use a single session for each request. Default is "true".
        /// If set to false, each data access operation or transaction will use
        /// its own session (like without Open Session in View). Each of those
        /// sessions will be registered for deferred close, though, actually
        /// processed at request completion.
        /// </summary>
        public bool SingleSession
        {
            get { return settings.SingleSession; }
        }

        /// <summary>
        /// Gets the flushmode to be applied on each newly created session.
        /// </summary>
        /// <remarks>
        /// This property defaults to <see cref="FlushMode.Never"/> to ensure that modifying objects outside the boundaries 
        /// of a transaction will not be persisted. It is recommended to not change this value but wrap any modifying operation
        /// within a transaction.
        /// </remarks>
        public FlushMode DefaultFlushMode
        {
            get { return settings.DefaultFlushMode; }
        }

        /// <summary>
        /// Get or set the configured SessionFactory
        /// </summary>
        public ISessionFactory SessionFactory
        {
            get
            {
                return settings.SessionFactory;
            }
        }

        /// <summary>
        /// Get or set the configured EntityInterceptor
        /// </summary>
        public IInterceptor EntityInterceptor
        {
            get
            {
                return settings.EntityInterceptor;
            }
        }

        /// <summary>
        /// Gets a flag, whether this scope is in "open" state on the current logical thread.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return (null != LogicalThreadContext.GetData(ISOPEN_KEY));
            }
        }

        /// <summary>
        /// Gets a flag, whether this scope manages it's own session for the current logical thread or not.
        /// </summary>
        public bool IsParticipating
        {
            get
            {
                return (null != LogicalThreadContext.GetData(PARTICIPATE_KEY));
            }
        }

        /// <summary>
        /// Sets a flag, whether this scope is in "open" state on the current logical thread.
        /// </summary>
        private void SetOpen(bool isOpen)
        {
            if (isOpen)
            {
                LogicalThreadContext.SetData(ISOPEN_KEY, ISOPEN_KEY);
            }
            else
            {
                LogicalThreadContext.FreeNamedDataSlot(ISOPEN_KEY);
            }
        }

        /// <summary>
        /// Gets/Sets a flag, whether this scope manages it's own session for the current logical thread or not.
        /// </summary>
        /// <value><c>false</c> if session is managed by this module. <c>false</c> otherwise</value>
        private void SetParticipating(bool participating)
        {
            if (participating)
            {
                LogicalThreadContext.SetData(PARTICIPATE_KEY, PARTICIPATE_KEY);
            }
            else
            {
                LogicalThreadContext.FreeNamedDataSlot(PARTICIPATE_KEY);
            }
        }

        /// <summary>
        /// Call <code>Close()</code>, 
        /// </summary>
        public virtual void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Opens a new session or participates in an existing session and 
        /// registers with spring's <see cref="TransactionSynchronizationManager"/>.
        /// </summary>
        public void Open()
        {
            if (IsParticipating || IsOpen)
            {
                throw new InvalidOperationException("This scope is already open");
            }

            bool isDebugEnabled = log.IsDebugEnabled;

            if (SingleSession)
            {
                // single session mode
                if (TransactionSynchronizationManager.HasResource(SessionFactory))
                {
                    // Do not modify the Session: just set the participate flag.
                    if (isDebugEnabled) log.Debug("Participating in existing Hibernate SessionFactory");
                    SetParticipating(true);
                }
                else
                {
                    if (isDebugEnabled) log.Debug("Opening single Hibernate Session in SessionScope");
                    TransactionSynchronizationManager.BindResource(SessionFactory, new LazySessionHolder(this));
                }
            }
            else
            {
                // deferred close mode
                if (SessionFactoryUtils.IsDeferredCloseActive(SessionFactory))
                {
                    // Do not modify deferred close: just set the participate flag.
                    if (isDebugEnabled) log.Debug("Participating in active deferred close mode");
                    SetParticipating(true);
                }
                else
                {
                    if (isDebugEnabled) log.Debug("Initializing deferred close mode");
                    SessionFactoryUtils.InitDeferredClose(SessionFactory);
                }
            }

            SetOpen(true);
        }

        /// <summary>
        /// Close the current view's session and unregisters 
        /// from spring's <see cref="TransactionSynchronizationManager"/>.
        /// </summary>
        public void Close()
        {
            bool isDebugEnabled = log.IsDebugEnabled;
            if (isDebugEnabled) log.Debug("Trying to close SessionScope");

            if (IsOpen)
            {
                try
                {
                    DoClose(isDebugEnabled);
                }
                finally
                {
                    SetOpen(false);
                    SetParticipating(false);                    
                }
            }
            else
            {
                if (isDebugEnabled) log.Debug("SessionScope is already closed - doing nothing");
            }
        }

        private void DoClose(bool isLogDebugEnabled)
        {
            if (!IsParticipating)
            {
                if (SingleSession)
                {
                    // single session mode
                    if (isLogDebugEnabled) log.Debug("Closing single Hibernate Session in SessionScope");
                    LazySessionHolder holder = (LazySessionHolder)TransactionSynchronizationManager.UnbindResource(SessionFactory);
                    holder.Close();
                }
                else
                {
                    // deferred close mode
                    if (isLogDebugEnabled) log.Debug("Closing all Hibernate Sessions");
                    SessionFactoryUtils.ProcessDeferredClose(SessionFactory);
                }
            }
            else
            {
                if (isLogDebugEnabled) log.Debug("Only participated Hibernate Session - doing nothing");
            }
        }

        private ISession DoOpenSession()
        {
            ISession session = SessionFactoryUtils.OpenSession(SessionFactory, EntityInterceptor);
            session.FlushMode = DefaultFlushMode;
            return session;
        }

        /// <summary>
        /// This sessionHolder creates a default session only if it is needed.
        /// </summary>
        /// <remarks>
        /// Although a NHibernateSession deferes creation of db-connections until they are really
        /// needed, instantiation a session is imho still more expensive than this LazySessionHolder. (EE)
        /// </remarks>
        private class LazySessionHolder : SessionHolder
        {
            private readonly ILog log = LogManager.GetLogger(typeof(LazySessionHolder));
            private SessionScope owner;
            private ISession session;

            /// <summary>
            /// Initialize a new instance.
            /// </summary>
            public LazySessionHolder(SessionScope owner)
            {
                if (log.IsDebugEnabled) log.Debug("Created LazySessionHolder");
                this.owner = owner;
            }

            /// <summary>
            /// Create a new session on demand
            /// </summary>
            protected override void EnsureInitialized()
            {
                if (session == null)
                {
                    if (log.IsDebugEnabled) log.Debug("session instance requested - opening new session");
                    session = owner.DoOpenSession();
                    AddSession(session);
                }
            }

            /// <summary>
            /// Ensure session is closed (if any) and remove circular references to avoid memory leaks!
            /// </summary>
            public void Close()
            {
                owner = null;
                if (session != null)
                {
                    ISession tmpSession = session;
                    session = null;
                    SessionFactoryUtils.CloseSession(tmpSession);
                }
                if (log.IsDebugEnabled) log.Debug("Closed LazySessionHolder");
            }
        }
    }
}
