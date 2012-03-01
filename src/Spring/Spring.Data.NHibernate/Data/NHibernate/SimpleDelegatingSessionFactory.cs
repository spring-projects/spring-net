using System.Collections;

using NHibernate;
using NHibernate.Cfg;
using NhCfg = NHibernate.Cfg;

using Spring.Collections;
using Spring.Threading;
using Spring.Data.Common;
using Spring.Context.Support;
using NHibernate.Engine;
using System;

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// SimpleDelegatingSessionFactory class
    /// </summary>
    public class SimpleDelegatingSessionFactory : DelegatingSessionFactory
    {
        /// <summary>
        /// Connection string config element name
        /// </summary>
        public const string CONNECTION_STRING = "SimpleDelegatingSessionFactory.ConnectionString";

        private Configuration _configuration;

        private string _defaultConnectionString;

        private object _monitor = new object();

        private IDictionary _targetSessionFactories = new SynchronizedHashtable();

        /// <summary>
        /// public Constructor
        /// </summary>
        /// <param name="defaultConfiguration"></param>
        public SimpleDelegatingSessionFactory(Configuration defaultConfiguration)
        {
            if (defaultConfiguration == null)
            {
                throw new ArgumentException("Configuration cannot be null", "defaultConfiguration");
            }

            _configuration = defaultConfiguration;
            if (!_configuration.Properties.ContainsKey(NhCfg.Environment.ConnectionString))
            {
                throw new ArgumentException("Must specify connection string");
            }

            _defaultConnectionString = _configuration.Properties[NhCfg.Environment.ConnectionString] as string;
            if (_defaultConnectionString == null)
            {
                throw new ArgumentException("Connection string property must be of type string, not " +
                        _configuration.Properties[NhCfg.Environment.ConnectionString].GetType().FullName);
            }
        }

        /// <summary>
        /// TargetSessionFactory
        /// </summary>
        public override ISessionFactory TargetSessionFactory
        {
            get
            {
                string connectionString = LogicalThreadContext.GetData(CONNECTION_STRING) as string;

                System.Diagnostics.Trace.WriteLine(String.Format("{0} = {1}", System.Threading.Thread.CurrentThread.GetHashCode(), connectionString));
                
                if (connectionString == null)
                {
                    connectionString = _defaultConnectionString;
                }

                lock (_monitor)
                {
                    if (!_targetSessionFactories.Contains(connectionString))
                    {
                        System.Diagnostics.Trace.WriteLine(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " = (created) ");

                        _configuration.Properties[NhCfg.Environment.ConnectionString] = connectionString;
                        ISessionFactory sessionFactory = _configuration.BuildSessionFactory();

                        LocalSessionFactoryObject.DbProviderWrapper dbProviderWrapper = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider as LocalSessionFactoryObject.DbProviderWrapper;
                        if (dbProviderWrapper != null)
                        {
                            dbProviderWrapper.DbProvider = (IDbProvider)ContextRegistry.GetContext().GetObject("DbProvider");
                        }

                        _targetSessionFactories[connectionString] = sessionFactory;
                    }
                    else
                        System.Diagnostics.Trace.WriteLine(System.Threading.Thread.CurrentThread.GetHashCode().ToString() + " =  (cached) ");

                    ISessionFactory factory = _targetSessionFactories[connectionString] as ISessionFactory;

                    System.Diagnostics.Trace.WriteLine(String.Format("{0} =  {1}", System.Threading.Thread.CurrentThread.GetHashCode(), connectionString));
                    
                    return factory;
                }
            }
        }

    }
}
