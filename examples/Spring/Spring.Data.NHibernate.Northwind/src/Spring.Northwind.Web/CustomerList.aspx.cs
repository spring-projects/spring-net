using System;
using System.Collections;
using System.Web.UI.WebControls;
using Spring.Northwind.Dao;
using Spring.Northwind.Domain;

public partial class CustomerList : Spring.Web.UI.Page
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

  public Customer SelectedCustomer
  {
    get
    {
      return (Customer) this.customerList.SelectedItem.DataItem;
    }
  }

  public CustomerList()
  {
    this.InitializeControls+=new EventHandler(Page_InitializeControls);
    this.DataBound+=new EventHandler(Page_DataBound);
    this.DataUnbound+=new EventHandler(Page_DataUnbound);
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
    // create/initialize controls here
    customerList.DataSource = customerDao.GetAll();
    customerList.ItemCommand+=new DataGridCommandEventHandler(CustomerList_ItemCommand);
    customerList.PageIndexChanged+=new DataGridPageChangedEventHandler(CustomerList_PageIndexChanged);
    if (!IsPostBack)
    {
      customerList.DataBind();
    }
    else
    {
      customerList.ItemCreated+=new DataGridItemEventHandler(this.CustomerList_ItemCreated);
    }
  }

  private void CustomerList_PageIndexChanged(object source, DataGridPageChangedEventArgs e)
  {
    customerList.CurrentPageIndex = e.NewPageIndex;
    customerList.DataBind();
  }

  private void CustomerList_ItemCommand(object source, DataGridCommandEventArgs e)
  {
    switch(e.CommandName)
    {
      case "ViewOrders":
      case "ViewCustomer":
        customerList.SelectedIndex = e.Item.ItemIndex;
        customerEditController.EditCustomer(this.SelectedCustomer);
        SetResult(e.CommandName);
        break;
    }
  }

  private void CustomerList_ItemCreated(object sender, DataGridItemEventArgs e)
  {
    if(e.Item.DataSetIndex > -1)
    {
      e.Item.DataItem = ((IList) customerList.DataSource)[e.Item.DataSetIndex];
    }
  }
}
