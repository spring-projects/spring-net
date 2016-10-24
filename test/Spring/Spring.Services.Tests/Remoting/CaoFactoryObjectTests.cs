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

using NUnit.Framework;

using Spring.Context;
using Spring.Context.Support;

#endregion

namespace Spring.Remoting
{
    /// <summary>
    /// Unit tests for the CaoFactoryObject class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class CaoFactoryObjectTests : BaseRemotingTestFixture
    {
        [Test]
        public void BailsWhenNotConfigured()
        {
            CaoFactoryObject cfo = new CaoFactoryObject();
            Assert.Throws<ArgumentException>(() => cfo.AfterPropertiesSet());
        }

        [Test]
        public void GetSimpleObject()
        {
            IApplicationContext ctx = new XmlApplicationContext("assembly://Spring.Services.Tests/Spring.Data.Spring.Remoting/cao.xml");
            ContextRegistry.RegisterContext(ctx);

            object obj = ctx.GetObject("remoteCaoCounter1");

            Assert.IsNotNull(obj, "Object is null even though a object has been registered.");
            Assert.IsTrue((obj is ISimpleCounter), "Object should implement 'ISimpleCounter' interface.");

            ISimpleCounter sc = (ISimpleCounter) obj;
            Assert.AreEqual(7, sc.Counter, "Remote object hasn't been activated by the client.");
            sc.Count();
            Assert.AreEqual(8, sc.Counter);
        }

        [Test]
        public void DisconnectFromClient()
        {
            IApplicationContext ctx = new XmlApplicationContext("assembly://Spring.Services.Tests/Spring.Data.Spring.Remoting/cao.xml");
            ContextRegistry.RegisterContext(ctx);

            object obj = ctx.GetObject("remoteCaoCounter1");
            Assert.IsNotNull(obj, "CAO is null even though a CAO has been registered.");

            IDisposable cao = obj as IDisposable;
            Assert.IsNotNull(cao, "CAO should implement 'IDisposable' interface.");
        }
    }
}