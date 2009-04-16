<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmployeeInfoEditor.ascx.cs" Inherits="EmployeeInfoEditor" %>
<%@ Register TagPrefix="spring" Namespace="Spring.Web.UI.Controls" Assembly="Spring.Web" %>
    <table cellpadding="3" cellspacing="3" border="0">
        <tr>
            <td>Employee ID:</td>
            <td>
                <asp:TextBox ID="txtId" runat="server" EnableViewState="false" />
                <spring:ValidationError ID="errId" Provider="id.errors" runat="server" />
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
