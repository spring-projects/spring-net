using NUnit.Framework;

using NhCfg = NHibernate.Cfg;

using System.Collections.Generic;

using Spring.Data.Common;

using NHibernate.Dialect;

using Spring.Context.Support;

using NHibernate.Connection;
using NHibernate.Driver;

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

            IDictionary<string, string> properties = new Dictionary<string, string>();
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
        }
    }
}