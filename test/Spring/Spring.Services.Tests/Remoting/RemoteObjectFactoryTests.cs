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

#endregion

namespace Spring.Remoting
{
    /// <summary>
    /// Unit tests for the RemoteObjectFactory class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class RemoteObjectFactoryTests
    {
        [Test]
        public void BailsWhenNotConfigured()
        {
            RemoteObjectFactory factory = new RemoteObjectFactory();
            Assert.Throws<ArgumentException>(() => factory.AfterPropertiesSet(), "The Target property is required.");
        }

        [Test]
        public void CreateRemoteObject()
        {
            RemoteObjectFactory factory = new RemoteObjectFactory();
            factory.Target = new SimpleCounter();
            factory.AfterPropertiesSet();

            object obj = factory.GetObject();

            Assert.IsTrue((obj is MarshalByRefObject), "Object should derive from MarshalByRefObject.");
        }

        [Test]
        public void ReturnsInterfaceImplementation()
        {
            RemoteObjectFactory factory = new RemoteObjectFactory();
            factory.Target = new SimpleCounter();
            factory.AfterPropertiesSet();

            object obj = factory.GetObject();

            Assert.IsTrue((obj is ISimpleCounter), "Object should implement an interface.");
        }

        [Test]
        public void CreateRemoteObjectWithLeaseInfo()
        {
            RemoteObjectFactory factory = new RemoteObjectFactory();
            factory.Target = new SimpleCounter();
            factory.Infinite = false;
            factory.InitialLeaseTime = TimeSpan.FromMilliseconds(10000);
            factory.RenewOnCallTime = TimeSpan.FromMilliseconds(1000);
            factory.SponsorshipTimeout = TimeSpan.FromMilliseconds(100);
            MarshalByRefObject remoteObject = (MarshalByRefObject) factory.GetObject();

            ILease lease = (ILease) remoteObject.InitializeLifetimeService();
            Assert.AreEqual(TimeSpan.FromMilliseconds(10000), lease.InitialLeaseTime, "InitialLeaseTime");
            Assert.AreEqual(TimeSpan.FromMilliseconds(1000), lease.RenewOnCallTime, "RenewOnCallTime");
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), lease.SponsorshipTimeout, "SponsorshipTimeout");
        }
    }
}
