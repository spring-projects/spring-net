using System;
using System.Web.Security;
using Spring.Web.UI;

public partial class Admin_Login : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }

    protected void LoginAction_Click(object sender, EventArgs e)
    {        
        string user = UsernameText.Text;
        string pass = PasswordText.Text;

            if (Membership.ValidateUser(user, pass))
            {
                FormsAuthentication.RedirectFromLoginPage(user, false);
            }
            else
            {
                LegendStatus.Text = "Error!";
            }        
    }
}