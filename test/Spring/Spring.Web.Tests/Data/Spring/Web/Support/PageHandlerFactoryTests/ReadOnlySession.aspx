<%@ Page language="c#" EnableSessionState="ReadOnly" AutoEventWireup="false" Inherits="Spring.Web.UI.Page" %><script language="c#" runat="server">
protected override void OnLoad(EventArgs e)
{
	base.OnLoad(e);
	
	try
	{
	    Session["disablesSession"] = "somevalue";
	    NUnit.Framework.Assert.Fail("must not be able to write to session");
	}
	catch(HttpException)
	{}
}
</script><%=Session.IsReadOnly?"OK":"NOK"%>