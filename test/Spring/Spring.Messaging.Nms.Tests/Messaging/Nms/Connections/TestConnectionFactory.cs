

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

        #endregion
    }
}