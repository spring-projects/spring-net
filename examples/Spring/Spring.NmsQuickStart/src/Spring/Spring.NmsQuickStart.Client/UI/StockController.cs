#region License

/*
 * Copyright 2002-2009 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion


using System.Collections;
using Spring.NmsQuickStart.Client.Gateways;
using Spring.NmsQuickStart.Common.Data;

namespace Spring.NmsQuickStart.Client.UI
{
    /// <summary>
    /// Handles requests from the UI and forwards them to the remote service.  Messages recieved from
    /// the service routed through this controller to update the UI. 
    /// </summary>
    public class StockController
    {
        private StockForm stockForm;
              
        private IStockService stockService;
        
        public StockForm StockForm
        {
            get { return stockForm; }
            set { stockForm = value; }
        }

        public IStockService StockService
        {
            get { return stockService; }
            set { stockService = value; }
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
            
            stockService.Send(tradeRequest);
        }       
        
        public void UpdateMarketData(IDictionary marketDataDict)
        {
            stockForm.UpdateMarketData(marketDataDict);
        }

        public void UpdateTrade(TradeResponse tradeResponse)
        {
            stockForm.UpdateTrade(tradeResponse);
        }

    }
}
