using System;
using System.Diagnostics;

/// <summary>
/// Notice that your web page has to extend Spring.Web.UI.Page class
/// in order to enable data binding and many other features.
/// </summary>
public partial class Navigation_InvalidInput : Spring.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = (string) this.Context.Items["input"];
    }


    protected void btnBack_Click(object sender, EventArgs e)
    {
        SetResult("start");
    }
}

