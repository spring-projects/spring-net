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

#endregion

namespace Spring.Objects.Support
{
	/// <summary>
	/// Unit tests for the StaticEventHandlerValue class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class StaticEventHandlerValueTests
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
        public void Wire () 
        {
            PingSource source = new PingSource ();
            StaticEventHandlerValue wirer
                = new StaticEventHandlerValue (source, "OnPing");
            wirer.EventName = "Ping";
            Type sink = typeof (StaticPingListener);
            wirer.Wire (source, sink);
            source.OnPing ();
            Assert.IsTrue (StaticPingListener.GotPing, "The event handler did not get notified when the event was raised.");
            Assert.AreEqual (1, StaticPingListener.PingCount, "The event handler was not get notified exactly once when the event was raised exactly once.");
        }

        [Test]
        public void WireWithInstanceHandler () 
        {
            StaticEventHandlerValue wirer = new StaticEventHandlerValue ();
            wirer.EventName = "Ping";
            wirer.MethodName = "OnPing";
            PingSource source = new PingSource ();
            Assert.Throws<FatalObjectException>(() => wirer.Wire (source, new PingListener ()));
        }
	}

    internal class StaticPingListener {

        public static bool OnPing (object sender, PingEventArgs e) {
            _ping = true;
            ++_pingCount;
            return true;
        }

        public static bool GotPing {
            get {
                return _ping;
            }
        }

        public static int PingCount {
            get {
                return _pingCount;
            }
        }

        protected static bool _ping;
        protected static int _pingCount;
    }
}
