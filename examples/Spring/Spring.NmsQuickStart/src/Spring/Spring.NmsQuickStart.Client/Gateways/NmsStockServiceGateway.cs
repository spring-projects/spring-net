


using System;
using Apache.NMS;
using Spring.Messaging.Nms.Core;
using Spring.NmsQuickStart.Common.Data;
using Spring.Objects.Factory;

namespace Spring.NmsQuickStart.Client.Gateways
{
    public class NmsStockServiceGateway : NmsGatewaySupport, IStockService
    {
        private IDestination defaultReplyToQueue;
        
        public IDestination DefaultReplyToQueue
        {
            set { defaultReplyToQueue = value; }
        }

        public void Send(TradeRequest tradeRequest)
        {            
            NmsTemplate.ConvertAndSendWithDelegate(tradeRequest, delegate(IMessage message)
                                                                     {
                                                                         message.NMSReplyTo = defaultReplyToQueue;
                                                                         message.NMSCorrelationID = new Guid().ToString();
                                                                         return message;
                                                                     });
        }        
    }
}