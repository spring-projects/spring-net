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
using NHibernate.Type;
using Spring.Aop.Framework;
using Spring.Dao;
using Spring.Data.Common;
using Spring.Data.Support;
using Spring.Objects.Factory;

namespace Spring.Data.NHibernate.Generic
{
    /// <summary>
    /// Generic version of the Helper class that simplifies NHibernate data access code
    /// </summary>
    /// <remarks>
    /// <p>Typically used to implement data access or business logic services that
    /// use NHibernate within their implementation but are Hibernate-agnostic in their
    /// interface. The latter or code calling the latter only have to deal with
    /// domain objects.</p>
    ///
    /// <p>The central method is Execute() supporting Hibernate access code which
    /// implements the HibernateCallback interface. It provides NHibernate Session
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
    /// <p>This class can be considered as a direct alternative to working with the raw
    /// Hibernate Session API (through SessionFactoryUtils.Session).
    /// </p>
    ///
    /// <p>LocalSessionFactoryObject is the preferred way of obtaining a reference
    /// to a specific NHibernate ISessionFactory.
    /// </p>
    /// </remarks>
    /// <author>Sree Nivask (.NET)</author>
    /// <author>Mark Pollack (.NET)</author>
    public class HibernateTemplate : HibernateAccessor, IHibernateOperations
    {
        NHibernate.HibernateTemplate classicHibernateTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateTemplate"/> class.
        /// </summary>
        public HibernateTemplate()
        {
            classicHibernateTemplate = new NHibernate.HibernateTemplate();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateTemplate"/> class.
        /// </summary>
        /// <remarks>Allows creation of a new non-transactional session when no
        /// transactional Session can be found for the current thread</remarks>
        /// <param name="sessionFactory">The session factory to create sessions.</param>
        public HibernateTemplate(ISessionFactory sessionFactory)
        {
            classicHibernateTemplate = new Spring.Data.NHibernate.HibernateTemplate(sessionFactory);
            AfterPropertiesSet();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateTemplate"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory to create sessions.</param>
        /// <param name="allowCreate">if set to <c>true</c> allow creation
        /// of a new non-transactional session when no transactional Session can be found
        /// for the current thread.</param>
        public HibernateTemplate(ISessionFactory sessionFactory, bool allowCreate)
        {
            classicHibernateTemplate = new Spring.Data.NHibernate.HibernateTemplate(sessionFactory, allowCreate);
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
            get { return classicHibernateTemplate.AllowCreate; }
            set { classicHibernateTemplate.AllowCreate = value; }
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
            get { return classicHibernateTemplate.AlwaysUseNewSession; }
            set { classicHibernateTemplate.AlwaysUseNewSession = value; }
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
            get { return classicHibernateTemplate.ExposeNativeSession; }
            set { classicHibernateTemplate.ExposeNativeSession = value; }
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
            get { return classicHibernateTemplate.TemplateFlushMode; }
            set { classicHibernateTemplate.TemplateFlushMode = value; }
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
        public override IInterceptor EntityInterceptor
        {
            get { return classicHibernateTemplate.EntityInterceptor; }
            set { classicHibernateTemplate.EntityInterceptor = value; }
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
            set { classicHibernateTemplate.EntityInterceptorObjectName = value;}
        }

        /// <summary>
        /// Gets or sets the session factory that should be used to create
        /// NHibernate ISessions.
        /// </summary>
        /// <value>The session factory.</value>
        public override ISessionFactory SessionFactory
        {
            get { return classicHibernateTemplate.SessionFactory; }
            set { classicHibernateTemplate.SessionFactory = value; }
        }

        /// <summary>
        /// Set the object factory instance.
        /// </summary>
        /// <value>The object factory instance</value>
        public override IObjectFactory ObjectFactory
        {
            set { classicHibernateTemplate.ObjectFactory = value; }
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
            get { return classicHibernateTemplate.CacheQueries; }
            set { classicHibernateTemplate.CacheQueries = value; }
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
            get { return classicHibernateTemplate.QueryCacheRegion; }
            set { classicHibernateTemplate.QueryCacheRegion = value; }
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
            get { return classicHibernateTemplate.FetchSize; }
            set { classicHibernateTemplate.FetchSize = value; }
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
            get { return classicHibernateTemplate.MaxResults; }
            set { classicHibernateTemplate.MaxResults = value; }
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
            get { return classicHibernateTemplate.AdoExceptionTranslator; }
            set { classicHibernateTemplate.AdoExceptionTranslator = value; }
        }

        /// <summary>
        /// Gets the classic hibernate template for access to non-generic methods.
        /// </summary>
        /// <value>The classic hibernate template.</value>
        public NHibernate.HibernateTemplate ClassicHibernateTemplate
        {
            get { return classicHibernateTemplate;  }
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
            get { return classicHibernateTemplate.ProxyFactory; }
            set { classicHibernateTemplate.ProxyFactory = value; }
        }

        /// <summary>
        /// Remove all objects from the Session cache, and cancel all pending saves,
        /// updates and deletes.
        /// </summary>
        public void Clear()
        {
            classicHibernateTemplate.Clear();
        }


        /// <summary>
        /// Delete the given persistent instance.
        /// </summary>
        /// <param name="entity">The persistent instance to delete.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Delete(object entity)
        {
            classicHibernateTemplate.Delete(entity);
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
            classicHibernateTemplate.Delete(entity, lockMode);
        }

        /// <summary>
        /// Delete all objects returned by the query.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language.</param>
        /// <returns>The number of entity instances deleted.</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public int Delete(string queryString)
        {
            return classicHibernateTemplate.Delete(queryString);
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
            return classicHibernateTemplate.Delete(queryString, value, type);
        }

        /// <summary>
        /// Delete all objects returned by the query.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language.</param>
        /// <param name="values">The values of the parameters.</param>
        /// <param name="types"> Hibernate types of the parameters (or <code>null</code>)</param>
        /// <returns>The number of entity instances deleted.</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public int Delete(string queryString, object[] values, IType[] types)
        {
            return classicHibernateTemplate.Delete(queryString, values, types);
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
            classicHibernateTemplate.Flush();
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
            classicHibernateTemplate.Load(entity, id);
        }

        /// <summary>
        /// Re-read the state of the given persistent instance.
        /// </summary>
        /// <param name="entity">The persistent instance to re-read.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Refresh(object entity)
        {
            classicHibernateTemplate.Refresh(entity);
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
            classicHibernateTemplate.Refresh(entity, lockMode);
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
            return classicHibernateTemplate.Contains(entity);
        }

        /// <summary>
        /// Remove the given object from the Session cache.
        /// </summary>
        /// <param name="entity">The persistent instance to evict.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Evict(object entity)
        {
            classicHibernateTemplate.Evict(entity);
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
            classicHibernateTemplate.Lock(entity, lockMode);
        }

        /// <summary>
        /// Persist the given transient instance.
        /// </summary>
        /// <param name="entity">The transient instance to persist.</param>
        /// <returns>The generated identifier.</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public object Save(object entity)
        {
            return classicHibernateTemplate.Save(entity);
        }

        /// <summary>
        /// Persist the given transient instance with the given identifier.
        /// </summary>
        /// <param name="entity">The transient instance to persist.</param>
        /// <param name="id">The identifier to assign.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Save(object entity, object id)
        {
            classicHibernateTemplate.Save(entity, id);
        }

        /// <summary>
        /// Update the given persistent instance.
        /// </summary>
        /// <param name="entity">The persistent instance to update.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public void Update(object entity)
        {
            classicHibernateTemplate.Update(entity);
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
            classicHibernateTemplate.Update(entity, lockMode);
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
            classicHibernateTemplate.SaveOrUpdate(entity);
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
            return classicHibernateTemplate.Merge(entity);
        }

        /// <summary>
        /// Return the persistent instance of the given entity type
        /// with the given identifier, or null if not found.
        /// Obtains the specified lock mode if the instance exists.
        /// </summary>
        /// <typeparam name="T">The object type to get.</typeparam>
        /// <param name="id">The id of the object to get.</param>
        /// <returns>the persistent instance, or null if not found</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public T Get<T>(object id)
        {
            return Get<T>(id, null);
        }

        /// <summary>
        /// Return the persistent instance of the given entity type
        /// with the given identifier, or null if not found.
        /// Obtains the specified lock mode if the instance exists.
        /// </summary>
        /// <typeparam name="T">The object type to get.</typeparam>
        /// <param name="id">The lock mode to obtain.</param>
        /// <param name="lockMode">The lock mode.</param>
        /// <returns>the persistent instance, or null if not found</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public T Get<T>(object id, LockMode lockMode)
        {
            return Execute(new GetByTypeHibernateCallback<T>(id, lockMode), true);
        }

        /// <summary>
        /// Return the persistent instance of the given entity class
        /// with the given identifier, throwing an exception if not found.
        /// </summary>
        /// <typeparam name="T">The object type to load.</typeparam>
        /// <param name="id">An identifier of the persistent instance.</param>
        /// <returns>The persistent instance</returns>
        /// <exception cref="ObjectRetrievalFailureException">If not found</exception>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public T Load<T>(object id)
        {
            return Load<T>(id, null);
        }

        /// <summary>
        /// Return the persistent instance of the given entity class
        ///  with the given identifier, throwing an exception if not found.
        /// Obtains the specified lock mode if the instance exists.
        /// </summary>
        /// <typeparam name="T">The object type to load.</typeparam>
        /// <param name="id">An identifier of the persistent instance.</param>
        /// <param name="lockMode">The lock mode.</param>
        /// <returns>The persistent instance</returns>
        /// <exception cref="ObjectRetrievalFailureException">If not found</exception>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public T Load<T>(object id, LockMode lockMode)
        {
            return Execute(new LoadByTypeHibernateCallback<T>(id, lockMode), true);
        }

        /// <summary>
        /// Return all persistent instances of the given entity class.
        /// Note: Use queries or criteria for retrieving a specific subset.
        /// </summary>
        /// <typeparam name="T">The object type to load.</typeparam>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> LoadAll<T>()
        {
            return ExecuteFind(new LoadAllByTypeHibernateCallback<T>(this), true);
        }

        /// <summary>
        /// Execute a query for persistent instances.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <returns>
        /// a generic List containing 0 or more persistent instances
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> Find<T>(string queryString)
        {
            return Find<T>(queryString, (object[]) null, (IType[]) null);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding
        /// one value to a "?" parameter in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <param name="value">the value of the parameter</param>
        /// <returns>
        /// a generic List containing 0 or more persistent instances
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> Find<T>(string queryString, object value)
        {
            return Find<T>(queryString, new object[] {value}, (IType[]) null);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding one value
        /// to a "?" parameter of the given type in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="type">Hibernate type of the parameter (or null)</param>
        /// <returns>
        /// a generic List containing 0 or more persistent instances
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> Find<T>(string queryString, object value, IType type)
        {
            return Find<T>(queryString, new object[] {value}, new IType[] {type});
        }

        /// <summary>
        /// Execute a query for persistent instances, binding a
        /// number of values to "?" parameters in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <param name="values">the values of the parameters</param>
        /// <returns>a generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> Find<T>(string queryString, object[] values)
        {
            return Find<T>(queryString, values, (IType[]) null);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding a number of
        /// values to "?" parameters of the given types in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>
        /// <param name="values">The values of the parameters</param>
        /// <param name="types">Hibernate types of the parameters (or null)</param>
        /// <returns>
        /// a generic List containing 0 or more persistent instances
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentException">If values and types are not null and their lengths are not equal</exception>
        public IList<T> Find<T>(string queryString, object[] values, IType[] types)
        {
            if (values != null && types != null && values.Length != types.Length)
            {
                throw new ArgumentException("Length of values array must match length of types array");
            }
            return ExecuteFind(new FindHibernateCallback<T>(this, queryString, values, types), true);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding
        /// one value to a  named parameter in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>a generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> FindByNamedParam<T>(string queryName, string paramName, object value)
        {
            return FindByNamedParam<T>(queryName, paramName, value, null);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding
        /// one value to a  named parameter in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">Hibernate type of the parameter (or null)</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> FindByNamedParam<T>(string queryName, string paramName, object value, IType type)
        {
            return FindByNamedParam<T>(queryName, new string[] {paramName}, new object[] {value}, new IType[] {type});
        }

        /// <summary>
        /// Execute a query for persistent instances, binding a
        /// number of values to  named parameters in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> FindByNamedParam<T>(string queryString, string[] paramNames, object[] values)
        {
            return FindByNamedParam<T>(queryString, paramNames, values, null);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding a
        /// number of values to  named parameters in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters</param>
        /// <param name="types">Hibernate types of the parameters (or null)</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentOutOfRangeException">If paramNames length is not equal to values length or
        /// if paramNames length is not equal to types length (when types is not null)</exception>
        public IList<T> FindByNamedParam<T>(string queryString, string[] paramNames, object[] values, IType[] types)
        {
            if (paramNames.Length != values.Length)
            {
                throw new ArgumentOutOfRangeException("paramNames",
                                                      "Length of paramNames array must match length of values array");
            }
            if (types != null && paramNames.Length != types.Length)
            {
                throw new ArgumentOutOfRangeException("paramNames",
                                                      "Length of paramNames array must match length of types array");
            }

            return
                ExecuteFind(new FindByNamedParamHibernateCallback<T>(this, queryString, paramNames, values, types), true);
        }

        /// <summary>
        /// Execute a named query for persistent instances.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> FindByNamedQuery<T>(string queryName)
        {
            return FindByNamedQuery<T>(queryName, (object[]) null, (IType[]) null);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a "?" parameter in the query string.
        ///  A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> FindByNamedQuery<T>(string queryName, object value)
        {
            return FindByNamedQuery<T>(queryName, new object[] {value}, (IType[]) null);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a "?" parameter in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">Hibernate type of the parameter (or null)</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> FindByNamedQuery<T>(string queryName, object value, IType type)
        {
            return FindByNamedQuery<T>(queryName, new object[] {value}, new IType[] {type});
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding a
        /// number of values to "?" parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="values">The values of the parameters</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> FindByNamedQuery<T>(string queryName, object[] values)
        {
            return FindByNamedQuery<T>(queryName, values, (IType[]) null);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding a
        /// number of values to "?" parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="values">The values of the parameters</param>
        /// <param name="types">Hibernate types of the parameters (or null)</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentOutOfRangeException">If values and types are not null and their lengths differ.</exception>
        public IList<T> FindByNamedQuery<T>(string queryName, object[] values, IType[] types)
        {
            if (values != null && types != null && values.Length != types.Length)
            {
                throw new ArgumentOutOfRangeException("Length of values array must match length of types array");
            }
            return ExecuteFind(new FindByNamedQueryHibernateCallback<T>(this, queryName, values, types), true);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a  named parameter in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> FindByNamedQueryAndNamedParam<T>(string queryName, string paramName, object value)
        {
            return FindByNamedQueryAndNamedParam<T>(queryName, paramName, value, null);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a  named parameter in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">The Hibernate type of the parameter (or null)</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> FindByNamedQueryAndNamedParam<T>(string queryName, string paramName, object value, IType type)
        {
            return FindByNamedQueryAndNamedParam<T>(
                queryName, new string[] {paramName}, new object[] {value}, new IType[] {type});
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// number of values to  named parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters.</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> FindByNamedQueryAndNamedParam<T>(string queryName, string[] paramNames, object[] values)
        {
            return FindByNamedQueryAndNamedParam<T>(queryName, paramNames, values, null);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// number of values to  named parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters.</param>
        /// <param name="types">Hibernate types of the parameters (or null)</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentOutOfRangeException">If paramNames length is not equal to values length or
        /// if paramNames length is not equal to types length (when types is not null)</exception>
        public IList<T> FindByNamedQueryAndNamedParam<T>(string queryName, string[] paramNames, object[] values,
                                                         IType[] types)
        {
            if (paramNames != null && values != null && paramNames.Length != values.Length)
            {
                throw new ArgumentOutOfRangeException("paramNames",
                                                      "Length of paramNames array must match length of values array");
            }
            if (paramNames != null && types != null && paramNames.Length != types.Length)
            {
                throw new ArgumentOutOfRangeException("paramNams",
                                                      "Length of paramNames array must match length of types array");
            }
            return
                ExecuteFind(
                    new FindByNamedQueryAndNamedParamHibernateCallback<T>(this, queryName, paramNames, values, types),
                    true);
        }

        /// <summary>
        /// Execute a named query for persistent instances, binding the properties
        /// of the given object to  named parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="valueObject">The values of the parameters</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> FindByNamedQueryAndValueObject<T>(string queryName, object valueObject)
        {
            return
                ExecuteFind(new FindByNamedQueryAndValueObjectHibernateCallback<T>(this, queryName, valueObject), true);
        }

        /// <summary>
        /// Execute a query for persistent instances, binding the properties
        /// of the given object to <i>named</i> parameters in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>
        /// <param name="valueObject">The values of the parameters</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> FindByValueObject<T>(string queryString, object valueObject)
        {
            return ExecuteFind(new FindByValueObjectHibernateCallback<T>(this, queryString, valueObject), true);
        }

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <remarks>
        /// Application exceptions thrown by the action object get propagated to the
        /// caller (can only be unchecked). Hibernate exceptions are transformed into
        /// appropriate DAO ones. Allows for returning the result object.
        /// <p>Note: Callback code is not supposed to handle transactions itself!
        /// Use an appropriate transaction manager like HibernateTransactionManager.
        /// Generally, callback code must not touch any Session lifecycle methods,
        /// like close, disconnect, or reconnect, to let the template do its work.
        /// </p>
        /// </remarks>
        /// <typeparam name="T">The object type retrieved.</typeparam>
        /// <param name="del">The delegate callback object that specifies the Hibernate action.</param>
        /// <returns>a result object returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public T Execute<T>(HibernateDelegate<T> del)
        {
            return Execute<T>(new ExecuteHibernateCallbackUsingDelegate<T>(del));
        }

        /// <summary>
        /// Execute the action specified by the delegate within a Session.
        /// </summary>
        /// <typeparam name="T">The object type retrieved.</typeparam>
        /// <param name="del">The HibernateDelegate that specifies the action
        /// to perform.</param>
        /// <param name="exposeNativeSession">if set to <c>true</c> expose the native hibernate session to
        /// callback code.</param>
        /// <returns>a result object returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public T Execute<T>(HibernateDelegate<T> del, bool exposeNativeSession)
        {
            return Execute<T>(new ExecuteHibernateCallbackUsingDelegate<T>(del), true);
        }

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <typeparam name="T">The object type retrieved.</typeparam>
        /// <param name="action">The callback object that specifies the Hibernate action.</param>
        /// <returns>
        /// a result object returned by the action, or null
        /// </returns>
        /// <remarks>
        /// Application exceptions thrown by the action object get propagated to the
        /// caller (can only be unchecked). Hibernate exceptions are transformed into
        /// appropriate DAO ones. Allows for returning the result object.
        /// <p>Note: Callback code is not supposed to handle transactions itself!
        /// Use an appropriate transaction manager like HibernateTransactionManager.
        /// Generally, callback code must not touch any Session lifecycle methods,
        /// like close, disconnect, or reconnect, to let the template do its work.
        /// </p>
        /// </remarks>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public T Execute<T>(IHibernateCallback<T> action)
        {
            return Execute<T>(action, ExposeNativeSession);
        }

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <typeparam name="T">The object type retrieved.</typeparam>
        /// <param name="action">callback object that specifies the Hibernate action.</param>
        /// <param name="exposeNativeSession">if set to <c>true</c> expose the native hibernate session to
        /// callback code.</param>
        /// <returns>
        /// a result object returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public T Execute<T>(IHibernateCallback<T> action, bool exposeNativeSession)
        {
            ISession session = Session;

            bool existingTransaction = SessionFactoryUtils.IsSessionTransactional(session, SessionFactory);
            if (existingTransaction)
            {
                //log.Debug("Found thread-bound Session for HibernateTemplate");
            }

            FlushModeHolder previousFlushModeHolder = new FlushModeHolder();
            try
            {
                previousFlushModeHolder = ApplyFlushMode(session, existingTransaction);
                ISession sessionToExpose = (exposeNativeSession ? session : classicHibernateTemplate.CreateSessionProxy(session));
                T result = action.DoInHibernate(sessionToExpose);
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
                    //log.Debug("Not closing pre-bound Hibernate Session after HibernateTemplate");
                    if (previousFlushModeHolder.ModeWasSet)
                    {
                        session.FlushMode = previousFlushModeHolder.Mode;
                    }
                }
                else
                {
                    SessionFactoryUtils.ReleaseSession(session, SessionFactory);
                }
            }
        }

        /// <summary>
        /// Execute the action specified by the given action object within a Session assuming that an IList is returned.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="action">callback object that specifies the Hibernate action.</param>
        /// <param name="exposeNativeSession">if set to <c>true</c> expose the native hibernate session to
        /// callback code.</param>
        /// <returns>
        /// an IList returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> ExecuteFind<T>(IFindHibernateCallback<T> action, bool exposeNativeSession)
        {
            ISession session = Session;

            bool existingTransaction = SessionFactoryUtils.IsSessionTransactional(session, SessionFactory);

            FlushModeHolder previousFlushModeHolder = new FlushModeHolder();
            try
            {
                previousFlushModeHolder = ApplyFlushMode(session, existingTransaction);
                ISession sessionToExpose = (exposeNativeSession ? session : classicHibernateTemplate.CreateSessionProxy(session));
                IList<T> result = action.DoInHibernate(sessionToExpose);
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
                    // Callback code throw application exception or other non DB related exception.
                    throw;
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
                    if (previousFlushModeHolder.ModeWasSet)
                    {
                        session.FlushMode = previousFlushModeHolder.Mode;
                    }
                }
                else
                {
                    SessionFactoryUtils.ReleaseSession(session, SessionFactory);
                }
            }
        }

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <remarks>
        /// Application exceptions thrown by the action object get propagated to the
        /// caller (can only be unchecked). Hibernate exceptions are transformed into
        /// appropriate DAO ones. Allows for returning the result object.
        /// <p>Note: Callback code is not supposed to handle transactions itself!
        /// Use an appropriate transaction manager like HibernateTransactionManager.
        /// Generally, callback code must not touch any Session lifecycle methods,
        /// like close, disconnect, or reconnect, to let the template do its work.
        /// </p>
        /// </remarks>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="del">The delegate callback object that specifies the Hibernate action.</param>
        /// <returns>A generic IList returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> ExecuteFind<T>(FindHibernateDelegate<T> del)
        {
            return ExecuteFind<T>(new ExecuteFindHibernateCallbackUsingDelegate<T>(del));
        }

        /// <summary>
        /// Execute the action specified by the delegate within a Session.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="del">The FindHibernateDelegate that specifies the action
        /// to perform.</param>
        /// <param name="exposeNativeSession">if set to <c>true</c> expose the native hibernate session to
        /// callback code.</param>
        /// <returns>A generic IList returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        public IList<T> ExecuteFind<T>(FindHibernateDelegate<T> del, bool exposeNativeSession)
        {
            return ExecuteFind<T>(new ExecuteFindHibernateCallbackUsingDelegate<T>(del), true);
        }

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="action">The callback object that specifies the Hibernate action.</param>
        /// <returns>
        /// A generic IList returned by the action, or null
        /// </returns>
        /// <remarks>
        /// Application exceptions thrown by the action object get propagated to the
        /// caller (can only be unchecked). Hibernate exceptions are transformed into
        /// appropriate DAO ones. Allows for returning the result object.
        /// <p>Note: Callback code is not supposed to handle transactions itself!
        /// Use an appropriate transaction manager like HibernateTransactionManager.
        /// Generally, callback code must not touch any Session lifecycle methods,
        /// like close, disconnect, or reconnect, to let the template do its work.
        /// </p>
        /// </remarks>
        public IList<T> ExecuteFind<T>(IFindHibernateCallback<T> action)
        {
            return ExecuteFind<T>(action, ExposeNativeSession);
        }
    }

    internal class ExecuteHibernateCallbackUsingDelegate<T> : IHibernateCallback<T>
    {
        private HibernateDelegate<T> del;

        public ExecuteHibernateCallbackUsingDelegate(HibernateDelegate<T> d)
        {
            del = d;
        }

        public T DoInHibernate(ISession session)
        {
            return del(session);
        }
    }

    internal class GetByTypeHibernateCallback<T> : IHibernateCallback<T>
    {
        private object id;
        private LockMode lockMode;

        public GetByTypeHibernateCallback(object id, LockMode lockMode)
        {
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
        public T DoInHibernate(ISession session)
        {
            if (lockMode != null)
            {
                return session.Get<T>(id, lockMode);
            }
            else
            {
                return session.Get<T>(id);
            }
        }
    }

    internal class LoadByTypeHibernateCallback<T> : IHibernateCallback<T>
    {
        private object id;
        private LockMode lockMode;

        public LoadByTypeHibernateCallback(object id, LockMode lockMode)
        {
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
        public T DoInHibernate(ISession session)
        {
            if (lockMode != null)
            {
                return session.Load<T>(id, lockMode);
            }
            else
            {
                return session.Load<T>(id);
            }
        }
    }

    internal class LoadAllByTypeHibernateCallback<T> : IFindHibernateCallback<T>
    {
        private HibernateTemplate outer;

        public LoadAllByTypeHibernateCallback(HibernateTemplate template)
        {
            outer = template;
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
        public IList<T> DoInHibernate(ISession session)
        {
            ICriteria criteria = session.CreateCriteria(typeof (T));
            outer.PrepareCriteria(criteria);
            return criteria.List<T>();
        }
    }

    internal class FindHibernateCallback<T> : IFindHibernateCallback<T>
    {
        private HibernateTemplate outer;
        private string queryString;
        private object[] values;
        private IType[] types;

        public FindHibernateCallback(HibernateTemplate template, string queryString, object[] values, IType[] types)
        {
            outer = template;
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
        public IList<T> DoInHibernate(ISession session)
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

            return queryObject.List<T>();
        }
    }

    internal class FindByNamedParamHibernateCallback<T> : IFindHibernateCallback<T>
    {
        HibernateTemplate outer;
        private string queryString;
        private string[] paramNames;
        private object[] values;
        private IType[] types;

        public FindByNamedParamHibernateCallback(HibernateTemplate template, string queryString, string[] paramNames,
                                                 object[] values, IType[] types)
        {
            outer = template;
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
        public IList<T> DoInHibernate(ISession session)
        {
            IQuery queryObject = session.CreateQuery(queryString);
            outer.PrepareQuery(queryObject);
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    outer.ApplyNamedParameterToQuery(queryObject, paramNames[i], values[i],
                                                     (types != null ? types[i] : null));
                }
            }
            return queryObject.List<T>();
        }
    }

    internal class FindByNamedQueryHibernateCallback<T> : IFindHibernateCallback<T>
    {
        HibernateTemplate outer;
        private string queryName;
        private object[] values;
        private IType[] types;

        public FindByNamedQueryHibernateCallback(HibernateTemplate template, string queryName, object[] values,
                                                 IType[] types)
        {
            outer = template;
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
        public IList<T> DoInHibernate(ISession session)
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
            return queryObject.List<T>();
        }
    }

    internal class FindByNamedQueryAndNamedParamHibernateCallback<T> : IFindHibernateCallback<T>
    {
        HibernateTemplate outer;
        private string queryName;
        private string[] paramNames;
        private object[] values;
        private IType[] types;

        public FindByNamedQueryAndNamedParamHibernateCallback(HibernateTemplate template, string queryName,
                                                              string[] paramNames, object[] values, IType[] types)
        {
            outer = template;
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
        public IList<T> DoInHibernate(ISession session)
        {
            IQuery queryObject = session.GetNamedQuery(queryName);
            outer.PrepareQuery(queryObject);
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    outer.ApplyNamedParameterToQuery(queryObject, paramNames[i], values[i],
                                                     (types != null ? types[i] : null));
                }
            }
            return queryObject.List<T>();
        }
    }

    internal class FindByNamedQueryAndValueObjectHibernateCallback<T> : IFindHibernateCallback<T>
    {
        HibernateTemplate outer;
        private string queryName;
        private object valueObject;

        public FindByNamedQueryAndValueObjectHibernateCallback(HibernateTemplate template, string queryName,
                                                               object valueObject)
        {
            outer = template;
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
        public IList<T> DoInHibernate(ISession session)
        {
            IQuery queryObject = session.GetNamedQuery(queryName);
            outer.PrepareQuery(queryObject);
            queryObject.SetProperties(valueObject);
            return queryObject.List<T>();
        }
    }

    internal class FindByValueObjectHibernateCallback<T> : IFindHibernateCallback<T>
    {
        HibernateTemplate outer;
        private string queryString;
        private object valueObject;

        public FindByValueObjectHibernateCallback(HibernateTemplate template, string queryString, object valueObject)
        {
            outer = template;
            this.queryString = queryString;
            this.valueObject = valueObject;
        }

        public IList<T> DoInHibernate(ISession session)
        {
            IQuery queryObject = session.CreateQuery(queryString);
            outer.PrepareQuery(queryObject);
            queryObject.SetProperties(valueObject);
            return queryObject.List<T>();
        }
    }

    internal class ExecuteFindHibernateCallbackUsingDelegate<T> : IFindHibernateCallback<T>
    {
        private FindHibernateDelegate<T> del;

        public ExecuteFindHibernateCallbackUsingDelegate(FindHibernateDelegate<T> d)
        {
            del = d;
        }

        public IList<T> DoInHibernate(ISession session)
        {
            return del(session);
        }
    }
}
