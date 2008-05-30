
namespace Spring.DataQuickStart.Domain
{
    /// <summary>
    /// Class encapsulating OrderDetail information suitable for
    /// use with the stored procedure CustOrdersDetail
    /// </summary>
    public class OrderDetails
    {
        private string productName;
        private double unitPrice;
        private int quantity;
        private int discount;
        private double extendedPrice;

        public string ProductName
        {
            get { return productName; }
            set { productName = value; }
        }

        public double UnitPrice
        {
            get { return unitPrice; }
            set { unitPrice = value; }
        }

        public int Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        public int Discount
        {
            get { return discount; }
            set { discount = value; }
        }

        public double ExtendedPrice
        {
            get { return extendedPrice; }
            set { extendedPrice = value; }
        }
    }
}
