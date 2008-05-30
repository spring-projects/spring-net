using System;
using System.Diagnostics;
using System.Web.UI.WebControls;

/// <summary>
/// Notice that your web page has to extend Spring.Web.UI.Page class
/// in order to enable data binding and many other features.
/// </summary>
public partial class DataBinding_EasyEmployeeInfo_Default : Spring.Web.UI.Page
{
    private EmployeeInfo employee = new EmployeeInfo();

    public EmployeeInfo Employee
    {
        get { return employee; }
    }

    protected override void OnInitializeControls(EventArgs e)
    {
        base.OnInitializeControls(e);

        InitializeHobbiesList();
    }

    private void InitializeHobbiesList()
    {
        // fill lstHobbies with 'Hobby' items
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

    protected void btnSave_Click(object sender, EventArgs e)
    {
        // do something with employee such as:
        // employeeDao.Save(Employee);

        Debug.Write(Employee);
    }
}

