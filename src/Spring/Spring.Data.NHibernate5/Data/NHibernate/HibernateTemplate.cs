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

using Spring.Aop.Framework;
using Spring.Data.Common;
using Spring.Data.Support;

using IInterceptor = NHibernate.IInterceptor;

using NHibernate;
using NHibernate.Type;

using Spring.Dao;
using Spring.Objects.Factory;

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// Helper class that simplifies NHibernate data access code
    /// </summary>
    /// <remarks>
    /// <p>Typically used to implement data access or business logic services that
    /// use NHibernate within their implementation but are Hibernate-agnostic in their
    /// interface. The latter or code calling the latter only have to deal with
    /// domain objects.</p>
    ///
    /// <p>The central method is Execute supporting Hibernate access code
    /// implementing the HibernateCallback interface. It provides NHibernate Session
    /// handling such that neither the IHibernateCallback implementation nor the calling
    /// code needs to explicitly care about retrieving/closing NHibernate Sessions,
    /// or handling Session lifecycle exceptions. For typical single step actions,
    /// there are various convenience methods (Find, Load, SaveOrUpdate, Delete).
    /// </p>
    /// 
    /// <p>Can be used within a service implementation via direct instantiation
    /// with a ISessionFactory reference, or get prepared in an application context
    /// and given to services as an object reference. Note: The ISessionFactory should
    /// always be configured as an object in the application context, in the first case
    /// given to the service directly, in the second case to the prepared template.
    /// </p>
    /// 
    /// <p>This class can be considered as direct alternative to working with the raw
    /// Hibernate Session API (through SessionFactoryUtils.Session).
    /// </p>
    /// 
    /// <p>LocalSessionFactoryObject is the preferred way of obtaining a reference
    /// to a specific NHibernate ISessionFactory.
    /// </p>
    /// </remarks>
    /// <author>Mark Pollack (.NET)</author>
    public class HibernateTemplate : HibernateAccessor, IHibernateOperations
    {
        /// <summary>
        /// The <see cref="ILog"/> instance for this class. 
        /// </summary>
        private readonly ILog log = LogManager.GetLogger(typeof (HibernateTemplate));

        private bool checkWriteOperations = true;

        private bool exposeNativeSession = false;

        private bool alwaysUseNewSession = false;
        private int maxResults = 0;
        private TemplateFlushMode templateFlushMode = TemplateFlushMode.Auto;
        private bool allowCreate = true;
        private ISessionFactory sessionFactory;
        private object entityInterceptor;
        private IObjectFactory objectFactory;
        private bool cacheQueries = false;
        private string queryCacheRegion;
        private int fetchSize = 0;

        private IAdoExceptionTranslator adoExceptionTranslator;

        private readonly object syncRoot = new object();
        private ProxyFactory sessionProxyFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateTemplate"/> class.
        /// </summary>
        public HibernateTemplate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateTemplate"/> class.
        /// </summary>
        /// <remarks>The default for creating a new non-transactional
        /// session when no transactional Session can be found for the current thread
        /// is set to true.</remarks>
        /// <param name="sessionFactory">The session factory to create sessions.</param>
        public HibernateTemplate(ISessionFactory sessionFactory)
        {
            SessionFactory = sessionFactory;
            AfterPropertiesSet();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateTemplate"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory to create sessions.</param>        
        /// <param name="allowCreate">if set to <c>true</c> allow creation
        /// of a new non-transactional when no transactional Session can be found 
        /// for the current thread.</param>
        public HibernateTemplate(ISessionFactory sessionFactory, bool allowCreate)
        {
            SessionFactory = sessionFactory;
            AllowCreate = allowCreate;
            AfterPropertiesSet();
        }

        /// <summary>
        /// Gets or sets if a new Session should be created when no transactional Session
        /// can be found for the current thread.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if allowed to create non-transaction session;
        /// otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// 	<p>HibernateTemplate is aware of a corresponding Session bound to the
        /// current thread, for example when using HibernateTransactionManager.
        /// If allowCreate is true, a new non-transactional Session will be created
        /// if none found, which needs to be closed at the end of the operation.
        /// If false, an InvalidOperationException will get thrown in this case.
        /// </p>
        /// </remarks>
        public override bool AllowCreate
        {
            get { return allowCreate; }
            set { allowCreate = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to always
        /// use a new Hibernate Session for this template.
        /// </summary>
        /// <value><c>true</c> if always use new session; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// 	<p>
        /// Default is "false"; if activated, all operations on this template will
        /// work on a new NHibernate ISession even in case of a pre-bound ISession
        /// (for example, within a transaction).
        /// </p>
        /// 	<p>Within a transaction, a new NHibernate ISession used by this template
        /// will participate in the transaction through using the same ADO.NET
        /// Connection. In such a scenario, multiple Sessions will participate
        /// in the same database transaction.
        /// </p>
        /// 	<p>Turn this on for operations that are supposed to always execute
        /// independently, without side effects caused by a shared NHibernate ISession.
        /// </p>
        /// </remarks>
        public override bool AlwaysUseNewSession
        {
            get { return alwaysUseNewSession; }
            set { alwaysUseNewSession = value; }
        }

        /// <summary>
        /// Gets or sets the template flush mode.
        /// </summary>
        /// <remarks>
        /// Default is Auto. Will get applied to any <b>new</b> ISession
        /// created by the template.
        /// </remarks>
        /// <value>The template flush mode.</value>
        public override TemplateFlushMode TemplateFlushMode
        {
            get { return templateFlushMode; }
            set { templateFlushMode = value; }
        }

        /// <summary>
        /// Gets or sets the entity interceptor that allows to inspect and change
        /// property values before writing to and reading from the database.
        /// </summary>
        /// <remarks>
        /// Will get applied to any <b>new</b> ISession created by this object.
        /// <p>Such an interceptor can either be set at the ISessionFactory level,
        /// i.e. on LocalSessionFactoryObject, or at the ISession level, i.e. on
        /// HibernateTemplate, HibernateInterceptor, and HibernateTransactionManager.
        /// It's preferable to set it on LocalSessionFactoryObject or HibernateTransactionManager
        /// to avoid repeated configuration and guarantee consistent behavior in transactions.
        /// </p>
        /// </remarks>
        /// <value>The interceptor.</value>
        /// <exception cref="InvalidOperationException">If object factory is not set and need to retrieve entity interceptor by name.</exception>
        public override IInterceptor EntityInterceptor
        {
            get
            {
                if (this.entityInterceptor is string)
                {
                    if (this.objectFactory == null)
                    {
                        throw new InvalidOperationException("Cannot get entity interceptor via object name if no object factory set");
                    }
                    return (IInterceptor) this.objectFactory.GetObject((String) this.entityInterceptor, typeof (IInterceptor));
                }

                return (IInterceptor) entityInterceptor;
            }
            set { entityInterceptor = value; }
        }

        /// <summary>
        /// Gets or sets the name of the cache region for queries executed by this template.
        /// </summary>
        /// <remarks>
        /// If this is specified, it will be applied to all IQuery and ICriteria objects
        /// created by this template (including all queries through find methods).
        /// <p>The cache region will not take effect unless queries created by this
        /// template are configured to be cached via the CacheQueries property.
        /// </p>
        /// </remarks>
        /// <value>The query cache region.</value>
        public override string QueryCacheRegion
        {
            get { return queryCacheRegion; }
            set { queryCacheRegion = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to 
        /// cache all queries executed by this template.
        /// </summary>
        /// <remarks>
        /// If this is true, all IQuery and ICriteria objects created by
        /// this template will be marked as cacheable (including all
        /// queries through find methods).
        /// <p>To specify the query region to be used for queries cached
        /// by this template, set the QueryCacheRegion property.
        /// </p>
        /// </remarks>
        /// <value><c>true</c> if cache queries; otherwise, <c>false</c>.</value>
        public override bool CacheQueries
        {
            get { return cacheQueries; }
            set { cacheQueries = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of rows for this HibernateTemplate.
        /// </summary>
        /// <value>The max results.</value>
        /// <remarks>
        /// This is important
        /// for processing subsets of large result sets, avoiding to read and hold
        /// the entire result set in the database or in the ADO.NET driver if we're
        /// never interested in the entire result in the first place (for example,
        /// when performing searches that might return a large number of matches).
        /// <p>Default is 0, indicating to use the driver's default.</p>
        /// </remarks>
        public override int MaxResults
        {
            get { return maxResults; }
            set { maxResults = value; }
        }

        /// <summary>
        /// Set whether to expose the native Hibernate Session to IHibernateCallback
        /// code. Default is "false": a Session proxy will be returned,
        /// suppressing <code>close</code> calls and automatically applying
        /// query cache settings and transaction timeouts.
        /// </summary>
        /// <value><c>true</c> if expose native session; otherwise, <c>false</c>.</value>
        public override bool ExposeNativeSession
        {
            get { return exposeNativeSession; }
            set { exposeNativeSession = value; }
        }

        /// <summary>
        /// Gets or sets  whether to check that the Hibernate Session is not in read-only mode
        /// in case of write operations (save/update/delete).
        /// </summary>
        /// <value>
        /// 	<c>true</c> if check that the Hibernate Session is not in read-only mode
        /// in case of write operations; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Default is "true", for fail-fast behavior when attempting write operations
        /// within a read-only transaction. Turn this off to allow save/update/delete
        /// on a Session with flush mode NEVER.
        /// </remarks>
        public virtual bool CheckWriteOperations
        {
            get { return checkWriteOperations; }
            set { checkWriteOperations = value; }
        }

        /// <summary>
        /// Set the object name of a Hibernate entity interceptor that allows to inspect
        /// and change property values before writing to and reading from the database.
        /// </summary>
        /// <remarks>
        /// Will get applied to any new Session created by this transaction manager.
        /// <p>Requires the object factory to be known, to be able to resolve the object
        /// name to an interceptor instance on session creation. Typically used for
        /// prototype interceptors, i.e. a new interceptor instance per session.
        /// </p>
        /// <p>Can also be used for shared interceptor instances, but it is recommended
        /// to set the interceptor reference directly in such a scenario.
        /// </p>
        /// </remarks>
        /// <value>The name of the entity interceptor in the object factory/application context.</value>
        public override string EntityInterceptorObjectName
        {
            set { this.entityInterceptor = value; }
        }

        /// <summary>
        /// Set the object factory instance.
        /// </summary>
        /// <value>The object factory instance</value>
        public override IObjectFactory ObjectFactory
        {
            set { objectFactory = value; }
        }

        /// <summary>
        /// Gets or sets the session factory that should be used to create
        /// NHibernate ISessions.
        /// </summary>
        /// <value>The session factory.</value>
        public override ISessionFactory SessionFactory
        {
            get { return sessionFactory; }
            set { sessionFactory = value; }
        }

        /// <summary>
        /// Gets or sets the fetch size for this HibernateTemplate.
        /// </summary>
        /// <value>The size of the fetch.</value>
        /// <remarks>This is important for processing
        /// large result sets: Setting this higher than the default value will increase
        /// processing speed at the cost of memory consumption; setting this lower can
        /// avoid transferring row data that will never be read by the application.
        /// <p>Default is 0, indicating to use the driver's default.</p>
        /// </remarks>
        public override int FetchSize
        {
            get { return fetchSize; }
            set { fetchSize = value; }
        }

        /// <summary>
        /// Gets or sets the proxy factory.
        /// </summary>
        /// <remarks>This may be useful to set if you create many instances of 
        /// HibernateTemplate and/or HibernateDaoSupport.  This allows the same
        /// ProxyFactory implementation to be used thereby limiting the
        /// number of dynamic proxy types created in the temporary assembly, which
        /// are never garbage collected due to .NET runtime semantics.
        /// </remarks>
        /// <value>The proxy factory.</value>
        public virtual ProxyFactory ProxyFactory
        {
            get { return sessionProxyFactory; }
            set { sessionProxyFactory = value; }
        }

        /// <summary>
        /// Set the ADO.NET exception translator for this instance.
        /// Applied to System.Data.Common.DbException (or provider specific exception type
        /// in .NET 1.1) thrown by callback code, be it direct
        /// DbException or wrapped Hibernate ADOExceptions.
        /// <p>The default exception translator is either a ErrorCodeExceptionTranslator
        /// if a DbProvider is available, or a FalbackExceptionTranslator otherwise
        /// </p>
        /// </summary>
        /// <value>The ADO exception translator.</value>
        public override IAdoExceptionTranslator AdoExceptionTranslator
        {
            set { adoExceptionTranslator = value; }
            get
            {
                if (adoExceptionTranslator == null)
                {
                    adoExceptionTranslator = SessionFactoryUtils.NewAdoExceptionTranslator(SessionFactory);
                }
                return adoExceptionTranslator;
            }
        }

        /// <summary>
        /// Delegate function that clears the session.
        /// </summary>
        /// <param name="session">The hibernate session.</param>
        /// <returns>null</returns>
        protected object ClearAction(ISession session)
        {
            session.Clear();
            return null;
        }

        /// <summary>
        /// Flush all pending saves, updates and deletes to the database.
        /// </summary>
        /// <remarks>
        /// Only invoke this for selective eager flushing, for example when ADO.NET code
        /// needs to see certain changes within the same transaction. Else, it's preferable
        /// to rely on auto-flushing at transaction completion.
        /// </remarks>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Flush()
        {
            Execute(new HibernateDelegate(FlushAction), true);
        }

        private object FlushAction(ISession session)
        {
            session.Flush();
            return null;
        }

        /// <summary>
        /// Return the persistent instance of the given entity type
        /// with the given identifier, or <code>null</code> if not found.
        /// </summary>
        /// <param name="entityType">The type.</param>
        /// <param name="id">An identifier of the persistent instance.</param>
        /// <returns>The persistent instance, or <code>null</code> if not found</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public object Get(Type entityType, object id)
        {
            return Get(entityType, id, null);
        }

        /// <summary>
        /// Return the persistent instance of the given entity type
        /// with the given identifier, or <code>null</code> if not found.
        /// Obtains the specified lock mode if the instance exists.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="id">The lock mode to obtain.</param>
        /// <param name="lockMode">The lock mode.</param>
        /// the persistent instance, or <code>null</code> if not found
        /// <returns>the persistent instance, or null if not found</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public object Get(Type type, object id, LockMode lockMode)
        {
            return Execute(new GetByTypeHibernateCallback(type, id, lockMode), true);
        }

        /// <summary>
        /// Return the persistent instance of the given entity class
        /// with the given identifier, throwing an exception if not found.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="id">An identifier of the persistent instance.</param>
        /// <returns>The persistent instance</returns>
        /// <exception cref="ObjectRetrievalFailureException">If not found</exception>     
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception> 
        public object Load(Type entityType, object id)
        {
            return Load(entityType, id, null);
        }

        /// <summary>
        /// Return the persistent instance of the given entity class
        ///  with the given identifier, throwing an exception if not found.
        /// Obtains the specified lock mode if the instance exists.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="id">An identifier of the persistent instance.</param>
        /// <param name="lockMode">The lock mode.</param>
        /// <returns>The persistent instance</returns>
        /// <exception cref="ObjectRetrievalFailureException">If not found</exception>     
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception> 
        public object Load(Type entityType, object id, LockMode lockMode)
        {
            return Execute(new LoadByTypeHibernateCallback(entityType, id, lockMode), true);
        }

        /// <summary>
        /// Load the persistent instance with the given identifier
        /// into the given object, throwing an exception if not found.
        /// </summary>
        /// <param name="entity">Entity the object (of the target class) to load into.</param>
        /// <param name="id">An identifier of the persistent instance.</param>
        /// <exception cref="ObjectRetrievalFailureException">If object not found.</exception>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Load(object entity, object id)
        {
            Execute(new LoadByEntityHibernateCallback(entity, id), true);
        }

        /// <summary>
        /// Return all persistent instances of the given entity class.
        /// Note: Use queries or criteria for retrieving a specific subset.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception> 
        public IList LoadAll(Type entityType)
        {
            return (IList) Execute(new LoadAllByTypeHibernateCallback(this, entityType), true);
        }

        /// <summary>
        /// Re-read the state of the given persistent instance.
        /// </summary>
        /// <param name="entity">The persistent instance to re-read.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Refresh(object entity)
        {
            Refresh(entity, null);
        }

        /// <summary>
        /// Re-read the state of the given persistent instance.
        /// Obtains the specified lock mode for the instance.
        /// </summary>
        /// <param name="entity">The persistent instance to re-read.</param>
        /// <param name="lockMode">The lock mode to obtain.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Refresh(object entity, LockMode lockMode)
        {
            Execute(new RefreshHibernateCallback(entity, lockMode), true);
        }

        /// <summary>
        /// Obtain the specified lock level upon the given object, implicitly
        /// checking whether the corresponding database entry still exists
        /// (throwing an OptimisticLockingFailureException if not found).
        /// </summary>
        /// <param name="entity">The he persistent instance to lock.</param>
        /// <param name="lockMode">The lock mode to obtain.</param>
        /// <exception cref="ObjectOptimisticLockingFailureException">If not found</exception>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Lock(object entity, LockMode lockMode)
        {
            Execute(new LockHibernateCallback(entity, lockMode), true);
        }

        /// <summary>
        /// Persist the given transient instance.
        /// </summary>
        /// <param name="entity">The transient instance to persist.</param>
        /// <returns>The generated identifier.</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public object Save(object entity)
        {
            return Execute(new SaveObjectHibernateCallback(this, entity), true);
        }

        /// <summary>
        /// Persist the given transient instance with the given identifier.
        /// </summary>
        /// <param name="entity">The transient instance to persist.</param>
        /// <param name="id">The identifier to assign.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Save(object entity, object id)
        {
            Execute(new SaveObjectWithIdHibernateCallback(this, entity, id), true);
        }

        /// <summary>
        /// Update the given persistent instance.
        /// </summary>
        /// <param name="entity">The persistent instance to update.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Update(object entity)
        {
            Update(entity, null);
        }

        /// <summary>
        /// Update the given persistent instance.
        /// Obtains the specified lock mode if the instance exists, implicitly
        /// checking whether the corresponding database entry still exists
        /// (throwing an OptimisticLockingFailureException if not found).
        /// </summary>
        /// <param name="entity">The persistent instance to update.</param>
        /// <param name="lockMode">The lock mode to obtain.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Update(object entity, LockMode lockMode)
        {
            Execute(new UpdateObjectHibernateCallback(this, entity, lockMode), true);
        }

        /// <summary>
        /// Save or update the given persistent instance,
        /// according to its id (matching the configured "unsaved-value"?).
        /// </summary>
        /// <param name="entity">The persistent instance to save or update
        /// (to be associated with the Hibernate Session).</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void SaveOrUpdate(object entity)
        {
            Execute(new SaveOrUpdateObjectHibernateCallback(this, entity), true);
        }

        /// <summary>
        /// Save or update all given persistent instances,
        /// according to its id (matching the configured "unsaved-value"?).
        /// </summary>
        /// <param name="entities">The persistent instances to save or update
        /// (to be associated with the Hibernate Session)he entities.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void SaveOrUpdateAll(ICollection entities)
        {
            Execute(new SaveOrUpdateAllHibernateCallback(this, entities), true);
        }

        /// <summary>
        /// Copy the state of the given object onto the persistent object with the same identifier. 
        /// If there is no persistent instance currently associated with the session, it will be loaded.
        /// Return the persistent instance. If the given instance is unsaved, 
        /// save a copy of and return it as a newly persistent instance.
        /// The given instance does not become associated with the session. 
        /// This operation cascades to associated instances if the association is mapped with cascade="merge".
        /// The semantics of this method are defined by JSR-220. 
        /// </summary>
        /// <param name="entity">The persistent object to merge.
        /// (<i>not</i> necessarily to be associated with the Hibernate Session)
        /// </param>
        /// <returns>An updated persistent instance</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public object Merge(object entity)
        {
            return Execute(new MergeHibernateCallback(this, entity), true);
        }

        /// <summary>
        /// Remove all objects from the Session cache, and cancel all pending saves,
        /// updates and deletes.
        /// </summary>
        public void Clear()
        {
            Execute(new HibernateDelegate(ClearAction), true);
        }

        /// <summary>
        /// Determines whether the given object is in the Session cache.
        /// </summary>
        /// <param name="entity">the persistence instance to check.</param>
        /// <returns>
        /// 	<c>true</c> if session cache contains the specified entity; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public bool Contains(object entity)
        {
            return (bool) Execute(new ContainsHibernateCallback(entity));
        }

        /// <summary>
        /// Remove the given object from the Session cache.
        /// </summary>
        /// <param name="entity">The persistent instance to evict.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Evict(object entity)
        {
            Execute(new EvictHibernateCallback(entity), true);
        }

        /// <summary>
        /// Delete the given persistent instance.
        /// </summary>
        /// <param name="entity">The persistent instance to delete.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Delete(object entity)
        {
            Delete(entity, null);
        }

        /// <summary>
        /// Delete the given persistent instance.
        /// </summary>
        /// <param name="entity">The persistent instance to delete.</param>
        /// <param name="lockMode">The lock mode to obtain.</param>
        /// <remarks>
        /// Obtains the specified lock mode if the instance exists, implicitly
        /// checking whether the corresponding database entry still exists
        /// (throwing an OptimisticLockingFailureException if not found).
        /// </remarks>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Delete(object entity, LockMode lockMode)
        {
            Execute(new DeleteLockModeHibernateCallback(this, entity, lockMode), true);
        }

        /// <summary>
        /// Delete all objects returned by the query.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language.</param>
        /// <returns>The number of entity instances deleted.</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public int Delete(string queryString)
        {
            return Delete(queryString, (Object[]) null, (IType[]) null);
        }

        /// <summary>
        ///  Delete all objects returned by the query.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="type">The Hibernate type of the parameter (or <code>null</code>).</param>
        /// <returns>The number of entity instances deleted.</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public int Delete(string queryString, object value, IType type)
        {
            return Delete(queryString, new Object[] {value}, new IType[] {type});
        }

        /// <summary>
        /// Delete all objects returned by the query.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language.</param>
        /// <param name="values">The values of the parameters.</param>
        /// <param name="types"> Hibernate types of the parameters (or <code>null</code>)</param>
        /// <returns>The number of entity instances deleted.</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentOutOfRangeException">If length for argument values and types are not equal.</exception>
        public int Delete(String queryString, Object[] values, IType[] types)
        {
            if (values != null && types != null && values.Length != types.Length)
            {
                throw new ArgumentOutOfRangeException("values", "Length of values array must match length of types array");
            }
            return (int) Execute(new DeletebyQueryHibernateCallback(this, queryString, values, types), true);
        }

        /// <summary>
        /// Delete all given persistent instances.
        /// </summary>
        /// <param name="entities">The persistent instances to delete.</param>
        /// <remarks>
        /// This can be combined with any of the find methods to delete by query
        /// in two lines of code, similar to Session's delete by query methods.
        /// </remarks>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void DeleteAll(ICollection entities)
        {
            Execute(new DeleteAllHibernateCallback(this, entities), true);
        }

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <remarks>
        /// Application exceptions thrown by the action object get propagated to the
        /// caller (can only be unchecked). Hibernate exceptions are transformed into
        /// appropriate DAO ones. Allows for returning a result object, i.e. a domain
        /// object or a collection of domain objects.
        /// <p>Note: Callback code is not supposed to handle transactions itself!
        /// Use an appropriate transaction manager like HibernateTransactionManager.
        /// Generally, callback code must not touch any Session lifecycle methods,
        /// like close, disconnect, or reconnect, to let the template do its work.
        /// </p>
        /// </remarks>
        /// <param name="del">The delegate callback object that specifies the Hibernate action.</param>
        /// <returns>a result object returned by the action, or <code>null</code>
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public object Execute(HibernateDelegate del)
        {
            return Execute(new ExecuteHibernateCallbackUsingDelegate(del));
        }

        /// <summary>
        /// Execute the action specified by the delegate within a Session.
        /// </summary>
        /// <param name="del">The HibernateDelegate that specifies the action
        /// to perform.</param>
        /// <param name="exposeNativeSession">if set to <c>true</c> expose the native hibernate session to
        /// callback code.</param>
        /// <returns>a result object returned by the action, or <code>null</code>
        /// </returns>
        public object Execute(HibernateDelegate del, bool exposeNativeSession)
        {
            return Execute(new ExecuteHibernateCallbackUsingDelegate(del), true);
        }

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <param name="action">The callback object that specifies the Hibernate action.</param>
        /// <returns>
        /// a result object returned by the action, or <code>null</code>
        /// </returns>
        /// <remarks>
        /// Application exceptions thrown by the action object get propagated to the
        /// caller (can only be unchecked). Hibernate exceptions are transformed into
        /// appropriate DAO ones. Allows for returning a result object, i.e. a domain
        /// object or a collection of domain objects.
        /// <p>Note: Callback code is not supposed to handle transactions itself!
        /// Use an appropriate transaction manager like HibernateTransactionManager.
        /// Generally, callback code must not touch any Session lifecycle methods,
        /// like close, disconnect, or reconnect, to let the template do its work.
        /// </p>
        /// </remarks>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public object Execute(IHibernateCallback action)
        {
            return Execute(action, ExposeNativeSession);
        }

        /// <summary>
        /// Execute the specified action assuming that the result object is a List.
        /// </summary>
        /// <remarks>
        /// This is a convenience method for executing Hibernate find calls or
        /// queries within an action.
        /// </remarks>
        /// <param name="action">The calback object that specifies the Hibernate action.</param>
        /// <returns>A IList returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList ExecuteFind(IHibernateCallback action)
        {
            Object result = Execute(action, ExposeNativeSession);
            if (result != null && !(result is IList))
            {
                throw new InvalidDataAccessApiUsageException(
                    "Result object returned from HibernateCallback isn't a List: [" + result + "]");
            }
            return (IList) result;
        }

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <param name="action">callback object that specifies the Hibernate action.</param>
        /// <param name="exposeNativeSession">if set to <c>true</c> expose the native hibernate session to
        /// callback code.</param>
        /// <returns>
        /// a result object returned by the action, or <code>null</code>
        /// </returns>
        public object Execute(IHibernateCallback action, bool exposeNativeSession)
        {
            ISession session = Session;

            bool existingTransaction = SessionFactoryUtils.IsSessionTransactional(session, SessionFactory);
            if (existingTransaction)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("Found thread-bound Session for HibernateTemplate");
                }
            }

            FlushModeHolder previousFlushModeHolder = new FlushModeHolder();
            try
            {
                previousFlushModeHolder = ApplyFlushMode(session, existingTransaction);
                ISession sessionToExpose = (exposeNativeSession ? session : CreateSessionProxy(session));
                Object result = action.DoInHibernate(sessionToExpose);
                FlushIfNecessary(session, existingTransaction);
                return result;
            }
            catch (ADOException ex)
            {
                IDbProvider dbProvider = SessionFactoryUtils.GetDbProvider(SessionFactory);
                if (dbProvider != null && dbProvider.IsDataAccessException(ex.InnerException))
                {
                    throw ConvertAdoAccessException(ex);
                }
                else
                {
                    throw new HibernateSystemException(ex);
                }
            }
            catch (HibernateException ex)
            {
                throw ConvertHibernateAccessException(ex);
            }
            catch (Exception ex)
            {
                IDbProvider dbProvider = SessionFactoryUtils.GetDbProvider(SessionFactory);
                if (dbProvider != null && dbProvider.IsDataAccessException(ex))
                {
                    throw ConvertAdoAccessException(ex);
                }
                else
                {
                    // Callback code throw application exception or other non DB related exception.
                    throw;
                }
            }
            finally
            {
                if (existingTransaction)
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Not closing pre-bound Hibernate Session after HibernateTemplate");
                    }
                    if (previousFlushModeHolder.ModeWasSet)
                    {
                        session.FlushMode = previousFlushModeHolder.Mode;
                    }
                }
                else
                {
                    // Never use deferred close for an explicitly new Session.
                    if (AlwaysUseNewSession)
                    {
                        SessionFactoryUtils.CloseSession(session);
                    }
                    else
                    {
                        SessionFactoryUtils.CloseSessionOrRegisterDeferredClose(session, SessionFactory);
                    }
                }
            }
        }

        /// <summary>
        /// Execute a query for persistent instances.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <returns>
        /// a List containing 0 or more persistent instances
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList Find(string queryString)
        {
            return Find(queryString, (object[]) null, (IType[]) null);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding
        /// one value to a "?" parameter in the query string.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <param name="value">the value of the parameter</param>
        /// <returns>
        /// a List containing 0 or more persistent instances
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList Find(string queryString, object value)
        {
            return Find(queryString, new object[] {value}, (IType[]) null);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding one value
        /// to a "?" parameter of the given type in the query string.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="type">Hibernate type of the parameter (or <code>null</code>)</param>
        /// <returns>
        /// a List containing 0 or more persistent instances
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList Find(string queryString, object value, IType type)
        {
            return Find(queryString, new object[] {value}, new IType[] {type});
        }

        /// <summary>
        /// Execute a query for persistent instances, binding a
        /// number of values to "?" parameters in the query string.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <param name="values">the values of the parameters</param>
        /// <returns>a List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList Find(string queryString, object[] values)
        {
            return Find(queryString, values, (IType[]) null);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding a number of
        /// values to "?" parameters of the given types in the query string.
        /// </summary>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>
        /// <param name="values">The values of the parameters</param>
        /// <param name="types">Hibernate types of the parameters (or <code>null</code>)</param>
        /// <returns>
        /// a List containing 0 or more persistent instances
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentException">If values and types are not null and their lengths are not equal</exception>          
        public IList Find(string queryString, object[] values, IType[] types)
        {
            if (values != null && types != null && values.Length != types.Length)
            {
                throw new ArgumentException("Length of values array must match length of types array");
            }
            return (IList) Execute(new FindHibernateCallback(this, queryString, values, types), true);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding
        /// one value to a named parameter in the query string.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>a List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList FindByNamedParam(string queryName, string paramName, object value)
        {
            return FindByNamedParam(queryName, paramName, value, null);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding
        /// one value to a named parameter in the query string.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">Hibernate type of the parameter (or <code>null</code>)</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList FindByNamedParam(string queryName, string paramName, object value, IType type)
        {
            return FindByNamedParam(queryName, new string[] {paramName}, new object[] {value}, new IType[] {type});
        }

        /// <summary>
        /// Execute a query for persistent instances, binding a
        /// number of values to named parameters in the query string.
        /// </summary>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList FindByNamedParam(string queryString, string[] paramNames, object[] values)
        {
            return FindByNamedParam(queryString, paramNames, values, null);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding a
        /// number of values to named parameters in the query string.
        /// </summary>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters</param>
        /// <param name="types">Hibernate types of the parameters (or <code>null</code>)</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentOutOfRangeException">If paramNames length is not equal to values length or
        /// if paramNames length is not equal to types length (when types is not null)</exception>
        public IList FindByNamedParam(string queryString, string[] paramNames, object[] values, IType[] types)
        {
            if (paramNames.Length != values.Length)
            {
                throw new ArgumentOutOfRangeException("paramNames", "Length of paramNames array must match length of values array");
            }
            if (types != null && paramNames.Length != types.Length)
            {
                throw new ArgumentOutOfRangeException("paramNames", "Length of paramNames array must match length of types array");
            }

            return (IList) Execute(new FindByNamedParamHibernateCallback(this, queryString, paramNames, values, types), true);
        }

        /// <summary>
        /// Execute a named query for persistent instances.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList FindByNamedQuery(string queryName)
        {
            return FindByNamedQuery(queryName, (object[]) null, (IType[]) null);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a "?" parameter in the query string.
        ///  A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList FindByNamedQuery(string queryName, object value)
        {
            return FindByNamedQuery(queryName, new object[] {value}, (IType[]) null);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a "?" parameter in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">Hibernate type of the parameter (or <code>null</code>)</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList FindByNamedQuery(string queryName, object value, IType type)
        {
            return FindByNamedQuery(queryName, new object[] {value}, new IType[] {type});
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding a
        /// number of values to "?" parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="values">The values of the parameters</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList FindByNamedQuery(string queryName, object[] values)
        {
            return FindByNamedQuery(queryName, values, (IType[]) null);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding a
        /// number of values to "?" parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="values">The values of the parameters</param>
        /// <param name="types">Hibernate types of the parameters (or <code>null</code>)</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentOutOfRangeException">If values and types are not null and their lengths differ.</exception>        
        public IList FindByNamedQuery(string queryName, object[] values, IType[] types)
        {
            if (values != null && types != null && values.Length != types.Length)
            {
                throw new ArgumentOutOfRangeException("Length of values array must match length of types array");
            }
            return (IList) Execute(new FindByNamedQueryHibernateCallback(this, queryName, values, types), true);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a named parameter in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList FindByNamedQueryAndNamedParam(string queryName, string paramName, object value)
        {
            return FindByNamedQueryAndNamedParam(queryName, paramName, value, null);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a named parameter in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">The Hibernate type of the parameter (or <code>null</code>)</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        public IList FindByNamedQueryAndNamedParam(string queryName, string paramName, object value, IType type)
        {
            return FindByNamedQueryAndNamedParam(
                queryName, new string[] {paramName}, new object[] {value}, new IType[] {type});
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// number of values to named parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters.</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList FindByNamedQueryAndNamedParam(string queryName, string[] paramNames, object[] values)
        {
            return FindByNamedQueryAndNamedParam(queryName, paramNames, values, null);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// number of values to named parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters.</param>
        /// <param name="types">Hibernate types of the parameters (or <code>null</code>)</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentOutOfRangeException">If paramNames length is not equal to values length or
        /// if paramNames length is not equal to types length (when types is not null)</exception>                
        public IList FindByNamedQueryAndNamedParam(string queryName, string[] paramNames, object[] values, IType[] types)
        {
            if (paramNames != null && values != null && paramNames.Length != values.Length)
            {
                throw new ArgumentOutOfRangeException("paramNames", "Length of paramNames array must match length of values array");
            }
            if (paramNames != null && types != null && paramNames.Length != types.Length)
            {
                throw new ArgumentOutOfRangeException("paramNams", "Length of paramNames array must match length of types array");
            }
            return (IList) Execute(new FindByNamedQueryAndNamedParamHibernateCallback(this, queryName, paramNames, values, types), true);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding the properties
        /// of the given object to  named parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="valueObject">The values of the parameters</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList FindByNamedQueryAndValueObject(string queryName, object valueObject)
        {
            return (IList) Execute(new FindByNamedQueryAndValueObjectHibernateCallback(this, queryName, valueObject), true);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding the properties
        /// of the given object to <i>named</i> parameters in the query string.
        /// </summary>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>       
        /// <param name="valueObject">The values of the parameters</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList FindByValueObject(string queryString, object valueObject)
        {
            return (IList) Execute(new FindByValueObjectHibernateCallback(this, queryString, valueObject), true);
        }

        /// <summary>
        /// Create a close-suppressing proxy for the given Hibernate Session.
        /// The proxy also prepares returned Query and Criteria objects.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>The session proxy.</returns>
        public virtual ISession CreateSessionProxy(ISession session)
        {
            //TODO can move to HibernateAccessor and make protected
            //     if issue reported with AOP+Multiple Threads resolve.
            //     have not been able to reproduce so added lock as a precaution.
            //     
            lock (syncRoot)
            {
                if (sessionProxyFactory == null)
                {
                    sessionProxyFactory = new ProxyFactory();
                    sessionProxyFactory.AddAdvice(new CloseSuppressingMethodInterceptor(this));
                }

                sessionProxyFactory.Target = session;

                return (ISession) sessionProxyFactory.GetProxy();
            }
        }

        /// <summary>
        /// Check whether write operations are allowed on the given Session.
        /// </summary>
        /// <remarks>
        /// Default implementation throws an InvalidDataAccessApiUsageException
        /// in case of FlushMode.Never. Can be overridden in subclasses.
        /// </remarks>
        /// <param name="session">The current Hibernate session.</param>
        /// <exception cref="InvalidDataAccessApiUsageException">If write operation is attempted in read-only mode
        /// </exception>
        public virtual void CheckWriteOperationAllowed(ISession session)
        {
            if (CheckWriteOperations && TemplateFlushMode != TemplateFlushMode.Eager &&
                AreEqualFlushMode(TemplateFlushMode.Never, session.FlushMode))
            {
                throw new InvalidDataAccessApiUsageException(
                    "Write operations are not allowed in read-only mode (FlushMode.NEVER) - turn your Session " +
                    "into FlushMode.AUTO or remove 'readOnly' marker from transaction definition");
            }
        }

        /// <summary>
        /// Compares if the flush mode enumerations, Spring's
        /// TemplateFlushMode and NHibernates FlushMode have equal
        /// settings.
        /// </summary>
        /// <param name="tfm">The template flush mode.</param>
        /// <param name="fm">The NHibernate flush mode.</param>
        /// <returns>
        /// Returns true if both are Never, Auto, or Commit, false
        /// otherwise.
        /// </returns>
        protected bool AreEqualFlushMode(TemplateFlushMode tfm, FlushMode fm)
        {
            if ((tfm == TemplateFlushMode.Never && fm == FlushMode.Never) ||
                (tfm == TemplateFlushMode.Auto && fm == FlushMode.Auto) ||
                (tfm == TemplateFlushMode.Commit && fm == FlushMode.Commit))
            {
                return true;
            }
            else
            {
                return false;
            }
            //TODO other combinations.
        }
    }

    //TODO see if can create common base class for some callbacks.

    internal class ContainsHibernateCallback : IHibernateCallback
    {
        private object entity;

        public ContainsHibernateCallback(object entity)
        {
            this.entity = entity;
        }

        public object DoInHibernate(ISession session)
        {
            return session.Contains(entity);
        }
    }

    internal class DeleteLockModeHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private object entity;
        private LockMode lockMode;

        public DeleteLockModeHibernateCallback(HibernateTemplate template, object entity, LockMode lockMode)
        {
            this.outer = template;
            this.entity = entity;
            this.lockMode = lockMode;
        }

        public object DoInHibernate(ISession session)
        {
            outer.CheckWriteOperationAllowed(session);
            if (lockMode != null)
            {
                session.Lock(entity, lockMode);
            }
            session.Delete(entity);
            return null;
        }
    }

    internal class DeletebyQueryHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private string queryString;
        private object[] values;
        private IType[] types;

        public DeletebyQueryHibernateCallback(HibernateTemplate template, string queryString, object[] values, IType[] types)
        {
            this.outer = template;
            this.queryString = queryString;
            this.values = values;
            this.types = types;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            outer.CheckWriteOperationAllowed(session);
            if (values != null)
            {
                return session.Delete(queryString, values, types);
            }
            else
            {
                return session.Delete(queryString);
            }
        }
    }

    internal class DeleteAllHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private ICollection entities;

        public DeleteAllHibernateCallback(HibernateTemplate template, ICollection entities)
        {
            this.outer = template;
            this.entities = entities;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            outer.CheckWriteOperationAllowed(session);
            foreach (object entity in entities)
            {
                session.Delete(entity);
            }
            return null;
        }
    }

    internal class EvictHibernateCallback : IHibernateCallback
    {
        private object entity;

        public EvictHibernateCallback(object entity)
        {
            this.entity = entity;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            session.Evict(entity);
            return null;
        }
    }

    internal class FindHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private string queryString;
        private object[] values;
        private IType[] types;

        public FindHibernateCallback(HibernateTemplate template, string queryString, object[] values, IType[] types)
        {
            this.outer = template;
            this.queryString = queryString;
            this.values = values;
            this.types = types;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            IQuery queryObject = session.CreateQuery(queryString);
            outer.PrepareQuery(queryObject);
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (types != null && types[i] != null)
                    {
                        queryObject.SetParameter(i, values[i], types[i]);
                    }
                    else
                    {
                        queryObject.SetParameter(i, values[i]);
                    }
                }
            }

            return queryObject.List();
        }
    }

    internal class FindByNamedParamHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private string queryString;
        private string[] paramNames;
        private object[] values;
        private IType[] types;

        public FindByNamedParamHibernateCallback(HibernateTemplate template, string queryString, string[] paramNames, object[] values, IType[] types)
        {
            this.outer = template;
            this.queryString = queryString;
            this.paramNames = paramNames;
            this.values = values;
            this.types = types;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            IQuery queryObject = session.CreateQuery(queryString);
            outer.PrepareQuery(queryObject);
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    outer.ApplyNamedParameterToQuery(queryObject, paramNames[i], values[i], (types != null ? types[i] : null));
                }
            }
            return queryObject.List();
        }
    }

    internal class FindByNamedQueryHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private string queryName;
        private object[] values;
        private IType[] types;

        public FindByNamedQueryHibernateCallback(HibernateTemplate template, string queryName, object[] values, IType[] types)
        {
            this.outer = template;
            this.queryName = queryName;
            this.values = values;
            this.types = types;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            IQuery queryObject = session.GetNamedQuery(queryName);
            outer.PrepareQuery(queryObject);
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (types != null && types[i] != null)
                    {
                        queryObject.SetParameter(i, values[i], types[i]);
                    }
                    else
                    {
                        queryObject.SetParameter(i, values[i]);
                    }
                }
            }
            return queryObject.List();
        }
    }

    internal class FindByNamedQueryAndNamedParamHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private string queryName;
        private string[] paramNames;
        private object[] values;
        private IType[] types;

        public FindByNamedQueryAndNamedParamHibernateCallback(HibernateTemplate template, string queryName, string[] paramNames, object[] values, IType[] types)
        {
            this.outer = template;
            this.queryName = queryName;
            this.paramNames = paramNames;
            this.values = values;
            this.types = types;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            IQuery queryObject = session.GetNamedQuery(queryName);
            outer.PrepareQuery(queryObject);
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    outer.ApplyNamedParameterToQuery(queryObject, paramNames[i], values[i], (types != null ? types[i] : null));
                }
            }
            return queryObject.List();
        }
    }

    internal class FindByNamedQueryAndValueObjectHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private string queryName;
        private object valueObject;

        public FindByNamedQueryAndValueObjectHibernateCallback(HibernateTemplate template, string queryName, object valueObject)
        {
            this.outer = template;
            this.queryName = queryName;
            this.valueObject = valueObject;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            IQuery queryObject = session.GetNamedQuery(queryName);
            outer.PrepareQuery(queryObject);
            queryObject.SetProperties(valueObject);
            return queryObject.List();
        }
    }

    internal class FindByValueObjectHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private string queryString;
        private object valueObject;

        public FindByValueObjectHibernateCallback(HibernateTemplate template, string queryString, object valueObject)
        {
            this.outer = template;
            this.queryString = queryString;
            this.valueObject = valueObject;
        }

        public object DoInHibernate(ISession session)
        {
            IQuery queryObject = session.CreateQuery(queryString);
            outer.PrepareQuery(queryObject);
            queryObject.SetProperties(valueObject);
            return queryObject.List();
        }
    }

    internal class ExecuteHibernateCallbackUsingDelegate : IHibernateCallback
    {
        private HibernateDelegate del;

        public ExecuteHibernateCallbackUsingDelegate(HibernateDelegate d)
        {
            del = d;
        }

        public object DoInHibernate(ISession session)
        {
            return del(session);
        }
    }

    internal class GetByTypeHibernateCallback : IHibernateCallback
    {
        private Type entityType;
        private object id;
        private LockMode lockMode;

        public GetByTypeHibernateCallback(Type entityType, object id, LockMode lockMode)
        {
            this.entityType = entityType;
            this.id = id;
            this.lockMode = lockMode;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            if (lockMode != null)
            {
                return session.Get(entityType, id, lockMode);
            }
            else
            {
                return session.Get(entityType, id);
            }
        }
    }

    internal class LoadByTypeHibernateCallback : IHibernateCallback
    {
        private Type entityType;
        private object id;
        private LockMode lockMode;

        public LoadByTypeHibernateCallback(Type entityType, object id, LockMode lockMode)
        {
            this.entityType = entityType;
            this.id = id;
            this.lockMode = lockMode;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            if (lockMode != null)
            {
                return session.Load(entityType, id, lockMode);
            }
            else
            {
                return session.Load(entityType, id);
            }
        }
    }

    internal class LoadByEntityHibernateCallback : IHibernateCallback
    {
        private object entity;
        private object id;

        public LoadByEntityHibernateCallback(object entity, object id)
        {
            this.entity = entity;
            this.id = id;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            session.Load(entity, id);
            return null;
        }
    }

    internal class LoadAllByTypeHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private Type entityType;

        public LoadAllByTypeHibernateCallback(HibernateTemplate template, Type entityType)
        {
            outer = template;
            this.entityType = entityType;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            ICriteria criteria = session.CreateCriteria(entityType);
            outer.PrepareCriteria(criteria);
            return criteria.List();
        }
    }

    internal class LockHibernateCallback : IHibernateCallback
    {
        private object entity;
        private LockMode lockMode;

        public LockHibernateCallback(object entity, LockMode lockMode)
        {
            this.entity = entity;
            this.lockMode = lockMode;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            session.Lock(entity, lockMode);
            return null;
        }
    }

    internal class RefreshHibernateCallback : IHibernateCallback
    {
        private object entity;
        private LockMode lockMode;

        public RefreshHibernateCallback(object entity, LockMode lockMode)
        {
            this.entity = entity;
            this.lockMode = lockMode;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            if (lockMode != null)
            {
                session.Refresh(entity, lockMode);
            }
            else
            {
                session.Refresh(entity);
            }
            return null;
        }
    }

    internal class SaveObjectHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private object entity;

        public SaveObjectHibernateCallback(HibernateTemplate template, object entity)
        {
            this.outer = template;
            this.entity = entity;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            outer.CheckWriteOperationAllowed(session);
            return session.Save(entity);
        }
    }

    internal class SaveObjectWithIdHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private object entity;
        private object id;

        public SaveObjectWithIdHibernateCallback(HibernateTemplate template, object entity, object id)
        {
            this.outer = template;
            this.entity = entity;
            this.id = id;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            outer.CheckWriteOperationAllowed(session);
            session.Save(entity, id);
            return null;
        }
    }

    internal class UpdateObjectHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private object entity;
        private LockMode lockMode;

        public UpdateObjectHibernateCallback(HibernateTemplate template, object entity, LockMode lockMode)
        {
            this.outer = template;
            this.entity = entity;
            this.lockMode = lockMode;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            outer.CheckWriteOperationAllowed(session);
            session.Update(entity);
            if (lockMode != null)
            {
                session.Lock(entity, lockMode);
            }
            return null;
        }
    }

    internal class SaveOrUpdateObjectHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private object entity;

        public SaveOrUpdateObjectHibernateCallback(HibernateTemplate template, object entity)
        {
            this.outer = template;
            this.entity = entity;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            outer.CheckWriteOperationAllowed(session);
            session.SaveOrUpdate(entity);
            return null;
        }
    }

    internal class SaveOrUpdateAllHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private ICollection entities;

        public SaveOrUpdateAllHibernateCallback(HibernateTemplate template, ICollection entities)
        {
            this.outer = template;
            this.entities = entities;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            outer.CheckWriteOperationAllowed(session);
            foreach (object entity in entities)
            {
                session.SaveOrUpdate(entity);
            }
            return null;
        }
    }

    internal class MergeHibernateCallback : IHibernateCallback
    {
        private HibernateTemplate outer;
        private object entity;

        public MergeHibernateCallback(HibernateTemplate template, object entity)
        {
            this.outer = template;
            this.entity = entity;
        }

        /// <summary>
        /// Gets called by HibernateTemplate with an active
        /// Hibernate Session. Does not need to care about activating or closing
        /// the Session, or handling transactions.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Allows for returning a result object created within the callback, i.e.
        /// a domain object or a collection of domain objects. Note that there's
        /// special support for single step actions: see HibernateTemplate.find etc.
        /// </p>
        /// </remarks>
        public object DoInHibernate(ISession session)
        {
            outer.CheckWriteOperationAllowed(session);
            return session.Merge(entity);
        }
    }
}
