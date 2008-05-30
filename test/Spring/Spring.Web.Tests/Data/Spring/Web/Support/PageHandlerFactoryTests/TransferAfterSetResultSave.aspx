<%@ Page language="c#" AutoEventWireup="false" EnableViewStateMac="false" Inherits="Spring.Web.UI.Page" %>
<%@ Reference Page="TransferAfterSetResult.aspx" %>
<script language="c#" runat="server">
public string SomeProperty;

protected override void OnLoad(EventArgs e)
{
    base.OnLoad(e);
    NUnit.Framework.Assert.AreEqual( typeof(TransferAfterSetResult), this.PreviousPage.GetType() );
    NUnit.Framework.Assert.AreEqual( "someValue", SomeProperty );
}
</script>OK