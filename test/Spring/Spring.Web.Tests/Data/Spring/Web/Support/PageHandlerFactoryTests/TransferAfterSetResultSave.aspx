<%@ Page language="c#" AutoEventWireup="false" EnableViewStateMac="false" Inherits="Spring.Web.UI.Page" %>
<%@ Reference Page="TransferAfterSetResult.aspx" %>
<script language="c#" runat="server">
    public string SomeProperty;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        NUnit.Framework.Assert.That(this.PreviousPage.GetType(), NUnit.Framework.Is.EqualTo(typeof(TransferAfterSetResult)));
        NUnit.Framework.Assert.That(SomeProperty, NUnit.Framework.Is.EqualTo("someValue"));
    }
</script>OK