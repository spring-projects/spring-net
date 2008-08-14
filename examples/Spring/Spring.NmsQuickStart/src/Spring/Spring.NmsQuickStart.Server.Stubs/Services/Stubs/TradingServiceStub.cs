

using Spring.NmsQuickStart.Common.Data;
using Spring.NmsQuickStart.Server.Services;

namespace Spring.NmsQuickStart.Server.Stubs.Services.Stubs
{
    public class TradingServiceStub : ITradingService
    {
        public void ProcessTrade(TradeRequest request, TradeResponse response)
        {
            //do nothing implementation, typical implementations would persist state to the database.
        }
    }
}