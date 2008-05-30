using System;
using System.Collections;
using Spring.DataBinding;
using Spring.Globalization.Formatters;

/// <summary>
/// Notice that your web page has to extend Spring.Web.UI.Page class
/// in order to enable data binding and many other features.
/// </summary>
public partial class DataBinding_Collections_Default : Spring.Web.UI.Page
{
    #region Fields

    private IList products;
    private readonly IntegerFormatter quantityFormatter = new IntegerFormatter();
    private readonly CurrencyFormatter priceFormatter = new CurrencyFormatter();

    #endregion

    #region Properties

    public IList Products
    {
        get { return products; }
    }

    public IntegerFormatter QuantityFormatter
    {
        get { return quantityFormatter; }
    }

    public CurrencyFormatter PriceFormatter
    {
        get { return priceFormatter; }
    }

    #endregion

    /// <summary>
    /// In order to declare data bindings, all you need to do is
    /// override InitializeDataBindings method and add all necessary
    /// data bindings to the BindingManager.
    /// </summary>
    protected override void InitializeDataBindings()
    {
        // HttpRequestListBindingContainer unbinds specified values from Request -> Productlist
        HttpRequestListBindingContainer requestBindings =
            new HttpRequestListBindingContainer("sku,name,quantity,price", "Products", typeof(ProductInfo));
        requestBindings.AddBinding("sku", "Sku");
        requestBindings.AddBinding("name", "Name");
        requestBindings.AddBinding("quantity", "Quantity", quantityFormatter);
        requestBindings.AddBinding("price", "Price", priceFormatter);

        BindingManager.AddBinding(requestBindings);
    }

    #region Model Management

    protected override void InitializeModel()
    {
        products = new ArrayList();
    }

    protected override void LoadModel(object savedModel)
    {
        products = (IList) savedModel;
    }

    protected override object SaveModel()
    {
        return products;
    }

    #endregion
    
    protected void btnAdd_Click(object sender, EventArgs e)
    {
        products.Add(new ProductInfo());
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        // do something with products...
    }
}
