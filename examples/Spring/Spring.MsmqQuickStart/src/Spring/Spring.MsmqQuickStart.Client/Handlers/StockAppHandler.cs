

using System.Collections;
using Common.Logging;
using Spring.MsmqQuickStart.Client.UI;
using Spring.MsmqQuickStart.Common.Data;

namespace Spring.MsmqQuickStart.Client.Handlers
{
    public class StockAppHandler
    {
        #region Logging Definition

        private readonly ILog log = LogManager.GetLogger(typeof(StockAppHandler));

        #endregion

        private StockController stockController;


        public StockController StockController
        {
            get { return stockController; }
            set { stockController = value; }
        }

        public void Handle(string data)
        {
            log.Info(string.Format("Received market data. " + data));

            // forward to controller to update view
            stockController.UpdateMarketData(data);

        }


        public void Handle(TradeResponse tradeResponse)
        {
            log.Info(string.Format("Received trade resonse.  Ticker = {0}, Price = {1}", tradeResponse.Ticker, tradeResponse.Price));
            stockController.UpdateTrade(tradeResponse);
        }

        public void Handle(object catchAllObject)
        {
            log.Error("could not handle object of type = " + catchAllObject.GetType());
        }
    }
}