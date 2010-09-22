

using System;
using Apache.NMS;

namespace Spring.Messaging.Nms.Connections
{
    public class TestConnectionFactory : IConnectionFactory
    {
        #region IConnectionFactory Members

        public IConnection CreateConnection()
        {
            return new TestConnection();
        }

        public IConnection CreateConnection(string userName, string password)
        {
            return new TestConnection();
        }

        public Uri BrokerUri
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IRedeliveryPolicy RedeliveryPolicy
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ConsumerTransformerDelegate ConsumerTransformer
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ProducerTransformerDelegate ProducerTransformer
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion
    }
}