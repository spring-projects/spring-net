

using System.Collections;
using Spring.EmsQuickStart.Common.Data;
using Spring.EmsQuickStart.Server.Services;

namespace Spring.EmsQuickStart.Server.Services.Stubs
{
    public class CreditCheckServiceStub : ICreditCheckService
    {
        public bool CanExecute(TradeRequest tradeRequest, IList errors)
        {
            return true;
        }
    }
}