using System;
using Apache.NMS;

namespace Spring.Messaging.Nms.Connection
{
    public class TestConnection : IConnection
    {
        private int startCount;
        private int closeCount;
        private int createSessionCount;

        public int StartCount
        {
            get { return startCount; }
        }

        public int CloseCount
        {
            get { return closeCount; }
        }

        public event ExceptionListener ExceptionListener;

        public ISession CreateSession()
        {
            createSessionCount++;
            return new TestSession();
        }

        public ISession CreateSession(AcknowledgementMode acknowledgementMode)
        {
            createSessionCount++;
            return new TestSession();
        }

        public void Close()
        {
            closeCount++;
        }

        public AcknowledgementMode AcknowledgementMode
        {
            get { return AcknowledgementMode.ClientAcknowledge; }
            set { }
        }

        public string ClientId
        {
            get { return null; }
            set { }
        }

        public void Dispose()
        {
        }

        public void Start()
        {
            startCount++;
        }

        public bool IsStarted
        {
            get
            {
                if (startCount > 0) return true;
                return false;
            }
        }

        public void Stop()
        {         
        }

        public void FireExcpetionEvent(Exception e)
        {
            ExceptionListener(e);
        }
    }
}