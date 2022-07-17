

using System;
using System.Threading.Tasks;
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

        public Task<IConnection> CreateConnectionAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IConnection> CreateConnectionAsync(string userName, string password)
        {
            throw new NotImplementedException();
        }

        public INMSContext CreateContext()
        {
            throw new NotImplementedException();
        }

        public INMSContext CreateContext(AcknowledgementMode acknowledgementMode)
        {
            throw new NotImplementedException();
        }

        public INMSContext CreateContext(string userName, string password)
        {
            throw new NotImplementedException();
        }

        public INMSContext CreateContext(string userName, string password, AcknowledgementMode acknowledgementMode)
        {
            throw new NotImplementedException();
        }

        public Task<INMSContext> CreateContextAsync()
        {
            throw new NotImplementedException();
        }

        public Task<INMSContext> CreateContextAsync(AcknowledgementMode acknowledgementMode)
        {
            throw new NotImplementedException();
        }

        public Task<INMSContext> CreateContextAsync(string userName, string password)
        {
            throw new NotImplementedException();
        }

        public Task<INMSContext> CreateContextAsync(string userName, string password, AcknowledgementMode acknowledgementMode)
        {
            throw new NotImplementedException();
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