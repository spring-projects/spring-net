<%@ Page Language="C#" AutoEventWireup="true" CodeFile="UserLogin.aspx.cs" Inherits="Admin_Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>login</title>
</head>
<body>
    <form id="LoginForm" runat="server">
        <div align="center">
            Please enter your login information:<br />
                <br />
                <table width="450px" border="0" cellpadding="0" cellspacing="0">
                    <tr>
                        <td align="left" >Username:</td>
                        <td align="left">
                            <asp:TextBox ID="UsernameText" runat="server" />                            
                        </td>
                    </tr>                    
                    <tr>
                        <td align="left">Password:</td>
                        <td align="left">
                            <asp:TextBox ID="PasswordText" runat="server" TextMode="Password" />                            
                        </td>
                    </tr>
                </table>
                <br />
                <asp:Button ID="LoginAction" runat="server" OnClick="LoginAction_Click" Text="Login" /><br />
                <asp:Label ID="LegendStatus" runat="server" EnableViewState="false" Text="" />
        </div>
    </form>
</body>
</html>