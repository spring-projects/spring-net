using System;
using System.Collections;
using Spring.Messaging.Ems.Common;

namespace Spring.Messaging.Ems.Core
{
    public class SimpleGateway : EmsGatewaySupport
    {
        public void Publish(string ticker, double price)
        {            
            EmsTemplate.SendWithDelegate("APP.STOCK.MARKETDATA",
                          delegate(ISession session)
                          {
                              MapMessage message = session.CreateMapMessage();
                              message.SetString("TICKER", ticker);
                              message.SetDouble("PRICE", price);
                              message.Priority = 5;
                              return message;
                          });
        }

        public void PublishUsingDict(string ticker, double price)
        {
            IDictionary marketData = new Hashtable();
            marketData.Add("TICKER", ticker);
            marketData.Add("PRICE", price);
            EmsTemplate.ConvertAndSendWithDelegate("APP.STOCK.MARKETDATA", marketData,
                     delegate(Message message)
                     {
                         message.Priority = 5;
                         message.CorrelationID = new Guid().ToString();
                         return message;
                     });
        }
    }
}