using NUnit.Framework;

namespace Spring.Objects.Factory.Config;

[TestFixture]
public class EventValuesTests
{
    [Test]
    public void EmptyEventValues()
    {
        EventValues eventValues = new EventValues();
        Assert.AreEqual(0, eventValues.Events.Count);
    }

    [Test]
    public void OneEventValue()
    {
        EventValues eventValues = new EventValues();
        eventValues.AddHandler(new MyEventHandler());
        Assert.AreEqual(1, eventValues.Events.Count);
    }

    [Test]
    public void TwoEventHandlersForSameEvent()
    {
        EventValues eventValues = new EventValues();
        eventValues.AddHandler(new MyEventHandler());
        eventValues.AddHandler(new MyEventHandler());
        Assert.AreEqual(1, eventValues.Events.Count);
    }

    [Test]
    public void CopyEventHandlerValues()
    {
        EventValues eventValues = new EventValues();
        eventValues.AddHandler(new MyEventHandler());
        eventValues.AddHandler(new MyEventHandler());
        Assert.AreEqual(1, eventValues.Events.Count);
        EventValues eventValues2 = new EventValues(eventValues);
        Assert.AreEqual(1, eventValues2.Events.Count);
    }

    [Test]
    public void NullEventHandlerValue()
    {
        EventValues eventValues = new EventValues();
        eventValues.AddAll(null);
        Assert.AreEqual(0, eventValues.Events.Count);
    }

    internal class MyEventHandler : IEventHandlerValue
    {
        #region IEventHandlerValue Members

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

        #endregion
    }

    internal class MyEventHandler2 : IEventHandlerValue
    {
        #region IEventHandlerValue Members

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

        #endregion
    }
}
