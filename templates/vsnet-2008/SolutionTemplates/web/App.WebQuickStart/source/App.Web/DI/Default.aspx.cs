using System;
using System.Web.UI;

/// <summary>
/// Notice that you don't need to extend Spring.Web.UI.Page for DI features!
/// </summary>
public partial class DI_HelloWorld_Default : System.Web.UI.Page
{
  private string message;

  public string Message
  {
    get { return this.message; }
    set { this.message = value; }
  }

  /// <summary>
  /// Values are injected right before OnInit() is called.
  /// </summary>
  /// <param name="e"></param>
  protected override void OnInit(EventArgs e)
  {
    if(this.Message == null)
    {
      throw new ArgumentNullException("Message");
    }

    base.OnInit(e);
  }
}
