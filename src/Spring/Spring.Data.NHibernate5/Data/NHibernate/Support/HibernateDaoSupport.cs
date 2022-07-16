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
using Spring.Dao;
using Spring.Dao.Support;

namespace Spring.Data.NHibernate.Support
{
	/// <summary>
    ///  Convenient super class for Hibernate data access objects.
	/// </summary>
	/// <remarks>
	/// <para>Requires a SessionFactory to be set, providing a HibernateTemplate
    ///  based on it to subclasses. Can alternatively be initialized directly with
    /// a HibernateTemplate, to reuse the latter's settings such as the SessionFactory,
    ///  exception translator, flush mode, etc
    /// </para>
    /// This base call is mainly intended for HibernateTemplate usage.
    /// <para>
    /// This class will create its own HibernateTemplate if only a SessionFactory
    /// is passed in. The "allowCreate" flag on that HibernateTemplate will be "true"
    /// by default. A custom HibernateTemplate instance can be used through overriding
    /// <code>CreateHibernateTemplate</code>.
    /// </para>
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public abstract class HibernateDaoSupport : DaoSupport
	{
	    private HibernateTemplate hibernateTemplate;

	    /// <summary>
		/// Initializes a new instance of the <see cref="HibernateDaoSupport"/> class.
                /// </summary>
		public 	HibernateDaoSupport()
		{

		}

	    /// <summary>
        /// Gets or sets the hibernate template.
        /// </summary>
        /// <remarks>Set the HibernateTemplate for this DAO explicitly,
        /// as an alternative to specifying a SessionFactory.
        /// </remarks>
        /// <value>The hibernate template.</value>
	    public HibernateTemplate HibernateTemplate
	    {
	        get
	        {
	            return hibernateTemplate;
	        }
	        set
	        {
	            hibernateTemplate = value;
	        }
	    }

        /// <summary>
        /// Gets or sets the session factory to be used by this DAO.
        /// Will automatically create a HibernateTemplate for the given SessionFactory.
        /// </summary>
        /// <value>The session factory.</value>
	    public ISessionFactory SessionFactory
        {
            get
            {
                return (this.hibernateTemplate != null ? this.hibernateTemplate.SessionFactory : null);
            }
            set
            {
                hibernateTemplate = CreateHibernateTemplate(value);
            }
        }
	    
        /// <summary>
        /// Get a Hibernate Session, either from the current transaction or a new one.
        /// The latter is only allowed if the "allowCreate" setting of this object's
        /// HibernateTemplate is true.
        /// </summary>
        /// <remarks>
        /// <p><b>Note that this is not meant to be invoked from HibernateTemplate code
        /// but rather just in plain Hibernate code.</b> Use it in combination with
        /// <b>ReleaseSession</b>.
        /// </p>
        /// <p>In general, it is recommended to use HibernateTemplate, either with
        /// the provided convenience operations or with a custom HibernateCallback
        /// that provides you with a Session to work on. HibernateTemplate will care
        /// for all resource management and for proper exception conversion.
        /// </p>
        /// </remarks>
        /// <value>The Hibernate session.</value>
	    public ISession Session
	    {
	        get
	        {
                return DoGetSession(HibernateTemplate.AllowCreate);
	        }
	    }

	    /// <summary>
        /// Create a HibernateTemplate for the given ISessionFactory.
        /// </summary>
        /// <remarks>
        /// Only invoked if populating the DAO with a ISessionFactory reference!
        /// <p>Can be overridden in subclasses to provide a HibernateTemplate instance
        /// with different configuration, or a custom HibernateTemplate subclass.
        /// </p>
        /// </remarks>
        protected virtual HibernateTemplate CreateHibernateTemplate(ISessionFactory sessionFactory) 
        {
            return new HibernateTemplate(sessionFactory);
        }

        /// <summary>
        /// Check if the hibernate template property has been set.
        /// </summary>
        protected override void CheckDaoConfig()
        {
            if (this.hibernateTemplate == null)
            {
                throw new ArgumentException("sessionFactory or hibernateTemplate is required");
            }
        }

        /// <summary>
        /// Get a Hibernate Session, either from the current transaction or
        /// a new one. The latter is only allowed if "allowCreate" is true.
        /// </summary>
        /// <remarks>Note that this is not meant to be invoked from HibernateTemplate code
        /// but rather just in plain Hibernate code. Either rely on a thread-bound
        /// Session (via HibernateInterceptor), or use it in combination with
        /// ReleaseSession.
        /// <para>
        /// In general, it is recommended to use HibernateTemplate, either with
        /// the provided convenience operations or with a custom HibernateCallback
        /// that provides you with a Session to work on. HibernateTemplate will care
        /// for all resource management and for proper exception conversion.
        /// </para>
        /// </remarks>
        /// <param name="allowCreate"> if a non-transactional Session should be created when no
	    /// transactional Session can be found for the current thread
	    /// </param>
        /// <returns>Hibernate session.</returns>
        /// <exception cref="DataAccessResourceFailureException">
        /// If the Session couldn't be created
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// if no thread-bound Session found and allowCreate false
        /// </exception>
        /// <seealso cref="ReleaseSession"/>
        protected ISession DoGetSession(bool allowCreate)
        {
            return (!allowCreate ?
                SessionFactoryUtils.GetSession(SessionFactory, false) :
                    SessionFactoryUtils.GetSession(
                            SessionFactory,
                            this.hibernateTemplate.EntityInterceptor,
                            this.hibernateTemplate.AdoExceptionTranslator));
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
        protected DataAccessException ConvertHibernateAccessException(HibernateException ex)
        {
            return hibernateTemplate.ConvertHibernateAccessException(ex);
        }


        /// <summary>
        /// Close the given Hibernate Session, created via this DAO's SessionFactory,
        /// if it isn't bound to the thread.
        /// </summary>
        /// <remarks>
        /// Typically used in plain Hibernate code, in combination with the 
        /// Session property and ConvertHibernateAccessException.
        /// </remarks>      
        /// <param name="session">The session to close.</param>
    	protected void ReleaseSession(ISession session) {
		    SessionFactoryUtils.ReleaseSession(session, SessionFactory);
	    }
	}
}
