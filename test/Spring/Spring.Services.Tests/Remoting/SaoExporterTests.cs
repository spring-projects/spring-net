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
using Spring.Objects;
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Remoting
{
	/// <summary>
	/// Unit tests for the SaoExporter class.
	/// </summary>
	/// <author>Bruno Baia</author>
	/// <author>Mark Pollack</author>
	[TestFixture]
	public class SaoExporterTests : BaseRemotingTestFixture
	{
        [Test]
		[ExpectedException(typeof(ArgumentException))]
		public void BailsWhenNotConfigured ()
		{
			SaoExporter exp = new SaoExporter();
			exp.AfterPropertiesSet();
		}

        [Test]
        public void ExportSingleton()
        {
            using(DefaultListableObjectFactory of = new DefaultListableObjectFactory())
            {
                of.RegisterSingleton("simpleCounter", new SimpleCounter());
                SaoExporter saoExporter = new SaoExporter();
                saoExporter.ObjectFactory = of;
                saoExporter.TargetName = "simpleCounter";
                saoExporter.ServiceName = "RemotedSaoSingletonCounter";
                saoExporter.AfterPropertiesSet();
                of.RegisterSingleton("simpleCounterExporter", saoExporter); // also tests SaoExporter.Dispose()!

                ISimpleCounter client = (ISimpleCounter) Activator.GetObject(typeof(ISimpleCounter), "tcp://localhost:8005/RemotedSaoSingletonCounter" );
                client.Count();
                client.Count();

                Assert.AreEqual(2, client.Counter);
            }
        }

        [Test]
        public void ExportSingleCall()
        {
            using(DefaultListableObjectFactory of = new DefaultListableObjectFactory())
            {
                of.RegisterObjectDefinition("simpleCounter", new RootObjectDefinition(typeof(SimpleCounter), false));
                SaoExporter saoExporter = new SaoExporter();
                saoExporter.ObjectFactory = of;
                saoExporter.TargetName = "simpleCounter";
                saoExporter.ServiceName = "RemotedSaoSingleCallCounter";
                saoExporter.AfterPropertiesSet();
                of.RegisterSingleton("simpleCounterExporter", saoExporter); // also tests SaoExporter.Dispose()!

                ISimpleCounter client = (ISimpleCounter) Activator.GetObject(typeof(ISimpleCounter), "tcp://localhost:8005/RemotedSaoSingleCallCounter" );
                client.Count();
                client.Count();
                Assert.AreEqual(0, client.Counter);
            }
        }
	}
}
