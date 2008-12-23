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
using Spring.Data.NHibernate.Support;
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
    public class MultipleDbTests
    {
        #region Fields

        private IApplicationContext ctx;

        #endregion


        #region Constants

        /// <summary>
        /// The shared <see cref="log4net.ILog"/> instance for this class (and derived classes). 
        /// </summary>
        protected static readonly ILog log =
            LogManager.GetLogger(typeof(TemplateTests));

        #endregion

        #region Constructor (s)

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleDbTests"/> class.
        /// </summary>
        public MultipleDbTests()
        {

        }

        #endregion

        [SetUp]
        public void SetUp()
        {
            BasicConfigurator.Configure();
#if NH_2_1
            ctx = new XmlApplicationContext("assembly://Spring.Data.NHibernate21.Integration.Tests/Spring.Data.NHibernate/MultipleDbTests.xml");
#elif NH_2_0
            ctx = new XmlApplicationContext("assembly://Spring.Data.NHibernate20.Integration.Tests/Spring.Data.NHibernate/MultipleDbTests.xml");
#else
            ctx = new XmlApplicationContext("assembly://Spring.Data.NHibernate.Integration.Tests/Spring.Data.NHibernate/MultipleDbTests.xml");
#endif

        }

        [Test]
        public void MultipleDBAccess()
        {
            float transferAmount = 114;
            IAccountManager mgr = null;

            Assert.IsNotNull(ctx, "Application Context is null");
            mgr = ctx["accountManager"] as IAccountManager;
            Assert.IsNotNull(mgr, "accountManager not of expected type. Type = " + ctx["accountManager"].GetType().ToString());
            mgr.DoTransfer(transferAmount, transferAmount);
        }

        [Test]
        public void MultipleDBAccessUsingMultipleSessionScopes()
        {
            SessionScope scope1 = new SessionScope( (ISessionFactory) ctx["SessionFactory1"], false );
            SessionScope scope2 = new SessionScope( (ISessionFactory) ctx["SessionFactory2"], false );

            scope1.Open();
            scope2.Open();

            // do something
            MultipleDBAccess();

            scope1.Close();
            scope2.Close();
        }

    }
}
