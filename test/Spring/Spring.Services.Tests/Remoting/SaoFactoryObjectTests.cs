#region License

/*
 * Copyright 2005 the original author or authors.
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using NUnit.Framework;
using Spring.Aop.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Remoting
{
	/// <summary>
	/// Unit tests for the SaoFactoryObject class.
	/// </summary>
	/// <author>Mark Pollack</author>
	/// <author>Bruno Baia</author>
	[TestFixture]
	public class SaoFactoryObjectTests : BaseRemotingTestFixture
	{
	    [Test]
		[ExpectedException(typeof(ArgumentException))]
		public void BailsWhenNotConfigured ()
		{
			SaoFactoryObject sfo = new SaoFactoryObject();
			sfo.AfterPropertiesSet();
		}

		/// <summary>
		/// Make sure that transparent proxies are recognized 
		/// correctly by the core object factory.
		/// </summary>
		/// <remarks>
		/// This is a test for SPRNET-200
		/// </remarks>
		[Test]
		public void WiringWithTransparentProxy()
		{
            IApplicationContext ctx = new XmlApplicationContext("assembly://Spring.Services.Tests/Spring.Data.Spring.Remoting/autowire.xml");
            ContextRegistry.RegisterContext(ctx);

			TestObject to = (TestObject) ctx.GetObject("kerry3");
			Assert.IsNotNull(to, "Object is null even though a object has been configured.");
			Assert.IsTrue(RemotingServices.IsTransparentProxy(to.Sibling), "IsTransparentProxy");
			Assert.AreEqual("Kerry3", to.Name, "Name");

			to = (TestObject) ctx.GetObject("objectWithCtorArgRefShortcuts");
			Assert.IsTrue(RemotingServices.IsTransparentProxy(to.Spouse), "IsTransparentProxy");

			ConstructorDependenciesObject cdo = (ConstructorDependenciesObject) ctx.GetObject("rod2");
			Assert.IsTrue(RemotingServices.IsTransparentProxy(cdo.Spouse1), "IsTransparentProxy");
			Assert.IsTrue(RemotingServices.IsTransparentProxy(cdo.Spouse2), "IsTransparentProxy");
		}

		[Test]
		public void GetSaoWithSingletonMode()
		{
            IApplicationContext ctx = new XmlApplicationContext("assembly://Spring.Services.Tests/Spring.Data.Spring.Remoting/saoSingleton.xml");
            ContextRegistry.RegisterContext(ctx);

			object obj = ctx.GetObject("remoteSaoSingletonCounter");

			Assert.IsNotNull(obj, "Object is null even though a object has been exported.");
			Assert.IsTrue((obj is ISimpleCounter), "Object should implement 'ISimpleCounter' interface.");

			ISimpleCounter sc = (ISimpleCounter) obj;
			Assert.AreEqual(1, sc.Counter, "Remote object hasn't been activated by the server.");
			sc.Count();
			Assert.AreEqual(2, sc.Counter, "Remote object doesn't work in a 'Singleton' mode.");
		}

        [Test]
        public void GetSaoWithSingletonModeAndAop()
        {
            IApplicationContext ctx = new XmlApplicationContext("assembly://Spring.Services.Tests/Spring.Data.Spring.Remoting/saoSingleton-aop.xml");
            ContextRegistry.RegisterContext(ctx);

            //object saoFactory = ctx.GetObject("&remoteCounter");
            //Assert.IsNotNull(saoFactory);

            object obj = ctx.GetObject("remoteCounter");

            Assert.IsNotNull(obj, "Object is null even though a object has been exported.");
            Assert.IsTrue(AopUtils.IsAopProxy(obj));
            Assert.IsTrue((obj is ISimpleCounter), "Object should implement 'ISimpleCounter' interface.");
            

            MethodCounter aopCounter = ctx.GetObject("countingBeforeAdvice") as MethodCounter;
            Assert.IsNotNull(aopCounter);

            int aopCount = aopCounter.GetCalls("Count");
            Assert.AreEqual(0, aopCount);

            ISimpleCounter sc = (ISimpleCounter)obj;
            Assert.AreEqual(1, sc.Counter, "Remote object hasn't been activated by the server.");
            sc.Count();
            Assert.AreEqual(2, sc.Counter, "Remote object doesn't work in a 'Singleton' mode.");
            Assert.AreEqual(1, aopCounter.GetCalls("Count"));
        }

		[Test]
		public void GetSaoWithSingleCallMode()
		{
            IApplicationContext ctx = new XmlApplicationContext("assembly://Spring.Services.Tests/Spring.Data.Spring.Remoting/saoSingleCall.xml");
            ContextRegistry.RegisterContext(ctx);

			object obj = ctx.GetObject("remoteSaoSingleCallCounter");

			Assert.IsNotNull(obj, "Object is null even though a object has been exported.");
			Assert.IsTrue((obj is ISimpleCounter), "Object should implement 'ISimpleCounter' interface.");

			ISimpleCounter sc = (ISimpleCounter) obj;
			Assert.IsNotNull(sc, "Object is null even though a object has been exported.");
			Assert.AreEqual(1, sc.Counter, "Remote object hasn't been activated by the server.");
			sc.Count();
			Assert.AreEqual(1, sc.Counter, "Remote object don't works in a 'SingleCall' mode.");
		}

		[Test]
		public void GetSaoWithLifetimeService()
		{
            IApplicationContext ctx = new XmlApplicationContext("assembly://Spring.Services.Tests/Spring.Data.Spring.Remoting/saoLifetimeService.xml");
            ContextRegistry.RegisterContext(ctx);

			MarshalByRefObject obj = (MarshalByRefObject) ctx.GetObject("remoteSaoCounter");
			ILease lease = (ILease) obj.GetLifetimeService();

			Assert.AreEqual(TimeSpan.FromMilliseconds(10000), lease.InitialLeaseTime, "InitialLeaseTime");
			Assert.AreEqual(TimeSpan.FromMilliseconds(1000), lease.RenewOnCallTime, "RenewOnCallTime");
			Assert.AreEqual(TimeSpan.FromMilliseconds(100), lease.SponsorshipTimeout, "SponsorshipTimeout");
		}
	}
}
