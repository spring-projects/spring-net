#if NH_2_1XX
using log4net.Config;

using NHibernate.Bytecode;
using NHibernate.Cfg;
using NHibernate.Engine;
using NHibernate.Tool.hbm2ddl;

using NUnit.Framework;

namespace Spring.Data.NHibernate.Bytecode
{
    public abstract class AbstractInjectableUserTypeFixture
    {
        protected ISessionFactoryImplementor sessions;

        static AbstractInjectableUserTypeFixture()
        {
            XmlConfigurator.Configure();
        }

        /// <summary>
        /// Initialize the ServiceLocator registering all services needed by this test.
        /// </summary>
        /// <remarks>
        /// Services needed, in this test, are:
        /// 
        /// - uNhAddIns.Adapters.CommonTests.EnhancedBytecodeProvider.IDelimiter 
        ///		Implementation: uNhAddIns.Adapters.CommonTests.EnhancedBytecodeProvider.ParenDelimiter
        /// 
        /// - uNhAddIns.Adapters.CommonTests.EnhancedBytecodeProvider.InjectableStringUserType 
        /// </remarks>
        protected abstract void InitializeServiceLocator();

        protected abstract IBytecodeProvider GetBytecodeProvider();

        [Ignore("Work in progress")]
        [TestFixtureSetUp]
        public void CreateDb()
        {
            InitializeServiceLocator();
            var cfg = new Configuration();
            Environment.BytecodeProvider = GetBytecodeProvider();
            cfg.Configure();
            cfg.AddResource("uNhAddIns.Adapters.CommonTests.EnhancedBytecodeProvider.Foo.Spechbm.xml",
                                            typeof(AbstractInjectableUserTypeFixture).Assembly);
            new SchemaExport(cfg).Create(false, true);
            sessions = (ISessionFactoryImplementor)cfg.BuildSessionFactory();
        }

        [Ignore("Work in progress")]
        [TestFixtureTearDown]
        public void CloseSessionFactory()
        {
            if (sessions != null)
            {
                sessions.Dispose();
            }
            sessions = null;
        }

        [Ignore("Work in progress")]
        [Test]
        public void SaveObjectWithStringChangedToUpper()
        {
            object savedId;
            Foo f = new Foo { Description = "something" };

            using (var s = sessions.OpenSession())
            using (var tx = s.BeginTransaction())
            {
                savedId = s.Save(f);
                tx.Commit();
            }

            using (var s = sessions.OpenSession())
            {
                var upperFoo = s.Get<Foo>(savedId);

                Assert.AreEqual("[something]", upperFoo.Description);
            }
        }

        [Ignore("Work in progress")]
        [Test]
        public void SaveNullPropertyAndGetItBack()
        {
            object savedId;
            var f = new Foo();

            using (var s = sessions.OpenSession())
            using (var tx = s.BeginTransaction())
            {
                savedId = s.Save(f);
                tx.Commit();
            }

            using (var s = sessions.OpenSession())
            {
                var upperFoo = s.Get<Foo>(savedId);

                Assert.IsNull(upperFoo.Description);
            }
        }

    }
}
#endif