using System;
using System.Diagnostics;
using Spring.Globalization.Formatters;

/// <summary>
/// Notice that your web page has to extend Spring.Web.UI.Page class
/// in order to enable data binding and many other features.
/// </summary>
public partial class DataBinding_RobustEmployeeInfo_Default : Spring.Web.UI.Page
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
        BindingManager.AddBinding("txtId.Text", "Employee.Id")
            .SetErrorMessage("ID has to be an integer", "id.errors");
        BindingManager.AddBinding("txtFirstName.Text", "Employee.FirstName");
        BindingManager.AddBinding("txtLastName.Text", "Employee.LastName");
        BindingManager.AddBinding("txtDOB.Text", "Employee.DateOfBirth")
            .SetErrorMessage("Invalid date value", "dob.errors");
        BindingManager.AddBinding("txtSalary.Text", "Employee.Salary", new CurrencyFormatter())
            .SetErrorMessage("Salary must be a valid currency value.", "salary.errors");
        BindingManager.AddBinding("rbgGender.Value", "Employee.Gender");
        BindingManager.AddBinding("ddlAddressType.SelectedValue", "Employee.MailingAddress.AddressType");
        BindingManager.AddBinding("txtStreet1.Text", "Employee.MailingAddress.Street1");
        BindingManager.AddBinding("txtStreet2.Text", "Employee.MailingAddress.Street2");
        BindingManager.AddBinding("txtCity.Text", "Employee.MailingAddress.City");
        BindingManager.AddBinding("txtState.Text", "Employee.MailingAddress.State");
        BindingManager.AddBinding("txtPostalCode.Text", "Employee.MailingAddress.PostalCode");
        BindingManager.AddBinding("txtCountry.Text", "Employee.MailingAddress.Country");
    }
    
    protected void Page_Load(object sender, EventArgs e)
    {}


    protected void btnSave_Click(object sender, EventArgs e)
    {
        // do something with employee such as:
        // employeeDao.Save(Employee);
        
        Debug.Write(Employee);
    }
}

