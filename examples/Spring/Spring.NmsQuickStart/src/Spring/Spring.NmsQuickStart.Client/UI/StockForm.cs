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

using System;
using System.Collections;
using System.Windows.Forms;
using Common.Logging;
using Spring.Context.Support;
using Spring.NmsQuickStart.Common.Data;

namespace Spring.NmsQuickStart.Client.UI
{
    public partial class StockForm : Form
    {
        #region Logging Definition

        private static readonly ILog log = LogManager.GetLogger(typeof (StockForm));

        #endregion

        private StockController stockController;

        public StockForm()
        {
            InitializeComponent();
            stockController = ContextRegistry.GetContext()["StockController"] as StockController;
            stockController.StockForm = this;
        }

        public StockController Controller
        {
            set { stockController = value; }
        }

        private void OnSendTradeRequest(object sender, EventArgs e)
        {
            //In this simple example no data is collected from the view.
            //Instead a hardcoded trade request is created in the controller.
            tradeRequestStatusTextBox.Text = "Request Pending...";
            stockController.SendTradeRequest();            
            log.Info("Sent trade request.");
        }

        public void UpdateTrade(TradeResponse trade)
        {
            Invoke(new MethodInvoker(
                       delegate
                           {
                               tradeRequestStatusTextBox.Text = "Confirmed. " + trade.Ticker + " " + trade.Price;
                           }));
        }

        public void UpdateMarketData(IDictionary marketDataDict)
        {
            Invoke(new MethodInvoker(
                       delegate
                           {                               
                               marketDataListBox.Items.Add(marketDataDict["TICKER"] + " " + marketDataDict["PRICE"]);
                           }));
        }

        private void OnPortfolioRequest(object sender, EventArgs e)
        {
            MessageBox.Show("Get Portfolio operation not yet implemented.", "NmsQuickStart");
        }


    }
}