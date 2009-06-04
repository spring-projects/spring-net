using System;
using Spring.Messaging.Ems.Common;
using Spring.Messaging.Nms.Connections;
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Connections
{
    public class TestConnection : IConnection
    {
        private int startCount;
        private int closeCount;
        private int createSessionCount;
        private string clientId;
        private IExceptionListener exceptionListener;


        public event EMSExceptionHandler EMSExceptionHandler;

        public Connection NativeConnection
        {
            get { throw new NotImplementedException(); }
        }

        public string ActiveURL
        {
            get { throw new NotImplementedException(); }
        }

        public long ConnID
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSecure
        {
            get { throw new NotImplementedException(); }
        }

        public ConnectionMetaData MetaData
        {
            get { throw new NotImplementedException(); }
        }

        public string ClientID
        {
            get { return clientId; }
            set { clientId = value; }
        }

        public IExceptionListener ExceptionListener
        {
            get { return exceptionListener; }
            set { exceptionListener = value; }
        }

        public void Close()
        {
            closeCount++;
        }


        public void Start()
        {
            startCount++;
        }

        public void Stop()
        {         
        }


        public ISession CreateSession(bool transacted, int acknowledgementMode)
        {
            createSessionCount++;
            return new TestSession();
        }

        public ISession CreateSession(bool transacted, SessionMode acknowledgementMode)
        {
            createSessionCount++;
            return new TestSession();
        }

        // To be ported methods



       // utility methods

        public bool IsStarted
        {
            get
            {
                if (startCount > 0) return true;
                return false;
            }
        }

        public void FireExcpetionEvent(EMSException e)
        {
            exceptionListener.OnException(e);
        }

        public int StartCount
        {
            get { return startCount; }
        }

        public int CloseCount
        {
            get { return closeCount; }
        }
    }
}