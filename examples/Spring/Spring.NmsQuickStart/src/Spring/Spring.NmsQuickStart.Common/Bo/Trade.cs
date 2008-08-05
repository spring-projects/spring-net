

namespace Spring.NmsQuickStart.Common.Bo
{
    public class Trade
    {
        private string ticker;

        private long quantity;

        private double price;

        private string orderType;


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
    }
}