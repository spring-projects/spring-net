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
using log4net;
using log4net.Config;
using NHibernate;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Common;
using Spring.Data.Support;
using Spring.Transaction;
using Spring.Transaction.Support;

#endregion

namespace Spring.Data.NHibernate
{
	/// <summary>
	/// Use of Hibernate Template against database.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	[TestFixture]
	public class TemplateTests 
	{
		#region Fields
        private IDbProvider dbProvider;

        private IPlatformTransactionManager transactionManager;

        private IApplicationContext ctx;
		#endregion

		#region Constants

		/// <summary>
		/// The shared <see cref="log4net.ILog"/> instance for this class (and derived classes). 
		/// </summary>
		protected static readonly ILog log =
			LogManager.GetLogger(typeof (TemplateTests));

		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="TemplateTests"/> class.
                /// </summary>
		public 	TemplateTests()
		{

		}

		#endregion

        [SetUp]
        public void SetUp()
        {
            //NamespaceParserRegistry.RegisterParser(typeof(DatabaseNamespaceParser));
            BasicConfigurator.Configure();
#if NH_2_0
            ctx = new XmlApplicationContext("assembly://Spring.Data.NHibernate20.Integration.Tests/Spring.Data.NHibernate/templateTests.xml");
#else
            ctx = new XmlApplicationContext("assembly://Spring.Data.NHibernate.Integration.Tests/Spring.Data.NHibernate/templateTests.xml");
#endif
            dbProvider = ctx["DbProvider"] as IDbProvider;
            transactionManager = ctx["hibernateTransactionManager"] as IPlatformTransactionManager;
            
        }

	    [Test]
        public void ExceptionTranslator()
        {
	        ISessionFactory sessionFactory = ctx["SessionFactory"] as ISessionFactory;
	        HibernateTemplate template = new HibernateTemplate(sessionFactory);
            IAdoExceptionTranslator translator = template.AdoExceptionTranslator;
	        Assert.IsNotNull(translator, "ADO.NET exception translator should not be null");
	        
	        
	        Assert.IsInstanceOfType(typeof(ErrorCodeExceptionTranslator), translator);
        }
	    
	    [Test]
	    public void FallbackExceptionTranslator()
	    {
            //ISessionFactory sessionFactory = ctx["SessionFactory"] as ISessionFactory;
            //HibernateTemplate template = new HibernateTemplate(sessionFactory);
            IAdoExceptionTranslator fallbackTranslator = new FallbackExceptionTranslator();
            fallbackTranslator.Translate("test", "sql", new Exception("foo"));
	        
	    }
	    
        [Test]
        [Ignore("Just for demo purposes")]
        public void DemoDao()
        {

            ITestObjectDao dao = (ITestObjectDao)ctx["testObjectDaoViaTxAttributes"];
            TestObject toGeorge = new TestObject();
            toGeorge.Name = "George";
            toGeorge.Age = 33;
            dao.Create(toGeorge);

        }

        [Test]
        [Ignore("Just for demo purposes")]
        public void SimpleDao()
        {
            ITestObjectDao dao = (ITestObjectDao)ctx["NHTestObjectDao"];
            Assert.IsNotNull(dao);
            TestObject to = dao.FindByName("Gabriel");
            Assert.IsNotNull(to);
            Assert.AreEqual("Gabriel", to.Name);

        }

	    /// <summary>
	    /// Test simple data base operations using attributes for 
	    /// declarative transaction demarcation.
	    /// </summary>
        [Test]
        public void DaoOperationsViaProxyFactoryWithTxAttributes()
        {

            ITestObjectDao dao = (ITestObjectDao)ctx["testObjectDaoViaTxAttributes"];
            ExecuteDaoOperations(dao);

        }


        [Test]
        public void DaoOperationsViaTransactionProxy()
        {

            ITestObjectDao dao = (ITestObjectDao)ctx["testObjectDaoTransProxy"];
            ExecuteDaoOperations(dao);

        }

        [Test]
        public void DaoOperationsWithRollback()
        {
	        ITestObjectDao dao = (ITestObjectDao)ctx["testObjectDaoTransProxy"];
            try 
            {
                ExecuteAndRollbackDaoOperations(dao);
            } catch (Exception e)
            {
                TestObject to = dao.FindByName("Bugs");
                Assert.IsNull(to);
                
                //just to get rid of compiler warning...
                Assert.IsNotNull(e);
            }
        }

        private static void ExecuteAndRollbackDaoOperations(ITestObjectDao dao)
        {
            TestObject toBugs = new TestObject();
            toBugs.Name = "Bugs";
            toBugs.Age = 33;
            dao.CreateUpdateRollback(toBugs);

        }
	    private static void ExecuteDaoOperations(ITestObjectDao dao)
	    {
	        TestObject toGeorge = new TestObject();
	        toGeorge.Name = "George";
	        toGeorge.Age = 33;
	        dao.Create(toGeorge);
	        TestObject to = dao.FindByName("George");
	        Assert.IsNotNull(to, "FindByName for George should not return null");
	        Assert.AreEqual("George", to.Name);
	        Assert.AreEqual(33, to.Age);
    
	        to.Age=34;
	        dao.Update(to);
    
	        TestObject to2 = dao.FindByName("George");
	        Assert.AreEqual(34, to2.Age);
    
	        dao.Delete(to);
    
	        TestObject to3 = dao.FindByName("George");
	        Assert.IsNull(to3, "Should not have found TestObject with name George. TestObject = " + to.ToString()   );
	    }


	    [Test]
        public void ExecuteTemplate()
        {
            ITestObjectDao dao = (ITestObjectDao)ctx["NHTestObjectDao"];
            TransactionTemplate tt = new TransactionTemplate(transactionManager);
            object result = tt.Execute(new SimpleTransactionCallback(dbProvider, dao));
            TestObject to = result as TestObject;
            Assert.IsNotNull(to,"FindByName for Gabriel should not return null");
            Assert.AreEqual("Gabriel", to.Name);
        }

        [Test]
        public void ExecuteTransactionManager()
        {
            DefaultTransactionDefinition def = new DefaultTransactionDefinition();
            def.PropagationBehavior = TransactionPropagation.Required;

            ITransactionStatus status = transactionManager.GetTransaction(def);

            ITestObjectDao dao = (ITestObjectDao)ctx["NHTestObjectDao"];
            TestObject to;
            try
            {
                 to = dao.FindByName("Gabriel");
            } 
            catch (Exception)
            {
                transactionManager.Rollback(status);
                throw;
            }
            transactionManager.Commit(status);
            Assert.IsNotNull(to,"FindByName for Gabriel should not return null");
            Assert.AreEqual("Gabriel", to.Name);
        }

        private class SimpleTransactionCallback : ITransactionCallback
        {
            private IDbProvider dbProvider;
            private ITestObjectDao dao;

            public SimpleTransactionCallback(IDbProvider dbp, ITestObjectDao dao)
            {
                dbProvider = dbp;
                this.dao = dao;
            }
            /// <summary>
            /// Gets called by TransactionTemplate.Execute within a 
            /// transaction context.
            /// </summary>
            /// <param name="status">The associated transaction status.</param>
            /// <returns>a result object or <c>null</c></returns>
            public object DoInTransaction(ITransactionStatus status)
            {
                return dao.FindByName("Gabriel");
            }
        }


    }
}
