

using Spring.MsmqQuickStart.Common.Data;
using Spring.MsmqQuickStart.Server.Services;


namespace Spring.MsmqQuickStart.Server.Services.Stubs
{
    public class TradingServiceStub : ITradingService
    {
        public void ProcessTrade(TradeRequest request, TradeResponse response)
        {
            //do nothing implementation, typical implementations would persist state to the database.
        }
    }
}