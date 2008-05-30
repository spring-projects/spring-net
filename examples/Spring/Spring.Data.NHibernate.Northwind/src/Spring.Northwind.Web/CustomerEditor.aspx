<%@ Page Language="C#" CodeFile="CustomerEditor.aspx.cs" Inherits="CustomerEditor" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
<spring:DataBindingPanel runat="server">
    <table>
      <tr><td>ID:</td><td><asp:TextBox runat="server" BindingTarget="CurrentCustomer.Id" BindingDirection="TargetToSource" readonly="true" /></td></tr>
      <tr><td>Contact:</td><td><asp:TextBox runat="server" BindingTarget="CurrentCustomer.ContactName" /></td></tr>
    </table>
</spring:DataBindingPanel>
    </div>
    <asp:Button runat="server" id="btnSave" Text="Save" />
    </form>
</body>
</html>
