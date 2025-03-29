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

using NUnit.Framework;

namespace Spring.Objects.Support;

/// <summary>
/// Unit tests for the AutoWiringEventHandlerValue class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public class AutoWiringEventHandlerValueTests
{
    /// <summary>
    /// Kinda defeats the purpose of the AutoWiringEventHandlerValue class, but
    /// still a supported and legal option.
    /// </summary>
    [Test]
    public void AutowireExplicitly()
    {
        // create the event source, and a listener to wire to an event (s) on the source
        PingSource source = new PingSource();
        PingListener listener = new PingListener();

        // wire them up
        IEventHandlerValue wirer = new AutoWiringEventHandlerValue();
        wirer.EventName = "Ping";
        wirer.MethodName = "OnPing";
        wirer.Wire(source, listener);

        // raise the event on the source
        source.OnPing();

        // ascertain that the event listener was indeed notified of the raised event
        Assert.IsTrue(listener.GotPing, "The listener was not notified of the raised event.");
        Assert.IsTrue(listener.PingCount == 1, "The listener was notified more than once for a single raising of the source event (registered to listen more than once?)");
        Assert.IsFalse(listener.GotPinging, "The listener was (incorrectly) notified of an event that wasn't raised.");
        Assert.IsFalse(listener.GotPinged, "The listener was (incorrectly) notified of an event that wasn't raised.");
    }

    [Test]
    public void AutowireExplicitlyWithBadSignatureMethod()
    {
        // create the event source, and a listener to wire to an event (s) on the source
        PingSource source = new PingSource();
        PingListener listener = new PingListener();

        // wire them up
        IEventHandlerValue wirer = new AutoWiringEventHandlerValue();
        wirer.EventName = "Ping";
        wirer.MethodName = "OnPinged"; // signature is incompatible
        wirer.Wire(source, listener);

        // raise the event on the source
        source.OnPing();

        // ascertain that the event listener was not notified of the raised event
        Assert.IsFalse(listener.GotPing, "The listener was (incorrectly) notified of the raised event.");
        Assert.IsFalse(listener.GotPinging, "The listener was (incorrectly) notified of an event that wasn't raised.");
        Assert.IsFalse(listener.GotPinged, "The listener was (incorrectly) notified of an event that wasn't raised.");
    }

    [Test]
    public void AutowireAllHandlersWithCompatibleSignatures()
    {
        // create the event source, and a listener to wire to an event (s) on the source
        PingSource source = new PingSource();
        PingListener listener = new PingListener();

        // wire them up (any methods on the listener that have a compatible signature will be wired up)
        IEventHandlerValue wirer = new AutoWiringEventHandlerValue();
        wirer.EventName = "^Ping$";
        wirer.Wire(source, listener);

        // raise the event on the source
        source.OnPing();

        // ascertain that the event handler was indeed notified of the raised event
        Assert.IsTrue(listener.GotPing, "The listener was not notified of the raised event.");
        Assert.IsFalse(listener.GotPinging, "The listener was (incorrectly) notified of an event that wasn't raised.");
        Assert.IsFalse(listener.GotPinged, "The listener was (incorrectly) notified of an event that wasn't raised.");
    }

    [Test]
    public void AutowireAllEventsOnSource()
    {
        // create the event source, and a listener to wire to an event (s) on the source
        PingSource source = new PingSource();
        PingListener listener = new PingListener();

        // wire them up (any methods on the listener that have a signature compatible
        // with any event exposed on the source will be wired up)
        IEventHandlerValue wirer = new AutoWiringEventHandlerValue();
        wirer.Wire(source, listener);

        // raise the event (s) on the source
        source.OnPinging();
        source.OnPing();
        source.OnPinged();

        // ascertain that the event listener was indeed notified of the raised event
        Assert.IsTrue(listener.GotPinging, "The listener was not notified of the raised event.");
        Assert.IsTrue(listener.GotPing, "The listener was not notified of the raised event.");
        Assert.IsTrue(listener.GotPinged, "The listener was not notified of the raised event.");
    }

    [Test]
    public void AutowiringIsCaseInsensitive()
    {
        // create the event source, and a listener to wire to an event (s) on the source
        PingSource source = new PingSource();
        PingListener listener = new PingListener();

        // wire them up
        IEventHandlerValue wirer = new AutoWiringEventHandlerValue();
        wirer.EventName = "pInG";
        wirer.MethodName = "onpiNg";
        wirer.Wire(source, listener);

        // raise the event (s) on the source
        source.OnPing();

        // ascertain that the event listener was indeed notified of the raised event
        Assert.IsFalse(listener.GotPinging, "The listener was (incorrectly) notified of the raised event.");
        Assert.IsTrue(listener.GotPing, "The listener was not  notified of the raised event.");
        Assert.IsFalse(listener.GotPinged, "The listener was (incorrectly) notified of the raised event.");
    }

    [Test]
    public void AllCompatibleSignaturesAreWired()
    {
        // create the event source, and a listener to wire to an event (s) on the source
        PingSource source = new PingSource();
        PingListener listener = new PingListener();

        // wire them up
        IEventHandlerValue wirer = new AutoWiringEventHandlerValue();
        wirer.EventName = "Pinging";
        wirer.MethodName = ".+"; // both OnPinging and OnPinged have compatible sigs :'(
        wirer.Wire(source, listener);

        // raise the event (s) on the source
        source.OnPinging();

        // ascertain that the event listener was indeed notified of the raised event
        Assert.IsTrue(listener.GotPinging, "The listener was not notified of the raised event.");
        Assert.IsFalse(listener.GotPing, "The listener was (incorrectly) notified of the raised event.");
        // the moral of the story is...
        //     don't use greedy method name regex's for autowiring, be explicit
        Assert.IsTrue(listener.GotPinged, "The listener was not notified of the raised event.");
    }

    [Test]
    public void TestDefaultMethodMatching()
    {
        // create the event source, and a listener to wire to an event (s) on the source
        PingSource source = new PingSource();
        PingListener listener = new PerversePingListener();

        // wire them up
        IEventHandlerValue wirer = new AutoWiringEventHandlerValue();
        wirer.EventName = "^Ping$";
        wirer.MethodName = ".+${event}.+"; // matches 'WhatClubDoYouUsePingAhGood'
        wirer.Wire(source, listener);

        // raise the event on the source
        source.OnPing();

        // ascertain that the event listener was not notified of the raised event
        Assert.IsTrue(listener.GotPing, "The listener was not notified of the raised event.");
        Assert.IsFalse(listener.GotPinging, "The listener was (incorrectly) notified of an event that wasn't raised.");
        Assert.IsFalse(listener.GotPinged, "The listener was (incorrectly) notified of an event that wasn't raised.");
    }
}

