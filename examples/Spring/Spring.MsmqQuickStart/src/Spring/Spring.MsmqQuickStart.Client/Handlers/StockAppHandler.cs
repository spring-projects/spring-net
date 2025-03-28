using Microsoft.Extensions.Logging;
using Spring.MsmqQuickStart.Client.UI;
using Spring.MsmqQuickStart.Common.Data;

namespace Spring.MsmqQuickStart.Client.Handlers
{
    public class StockAppHandler
    {
        #region Logging Definition

        private readonly ILogger log = LogManager.GetLogger(typeof(StockAppHandler));

        #endregion

        private StockController stockController;


        public StockController StockController
        {
            get { return stockController; }
            set { stockController = value; }
        }

        public void Handle(string data)
        {
            log.LogInformation("Received market data. " + data);

            // forward to controller to update view
            stockController.UpdateMarketData(data);

        }


        public void Handle(TradeResponse tradeResponse)
        {
            log.LogInformation("Received trade resonse.  Ticker = {TradeResponseTicker}, Price = {TradeResponsePrice}", tradeResponse.Ticker, tradeResponse.Price);
            stockController.UpdateTrade(tradeResponse);
        }

        public void Handle(object catchAllObject)
        {
            log.LogError("could not handle object of type {ObjectType}", catchAllObject.GetType());
        }
    }
}
