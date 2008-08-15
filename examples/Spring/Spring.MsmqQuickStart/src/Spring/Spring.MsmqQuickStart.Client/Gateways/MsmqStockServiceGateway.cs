


using System;
using System.Messaging;
using Spring.Messaging.Core;
using Spring.MsmqQuickStart.Common.Data;

namespace Spring.MsmqQuickStart.Client.Gateways
{
    public class MsmqStockServiceGateway : MessageQueueGatewaySupport, IStockService
    {
        private Random random = new Random();

        private string defaultResponseQueueObjectName;

        public string DefaultResponseQueueObjectName
        {
            set { defaultResponseQueueObjectName = value; }
        }

        public void Send(TradeRequest tradeRequest)
        {
            // post process message from conversion before sending
            MessageQueueTemplate.ConvertAndSend(tradeRequest, delegate(Message message)
                                                                  {
                                                                      message.ResponseQueue = GetResponseQueue();
                                                                      message.AppSpecific = random.Next();
                                                                      return message;
                                                                  });
        }
       
        private MessageQueue GetResponseQueue()
        {
            return MessageQueueFactory.CreateMessageQueue(defaultResponseQueueObjectName);
        }
       
    }
}