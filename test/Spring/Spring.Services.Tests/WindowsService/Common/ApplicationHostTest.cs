#region License
/*
* Copyright 2002-2010 the original author or authors.
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
using System;
using System.Reflection;
using DotNetMock.Dynamic;
using NUnit.Framework;
using Spring.Objects.Factory.Config;

namespace Spring.Services.WindowsService.Common
{
    [TestFixture]	
    public class ApplicationHostTest
	{
        ApplicationHost host;
        IConfigurableListableObjectFactory factory;
        IDynamicMock factoryMock;

        private ApplicationHost NewApplicationHost (AppDomain domain, IConfigurableListableObjectFactory factory)
        {
            BindingFlags InstantiationBindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance;
            ConstructorInfo c = typeof(ApplicationHost).GetConstructor(InstantiationBindingFlags, null, new Type[] {typeof (AppDomain), factory.GetType()} , null);
            return (ApplicationHost) c.Invoke(new object[] {domain, factory});
        }

        [SetUp]
        public void SetUp ()
        {
            factoryMock = new DynamicMock(typeof(IConfigurableListableObjectFactory));
            factory = (IConfigurableListableObjectFactory) factoryMock.Object;
            host = NewApplicationHost (AppDomain.CurrentDomain, factory);
        }

        [TearDown]
        public void TearDown ()
        {
            factoryMock.Verify ();
        }

        [Test]
		public void WhenStartedPreInstantiateSingletonsOnFactory()
		{
            factoryMock.Expect("PreInstantiateSingletons");
            host.Start();
        }

        [Test]
		public void WhenStoppedDestroySingletons()
		{
            host.Start();
            factoryMock.Expect("Dispose");
            host.Stop();
        }

        [Test]
        public void GivesAnInfiniteLifetimeByPreventingALeaseFromBeingCreated ()
        {
            Assert.IsNull(host.InitializeLifetimeService());
        }
	}
}
