using System;
using Apache.NMS;
using Spring.Messaging.Nms.Support.Converter;
using Spring.NmsQuickStart.Common.Bo;

namespace Spring.NmsQuickStart.Common.Converters
{
    public class TradeRequestConverter : AbstractNamedMessageConverter
    {
        public override IMessage ToMessage(object objectToConvert, ISession session)
        {
            TradeRequest tradeRequest = objectToConvert as TradeRequest;
            if (tradeRequest == null)
            {
                throw new MessageConversionException("TradeRequestConverter can not convert object of type " +
                                                     objectToConvert.GetType());
            }

            try
            {
                IMapMessage mm = session.CreateMapMessage();


                mm.Body.SetString("accountName", tradeRequest.AccountName);
                mm.Body.SetBool("buyRequest", tradeRequest.BuyRequest);
                mm.Body.SetString("orderType", tradeRequest.OrderType);
                mm.Body.SetDouble("price", tradeRequest.Price);
                mm.Body.SetLong("quantity", tradeRequest.Quantity);
                mm.Body.SetString("requestId", tradeRequest.RequestId);
                mm.Body.SetString("ticker", tradeRequest.Ticker);
                mm.Body.SetString("username", tradeRequest.UserName);

                return mm;
                
            } catch (Exception e)
            {
                throw new MessageConversionException("Could not convert TradeRequest to message", e);
            }
        }

        public override object FromMessage(IMessage messageToConvert)
        {
            IMapMessage mm = messageToConvert as IMapMessage;
            if (mm != null)
            {
                TradeRequest tradeRequest = new TradeRequest();
                tradeRequest.AccountName = mm.Body.GetString("accountName");
                tradeRequest.BuyRequest = mm.Body.GetBool("buyRequest");
                tradeRequest.OrderType = mm.Body.GetString("orderType");
                tradeRequest.Price = mm.Body.GetDouble("price");
                tradeRequest.Quantity = mm.Body.GetLong("quantity");
                tradeRequest.RequestId = mm.Body.GetString("requestId");
                tradeRequest.Ticker = mm.Body.GetString("ticker");
                tradeRequest.UserName = mm.Body.GetString("username");

                return tradeRequest;
            }
            else
            {
                throw new MessageConversionException("Not of expected type MapMessage.  Message = " + messageToConvert);
            }
        }

        public override Type TargetType
        {
            get { return typeof (TradeRequest); }
        }
    }    
}