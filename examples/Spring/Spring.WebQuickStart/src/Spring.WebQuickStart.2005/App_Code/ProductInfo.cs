/// <summary>
/// Summary description for ProductInfo
/// </summary>
public class ProductInfo
{
    private string sku;
    private string name;
    private int quantity;
    private double price;
    
    public ProductInfo()
    {}

    public string Sku
    {
        get { return sku; }
        set { sku = value; }
    }

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    public int Quantity
    {
        get { return quantity; }
        set { quantity = value; }
    }

    public double Price
    {
        get { return price; }
        set { price = value; }
    }
}