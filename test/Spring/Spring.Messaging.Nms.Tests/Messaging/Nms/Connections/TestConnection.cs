using System;
using System.Threading.Tasks;
using Apache.NMS;

namespace Spring.Messaging.Nms.Connections
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

        public IConnectionMetaData MetaData
        {
            get { throw new NotImplementedException(); }
        }

#pragma warning disable 67
        public event ExceptionListener ExceptionListener;
        public event ConnectionInterruptedListener ConnectionInterruptedListener;
        public event ConnectionResumedListener ConnectionResumedListener;
#pragma warning restore 67

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

        public Task<ISession> CreateSessionAsync()
        {
            return Task.FromResult(CreateSession());
        }

        public Task<ISession> CreateSessionAsync(AcknowledgementMode acknowledgementMode)
        {
            return Task.FromResult(CreateSession(acknowledgementMode));
        }

        public ISession CreateSession(AcknowledgementMode acknowledgementMode, TimeSpan requestTimeout)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            closeCount++;
        }

        public Task CloseAsync()
        {
            throw new NotImplementedException();
        }

        public void PurgeTempDestinations()
        {

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

        public TimeSpan RequestTimeout
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
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

        public IRedeliveryPolicy RedeliveryPolicy
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
        }

        public void Start()
        {
            startCount++;
        }

        public Task StartAsync()
        {
            startCount++;
            return Task.CompletedTask;
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

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }

        public void FireExcpetionEvent(Exception e)
        {
            ExceptionListener(e);
        }
    }
}