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

using NUnit.Framework;

namespace Spring.Objects.Events.Support.Tests;

[TestFixture]
public class EventRegistryTests
{
    #region Helper Classes

    internal delegate void SimpleClientEvent(object sender, MyClientEventArgs args);

    internal delegate string SimpleClientEvent2(MyClientEventArgs args);

    internal class SimpleClient2
    {
        public event SimpleClientEvent MyClientEvent;

        public SimpleClient2()
        {
        }

        public void ClientMethodThatTriggersEvent()
        {
            if (MyClientEvent != null)
            {
                MyClientEvent(this, new MyClientEventArgs("Event raised"));
            }
        }
    }

    internal interface ISimpleClient
    {
        event SimpleClientEvent MyClientEvent1;
        event SimpleClientEvent MyClientEvent2;
        event SimpleClientEvent2 MyClientEvent3;
    }

    internal class SimpleClient : ISimpleClient
    {
        private string _clientName;

        public event SimpleClientEvent MyClientEvent1;
        public event SimpleClientEvent MyClientEvent2;
        public event SimpleClientEvent2 MyClientEvent3;

        public SimpleClient(string clientName)
        {
            _clientName = clientName;
        }

        public void ClientMethodThatTriggersEvent()
        {
            if (MyClientEvent1 != null)
            {
                MyClientEvent1(this, new MyClientEventArgs("Event raised from " + _clientName));
            }
        }

        public void ClientMethodThatTriggersEvent2()
        {
            if (MyClientEvent2 != null)
            {
                MyClientEvent2(this, new MyClientEventArgs("Event raised from " + _clientName));
            }
        }

        public void ClientMethodThatTriggersEvent3()
        {
            if (MyClientEvent3 != null)
            {
                MyClientEvent3(new MyClientEventArgs("Event raised from " + _clientName));
            }
        }
    }

    internal class MyClientEventArgs : EventArgs
    {
        private string _eventMessage;

        public MyClientEventArgs(string eventMessage)
        {
            _eventMessage = eventMessage;
        }

        public string EventMessage
        {
            get { return _eventMessage; }
        }
    }

    internal class SimpleSubscriber
    {
        protected int _eventCount;

        protected bool _eventRaised = false;

        public SimpleSubscriber()
        {
        }

        public int EventCount
        {
            get { return _eventCount; }
        }

        public bool EventRaised
        {
            get { return _eventRaised; }
        }
    }

    internal class EventSubscriber : SimpleSubscriber
    {
        public EventSubscriber()
        {
        }

        public void HandleClientEvents(object sender, MyClientEventArgs args)
        {
            _eventRaised = true;
            _eventCount++;
        }
    }

    internal class NoEventSubscriber : SimpleSubscriber
    {
        public NoEventSubscriber()
        {
        }

        public void HandleClientEvents(object sender)
        {
            _eventRaised = true;
        }
    }

    internal class OtherEventSubscriber : SimpleSubscriber
    {
        public OtherEventSubscriber()
        {
        }

        public string HandleClientEvents(MyClientEventArgs args)
        {
            _eventRaised = true;
            return String.Empty;
        }
    }

    #endregion

    [Test]
    public void RespectsInheritance()
    {
        SimpleClient source = new SimpleClient("foo");

        IEventRegistry registry = new EventRegistry();
        registry.PublishEvents(source);

        EventSubscriber sub = new EventSubscriber();
        Assert.IsFalse(sub.EventRaised, "Event raised");

        source.ClientMethodThatTriggersEvent();
        Assert.IsFalse(sub.EventRaised, "Event raised");

        registry.Subscribe(sub, typeof(ISimpleClient));
        source.ClientMethodThatTriggersEvent();
        Assert.IsTrue(sub.EventRaised, "Event Not Raised");
    }

    [Test]
    public void PublishAllEvents()
    {
        IEventRegistry registry = new EventRegistry();
        SimpleClient client = new SimpleClient("PublishAllEvents");
        registry.PublishEvents(client);
        EventSubscriber sub = new EventSubscriber();
        Assert.IsFalse(sub.EventRaised, "Event raised");

        client.ClientMethodThatTriggersEvent();
        Assert.IsFalse(sub.EventRaised, "Event raised");

        registry.Subscribe(sub);
        client.ClientMethodThatTriggersEvent();
        Assert.IsTrue(sub.EventRaised, "Event Not Raised");
    }

    [Test]
    public void PublishAllEventsMultipleSubscribers()
    {
        IEventRegistry registry = new EventRegistry();
        SimpleClient client = new SimpleClient("PublishAllEvents");
        registry.PublishEvents(client);
        EventSubscriber sub = new EventSubscriber();
        EventSubscriber sub2 = new EventSubscriber();
        Assert.IsFalse(sub.EventRaised, "Event raised");
        Assert.IsFalse(sub2.EventRaised, "Event raised");

        client.ClientMethodThatTriggersEvent();
        Assert.IsFalse(sub.EventRaised, "Event raised");
        Assert.IsFalse(sub2.EventRaised, "Event raised");

        registry.Subscribe(sub);
        registry.Subscribe(sub2);
        client.ClientMethodThatTriggersEvent();
        Assert.IsTrue(sub.EventRaised, "Event Not Raised");
        Assert.IsTrue(sub2.EventRaised, "Event Not Raised");
    }

