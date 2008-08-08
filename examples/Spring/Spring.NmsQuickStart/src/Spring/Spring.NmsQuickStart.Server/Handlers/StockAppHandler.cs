

using System.Collections;
using Common.Logging;
using Spring.NmsQuickStart.Common.Data;
using Spring.NmsQuickStart.Server.Services;

namespace Spring.NmsQuickStart.Server.Handlers
{
    public class StockAppHandler
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(StockAppHandler));

        private IExecutionVenueService executionVenueService;

        private ICreditCheckService creditCheckService;

        private ITradingService tradingService;
        
        public TradeResponse Handle(TradeRequest tradeRequest)
        {
            TradeResponse tradeResponse;
            IList errors = new ArrayList();
            if (creditCheckService.CanExecute(tradeRequest, errors))
            {
                tradeResponse = executionVenueService.ExecuteTradeRequest(tradeRequest);
                tradingService.ProcessTrade(tradeRequest, tradeResponse);
            }
            else
            {
                tradeResponse = new TradeResponse();
                tradeResponse.Error = true;
                tradeResponse.ErrorMessage = errors[0].ToString();
            }
            return tradeResponse;
        }
    }
}