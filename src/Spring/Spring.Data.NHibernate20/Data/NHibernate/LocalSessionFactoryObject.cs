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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using Common.Logging;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Tool.hbm2ddl;

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
	/// <version>$Id: LocalSessionFactoryObject.cs,v 1.2 2008/05/01 12:50:34 lahma Exp $</version>
    public class LocalSessionFactoryObject : IFactoryObject, IInitializingObject, IPersistenceExceptionTranslator, System.IDisposable
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

        private FilterDefinition[] filterDefinitions;

        private IDictionary eventListeners;

        private bool schemaUpdate = false;

        private IAdoExceptionTranslator adoExceptionTranslator;

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
        /// Specify the NHibernate FilterDefinitions to register with the SessionFactory.
        /// This is an alternative to specifying &lt;filter-def&gt; elements in
        /// Hibernate mapping files.
        /// </summary>
        /// <remarks>
        /// Typically, the passed-in FilterDefinition objects will have been defined
        /// as Spring FilterDefinitionFactoryBeans, probably as inner beans within the
        /// LocalSessionFactoryObject definition.
        /// </remarks>
        /// <see cref="FilterDefinitionFactoryObject" />
        public FilterDefinition[] FilterDefinitions
        {
            set { this.filterDefinitions = value; }
        }

        /// <summary>
        /// Specify the NHibernate event listeners to register, with listener types
        /// as keys and listener objects as values.
        /// <p>
        /// Instead of a single listener object, you can also pass in a list
        /// or set of listeners objects as value.
        /// </p>
        /// </summary>
        /// <value>listener objects as values</value>
        /// <remarks>
        /// See the NHibernate documentation for further details on listener types
        /// and associated listener interfaces.
        /// </remarks>
        public IDictionary EventListeners
        {
            set { this.eventListeners = value; }
        }

        /// <summary>
        /// Set whether to execute a schema update after SessionFactory initialization.
	    /// <p>
	    /// For details on how to make schema update scripts work, see the NHibernate
	    /// documentation, as this class leverages the same schema update script support
	    /// in <see cref="Configuration" /> as NHibernate's own SchemaUpdate tool.
	    /// </p>
        /// </summary>
	    public bool SchemaUpdate
	    {
	        set { schemaUpdate = value; }
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
                                   "Spring.Data.NHibernate.SpringSessionContext, Spring.Data.NHibernate20");
            }

            if (this.filterDefinitions != null)
            {
                // Register specified NHibernate FilterDefinitions.
                for (int i = 0; i < this.filterDefinitions.Length; i++)
                {
                    config.AddFilterDefinition(this.filterDefinitions[i]);
                }
            }

            if (this.hibernateProperties != null)
            {
                if (config.GetProperty(Environment.ConnectionProvider) !=null &&
                    hibernateProperties.Contains(Environment.ConnectionProvider))
                {
                    #region Logging
                    if (log.IsInfoEnabled)
                    {
                        log.Info(("Overriding use of Spring's Hibernate Connection Provider with [" +
                                 hibernateProperties[Environment.ConnectionProvider] + "]");
                    }
                    #endregion 
                    config.Properties.Remove(Environment.ConnectionProvider);
                }

                Dictionary<string, string> genericHibernateProperties = new Dictionary<string, string>();
                foreach (DictionaryEntry entry in hibernateProperties)
                {
                    genericHibernateProperties.Add((string) entry.Key, (string) entry.Value);
                }
                config.AddProperties(genericHibernateProperties);
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

            if (this.eventListeners != null) 
            {
				// Register specified NHibernate event listeners.
                foreach (DictionaryEntry entry in eventListeners)
                {
					ListenerType listenerType;
                    try
                    {
                        listenerType = (ListenerType) Enum.Parse(typeof (ListenerType), (string) entry.Key);
                    }
                    catch
                    {
                        throw new ArgumentException(string.Format("Unable to parse string '{0}' as valid {1}", entry.Key, typeof (ListenerType)));
                    }

                    object listenerObject = entry.Value;
					if (listenerObject is ICollection) 
                    {
						ICollection listeners = (ICollection) listenerObject;
						EventListeners listenerRegistry = config.EventListeners;

                        // create the array and check that types are valid at the same time
                        ArrayList items = new ArrayList(listeners);
                        object[] listenerArray = (object[])items.ToArray(listenerRegistry.GetListenerClassFor(listenerType));
						config.SetListeners(listenerType, listenerArray);
					}
					else 
                    {
						config.SetListener(listenerType, listenerObject);
					}
				}
			}


            // Perform custom post-processing in subclasses.
            PostProcessConfiguration(config);

            // Build SessionFactory instance.
            log.Info("Building new Hibernate SessionFactory");
            this.configuration = config;
            this.sessionFactory = NewSessionFactory(config);

            AfterSessionFactoryCreation();
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
        /// Executes schema update if requested.
        /// </summary>
        protected virtual void AfterSessionFactoryCreation()
        {
            if (this.schemaUpdate)
            {
                UpdateDatabaseSchema();
            }
        }
	
        /// <summary>
        /// Execute schema drop script, determined by the Configuration object
        /// used for creating the SessionFactory. A replacement for NHibernate's
        /// SchemaExport class, to be invoked on application setup.
        /// </summary>
        /// <remarks>
        /// Fetch the LocalSessionFactoryBean itself rather than the exposed
        /// SessionFactory to be able to invoke this method, e.g. via
        /// <code>LocalSessionFactoryObject lsfb = (LocalSessionFactoryObject) ctx.GetObject("mySessionFactory");</code>.
        /// <p>
        /// Uses the SessionFactory that this bean generates for accessing a ADO.NET
        /// connection to perform the script.
        /// </p>
        /// </remarks>
        public void DropDatabaseSchema()
        {
		    log.Info("Dropping database schema for NHibernate SessionFactory");
		    HibernateTemplate hibernateTemplate = new HibernateTemplate(sessionFactory);
                hibernateTemplate.Execute(
                    new HibernateDelegate(delegate(ISession session)
                                              {
                                                  IDbConnection con = session.Connection;
                                                  Dialect dialect = Dialect.GetDialect(Configuration.Properties);
                                                  string[] sql = Configuration.GenerateDropSchemaScript(dialect);
                                                  ExecuteSchemaScript(con, sql);
                                                  return null;
                                              }));
                
	    }

        /// <summary>
        /// Execute schema creation script, determined by the Configuration object
        /// used for creating the SessionFactory. A replacement for NHibernate's
        /// SchemaExport class, to be invoked on application setup.
        /// </summary>
        /// <remarks>
        /// Fetch the LocalSessionFactoryObject itself rather than the exposed
        /// SessionFactory to be able to invoke this method, e.g. via
        /// <code>LocalSessionFactoryObject lsfo = (LocalSessionFactoryObject) ctx.GetObject("mySessionFactory");</code>.
        /// <p>
        /// Uses the SessionFactory that this bean generates for accessing a ADO.NET
        /// connection to perform the script.
        /// </p>
        /// </remarks>
        public void CreateDatabaseSchema()
        {
		    log.Info("Creating database schema for Hibernate SessionFactory");
		    HibernateTemplate hibernateTemplate = new HibernateTemplate(sessionFactory);
		    hibernateTemplate.Execute(
			    new HibernateDelegate(delegate (ISession session)
                {
					    IDbConnection con = session.Connection;
					    Dialect dialect = Dialect.GetDialect(Configuration.Properties);
					    string[] sql = Configuration.GenerateSchemaCreationScript(dialect);
					    ExecuteSchemaScript(con, sql);
					    return null;
    			
			    }));		
	    }

        /// <summary>
        /// Execute schema update script, determined by the Configuration object
        /// used for creating the SessionFactory. A replacement for NHibernate's
        /// SchemaUpdate class, for automatically executing schema update scripts
        /// on application startup. Can also be invoked manually.
        /// </summary>
        /// <remarks>
        /// Fetch the LocalSessionFactoryObject itself rather than the exposed
        /// SessionFactory to be able to invoke this method, e.g. via
        /// <code>LocalSessionFactoryObject lsfo = (LocalSessionFactoryObject) ctx.GetObject("mySessionFactory");</code>.
        /// <p>
        /// Uses the SessionFactory that this bean generates for accessing a ADO.NET
        /// connection to perform the script.
        /// </p>
        /// </remarks>
	    public virtual void UpdateDatabaseSchema() 
        {
		    log.Info("Updating database schema for Hibernate SessionFactory");
		    HibernateTemplate hibernateTemplate = new HibernateTemplate(sessionFactory);
		    hibernateTemplate.TemplateFlushMode = TemplateFlushMode.Never;
            hibernateTemplate.Execute(
                new HibernateDelegate(delegate(ISession session)
                                          {
                                              IDbConnection con = session.Connection;
                                              Dialect dialect = Dialect.GetDialect(Configuration.Properties);
                                              DatabaseMetadata metadata = new DatabaseMetadata((DbConnection) con, dialect);
                                              string[] sql = Configuration.GenerateSchemaUpdateScript(dialect, metadata);
                                              ExecuteSchemaScript(con, sql);
                                              return null;
                                          }));
	    }


        /// <summary>
        /// Execute the given schema script on the given ADO.NET Connection.
        /// </summary>
        /// <remarks>
	    /// Note that the default implementation will log unsuccessful statements
	    /// and continue to execute. Override the <code>ExecuteSchemaStatement</code>
	    /// method to treat failures differently.
        /// </remarks>
        /// <param name="con">The connection to use.</param>
        /// <param name="sql">The SQL statement to execute.</param>
        protected virtual void ExecuteSchemaScript(IDbConnection con, string[] sql)
        {
		    if (sql != null && sql.Length > 0) 
            {
			    IDbCommand cmd = con.CreateCommand();
			    try 
                {
				    for (int i = 0; i < sql.Length; i++) 
                    {
					    ExecuteSchemaStatement(cmd, sql[i]);
				    }
			    }
			    finally 
                {
                    AdoUtils.DisposeCommand(cmd);
			    }
    		}
	    }

	    /// <summary>
	    /// Execute the given schema SQL on the given ADO.NET command.
	    /// </summary>
	    /// <remarks>
	    /// Note that the default implementation will log unsuccessful statements
	    /// and continue to execute. Override this method to treat failures differently.
	    /// </remarks>
	    /// <param name="cmd"></param>
	    /// <param name="sql"></param>
        protected virtual void ExecuteSchemaStatement(IDbCommand cmd, string sql)
        {
		    if (log.IsDebugEnabled) 
            {
			    log.Debug("Executing schema statement: " + sql);
		    }
		    try 
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
		    }
		    catch (ADOException ex) 
            {
			    if (log.IsWarnEnabled) 
                {
				    log.Warn("Unsuccessful schema statement: " + sql, ex);
			    }
		    }
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
                return ConvertHibernateException((HibernateException) ex);
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
