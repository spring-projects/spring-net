using System;
using System.Diagnostics;
using System.Web.UI.WebControls;
using Spring.DataBinding;
using Spring.Globalization;
using Spring.Globalization.Formatters;
using Spring.Web.UI;

/// <summary>
/// Notice that your web page has to extend Spring.Web.UI.Page class
/// in order to enable data binding and many other features.
/// </summary>
public partial class DataBinding_Lists_Default : Page
{
    private EmployeeInfo employee = new EmployeeInfo();

    public EmployeeInfo Employee
    {
        get { return employee; }
    }

    /// <summary>
    /// In order to declare data bindings, all you need to do is
    /// override InitializeDataBindings method and add all necessary
    /// data bindings to the BindingManager.
    /// </summary>
    protected override void InitializeDataBindings()
    {
        BindingManager.AddBinding("FindControl('txtId').Text", "Employee.Id");
        BindingManager.AddBinding("FindControl('txtFirstName').Text", "Employee.FirstName");

        // this is rather verbose to show how it works

        // the formatter must convert between ListControl values and domain objects identified by these values (e.g. a key)
        IFormatter dsFormatter = new DataSourceItemFormatter("DataSource", "DataValueField");
        // bind the lstHobbies control to Employee.Hobbies IList
        MultipleSelectionListControlBinding listBinding = new MultipleSelectionListControlBinding("FindControl('lstHobbies')", "Employee.Hobbies", BindingDirection.Bidirectional, dsFormatter);
        BindingManager.AddBinding(listBinding);

        // use simple name=value binding
        BindingManager.AddBinding(new MultipleSelectionListControlBinding("FindControl('lstFavoriteFood')", "Employee.FavoriteFood", BindingDirection.Bidirectional, new NullFormatter()));
    }

    protected override void OnInitializeControls(EventArgs e)
    {
        base.OnInitializeControls(e);

        InitializeHobbiesList();
    }

    private void InitializeHobbiesList()
    {
        Hobby[] hobbies = {
			                  	new Hobby("1", "Tennis"),
			                  	new Hobby("2", "Climbing"),
			                  	new Hobby("3", "Sailing"),
			                  	new Hobby("4", "Reading")
			                  };

        this.lstHobbies.SelectionMode = ListSelectionMode.Multiple;

        // assign the list in any case - this is required for DataSourceItemFormatter to be able
        // to access Hobby objects
        this.lstHobbies.DataSource = hobbies;
        this.lstHobbies.DataTextField = "Title";
        this.lstHobbies.DataValueField = "UniqueKey";
        if (!IsPostBack)
        {
            this.lstHobbies.DataBind();
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
    }


    protected void btnSave_Click(object sender, EventArgs e)
    {
        // do something with employee such as:
        // employeeDao.Save(Employee);

        Debug.Write(Employee);
    }
}
