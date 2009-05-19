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
        // the line below would also work in full trusted environments due to control members 
        // being generated as protected members by ASP.NET:
        //        BindingManager.AddBinding("txtId.Text", "Employee.Id")

        BindingManager.AddBinding("FindControl('txtId').Text", "Employee.Id")
            .SetErrorMessage("ID has to be an integer", "id.errors"); // send msg to "id.errors" provider
        BindingManager.AddBinding("FindControl('txtFirstName').Text", "Employee.FirstName");
        BindingManager.AddBinding("FindControl('txtLastName').Text", "Employee.LastName");
        BindingManager.AddBinding("FindControl('txtDOB').Text", "Employee.DateOfBirth")
            .SetErrorMessage("Invalid date value", "dob.errors");
        BindingManager.AddBinding("FindControl('txtSalary').Text", "Employee.Salary", new CurrencyFormatter())
            .SetErrorMessage("Salary must be a valid currency value.", "salary.errors");
        BindingManager.AddBinding("FindControl('rbgGender').Value", "Employee.Gender");
        BindingManager.AddBinding("FindControl('ddlAddressType').SelectedValue", "Employee.MailingAddress.AddressType");
        BindingManager.AddBinding("FindControl('txtStreet1').Text", "Employee.MailingAddress.Street1");
        BindingManager.AddBinding("FindControl('txtStreet2').Text", "Employee.MailingAddress.Street2");
        BindingManager.AddBinding("FindControl('txtCity').Text", "Employee.MailingAddress.City");
        BindingManager.AddBinding("FindControl('txtState').Text", "Employee.MailingAddress.State");
        BindingManager.AddBinding("FindControl('txtPostalCode').Text", "Employee.MailingAddress.PostalCode");
        BindingManager.AddBinding("FindControl('txtCountry').Text", "Employee.MailingAddress.Country");
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

