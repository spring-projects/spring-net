using Spring.Collections;
using Spring.Messaging.Nms.Support;
using Apache.NMS;

namespace Spring.Messaging.Nms.Listener
{
    public class SimpleMessageListenerContainer : AbstractMessageListenerContainer
    {
        private bool pubSubNoLocal = false;

        private int concurrentConsumers = 1;

        //private TaskExecutor taskExecutor;

        private ISet sessions;

        private ISet consumers;


        public int ConcurrentConsumers
        {
            get { return concurrentConsumers; }
            set { concurrentConsumers = value; }
        }

        public bool PubSubNoLocal
        {
            get { return pubSubNoLocal; }
            set { pubSubNoLocal = value; }
        }

        protected override bool SharedConnectionEnabled
        {
            get { return true; }
        }

        protected override void RegisterListener()
        {
            this.sessions = new HashedSet();
            this.consumers = new HashedSet();
            for (int i = 0; i < this.concurrentConsumers; i++)
            {
                ISession session = CreateSession(SharedConnection);
                IMessageConsumer consumer = CreateListenerConsumer(session);
                this.sessions.Add(session);
                this.consumers.Add(consumer);
            }
        }

        private IMessageConsumer CreateListenerConsumer(ISession session)
        {
            IDestination destination = Destination;
            if (destination == null)
            {
                destination = ResolveDestinationName(session, DestinationName);
            }
            IMessageConsumer consumer = CreateConsumer(session, destination);
            //TODO TaskExectuor abstraction would go here...
			
	        SimpleMessageListener listener = new SimpleMessageListener(this, session);
            consumer.Listener += new Apache.NMS.MessageListener(listener.OnMessage); 
            return consumer;
        }

        protected override void DestroyListener()
        {
            logger.Debug("Closing NMS IMessageConsumers");
            foreach (IMessageConsumer messageConsumer in consumers)
            {
                NmsUtils.CloseMessageConsumer(messageConsumer);
            }
            logger.Debug("Closing NMS ISessions");
            foreach (ISession session in sessions)
            {
                NmsUtils.CloseSession(session);
            }
        }

        public override void AfterPropertiesSet()
        {
            if (this.concurrentConsumers <= 0)
            {
                throw new System.ArgumentException("concurrentConsumers value must be at least 1 (one)");
            }
            if (SubscriptionDurable && this.concurrentConsumers != 1)
            {
                throw new System.ArgumentException("Only 1 concurrent consumer supported for durable subscription");
            }

            base.AfterPropertiesSet();
        }

        protected IMessageConsumer CreateConsumer(ISession session, IDestination destination)
        {
            // Only pass in the NoLocal flag in case of a Topic:
            // Some NMS providers, such as WebSphere MQ 6.0, throw IllegalStateException
            // in case of the NoLocal flag being specified for a Queue.
            if (PubSubDomain)
            {
                if (SubscriptionDurable && destination is ITopic)
                {
                    return session.CreateDurableConsumer(
                        (ITopic) destination, DurableSubscriptionName, MessageSelector, PubSubNoLocal);
                }
                else
                {
                    return session.CreateConsumer(destination, MessageSelector, PubSubNoLocal);
                }
            }
            else
            {
                return session.CreateConsumer(destination, MessageSelector);
            }
        }
    }

    internal class SimpleMessageListener : IMessageListener
    {
        private SimpleMessageListenerContainer container;
        private ISession session;

        public SimpleMessageListener(SimpleMessageListenerContainer container, ISession session)
        {
            this.container = container;
            this.session = session;
        }

        public void OnMessage(IMessage message)
        {
            container.ExecuteListener(session, message);
        }
    }
}
