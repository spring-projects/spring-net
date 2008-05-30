using System;

/// <summary>
/// Notice that you don't need to extend Spring.Web.UI.UserControl for DI features!
/// </summary>
public partial class DI_HelloWorld_CustomControl : System.Web.UI.UserControl
{
  private string anotherMessage;

  public string AnotherMessage
  {
    get { return this.anotherMessage; }
    set { this.anotherMessage = value; }
  }

  /// <summary>
  /// Values are injected right before OnInit() is called.
  /// </summary>
  /// <param name="e"></param>
  protected override void OnInit(EventArgs e)
  {
    if (this.AnotherMessage == null)
    {
      throw new ArgumentNullException("AnotherMessage");
    }

    base.OnInit(e);
  }
}
