<%@ Page Language="C#" EnableViewState="false" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="DataBinding_RobustEmployeeInfo_Default" %>
<%@ Register TagPrefix="spring" Namespace="Spring.Web.UI.Controls" Assembly="Spring.Web" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Data Binding - Robust Employee Info example</title>
</head>
<body>
    <h2><a href="../../Default.aspx">Welcome to Spring.NET Web Framework Quick Start Guide</a></h2>
    <h2><a href="../Default.aspx">Bi-directional DataBinding</a></h2>
    <form id="form1" runat="server">
    <div>
    <h2>Robust Employee Info example</h2>
    <p>
    Now that we have basic data binding rules configured, let's make it more robust.
    </p>
    <p>
    In general, you should specify data-type conversion error handlers for every
    data binding that converts values from one type to another. Optionally, you can
    also specify a formatter if you need to format and/or parse the value during data
    binding process, which is exactly what we did for the Salary field. Now you can 
    enter its value using currency symbol and thousands separator, but entering plain
    number will still work (and it will be properly formatted after the postback).
    </p>
    <p>
    Control definitions below are almost the same as in the previous example. The only 
    difference is that we added controls that will be used to display validation errors.
    </p>
    <p>
    Just like in the previous example, you should put a breakpoint
    on the Debug.Write(Employee) statement in the event handler for the Save button, 
    so you can see how Employee object was populated by the framework.
    </p>
    <!-- this summary control captures validation errors on this page -->
    <spring:ValidationSummary ID="summary" Provider="summary" runat="server" />    
    <table cellpadding="3" cellspacing="3" border="0">
        <tr>
            <td>Employee ID:</td>
            <td>
                <asp:TextBox ID="txtId" runat="server" EnableViewState="false" />
                <spring:ValidationError ID="errId" Provider="id.errors" runat="server" /><!-- read msg from "id.error" provider -->
            </td>
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
            <td>
                <asp:TextBox ID="txtDOB" runat="server" EnableViewState="false" />
                <spring:ValidationError ID="errDOB" Provider="dob.errors" runat="server" />
            </td>
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
            <td>
                <asp:TextBox ID="txtSalary" runat="server" EnableViewState="false" />
                <spring:ValidationError ID="errSalary" Provider="salary.errors" runat="server" />
            </td>
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