    [Test]
    public void PublishAllEventsMultipleSubscribersAndUnsubscribe()
    {
        IEventRegistry registry = new EventRegistry();
        SimpleClient client = new SimpleClient("PublishAllEvents");
        registry.PublishEvents(client);
        EventSubscriber sub = new EventSubscriber();
        EventSubscriber sub2 = new EventSubscriber();
        registry.Subscribe(sub);
        registry.Subscribe(sub2);
        client.ClientMethodThatTriggersEvent();
        Assert.IsTrue(sub.EventRaised, "Event Not Raised");
        Assert.IsTrue(sub2.EventRaised, "Event Not Raised");
        Assert.AreEqual(1, sub.EventCount);
        Assert.AreEqual(1, sub2.EventCount);

        registry.Unsubscribe(sub2);
        client.ClientMethodThatTriggersEvent();
        Assert.AreEqual(2, sub.EventCount);
        Assert.AreEqual(1, sub2.EventCount);
    }

    [Test]
    public void PublishAllEventsSubscribeToNamedEvents()
    {
        IEventRegistry registry = new EventRegistry();
        SimpleClient client = new SimpleClient("PublishAllEvents");
        SimpleClient2 client2 = new SimpleClient2();

        registry.PublishEvents(client);
        registry.PublishEvents(client2);

        EventSubscriber sub = new EventSubscriber();
        EventSubscriber sub2 = new EventSubscriber();

        Assert.IsFalse(sub.EventRaised, "Event raised");
        Assert.IsFalse(sub2.EventRaised, "Event raised");

        client.ClientMethodThatTriggersEvent();
        client2.ClientMethodThatTriggersEvent();
        Assert.IsFalse(sub.EventRaised, "Event raised");
        Assert.IsFalse(sub2.EventRaised, "Event raised");

        registry.Subscribe(sub, typeof(SimpleClient));
        registry.Subscribe(sub2, typeof(SimpleClient2));

        client.ClientMethodThatTriggersEvent();
        Assert.IsTrue(sub.EventRaised, "Event Not Raised");
        Assert.IsFalse(sub2.EventRaised, "Event raised");

        client2.ClientMethodThatTriggersEvent();
        Assert.IsTrue(sub.EventRaised, "Event Not Raised");
        Assert.IsTrue(sub2.EventRaised, "Event Not Raised");
    }

    [Test]
    public void NoValidEventHandlersOrEventsToSubscribeto()
    {
        IEventRegistry registry = new EventRegistry();
        SimpleClient client = new SimpleClient("PublishAllEvents");
        NoEventSubscriber sub = new NoEventSubscriber();
        registry.PublishEvents(client);

        Assert.IsFalse(sub.EventRaised, "Event raised");
        client.ClientMethodThatTriggersEvent();
        Assert.IsFalse(sub.EventRaised, "Event raised");

        registry.Subscribe(sub);
        client.ClientMethodThatTriggersEvent();
        Assert.IsFalse(sub.EventRaised, "Event Raised");
    }

    [Test]
    public void NoPublishers()
    {
        IEventRegistry registry = new EventRegistry();
        SimpleClient client = new SimpleClient("PublishAllEvents");
        SimpleSubscriber sub = new SimpleSubscriber();
        Assert.IsFalse(sub.EventRaised, "Event raised");

        client.ClientMethodThatTriggersEvent();
        Assert.IsFalse(sub.EventRaised, "Event raised");

        registry.Subscribe(sub);
        client.ClientMethodThatTriggersEvent();
        Assert.IsFalse(sub.EventRaised, "Event Raised");
    }

    [Test]
    public void PublishAllEventsAndSubscribeToSome()
    {
        IEventRegistry registry = new EventRegistry();
        SimpleClient client = new SimpleClient("PublishAllEventsAndSubscribeToSome");
        registry.PublishEvents(client);
        EventSubscriber sub = new EventSubscriber();
        OtherEventSubscriber sub2 = new OtherEventSubscriber();
        Assert.IsFalse(sub.EventRaised, "Event raised");
        Assert.IsFalse(sub2.EventRaised, "Event raised");

        client.ClientMethodThatTriggersEvent();
        Assert.IsFalse(sub.EventRaised, "Event raised");

        client.ClientMethodThatTriggersEvent3();
        Assert.IsFalse(sub2.EventRaised, "Event raised");

        registry.Subscribe(sub);
        registry.Subscribe(sub2);
        client.ClientMethodThatTriggersEvent();
        client.ClientMethodThatTriggersEvent3();
        Assert.IsTrue(sub.EventRaised, "Event Not Raised");
        Assert.IsTrue(sub2.EventRaised, "Event Not Raised");
    }
}
