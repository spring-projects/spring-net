

using System.Collections;

namespace Spring.NmsQuickStart.Common.Bo
{
    public class TradeRequest
    {
        private string ticker;

        private long quantity;

        private double price;

        private string orderType;

        private string accountName;

        private bool buyRequest;

        private string userName;

        private string requestId;


        public string Ticker
        {
            get { return ticker; }
            set { ticker = value; }
        }

        public long Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        public double Price
        {
            get { return price; }
            set { price = value; }
        }

        public string OrderType
        {
            get { return orderType; }
            set { orderType = value; }
        }

        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }

        public bool BuyRequest
        {
            get { return buyRequest; }
            set { buyRequest = value; }
        }

        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        public string RequestId
        {
            get { return requestId; }
            set { requestId = value; }
        }

        public bool Validate(IList validationErrors)
        {
            // Not intended to be an example best practices for validation
            // The intention is to include some simple behavior in the class

            if (userName == null)
            {
                validationErrors.Add("User name not specified");
            }
            if (requestId == null || requestId.Length == 0)
            {
                validationErrors.Add("Request Id not specified");
            }
            if (!orderType.Equals("MARKET"))
            {
                if (price <= 0)
                {
                    validationErrors.Add("Market order must have a price");
                }
            }
            return validationErrors.Count > 0;
        }
    }
}