<%@ Page language="c#" AutoEventWireup="false" Inherits="Spring.Web.UI.Page" %><script language="c#" runat="server">
                                                                                   protected override void OnLoad(EventArgs e)
                                                                                   {
                                                                                       base.OnLoad(e);
                                                                                       Session["maintainsSession"] = "somevalue";
                                                                                   }
                                                                               </script>OK