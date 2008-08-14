

using System.Collections;
using Spring.MsmqQuickStart.Common.Data;


namespace Spring.MsmqQuickStart.Server.Services.Stubs
{
    public class CreditCheckServiceStub : ICreditCheckService
    {
        public bool CanExecute(TradeRequest tradeRequest, IList errors)
        {
            return true;
        }
    }
}