


using Apache.NMS;
using Spring.Messaging.Nms.Core;
using Spring.NmsQuickStart.Common.Data;
using Spring.Objects.Factory;

namespace Spring.NmsQuickStart.Client.Gateways
{
    public class NmsStockServiceGateway : NmsGatewaySupport, IStockService, IInitializingObject
    {
        private IDestination defaultReplyToQueue;
        
        public IDestination DefaultReplyToQueue
        {
            set { defaultReplyToQueue = value; }
        }

        public void Send(TradeRequest tradeRequest)
        {            
            NmsTemplate.ConvertAndSend(tradeRequest, new ReplyToPostProcessor(defaultReplyToQueue));
        }

        
        public class ReplyToPostProcessor : IMessagePostProcessor
        {
            private IDestination replyToDestination;

            public ReplyToPostProcessor(IDestination replyToDestination)
            {
                this.replyToDestination = replyToDestination;
            }

            public IMessage PostProcessMessage(IMessage message)
            {
                message.NMSReplyTo = replyToDestination;
                return message;
            }
        }

    }
}