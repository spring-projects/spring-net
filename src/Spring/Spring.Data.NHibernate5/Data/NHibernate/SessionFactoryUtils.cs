/*
 * Copyright © 2002-2011 the original author or authors.
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
using Common.Logging;
using NHibernate;
using NHibernate.Connection;
using NHibernate.Driver;
using NHibernate.Engine;
using Spring.Collections;
using Spring.Context;
using Spring.Dao;
using Spring.Data.Common;
using Spring.Data.Support;
using Spring.Objects.Factory.Config;
using Spring.Threading;
using Spring.Transaction.Support;
using Spring.Util;

namespace Spring.Data.NHibernate
{
	/// <summary>
	///  Helper class featuring methods for Hibernate Session handling,
    ///  allowing for reuse of Hibernate Session instances within transactions.
    ///  Also provides support for exception translation.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public abstract class SessionFactoryUtils 
	{
	    /// <summary>
        /// The <see cref="ILog"/> instance for this class. 
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(SessionFactoryUtils));

	    /// <summary>
        /// The ordering value for synchronizaiton this session resources.
        /// Set to be lower than ADO.NET synchronization.
        /// </summary>
        public static readonly int SESSION_SYNCHRONIZATION_ORDER =
            AdoUtils.CONNECTION_SYNCHRONIZATION_ORDER - 100;

        private static readonly string DeferredCloseHolderDataSlotName = "Spring.Data.NHibernate:deferredCloseHolder";

	    /// <summary>
		/// Initializes a new instance of the <see cref="SessionFactoryUtils"/> class.
        /// </summary>
		public SessionFactoryUtils()
		{

		}

	    /// <summary>
        /// Get a new Hibernate Session from the given SessionFactory.
        /// Will return a new Session even if there already is a pre-bound
        /// Session for the given SessionFactory.
        /// </summary>
        /// <remarks>
        /// Within a transaction, this method will create a new Session
        /// that shares the transaction's ADO.NET Connection. More specifically,
        /// it will use the same ADO.NET Connection as the pre-bound Hibernate Session.
        /// </remarks>
        /// <param name="sessionFactory">The session factory to create the session with.</param>
        /// <param name="interceptor">The Hibernate entity interceptor, or <code>null</code> if none.</param>
        /// <returns>The new session.</returns>
        /// <exception cref="DataAccessResourceFailureException">If could not open Hibernate session</exception>
	    public static ISession GetNewSession(ISessionFactory sessionFactory, IInterceptor interceptor)
	    {
            try 
            {
                SessionHolder sessionHolder = (SessionHolder) TransactionSynchronizationManager.GetResource(sessionFactory);
                if (sessionHolder != null && !sessionHolder.IsEmpty) 
                {
                    if (interceptor != null) 
                    {
                        return sessionFactory.OpenSession(sessionHolder.AnySession.Connection, interceptor);
                    }
                    else 
                    {
                        return sessionFactory.OpenSession(sessionHolder.AnySession.Connection);
                    }
                }
                else 
                {
                    if (interceptor != null) 
                    {
                        return sessionFactory.OpenSession(interceptor);
                    }
                    else 
                    {
                        return sessionFactory.OpenSession();
                    }
                }
            }
            catch (HibernateException ex) 
            {
                throw new DataAccessResourceFailureException("Could not open Hibernate Session", ex);
            }
	    }

        /// <summary>
        /// Get a Hibernate Session for the given SessionFactory. Is aware of and will
        /// return any existing corresponding Session bound to the current thread, for
        /// example when using HibernateTransactionManager. Will always create a new
        /// Session otherwise.
        /// </summary>
        /// <remarks>
        /// Supports setting a Session-level Hibernate entity interceptor that allows
        /// to inspect and change property values before writing to and reading from the
        /// database. Such an interceptor can also be set at the SessionFactory level
        /// (i.e. on LocalSessionFactoryObject), on HibernateTransactionManager, or on
        /// HibernateInterceptor/HibernateTemplate.
        /// </remarks>
        /// <param name="sessionFactory">The session factory to create the
        /// session with.</param>
        /// <param name="entityInterceptor">Hibernate entity interceptor, or <code>null</code> if none.</param>
        /// <param name="adoExceptionTranslator"> AdoExceptionTranslator to use for flushing the
        /// Session on transaction synchronization (can be <code>null</code>; only used when actually
        /// registering a transaction synchronization).</param>
        /// <returns>The Hibernate Session</returns>
        /// <exception cref="DataAccessResourceFailureException">
        /// If the session couldn't be created.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If no thread-bound Session found and allowCreate is false.
        /// </exception>
        public static ISession GetSession(
            ISessionFactory sessionFactory, IInterceptor entityInterceptor,
            IAdoExceptionTranslator adoExceptionTranslator)
        {
            try
            {
                return GetSession(sessionFactory, entityInterceptor, adoExceptionTranslator, true);
            }
            catch (HibernateException ex)
            {
                throw new DataAccessResourceFailureException("Could not open Hibernate Session", ex);
            }
        }   

        /// <summary>
        /// Get a Hibernate Session for the given SessionFactory. Is aware of and will
        /// return any existing corresponding Session bound to the current thread, for
        /// example when using <see cref="HibernateTransactionManager"/>. Will create a new Session
        /// otherwise, if allowCreate is true.
        /// </summary>
        /// <param name="sessionFactory">The session factory to create the session with.</param>
        /// <param name="allowCreate">if set to <c>true</c> create a non-transactional Session when no
        /// transactional Session can be found for the current thread.</param>
        /// <returns>The hibernate session</returns>
        /// <exception cref="DataAccessResourceFailureException">
        /// If the session couldn't be created.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If no thread-bound Session found and allowCreate is false.
        /// </exception>
        public static ISession GetSession(ISessionFactory sessionFactory, bool allowCreate)
        {
            try
            {
                return GetSession(sessionFactory, null, null, allowCreate);
            }
            catch (HibernateException ex)
            {
                throw new DataAccessResourceFailureException("Could not open Hibernate Session", ex);
            }
        }

        /// <summary>
        /// Get a Hibernate Session for the given SessionFactory.
        /// </summary>
        /// <remarks>Is aware of and will return any existing corresponding
        /// Session bound to the current thread, for example whenusing
        /// <see cref="HibernateTransactionManager"/>.  Will create a new 
        /// Session otherwise, if "allowCreate" is true.
        /// <p>Throws the orginal HibernateException, in contrast to
        /// <see cref="GetSession(ISessionFactory, bool)"/>.
        /// </p></remarks>
        /// <param name="sessionFactory">The session factory.</param>
        /// <param name="allowCreate">if set to <c>true</c> [allow create].</param>
        /// <returns>The Hibernate Session</returns>
        /// <exception cref="HibernateException">
        /// if the Session couldn't be created
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If no thread-bound Session found and allowCreate is false.
        /// </exception>
        public static ISession DoGetSession(ISessionFactory sessionFactory, bool allowCreate)
        {
            return DoGetSession(sessionFactory, null, null, allowCreate);
        }

        private static ISession GetSession(
            ISessionFactory sessionFactory, IInterceptor entityInterceptor,
            IAdoExceptionTranslator adoExceptionTranslator, bool allowCreate)
        {
            try
            {
                return DoGetSession(sessionFactory, entityInterceptor, adoExceptionTranslator, allowCreate);
            }
            catch (HibernateException ex)
            {
                throw new DataAccessResourceFailureException("Could not open Hibernate Session", ex);
            }
        }


        private static ISession DoGetSession(
            ISessionFactory sessionFactory, IInterceptor entityInterceptor,
            IAdoExceptionTranslator adoExceptionTranslator, bool allowCreate)
        {
            AssertUtils.ArgumentNotNull(sessionFactory, "sessionFactory", "SessionFactory can not be null");

            SessionHolder sessionHolder = (SessionHolder) TransactionSynchronizationManager.GetResource(sessionFactory);
            if (sessionHolder != null && !sessionHolder.IsEmpty) 
            {
                // pre-bound Hibernate Session
                ISession session = null;
                if (TransactionSynchronizationManager.SynchronizationActive &&
                    sessionHolder.DoesNotHoldNonDefaultSession)
                {
                    // Spring transaction management is active ->
                    // register pre-bound Session with it for transactional flushing.
                    session = sessionHolder.ValidatedSession;
                    if (session != null && !sessionHolder.SynchronizedWithTransaction) 
                    {
                        log.Debug("Registering Spring transaction synchronization for existing Hibernate Session");
                        TransactionSynchronizationManager.RegisterSynchronization(
                            new SpringSessionSynchronization(sessionHolder, sessionFactory, adoExceptionTranslator, false));
                        sessionHolder.SynchronizedWithTransaction = true;
                        // Switch to FlushMode.AUTO if we're not within a read-only transaction.
                        FlushMode flushMode = session.FlushMode;
                        if (FlushMode.Never == flushMode &&
                            !TransactionSynchronizationManager.CurrentTransactionReadOnly) 
                        {
                            session.FlushMode = FlushMode.Auto;
                            sessionHolder.PreviousFlushMode = flushMode;
                        }
                    }
                }
                else
                {
                   // No Spring transaction management active -> simply return default thread-bound Session, if any
                   // (possibly from OpenSessionInViewModule)
                    session = sessionHolder.ValidatedSession;
                }
                
                if (session != null) 
                {
                    return session;
                }
                
            }


            ISession sess = OpenSession(sessionFactory, entityInterceptor);
            // Set Session to FlushMode.Never if we're within a read-only transaction.
            // Use same Session for further Hibernate actions within the transaction.
            // Thread object will get removed by synchronization at transaction completion.
            if (TransactionSynchronizationManager.SynchronizationActive) 
            {
                log.Debug("Registering Spring transaction synchronization for new Hibernate Session");
                SessionHolder holderToUse = sessionHolder;
                if (holderToUse == null) 
                {
                    holderToUse = new SessionHolder(sess);
                }
                else 
                {
                    holderToUse.AddSession(sess);
                }
                if (TransactionSynchronizationManager.CurrentTransactionReadOnly) 
                {
                    sess.FlushMode = FlushMode.Never;
                }
                TransactionSynchronizationManager.RegisterSynchronization(
                    new SpringSessionSynchronization(holderToUse, sessionFactory, adoExceptionTranslator, true));
                holderToUse.SynchronizedWithTransaction = true;
                if (holderToUse != sessionHolder) 
                {
                    TransactionSynchronizationManager.BindResource(sessionFactory, holderToUse);
                }
            }

            

            // Check whether we are allowed to return the Session.
            if (!allowCreate && !IsSessionTransactional(sess, sessionFactory)) 
            {
                CloseSession(sess);
                throw new InvalidOperationException ("No Hibernate Session bound to thread, " +
                    "and configuration does not allow creation of non-transactional one here");
            }

            return sess;


        }

	    /// <summary>
	    /// Open a new Session from the factory.
	    /// </summary>
        /// <param name="sessionFactory">The session factory to create the session with.</param>
        /// <param name="entityInterceptor">Hibernate entity interceptor, or <code>null</code> if none.</param>
        /// <returns>the newly opened session</returns>
        internal static ISession OpenSession(ISessionFactory sessionFactory, IInterceptor entityInterceptor)
        {
            log.Debug("Opening Hibernate Session");
            ISession session = (
                                   (entityInterceptor != null)
                                   ? sessionFactory.OpenSession(entityInterceptor) 
                                   : sessionFactory.OpenSession()
                               );

            return session;
        }	         
   
        /// <summary>
        /// Perform the actual closing of the Hibernate Session
        /// catching and logging any cleanup exceptions thrown.
        /// </summary>
        /// <param name="session">The hibernate session to close</param>
        public static void CloseSession(ISession session) 
        {
            if (session != null) 
            {
                log.Debug("Closing Hibernate Session");
                try 
                {
                    session.Close();
                }
                catch (HibernateException ex) 
                {
                    log.Error("Could not close Hibernate Session", ex);
                }
                catch (Exception ex) 
                {
                    log.Error("Unexpected exception on closing Hibernate Session", ex);
                }
            }
        }

        /// <summary>
        /// Return whether the given Hibernate Session is transactional, that is,
        /// bound to the current thread by Spring's transaction facilities.
        /// </summary>
        /// <param name="session">The hibernate session to check</param>
        /// <param name="sessionFactory">The session factory that the session
        /// was created with, can be null.</param>
        /// <returns>
        /// 	<c>true</c> if the session transactional; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSessionTransactional(ISession session, ISessionFactory sessionFactory) 
        {
            if (sessionFactory == null) 
            {
                return false;
            }
            SessionHolder sessionHolder =
                (SessionHolder) TransactionSynchronizationManager.GetResource(sessionFactory);
            return (sessionHolder != null && sessionHolder.ContainsSession(session));
        }

	    /// <summary>
        /// Converts a Hibernate ADOException to a Spring DataAccessExcption, extracting the underlying error code from 
        /// ADO.NET.  Will extract the ADOException Message and SqlString properties and pass them to the translate method
        /// of the provided IAdoExceptionTranslator.
        /// </summary>
        /// <param name="translator">The IAdoExceptionTranslator, may be a user provided implementation as configured on
        /// HibernateTemplate.
        /// </param>
        /// <param name="ex">The ADOException throw</param>
        /// <returns>The translated DataAccessException or UncategorizedAdoException in case of an error in translation
        /// itself.</returns>
        public static DataAccessException ConvertAdoAccessException(IAdoExceptionTranslator translator, ADOException ex)
        {
            try
            {
                string sqlString = (ex.SqlString != null)
                                       ? ex.SqlString.ToString()
                                       : string.Empty;
                return translator.Translate(
                    "Hibernate operation: " + ex.Message, sqlString, ex.InnerException);
            } catch (Exception e)
            {
                log.Error("Exception thrown during exception translation. Message = [" + e.Message + "]", e);
                log.Error("Exception that was attempted to be translated was [" + ex.Message + "]", ex);                
                if (ex.InnerException != null)
                {
                    log.Error("  Inner Exception was [" + ex.InnerException.Message + "]", ex.InnerException);
                }
                throw new UncategorizedAdoException(e.Message, "", "", e);
            }
        }

        /// <summary>
        /// Convert the given HibernateException to an appropriate exception from the
        /// <code>Spring.Dao</code> hierarchy. Note that it is advisable to
        /// handle AdoException specifically by using a AdoExceptionTranslator for the
        /// underlying ADO.NET exception.
        /// </summary>
        /// <param name="ex">The Hibernate exception that occured.</param>
        /// <returns>DataAccessException instance</returns>
        public static DataAccessException ConvertHibernateAccessException(HibernateException ex)
        {
            if (ex is ADOException)
            {
                // ADOException during Hibernate access: only passed in here from custom code,
                // as HibernateTemplate etc will use AdoExceptionTranslator-based handling.
                return new HibernateAdoException("Ado Exception", (ADOException) ex);
            }
            if (ex is UnresolvableObjectException)
            {
                return new HibernateObjectRetrievalFailureException((UnresolvableObjectException) ex);
            }
            if (ex is ObjectDeletedException)
            {
                return new InvalidDataAccessApiUsageException(ex.Message, ex);
            }
            if (ex is WrongClassException)
            {
                return new HibernateObjectRetrievalFailureException((WrongClassException) ex);
            }
            if (ex is StaleObjectStateException)
            {
                return new HibernateOptimisticLockingFailureException((StaleObjectStateException) ex);
            }
            if (ex is StaleStateException)
            {
                return new HibernateOptimisticLockingFailureException((StaleStateException)ex);
            }
            if (ex is QueryException)
            {
                return new HibernateQueryException((QueryException) ex);
            }

            if (ex is PersistentObjectException)
            {
                return new InvalidDataAccessApiUsageException(ex.Message, ex);
            }
            if (ex is TransientObjectException)
            {
                return new InvalidDataAccessApiUsageException(ex.Message, ex);
            }

            if (ex is PropertyValueException)
            {
                return new DataIntegrityViolationException(ex.Message, ex);
            }
            if (ex is PersistentObjectException)
            {
                return new InvalidDataAccessApiUsageException(ex.Message, ex);
            }
            if (ex is NonUniqueResultException)
            {
                return new IncorrectResultSizeDataAccessException(ex.Message, 1);
            }
            // fallback
            return new HibernateSystemException(ex);
        }

        /// <summary>
        /// Close the given Session, created via the given factory,
        /// if it is not managed externally (i.e. not bound to the thread).
        /// </summary>
        /// <param name="session">The hibernate session to close</param>
        /// <param name="sessionFactory">The hibernate SessionFactory that
        /// the session was created with.</param>
	    public static void ReleaseSession(ISession session, ISessionFactory sessionFactory)
	    {
            if (session == null) 
            {
                return;
            }
            // Only close non-transactional Sessions.
            if (!IsSessionTransactional(session, sessionFactory)) 
            {
                CloseSessionOrRegisterDeferredClose(session, sessionFactory);
            }
	    }

        /// <summary>
        /// Close the given Session or register it for deferred close.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="sessionFactory">The session factory.</param>
	    internal static void CloseSessionOrRegisterDeferredClose(ISession session, ISessionFactory sessionFactory)
	    {
            IDictionary holderDictionary = LogicalThreadContext.GetData(DeferredCloseHolderDataSlotName) as IDictionary;
            
            if (holderDictionary != null && sessionFactory != null && holderDictionary.Contains(sessionFactory)) 
            {
                log.Debug("Registering Hibernate Session for deferred close");
                // Switch Session to FlushMode.NEVER for remaining lifetime.
                session.FlushMode = FlushMode.Never;
                Set sessions = (Set) holderDictionary[sessionFactory];
                sessions.Add(session);
            }
            else 
            {
                CloseSession(session);
            }
	    }

        /// <summary>
	    ///Initialize deferred close for the current thread and the given SessionFactory.
	    /// Sessions will not be actually closed on close calls then, but rather at a
	    /// processDeferredClose call at a finishing point (like request completion).
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        public static void InitDeferredClose(ISessionFactory sessionFactory) 
        {
            AssertUtils.ArgumentNotNull(sessionFactory, "No SessionFactory specified");

            log.Debug("Initializing deferred close of Hibernate Sessions");

            IDictionary holderDictionary = LogicalThreadContext.GetData(DeferredCloseHolderDataSlotName) as IDictionary;
          
            if (holderDictionary == null) 
            {
                holderDictionary = new Hashtable();
                LogicalThreadContext.SetData(DeferredCloseHolderDataSlotName, holderDictionary);
            }
            holderDictionary.Add(sessionFactory, new ListSet());
        }

        /// <summary>
        /// Return if deferred close is active for the current thread
        /// and the given SessionFactory.</summary>
        /// <param name="sessionFactory">The session factory.</param>
        /// <returns>
        /// 	<c>true</c> if [is deferred close active] [the specified session factory]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If SessionFactory argument is null.</exception>
        public static bool IsDeferredCloseActive(ISessionFactory sessionFactory) 
        {
            if (sessionFactory == null)
            {
                throw new ArgumentNullException("sessionFactory", "No SessionFactory specified");
            }
            IDictionary holderDictionary = LogicalThreadContext.GetData(DeferredCloseHolderDataSlotName) as IDictionary;          
            return (holderDictionary != null && holderDictionary.Contains(sessionFactory));
        }

        /// <summary>
        /// Process Sessions that have been registered for deferred close
        /// for the given SessionFactory.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        /// <exception cref="InvalidOperationException">If there is no session factory associated with the thread.</exception>
        public static void ProcessDeferredClose(ISessionFactory sessionFactory) 
        {
            AssertUtils.ArgumentNotNull(sessionFactory, "No SessionFactory specified");

            IDictionary holderDictionary = LogicalThreadContext.GetData(DeferredCloseHolderDataSlotName) as IDictionary;
          
            if (holderDictionary == null || !holderDictionary.Contains(sessionFactory)) 
            {
                throw new InvalidOperationException("Deferred close not active for SessionFactory [" + sessionFactory + "]");
            }
            log.Debug("Processing deferred close of Hibernate Sessions");
            Set sessions = (Set) holderDictionary[sessionFactory];
            holderDictionary.Remove(sessionFactory);
            foreach (ISession session in sessions)
            {
                CloseSession(session);
            }

            if (holderDictionary.Count == 0)
            {
                LogicalThreadContext.FreeNamedDataSlot(DeferredCloseHolderDataSlotName);
            }


        }

        /// <summary>
        /// Applies the current transaction timeout, if any, to the given
        /// criteria object
        /// </summary>
        /// <param name="criteria">The Hibernate Criteria object.</param>
        /// <param name="sessionFactory">Hibernate SessionFactory that the Criteria was created for
        /// (can be <code>null</code>).</param>
        /// <exception cref="ArgumentNullException">If criteria argument is null.</exception>
        public static void ApplyTransactionTimeout(ICriteria criteria, ISessionFactory sessionFactory) 
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "No Criteria object specified");
            }

            SessionHolder sessionHolder =
                (SessionHolder) TransactionSynchronizationManager.GetResource(sessionFactory);
            if (sessionHolder != null && sessionHolder.HasTimeout) 
            {
                criteria.SetTimeout(sessionHolder.TimeToLiveInSeconds);
            }
        }

        /// <summary>
        /// Applies the current transaction timeout, if any, to the given
        /// Hibenrate query object.
        /// </summary>
        /// <param name="query">The Hibernate Query object.</param>
        /// <param name="sessionFactory">Hibernate SessionFactory that the Query was created for
        /// (can be <code>null</code>).</param>
        /// <exception cref="ArgumentNullException">If query argument is null.</exception>
	    public static void ApplyTransactionTimeout(IQuery query, ISessionFactory sessionFactory)
	    {	        
            if (query == null)
            {
                throw new ArgumentNullException("queryObject", "No query object specified");
            }
            if (sessionFactory != null) 
            {
                SessionHolder sessionHolder =
                    (SessionHolder) TransactionSynchronizationManager.GetResource(sessionFactory);
                if (sessionHolder != null && sessionHolder.HasTimeout) 
                {
                    query.SetTimeout(sessionHolder.TimeToLiveInSeconds);
                }
            }
	    }
        /// <summary>
        /// Gets the Spring IDbProvider given the ISessionFactory.
        /// </summary>
        /// <remarks>The matching is performed by comparing the assembly qualified
        /// name string of the hibernate Driver.ConnectionType to those in
        /// the DbProviderFactory definitions.  No connections are created
        /// in performing this comparison.</remarks>
        /// <param name="sessionFactory">The session factory.</param>
        /// <returns>The corresponding IDbProvider, null if no mapping was found.</returns>
        /// <exception cref="InvalidOperationException">If DbProviderFactory's ApplicaitonContext is not
        /// an instance of IConfigurableApplicaitonContext.</exception>
	    public static IDbProvider GetDbProvider(ISessionFactory sessionFactory)
	    {
	        ISessionFactoryImplementor sfi = sessionFactory as ISessionFactoryImplementor;
	        if (sfi != null)
	        {

                IConnectionProvider cp = sfi.ConnectionProvider;
	            if (cp != null)
	            {
                    IConfigurableApplicationContext ctx =
	                    DbProviderFactory.ApplicationContext as IConfigurableApplicationContext;
                    if (ctx == null)
                    {
                        throw new InvalidOperationException(
                            "Implementations of IApplicationContext must also implement IConfigurableApplicationContext");
                    }


                    DriverBase db = cp.Driver as DriverBase;
                    if (db != null)
                    {
                        Type hibCommandType = db.CreateCommand().GetType();

                        var providerNames = ctx.GetObjectNamesForType(typeof(DbProvider), true, false);
                        string hibCommandAQN = hibCommandType.AssemblyQualifiedName;
                        string hibCommandAQNWithoutVersion = hibCommandType.FullName + ", " + hibCommandType.Assembly.GetName().Name;
                        foreach (string providerName in providerNames)
                        {
                            IObjectDefinition objectdef = ctx.ObjectFactory.GetObjectDefinition(providerName);
                            ConstructorArgumentValues ctorArgs = objectdef.ConstructorArgumentValues;
                            ConstructorArgumentValues.ValueHolder vh = ctorArgs.NamedArgumentValues["dbmetadata"] as ConstructorArgumentValues.ValueHolder;
                            IObjectDefinition od = ((ObjectDefinitionHolder)vh.Value).ObjectDefinition;
                            ConstructorArgumentValues dbmdCtorArgs = od.ConstructorArgumentValues;
                            string commandType = dbmdCtorArgs.GetArgumentValue("commandType", typeof(string)).Value as string;
                            
                            if (hibCommandAQN.Equals(commandType) || hibCommandAQNWithoutVersion.Equals(commandType))
                            {
                                IDbProvider prov = DbProviderFactory.GetDbProvider(providerName);
                                return prov;
                            }
                        }
                    }
	                else
                    {
                        log.Info("Could not derive IDbProvider from SessionFactory");
                    }
	            }
	            
	            
	        }
	        return null;
	    }

        /// <summary>
        /// Create a IAdoExceptionTranslator from the given SessionFactory.
        /// </summary>
        /// <remarks>If a corresponding IDbProvider is found, a ErrorcodeExceptionTranslator
        /// for the IDbProvider is created.  Otherwise, a FallbackException is created.</remarks>
        /// <param name="sessionFactory">The session factory to create the translator for</param>
        /// <returns>An IAdoExceptionTranslator</returns>
	    public static IAdoExceptionTranslator NewAdoExceptionTranslator(ISessionFactory sessionFactory)
	    {
            IDbProvider dbProvider = GetDbProvider(sessionFactory);
	        if (dbProvider != null)
	        {
                return new ErrorCodeExceptionTranslator(dbProvider);
	        }
	        log.Warn("Using FallbackException Translator.  Could not translate from ISessionFactory to IDbProvider");
            return new FallbackExceptionTranslator();
	        
	    }
	}
}
