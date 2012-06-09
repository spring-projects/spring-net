using System;
using Spring.Northwind.Dao;
using Spring.Northwind.Domain;
using Spring.Web.UI;

public partial class CustomerEditor : Page
{
    private ICustomerEditController customerEditController;
    private ICustomerDao customerDao;

    public ICustomerDao CustomerDao
    {
        set { customerDao = value; }
    }

    public ICustomerEditController CustomerEditController
    {
        set { customerEditController = value; }
    }

    public Customer CurrentCustomer
    {
        get
        {
            return customerEditController.CurrentCustomer;
        }
    }

    public CustomerEditor()
    {
        InitializeControls += Page_InitializeControls;
        DataBound += Page_DataBound;
        DataUnbound += Page_DataUnbound;
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
        btnSave.Click += BtnSave_Click;
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        customerDao.Update(CurrentCustomer);
        SetResult("ViewCustomer");
    }
    protected void customerList_Click(object sender, EventArgs e)
    {
        SetResult("CustomerList");
    }
    protected void cancel_Click(object sender, EventArgs e)
    {
        SetResult("CancelEdit");
    }
}
