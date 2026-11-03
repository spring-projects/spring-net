using NUnit.Framework;

namespace Spring.Objects.Factory.Config;

[TestFixture]
public class EventValuesTests
{
    [Test]
    public void EmptyEventValues()
    {
        EventValues eventValues = new EventValues();
        Assert.That(eventValues.Events.Count, Is.EqualTo(0));
    }

    [Test]
    public void OneEventValue()
    {
        EventValues eventValues = new EventValues();
        eventValues.AddHandler(new MyEventHandler());
        Assert.That(eventValues.Events.Count, Is.EqualTo(1));
    }

    [Test]
    public void TwoEventHandlersForSameEvent()
    {
        EventValues eventValues = new EventValues();
        eventValues.AddHandler(new MyEventHandler());
        eventValues.AddHandler(new MyEventHandler());
        Assert.That(eventValues.Events.Count, Is.EqualTo(1));
    }

    [Test]
    public void CopyEventHandlerValues()
    {
        EventValues eventValues = new EventValues();
        eventValues.AddHandler(new MyEventHandler());
        eventValues.AddHandler(new MyEventHandler());
        Assert.That(eventValues.Events.Count, Is.EqualTo(1));
        EventValues eventValues2 = new EventValues(eventValues);
        Assert.That(eventValues2.Events.Count, Is.EqualTo(1));
    }

    [Test]
    public void NullEventHandlerValue()
    {
        EventValues eventValues = new EventValues();
        eventValues.AddAll(null);
        Assert.That(eventValues.Events.Count, Is.EqualTo(0));
    }

    internal class MyEventHandler : IEventHandlerValue
    {
        private object _source = new object();
        private string _eventName = "MyEvent";
        private string _methodName = "MyEventHandlerMethod";

        public object Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }

        public string EventName
        {
            get
            {
                return _eventName;
            }
            set
            {
                _eventName = value;
            }
        }

        public string MethodName
        {
            get
            {
                return _methodName;
            }
            set
            {
                _methodName = value;
            }
        }

        public void Wire(object source, object handler)
        {
        }
    }

    internal class MyEventHandler2 : IEventHandlerValue
    {
        private object _source = new object();
        private string _eventName = "MyEvent";
        private string _methodName = "MyEventHandlerMethod";

        public object Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }

        public string EventName
        {
            get
            {
                return _eventName;
            }
            set
            {
                _eventName = value;
            }
        }

        public string MethodName
        {
            get
            {
                return _methodName;
            }
            set
            {
                _methodName = value;
            }
        }

        public void Wire(object source, object handler)
        {
        }
    }
}
