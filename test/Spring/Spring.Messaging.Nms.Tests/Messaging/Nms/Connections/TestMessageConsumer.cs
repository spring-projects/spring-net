

using System;
using Apache.NMS;

namespace Spring.Messaging.Nms.Connections
{
    public class TestMessageConsumer : IMessageConsumer
    {

        public event MessageListener Listener;

        public IMessage Receive()
        {
            throw new NotImplementedException();
        }

        public IMessage Receive(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public IMessage ReceiveNoWait()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}