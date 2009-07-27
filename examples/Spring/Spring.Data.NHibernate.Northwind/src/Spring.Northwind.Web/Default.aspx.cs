using System;
using System.Diagnostics;
using Spring.Northwind.Dao;
using Spring.Northwind.Domain;
using Spring.Northwind.Service;

public partial class _Default : Spring.Web.UI.Page
{
    private ICustomerDao customerDao;
    private IFulfillmentService fulfillmentService;

    public IFulfillmentService FulfillmentService
    {
        set { this.fulfillmentService = value; }
    }

    public ICustomerDao CustomerDao
    {
        set { this.customerDao = value; }
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        ProcessCustomer();
    }

    public void ProcessCustomer()
    {
        string customerId = "ERNSH";

        // check, if exists
        Customer customer = customerDao.Get(customerId);

        //Find all orders for customer and ship them
        this.fulfillmentService.ProcessCustomer(customer.Id);
        //assertions....

        // prepare for display - note that OrderDetails are loaded lazy here!
        foreach (Order order in customer.Orders)
        {
            Debug.Assert(order.OrderDetails.Count > 0);
            foreach(OrderDetail details in order.OrderDetails)
            {
                details.Quantity = details.Quantity - 1;
                // do something here
            }
        }
    }
    protected void customerList_Click(object sender, EventArgs e)
    {
        SetResult("CustomerList");
    }
}