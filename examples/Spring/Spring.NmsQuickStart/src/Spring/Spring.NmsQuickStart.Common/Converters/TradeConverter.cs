

using System;
using Apache.NMS;
using Spring.Messaging.Nms.Support.Converter;
using Spring.NmsQuickStart.Common.Bo;

namespace Spring.NmsQuickStart.Common.Converters
{
    public class TradeConverter : AbstractNamedMessageConverter
    {
        public override Type TargetType
        {
            get { return typeof (Trade); }
        }

        public override IMessage ToMessage(object objectToConvert, ISession session)
        {
            Trade trade = objectToConvert as Trade;
            if (trade == null)
            {
                throw new MessageConversionException("TradeConverter can not convert object of type " +
                                                     objectToConvert.GetType());
            }
            try
            {
                IMapMessage mm = session.CreateMapMessage();

                mm.Body.SetString("orderType", trade.OrderType);
                mm.Body.SetDouble("price", trade.Price);
                mm.Body.SetLong("quantity", trade.Quantity);
                mm.Body.SetString("ticker", trade.Ticker);

                return mm;

            }
            catch (Exception e)
            {
                throw new MessageConversionException("Could not convert TradeRequest to message", e);
            }
        }

        public override object FromMessage(IMessage messageToConvert)
        {
            IMapMessage mm = messageToConvert as IMapMessage;
            if (mm != null)
            {
                Trade trade = new Trade();
                trade.OrderType = mm.Body.GetString("orderType");
                trade.Price = mm.Body.GetDouble("price");
                trade.Quantity = mm.Body.GetLong("quantity");
                trade.Ticker = mm.Body.GetString("ticker");
                return trade;
            }
            else
            {
                throw new MessageConversionException("Not of expected type MapMessage.  Message = " + messageToConvert);
            }
        }
    }
}