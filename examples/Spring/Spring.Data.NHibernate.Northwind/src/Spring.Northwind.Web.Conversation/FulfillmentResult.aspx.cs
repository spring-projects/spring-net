using System;
using Spring.Northwind.Service;

public partial class FullfillmentResult : ConversationPage
{
    private IFulfillmentService fulfillmentService;
    private ICustomerEditController customerEditController;

    public IFulfillmentService FulfillmentService
    {
        set { fulfillmentService = value; }
    }

    public ICustomerEditController CustomerEditController
    {
        set { customerEditController = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
    }

    protected void customerOrders_Click(object sender, EventArgs e)
    {
        SetResult("Back");
    }
}
