using System;

using Spring.Web.Conversation;
using Spring.Entities;
using NHibernate;

public partial class SessionIsClosedB : System.Web.UI.Page
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

    private IConversationState conversationA;
    /// <summary>
    /// "convASessionIsClosed"
    /// </summary>
    public IConversationState ConversationA
    {
        get { return conversationA; }
        set { conversationA = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        this.Session["result"] = "NOT OK";

        try
        {
            if ("endA_FreeEnded".Equals(this.Request["command"]))
            {
                this.Conversation.StartResumeConversation();

                /*
                 * Producing the error "HibernateException: Session is closed..." This is because in 
                 * "SessionPerConversationScope.LazySessionPerConversationHolder.CloseConversation(IConversationState)" 
                 * we are closing the "SessionPerConversationScope.LazySessionPerConversationHolder.activeConversation" 
                 * instead of parameter "conversation" (BUG).
                 */
                this.ConversationA.EndConversation();
                this.ConversationA.ConversationManager.FreeEnded();
                this.Conversation.ConversationManager.PauseConversations();
            }

            this.Conversation.StartResumeConversation();

            //database access
            ISession session = this.Conversation.SessionFactory.GetCurrentSession();
            SPCMasterEnt sPCMasterEnt = session.Get<SPCMasterEnt>(1);

            this.Session["result"] = "OK";
        }
        catch (Exception ex)
        {
            this.Session["result"] = ex.Message;
        }
    }
}
