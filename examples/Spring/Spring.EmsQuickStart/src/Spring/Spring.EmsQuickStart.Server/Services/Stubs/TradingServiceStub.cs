

using Spring.EmsQuickStart.Common.Data;
using Spring.EmsQuickStart.Server.Services;

namespace Spring.EmsQuickStart.Server.Services.Stubs
{
    public class TradingServiceStub : ITradingService
    {
        public void ProcessTrade(TradeRequest request, TradeResponse response)
        {
            //do nothing implementation, typical implementations would persist state to the database.
        }
    }
}