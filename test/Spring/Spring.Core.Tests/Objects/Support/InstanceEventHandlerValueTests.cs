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

using NUnit.Framework;

#endregion

namespace Spring.Objects.Support
{
	/// <summary>
	/// Unit tests for the InstanceEventHandlerValue class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class InstanceEventHandlerValueTests
    {
        [Test]
        public void Instantiation () 
        {
            PingSource source = new PingSource ();
            StaticEventHandlerValue wirer
                = new StaticEventHandlerValue (source, "OnPing");
            Assert.IsNotNull (wirer.Source);
            Assert.AreEqual ("OnPing", wirer.MethodName);
            Assert.IsTrue (wirer.ToString ().IndexOf (wirer.MethodName) > 0);
        }

        [Test]
        public void Wire () {
            InstanceEventHandlerValue wirer = new InstanceEventHandlerValue ();
            wirer.EventName = "Ping";
            wirer.MethodName = "OnPing";
            PingSource source = new PingSource ();
            PingListener sink = new PingListener ();
            wirer.Wire (source, sink);
            source.OnPing ();
            Assert.IsTrue (sink.GotPing, "The event handler did not get notified when the event was raised.");
            Assert.AreEqual (1, sink.PingCount, "The event handler was not get notified exactly once when the event was raised exactly once.");
        }

        [Test]
        public void WireWithStaticHandler () 
        {
            PingSource source = new PingSource ();
            InstanceEventHandlerValue wirer
                = new InstanceEventHandlerValue (source, "OnPing");
            wirer.EventName = "Ping";
            wirer.Wire (source, typeof (StaticPingListener));
        }

        [Test]
        public void BailsIfMethodDoesntExist () 
        {
            PingSource source = new PingSource ();
            InstanceEventHandlerValue wirer
                = new InstanceEventHandlerValue (source, "Roo");
            wirer.EventName = "Ping";
            Assert.Throws<FatalObjectException>(() => wirer.Wire (source, typeof (StaticPingListener)));
        }
	}
}
