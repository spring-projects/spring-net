<%@ Page language="c#" EnableSessionState="false" AutoEventWireup="false" Inherits="Spring.Web.UI.Page" %><script language="c#" runat="server">
                                                                                                              protected override void OnLoad(EventArgs e)
                                                                                                              {
                                                                                                                  base.OnLoad(e);
                                                                                                                  try
                                                                                                                  {
                                                                                                                      Session["disablesSession"] = "somevalue";
                                                                                                                      NUnit.Framework.Assert.Fail("must not be able to access session");
                                                                                                                  }
                                                                                                                  catch (HttpException)
                                                                                                                  {
                                                                                                                  }
                                                                                                              }
                                                                                                          </script>OK