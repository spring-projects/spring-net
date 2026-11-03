using NUnit.Framework;
using NhCfg = NHibernate.Cfg;
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

            Assert.That(lsfo.Configuration, Is.Not.Null);
            Assert.That(typeof(DriverConnectionProvider).AssemblyQualifiedName, Is.EqualTo(lsfo.Configuration.Properties[NhCfg.Environment.ConnectionProvider]));
            Assert.That(typeof(SqlClientDriver).AssemblyQualifiedName, Is.EqualTo(lsfo.Configuration.Properties[NhCfg.Environment.ConnectionDriver]));
            Assert.That(typeof(MsSql2000Dialect).AssemblyQualifiedName, Is.EqualTo(lsfo.Configuration.Properties[NhCfg.Environment.Dialect]));
        }
    }
}
