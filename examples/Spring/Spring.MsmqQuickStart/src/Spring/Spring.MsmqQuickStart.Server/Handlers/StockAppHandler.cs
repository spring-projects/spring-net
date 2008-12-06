

using System.Collections;
using System.Threading;
using Common.Logging;
using Spring.MsmqQuickStart.Common.Data;
using Spring.MsmqQuickStart.Server.Services;
using Spring.Util;

namespace Spring.MsmqQuickStart.Server.Handlers
{
    public class StockAppHandler
    {
        private readonly ILog log = LogManager.GetLogger(typeof(StockAppHandler));

        private readonly IExecutionVenueService executionVenueService;
        private readonly ICreditCheckService creditCheckService;
        private readonly ITradingService tradingService;

        public StockAppHandler(IExecutionVenueService executionVenueService, ICreditCheckService creditCheckService, ITradingService tradingService)
        {
            this.executionVenueService = executionVenueService;
            this.creditCheckService = creditCheckService;
            this.tradingService = tradingService;
        }

        public TradeResponse Handle(TradeRequest tradeRequest)
        {
            log.Info("received trade request - sleeping 2s to simulate long-running task");
            TradeResponse tradeResponse;
            ArrayList errors = new ArrayList();
            if (creditCheckService.CanExecute(tradeRequest, errors))
            {
                tradeResponse = executionVenueService.ExecuteTradeRequest(tradeRequest);
            }
            else
            {
                tradeResponse = new TradeResponse();
                tradeResponse.Error = true;
                tradeResponse.ErrorMessage = StringUtils.ArrayToCommaDelimitedString(errors.ToArray());
                
            }
            tradingService.ProcessTrade(tradeRequest, tradeResponse);

            Thread.Sleep(2000);
            return tradeResponse;
        }
    }
}