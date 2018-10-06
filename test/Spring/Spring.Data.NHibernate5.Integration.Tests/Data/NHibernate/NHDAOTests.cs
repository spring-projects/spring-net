#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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
using NHibernate;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Common;
using Spring.Data.NHibernate.Support;

#endregion

namespace Spring.Data.NHibernate
{
	[TestFixture]
	public class NHDAOTests 
	{
	    private IApplicationContext ctx;

	    [SetUp]
        public void SetUp()
        {
            //BasicConfigurator.Configure();
            string assemblyName = GetType().Assembly.GetName().Name;
            //ctx = new XmlApplicationContext("assembly://" + assemblyName + "/Spring.Data.NHibernate/NHDAOTests.xml");
            string[] contextFiles = new string[]
                                        {
                                            "assembly://" + assemblyName + "/Spring.Data.NHibernate/Controllers.xml",
                                            "assembly://" + assemblyName + "/Spring.Data.NHibernate/Services.xml",
                                            "assembly://" + assemblyName + "/Spring.Data.NHibernate/Dao.xml"

        };
            ctx = new XmlApplicationContext(contextFiles);
            ctx.Name = AbstractApplicationContext.DefaultRootContextName;

            if (!ContextRegistry.IsContextRegistered(AbstractApplicationContext.DefaultRootContextName))
            {
                ContextRegistry.RegisterContext(ctx);
            }
        }

        [Test]
        public void DbProviderTranslation()
        {
            ISessionFactory sf = ctx["SessionFactory"] as ISessionFactory;
            IDbProvider dbProvider = SessionFactoryUtils.GetDbProvider(sf);

            Assert.That(dbProvider.DbMetadata.ProductName, Does.Contain("Microsoft SQL Server"));
        }

        [Test]
        public void SessionScopeOutsideTransaction()
        {
            float transferAmount = 113;
            using (new SessionScope())
            {
                IAccountManager mgr = null;
                mgr = ctx["accountManager"] as IAccountManager;
                Assert.IsNotNull(mgr, "accountManager not of expected type. Type = " + ctx["accountManager"].GetType());
                mgr.DoTransfer(transferAmount, transferAmount);
            }
        }

        [Test]
        public void DeclarativeWithAttributes()
        {
            float transferAmount = 113;
            IAccountManager mgr = null;
            try
            {
                Assert.IsNotNull(ctx,"Application Context is null");
                mgr = ctx["accountManager"] as IAccountManager;
                Assert.IsNotNull(mgr,"accountManager not of expected type. Type = " + ctx["accountManager"].GetType().ToString());
                mgr.DoTransfer(transferAmount, transferAmount);
                Assert.IsTrue(ContainsNewData(transferAmount));
            } catch (Exception e)
            {
                if (mgr.ThrowException)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Assert.IsTrue(DoesNotContainNewData(transferAmount));
                }
            }
        }

	    private bool ContainsNewData(float amount)
	    {
	        //to be done
	        return true;
	    }

	    private bool DoesNotContainNewData(float amount)
	    {
	        //to be done
	        return true;
	    }
	}
}
