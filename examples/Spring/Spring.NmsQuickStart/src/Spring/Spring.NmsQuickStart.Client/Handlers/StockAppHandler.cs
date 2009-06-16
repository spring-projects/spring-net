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