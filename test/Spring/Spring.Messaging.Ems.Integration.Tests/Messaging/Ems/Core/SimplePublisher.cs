using Spring.Messaging.Ems.Common;

namespace Spring.Messaging.Ems.Core
{
    public class SimplePublisher
    {
        private EmsTemplate emsTemplate;

        public SimplePublisher()
        {
            emsTemplate = new EmsTemplate(new EmsConnectionFactory("tcp://localhost:7222"));
        }

        public void Publish(string ticker, double price)
        {
            emsTemplate.SendWithDelegate("APP.STOCK.MARKETDATA",
                          delegate(ISession session)
                          {
                              MapMessage message = session.CreateMapMessage();
                              message.SetString("TICKER", ticker);
                              message.SetDouble("PRICE", price);
                              message.Priority = 5;
                              return message;
                          });
        }
    }
}