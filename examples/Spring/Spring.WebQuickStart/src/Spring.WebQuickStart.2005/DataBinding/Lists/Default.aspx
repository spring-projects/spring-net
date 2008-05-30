<%@ Page Language="C#" EnableViewState="true" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="DataBinding_Lists_Default" %>
<%@ Register TagPrefix="spring" Namespace="Spring.Web.UI.Controls" Assembly="Spring.Web" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Data Binding - Multi-Selection Lists Example</title>
</head>
<body>
    <h2><a href="../../Default.aspx">Welcome to Spring.NET Web Framework Quick Start Guide</a></h2>
    <h2><a href="../Default.aspx">Bi-directional DataBinding</a></h2>
    <form id="form1" runat="server">
    <div>
    <h2>Multi-Selection Lists example</h2>
    <p>
    Life is not 1-dimensional ;-). This sample demonstrates binding of multiple selection lists.
    </p>
    <p>
    This form allows you to enter employee information and it updates properties
    of the Employee property based on the entered values. You should put a breakpoint
    on the Debug.Write(Employee) statement in the event handler for the Save button, 
    so you can see how Employee object was populated by the framework.
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
            <td>Hobbies:</td>
            <td><asp:ListBox id="lstHobbies" runat="server" SelectionMode="Multiple" EnableViewState="false" /></td>
        </tr>
        <tr>
            <td>Favorite Food:</td>
            <td>   
                <asp:CheckBoxList id="lstFavoriteFood" runat="server" EnableViewState="false">
                    <asp:ListItem value="0">Cheese</asp:ListItem>
                    <asp:ListItem value="1">Noodles</asp:ListItem>
                    <asp:ListItem value="2">Fruits</asp:ListItem>
                    <asp:ListItem value="3">Vegetables</asp:ListItem>
                </asp:CheckBoxList>
            </td>
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
