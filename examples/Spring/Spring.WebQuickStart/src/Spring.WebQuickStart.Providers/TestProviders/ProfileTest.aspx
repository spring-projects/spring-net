<%@ Page Language="C#" EnableViewState="true" %>
<%@ Import namespace="System.Web.Profile"%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
<title>Profile Provider Test</title>
<SCRIPT runat="server">
private void Page_Load(object sender, System.EventArgs e)
{ 
       
    this.TxtGreeting.Text = Profile.Greeting;
    this.TxtUsername.Text = Profile.UserName;
    
}
</SCRIPT>
</head>
<body>
    <form id="form1" runat="server">
                    
            <asp:Label ID="LblUsername" runat="server" Text="Logged Username:"></asp:Label>
            <asp:TextBox ID="TxtUsername" runat="server"></asp:TextBox>
            <br />
            <asp:Label ID="Lblgreeting" runat="server" Text="User's Greeting:"></asp:Label>
            <asp:TextBox ID="TxtGreeting" runat="server"></asp:TextBox><br />    
    </form>
</body>
</html>