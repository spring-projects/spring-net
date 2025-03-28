<%@ Page language="c#" AutoEventWireup="false" Inherits="Spring.Web.UI.Page" ClassName="TransferAfterSetResult" %>
<script language="c#" runat="server">
    private void Save_Clicked(object sender, EventArgs e)
    {
        SetResult("save");
    }
</script>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
  <HEAD>
    <title>GuestBook</title>
  </HEAD>
  <body>
    <form id="GuestBook" method="post" runat="server">
      <p><b>Guest Book:</b></p>
      
      <u>Enter your name:</u>
      <table>
      <tr><td>Name:</td><td><asp:textbox id="name" Runat="server"></asp:textbox></td></tr>
      </table>
      <asp:Button ID="save" Runat="server" Text="Save" OnClick="Save_Clicked"></asp:Button>
    </form>
  </body>
</HTML>
