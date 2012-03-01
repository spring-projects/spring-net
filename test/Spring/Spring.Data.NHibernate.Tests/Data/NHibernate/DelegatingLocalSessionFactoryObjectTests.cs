using System;
using NUnit.Framework;
using NhCfg = NHibernate.Cfg;
using NHibernate;
using System.Collections;
using Spring.Data.Common;
using NHibernate.Dialect;
using Spring.Context.Support;
using NHibernate.Connection;
using NHibernate.Driver;
using Spring.Data.NHibernate.Bytecode;

namespace Spring.Data.NHibernate
{

    [TestFixture]
    public class DelegatingLocalSessionFactoryObjectTests
    {
        [Test]
        public void CanSetConfigurationAndProperties()
        {
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            dbProvider.ConnectionString = "Data Source=(local);Database=Spring;Trusted_Connection=false";
            DelegatingLocalSessionFactoryObject lsfo = new DelegatingLocalSessionFactoryObject();
            lsfo.DbProvider = dbProvider;
            lsfo.ApplicationContext = new StaticApplicationContext();

            IDictionary properties = new Hashtable();
            properties.Add(NhCfg.Environment.Dialect, typeof(MsSql2000Dialect).AssemblyQualifiedName);
            properties.Add(NhCfg.Environment.ConnectionDriver, typeof(SqlClientDriver).AssemblyQualifiedName);
            properties.Add(NhCfg.Environment.ConnectionProvider, typeof(DriverConnectionProvider).AssemblyQualifiedName);

            properties.Add(NhCfg.Environment.Hbm2ddlKeyWords, "none");

            lsfo.HibernateProperties = properties;
            lsfo.AfterPropertiesSet();

            Assert.IsNotNull(lsfo.Configuration);
            Assert.AreEqual(lsfo.Configuration.Properties[NhCfg.Environment.ConnectionProvider], typeof(DriverConnectionProvider).AssemblyQualifiedName);
            Assert.AreEqual(lsfo.Configuration.Properties[NhCfg.Environment.ConnectionDriver], typeof(SqlClientDriver).AssemblyQualifiedName);
            Assert.AreEqual(lsfo.Configuration.Properties[NhCfg.Environment.Dialect], typeof(MsSql2000Dialect).AssemblyQualifiedName);

            Assert.AreEqual(lsfo.Configuration.Properties[NhCfg.Environment.ProxyFactoryFactoryClass], typeof(ProxyFactoryFactory).AssemblyQualifiedName);

        }

    }

}
