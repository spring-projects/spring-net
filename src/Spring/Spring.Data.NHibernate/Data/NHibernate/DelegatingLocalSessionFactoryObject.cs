using System.Data;
using System.Collections;
using System.Collections.Generic;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Connection;

using Spring.Data.NHibernate;

namespace Spring.Data.NHibernate
{
    ///<summary>
    /// Delegates to an implementation of ISessionFactory that can select among multiple instances based on
    /// thread local storage.
    ///</summary>
    public class DelegatingLocalSessionFactoryObject : LocalSessionFactoryObject
    {
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
        protected override ISessionFactory NewSessionFactory(Configuration config)
        {
            return new SimpleDelegatingSessionFactory(config);
        }

        /// <summary>
        /// PostProcessConfiguration
        /// </summary>
        /// <param name="config"></param>
        protected override void PostProcessConfiguration(Configuration config)
        {
            // called before NewSessionFactory
            if (!config.Properties.ContainsKey(Environment.ConnectionString))
            {
                throw new System.ArgumentException("Must specify connection string");
            }
        }

    }

}