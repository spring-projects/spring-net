using System;
using System.Web;
using Spring.Northwind.Dao;
using Spring.Northwind.Domain;
using Spring.Web.UI;

public partial class CustomerView : Page
{
    private ICustomerEditController customerEditController;
    private ICustomerDao customerDao;

    public ICustomerDao CustomerDao
    {
        set { this.customerDao = value; }
    }

    public ICustomerEditController CustomerEditController
    {
        set { this.customerEditController = value; }
    }

    public Customer CurrentCustomer
    {
        get { return customerEditController.CurrentCustomer; }
    }

    //  public static void Edit( Customer customer )
    //  {
    //    HttpContext.Current.Session[typeof(CustomerEditor).FullName + ".Customer"] = customer;
    //  }

    public CustomerView()
    {
        this.InitializeControls += new EventHandler(Page_InitializeControls);
        this.DataBound += new EventHandler(Page_DataBound);
        this.DataUnbound += new EventHandler(Page_DataUnbound);
    }

    override protected void InitializeDataBindings()
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
        
    }

    protected void customerList_Click(object sender, EventArgs e)
    {
        SetResult("CustomerList");
    }
    protected void editCustomer_Click(object sender, EventArgs e)
    {
        SetResult("EditCustomer");
    }
}
