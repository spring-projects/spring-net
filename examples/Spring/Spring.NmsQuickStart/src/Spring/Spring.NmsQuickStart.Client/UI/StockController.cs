

using System.Collections;
using Spring.NmsQuickStart.Client.Gateways;
using Spring.NmsQuickStart.Common.Data;

namespace Spring.NmsQuickStart.Client.UI
{
    public class StockController
    {
        private StockForm stockForm;
              
        private IStockServiceGateway stockServiceGateway;


        
        public StockForm StockForm
        {
            get { return stockForm; }
            set { stockForm = value; }
        }

        public IStockServiceGateway StockServiceGateway
        {
            get { return stockServiceGateway; }
            set { stockServiceGateway = value; }
        }

        public void SendTradeRequest()
        {
            TradeRequest tradeRequest = new TradeRequest();
            tradeRequest.AccountName = "ACCT-123";
            tradeRequest.BuyRequest = true;
            tradeRequest.OrderType = "MARKET";
            tradeRequest.Quantity = 314000000;
            tradeRequest.RequestID = "REQ-1";
            tradeRequest.Ticker = "CSCO";
            tradeRequest.UserName = "Joe Trader";
            
            stockServiceGateway.Send(tradeRequest);
        }
        
        
        public void UpdateMarketData(IDictionary marketDataDict)
        {
            stockForm.UpdateMarketData(marketDataDict);
        }

        public void UpdateTrade(Trade trade)
        {
            stockForm.UpdateTrade(trade);
        }

    }
}
