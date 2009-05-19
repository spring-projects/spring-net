using System;
using System.Diagnostics;
using Spring.Globalization.Formatters;

public partial class EmployeeInfoEditor : Spring.Web.UI.UserControl
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
        BindingManager.AddBinding("FindControl('txtId').Text", "Employee.Id")
            .SetErrorMessage("ID has to be an integer", "id.errors", "summary");
        BindingManager.AddBinding("FindControl('txtFirstName').Text", "Employee.FirstName");
        BindingManager.AddBinding("FindControl('txtLastName').Text", "Employee.LastName");
        BindingManager.AddBinding("FindControl('txtDOB').Text", "Employee.DateOfBirth")
            .SetErrorMessage("Invalid date value", "dob.errors", "summary");
        BindingManager.AddBinding("FindControl('txtSalary').Text", "Employee.Salary", new CurrencyFormatter())
            .SetErrorMessage("Salary must be a valid currency value.", "salary.errors", "summary");
        BindingManager.AddBinding("FindControl('rbgGender').Value", "Employee.Gender");
        BindingManager.AddBinding("FindControl('ddlAddressType').SelectedValue", "Employee.MailingAddress.AddressType");
        BindingManager.AddBinding("FindControl('txtStreet1').Text", "Employee.MailingAddress.Street1");
        BindingManager.AddBinding("FindControl('txtStreet2').Text", "Employee.MailingAddress.Street2");
        BindingManager.AddBinding("FindControl('txtCity').Text", "Employee.MailingAddress.City");
        BindingManager.AddBinding("FindControl('txtState').Text", "Employee.MailingAddress.State");
        BindingManager.AddBinding("FindControl('txtPostalCode').Text", "Employee.MailingAddress.PostalCode");
        BindingManager.AddBinding("FindControl('txtCountry').Text", "Employee.MailingAddress.Country");
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        // do something with employee such as:
        // employeeDao.Save(Employee);

        Debug.Write(Employee);
    }
}
