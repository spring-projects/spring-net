using System;
using System.Collections;
using System.Web.UI.WebControls;
using Spring.Northwind.Dao;
using Spring.Northwind.Domain;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI;

public partial class CustomerList : ConversationPage
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

  /// <summary>
  /// List not attached. This page will make searching the database only once per conversation.
  /// </summary>
  private IList<Customer> CustomersLoadedOncePerConvList
  {
      get
      {
          return (IList<Customer>)this.Conversation["CustomersLoadedOncePerConvList"];
      }
      set
      {
          this.Conversation["CustomersLoadedOncePerConvList"] = value;
      }
  }

  private void Page_InitializeControls(object sender, EventArgs e)
  {
    //searching the database only once per conversation.
    if (this.CustomersLoadedOncePerConvList == null)
    { 
    // create/initialize controls here
      this.CustomersLoadedOncePerConvList = customerDao.GetAll();
    }

    customerList.DataSource = this.CustomersLoadedOncePerConvList;
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

  protected void BtnShowOrders_Click(object sender, EventArgs e)
  {
    Button BtnShowOrders = (Button)sender;
    DataGridItem dgi = (DataGridItem)BtnShowOrders.NamingContainer;
    DataGrid DtgCustomerOrderList = (DataGrid)dgi.FindControl("DtgCustomerOrderList");
    HtmlControl DivCustomerOrderList = (HtmlControl)dgi.FindControl("DivCustomerOrderList");
    if (BtnShowOrders.Text == "+")
    {
      //discovery index for Customer
      int customerIndex;
      customerIndex = (this.customerList.PageSize * this.customerList.CurrentPageIndex) + dgi.ItemIndex;
      Customer customerFromLine = this.CustomersLoadedOncePerConvList[customerIndex];
      DtgCustomerOrderList.DataSource = customerFromLine.Orders;
      DtgCustomerOrderList.DataBind();
      DtgCustomerOrderList.Visible = true;
      DivCustomerOrderList.Visible = true;
      //background-color:White; position:absolute; left:-180px. Positioning, just visual.
      DivCustomerOrderList.Style[HtmlTextWriterStyle.BackgroundColor] = "White";
      DivCustomerOrderList.Style[HtmlTextWriterStyle.Position] = "absolute";
      DivCustomerOrderList.Style[HtmlTextWriterStyle.Left] = "-380px";

      BtnShowOrders.Text = "-";
    }
    else
    {
      DtgCustomerOrderList.Visible = false;
      DivCustomerOrderList.Visible = false;
      BtnShowOrders.Text = "+";
    }
  }
}
