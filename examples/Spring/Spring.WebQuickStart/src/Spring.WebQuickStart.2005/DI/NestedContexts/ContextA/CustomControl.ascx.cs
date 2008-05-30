using System;

public partial class DI_NestedContexts_ContextA_CustomControl : System.Web.UI.UserControl
{
  private string _message;

  public string Message
  {
    get { return this._message; }
    set { this._message = value; }
  }
}