internal class PingEventArgs : EventArgs
{
}

internal delegate bool PingEventHandler(object sender, PingEventArgs e);

internal class PingSource
{
    public event EventHandler Pinging;

    public event PingEventHandler Ping;

    public event EventHandler Pinged;

    public void OnPinging()
    {
        if (Pinging != null)
        {
            Pinging(this, EventArgs.Empty);
        }
    }

    public void OnPing()
    {
        if (Ping != null)
        {
            Ping(this, new PingEventArgs());
        }
    }

    public void OnPinged()
    {
        if (Pinged != null)
        {
            Pinged(this, EventArgs.Empty);
        }
    }
}

internal class PingListener
{
    public void OnPinging(object sender, EventArgs e)
    {
        _pinging = true;
        ++_pingingCount;
    }

    private bool OnPing(object sender, PingEventArgs e)
    {
        _ping = true;
        ++_pingCount;
        return true;
    }

    public void OnPinged(object sender, EventArgs e)
    {
        _pinged = true;
        ++_pingedCount;
    }

    public bool GotPinging
    {
        get
        {
            return _pinging;
        }
    }

    public bool GotPing
    {
        get
        {
            return _ping;
        }
    }

    public bool GotPinged
    {
        get
        {
            return _pinged;
        }
    }

    public int PingingCount
    {
        get
        {
            return _pingingCount;
        }
    }

    public int PingCount
    {
        get
        {
            return _pingCount;
        }
    }

    public int PingedCount
    {
        get
        {
            return _pingedCount;
        }
    }

    protected bool _pinging;
    protected bool _ping;
    protected bool _pinged;

    protected int _pingingCount;
    protected int _pingCount;
    protected int _pingedCount;
}

/// <summary>
/// Specialisation with obtuse names for listener methods.
/// </summary>
internal class PerversePingListener : PingListener
{
    public void PingingListener(object sender, EventArgs e)
    {
        _pinging = true;
    }

    public bool WhatClubDoYouUsePingAhGood(object sender, PingEventArgs e)
    {
        _ping = true;
        return true;
    }

    public void AHotDogImPingedOnMyLiason(object sender, EventArgs e)
    {
        _pinged = true;
    }
}
