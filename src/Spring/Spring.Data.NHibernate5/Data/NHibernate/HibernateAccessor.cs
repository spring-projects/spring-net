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
using System.Reflection;
using AopAlliance.Intercept;
using Common.Logging;
using NHibernate;
using NHibernate.Impl;
using NHibernate.Type;
using Spring.Core.TypeResolution;
using Spring.Dao;
using Spring.Data.Support;
using Spring.Objects.Factory;
using IInterceptor=NHibernate.IInterceptor;
using ICriteria=NHibernate.ICriteria;

namespace Spring.Data.NHibernate
{
	/// <summary>
    /// Base class for HibernateTemplate defining common
    /// properties like SessionFactory and flushing behavior.
	/// </summary>
	/// <remarks>
	/// <p>Not intended to be used directly. See HibernateTemplate.
	/// </p>
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public abstract class HibernateAccessor : IInitializingObject, IObjectFactoryAware
	{

        private Type criteriaType;

	    /// <summary>
		/// The <see cref="ILog"/> instance for this class. 
		/// </summary>
		private readonly ILog log = LogManager.GetLogger(typeof (HibernateAccessor));

	    /// <summary>
		/// Initializes a new instance of the <see cref="HibernateAccessor"/> class.
        /// </summary>
		public HibernateAccessor()
		{
	        
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
        public abstract bool AllowCreate
        {
            get;
            set;
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
        public abstract bool AlwaysUseNewSession
        {
            get;
            set;
        }

        /// <summary>
        /// Set whether to expose the native Hibernate Session to IHibernateCallback
        /// code. Default is "false": a Session proxy will be returned,
        /// suppressing <code>close</code> calls and automatically applying
        /// query cache settings and transaction timeouts.
        /// </summary>
        /// <value><c>true</c> if expose native session; otherwise, <c>false</c>.</value>
        public abstract bool ExposeNativeSession
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the template flush mode.
        /// </summary>
        /// <remarks>
        /// Default is Auto. Will get applied to any <b>new</b> ISession
        /// created by the template.
        /// </remarks>
        /// <value>The template flush mode.</value>
        public abstract TemplateFlushMode TemplateFlushMode
        {
            get;
            set;
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
        public abstract IInterceptor EntityInterceptor
        {
            get;
            set;
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
        public abstract string EntityInterceptorObjectName
        {
            set;
        }

        /// <summary>
        /// Gets or sets the session factory that should be used to create
        /// NHibernate ISessions.
        /// </summary>
        /// <value>The session factory.</value>
        public abstract ISessionFactory SessionFactory
        {
            get;
            set;
        }

        /// <summary>
        /// Set the object factory instance.
        /// </summary>
        /// <value>The object factory instance.</value>
        public abstract IObjectFactory ObjectFactory
        {
            set;
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
        public abstract bool CacheQueries
        {
            get;
            set;
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
        public abstract string QueryCacheRegion
        {
            get;
            set;
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
        public abstract int FetchSize
        {
            get;
            set;
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
        public abstract int MaxResults
        {
            get;
            set;
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
        public abstract IAdoExceptionTranslator AdoExceptionTranslator
        {
            set;
            get;
        }
	    
        /// <summary>
        /// Gets a Session for use by this template.
        /// </summary>
        /// <value>The session.</value>
        /// <remarks>
        /// - Returns a new Session in case of "alwaysUseNewSession" (using the same ADO.NET connection as a transaction Session, if applicable)
        /// - a pre-bound Session in case of "AllowCreate" is set to false (not the default) 
        /// - or a pre-bound Session or new Session if no transactional or other pre-bound Session exists.
        /// </remarks>
        protected ISession Session
        {
            get
            {
                if (AlwaysUseNewSession)
                {
                    return SessionFactoryUtils.GetNewSession(SessionFactory, EntityInterceptor);
                }
                else if (!AllowCreate)
                {
                    return SessionFactoryUtils.GetSession(SessionFactory, false);
                }
                else
                {
                    return SessionFactoryUtils.GetSession(
                        SessionFactory, EntityInterceptor, AdoExceptionTranslator);
                }

            }

        }

	    /// <summary>
        /// Apply the flush mode that's been specified for this accessor
        /// to the given Session.
        /// </summary>
        /// <param name="session">The current Hibernate Session.</param>
        /// <param name="existingTransaction">if set to <c>true</c>
        /// if executing within an existing transaction.</param>
        /// <returns>
        /// the previous flush mode to restore after the operation,
        /// or <code>null</code> if none
        /// </returns>
        protected FlushModeHolder ApplyFlushMode(ISession session, bool existingTransaction) 
        {
            if (TemplateFlushMode == TemplateFlushMode.Never) 
            {
                if (existingTransaction) 
                {
                    FlushMode previousFlushMode = session.FlushMode;
                    if (previousFlushMode != FlushMode.Never) 
                    {
                        session.FlushMode = FlushMode.Never;
                        return new FlushModeHolder(previousFlushMode);
                    }
                }
                else 
                {
                    session.FlushMode = FlushMode.Never;
                }
            }
            else if (TemplateFlushMode == TemplateFlushMode.Eager) 
            {
                if (existingTransaction) 
                {
                    FlushMode previousFlushMode = session.FlushMode;
                    if (previousFlushMode != FlushMode.Auto) 
                    {
                        session.FlushMode = FlushMode.Auto;
                        return new FlushModeHolder(previousFlushMode);
                    }
                }
                else 
                {
                    // rely on default FlushMode.AUTO
                }
            }
            else if (TemplateFlushMode == TemplateFlushMode.Commit) 
            {
                if (existingTransaction) 
                {
                    FlushMode previousFlushMode = session.FlushMode;
                    if (previousFlushMode == FlushMode.Auto) 
                    {
                        session.FlushMode = FlushMode.Commit;
                        return new FlushModeHolder(previousFlushMode);
                    }
                }
                else 
                {
                    session.FlushMode = FlushMode.Commit;
                }
            }
            return new FlushModeHolder();
        }

	
        /// <summary>
        ///  Flush the given Hibernate Session if necessary.
        /// </summary>
        /// <param name="session">The current Hibernate Session.</param>
        /// <param name="existingTransaction">if set to <c>true</c> 
        /// if executing within an existing transaction.</param>
        protected void FlushIfNecessary(ISession session, bool existingTransaction)
        {
            if (TemplateFlushMode == TemplateFlushMode.Eager ||
                (!existingTransaction && TemplateFlushMode != TemplateFlushMode.Never)) 
            {
                log.Debug("Eagerly flushing Hibernate session");
                session.Flush();
            }
        }

        /// <summary>
        /// Convert the given HibernateException to an appropriate exception from the
        /// <code>org.springframework.dao</code> hierarchy. Will automatically detect
        /// wrapped ADO.NET Exceptions and convert them accordingly.
        /// </summary>
        /// <param name="ex">HibernateException that occured.</param>
        /// <returns>
        /// The corresponding DataAccessException instance
        /// </returns>
        /// <remarks>
        /// The default implementation delegates to SessionFactoryUtils
        /// and convertAdoAccessException. Can be overridden in subclasses.
        /// </remarks>
        public virtual DataAccessException ConvertHibernateAccessException(HibernateException ex)
        {
            if (ex is ADOException)
            {
                return ConvertAdoAccessException((ADOException) ex);
            }   
            return SessionFactoryUtils.ConvertHibernateAccessException(ex);
        }

        /// <summary>
        /// Converts the ADO.NET access exception to an appropriate exception from the
        /// <code>org.springframework.dao</code> hierarchy. Can be overridden in subclasses.
        /// </summary>
        /// <param name="ex">ADOException that occured, wrapping underlying ADO.NET exception.</param>
        /// <returns>
        /// the corresponding DataAccessException instance
        /// </returns>
        protected virtual DataAccessException ConvertAdoAccessException(ADOException ex)
        {
            return SessionFactoryUtils.ConvertAdoAccessException(AdoExceptionTranslator, ex);         
        }

        /// <summary>
        /// Converts the ADO.NET access exception to an appropriate exception from the
        /// <code>org.springframework.dao</code> hierarchy. Can be overridden in subclasses.
        /// </summary>
        /// <remarks>
        /// <note>
        /// Note that a direct SQLException can just occur when callback code
        /// performs direct ADO.NET access via <code>ISession.Connection()</code>.
        /// </note>
        /// </remarks>
        /// <param name="ex">The ADO.NET exception.</param>
        /// <returns>The corresponding DataAccessException instance</returns>
        protected virtual DataAccessException ConvertAdoAccessException(Exception ex) 
        {           
            return AdoExceptionTranslator.Translate("Hibernate operation", null, ex);
        }

	    
        /// <summary>
        /// Prepare the given IQuery object, applying cache settings and/or
        /// a transaction timeout.
        /// </summary>
        /// <param name="queryObject">The query object to prepare.</param>
        public virtual void PrepareQuery(IQuery queryObject)
        {
            if (CacheQueries)
            {
                queryObject.SetCacheable(true);
                if (QueryCacheRegion != null)
                {
                    queryObject.SetCacheRegion(QueryCacheRegion);
                }
            }
                      
            if (FetchSize > 0)
            {
                AbstractQueryImpl queryImpl = queryObject as AbstractQueryImpl;
                if (queryImpl != null)
                {
                    queryImpl.SetFetchSize(FetchSize);
                }
                else
                {
                    log.Warn("Could not set FetchSize for IQuery.  Expected Implemention to be of type AbstractQueryImpl");
                }
            }
            
            if (MaxResults > 0)
            {
                queryObject.SetMaxResults(MaxResults);
            }

            SessionFactoryUtils.ApplyTransactionTimeout(queryObject, SessionFactory);

        }

        /// <summary>
        /// Apply the given name parameter to the given Query object.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">The NHibernate type of the parameter (or <code>null</code> if none specified)</param>
        public virtual void ApplyNamedParameterToQuery(IQuery queryObject, string paramName, object value, IType type)
        {

            if (value is ICollection)
            {
                if (type != null)
                {
                    queryObject.SetParameterList(paramName, (ICollection)value, type);
                }
                else
                {
                    queryObject.SetParameterList(paramName, (ICollection)value);
                }
            }
            else if (value is Object[])
            {

                //TODO investigate support for this conversion.
                if (type != null)
                {
                    queryObject.SetParameterList(paramName, (Object[])value, type);
                }
                else
                {
                    queryObject.SetParameterList(paramName, (Object[])value);
                }
            }
            else
            {
                if (type != null)
                {
                    queryObject.SetParameter(paramName, value, type);
                }
                else
                {
                    queryObject.SetParameter(paramName, value);
                }
            }
        }

        /// <summary>
        /// Prepare the given Criteria object, applying cache settings and/or
        /// a transaction timeout.
        /// </summary>
        /// <remarks>
        /// <para>Note that for NHibernate 1.2 this only works if the
        /// implementation is of the type CriteriaImpl, which should generally
        /// be the case.  The SetFetchSize method is not available on the
        /// ICriteria interface
        /// </para>
        /// <para>This is a no-op for NHibernate 1.0.x since
        /// the SetFetchSize method is not on the ICriteria interface and
        /// the implementation class is has internal access.
        /// </para>
        /// <para>To remove the method completely for Spring's NHibernate 1.0 
        /// support while reusing code for NHibernate 1.2 would not be 
        /// possible.  So now this ineffectual operation is left in tact for
        /// NHibernate 1.0.2 support.</para>
        /// </remarks>
        /// <param name="criteria">The criteria object to prepare</param>
        public void PrepareCriteria(ICriteria criteria)
        {
            if (CacheQueries)
            {
                criteria.SetCacheable(true);
                if (QueryCacheRegion != null)
                {
                    criteria.SetCacheRegion(QueryCacheRegion);
                }
            }



            if (FetchSize > 0)
            {
                //TODO see if we can optimize performance.
                //CriteriaImpl is internal in NH 1.0.x
                object[] args = new object[] { FetchSize };
                try
                {
                    Type t = TypeResolutionUtils.ResolveType("NHibernate.Impl.CriteriaImpl, NHibernate");
                    if (t != null)
                    {
                        t.InvokeMember("SetFetchSize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                                                       | BindingFlags.InvokeMethod,
                                       null, criteria, args);
                    }
                }
                catch (TypeLoadException e)
                {
                    log.Warn("Can't set FetchSize for ICriteria", e);
                }
            }
            
            if (MaxResults > 0)
            {
                criteria.SetMaxResults(MaxResults);
            }

            SessionFactoryUtils.ApplyTransactionTimeout(criteria, SessionFactory);
        }
	    
	    private void InitCriteriaType()
	    {
	                    
            try
            {
                criteriaType = TypeResolutionUtils.ResolveType("Hibernate.Impl.CriteriaImpl, NHibernate");
            }
            catch (TypeLoadException e)
            {
                log.Warn("CriteriaImpl not available. FetchSize can not be set on ICriteria objects", e);
            }
	    }

	    /// <summary>
        /// Ensure SessionFactory is not null
        /// </summary>
        /// <exception cref="ArgumentException">If SessionFactory property is null.</exception>
	    public virtual void AfterPropertiesSet()
	    {
            if (SessionFactory == null) 
            {
                throw new ArgumentException("sessionFactory is required");
            }
	    }

	    /// <summary>
        /// Helper class to determine if the FlushMode enumeration
        /// was changed from its default value
        /// </summary>
        protected class FlushModeHolder
        {
            /// <summary>
            /// Gets or sets a value indicating whether the FlushMode
            /// property was set..
            /// </summary>
            /// <value><c>true</c> if FlushMode was set; otherwise, <c>false</c>.</value>
            public bool ModeWasSet
            {
                get { return modeWasSet; }
                set { modeWasSet = value; }
            }

            /// <summary>
            /// Gets or sets the FlushMode.
            /// </summary>
            /// <value>The FlushMode.</value>
            public FlushMode Mode
            {
                get { return flushMode; }
                set { flushMode = value; }
            }

            private bool modeWasSet = false;
            private FlushMode flushMode;

            /// <summary>
            /// Initializes a new instance of the <see cref="FlushModeHolder"/> class.
            /// </summary>
            public FlushModeHolder()
            {                
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FlushModeHolder"/> class.
            /// </summary>
            /// <param name="mode">The flush mode.</param>
            public FlushModeHolder(FlushMode mode)
            {
                Mode = mode;
                ModeWasSet = true;
            }

        }
	}

    internal class CloseSuppressingMethodInterceptor : IMethodInterceptor
    {
        private HibernateAccessor hibernateAccessor;

        public CloseSuppressingMethodInterceptor(HibernateAccessor accessor)
        {
            hibernateAccessor = accessor;
        }

        public object Invoke(IMethodInvocation invocation)
        {
            //anything special for equals/hashcode?
            if (invocation.Method.Name.Equals("Close"))
            {
                return null;
            }
            else
            {
                object retValue = invocation.Proceed();
                if (retValue is IQuery)
                {
                    hibernateAccessor.PrepareQuery((IQuery)retValue);
                }
                if (retValue is ICriteria)
                {
                    hibernateAccessor.PrepareCriteria((ICriteria)retValue);
                }
                return retValue;
            }
        }
    }
}
