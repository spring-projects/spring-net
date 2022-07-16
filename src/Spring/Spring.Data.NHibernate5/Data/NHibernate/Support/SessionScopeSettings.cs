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

using NHibernate;
using Spring.Util;

namespace Spring.Data.NHibernate.Support
{
    /// <summary>
    /// Holds the references and configuration settings for a <see cref="SessionScope"/> instance.
    /// </summary>
    public class SessionScopeSettings
    {
		/// <summary>
		/// Default value for <see cref="SingleSession"/> property.
		/// </summary>
        public static readonly bool SINGLESESSION_DEFAULT = true;
		/// <summary>
		/// Default value for <see cref="DefaultFlushMode"/> property.
		/// </summary>
		public static readonly FlushMode FLUSHMODE_DEFAULT = FlushMode.Never;

        private ISessionFactory sessionFactory;
        private bool sessionFactoryInitialized;
        private IInterceptor entityInterceptor;
        private bool entityInterceptorInitialized;
        private bool singleSession;
        private FlushMode defaultFlushMode;

        /// <summary>
        /// Initialize a new instance of <see cref="SessionScopeSettings"/> with default values.
        /// </summary>
        /// <remarks>
        /// Calling this constructor from your derived class leaves <see cref="SessionFactory"/> and <see cref="EntityInterceptor"/>
        /// uninitialized. See <see cref="ResolveSessionFactory"/> and <see cref="ResolveEntityInterceptor"/> for more.
        /// </remarks>
        protected SessionScopeSettings()
        {
            this.sessionFactory = null;
            this.sessionFactoryInitialized = false;
            this.entityInterceptor = null;
            this.entityInterceptorInitialized = false;
            this.singleSession = SINGLESESSION_DEFAULT;
            this.defaultFlushMode = FLUSHMODE_DEFAULT;
        }

        /// <summary>
        /// Initialize a new instance of <see cref="SessionScopeSettings"/> with the given sessionFactory
        /// and default values for all other settings.
        /// </summary>
        /// <param name="sessionFactory">
        /// The <see cref="ISessionFactory"/> instance to be used for obtaining <see cref="ISession"/> instances.
        /// </param>
        /// <remarks>
        /// Calling this constructor marks all properties initialized.
        /// </remarks>
        public SessionScopeSettings(ISessionFactory sessionFactory)
            :this(sessionFactory, null, SINGLESESSION_DEFAULT, FLUSHMODE_DEFAULT)
        {
            // noop
            this.entityInterceptorInitialized = false;
        }

        /// <summary>
        /// Initialize a new instance of <see cref="SessionScopeSettings"/> with the given values and references.
        /// </summary>
        /// <param name="sessionFactory">
        /// The <see cref="ISessionFactory"/> instance to be used for obtaining <see cref="ISession"/> instances.
        /// </param>
        /// <param name="entityInterceptor">
        /// Specify the <see cref="IInterceptor"/> to be set on each session provided by the <see cref="SessionScope"/> instance.
        /// </param>
        /// <param name="singleSession">
        /// Set whether to use a single session for each request. See <see cref="SingleSession"/> property for details.
        /// </param>
        /// <param name="defaultFlushMode">
        /// Specify the flushmode to be applied on each session provided by the <see cref="SessionScope"/> instance.
        /// </param>
        /// <remarks>
        /// Calling this constructor marks all properties initialized.
        /// </remarks>
        public SessionScopeSettings(ISessionFactory sessionFactory, IInterceptor entityInterceptor, bool singleSession, FlushMode defaultFlushMode)
        {
            AssertUtils.ArgumentNotNull(sessionFactory, "sessionFactory");

            this.sessionFactory = sessionFactory;
            this.sessionFactoryInitialized = true;
            this.entityInterceptor = entityInterceptor;
            this.entityInterceptorInitialized = true;
            this.singleSession = singleSession;
            this.defaultFlushMode = defaultFlushMode;
        }

        /// <summary>
        /// Gets the configured <see cref="IInterceptor"/> instance to be used.
        /// </summary>
        /// <remarks>
        /// If the entity interceptor is not set by the constructor, this property calls
        /// <see cref="ResolveEntityInterceptor"/> to obtain an instance. This allows derived classes to
        /// override the behaviour of how to obtain the concrete <see cref="IInterceptor"/> instance.
        /// </remarks>
        public IInterceptor EntityInterceptor
        {
            get
            {
                if (!entityInterceptorInitialized)
                {
                    return ResolveEntityInterceptor();
                }
                return entityInterceptor;
            }
        }

        /// <summary>
        /// Gets the configured <see cref="ISessionFactory"/> instance to be used.
        /// </summary>
        /// <remarks>
        /// If this property is requested for the first time, <see cref="ResolveSessionFactory"/> is called.
        /// This allows derived classes to override the behaviour of how to obtain the concrete <see cref="ISessionFactory"/> instance.
        /// </remarks>
        /// <exception cref="ArgumentException">If the <see cref="ISessionFactory"/> instance cannot be resolved.</exception>
        public ISessionFactory SessionFactory
        {
            get
            {
                if (!sessionFactoryInitialized)
                {
                    sessionFactoryInitialized = true;
                    sessionFactory = ResolveSessionFactory();
                    if (sessionFactory == null)
                    {
                        throw new ArgumentException(string.Format("mandatory SessionFactory not found"));
                    }
                }
                return sessionFactory;
            }
        }

        ///<summary>
        /// Set whether to use a single session for each request. Default is "true".
        /// If set to false, each data access operation or transaction will use
        /// its own session (like without Open Session in View). Each of those
        /// sessions will be registered for deferred close, though, actually
        /// processed at request completion.
        /// </summary>
        public bool SingleSession
        {
            get { return singleSession; }
            set { singleSession = value; }
        }

        /// <summary>
        /// Gets or Sets the flushmode to be applied on each newly created session.
        /// </summary>
        /// <remarks>
        /// This property defaults to <see cref="FlushMode.Never"/> to ensure that modifying objects outside the boundaries
        /// of a transaction will not be persisted. It is recommended to not change this value but wrap any modifying operation
        /// within a transaction.
        /// </remarks>
        public FlushMode DefaultFlushMode
        {
            get { return defaultFlushMode; }
            set { defaultFlushMode = value; }
        }

        /// <summary>
        /// Override this method to resolve an <see cref="IInterceptor"/> instance according to your chosen strategy.
        /// </summary>
        protected virtual IInterceptor ResolveEntityInterceptor()
        {
            return null;
        }

        /// <summary>
        /// Override this method to resolve an <see cref="ISessionFactory"/> instance according to your chosen strategy.
        /// </summary>
        protected virtual ISessionFactory ResolveSessionFactory()
        {
            throw new NotSupportedException("you need to override this method to resolve an ISessionFactory instance");
        }
    }
}
