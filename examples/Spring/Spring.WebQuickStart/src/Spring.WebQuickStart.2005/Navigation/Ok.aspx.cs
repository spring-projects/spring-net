using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Navigation_Ok : Spring.Web.UI.Page
{
    protected void Page_Load( object sender, EventArgs e )
    {
    }

    protected void btnRestart_Click(object sender, EventArgs e)
    {
        SetResult("start");
    }
}
