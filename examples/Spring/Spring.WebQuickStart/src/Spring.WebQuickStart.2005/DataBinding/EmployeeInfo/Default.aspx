<%@ Page Language="C#" EnableViewState="false" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="DataBinding_EmployeeInfo_Default" %>
<%@ Register TagPrefix="spring" Namespace="Spring.Web.UI.Controls" Assembly="Spring.Web" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Data Binding - Employee Info example</title>
</head>
<body>
    <h2><a href="../../Default.aspx">Welcome to Spring.NET Web Framework Quick Start Guide</a></h2>
    <h2><a href="../Default.aspx">Bi-directional DataBinding</a></h2>
    <form id="form1" runat="server">
    <div>
    <h2>Employee Info example</h2>
    <p>
    Now we are getting into more interesting stuff :-).
    </p>
    <p>
    This form allows you to enter employee information and it updates properties
    of the Employee property based on the entered values. You should put a breakpoint
    on the Debug.Write(Employee) statement in the event handler for the Save button, 
    so you can see how Employee object was populated by the framework.
    </p>
    <p>
    You should note that there a number of type conversions are performed quietly
    behind the scenes, such as string to int, double, DateTime, and even enum values.
    Please also keep in mind that this form is very fragile and it will throw
    an exception if you enter a value that cannot be converted using standard type
    conversion mechanism, so make sure that you enter valid values for the ID, Date of Birth,
    and Salary fields. In the next example we will show you how to make it more robust
    using formatters and built-in data type validation.
    </p>
    <table cellpadding="3" cellspacing="3" border="0">
        <tr>
            <td>Employee ID:</td>
            <td><asp:TextBox ID="txtId" runat="server" EnableViewState="false" /></td>
        </tr>
        <tr>
            <td>First Name:</td>
            <td><asp:TextBox ID="txtFirstName" runat="server" EnableViewState="false" /></td>
        </tr>
        <tr>
            <td>Last Name:</td>
            <td><asp:TextBox ID="txtLastName" runat="server" EnableViewState="false" /></td>
        </tr>
        <tr>
            <td>Date of Birth:</td>
            <td><asp:TextBox ID="txtDOB" runat="server" EnableViewState="false" /></td>
        </tr>
        <tr>
            <td>Gender:</td>
            <td>
                <!-- 
                  RadioButtonGroup control exposes Value property which is set
                  to the value of the ID property of the selected radio button
                  -->
                <spring:RadioButtonGroup ID="rbgGender" runat="server" EnableViewState="false">
                    <asp:RadioButton ID="Male" Text="Male" runat="server" EnableViewState="false" />
                    <asp:RadioButton ID="Female" Text="Female" runat="server" EnableViewState="false" />
                </spring:RadioButtonGroup>
            </td>
        </tr>
        <tr>
            <td>Salary:</td>
            <td><asp:TextBox ID="txtSalary" runat="server" EnableViewState="false" /></td>
        </tr>
        <tr>
            <td>Address Type:</td>
            <td>
                <!-- 
                  Another way to select one of the predefined values, which is
                  how enum values should always be entered
                  -->
                <asp:DropDownList ID="ddlAddressType" runat="server" EnableViewState="false">
                    <asp:ListItem Value="Home" Text="Home" Selected="True" />
                    <asp:ListItem Value="Office" Text="Office" />
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td>Street 1:</td>
            <td><asp:TextBox ID="txtStreet1" runat="server" EnableViewState="false" /></td>
        </tr>
        <tr>
            <td>Street 2:</td>
            <td><asp:TextBox ID="txtStreet2" runat="server" EnableViewState="false" /></td>
        </tr>
        <tr>
            <td>City:</td>
            <td><asp:TextBox ID="txtCity" runat="server" EnableViewState="false" /></td>
        </tr>
        <tr>
            <td>State:</td>
            <td><asp:TextBox ID="txtState" runat="server" EnableViewState="false" /></td>
        </tr>
        <tr>
            <td>Postal Code/ZIP:</td>
            <td><asp:TextBox ID="txtPostalCode" runat="server" EnableViewState="false" /></td>
        </tr>
        <tr>
            <td>Country:</td>
            <td><asp:TextBox ID="txtCountry" runat="server" EnableViewState="false" /></td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td><asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" /></td>
        </tr>
    </table>
    </div>
    </form>
</body>
</html>
