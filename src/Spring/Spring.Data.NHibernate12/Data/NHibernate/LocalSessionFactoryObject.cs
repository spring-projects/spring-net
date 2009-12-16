#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

#region Imports

using System;
using System.Collections;
using System.Data;
using Common.Logging;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Connection;
using Spring.Context;
using Spring.Core.IO;
using Spring.Dao;
using Spring.Dao.Attributes;
using Spring.Dao.Support;
using Spring.Data.Common;
using Spring.Data.Support;
using Spring.Objects.Factory;
using Environment = NHibernate.Cfg.Environment;

#endregion

namespace Spring.Data.NHibernate
{
	/// <summary>
	///  An IFactoryObject that creates a local Hibernate SessionFactory instance.
    /// Behaves like a SessionFactory instance when used as bean reference,
    /// e.g. for HibernateTemplate's "SessionFactory" property.
	/// </summary>
	/// <remarks>
	/// The typical usage will be to register this as singleton factory
	/// in an application context and give objects references to application services
	/// that need it.
	/// <para>
	/// Hibernate configuration settings can be set using the IDictionary property 'HibernateProperties'.
	/// </para>
    /// <para>
    /// This class implements the <see cref="IPersistenceExceptionTranslator"/> interface,
    /// as autodetected by Spring's <see cref="PersistenceExceptionTranslationPostProcessor"/>
    /// for AOP-based translation of PersistenceExceptionTranslationPostProcessor.
    /// Hence, the presence of e.g. LocalSessionFactoryBean automatically enables
    /// a PersistenceExceptionTranslationPostProcessor to translate Hibernate exceptions. 
    /// </para>
    /// </remarks>
	/// <author>Mark Pollack (.NET)</author>
    public class LocalSessionFactoryObject : IFactoryObject, IInitializingObject, IPersistenceExceptionTranslator, System.IDisposable, 
		IApplicationContextAware
	{
		#region Fields

        private Configuration configuration;

	    private ISessionFactory sessionFactory;

        private string[] mappingAssemblies;

        private string[] mappingResources;

	    private string[] configFilenames;

        /// <summary>
        /// TODO: consider changing to NamevalueCollection for easier
        /// cut-n-paste from existing App.config based configurations.
        /// </summary>
        private IDictionary hibernateProperties;

        private IDbProvider dbProvider;

	    private bool exposeTransactionAwareSessionFactory = false;
        
        private IAdoExceptionTranslator adoExceptionTranslator;

		private IResourceLoader resourceLoader;
		
		private IApplicationContext applicationContext;


		#endregion

		#region Constants

		/// <summary>
		/// The shared <see cref="ILog"/> instance for this class (and derived classes).
		/// </summary>
		protected static readonly ILog log =
			LogManager.GetLogger(typeof (LocalSessionFactoryObject));



	    #endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="LocalSessionFactoryObject"/> class.
        /// </summary>
		public LocalSessionFactoryObject()
		{

		}

		#endregion

		#region Properties

		/// <summary>
		/// Setting the Application Context determines were resources are loaded from
		/// </summary>
		public IApplicationContext ApplicationContext
		{
			set { this.applicationContext = value; }
#if NET_1_1
            get { return this.applicationContext; }
#else
            protected get { return this.applicationContext; }
#endif
		}

		/// <summary>
		/// Gets or sets the <see cref="IResourceLoader"/> to use for loading mapping assemblies etc.
		/// </summary>
		public IResourceLoader ResourceLoader
		{
			get
			{
				if (resourceLoader == null)
				{
					resourceLoader = new ConfigurableResourceLoader();
				}
				return resourceLoader;
			}
			set { resourceLoader = value; }
		}



        /// <summary>
        /// Sets the assemblies to load that contain mapping files.
        /// </summary>
        /// <value>The mapping assemblies.</value>
	    public string[] MappingAssemblies
	    {
	        set { mappingAssemblies = value; }
	    }

        /// <summary>
        /// Sets the hibernate configuration files to load, i.e. hibernate.cfg.xml.
        /// </summary>
	    public string[] ConfigFilenames
	    {
	        set { configFilenames = value; }
	    }

	    /// <summary>
        /// Sets the locations of Spring IResources that contain mapping
        /// files.
        /// </summary>
        /// <value>The location of mapping resources.</value>
	    public string[] MappingResources
	    {
	        set { mappingResources = value;}
	    }

        /// <summary>
        /// Return the Configuration object used to build the SessionFactory.
        /// Allows access to configuration metadata stored there (rarely needed).
        /// </summary>
        /// <value>The hibernate configuration.</value>
	    public Configuration Configuration
	    {
	        get { return configuration; }
	    }

        /// <summary>
        /// Set NHibernate configuration properties, like "hibernate.dialect".
        /// </summary>
        /// <value>The hibernate properties.</value>
        /// <remarks>
        /// 	<p>Can be used to override values in a NHibernate XML config file,
        /// or to specify all necessary properties locally.
        /// </p>
        /// 	<p>Note: Do not specify a transaction provider here when using
        /// Spring-driven transactions. It is also advisable to omit connection
        /// provider settings and use a Spring-set IDbProvider instead.
        /// </p>
        /// </remarks>
	    public IDictionary HibernateProperties
	    {
	        get
	        {
	            if (hibernateProperties == null)
	            {
	                hibernateProperties = new Hashtable();
	            }
                return hibernateProperties;
	        }
	        set
	        {
	            hibernateProperties = value;
	        }
	    }

        /// <summary>
        /// Get or set the DataSource to be used by the SessionFactory.
        /// </summary>
        /// <value>The db provider.</value>
        /// <remarks>
        /// If set, this will override corresponding settings in Hibernate properties.
        /// <note>Note: If this is set, the Hibernate settings should not define
        /// a connection string
        /// (hibernate.connection.connection_string) to avoid meaningless double configuration.
        /// </note>
        /// </remarks>
	    public IDbProvider DbProvider
	    {
	        set { dbProvider = value; }
            get { return dbProvider; }
	    }

        /// <summary>
        /// Gets or sets a value indicating whether to expose a transaction aware session factory.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if want to expose transaction aware session factory; otherwise, <c>false</c>.
        /// </value>
	    public bool ExposeTransactionAwareSessionFactory
	    {
	        set { exposeTransactionAwareSessionFactory = value; }
            get { return exposeTransactionAwareSessionFactory; }
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
        public virtual IAdoExceptionTranslator AdoExceptionTranslator
        {
            set { adoExceptionTranslator = value; }
            get
            {
                if (adoExceptionTranslator == null)
                {
                    adoExceptionTranslator = SessionFactoryUtils.NewAdoExceptionTranslator(sessionFactory);
                }
                return adoExceptionTranslator;
            }
        }

	    #endregion

	    #region Methods

	    #endregion

	    /// <summary>
        /// Return the singleon session factory.
        /// </summary>
        /// <returns>The singleon session factory.</returns>
	    public object GetObject()
	    {
	        return sessionFactory;
	    }

        /// <summary>
        /// Return the type <see cref="ISessionFactory"/> or subclass.
        /// </summary>
        /// <value>The type created by this factory</value>
	    public System.Type ObjectType
	    {
	        get
	        {
                return (sessionFactory != null) ? sessionFactory.GetType() : typeof(ISessionFactory);
	        }
	    }

        /// <summary>
        /// Returns true
        /// </summary>
        /// <value>true</value>
	    public bool IsSingleton
	    {
            get
            {
                return true;
            }
	    }

        /// <summary>
        /// Initialize the SessionFactory for the given or the
        /// default location.
        /// </summary>
	    public virtual void AfterPropertiesSet()
	    {
            // Create Configuration instance.
            Configuration config = NewConfiguration();


            if (this.dbProvider != null)
            {
                config.SetProperty(Environment.ConnectionString,
                                   dbProvider.ConnectionString);
                config.SetProperty(Environment.ConnectionProvider, typeof(DbProviderWrapper).AssemblyQualifiedName);

            }

            if (ExposeTransactionAwareSessionFactory)
            {
                // Set ICurrentSessionContext implementation,
                // providng the Spring-managed ISession s current Session.
                // Can be overridden by a custom value for the corresponding Hibernate property
                config.SetProperty(Environment.CurrentSessionContextClass,
                                   "Spring.Data.NHibernate.SpringSessionContext, Spring.Data.NHibernate12");
            }

            if (this.hibernateProperties != null)
            {
                if (config.GetProperty(Environment.ConnectionProvider) !=null &&
                    hibernateProperties.Contains(Environment.ConnectionProvider))
                {
                    #region Logging
                    if (log.IsInfoEnabled)
                    {
                        log.Info("Overriding use of Spring's Hibernate Connection Provider with [" +
                                 hibernateProperties[Environment.ConnectionProvider] + "]");
                    }
                    #endregion 
                    config.Properties.Remove(Environment.ConnectionProvider);
                }
                config.AddProperties(hibernateProperties);
            }
            if (this.mappingAssemblies != null)
            {
                foreach (string assemblyName in mappingAssemblies)
                {
                    config.AddAssembly(assemblyName);
                }
            }

            
            IResourceLoader resourceLoader = new ConfigurableResourceLoader();

            if (this.mappingResources != null)
            {
				IResourceLoader loader = this.ResourceLoader;
				if (loader == null)
				{
					loader = this.applicationContext;
				}

                foreach (string resourceName in mappingResources)
                {
                    config.AddInputStream(loader.GetResource(resourceName).InputStream);
                }
            }

            if (configFilenames != null)
            {
                foreach (string configFilename in configFilenames)
                {
                    config.Configure(configFilename);
                }
            }

            // Perform custom post-processing in subclasses.
            PostProcessConfiguration(config);

            // Build SessionFactory instance.
            log.Info("Building new Hibernate SessionFactory");
            this.configuration = config;
            this.sessionFactory = NewSessionFactory(config);


	    }

	    /// <summary>
        /// Close the SessionFactory on application context shutdown.
        /// </summary>
	    public void Dispose()
	    {
            if (sessionFactory != null)
            {
                #region Instrumentation
                if (log.IsInfoEnabled)
                {
                    log.Info("Closing Hibernate SessionFactory");
                }  
                #endregion                  
                sessionFactory.Close();
            }

	    }

        /// <summary>
        /// Subclasses can override this method to perform custom initialization
        /// of the Configuration instance used for ISessionFactory creation.
        /// </summary>
        /// <remarks>
        /// The properties of this LocalSessionFactoryObject will be applied to
        /// the Configuration object that gets returned here.
        /// <p>The default implementation creates a new Configuration instance.
        /// A custom implementation could prepare the instance in a specific way,
        /// or use a custom Configuration subclass.
        /// </p>
        /// </remarks>
        /// <returns>The configuration instance.</returns>
        protected virtual Configuration NewConfiguration()
        {
            return new Configuration();
        }

        /// <summary>
        /// To be implemented by subclasses that want to to perform custom
        /// post-processing of the Configuration object after this FactoryObject
        /// performed its default initialization.
        /// </summary>
        /// <param name="config">The current configuration object.</param>
        protected virtual void PostProcessConfiguration(Configuration config)
        {
        }

        /// <summary>
        /// Subclasses can override this method to perform custom initialization
        /// of the SessionFactory instance, creating it via the given Configuration
        /// object that got prepared by this LocalSessionFactoryObject.
        /// </summary>
        /// <remarks>
        /// <p>The default implementation invokes Configuration's BuildSessionFactory.
        /// A custom implementation could prepare the instance in a specific way,
        /// or use a custom ISessionFactory subclass.
        /// </p>
        /// </remarks>
        /// <returns>The ISessionFactory instance.</returns>
        protected virtual ISessionFactory NewSessionFactory(Configuration config)
        {
            ISessionFactory sf = config.BuildSessionFactory();
            DbProviderWrapper dbProviderWrapper = sf.ConnectionProvider as DbProviderWrapper;
            if (dbProviderWrapper != null)
            {
                dbProviderWrapper.DbProvider = dbProvider;
            }
            return sf;
        }

        #region DbProviderWrapper Helper class

        internal class DbProviderWrapper : ConnectionProvider
        {
            private IDbProvider _dbProvider;

            public DbProviderWrapper()
            {
            }

            public IDbProvider DbProvider
            {
                get { return _dbProvider; }
                set { _dbProvider = value; }
            }

            public override void CloseConnection(IDbConnection conn)
            {
                base.CloseConnection(conn);
                conn.Dispose();
            }

            public override IDbConnection GetConnection()
            {
                IDbConnection dbCon = _dbProvider.CreateConnection();
                dbCon.Open();
                return dbCon;
            }
        }

        #endregion // DbProviderWrapper Helper class

        #region IPersistenceExceptionTranslator Members

        /// <summary>
        /// Implementation of the PersistenceExceptionTranslator interface,
        /// as autodetected by Spring's PersistenceExceptionTranslationPostProcessor.
        /// Converts the exception if it is a HibernateException;
        /// else returns <code>null</code> to indicate an unknown exception.
        /// translate the given exception thrown by a persistence framework to a
        /// corresponding exception from Spring's generic DataAccessException hierarchy,
        /// if possible.
        /// </summary>
        /// <param name="ex">The exception thrown.</param>
        /// <returns>
        /// the corresponding DataAccessException (or <code>null</code> if the
        /// exception could not be translated.
        /// </returns>
        /// <seealso cref="PersistenceExceptionTranslationPostProcessor"/>
        public DataAccessException TranslateExceptionIfPossible(Exception ex)
        {
            if (ex is HibernateException)
            {
                return ConvertHibernateException((HibernateException)ex);
            }
            return null;
        }

        /// <summary>
        /// Convert the given HibernateException to an appropriate exception from the
        /// Spring's DAO Exception hierarchy.
        /// Will automatically apply a specified IAdoExceptionTranslator to a
        /// Hibernate ADOException, else rely on Hibernate's default translation.
        /// </summary>
        /// <param name="ex">The Hibernate exception that occured.</param>
        /// <returns>A corresponding DataAccessException</returns>
        protected virtual DataAccessException ConvertHibernateException(HibernateException ex)
        {
            if (ex is ADOException)
            {
                return ConvertAdoAccessException((ADOException)ex);
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

            string sqlString = (ex.SqlString != null)
                ? ex.SqlString.ToString()
                : string.Empty;
            return AdoExceptionTranslator.Translate(
                "Hibernate operation: " + ex.Message, sqlString, ex.InnerException);

        }

        #endregion
	}
}
