


using System;
using Spring.Messaging.Ems.Core;
using Spring.EmsQuickStart.Common.Data;
using Spring.Objects.Factory;
using TIBCO.EMS;

namespace Spring.EmsQuickStart.Client.Gateways
{
    public class EmsStockServiceGateway : EmsGatewaySupport, IStockService
    {
        private Destination defaultReplyToQueue;
        
        public Destination DefaultReplyToQueue
        {
            set { defaultReplyToQueue = value; }
        }

        public void Send(TradeRequest tradeRequest)
        {            
            EmsTemplate.ConvertAndSendWithDelegate(tradeRequest, delegate(Message message)
                                                                     {
                                                                         message.ReplyTo = defaultReplyToQueue;
                                                                         message.CorrelationID = new Guid().ToString();
                                                                         return message;
                                                                     });
        }        
    }
}