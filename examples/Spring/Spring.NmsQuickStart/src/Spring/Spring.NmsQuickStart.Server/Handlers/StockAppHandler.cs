

using Common.Logging;
using Spring.NmsQuickStart.Common.Data;

namespace Spring.NmsQuickStart.Server.Handlers
{
    public class StockAppHandler
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(StockAppHandler));


        public Trade Handle(TradeRequest tradeRequest)
        {
            log.Info("Received TradeRequest");
            return new Trade();
        }
    }
}