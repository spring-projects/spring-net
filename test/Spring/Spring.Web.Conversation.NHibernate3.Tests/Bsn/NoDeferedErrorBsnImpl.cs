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

using NHibernate;
using Spring.Entities;
using NUnit.Framework;
using Spring.Spring.Data.Common;

namespace Spring.Bsn
{
    /// <summary>
    /// Outside the "transaction boundaries", each statement
    /// execution (lazy loads) was being made a call to
    /// "IDbProvider.CreateConnection()". This would
    /// cause a large over-reading because most of the "lazy loads" tend to
    /// occur outside the "transaction boundaries".
    /// The solution to this is to use "connection.release_mode" with "on_close"
    /// in "HibernateProperties."
    /// This test was created to show how the connection openings
    /// occur in the following scenarios:
    /// *Test with conversation and "connection.release_mode" 
    /// "auto"(ConnectionReleaseMode.AfterTransaction).
    /// *Test with NO conversation and "connection.release_mode" 
    /// "auto"(ConnectionReleaseMode.AfterTransaction)
    /// on block within the scope of "transaction boundary".
    /// *Test with NO conversation and "connection.release_mode" 
    /// "auto"(ConnectionReleaseMode.AfterTransaction).
    /// *Test with conversation and "connection.release_mode" 
    /// "on_close"(ConnectionReleaseMode.OnClose).
    /// </summary>
    public class ConnectionReleaseModeIssueBsnImpl : IConnectionReleaseModeIssueBsn
    {
        private ISessionFactory sessionFactory;
        /// <summary>
        /// SessionFactory.
        /// </summary>
        public ISessionFactory SessionFactory
        {
            get { return sessionFactory; }
            set { sessionFactory = value; }
        }

        #region IConnectionReleaseModeIssueBsn Members

        /// <summary>
        /// Test.
        /// </summary>
        [Transaction.Interceptor.Transaction(ReadOnly=true)]
        public void Test()
        {
            ISession sessionNoConv = this.SessionFactory.GetCurrentSession();
            SPCMasterEnt masterEnt2 = sessionNoConv.Get<SPCMasterEnt>(2);
            Assert.AreEqual(1, masterEnt2.SPCDetailEntList.Count, "masterEnt2.SPCDetailEntList.Count");

            SPCMasterEnt masterEnt3 = sessionNoConv.Get<SPCMasterEnt>(3);
            Assert.AreEqual(1, masterEnt3.SPCDetailEntList.Count, "masterEnt3.SPCDetailEntList.Count");

            Assert.AreEqual(1, ConnectionCreationTrackingDbProvider.Count, "ConnectionCreationTrackingDbProvider.Count");
        }

        #endregion
    }
}
