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

using System.Collections;
using System.Data;
using Common.Logging;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Connection;
#if NH21
using NHibernate.Engine;
#endif

using Spring.Core.IO;
using Spring.Data.Common;
using Spring.Objects.Factory;
using Environment = NHibernate.Cfg.Environment;

#endregion

namespace Spring.Data.NHibernate
{
	/// <summary>
	/// An IFactoryObject that creates a local Hibernate SessionFactory instance.
    /// Behaves like a SessionFactory instance when used as bean reference,
    /// e.g. for HibernateTemplate's "SessionFactory" property.
	/// </summary>
	/// <remarks>
	/// The typical usage will be to register this as singleton factory 
	/// in an application context and give objects references to application services
	/// that need it.
	/// <para>
	/// Hibernate configuration settings can be set using the IDictionary property 'HibernateProperties'.
	/// 
	/// </para>
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public class LocalSessionFactoryObject : IFactoryObject, IInitializingObject, System.IDisposable
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
		public 	LocalSessionFactoryObject()
		{

		}

		#endregion

		#region Properties

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
            get { return dbProvider;  }
	    }
	   

	    #endregion

		#region Methods

		#endregion

        /// <summary>
        /// Return the singleon session factory.
        /// </summary>
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
	    public void AfterPropertiesSet()
	    {
            // Create Configuration instance.
            Configuration config = NewConfiguration();


            if (this.dbProvider != null)
            {
                config.SetProperty(Environment.ConnectionString, dbProvider.ConnectionString);
                config.SetProperty(Environment.ConnectionProvider, typeof(DbProviderWrapper).AssemblyQualifiedName);
            }

            if (this.hibernateProperties != null)
            {
                if (config.GetProperty(Environment.ConnectionProvider) != null &&
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
                foreach (string resourceName in mappingResources)
                {
                    config.AddInputStream(resourceLoader.GetResource(resourceName).InputStream);
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
        protected virtual ISessionFactory NewSessionFactory(Configuration config) 
        {
            ISessionFactory sf = config.BuildSessionFactory();
            DbProviderWrapper dbProviderWrapper = null;
#if NH21
            ISessionFactoryImplementor sfImplementor = sf as ISessionFactoryImplementor;
            if (sfImplementor != null)
            {
                dbProviderWrapper = sfImplementor.ConnectionProvider as DbProviderWrapper;
            }
#else
            dbProviderWrapper = sf.ConnectionProvider as DbProviderWrapper;
#endif
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

	}
}
