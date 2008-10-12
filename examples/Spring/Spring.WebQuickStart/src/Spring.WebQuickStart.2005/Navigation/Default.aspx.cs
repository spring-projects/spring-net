using System;

/// <summary>
/// Notice that your web page has to extend Spring.Web.UI.Page class
/// in order to enable data binding and many other features.
/// </summary>
public partial class Navigation_Default : Spring.Web.UI.Page
{
    public int Age;

    protected override void FrameworkInitialize()
    {
        this.MasterPageFile = "Master.master";
        base.FrameworkInitialize();
    }

    protected void Page_Load( object sender, EventArgs e )
    { }


    protected void btnContinue_Click( object sender, EventArgs e )
    {
        if (Age > 21)
        {
            SetResult( "ok" );
        }
        else
        {
            Args["input"] = txtAge.Text;
            SetResult( "invalid_input" );
        }
    }

    protected void btnReset_Click( object sender, EventArgs e )
    {
        SetResult( "start" );
    }
}
