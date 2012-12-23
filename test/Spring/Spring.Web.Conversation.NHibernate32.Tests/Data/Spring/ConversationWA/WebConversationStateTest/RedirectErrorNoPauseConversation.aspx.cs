using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Spring.ConversationWA;
using Spring.Entities;
using System.Threading;
using NHibernate;

public partial class RedirectErrorNoPauseConversation : System.Web.UI.Page
{
    private IConversationState conversation;
    public IConversationState Conversation
    {
        get { return conversation; }
        set { conversation = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (this.Request.Params["step"].Equals("obtain_session_cookie"))
        {
            //nothing, only for obtain session cookie
        }
        if (this.Request.Params["step"].Equals("step_01"))
        {
            this.Conversation.StartResumeConversation();
            //Open Session for the first time
            ISession ss = this.Conversation.SessionFactory.GetCurrentSession();
            this.Response.Redirect("RedirectErrorNoPauseConversation.aspx?step=step_02");
        }
        else if (this.Request.Params["step"].Equals("step_02"))
        {
            this.Conversation.StartResumeConversation();
        }
        else if (this.Request.Params["step"].Equals("Some_Exception"))
        {
            this.Conversation.StartResumeConversation();
            //Open Session for the first time
            ISession ss = this.Conversation.SessionFactory.GetCurrentSession();
            throw new Exception("Some_Exception");
        }
        else if (this.Request.Params["step"].Equals("Post_Some_Exception"))
        {
            this.Conversation.StartResumeConversation();
        }
        else if (this.Request.Params["step"].Equals("end_conversation"))
        {
            this.Conversation.EndConversation();
        }
    }
}
