

using System.Collections;
using Common.Logging;
using Spring.NmsQuickStart.Client.UI;
using Spring.NmsQuickStart.Common.Data;

namespace Spring.NmsQuickStart.Client.Handlers
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

        public void Handle(Hashtable data)
        {
            log.Info(string.Format("Received market data.  Ticker = {0}, Price = {1}", data["TICKER"], data["PRICE"]));

            // forward to controller to update view
            stockController.UpdateMarketData(data);

        }


        public void Handle(Trade trade)
        {
            log.Info(string.Format("Received trade.  Ticker = {0}, Price = {1}", trade.Ticker, trade.Price));
            stockController.UpdateTrade(trade);
        }

        public void Handle(object catchAllObject)
        {
            log.Error("could not handle object of type = " + catchAllObject.GetType());
        }
    }
}