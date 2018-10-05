using System;

using Spring.Web.Conversation;
using Spring.Entities;
using NHibernate;

public partial class EndPausedSessionIsClosedA : System.Web.UI.Page
{

    private IConversationState conversation;
    /// <summary>
    /// <see cref="IConversationState"/>
    /// </summary>
    public IConversationState Conversation
    {
        get { return conversation; }
        set { conversation = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        this.Session["result"] = "NOT OK";

        this.Conversation.StartResumeConversation();

        //database access
        ISession session = this.Conversation.SessionFactory.GetCurrentSession();
        SPCMasterEnt sPCMasterEnt = session.Get<SPCMasterEnt>(1);

        this.Session["result"] = "OK";
    }
}
