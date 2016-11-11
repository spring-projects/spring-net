#region License

/*
 * Copyright 2004 the original author or authors.
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
using System.Runtime.Remoting.Lifetime;

using NUnit.Framework;

using Spring.Context;
using Spring.Context.Support;
using Spring.Remoting.Support;

#endregion

namespace Spring.Remoting
{
    /// <summary>
    /// Unit tests for the CaoExporter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class CaoExporterTests : BaseRemotingTestFixture
    {
        [Test]
        public void BailsWhenNotConfigured()
        {
            CaoExporter exp = new CaoExporter();
            Assert.Throws<ArgumentException>(() => exp.AfterPropertiesSet());
        }

        [Test]
        public void RegistersSimpleObject()
        {
            IApplicationContext ctx = new XmlApplicationContext("assembly://Spring.Services.Tests/Spring.Data.Spring.Remoting/caoLifetimeService.xml");
            ContextRegistry.RegisterContext(ctx);

            ICaoRemoteFactory caoFactory = Activator.GetObject(typeof(ICaoRemoteFactory), "tcp://localhost:8005/counter2") as ICaoRemoteFactory;
            Assert.IsNotNull(caoFactory, "Cao factory is null even though it has been registered.");

            MarshalByRefObject cao = caoFactory.GetObject() as MarshalByRefObject;
            Assert.IsNotNull(cao);
        }

        [Test]
        public void RegistersWithLifetimeService()
        {
            IApplicationContext ctx = new XmlApplicationContext("assembly://Spring.Services.Tests/Spring.Data.Spring.Remoting/caoLifetimeService.xml");
            ContextRegistry.RegisterContext(ctx);

            ICaoRemoteFactory caoFactory = Activator.GetObject(typeof(ICaoRemoteFactory), "tcp://localhost:8005/counter2") as ICaoRemoteFactory;
            Assert.IsNotNull(caoFactory, "Cao factory is null even though it has been registered.");

            MarshalByRefObject cao = caoFactory.GetObject() as MarshalByRefObject;
            Assert.IsNotNull(cao);

            ILease lease = (ILease) cao.GetLifetimeService();
            Assert.AreEqual(TimeSpan.FromMilliseconds(10000), lease.InitialLeaseTime, "InitialLeaseTime");
            Assert.AreEqual(TimeSpan.FromMilliseconds(1000), lease.RenewOnCallTime, "RenewOnCallTime");
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), lease.SponsorshipTimeout, "SponsorshipTimeout");
        }
    }
}