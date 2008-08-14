

using System.Collections;
using Spring.NmsQuickStart.Common.Data;
using Spring.NmsQuickStart.Server.Services;

namespace Spring.NmsQuickStart.Server.Services.Stubs
{
    public class CreditCheckServiceStub : ICreditCheckService
    {
        public bool CanExecute(TradeRequest tradeRequest, IList errors)
        {
            return true;
        }
    }
}