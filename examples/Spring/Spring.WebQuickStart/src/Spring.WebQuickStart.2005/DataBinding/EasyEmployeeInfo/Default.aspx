<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="DataBinding_EasyEmployeeInfo_Default" %>
<%@ Register TagPrefix="spring" Namespace="Spring.Web.UI.Controls" Assembly="Spring.Web" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Data Binding - Easy Employee Info example</title>
</head>
<body>
    <h2><a href="../../Default.aspx">Welcome to Spring.NET Web Framework Quick Start Guide</a></h2>
    <h2><a href="../Default.aspx">Bi-directional DataBinding</a></h2>
    <form id="form1" runat="server">
    <div>
    <h2>Easy Employee Info example</h2>
    <p>
    Now that you have an idea of how databinding works, here's the easy way using a DataBindingPanel.
    </p>
    <p>
    By placing your controls within a DataBindingPanel, you can use additional attributes 
    to specify binding information on your server controls.
    </p>
    <spring:ValidationSummary ID="ctlErrorList" Provider="errors.summary" runat="server" />

<spring:DataBindingPanel ID="ctlDataBindingPanel" runat="server">
    <table cellpadding="3" cellspacing="3" border="0">
        <tr>
            <td>Employee ID:</td>
            <td>
                <asp:TextBox ID="txtId" runat="server" BindingTarget="Employee.Id" MessageId="ID has to be an integer" ErrorProviders="id.errors,errors.summary" />
                <spring:ValidationError ID="ctlIdErrors" Provider="id.errors" runat="server" />
            </td>
        </tr>
        <tr>
            <td>First Name:</td>
            <td><asp:TextBox ID="txtFirstName" runat="server" BindingTarget="Employee.FirstName" /></td>
        </tr>
        <tr>
            <td>Last Name:</td>
            <td><asp:TextBox ID="txtLastName" runat="server" BindingTarget="Employee.LastName" /></td>
        </tr>
        <tr>
            <td>Date of Birth:</td>
            <td>
                <asp:TextBox ID="txtDOB" runat="server" BindingTarget="Employee.DateOfBirth" MessageId="Invalid date value" ErrorProviders="errors.summary,dob.errors"/>
                <spring:ValidationError ID="ctlDOBErrors" Provider="dob.errors" runat="server" />
            </td>
        </tr>
        <tr>
            <td>Gender:</td>
            <td>
                <!-- 
                  RadioButtonGroup control exposes Value property which is set
                  to the value of the ID property of the selected radio button
                  -->
                <spring:RadioButtonGroup ID="rbgGender" runat="server" BindingTarget="Employee.Gender">
                    <asp:RadioButton ID="Male" Text="Male" runat="server" />
                    <asp:RadioButton ID="Female" Text="Female" runat="server" />
                </spring:RadioButtonGroup>
            </td>
        </tr>
        <tr>
            <td>Salary:</td>
            <td>
                <asp:TextBox ID="txtSalary" runat="server" BindingTarget="Employee.Salary" BindingFormatter="theCurrencyFormatter" MessageId="Salary must be a valid currency value." ErrorProviders="errors.summary,salary.errors" />
                <spring:ValidationError ID="ctlSalaryErrors" Provider="salary.errors" runat="server" />
            </td>
        </tr>
        <tr>
            <td>Address Type:</td>
            <td>
                <!-- 
                  Another way to select one of the predefined values, which is
                  how enum values should always be entered
                  -->
                <asp:DropDownList ID="ddlAddressType" runat="server" BindingTarget="Employee.MailingAddress.AddressType">
                    <asp:ListItem Value="Home" Text="Home" Selected="True" />
                    <asp:ListItem Value="Office" Text="Office" />
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td>Street 1:</td>
            <td><asp:TextBox ID="txtStreet1" runat="server" BindingTarget="Employee.MailingAddress.Street1" /></td>
        </tr>
        <tr>
            <td>Street 2:</td>
            <td><asp:TextBox ID="txtStreet2" runat="server" BindingTarget="Employee.MailingAddress.Street2" /></td>
        </tr>
        <tr>
            <td>City:</td>
            <td><asp:TextBox ID="txtCity" runat="server" BindingTarget="Employee.MailingAddress.City" /></td>
        </tr>
        <tr>
            <td>State:</td>
            <td><asp:TextBox ID="txtState" runat="server" BindingTarget="Employee.MailingAddress.State" /></td>
        </tr>
        <tr>
            <td>Postal Code/ZIP:</td>
            <td><asp:TextBox ID="txtPostalCode" runat="server" BindingTarget="Employee.MailingAddress.PostalCode" /></td>
        </tr>
        <tr>
            <td>Country:</td>
            <td><asp:TextBox ID="txtCountry" runat="server" BindingTarget="Employee.MailingAddress.Country" /></td>
        </tr>
        <tr>
            <td>Hobbies:</td>
            <td><asp:ListBox id="lstHobbies" runat="server" SelectionMode="Multiple" BindingTarget="Employee.Hobbies" /></td>
        </tr>
        <tr>
            <td>Favorite Food:</td>
            <td>   
                <spring:CheckBoxList id="lstFavoriteFood" runat="server" BindingTarget="Employee.FavoriteFood">
                    <asp:ListItem value="0">Cheese</asp:ListItem>
                    <asp:ListItem value="1">Noodles</asp:ListItem>
                    <asp:ListItem value="2">Fruits</asp:ListItem>
                    <asp:ListItem value="3">Vegetables</asp:ListItem>
                </spring:CheckBoxList>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td><asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" /></td>
        </tr>
    </table>
</spring:DataBindingPanel>
    </div>
    </form>
</body>
</html>
