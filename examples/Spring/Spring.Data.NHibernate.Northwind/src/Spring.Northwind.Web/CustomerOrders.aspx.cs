using System;
using System.Collections;
using System.Web.UI.WebControls;
using Spring.Northwind.Domain;

public partial class CustomerOrders : Spring.Web.UI.Page
{
    private ICustomerEditController customerEditController;

    public ICustomerEditController CustomerEditController
    {
        set { customerEditController = value; }
    }

    public Customer SelectedCustomer
    {
        get { return customerEditController.CurrentCustomer; }
    }

    public CustomerOrders()
    {
        InitializeControls += Page_InitializeControls;
        DataBound += Page_DataBound;
        DataUnbound += Page_DataUnbound;
    }

    protected override void InitializeDataBindings()
    {
        base.InitializeDataBindings();

        // do the "one time" setup for databinding
    }

    private void Page_DataBound(object sender, EventArgs e)
    {
        // perform custom tasks for binding data from model to the form
    }

    private void Page_DataUnbound(object sender, EventArgs e)
    {
        // perform custom tasks for unbinding data from form to the model
    }

    private void Page_InitializeControls(object sender, EventArgs e)
    {
        // create/initialize controls here
        customerOrders.DataSource = SelectedCustomer.Orders;
        customerName.Text = SelectedCustomer.CompanyName;
        if (!IsPostBack)
        {
            customerOrders.DataBind();
        }
        else
        {
            customerOrders.ItemCreated += CustomerList_ItemCreated;
        }
    }

    private void CustomerList_ItemCreated(object sender, DataGridItemEventArgs e)
    {
        if (e.Item.DataSetIndex > -1)
        {
            e.Item.DataItem = ((IList) customerOrders.DataSource)[e.Item.DataSetIndex];
        }
    }
    protected void customerList_Click(object sender, EventArgs e)
    {
        SetResult("CustomerList");
    }
    protected void processOrders_Click(object sender, EventArgs e)
    {
        SetResult("ProcessOrders");
    }
}