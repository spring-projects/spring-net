

using System;
using Spring.Messaging.Ems.Common;
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Connections
{
    public class TestTopicSubscriber : TestMessageConsumer, ITopicSubscriber
    {
        public bool NoLocal
        {
            get { throw new NotImplementedException(); }
        }

        public Topic Topic
        {
            get { throw new NotImplementedException(); }
        }
    }
}